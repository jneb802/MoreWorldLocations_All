
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

        Common.LocationManager.AddLocation(assetBundle, "MWL_MountainAedicule1", Mountains_Pack_1Plugin.MWL_MountainAedicule1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MountainAedicule1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MountainPagota1", Mountains_Pack_1Plugin.MWL_MountainPagota1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MountainPagota1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MountainTower1", Mountains_Pack_1Plugin.MWL_MountainTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MountainTower1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MountainTreasury1", Mountains_Pack_1Plugin.MWL_MountainTreasury1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MountainTreasury1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MoutainHogan1", Mountains_Pack_1Plugin.MWL_MountainHogan1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MountainHogan1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}