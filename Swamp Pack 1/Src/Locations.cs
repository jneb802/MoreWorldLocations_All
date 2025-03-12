
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
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_GuckPit1_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_GuckPit1_Configuration.CreatureList.Value, 
            1, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_GuckPit1_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_GuckPit1_Configuration.LootList.Value,
            LocationConfigs.MWL_GuckPit1_Config);

        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampAltar1", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar1_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampAltar1_Configuration.CreatureList.Value, 
            5, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar1_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_SwampAltar1_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampAltar1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampAltar2", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar2_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampAltar2_Configuration.CreatureList.Value, 
            6, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar2_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_SwampAltar2_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampAltar2_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampAltar3", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar3_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampAltar3_Configuration.CreatureList.Value, 
            4, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar3_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_SwampAltar3_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampAltar3_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampAltar4", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar4_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampAltar4_Configuration.CreatureList.Value, 
            5, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampAltar4_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_SwampAltar4_Configuration.LootList.Value,
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
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampCastle2_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampCastle2_Configuration.CreatureList.Value, 
            11, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampCastle2_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_SwampCastle2_Configuration.LootList.Value,
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
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampGrave1_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampGrave1_Configuration.CreatureList.Value, 
            0, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampGrave1_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_SwampGrave1_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampGrave1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampHouse1", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampHouse1_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampHouse1_Configuration.CreatureList.Value, 
            3, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampHouse1_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_SwampHouse1_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampHouse1_Config);
        
        LocationManager.AddLocation(assetBundle, 
            "MWL_SwampRuin1", 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampRuin1_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampRuin1_Configuration.CreatureList.Value, 
            8, 
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampRuin1_Configuration.LootYaml.Value), 
            Swamp_Pack_1Plugin.MWL_SwampRuin1_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampRuin1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_SwampTower1",
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower1_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower1_Configuration.CreatureList.Value,
            4,
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower1_Configuration.LootYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower1_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampTower1_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_SwampTower2",
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower2_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower2_Configuration.CreatureList.Value,
            2,
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower2_Configuration.LootYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower2_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampTower2_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_SwampTower3",
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower3_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower3_Configuration.CreatureList.Value,
            3,
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampTower3_Configuration.LootYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampTower3_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampTower3_Config);

        LocationManager.AddLocation(assetBundle,
            "MWL_SwampWell1",
            Swamp_Pack_1Plugin.swampYAMLmanager.GetCreatureYamlContent(Swamp_Pack_1Plugin.MWL_SwampWell1_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampWell1_Configuration.CreatureList.Value,
            0,
            Swamp_Pack_1Plugin.swampYAMLmanager.GetLootYamlContent(Swamp_Pack_1Plugin.MWL_SwampWell1_Configuration.CreatureYaml.Value),
            Swamp_Pack_1Plugin.MWL_SwampWell1_Configuration.LootList.Value,
            LocationConfigs.MWL_SwampWell1_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}