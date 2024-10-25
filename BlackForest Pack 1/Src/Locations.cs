
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace BlackForest_Pack_1;

public class Locations
{
    public enum LocationPosition
    {
        Interior,
        Exterior
    }
    
    public static void AddAllLocations()
    {
        var assetBundle = BlackForest_Pack_1Plugin.assetBundle;
        var creatureYAMLContent = BlackForest_Pack_1Plugin.blackforestYAMLmanager.creatureYAMLContent;
        var lootYAMLContent = BlackForest_Pack_1Plugin.blackforestYAMLmanager.lootYAMLContent;
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsArena2", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsArena2_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsArena2_CreatureList_Config.Value, 
            10, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsArena2_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsArena2_LootList_Config.Value,
            LocationConfigs.MWL_RuinsArena2_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsCastle1", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsCastle1_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsCastle1_CreatureList_Config.Value, 
            3, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsCastle1_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsCastle1_LootList_Config.Value,
            LocationConfigs.MWL_RuinsCastle1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsCastle3", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsCastle3_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsCastle3_CreatureList_Config.Value, 
            5, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsCastle3_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsCastle3_LootList_Config.Value,
            LocationConfigs.MWL_RuinsCastle3_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsTower3", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTower3_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsTower3_CreatureList_Config.Value, 
            3, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTower3_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsTower3_LootList_Config.Value,
            LocationConfigs.MWL_RuinsTower3_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsTower8", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTower8_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsTower8_CreatureList_Config.Value, 
            2, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTower8_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsTower8_LootList_Config.Value,
            LocationConfigs.MWL_RuinsTower8_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_Tavern1", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_Tavern1_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_Tavern1_CreatureList_Config.Value, 
            3, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_Tavern1_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_Tavern1_LootList_Config.Value,
            LocationConfigs.MWL_Tavern1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_WoodTower1", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower1_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_WoodTower1_CreatureList_Config.Value, 
            5, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower1_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_WoodTower1_LootList_Config.Value,
            LocationConfigs.MWL_WoodTower1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_WoodTower2", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower2_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_WoodTower2_CreatureList_Config.Value, 
            2, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower2_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_WoodTower2_LootList_Config.Value,
            LocationConfigs.MWL_WoodTower2_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_WoodTower3", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower3_CreatureYaml_Config.Value),
            BlackForest_Pack_1Plugin.MWL_WoodTower3_CreatureList_Config.Value, 
            5, 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower3_LootYaml_Config.Value), 
            BlackForest_Pack_1Plugin.MWL_WoodTower3_LootList_Config.Value,
            LocationConfigs.MWL_WoodTower3_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}