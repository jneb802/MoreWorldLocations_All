using System.Collections.Generic;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Underground_Ruins;

public class CustomPrefabs
{
    public static void RegisterKitPrefabs()
{
    Jotunn.Logger.LogDebug("RegisterKitPrefabs invoked");

    var prefabs = new List<string>()
    {
        // Existing prefabs
        "BFD_CastleKit_groundtorch_blue.prefab",
        // "BFD_chest_loot.prefab",
        // "BFD_ModularEnd1_Pickable1.prefab",
        "BFD_Pickable_SurtlingCoreStand.prefab",
        "BFD_Spawner_GreydwarfNest.prefab",
        "BFD_Vegvisir_GDKing.prefab",
        "PuzzleSecretDoor.prefab",
        "PuzzleLever.prefab",
        "Warp_Greydwarf_Root.prefab",
        "Warp_MineRock_Copper.prefab",
        "BFD_CryptKey.prefab",
        // "BFD_offeraltar.prefab",
        
        "BFD_chest_spawner1.prefab",
        "BFD_chest_spawner2.prefab",
        
        "BFD_Modular5_chestLoot1.prefab",
        "BFD_Modular5_chestLoot2.prefab",
        "BFD_Modular5_Spawner1.prefab",
        "BFD_Modular5_Spawner2.prefab",
        "BFD_Modular5_Spawner3.prefab",

        "BFD_Modular6_Spawner1.prefab",
        "BFD_Modular6_Spawner2.prefab",
        "BFD_Modular6_Spawner3.prefab",
        "BFD_Modular6_Spawner5.prefab",

        "BFD_Modular7_Spawner1.prefab",
        "BFD_Modular7_Spawner2.prefab",

        "BFD_Modular8_Puzzle_chestLoot1.prefab",
        "BFD_Modular8_Puzzle_chestLoot2.prefab",
        "BFD_Modular8_Puzzle_chestLoot3.prefab",
        "BFD_Modular8_Puzzle_Pickable1.prefab",
        "BFD_Modular8_Puzzle_Pickable2.prefab",
        "BFD_Modular8_Puzzle_Pickable3.prefab",

        "BFD_Modular9_Pickable1.prefab",
        "BFD_Modular9_Pickable2.prefab",
        "BFD_Modular9_Pickable3.prefab",
        "BFD_Modular9_Pickable4.prefab",
        "BFD_Modular9_Pickable5.prefab",
        "BFD_Modular9_Pickable6.prefab",
        "BFD_Modular9_Spawner1.prefab",
        "BFD_Modular9_Spawner2.prefab",
        "BFD_Modular9_Spawner3.prefab",
        "BFD_Modular9_Spawner4.prefab",

        "BFD_Modular12_Spawner1.prefab",
        "BFD_Modular12_Spawner2.prefab",
        "BFD_Modular12_Spawner3.prefab",

        "BFD_Modular13_Spawner1.prefab",

        "BFD_Modular14_chestLoot1.prefab",
        "BFD_Modular14_Pickable2.prefab",
        "BFD_Modular14_Pickable3.prefab",
        "BFD_Modular14_Spawner1.prefab",

        "BFD_ModularElbow_Spawner1.prefab",
        "BFD_ModularElbow_Spawner2.prefab",

        "BFD_ModularEnd1_Pickable2.prefab",
        "BFD_ModularEnd1_Pickable3.prefab",

        "BFD_ModularEnd4_chestLoot1.prefab",
        "BFD_ModularEnd6_chestLoot1.prefab",
        "BFD_ModularEnd7_chestLoot1.prefab",

        "BFD_Stairwell5_chestLoot1.prefab"
    };
    
    for (int i = 0; i <= 7; i++)
    {
        prefabs.Add($"{i}_PuzzleStand.prefab");
        prefabs.Add($"{i}_PuzzlePickable.prefab");
    }
    
    foreach (var pref in prefabs)
    {
        RegisterCustomPrefab(Underground_RuinsPlugin.assetBundle, pref);
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