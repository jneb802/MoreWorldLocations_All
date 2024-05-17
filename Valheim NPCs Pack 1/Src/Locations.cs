
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Valheim_NPCs_Pack_1;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = Valheim_NPCs_Pack_1Plugin.assetBundle;
        var creatureYAMLContent = YAMLManager.creatureYAMLContent;
        var lootYAMLContent = YAMLManager.lootYAMLContent;
        
        Common.LocationManager.AddLocation(assetBundle, 
            "MWL_FenrisMaw", 
            YAMLManager.GetCreatureYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_FenrisMaw_CreatureYamlConfig.Value),
            Valheim_NPCs_Pack_1Plugin.MWL_FenrisMaw_CreatureListConfig.Value, 
            0, 
            YAMLManager.GetLootYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_FenrisMaw_LootYamlConfig.Value), 
            Valheim_NPCs_Pack_1Plugin.MWL_FenrisMaw_LootListConfig.Value,
            LocationConfigs.MWL_FenrisMaw_Config);
        
        Common.LocationManager.AddLocation(assetBundle, 
            "MWL_MarketplaceLarge1", 
            YAMLManager.GetCreatureYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_MarketplaceLarge1_CreatureYamlConfig.Value),
            Valheim_NPCs_Pack_1Plugin.MWL_MarketplaceLarge1_CreatureListConfig.Value, 
            0, 
            YAMLManager.GetLootYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_MarketplaceLarge1_LootYamlConfig.Value), 
            Valheim_NPCs_Pack_1Plugin.MWL_MarketplaceLarge1_LootListConfig.Value,
            LocationConfigs.MWL_MarketplaceLarge1_Config);
        
        Common.LocationManager.AddLocation(assetBundle, 
            "MWL_MarketSmall1", 
            YAMLManager.GetCreatureYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_MarketSmall1_CreatureYamlConfig.Value),
            Valheim_NPCs_Pack_1Plugin.MWL_MarketSmall1_CreatureListConfig.Value, 
            0, 
            YAMLManager.GetLootYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_MarketSmall1_LootYamlConfig.Value), 
            Valheim_NPCs_Pack_1Plugin.MWL_MarketSmall1_LootListConfig.Value,
            LocationConfigs.MWL_MarketSmall1_Config);
        
        Common.LocationManager.AddLocation(assetBundle, 
            "MWL_ShopSmall1", 
            YAMLManager.GetCreatureYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_ShopSmall1_CreatureYamlConfig.Value),
            Valheim_NPCs_Pack_1Plugin.MWL_ShopSmall1_CreatureListConfig.Value, 
            0, 
            YAMLManager.GetLootYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_ShopSmall1_LootYamlConfig.Value), 
            Valheim_NPCs_Pack_1Plugin.MWL_ShopSmall1_LootListConfig.Value,
            LocationConfigs.MWL_ShopSmall1_Config);
        
        Common.LocationManager.AddLocation(assetBundle, 
            "MWL_ThirstyRavenInn", 
            YAMLManager.GetCreatureYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_ThirstyRavenInn_CreatureYamlConfig.Value),
            Valheim_NPCs_Pack_1Plugin.MWL_ThirstyRavenInn_CreatureListConfig.Value, 
            0, 
            YAMLManager.GetLootYamlContent(Valheim_NPCs_Pack_1Plugin.MWL_ThirstyRavenInn_LootYamlConfig.Value), 
            Valheim_NPCs_Pack_1Plugin.MWL_ThirstyRavenInn_LootListConfig.Value,
            LocationConfigs.MWL_ThirstyRavenInn_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}