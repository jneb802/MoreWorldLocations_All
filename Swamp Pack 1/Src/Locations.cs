
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Swamp_Pack_1;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = Swamp_Pack_1Plugin.assetBundle;
        var creatureYAMLContent = Swamp_Pack_1Plugin.swampYAMLmanager.creatureYAMLContent;
        var lootYAMLContent = Swamp_Pack_1Plugin.swampYAMLmanager.lootYAMLContent;
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_GuckPit1", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_GuckPit1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_GuckPit1_CreatureListConfig.Value, 
            1, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_GuckPit1_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_GuckPit1_LootListConfig.Value,
            LocationConfigs.MWL_GuckPit1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampAltar1", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampAltar1_CreatureListConfig.Value, 
            5, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar1_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampAltar1_LootListConfig.Value,
            LocationConfigs.MWL_SwampAltar1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampAltar2", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar2_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampAltar2_CreatureListConfig.Value, 
            6, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar2_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampAltar2_LootListConfig.Value,
            LocationConfigs.MWL_SwampAltar2_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampAltar3", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar3_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampAltar3_CreatureListConfig.Value, 
            4, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar3_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampAltar3_LootListConfig.Value,
            LocationConfigs.MWL_SwampAltar3_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampAltar4", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar4_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampAltar4_CreatureListConfig.Value, 
            5, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar4_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampAltar4_LootListConfig.Value,
            LocationConfigs.MWL_SwampAltar4_Config);
        
        /*LocationManager.AddLocation(assetBundle, 
            "MWL_SwampCastle1", 
            YAMLManager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampCastle1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampCastle1_CreatureListConfig.Value, 
            11, 
            YAMLManager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampCastle1_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampCastle1_LootListConfig.Value,
            LocationConfigs.MWL_SwampCastle1_Config);*/
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampCastle2", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampCastle2_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampCastle2_CreatureListConfig.Value, 
            11, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampCastle2_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampCastle2_LootListConfig.Value,
            LocationConfigs.MWL_SwampCastle2_Config);
        
        /*LocationManager.AddLocation(assetBundle, 
            "MWL_SwampChurch1", 
            YAMLManager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampChurch1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampChurch1_CreatureListConfig.Value, 
            11, 
            YAMLManager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampChurch1_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampChurch1_LootListConfig.Value,
            LocationConfigs.MWL_SwampChurch1_Config);*/
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampGrave1", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampGrave1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampGrave1_CreatureListConfig.Value, 
            0, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampGrave1_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampGrave1_LootListConfig.Value,
            LocationConfigs.MWL_SwampGrave1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampHouse1", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampHouse1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampHouse1_CreatureListConfig.Value, 
            3, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampHouse1_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampHouse1_LootListConfig.Value,
            LocationConfigs.MWL_SwampHouse1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampRuin1", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampRuin1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampRuin1_CreatureListConfig.Value, 
            8, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampRuin1_LootYamlConfig.Value), 
            Swamp_Pack_1Plugin.MWL_SwampRuin1_LootListConfig.Value,
            LocationConfigs.MWL_SwampRuin1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_SwampTower1",
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampRuin1_CreatureListConfig.Value,
            4,
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower1_LootYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower1_LootListConfig.Value,
            LocationConfigs.MWL_SwampTower1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_SwampTower2",
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower2_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower2_CreatureListConfig.Value,
            2,
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower2_LootYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower2_LootListConfig.Value,
            LocationConfigs.MWL_SwampTower2_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_SwampTower3",
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower3_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower3_CreatureListConfig.Value,
            3,
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower3_LootYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower3_LootListConfig.Value,
            LocationConfigs.MWL_SwampTower3_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_SwampWell1",
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampWell1_CreatureYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampWell1_CreatureListConfig.Value,
            0,
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampWell1_LootYamlConfig.Value),
            Swamp_Pack_1Plugin.MWL_SwampWell1_LootListConfig.Value,
            LocationConfigs.MWL_SwampWell1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}