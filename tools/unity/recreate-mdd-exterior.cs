// Unity Pipeline eval_file. Rebuild geometry with current native assets; no old game assets imported.
string repository = "/Users/benjmarston/Develop/MoreWorldLocations-crypts-caverns";
Newtonsoft.Json.Linq.JObject plan = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(repository + "/docs/mdd-exterior-recreation.json"));
string destination = (string)plan["destination"];
if (System.IO.File.Exists(destination)) throw new System.Exception("Destination already exists: " + destination);
System.Collections.Generic.Dictionary<string, UnityEngine.GameObject> assets = new System.Collections.Generic.Dictionary<string, UnityEngine.GameObject>();
foreach (Newtonsoft.Json.Linq.JProperty entry in ((Newtonsoft.Json.Linq.JObject)plan["candidates"]).Properties())
{
    if (entry.Value.Count() != 1) throw new System.Exception("Ambiguous prefab: " + entry.Name);
    string path = (string)entry.Value[0];
    UnityEngine.GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
    if (asset == null || asset.name != entry.Name) throw new System.Exception("Native prefab not found: " + path);
    assets.Add(entry.Name, asset);
}
UnityEngine.Vector3 Vector(Newtonsoft.Json.Linq.JToken value) { return new UnityEngine.Vector3((float)value["x"], (float)value["y"], (float)value["z"]); }
UnityEngine.Quaternion Rotation(Newtonsoft.Json.Linq.JToken value) { return new UnityEngine.Quaternion((float)value["x"], (float)value["y"], (float)value["z"], (float)value["w"]); }
float MatrixError(UnityEngine.Matrix4x4 a, UnityEngine.Matrix4x4 b)
{
    float result = 0;
    for (int i = 0; i < 16; i++) result = UnityEngine.Mathf.Max(result, UnityEngine.Mathf.Abs(a[i] - b[i]));
    return result;
}
UnityEngine.SceneManagement.Scene preview = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
System.Collections.Generic.Dictionary<string, UnityEngine.Transform> nodes = new System.Collections.Generic.Dictionary<string, UnityEngine.Transform>();
System.Collections.Generic.Dictionary<string, UnityEngine.Matrix4x4> expectedMatrices = new System.Collections.Generic.Dictionary<string, UnityEngine.Matrix4x4>();
System.Collections.Generic.List<object> instances = new System.Collections.Generic.List<object>();
UnityEngine.GameObject root = null;
float maximumError = 0;
try
{
    foreach (Newtonsoft.Json.Linq.JObject node in plan["nodes"])
    {
        string prefab = (string)node["prefab"];
        string parent = (string)node["parent"];
        UnityEngine.GameObject obj = prefab == null ? new UnityEngine.GameObject((string)node["name"]) : (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(assets[prefab], preview);
        if (prefab == null) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, preview);
        if (parent != null) obj.transform.SetParent(nodes[parent], false); else root = obj;
        obj.name = (string)node["name"];
        obj.transform.localPosition = Vector(node["position"]);
        obj.transform.localRotation = Rotation(node["rotation"]);
        obj.transform.localScale = Vector(node["scale"]);
        obj.SetActive((int)node["active"] != 0);
        string id = (string)node["id"];
        nodes.Add(id, obj.transform);
        UnityEngine.Matrix4x4 matrix = UnityEngine.Matrix4x4.TRS(Vector(node["position"]), Rotation(node["rotation"]), Vector(node["scale"]));
        if (parent != null) matrix = expectedMatrices[parent] * matrix;
        expectedMatrices.Add(id, matrix);
        float error = MatrixError(obj.transform.localToWorldMatrix, matrix);
        if (error > 0.0001f) throw new System.Exception("Placement changed: " + id);
        maximumError = UnityEngine.Mathf.Max(maximumError, error);
        if (prefab != null)
        {
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(obj.transform);
            instances.Add(new { id, name = obj.name, prefab, source = UnityEditor.AssetDatabase.GetAssetPath(assets[prefab]) });
        }
    }
    string directory = System.IO.Path.GetDirectoryName(destination).Replace('\\', '/');
    if (!UnityEditor.AssetDatabase.IsValidFolder(directory)) UnityEditor.AssetDatabase.CreateFolder("Assets/WarpProjects/CryptRecovery", "Exterior");
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, destination, out bool saved);
    if (!saved) throw new System.Exception("Could not save exterior");
}
finally { UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview); }
UnityEngine.GameObject persisted = UnityEditor.PrefabUtility.LoadPrefabContents(destination);
try
{
    System.Collections.Generic.Dictionary<string, UnityEngine.Transform> savedNodes = new System.Collections.Generic.Dictionary<string, UnityEngine.Transform>();
    System.Collections.Generic.Dictionary<string, int> childIndices = new System.Collections.Generic.Dictionary<string, int>();
    foreach (Newtonsoft.Json.Linq.JObject node in plan["nodes"])
    {
        string id = (string)node["id"];
        string parent = (string)node["parent"];
        UnityEngine.Transform current;
        if (parent == null) current = persisted.transform;
        else
        {
            int index = childIndices.ContainsKey(parent) ? childIndices[parent] : 0;
            current = savedNodes[parent].GetChild(index);
            childIndices[parent] = index + 1;
        }
        savedNodes[id] = current;
        if (current.name != (string)node["name"] || current.gameObject.activeSelf != ((int)node["active"] != 0)) throw new System.Exception("Saved hierarchy differs: " + id);
        float error = MatrixError(current.localToWorldMatrix, expectedMatrices[id]);
        if (error > 0.0001f) throw new System.Exception("Saved placement differs: " + id);
        maximumError = UnityEngine.Mathf.Max(maximumError, error);
        string prefab = (string)node["prefab"];
        if (prefab != null && UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(current.gameObject) != assets[prefab]) throw new System.Exception("Lost native prefab connection: " + id);
    }
    foreach (UnityEngine.Component component in persisted.GetComponentsInChildren<UnityEngine.Component>(true))
        if (component == null) throw new System.Exception("Missing component in recreated exterior");
}
finally { UnityEditor.PrefabUtility.UnloadPrefabContents(persisted); }
string report = Newtonsoft.Json.JsonConvert.SerializeObject(new { destination, instances = instances.Count, preservedNodes = nodes.Count, maximumTransformError = maximumError, nativePrefabs = instances, functionalDungeonSetup = false }, Newtonsoft.Json.Formatting.Indented);
System.IO.File.WriteAllText(repository + "/docs/mdd-exterior-recreation-result.json", report + "\n");
UnityEditor.Selection.activeObject = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(destination);
UnityEditor.EditorGUIUtility.PingObject(UnityEditor.Selection.activeObject);
return new { destination, pieces = instances.Count, maximumTransformError = maximumError };
