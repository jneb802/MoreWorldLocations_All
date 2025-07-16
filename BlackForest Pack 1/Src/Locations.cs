
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
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsArena2_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsArena2_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsArena2_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsArena2_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsArena2_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsCastle1", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsCastle1_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsCastle1_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsCastle1_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsCastle1_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsCastle1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsCastle3", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsCastle3_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsCastle3_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsCastle3_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsCastle3_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsCastle3_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsTower3", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTower3_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsTower3_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTower3_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsTower3_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsTower3_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsTower8", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTower8_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsTower8_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTower8_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsTower8_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsTower8_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_Tavern1", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTavern1_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_RuinsTavern1_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_RuinsTavern1_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_RuinsTavern1_Configuration.LootList.Value,
            LocationConfigs.MWL_Tavern1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_WoodTower1", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower1_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_WoodTower1_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower1_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_WoodTower1_Configuration.LootList.Value,
            LocationConfigs.MWL_WoodTower1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_WoodTower2", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower2_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_WoodTower2_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower2_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_WoodTower2_Configuration.LootList.Value,
            LocationConfigs.MWL_WoodTower2_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_WoodTower3", 
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetCreatureYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower3_Configuration.CreatureYaml.Value),
            BlackForest_Pack_1Plugin.MWL_WoodTower3_Configuration.CreatureList.Value,
            BlackForest_Pack_1Plugin.blackforestYAMLmanager.GetLootYamlContent(BlackForest_Pack_1Plugin.MWL_WoodTower3_Configuration.LootYaml.Value), 
            BlackForest_Pack_1Plugin.MWL_WoodTower3_Configuration.LootList.Value,
            LocationConfigs.MWL_WoodTower3_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}