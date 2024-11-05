using System.Collections.Generic;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Dungeon_The_Ritual;

public class CustomPrefabs
{
    public static void RegisterKitPrefabs()
    {
        Jotunn.Logger.LogDebug("RegisterKitPrefabs invoked");

        var prefabs = new List<string>()
        {
            "BFD_CastleKit_groundtorch_blue.prefab",
            "BFD_chest_loot_roomName1.prefab",
            "BFD_chest_spawner1.prefab",
            "BFD_ModularEnd1_Pickable1.prefab",
            "BFD_Pickable_SurtlingCoreStand.prefab",
            "BFD_Spawner_GreydwarfNest.prefab",
            "BFD_Vegvisir_GDKing.prefab",
            "PuzzleSecretDoor.prefab",
            "PuzzleStand.prefab",
            "Warp_Greydwarf_Root.prefab",
            "Warp_MineRock_Copper.prefab",
            "BFD_offeraltar.prefab"
        };

        foreach (var pref in prefabs)
        {
            RegisterCustomPrefab(Dungeon_The_RitualPlugin.assetBundle, pref);
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