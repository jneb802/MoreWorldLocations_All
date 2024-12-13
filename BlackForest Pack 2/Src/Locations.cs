
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace BlackForest_Pack_2;

public class Locations
{
    public enum LocationPosition
    {
        Interior,
        Exterior
    }
    
    public static void AddAllLocations()
    {
        AssetBundle assetBundle = BlackForest_Pack_2Plugin.assetBundle;
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestForge1", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestForge1_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_ForestForge1_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestForge1_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestForge1_LootList_Config.Value, 
            LocationConfigs.MWL_ForestForge1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestForge2", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestForge2_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_ForestForge2_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestForge2_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestForge2_LootList_Config.Value,
            LocationConfigs.MWL_ForestForge2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestGreatHouse2", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_LootList_Config.Value,
            LocationConfigs.MWL_ForestGreatHouse2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestHouse2", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestHouse2_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_ForestHouse2_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestHouse2_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestHouse2_LootList_Config.Value,
            LocationConfigs.MWL_ForestHouse2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestRuin1", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestRuin1_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_ForestRuin1_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestRuin1_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestRuin1_LootList_Config.Value,
            LocationConfigs.MWL_ForestRuin1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestTower2", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestTower2_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_ForestTower2_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestTower2_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestTower2_LootList_Config.Value,
            LocationConfigs.MWL_ForestTower2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestTower3", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestTower3_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_ForestTower3_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestTower3_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestTower3_LootList_Config.Value,
            LocationConfigs.MWL_ForestTower3_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_MassGrave1", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_MassGrave1_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_MassGrave1_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_MassGrave1_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_MassGrave1_LootList_Config.Value,
            LocationConfigs.MWL_MassGrave1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneFormation1", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_StoneFormation1_CreatureYaml_Config.Value),
            BlackForest_Pack_2Plugin.MWL_StoneFormation1_CreatureList_Config.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_StoneFormation1_LootYaml_Config.Value), 
            BlackForest_Pack_2Plugin.MWL_StoneFormation1_LootList_Config.Value,
            LocationConfigs.MWL_StoneFormation1_Config);

        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}