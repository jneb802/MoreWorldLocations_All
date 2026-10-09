// Run through Unity Pipeline's eval_file in the Crypts & Caverns authoring project.
// Back up each prefab before removing mock prefixes from embedded visual models.
string folder = "Assets/WarpProjects/CryptRecovery/Rooms";
string backup = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
    "mwl-visual-mocks-" + System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff"));
System.IO.Directory.CreateDirectory(backup);
System.Collections.Generic.List<object> changes = new System.Collections.Generic.List<object>();
foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new string[] { folder }))
{
    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
    string copy = System.IO.Path.Combine(backup, System.IO.Path.GetFileName(path));
    System.IO.File.Copy(path, copy);
    System.IO.File.Copy(path + ".meta", copy + ".meta");
    UnityEngine.GameObject root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
    System.Collections.Generic.Dictionary<string, int> renamed = new System.Collections.Generic.Dictionary<string, int>();
    try
    {
        foreach (UnityEngine.Transform transform in root.GetComponentsInChildren<UnityEngine.Transform>(true))
        {
            const string prefix = "JVLmock_";
            if (!transform.name.StartsWith(prefix, System.StringComparison.Ordinal)) continue;
            UnityEngine.MeshFilter[] meshes = transform.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
            if (meshes.Length == 0) continue;
            bool visualOnly = true;
            foreach (UnityEngine.MeshFilter mesh in meshes)
                if (mesh.sharedMesh == null) visualOnly = false;
            foreach (UnityEngine.Component component in transform.GetComponentsInChildren<UnityEngine.Component>(true))
                if (!(component is UnityEngine.Transform) && !(component is UnityEngine.MeshFilter) &&
                    !(component is UnityEngine.MeshRenderer)) visualOnly = false;
            if (!visualOnly) continue;

            string oldName = transform.name;
            if (!renamed.ContainsKey(oldName)) renamed[oldName] = 0;
            renamed[oldName]++;
            transform.gameObject.name = oldName.Substring(prefix.Length);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(transform.gameObject);
        }
        if (renamed.Count > 0) UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);
    }
    finally
    {
        UnityEditor.PrefabUtility.UnloadPrefabContents(root);
    }
    changes.Add(new { path, renamed });
}
string report = Newtonsoft.Json.JsonConvert.SerializeObject(new { backup, changes }, Newtonsoft.Json.Formatting.Indented);
System.IO.File.WriteAllText(System.IO.Path.Combine(backup, "report.json"), report);
return report;
