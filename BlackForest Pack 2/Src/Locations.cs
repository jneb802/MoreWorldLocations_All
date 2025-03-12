
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
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestForge1_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_ForestForge1_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestForge1_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestForge1_Configuration.LootList.Value, 
            LocationConfigs.MWL_ForestForge1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestForge2", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestForge2_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_ForestForge2_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestForge2_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestForge2_Configuration.LootList.Value,
            LocationConfigs.MWL_ForestForge2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestGreatHouse2", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_Configuration.LootList.Value,
            LocationConfigs.MWL_ForestGreatHouse2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestHouse2", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestHouse2_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_ForestHouse2_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestHouse2_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestHouse2_Configuration.LootList.Value,
            LocationConfigs.MWL_ForestHouse2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestRuin1", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestRuin1_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_ForestRuin1_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestRuin1_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestRuin1_Configuration.LootList.Value,
            LocationConfigs.MWL_ForestRuin1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestTower2", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestTower2_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_ForestTower2_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestTower2_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestTower2_Configuration.LootList.Value,
            LocationConfigs.MWL_ForestTower2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_ForestTower3", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_ForestTower3_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_ForestTower3_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_ForestTower3_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_ForestTower3_Configuration.LootList.Value,
            LocationConfigs.MWL_ForestTower3_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_MassGrave1", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_MassGrave1_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_MassGrave1_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_MassGrave1_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_MassGrave1_Configuration.LootList.Value,
            LocationConfigs.MWL_MassGrave1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneFormation1", 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetCreatureYamlContent(BlackForest_Pack_2Plugin.MWL_StoneFormation1_Configuration.CreatureYaml.Value),
            BlackForest_Pack_2Plugin.MWL_StoneFormation1_Configuration.CreatureList.Value, 
            BlackForest_Pack_2Plugin.blackforest2YAMLmanager.GetLootYamlContent(BlackForest_Pack_2Plugin.MWL_StoneFormation1_Configuration.LootYaml.Value), 
            BlackForest_Pack_2Plugin.MWL_StoneFormation1_Configuration.LootList.Value,
            LocationConfigs.MWL_StoneFormation1_Config);

        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}