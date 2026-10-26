// Unity Pipeline eval_file. Replace only this verified model mapping.
string folder = "Assets/WarpProjects/CryptRecovery/Rooms";
string targetPath = "Assets/world/Props/CastleBuildingKit/SunkenKit_int_wall_1x2.prefab";
UnityEngine.GameObject target = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(targetPath);
if (target == null) throw new System.Exception("Missing target prefab: " + targetPath);
UnityEngine.MeshFilter targetHigh = target.transform.Find("stone_hgih").GetComponent<UnityEngine.MeshFilter>();
string backup = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mwl-sunkenkit-" + System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff"));
System.IO.Directory.CreateDirectory(backup);
System.Collections.Generic.List<object> report = new System.Collections.Generic.List<object>();
foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new string[] { folder }))
{
    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
    UnityEngine.GameObject root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
    int replaced = 0;
    try
    {
        System.Collections.Generic.List<UnityEngine.Transform> matches = new System.Collections.Generic.List<UnityEngine.Transform>();
        foreach (UnityEngine.Transform transform in root.GetComponentsInChildren<UnityEngine.Transform>(true))
            if (transform.name == "JVLmock_stone_2x1_high") matches.Add(transform);
        if (matches.Count == 0) continue;
        System.IO.File.Copy(path, System.IO.Path.Combine(backup, System.IO.Path.GetFileName(path)));
        System.IO.File.Copy(path + ".meta", System.IO.Path.Combine(backup, System.IO.Path.GetFileName(path) + ".meta"));

        foreach (UnityEngine.Transform old in matches)
        {
            UnityEngine.MeshFilter[] meshes = old.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
            if (meshes.Length != 1 || meshes[0].sharedMesh != targetHigh.sharedMesh)
                throw new System.Exception("Unexpected source mesh in " + path);
            foreach (UnityEngine.Component component in old.GetComponentsInChildren<UnityEngine.Component>(true))
                if (!(component is UnityEngine.Transform) && !(component is UnityEngine.MeshFilter) && !(component is UnityEngine.MeshRenderer))
                    throw new System.Exception("Source contains nonvisual components in " + path);

            UnityEngine.GameObject replacement = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(target, root.scene);
            replacement.transform.SetParent(old.parent, false);
            replacement.transform.localPosition = old.localPosition;
            replacement.transform.localRotation = old.localRotation;
            replacement.transform.localScale = old.localScale;
            replacement.transform.SetSiblingIndex(old.GetSiblingIndex());
            replacement.name = "JVLmock_SunkenKit_int_wall_1x2";
            replacement.SetActive(old.gameObject.activeSelf);
            UnityEngine.MeshFilter newHigh = replacement.transform.Find("stone_hgih").GetComponent<UnityEngine.MeshFilter>();
            UnityEngine.Matrix4x4 before = meshes[0].transform.localToWorldMatrix;
            UnityEngine.Matrix4x4 after = newHigh.transform.localToWorldMatrix;
            for (int i = 0; i < 16; i++)
                if (UnityEngine.Mathf.Abs(before[i] - after[i]) > 0.0001f)
                    throw new System.Exception("Replacement would move the mesh in " + path);

            // Preserve explicit component references held outside the replaced model.
            System.Collections.Generic.Dictionary<UnityEngine.Object, UnityEngine.Object> remap = new System.Collections.Generic.Dictionary<UnityEngine.Object, UnityEngine.Object>
            {
                { old.gameObject, replacement }, { old, replacement.transform },
                { meshes[0].gameObject, newHigh.gameObject }, { meshes[0].transform, newHigh.transform },
                { meshes[0], newHigh }, { meshes[0].GetComponent<UnityEngine.MeshRenderer>(), newHigh.GetComponent<UnityEngine.MeshRenderer>() }
            };
            foreach (UnityEngine.Component component in root.GetComponentsInChildren<UnityEngine.Component>(true))
            {
                if (component == null || component is UnityEngine.Transform || component.transform.IsChildOf(old) || component.transform.IsChildOf(replacement.transform)) continue;
                UnityEditor.SerializedObject serialized = new UnityEditor.SerializedObject(component);
                UnityEditor.SerializedProperty property = serialized.GetIterator();
                bool changed = false;
                while (property.Next(true))
                    if (property.propertyType == UnityEditor.SerializedPropertyType.ObjectReference && property.objectReferenceValue != null && remap.TryGetValue(property.objectReferenceValue, out UnityEngine.Object newReference))
                    {
                        property.objectReferenceValue = newReference;
                        changed = true;
                    }
                if (changed) serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(replacement.transform);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(replacement);
            UnityEngine.Object.DestroyImmediate(old.gameObject);
            replaced++;
        }
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);
        report.Add(new { path, replaced });
    }
    finally { UnityEditor.PrefabUtility.UnloadPrefabContents(root); }
}
string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { backup, rooms = report }, Newtonsoft.Json.Formatting.Indented);
System.IO.File.WriteAllText(System.IO.Path.Combine(backup, "report.json"), json);
return json;
