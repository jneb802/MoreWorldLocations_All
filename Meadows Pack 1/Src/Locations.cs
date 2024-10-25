
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
        var creatureYAMLContent = Meadows_Pack_1Plugin.meadowsYAMLManager.creatureYAMLContent;
        var lootYAMLContent = Meadows_Pack_1Plugin.meadowsYAMLManager.lootYAMLContent;

        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins1", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins1_CreatureList_Config.Value, 
            0, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins1_LootList_Config.Value,
            LocationConfigs.MWL_Ruins1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins2", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins2_CreatureYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins2_CreatureList_Config.Value,
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins2_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins2_LootList_Config.Value,
            LocationConfigs.MWL_Ruins2_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins3", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins3_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins3_CreatureList_Config.Value, 
            2, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins3_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins3_LootList_Config.Value,
            LocationConfigs.MWL_Ruins3_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins6", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins6_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins6_CreatureList_Config.Value, 
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins6_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins6_LootList_Config.Value,
            LocationConfigs.MWL_Ruins6_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins7", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins7_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins7_CreatureList_Config.Value, 
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins7_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins7_LootList_Config.Value,
            LocationConfigs.MWL_Ruins7_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins8", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins8_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_Ruins8_CreatureList_Config.Value, 
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins8_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins8_LootList_Config.Value,
            LocationConfigs.MWL_Ruins8_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsArena1", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsArena1_CreatureList_Config.Value, 
            5, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsArena1_LootList_Config.Value,
            LocationConfigs.MWL_RuinsArena1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsArena3", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena3_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsArena3_CreatureList_Config.Value, 
            4, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena3_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsArena3_LootList_Config.Value,
            LocationConfigs.MWL_RuinsArena3_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsChurch1", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsChurch1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsChurch1_CreatureList_Config.Value, 
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsChurch1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsChurch1_LootList_Config.Value,
            LocationConfigs.MWL_RuinsChurch1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsWell1", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsWell1_CreatureYaml_Config.Value),
            Meadows_Pack_1Plugin.MWL_RuinsWell1_CreatureList_Config.Value, 
            0, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsWell1_LootYaml_Config.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsWell1_LootList_Config.Value,
            LocationConfigs.MWL_RuinsWell1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}