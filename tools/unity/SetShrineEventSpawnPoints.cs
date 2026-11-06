string path = "Assets/WarpProjects/Dungeons/MWL_ShrineEvent1.prefab";
UnityEditor.SceneManagement.PrefabStage stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
if (stage != null && stage.assetPath == path && stage.scene.isDirty)
    throw new System.InvalidOperationException("Save the open shrine prefab before applying this change.");
UnityEngine.GameObject root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
try
{
    UnityEngine.Transform parent = root.transform.Find("Interior/ShrineEventSpawnPoints");
    if (parent == null || (parent.childCount != 4 && parent.childCount != 8))
        throw new System.InvalidOperationException("Unexpected spawn marker layout.");
    float diagonal = 20f / UnityEngine.Mathf.Sqrt(2f);
    UnityEngine.Vector3[] positions = new UnityEngine.Vector3[] {
        new UnityEngine.Vector3(diagonal, 0, diagonal),
        new UnityEngine.Vector3(-diagonal, 0, diagonal),
        new UnityEngine.Vector3(-diagonal, 0, -diagonal),
        new UnityEngine.Vector3(diagonal, 0, -diagonal)
    };
    for (int i = 0; i < positions.Length; i++)
    {
        string name = "SpawnPoint_" + (i + 5);
        UnityEngine.Transform marker = parent.Find(name);
        if (marker == null)
        {
            marker = new UnityEngine.GameObject(name).transform;
            marker.SetParent(parent, false);
        }
        marker.localPosition = positions[i];
    }
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);
    return new { count = parent.childCount, radius = 20f };
}
finally { UnityEditor.PrefabUtility.UnloadPrefabContents(root); }
