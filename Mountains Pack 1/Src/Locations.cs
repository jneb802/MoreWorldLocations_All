
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
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneCastle1_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneCastle1_Configuration.CreatureList.Value,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneCastle1_Configuration.LootYaml.Value), 
            Mountains_Pack_1Plugin.MWL_StoneCastle1_Configuration.LootList.Value,
            LocationConfigs.MWL_StoneCastle1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneFort1", 
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneFort1_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneFort1_Configuration.CreatureList.Value, 
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneFort1_Configuration.LootYaml.Value), 
            Mountains_Pack_1Plugin.MWL_StoneFort1_Configuration.LootList.Value,
            LocationConfigs.MWL_StoneFort1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_StoneHall1", 
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneHall1_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneHall1_Configuration.CreatureList.Value, 
            6, 
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneHall1_Configuration.LootYaml.Value), 
            Mountains_Pack_1Plugin.MWL_StoneHall1_Configuration.LootList.Value,
            LocationConfigs.MWL_StoneHall1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_StoneTavern1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneTavern1_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneTavern1_Configuration.CreatureList.Value,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneTavern1_Configuration.LootYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneTavern1_Configuration.LootList.Value,
            LocationConfigs.MWL_StoneTavern1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_StoneTower1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneTower1_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneTower1_Configuration.CreatureList.Value,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneTower1_Configuration.LootYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneTower1_Configuration.LootList.Value,
            LocationConfigs.MWL_StoneTower1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_StoneTower2",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_StoneTower2_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneTower2_Configuration.CreatureList.Value,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_StoneTower2_Configuration.LootYaml.Value),
            Mountains_Pack_1Plugin.MWL_StoneTower2_Configuration.LootList.Value,
            LocationConfigs.MWL_StoneTower2_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_WoodBarn1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_WoodBarn1_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_WoodBarn1_Configuration.CreatureList.Value,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_WoodBarn1_Configuration.LootYaml.Value),
            Mountains_Pack_1Plugin.MWL_WoodBarn1_Configuration.LootList.Value,
            LocationConfigs.MWL_WoodBarn1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_WoodFarm1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_WoodFarm1_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_WoodFarm1_Configuration.CreatureList.Value,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_WoodFarm1_Configuration.LootYaml.Value),
            Mountains_Pack_1Plugin.MWL_WoodFarm1_Configuration.LootList.Value,
            LocationConfigs.MWL_WoodFarm1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_WoodHouse1",
            Mountains_Pack_1Plugin.MountainYAML.GetCreatureYamlContent(Mountains_Pack_1Plugin.MWL_WoodHouse1_Configuration.CreatureYaml.Value),
            Mountains_Pack_1Plugin.MWL_WoodHouse1_Configuration.CreatureList.Value,
            Mountains_Pack_1Plugin.MountainYAML.GetLootYamlContent(Mountains_Pack_1Plugin.MWL_WoodHouse1_Configuration.LootYaml.Value),
            Mountains_Pack_1Plugin.MWL_WoodHouse1_Configuration.LootList.Value,
            LocationConfigs.MWL_WoodHouse1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}