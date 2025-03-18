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
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_GoblinFort1_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_GoblinFort1_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_GoblinFort1_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_GoblinFort1_Configuration.LootList.Value,
            LocationConfigs.MWL_GoblinFort1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingRock1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingRock1_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingRock1_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingRock1_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingRock1_Configuration.LootList.Value,
            LocationConfigs.MWL_FulingRock1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingVillage1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingVillage1_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingVillage1_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingVillage1_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingVillage1_Configuration.LootList.Value,
            LocationConfigs.MWL_FulingVillage1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingVillage2",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingVillage2_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingVillage2_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingVillage2_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingVillage2_Configuration.LootList.Value,
            LocationConfigs.MWL_FulingVillage2_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_PlainsPillar1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_PlainsPillar1_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_PlainsPillar1_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_PlainsPillar1_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_PlainsPillar1_Configuration.LootList.Value,
            LocationConfigs.MWL_PlainsPillar1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingTemple1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple1_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple1_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple1_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple1_Configuration.LootList.Value,
            LocationConfigs.MWL_FulingTemple1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingTemple2",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple2_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple2_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple2_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple2_Configuration.LootList.Value,
            LocationConfigs.MWL_FulingTemple2_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingTemple3",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple3_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple3_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingTemple3_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingTemple3_Configuration.LootList.Value,
            LocationConfigs.MWL_FulingTemple3_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingWall1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingWall1_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingWall1_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingWall1_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingWall1_Configuration.LootList.Value,
            LocationConfigs.MWL_FulingWall1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_FulingTower1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_FulingTower1_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingTower1_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_FulingTower1_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_FulingTower1_Configuration.LootList.Value,
            LocationConfigs.MWL_FulingTower1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_GoblinCave1",
            Plains_Pack_1Plugin.plainsYAMLmanager.GetCreatureYamlContent(Plains_Pack_1Plugin.MWL_GoblinCave1_Configuration.CreatureYaml.Value),
            Plains_Pack_1Plugin.MWL_GoblinCave1_Configuration.CreatureList.Value,
            Plains_Pack_1Plugin.plainsYAMLmanager.GetLootYamlContent(Plains_Pack_1Plugin.MWL_GoblinCave1_Configuration.LootYaml.Value),
            Plains_Pack_1Plugin.MWL_GoblinCave1_Configuration.LootList.Value,
            LocationConfigs.MWL_GoblinCave1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}