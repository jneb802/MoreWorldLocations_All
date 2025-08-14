using System.Collections.Generic;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Forbidden_Catacombs;

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
            "CD_kit_iron_floor_2x2",
            "CD_kit_fi_vil_cath_decor_swords_cross",
            "CD_kit_fi_vil_shield05_a",
            "CD_kit_dungeon_sunkencrypt_irongate_rusty",
            "CD_kit_crypt_skeleton_chest",
            "CD_kit_water",
            "CD_kit_altarHolder",
            "CD_kit_secretdoor",
            "CD_kit_AltarDoor",
            
            "CD_kit_SunkenKit_int_wall_4x4",
            "CD_kit_SunkenKit_int_wall_2x4",
            "CD_kit_SunkenKit_int_floor_4x4",
            "CD_kit_SunkenKit_int_arch",
            "CD_kit_SunkenKit_int_stair",
            
            "CD_kit_sunken_crypt_gate",
            "CD_kit_Spawner_DraugrPile",
            
            "CD_kit_CastleKit_groundtorch",
            "CD_kit_CastleKit_groundtorch_green",
            
            "CD_kit_CryptKey"
        };
        
        var roomUpperSpawnerPrefabs = new List<string>()
        {
            "CD_Room1_Spawner1",
            "CD_Room2_Spawner1",
            
            "CD_RoomBig1_Spawner1",
            "CD_RoomBig1_Spawner2",
            "CD_RoomBig1_Spawner3",
            "CD_RoomBig1_Spawner4",
            
            "CD_RoomBig2_Spawner1",
            "CD_RoomBig2_Spawner2",
            
            "CD_RoomBig3_Spawner1",
            "CD_RoomBig3_Spawner2",
            "CD_RoomBig3_Spawner3",
            "CD_RoomBig3_Spawner4",
        };
        
        var roomLowerSpawnerPrefabs = new List<string>()
        { 
            "CD_Lower_Hallway2_Spawner1",
            "CD_Lower_Hallway2_Spawner2",
            
            "CD_Lower_Hallway4_Spawner1",
            
            "CD_Lower_Room1_Spawner1",
        };
        
        var roomUpperContainerPrefabs = new List<string>()
        {
            "CD_Endcap14_loot_chest_stone1",
            "CD_Room2_loot_chest_stone1",
            "CD_RoomBig1_loot_chest_stone1",
            "CD_RoomBig2_loot_chest_stone1"
        };
        
        var roomLowerContainerPrefabs = new List<string>()
        {
            "CD_Lower_Room1_loot_chest_stone1",
        };
        
        var roomPickablePrefabs = new List<string>()
        {
            
        };
        
        foreach (var prefab in customPrefabs)
        {
            RegisterCustomPrefab(Forbidden_CatacombsPlugin.assetBundle, prefab);
        }
        
        foreach (var prefab in roomUpperSpawnerPrefabs)
        {
            List<GameObject> upperCreatureList =
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.creatureListDictionary[
                    Forbidden_CatacombsPlugin.MWD_CastleDungeon_CreatureListUpper.Value];
            int randomIndex = Random.Range(0, upperCreatureList.Count);
            RegisterCustomPrefabSpawner(Forbidden_CatacombsPlugin.assetBundle, prefab, upperCreatureList[randomIndex]);
        }
        
        foreach (var prefab in roomLowerSpawnerPrefabs)
        {
            List<GameObject> lowerCreatureList =
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.creatureListDictionary[
                    Forbidden_CatacombsPlugin.MWD_CastleDungeon_CreatureListLower.Value];
            int randomIndex = Random.Range(0, lowerCreatureList.Count);
            RegisterCustomPrefabSpawner(Forbidden_CatacombsPlugin.assetBundle, prefab, lowerCreatureList[randomIndex]);
        }
        
        foreach (var prefab in roomUpperContainerPrefabs)
        {
            List<DropTable.DropData> upperLootList =
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.lootListDictionary[
                    Forbidden_CatacombsPlugin.MWD_CastleDungeon_LootListUpper.Value];
            RegisterCustomPrefabContainer(Forbidden_CatacombsPlugin.assetBundle, prefab, upperLootList);
        }
        
        foreach (var prefab in roomLowerContainerPrefabs)
        {
            List<DropTable.DropData> lowerLootList =
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.lootListDictionary[
                    Forbidden_CatacombsPlugin.MWD_CastleDungeon_LootListLower.Value];
            RegisterCustomPrefabContainer(Forbidden_CatacombsPlugin.assetBundle, prefab, lowerLootList);
        }
        
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
        
        public static GameObject RegisterCustomPrefabSpawner(AssetBundle bundle, string assetName, GameObject creatureName)
        {
            string prefabName = assetName.Replace(".prefab", "");
            if (!string.IsNullOrEmpty(prefabName))
            {
                CustomPrefab prefab = new CustomPrefab(bundle, assetName, true);
                Jotunn.Logger.LogDebug("Registering " + prefab.Prefab.name);
                if (prefab != null && prefab.IsValid())
                {
                    CreatureSpawner creatureSpawner = prefab.Prefab.GetComponent<CreatureSpawner>();
                    creatureSpawner.m_creaturePrefab = creatureName;
                    
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