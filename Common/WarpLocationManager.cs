
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Meadows_Pack_1;

public class WarpLocationManager
{
    
    public enum LocationPosition
    {
        Interior,
        Exterior
    }
    
    public static void AddAllLocations()
    {
        var assetBundle = WarpAssetManager.assetBundle;
        var creatureYAMLContent = WarpYAMLManager.creatureYAMLContent;
        var lootYAMLContent = WarpYAMLManager.lootYAMLContent;

        AddLocation(assetBundle, 
            "MWL_Ruins1", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins1_CreatureList_Config.Value, 
            0, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins1_LootList_Config.Value,
            WarpLocationConfigs.MWL_Ruins1_Config);
        AddLocation(assetBundle, 
            "MWL_Ruins2", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins2_CreatureYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins2_CreatureList_Config.Value,
            3, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins2_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins2_LootList_Config.Value,
            WarpLocationConfigs.MWL_Ruins2_Config);
        AddLocation(assetBundle, 
            "MWL_Ruins3", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins3_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins3_CreatureList_Config.Value, 
            2, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins3_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins3_LootList_Config.Value,
            WarpLocationConfigs.MWL_Ruins3_Config);
        AddLocation(assetBundle, 
            "MWL_Ruins6", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins6_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins6_CreatureList_Config.Value, 
            3, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins6_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins6_LootList_Config.Value,
            WarpLocationConfigs.MWL_Ruins6_Config);
        AddLocation(assetBundle, 
            "MWL_Ruins7", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins7_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins7_CreatureList_Config.Value, 
            3, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins7_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins7_LootList_Config.Value,
            WarpLocationConfigs.MWL_Ruins7_Config);
        AddLocation(assetBundle, 
            "MWL_Ruins8", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins8_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins8_CreatureList_Config.Value, 
            3, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins8_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins8_LootList_Config.Value,
            WarpLocationConfigs.MWL_Ruins8_Config);
        AddLocation(assetBundle, 
            "MWL_RuinsArena1", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsArena1_CreatureList_Config.Value, 
            5, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsArena1_LootList_Config.Value,
            WarpLocationConfigs.MWL_RuinsArena1_Config);
        AddLocation(assetBundle, 
            "MWL_RuinsArena3", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena3_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsArena3_CreatureList_Config.Value, 
            4, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena3_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsArena3_LootList_Config.Value,
            WarpLocationConfigs.MWL_RuinsArena3_Config);
        AddLocation(assetBundle, 
            "MWL_RuinsChurch1", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsChurch1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsChurch1_CreatureList_Config.Value, 
            3, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsChurch1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsChurch1_LootList_Config.Value,
            WarpLocationConfigs.MWL_RuinsChurch1_Config);
        AddLocation(assetBundle, 
            "MWL_RuinsWell1", 
            WarpYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsWell1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsWell1_CreatureList_Config.Value, 
            0, 
            WarpYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsWell1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsWell1_LootList_Config.Value,
            WarpLocationConfigs.MWL_RuinsWell1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }

    public static void AddLocation(AssetBundle assetBundle, string locationName, string creatureYAMLContent, string creatureListName, int creatureCount, string lootYAMLContent, string lootListName, LocationConfig locationConfig)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        
        WarpCreatureManager.SetupCreatures(LocationPosition.Exterior,creatureListName,jotunnLocationContainer,creatureCount,creatureYAMLContent);
        
        var lootList = WarpLootManager.CreateLootList(lootListName, lootYAMLContent);
        var locationChestContainers = WarpLootManager.GetLocationsContainers(jotunnLocationContainer,LocationPosition.Exterior);
        WarpLootManager.SetupChestLoot(locationChestContainers,lootList);
        var locationDropOnDestroyedLoot = WarpLootManager.GetLocationsDropOnDestroyeds(jotunnLocationContainer,LocationPosition.Exterior);
        if (locationDropOnDestroyedLoot != null)
        {
            WarpLootManager.SetupDropOnDestroyedLoot(locationDropOnDestroyedLoot,lootList);
        }
        
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
}