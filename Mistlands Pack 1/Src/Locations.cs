
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
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistFort2_Configuration.CreatureYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistFort2_Configuration.CreatureList.Value,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistFort2_Configuration.LootYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistFort2_Configuration.LootList.Value,
            LocationConfigs.MWL_MistFort2_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_SecretRoom1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_SecretRoom1_Configuration.CreatureYaml.Value),
            Mistlands_Pack_1Plugin.MWL_SecretRoom1_Configuration.CreatureList.Value,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_SecretRoom1_Configuration.LootYaml.Value),
            Mistlands_Pack_1Plugin.MWL_SecretRoom1_Configuration.LootList.Value,
            LocationConfigs.MWL_SecretRoom1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistWorkshop1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistWorkshop1_Configuration.CreatureYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistWorkshop1_Configuration.CreatureList.Value,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistWorkshop1_Configuration.LootYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistWorkshop1_Configuration.LootList.Value,
            LocationConfigs.MWL_MistWorkshop1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistTower1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistTower1_Configuration.CreatureYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistTower1_Configuration.CreatureList.Value,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistTower1_Configuration.LootYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistTower1_Configuration.LootList.Value,
            LocationConfigs.MWL_MistTower1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistWall1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistWall1_Configuration.CreatureYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistWall1_Configuration.CreatureList.Value,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistWall1_Configuration.LootYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistWall1_Configuration.LootList.Value,
            LocationConfigs.MWL_MistWall1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistTower2",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistTower2_Configuration.CreatureYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistTower2_Configuration.CreatureList.Value,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistTower2_Configuration.LootYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistTower2_Configuration.LootList.Value,
            LocationConfigs.MWL_MistTower2_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistHut1",
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetCreatureYamlContent(Mistlands_Pack_1Plugin.MWL_MistHut1_Configuration.CreatureYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistHut1_Configuration.CreatureList.Value,
            Mistlands_Pack_1Plugin.mistlandsYAMLManager.GetLootYamlContent(Mistlands_Pack_1Plugin.MWL_MistHut1_Configuration.LootYaml.Value),
            Mistlands_Pack_1Plugin.MWL_MistHut1_Configuration.LootList.Value,
            LocationConfigs.MWL_MistHut1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}