// Read-only saved-asset verification after replace-crypts-reviewed-prefabs.cs.
Newtonsoft.Json.Linq.JObject config = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("/tmp/mwl-prefab-swap-config.json"));
string output = (string)config["output"];
Newtonsoft.Json.Linq.JObject report = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(output, "applied.json")));
float Error(UnityEngine.Matrix4x4 a, UnityEngine.Matrix4x4 b)
{
    float error = 0;
    for (int i = 0; i < 16; i++) error = UnityEngine.Mathf.Max(error, UnityEngine.Mathf.Abs(a[i] - b[i]));
    return error;
}
float[] Numbers(string text)
{
    return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(text.Trim('(', ')').Split(','), value => float.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
}
int verified = 0;
float maxError = 0;
// Load temporary copies of the backups to include null entries inherited from nested prefabs.
System.Collections.Generic.Dictionary<string, int> baselineNulls = new System.Collections.Generic.Dictionary<string, int>();
string scratchName = "__MWLVerify_" + System.Guid.NewGuid().ToString("N");
string scratch = "Assets/" + scratchName;
UnityEditor.AssetDatabase.CreateFolder("Assets", scratchName);
try
{
    foreach (Newtonsoft.Json.Linq.JObject room in report["rooms"])
    {
        string path = (string)room["path"];
        string temporary = scratch + "/" + System.IO.Path.GetFileName(path);
        System.IO.File.Copy(System.IO.Path.Combine(output, "original-rooms", System.IO.Path.GetFileName(path)), temporary);
        UnityEditor.AssetDatabase.ImportAsset(temporary, UnityEditor.ImportAssetOptions.ForceSynchronousImport);
        UnityEngine.GameObject original = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(temporary);
        int nulls = 0;
        foreach (UnityEngine.LODGroup group in original.GetComponentsInChildren<UnityEngine.LODGroup>(true))
            foreach (UnityEngine.LOD lod in group.GetLODs())
                foreach (UnityEngine.Renderer renderer in lod.renderers) if (renderer == null) nulls++;
        baselineNulls[path] = nulls;
    }
}
finally { UnityEditor.AssetDatabase.DeleteAsset(scratch); }
System.Collections.Generic.List<object> rooms = new System.Collections.Generic.List<object>();
foreach (Newtonsoft.Json.Linq.JObject room in report["rooms"])
{
    string path = (string)room["path"];
    UnityEngine.GameObject root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
    try
    {
        System.Collections.Generic.HashSet<UnityEngine.Transform> consumed = new System.Collections.Generic.HashSet<UnityEngine.Transform>();
        foreach (Newtonsoft.Json.Linq.JObject item in room["replacements"])
        {
            string oldPath = (string)item["before"];
            int slash = oldPath.LastIndexOf('/');
            string parentPath = slash < 0 ? "" : oldPath.Substring(0, slash);
            float[] p = Numbers((string)item["localPosition"]);
            float[] q = Numbers((string)item["localRotation"]);
            float[] s = Numbers((string)item["localScale"]);
            UnityEngine.Matrix4x4 expected = UnityEngine.Matrix4x4.TRS(new UnityEngine.Vector3(p[0], p[1], p[2]), new UnityEngine.Quaternion(q[0], q[1], q[2], q[3]), new UnityEngine.Vector3(s[0], s[1], s[2]));
            UnityEngine.Transform actual = null;
            foreach (UnityEngine.Transform candidate in root.GetComponentsInChildren<UnityEngine.Transform>(true))
            {
                if (candidate.name != (string)item["after"] || consumed.Contains(candidate)) continue;
                if (UnityEditor.AnimationUtility.CalculateTransformPath(candidate.parent, root.transform) != parentPath) continue;
                float error = Error(expected, UnityEngine.Matrix4x4.TRS(candidate.localPosition, candidate.localRotation, candidate.localScale));
                if (error > 0.0001f) continue;
                actual = candidate;
                maxError = UnityEngine.Mathf.Max(maxError, error);
                break;
            }
            if (actual == null) throw new System.Exception("Saved placement not found: " + path + "/" + oldPath);
            consumed.Add(actual);
            string asset = (string)item["asset"];
            string child = (string)item["child"];
            UnityEngine.GameObject native = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(asset);
            UnityEngine.Transform target = string.IsNullOrEmpty(child) ? native.transform : native.transform.Find(child);
            if (string.IsNullOrEmpty(child) && UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(actual.gameObject) != target.gameObject)
                throw new System.Exception("Lost native prefab connection: " + oldPath);
            UnityEngine.MeshFilter[] actualMeshes = actual.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
            UnityEngine.MeshFilter[] targetMeshes = target.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
            if (actualMeshes.Length != targetMeshes.Length) throw new System.Exception("Saved mesh count differs: " + oldPath);
            for (int i = 0; i < targetMeshes.Length; i++)
            {
                if (actualMeshes[i].sharedMesh != targetMeshes[i].sharedMesh) throw new System.Exception("Saved mesh differs: " + oldPath);
                float error = Error(actual.worldToLocalMatrix * actualMeshes[i].transform.localToWorldMatrix, target.worldToLocalMatrix * targetMeshes[i].transform.localToWorldMatrix);
                if (error > 0.0001f) throw new System.Exception("Saved native geometry differs: " + oldPath);
                UnityEngine.Material[] actualMaterials = actualMeshes[i].GetComponent<UnityEngine.Renderer>().sharedMaterials;
                UnityEngine.Material[] targetMaterials = targetMeshes[i].GetComponent<UnityEngine.Renderer>().sharedMaterials;
                if (!System.Linq.Enumerable.SequenceEqual(actualMaterials, targetMaterials)) throw new System.Exception("Saved material differs: " + oldPath);
                foreach (UnityEngine.Material material in actualMaterials)
                    if (material == null || material.shader == null) throw new System.Exception("Missing material/shader: " + oldPath);
            }
            if (actual.GetComponentsInChildren<UnityEngine.Collider>(true).Length != (int)item["colliders"])
                throw new System.Exception("Saved collision differs: " + oldPath);
            verified++;
        }
        foreach (UnityEngine.Component component in root.GetComponentsInChildren<UnityEngine.Component>(true))
            if (component == null) throw new System.Exception("Missing component in " + path);
        int nullLodRenderers = 0;
        foreach (UnityEngine.LODGroup group in root.GetComponentsInChildren<UnityEngine.LODGroup>(true))
            foreach (UnityEngine.LOD lod in group.GetLODs())
                foreach (UnityEngine.Renderer renderer in lod.renderers)
                    if (renderer == null) nullLodRenderers++;
        int originalNullLodRenderers = baselineNulls[path];
        if (nullLodRenderers != originalNullLodRenderers) throw new System.Exception("LOD null-reference count changed: " + path);
        rooms.Add(new { path, verified = consumed.Count, preexistingNullLodRenderers = nullLodRenderers });
    }
    finally { UnityEditor.PrefabUtility.UnloadPrefabContents(root); }
}
string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { verified, maxSavedTransformError = maxError, rooms }, Newtonsoft.Json.Formatting.Indented);
System.IO.File.WriteAllText(System.IO.Path.Combine(output, "verification.json"), json);
return json;
