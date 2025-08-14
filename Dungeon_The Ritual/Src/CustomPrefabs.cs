using System.Collections.Generic;
using System.Linq;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using CreatureManager = Common.CreatureManager;

namespace Underground_Ruins;

public class CustomPrefabs
{
    public static void RegisterKitPrefabs()
{
    Jotunn.Logger.LogDebug("RegisterKitPrefabs invoked");

    var customPrefabs = new List<string>()
    {
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
    };
    
    var roomSpawnerPrefabs = new List<string>()
    {
        "BFD_Modular5_Spawner1.prefab",
        "BFD_Modular5_Spawner2.prefab",
        "BFD_Modular5_Spawner3.prefab",
        
        "BFD_Modular6_Spawner1.prefab",
        "BFD_Modular6_Spawner2.prefab",
        "BFD_Modular6_Spawner3.prefab",
        "BFD_Modular6_Spawner5.prefab",
        
        "BFD_Modular7_Spawner1.prefab",
        "BFD_Modular7_Spawner2.prefab",
        
        "BFD_Modular9_Spawner1.prefab",
        "BFD_Modular9_Spawner2.prefab",
        "BFD_Modular9_Spawner3.prefab",
        "BFD_Modular9_Spawner4.prefab",
        
        "BFD_Modular12_Spawner1.prefab",
        "BFD_Modular12_Spawner2.prefab",
        "BFD_Modular12_Spawner3.prefab",
        
        "BFD_Modular13_Spawner1.prefab",
        
        "BFD_Modular14_Spawner1.prefab",
        
        "BFD_ModularElbow_Spawner1.prefab",
        "BFD_ModularElbow_Spawner2.prefab"
    };
    
    var roomContainerPrefabs = new List<string>()
    {
        "BFD_Modular5_chest_loot1.prefab",
        "BFD_Modular5_chest_loot2.prefab",
        
        "BFD_Modular8_Puzzle_chest_loot1.prefab",
        "BFD_Modular8_Puzzle_chest_loot2.prefab",
        "BFD_Modular8_Puzzle_chest_loot3.prefab",
        
        "BFD_Modular14_chest_loot1.prefab",
        
        "BFD_ModularEnd4_chest_loot1.prefab",
        "BFD_ModularEnd6_chest_loot1.prefab",
        "BFD_ModularEnd7_chest_loot1.prefab",
        
        "BFD_Stairwell5_chest_loot1.prefab"
    };
    
    var roomPickablePrefabs = new List<string>()
    {
        "BFD_Modular8_Puzzle_Pickable1.prefab",
        "BFD_Modular8_Puzzle_Pickable2.prefab",
        "BFD_Modular8_Puzzle_Pickable3.prefab",
        
        "BFD_Modular9_Pickable1.prefab",
        "BFD_Modular9_Pickable2.prefab",
        "BFD_Modular9_Pickable3.prefab",
        "BFD_Modular9_Pickable4.prefab",
        "BFD_Modular9_Pickable5.prefab",
        "BFD_Modular9_Pickable6.prefab",
        
        "BFD_Modular14_Pickable2.prefab",
        "BFD_Modular14_Pickable3.prefab",
        
        "BFD_ModularEnd1_Pickable2.prefab",
        "BFD_ModularEnd1_Pickable3.prefab"
    };

    
    List<string> puzzlePrefabs = new List<string>();
    
    for (int i = 0; i <= 7; i++)
    {
        puzzlePrefabs.Add($"{i}_PuzzleStand.prefab");
        puzzlePrefabs.Add($"{i}_PuzzlePickable.prefab");
    }
    
    foreach (var prefab in customPrefabs)
    {
        RegisterCustomPrefab(Underground_RuinsPlugin.assetBundle, prefab);
    }
    
    foreach (var prefab in puzzlePrefabs)
    {
        RegisterCustomPrefab(Underground_RuinsPlugin.assetBundle, prefab);
    }
    
    foreach (var prefab in roomSpawnerPrefabs)
    {
        List<GameObject> creatureList = Underground_RuinsPlugin.dungeonBFDYamlManager.creatureListDictionary[Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.CreatureList.Value];
        int randomIndex = Random.Range(0, creatureList.Count);
        RegisterCustomPrefabSpawner(Underground_RuinsPlugin.assetBundle, prefab, creatureList[randomIndex]);
    }
    
    foreach (var prefab in roomContainerPrefabs)
    {
        // RegisterCustomPrefabContainer(Underground_RuinsPlugin.assetBundle, prefab, Underground_RuinsPlugin.dungeonBFDYamlManager.lootList);
        List<DropTable.DropData> lootList = Underground_RuinsPlugin.dungeonBFDYamlManager.lootListDictionary[Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.LootList.Value];
        RegisterCustomPrefabContainer(Underground_RuinsPlugin.assetBundle, prefab, lootList);
    }
    
    foreach (var prefab in roomPickablePrefabs)
    {
        RegisterCustomPrefabRandomItem(Underground_RuinsPlugin.assetBundle, prefab, Underground_RuinsPlugin.dungeonBFDYamlManager.pickableList);
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
    
    public static GameObject RegisterCustomPrefabSpawner(AssetBundle bundle, string assetName, GameObject creature)
    {
        string prefabName = assetName.Replace(".prefab", "");
        if (!string.IsNullOrEmpty(prefabName))
        {
            CustomPrefab prefab = new CustomPrefab(bundle, assetName, true);
            Jotunn.Logger.LogDebug("Registering " + prefab.Prefab.name);
            if (prefab != null && prefab.IsValid())
            {
                CreatureSpawner creatureSpawner = prefab.Prefab.GetComponent<CreatureSpawner>();
                creatureSpawner.m_creaturePrefab = creature;
                
                prefab.Prefab.FixReferences(true);
                
                PrefabManager.Instance.AddPrefab(prefab);
                return prefab.Prefab;
            }
        }
        return null;
    }
    
    public static GameObject RegisterCustomPrefabContainer(AssetBundle bundle, string assetName, List<DropTable.DropData> dropData)
    {
        string prefabName = assetName.Replace(".prefab", "");
        if (!string.IsNullOrEmpty(prefabName))
        {
            CustomPrefab prefab = new CustomPrefab(bundle, assetName, true);
            Jotunn.Logger.LogDebug("Registering " + prefab.Prefab.name);
            if (prefab != null && prefab.IsValid())
            {
                Container container = prefab.Prefab.GetComponent<Container>();
                container.m_defaultItems.m_drops = dropData;
                
                prefab.Prefab.FixReferences(true);
                
                PrefabManager.Instance.AddPrefab(prefab);
                return prefab.Prefab;
            }
        }
        return null;
    }
    
    public static GameObject RegisterCustomPrefabRandomItem(AssetBundle bundle, string assetName, List<PickableItem.RandomItem> randomItems)
    {
        string prefabName = assetName.Replace(".prefab", "");
        if (!string.IsNullOrEmpty(prefabName))
        {
            CustomPrefab prefab = new CustomPrefab(bundle, assetName, true);
            Jotunn.Logger.LogDebug("Registering " + prefab.Prefab.name);
            if (prefab != null && prefab.IsValid())
            {
                PickableItem.RandomItem[] randomItemsArray = randomItems.ToArray();
                PickableItem pickableItem = prefab.Prefab.GetComponent<PickableItem>();
                pickableItem.m_randomItemPrefabs = randomItemsArray;
                
                prefab.Prefab.FixReferences(true);
                
                PrefabManager.Instance.AddPrefab(prefab);
                return prefab.Prefab;
            }
        }
        return null;
    }
}