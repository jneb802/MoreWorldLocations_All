// Unity Pipeline eval_file. Configuration: /tmp/mwl-prefab-swap-config.json.
// Run with apply=false first. apply=true requires that preflight's unchanged input hashes.
Newtonsoft.Json.Linq.JObject config = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("/tmp/mwl-prefab-swap-config.json"));
bool apply = (bool)config["apply"];
string output = (string)config["output"];
string mappingPath = (string)config["mapping"];
System.IO.Directory.CreateDirectory(output);
if (UnityEditor.EditorApplication.isPlaying || UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
    throw new System.Exception("Stop Play mode and close Prefab mode before replacing room assets.");
for (int s = 0; s < UnityEngine.SceneManagement.SceneManager.sceneCount; s++)
    if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(s).isDirty)
        throw new System.Exception("Scene has unsaved changes; preserve them before changing prefab assets.");

float MatrixError(UnityEngine.Matrix4x4 a, UnityEngine.Matrix4x4 b)
{
    float error = 0;
    for (int i = 0; i < 16; i++) error = UnityEngine.Mathf.Max(error, UnityEngine.Mathf.Abs(a[i] - b[i]));
    return error;
}
string Hash(string path)
{
    using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
        return System.BitConverter.ToString(sha.ComputeHash(System.IO.File.ReadAllBytes(path))).Replace("-", "");
}
string MeshGuid(UnityEngine.Mesh mesh) { return UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(mesh)); }
Newtonsoft.Json.Linq.JObject evidence = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(mappingPath));
System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject> mappings = new System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>();
System.Collections.Generic.Dictionary<string, string> hashes = new System.Collections.Generic.Dictionary<string, string>();
hashes[mappingPath] = Hash(mappingPath);
foreach (Newtonsoft.Json.Linq.JObject row in evidence["mappings"])
{
    string status = (string)row["status"];
    if (status == "KEEP" || status == "APPLIED" || status == "REVIEW" || status == "PAIR_REVIEW") continue;
    mappings.Add((string)row["current_mock"], row);
    string asset = (string)row["candidate"]["path"];
    hashes[asset] = Hash(asset);
}
System.Collections.Generic.List<string> paths = new System.Collections.Generic.List<string>();
foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/WarpProjects/CryptRecovery/Rooms" }))
{
    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
    paths.Add(path);
    hashes[path] = Hash(path);
    hashes[path + ".meta"] = Hash(path + ".meta");
}
paths.Sort(System.StringComparer.Ordinal);
if (apply)
{
    System.Collections.Generic.Dictionary<string, string> expected = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(System.IO.File.ReadAllText(System.IO.Path.Combine(output, "preflight-input-hashes.json")));
    foreach (System.Collections.Generic.KeyValuePair<string, string> pair in hashes)
        if (!expected.ContainsKey(pair.Key) || expected[pair.Key] != pair.Value) throw new System.Exception("Input changed since preflight: " + pair.Key);
    string backup = System.IO.Path.Combine(output, "original-rooms");
    if (System.IO.Directory.Exists(backup)) throw new System.Exception("Use a new output directory; this backup already exists.");
    System.IO.Directory.CreateDirectory(backup);
    foreach (string path in paths)
    {
        System.IO.File.Copy(path, System.IO.Path.Combine(backup, System.IO.Path.GetFileName(path)));
        System.IO.File.Copy(path + ".meta", System.IO.Path.Combine(backup, System.IO.Path.GetFileName(path) + ".meta"));
    }
}
System.Collections.Generic.List<object> report = new System.Collections.Generic.List<object>();
System.Collections.Generic.HashSet<string> checkedGeometry = new System.Collections.Generic.HashSet<string>();
System.Collections.Generic.List<string> saved = new System.Collections.Generic.List<string>();
try
{
    foreach (string path in paths)
    {
        UnityEngine.GameObject root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
        try
        {
            UnityEngine.Transform[] originals = root.GetComponentsInChildren<UnityEngine.Transform>(true);
            System.Collections.Generic.Dictionary<UnityEngine.Object, UnityEngine.Object> remap = new System.Collections.Generic.Dictionary<UnityEngine.Object, UnityEngine.Object>();
            System.Collections.Generic.HashSet<UnityEngine.Object> removedObjects = new System.Collections.Generic.HashSet<UnityEngine.Object>();
            System.Collections.Generic.List<UnityEngine.GameObject> remove = new System.Collections.Generic.List<UnityEngine.GameObject>();
            System.Collections.Generic.List<object> replacements = new System.Collections.Generic.List<object>();
            foreach (UnityEngine.Transform old in originals)
            {
                if (!mappings.TryGetValue(old.name, out Newtonsoft.Json.Linq.JObject row)) continue;
                string asset = (string)row["candidate"]["path"];
                string child = (string)row["candidate"]["hierarchy"];
                UnityEngine.GameObject native = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(asset);
                UnityEngine.Transform target = string.IsNullOrEmpty(child) ? native.transform : native.transform.Find(child);
                if (target == null) throw new System.Exception("Native child missing: " + asset + "/" + child);
                UnityEngine.MeshFilter[] oldMeshes = old.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
                UnityEngine.MeshFilter[] targetMeshes = target.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
                if (oldMeshes.Length == 0) throw new System.Exception("Expected visual source: " + old.name);
                foreach (UnityEngine.Transform nested in old.GetComponentsInChildren<UnityEngine.Transform>(true))
                    if (nested != old && nested.name.StartsWith("JVLmock_")) throw new System.Exception("Nested mock needs separate review: " + old.name);

                UnityEngine.Matrix4x4 geometry = UnityEngine.Matrix4x4.identity;
                string wantedGuid = MeshGuid(oldMeshes[0].sharedMesh);
                if (row["scaled_geometry_comparison"] != null)
                {
                    Newtonsoft.Json.Linq.JToken scaled = row["scaled_geometry_comparison"];
                    wantedGuid = (string)scaled["target_mesh_guid"];
                    geometry = UnityEngine.Matrix4x4.TRS(new UnityEngine.Vector3((float)scaled["translation_after_scale"][0], (float)scaled["translation_after_scale"][1], (float)scaled["translation_after_scale"][2]), UnityEngine.Quaternion.identity, new UnityEngine.Vector3(0.5f, 1, 1));
                }
                else if (old.name == "JVLmock_CircleStone") wantedGuid = "108c7c25038ab7b4e87559acc94db80a";
                UnityEngine.MeshFilter targetMesh = null;
                foreach (UnityEngine.MeshFilter mesh in targetMeshes) if (MeshGuid(mesh.sharedMesh) == wantedGuid) { targetMesh = mesh; break; }
                if (targetMesh == null) throw new System.Exception("Target mesh missing for " + old.name);
                string geometryKey = MeshGuid(oldMeshes[0].sharedMesh) + ":" + wantedGuid;
                if (checkedGeometry.Add(geometryKey) && oldMeshes[0].sharedMesh != targetMesh.sharedMesh)
                {
                    // Verify the scaled/deduplicated geometry against the current assets, not only the report.
                    UnityEngine.Vector3[] a = oldMeshes[0].sharedMesh.vertices;
                    UnityEngine.Vector3[] b = targetMesh.sharedMesh.vertices;
                    for (int i = 0; i < b.Length; i++) b[i] = geometry.MultiplyPoint3x4(b[i]);
                    for (int direction = 0; direction < 2; direction++)
                    {
                        UnityEngine.Vector3[] from = direction == 0 ? a : b;
                        UnityEngine.Vector3[] to = direction == 0 ? b : a;
                        foreach (UnityEngine.Vector3 vertex in from)
                        {
                            float best = float.PositiveInfinity;
                            foreach (UnityEngine.Vector3 other in to) best = UnityEngine.Mathf.Min(best, (vertex - other).sqrMagnitude);
                            if (best > 0.000001f * 0.000001f) throw new System.Exception("Geometry changed: " + old.name);
                        }
                    }
                    if (oldMeshes[0].sharedMesh.triangles.Length != targetMesh.sharedMesh.triangles.Length) throw new System.Exception("Triangle count changed: " + old.name);
                }
                UnityEngine.Matrix4x4 sourceRelative = old.worldToLocalMatrix * oldMeshes[0].transform.localToWorldMatrix;
                UnityEngine.Matrix4x4 targetRelative = target.worldToLocalMatrix * targetMesh.transform.localToWorldMatrix;
                // Same-name native instances that already contain the native geometry need no edit.
                if (old.name == (string)row["candidate_mock"] && oldMeshes.Length == targetMeshes.Length &&
                    UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(old.gameObject) == target.gameObject && MatrixError(sourceRelative, targetRelative) < 0.00001f) continue;
                UnityEngine.Matrix4x4 correction = sourceRelative * geometry * targetRelative.inverse;
                UnityEngine.Matrix4x4 local = UnityEngine.Matrix4x4.TRS(old.localPosition, old.localRotation, old.localScale) * correction;
                UnityEngine.Vector3 scale = new UnityEngine.Vector3(local.GetColumn(0).magnitude, local.GetColumn(1).magnitude, local.GetColumn(2).magnitude);
                if (local.determinant < 0) scale.x = -scale.x;
                UnityEngine.Quaternion rotation = UnityEngine.Quaternion.LookRotation(local.GetColumn(2) / scale.z, local.GetColumn(1) / scale.y);
                UnityEngine.Vector3 position = local.GetColumn(3);
                if (MatrixError(local, UnityEngine.Matrix4x4.TRS(position, rotation, scale)) > 0.0001f)
                    throw new System.Exception("Transform contains shear; needs a wrapper: " + path + "/" + old.name);
                UnityEngine.GameObject replacement = string.IsNullOrEmpty(child)
                    ? (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(native, root.scene)
                    : UnityEngine.Object.Instantiate(target.gameObject);
                if (!string.IsNullOrEmpty(child)) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(replacement, root.scene);
                replacement.transform.SetParent(old.parent, false);
                replacement.transform.localPosition = position;
                replacement.transform.localRotation = rotation;
                replacement.transform.localScale = scale;
                replacement.transform.SetSiblingIndex(old.GetSiblingIndex());
                replacement.name = (string)row["candidate_mock"];
                replacement.SetActive(old.gameObject.activeSelf);
                string oldHierarchy = UnityEditor.AnimationUtility.CalculateTransformPath(old, root.transform);
                UnityEngine.MeshFilter[] newMeshes = replacement.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
                float maxError = 0;
                remap[old.gameObject] = replacement;
                remap[old] = replacement.transform;
                foreach (UnityEngine.MeshFilter oldMesh in oldMeshes)
                {
                    string guid = oldMesh == oldMeshes[0] ? wantedGuid : MeshGuid(oldMesh.sharedMesh);
                    UnityEngine.MeshFilter newMesh = null;
                    foreach (UnityEngine.MeshFilter mesh in newMeshes) if (MeshGuid(mesh.sharedMesh) == guid) { newMesh = mesh; break; }
                    if (newMesh == null) throw new System.Exception("Additional source mesh would be lost: " + oldHierarchy);
                    UnityEngine.Matrix4x4 expected = oldMesh.transform.localToWorldMatrix * (oldMesh == oldMeshes[0] ? geometry : UnityEngine.Matrix4x4.identity);
                    float error = MatrixError(expected, newMesh.transform.localToWorldMatrix);
                    maxError = UnityEngine.Mathf.Max(maxError, error);
                    if (error > 0.0002f) throw new System.Exception("Mesh placement changed by " + error + ": " + oldHierarchy);
                    remap[oldMesh] = newMesh;
                    UnityEngine.Renderer oldRenderer = oldMesh.GetComponent<UnityEngine.Renderer>();
                    if (oldRenderer != null) remap[oldRenderer] = newMesh.GetComponent<UnityEngine.Renderer>();
                    if (oldMesh.transform != old)
                    {
                        remap[oldMesh.gameObject] = newMesh.gameObject;
                        remap[oldMesh.transform] = newMesh.transform;
                    }
                }
                foreach (UnityEngine.Transform transform in old.GetComponentsInChildren<UnityEngine.Transform>(true))
                {
                    removedObjects.Add(transform.gameObject);
                    foreach (UnityEngine.Component component in transform.GetComponents<UnityEngine.Component>())
                    {
                        if (component == null) throw new System.Exception("Missing source component: " + oldHierarchy);
                        removedObjects.Add(component);
                        if (remap.ContainsKey(component)) continue;
                        UnityEngine.GameObject mapped = remap.TryGetValue(transform.gameObject, out UnityEngine.Object obj) ? obj as UnityEngine.GameObject : null;
                        UnityEngine.Component match = mapped == null ? null : mapped.GetComponent(component.GetType());
                        if (match == null)
                        {
                            UnityEngine.Component[] compatible = replacement.GetComponentsInChildren(component.GetType(), true);
                            if (compatible.Length == 1) match = compatible[0];
                        }
                        if (match != null) remap[component] = match;
                    }
                }
                if (string.IsNullOrEmpty(child))
                {
                    UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(replacement.transform);
                    UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(replacement);
                }
                remove.Add(old.gameObject);
                replacements.Add(new { before = oldHierarchy, after = replacement.name, asset, child, maxMeshMatrixError = maxError,
                    localPosition = position.ToString("F7"), localRotation = rotation.ToString("F7"), localScale = scale.ToString("F7"),
                    colliders = replacement.GetComponentsInChildren<UnityEngine.Collider>(true).Length });
            }
            int references = 0;
            foreach (UnityEngine.Component component in root.GetComponentsInChildren<UnityEngine.Component>(true))
            {
                if (component == null || component is UnityEngine.Transform || removedObjects.Contains(component)) continue;
                UnityEditor.SerializedObject serialized = new UnityEditor.SerializedObject(component);
                UnityEditor.SerializedProperty property = serialized.GetIterator();
                bool changed = false;
                while (property.Next(true))
                {
                    if (property.propertyType != UnityEditor.SerializedPropertyType.ObjectReference || property.objectReferenceValue == null) continue;
                    UnityEngine.Object original = property.objectReferenceValue;
                    if (!removedObjects.Contains(original)) continue;
                    if (!remap.TryGetValue(original, out UnityEngine.Object updated) || updated == null)
                        throw new System.Exception("Unmapped external reference: " + path + "/" + component.name + "/" + property.propertyPath);
                    property.objectReferenceValue = updated;
                    changed = true;
                    references++;
                }
                if (changed) serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach (UnityEngine.GameObject old in remove) UnityEngine.Object.DestroyImmediate(old);
            if (apply && remove.Count > 0)
            {
                saved.Add(path);
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
                if (!success) throw new System.Exception("Could not save " + path);
            }
            report.Add(new { path, count = remove.Count, references, replacements });
        }
        finally { UnityEditor.PrefabUtility.UnloadPrefabContents(root); }
    }
}
catch
{
    // No partial room conversion is left behind after an apply failure.
    if (apply) foreach (string path in saved)
    {
        System.IO.File.Copy(System.IO.Path.Combine(output, "original-rooms", System.IO.Path.GetFileName(path)), path, true);
        UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
    }
    throw;
}
string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { apply, rooms = report }, Newtonsoft.Json.Formatting.Indented);
System.IO.File.WriteAllText(System.IO.Path.Combine(output, apply ? "applied.json" : "preflight.json"), json);
if (!apply) System.IO.File.WriteAllText(System.IO.Path.Combine(output, "preflight-input-hashes.json"), Newtonsoft.Json.JsonConvert.SerializeObject(hashes, Newtonsoft.Json.Formatting.Indented));
return new { apply, output, rooms = report.Count };
