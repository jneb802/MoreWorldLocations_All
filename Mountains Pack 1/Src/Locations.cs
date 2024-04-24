
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;
using Mountains_Pack_1;

namespace Mountains_Pack_1;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = Mountains_Pack_1Plugin.assetBundle;
        var creatureYAMLContent = YAMLManager.creatureYAMLContent;
        var lootYAMLContent = YAMLManager.lootYAMLContent;

        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneCastle1", 
            YAMLManager.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneCastle1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneCastle1_CreatureListConfig.Value, 
            14, 
            YAMLManager.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneCastle1_LootYamlConfig.Value), 
            Mountains_Pack_1Plugin.MWL_StoneCastle1_LootListConfig.Value,
            LocationConfigs.MWL_StoneCastle1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneFort1", 
            YAMLManager.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneFort1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneFort1_CreatureListConfig.Value, 
            8, 
            YAMLManager.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneFort1_LootYamlConfig.Value), 
            Mountains_Pack_1Plugin.MWL_StoneFort1_LootListConfig.Value,
            LocationConfigs.MWL_StoneFort1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneHall1", 
            YAMLManager.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneHall1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneHall1_CreatureListConfig.Value, 
            6, 
            YAMLManager.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneHall1_LootYamlConfig.Value), 
            Mountains_Pack_1Plugin.MWL_StoneHall1_LootListConfig.Value,
            LocationConfigs.MWL_StoneHall1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}