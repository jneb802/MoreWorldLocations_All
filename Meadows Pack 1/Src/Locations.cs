
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
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins1_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_Ruins1_Configuration.CreatureList.Value, 
            0, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins1_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins1_Configuration.LootList.Value,
            LocationConfigs.MWL_Ruins1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins2", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins2_Configuration.CreatureYaml.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins2_Configuration.CreatureList.Value,
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins2_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins2_Configuration.LootList.Value,
            LocationConfigs.MWL_Ruins2_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins3", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins3_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_Ruins3_Configuration.CreatureList.Value, 
            2, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins3_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins3_Configuration.LootList.Value,
            LocationConfigs.MWL_Ruins3_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins6", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins6_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_Ruins6_Configuration.CreatureList.Value, 
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins6_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins6_Configuration.LootList.Value,
            LocationConfigs.MWL_Ruins6_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins7", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins7_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_Ruins7_Configuration.CreatureList.Value, 
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins7_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins7_Configuration.LootList.Value,
            LocationConfigs.MWL_Ruins7_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_Ruins8", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_Ruins8_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_Ruins8_Configuration.CreatureList.Value, 
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_Ruins8_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_Ruins8_Configuration.LootList.Value,
            LocationConfigs.MWL_Ruins8_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsArena1", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena1_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_RuinsArena1_Configuration.CreatureList.Value, 
            5, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena1_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsArena1_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsArena1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsArena3", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena3_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_RuinsArena3_Configuration.CreatureList.Value, 
            4, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsArena3_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsArena3_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsArena3_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsChurch1", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsChurch1_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_RuinsChurch1_Configuration.CreatureList.Value, 
            3, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsChurch1_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsChurch1_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsChurch1_Config);
        LocationManager.AddLocation(assetBundle, 
            "MWL_RuinsWell1", 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetCreatureYamlContent(Meadows_Pack_1Plugin.MWL_RuinsWell1_Configuration.CreatureYaml.Value),
            Meadows_Pack_1Plugin.MWL_RuinsWell1_Configuration.CreatureList.Value, 
            0, 
            Meadows_Pack_1Plugin.meadowsYAMLManager.GetLootYamlContent(Meadows_Pack_1Plugin.MWL_RuinsWell1_Configuration.LootYaml.Value), 
            Meadows_Pack_1Plugin.MWL_RuinsWell1_Configuration.LootList.Value,
            LocationConfigs.MWL_RuinsWell1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}