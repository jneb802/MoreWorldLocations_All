
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
        AssetBundle assetBundle = Mountains_Pack_1Plugin.assetBundle;

        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneCastle1", Mountains_Pack_1Plugin.MWL_StoneCastle1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneCastle1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneFort1", Mountains_Pack_1Plugin.MWL_StoneFort1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneFort1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneHall1", Mountains_Pack_1Plugin.MWL_StoneHall1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneHall1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneTavern1", Mountains_Pack_1Plugin.MWL_StoneTavern1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneTavern1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneTower1", Mountains_Pack_1Plugin.MWL_StoneTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneTower1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneTower2", Mountains_Pack_1Plugin.MWL_StoneTower2_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneTower2_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_WoodBarn1", Mountains_Pack_1Plugin.MWL_WoodBarn1_Configuration, LocationConfigs.AllLocationConfigs["MWL_WoodBarn1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_WoodFarm1", Mountains_Pack_1Plugin.MWL_WoodFarm1_Configuration, LocationConfigs.AllLocationConfigs["MWL_WoodFarm1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_WoodHouse1", Mountains_Pack_1Plugin.MWL_WoodHouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_WoodHouse1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}