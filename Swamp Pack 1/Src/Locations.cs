
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
        AssetBundle assetBundle = Swamp_Pack_1Plugin.assetBundle;

        Common.LocationManager.AddLocation(assetBundle, "MWL_GuckPit1", Swamp_Pack_1Plugin.MWL_GuckPit1_Configuration, LocationConfigs.AllLocationConfigs["MWL_GuckPit1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampAltar1", Swamp_Pack_1Plugin.MWL_SwampAltar1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampAltar1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampAltar2", Swamp_Pack_1Plugin.MWL_SwampAltar2_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampAltar2_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampAltar3", Swamp_Pack_1Plugin.MWL_SwampAltar3_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampAltar3_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampAltar4", Swamp_Pack_1Plugin.MWL_SwampAltar4_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampAltar4_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampCastle2", Swamp_Pack_1Plugin.MWL_SwampCastle2_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampCastle2_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampGrave1", Swamp_Pack_1Plugin.MWL_SwampGrave1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampGrave1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampHouse1", Swamp_Pack_1Plugin.MWL_SwampHouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampHouse1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampRuin1", Swamp_Pack_1Plugin.MWL_SwampRuin1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampRuin1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampTower1", Swamp_Pack_1Plugin.MWL_SwampTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampTower1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampTower2", Swamp_Pack_1Plugin.MWL_SwampTower2_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampTower2_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampTower3", Swamp_Pack_1Plugin.MWL_SwampTower3_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampTower3_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampWell1", Swamp_Pack_1Plugin.MWL_SwampWell1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampWell1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_AbandonedHouse1", Swamp_Pack_1Plugin.MWL_AbandonedHouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_AbandonedHouse1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_Treehouse1", Swamp_Pack_1Plugin.MWL_Treehouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_Treehouse1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_Shipyard1", Swamp_Pack_1Plugin.MWL_Shipyard1_Configuration, LocationConfigs.AllLocationConfigs["MWL_Shipyard1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FortBakkarhalt1", Swamp_Pack_1Plugin.MWL_FortBakkarhalt1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FortBakkarhalt1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_Belmont1", Swamp_Pack_1Plugin.MWL_Belmont1_Configuration, LocationConfigs.AllLocationConfigs["MWL_Belmont1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
    
}