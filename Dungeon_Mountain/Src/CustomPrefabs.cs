using System.Collections.Generic;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Dungeon_Mountain;

public class CustomPrefabs
{
    public static void RegisterKitPrefabs()
{
    Jotunn.Logger.LogDebug("RegisterKitPrefabs invoked");

    var prefabs = new List<string>()
    {
        
    };
    
    foreach (var pref in prefabs)
    {
        RegisterCustomPrefab(Dungeon_MountainPlugin.assetBundle, pref);
    }
    
    PrefabManager.OnVanillaPrefabsAvailable -= RegisterKitPrefabs;
}

    
    public static GameObject RegisterCustomPrefab(AssetBundle bundle, string assetName)
    {
        string prefabName = assetName.Replace(".prefab", "");
        if (!string.IsNullOrEmpty(prefabName))
        {
            var prefab = new CustomPrefab(bundle, assetName, true);
            Jotunn.Logger.LogDebug("Registering " + prefab.Prefab.name);
            if (prefab != null && prefab.IsValid())
            {
                prefab.Prefab.FixReferences(true);
                PrefabManager.Instance.AddPrefab(prefab);
                return prefab.Prefab;
            }
        }
        return null;
    }
}