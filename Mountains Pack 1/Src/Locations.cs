
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
        var creatureYAMLContent = Mountains_Pack_1Plugin.MountainYAML.creatureYAMLContent;
        var lootYAMLContent = Mountains_Pack_1Plugin.MountainYAML.lootYAMLContent;

        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneCastle1", 
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneCastle1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneCastle1_CreatureListConfig.Value, 
            14, 
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneCastle1_LootYamlConfig.Value), 
            Mountains_Pack_1Plugin.MWL_StoneCastle1_LootListConfig.Value,
            LocationConfigs.MWL_StoneCastle1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneFort1", 
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneFort1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneFort1_CreatureListConfig.Value, 
            8, 
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneFort1_LootYamlConfig.Value), 
            Mountains_Pack_1Plugin.MWL_StoneFort1_LootListConfig.Value,
            LocationConfigs.MWL_StoneFort1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneHall1", 
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneHall1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneHall1_CreatureListConfig.Value, 
            6, 
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneHall1_LootYamlConfig.Value), 
            Mountains_Pack_1Plugin.MWL_StoneHall1_LootListConfig.Value,
            LocationConfigs.MWL_StoneHall1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_StoneTavern1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneTavern1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneTavern1_CreatureListConfig.Value,
            9,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneTavern1_LootYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneTavern1_LootListConfig.Value,
            LocationConfigs.MWL_StoneTavern1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_StoneTower1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneTower1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneTower1_CreatureListConfig.Value,
            4,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneTower1_LootYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneTower1_LootListConfig.Value,
            LocationConfigs.MWL_StoneTower1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_StoneTower2",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneTower2_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneTower2_CreatureListConfig.Value,
            5,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneTower2_LootYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_StoneTower2_LootListConfig.Value,
            LocationConfigs.MWL_StoneTower2_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_WoodBarn1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_WoodBarn1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_WoodBarn1_CreatureListConfig.Value,
            7,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_WoodBarn1_LootYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_WoodBarn1_LootListConfig.Value,
            LocationConfigs.MWL_WoodBarn1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_WoodFarm1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_WoodFarm1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_WoodFarm1_CreatureListConfig.Value,
            2,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_WoodFarm1_LootYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_WoodFarm1_LootListConfig.Value,
            LocationConfigs.MWL_WoodFarm1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_WoodHouse1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_WoodHouse1_CreatureYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_WoodHouse1_CreatureListConfig.Value,
            5,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_WoodHouse1_LootYamlConfig.Value),
            Mountains_Pack_1Plugin.MWL_WoodHouse1_LootListConfig.Value,
            LocationConfigs.MWL_WoodHouse1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}