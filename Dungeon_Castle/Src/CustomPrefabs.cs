using System.Collections.Generic;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Dungeon_Castle;

public class CustomPrefabs
{
    public static void RegisterKitPrefabs()
    {
        Jotunn.Logger.LogDebug("RegisterKitPrefabs invoked");

        var customPrefabs = new List<string>()
        {
            "CD_kit_stone_arch.prefab",
            "CD_kit_stone_floor.prefab",
            "CD_kit_stone_floor_2x2.prefab",
            "CD_kit_stone_pillar.prefab",
            "CD_kit_stone_stair.prefab",
            "CD_kit_stone_wall_1x1.prefab",
            "CD_kit_stone_wall_2x1.prefab",
            "CD_kit_stone_wall_4x2.prefab",
            "CD_kit_iron_floor_1x1",
            "CD_kit_iron_floor_2x2"
        };
        
        var roomSpawnerPrefabs = new List<string>()
        {
        };
        
        var roomContainerPrefabs = new List<string>()
        {
        };
        
        var roomPickablePrefabs = new List<string>()
        {
            
        };
        
        
        foreach (var prefab in customPrefabs)
        {
            RegisterCustomPrefab(Dungeon_CastlePlugin.assetBundle, prefab);
        }
        
        // foreach (var prefab in roomSpawnerPrefabs)
        // {
        //     int randomIndex = Random.Range(0, Dungeon_CastlePlugin.dungeonCastleYamlManager.creatureList.Count);
        //     RegisterCustomPrefabSpawner(Dungeon_CastlePlugin.assetBundle, prefab, Dungeon_CastlePlugin.dungeonCastleYamlManager.creatureList[randomIndex]);
        // }
        //
        // foreach (var prefab in roomContainerPrefabs)
        // {
        //     RegisterCustomPrefabContainer(Dungeon_CastlePlugin.assetBundle, prefab, Dungeon_CastlePlugin.dungeonCastleYamlManager.lootList);
        // }
        //
        // foreach (var prefab in roomPickablePrefabs)
        // {
        //     RegisterCustomPrefabRandomItem(Dungeon_CastlePlugin.assetBundle, prefab, Dungeon_CastlePlugin.dungeonCastleYamlManager.pickableList);
        // }
        
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
        
        public static GameObject RegisterCustomPrefabSpawner(AssetBundle bundle, string assetName, string creatureName)
        {
            string prefabName = assetName.Replace(".prefab", "");
            if (!string.IsNullOrEmpty(prefabName))
            {
                CustomPrefab prefab = new CustomPrefab(bundle, assetName, true);
                Jotunn.Logger.LogDebug("Registering " + prefab.Prefab.name);
                if (prefab != null && prefab.IsValid())
                {
                    CreatureSpawner creatureSpawner = prefab.Prefab.GetComponent<CreatureSpawner>();
                    creatureSpawner.m_creaturePrefab = PrefabManager.Cache.GetPrefab<GameObject>(creatureName);
                    
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