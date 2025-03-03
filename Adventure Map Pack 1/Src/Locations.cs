
using System.Collections.Generic;
using Adventure_Map_Pack_1;
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
        var assetBundle = Adventure_Map_Pack_1Plugin.assetBundle;
        var creatureYAMLContent = Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.creatureYAMLContent;
        var lootYAMLContent = Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.lootYAMLContent;
        
        LocationManager.AddLocation(assetBundle, 
    "MWL_ForestCamp1",
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetCreatureYamlContent(Adventure_Map_Pack_1Plugin.MWL_ForestCamp1_CreatureYaml_Config.Value),
            Adventure_Map_Pack_1Plugin.MWL_ForestCamp1_CreatureList_Config.Value,
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetLootYamlContent(Adventure_Map_Pack_1Plugin.MWL_ForestCamp1_LootYaml_Config.Value), 
            Adventure_Map_Pack_1Plugin.MWL_ForestCamp1_LootList_Config.Value,
            LocationConfigs.MWL_ForestCamp1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_Misthut2", 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetCreatureYamlContent(Adventure_Map_Pack_1Plugin.MWL_Misthut2_CreatureYaml_Config.Value),
            Adventure_Map_Pack_1Plugin.MWL_Misthut2_CreatureList_Config.Value, 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetLootYamlContent(Adventure_Map_Pack_1Plugin.MWL_Misthut2_LootYaml_Config.Value), 
            Adventure_Map_Pack_1Plugin.MWL_Misthut2_LootList_Config.Value,
            LocationConfigs.MWL_MistHut2_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_MountainDvergrShrine1", 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetCreatureYamlContent(Adventure_Map_Pack_1Plugin.MWL_MountainDvergrShrine1_CreatureYaml_Config.Value),
            Adventure_Map_Pack_1Plugin.MWL_MountainDvergrShrine1_CreatureList_Config.Value, 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetLootYamlContent(Adventure_Map_Pack_1Plugin.MWL_MountainDvergrShrine1_LootYaml_Config.Value), 
            Adventure_Map_Pack_1Plugin.MWL_MountainDvergrShrine1_LootList_Config.Value,
            LocationConfigs.MWL_MountainDvergrShrine1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_MountainShrine1", 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetCreatureYamlContent(Adventure_Map_Pack_1Plugin.MWL_MountainShrine1_CreatureYaml_Config.Value),
            Adventure_Map_Pack_1Plugin.MWL_MountainShrine1_CreatureList_Config.Value, 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetLootYamlContent(Adventure_Map_Pack_1Plugin.MWL_MountainShrine1_LootYaml_Config.Value), 
            Adventure_Map_Pack_1Plugin.MWL_MountainShrine1_LootList_Config.Value,
            LocationConfigs.MWL_MountainShrine1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinedTower1", 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetCreatureYamlContent(Adventure_Map_Pack_1Plugin.MWL_RuinedTower1_CreatureYaml_Config.Value),
            Adventure_Map_Pack_1Plugin.MWL_RuinedTower1_CreatureList_Config.Value, 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetLootYamlContent(Adventure_Map_Pack_1Plugin.MWL_RuinedTower1_LootYaml_Config.Value), 
            Adventure_Map_Pack_1Plugin.MWL_RuinedTower1_LootList_Config.Value,
            LocationConfigs.MWL_RuinedTower1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_TreeTowers1", 
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetCreatureYamlContent(Adventure_Map_Pack_1Plugin.MWL_TreeTowers1_CreatureYaml_Config.Value),
            Adventure_Map_Pack_1Plugin.MWL_TreeTowers1_CreatureList_Config.Value,
            Adventure_Map_Pack_1Plugin.adventuremap1YAMLmanager.GetLootYamlContent(Adventure_Map_Pack_1Plugin.MWL_TreeTowers1_LootYaml_Config.Value), 
            Adventure_Map_Pack_1Plugin.MWL_TreeTowers1_LootList_Config.Value,
            LocationConfigs.MWL_TreeTowers1_Config);

        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}