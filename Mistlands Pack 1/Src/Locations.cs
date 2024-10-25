
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Mistlands_Pack_1;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = Mistlands_Pack_1Plugin.assetBundle;
        var creatureYAMLContent = Mistlands_Pack_1Plugin.mistlandsYAMLManager.creatureYAMLContent;
        var lootYAMLContent = Mistlands_Pack_1Plugin.mistlandsYAMLManager.lootYAMLContent;
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistFort2",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistFort2_CreatureYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistFort2_CreatureListConfig.Value,
            4,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistFort2_LootYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistFort2_LootListConfig.Value,
            LocationConfigs.MWL_MistFort2_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_SecretRoom1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_SecretRoom1_CreatureYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_SecretRoom1_CreatureListConfig.Value,
            5,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_SecretRoom1_LootYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_SecretRoom1_LootListConfig.Value,
            LocationConfigs.MWL_SecretRoom1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistWorkshop1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistWorkshop1_CreatureYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistWorkshop1_CreatureListConfig.Value,
            5,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistWorkshop1_LootYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistWorkshop1_LootListConfig.Value,
            LocationConfigs.MWL_MistWorkshop1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistTower1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistTower1_CreatureYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistTower1_CreatureListConfig.Value,
            4,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistTower1_LootYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistTower1_LootListConfig.Value,
            LocationConfigs.MWL_MistTower1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistWall1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistWall1_CreatureYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistWall1_CreatureListConfig.Value,
            9,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistWall1_LootYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistWall1_LootListConfig.Value,
            LocationConfigs.MWL_MistWall1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistTower2",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistTower2_CreatureYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistTower2_CreatureListConfig.Value,
            6,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistTower2_LootYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistTower2_LootListConfig.Value,
            LocationConfigs.MWL_MistTower2_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistHut1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistHut1_CreatureYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistHut1_CreatureListConfig.Value,
            6,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistHut1_LootYamlConfig.Value),
            Mistlands_Pack_1Plugin.MWL_MistHut1_LootListConfig.Value,
            LocationConfigs.MWL_MistHut1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}