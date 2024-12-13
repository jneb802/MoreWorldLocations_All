using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Plains_Pack_1;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = Plains_Pack_1Plugin.assetBundle;
        var creatureYAMLContent = Plains_Pack_1Plugin.plainsYAMLmanager.creatureYAMLContent;
        var lootYAMLContent = Plains_Pack_1Plugin.plainsYAMLmanager.lootYAMLContent;
        
        LocationManager.AddLocation(assetBundle,
            "MWL_GoblinFort1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_GoblinFort1_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_GoblinFort1_CreatureListConfig.Value,
            15,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_GoblinFort1_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_GoblinFort1_LootListConfig.Value,
            LocationConfigs.MWL_GoblinFort1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingRock1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingRock1_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingRock1_CreatureListConfig.Value,
            5,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingRock1_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingRock1_LootListConfig.Value,
            LocationConfigs.MWL_FulingRock1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingVillage1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingVillage1_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingVillage1_CreatureListConfig.Value,
            10,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingVillage1_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingVillage1_LootListConfig.Value,
            LocationConfigs.MWL_FulingVillage1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingVillage2",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingVillage2_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingVillage2_CreatureListConfig.Value,
            10,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingVillage2_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingVillage2_LootListConfig.Value,
            LocationConfigs.MWL_FulingVillage2_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_PlainsPillar1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_PlainsPillar1_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_PlainsPillar1_CreatureListConfig.Value,
            22,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_PlainsPillar1_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_PlainsPillar1_LootListConfig.Value,
            LocationConfigs.MWL_PlainsPillar1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingTemple1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple1_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple1_CreatureListConfig.Value,
            9,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple1_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple1_LootListConfig.Value,
            LocationConfigs.MWL_FulingTemple1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingTemple2",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple2_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple2_CreatureListConfig.Value,
            9,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple2_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple2_LootListConfig.Value,
            LocationConfigs.MWL_FulingTemple2_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingTemple3",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple3_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple3_CreatureListConfig.Value,
            9,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple3_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple3_LootListConfig.Value,
            LocationConfigs.MWL_FulingTemple3_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingWall1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingWall1_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingWall1_CreatureListConfig.Value,
            6,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingWall1_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingWall1_LootListConfig.Value,
            LocationConfigs.MWL_FulingWall1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingTower1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingTower1_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingTower1_CreatureListConfig.Value,
            6,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingTower1_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_FulingTower1_LootListConfig.Value,
            LocationConfigs.MWL_FulingTower1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_GoblinCave1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_GoblinCave1_CreatureYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_GoblinCave1_CreatureListConfig.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_GoblinCave1_LootYamlConfig.Value),
            Plains_Pack_1Plugin.MWL_GoblinCave1_LootListConfig.Value,
            LocationConfigs.MWL_GoblinCave1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}