
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Meadows_Pack_1;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = Meadows_Pack_1Plugin.assetBundle;
        var creatureYAMLContent = YAMLManager.creatureYAMLContent;
        var lootYAMLContent = YAMLManager.lootYAMLContent;

        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins1", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins1_CreatureList_Config.Value, 
            0, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins1_LootList_Config.Value,
            LocationConfigs.MWL_Ruins1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins2", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins2_CreatureYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins2_CreatureList_Config.Value,
            3, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins2_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins2_LootList_Config.Value,
            LocationConfigs.MWL_Ruins2_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins3", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins3_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins3_CreatureList_Config.Value, 
            2, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins3_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins3_LootList_Config.Value,
            LocationConfigs.MWL_Ruins3_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins6", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins6_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins6_CreatureList_Config.Value, 
            3, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins6_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins6_LootList_Config.Value,
            LocationConfigs.MWL_Ruins6_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins7", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins7_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins7_CreatureList_Config.Value, 
            3, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins7_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins7_LootList_Config.Value,
            LocationConfigs.MWL_Ruins7_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins8", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins8_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins8_CreatureList_Config.Value, 
            3, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins8_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins8_LootList_Config.Value,
            LocationConfigs.MWL_Ruins8_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsArena1", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsArena1_CreatureList_Config.Value, 
            5, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsArena1_LootList_Config.Value,
            LocationConfigs.MWL_RuinsArena1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsArena3", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena3_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsArena3_CreatureList_Config.Value, 
            4, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena3_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsArena3_LootList_Config.Value,
            LocationConfigs.MWL_RuinsArena3_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsChurch1", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsChurch1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsChurch1_CreatureList_Config.Value, 
            3, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsChurch1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsChurch1_LootList_Config.Value,
            LocationConfigs.MWL_RuinsChurch1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsWell1", 
            YAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsWell1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsWell1_CreatureList_Config.Value, 
            0, 
            YAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsWell1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsWell1_LootList_Config.Value,
            LocationConfigs.MWL_RuinsWell1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}