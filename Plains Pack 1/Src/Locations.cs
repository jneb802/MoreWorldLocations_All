using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Plains_Pack_1;

public class Locations
{
    public static void AddAllLocations()
    {
        AssetBundle assetBundle = Plains_Pack_1Plugin.assetBundle;

        Common.LocationManager.AddLocation(assetBundle, "MWL_GoblinFort1", Plains_Pack_1Plugin.MWL_GoblinFort1_Configuration, LocationConfigs.AllLocationConfigs["MWL_GoblinFort1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingRock1", Plains_Pack_1Plugin.MWL_FulingRock1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingRock1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingVillage1", Plains_Pack_1Plugin.MWL_FulingVillage1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingVillage1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingVillage2", Plains_Pack_1Plugin.MWL_FulingVillage2_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingVillage2_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_PlainsPillar1", Plains_Pack_1Plugin.MWL_PlainsPillar1_Configuration, LocationConfigs.AllLocationConfigs["MWL_PlainsPillar1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingTemple1", Plains_Pack_1Plugin.MWL_FulingTemple1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingTemple1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingTemple2", Plains_Pack_1Plugin.MWL_FulingTemple2_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingTemple2_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingTemple3", Plains_Pack_1Plugin.MWL_FulingTemple3_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingTemple3_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingWall1", Plains_Pack_1Plugin.MWL_FulingWall1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingWall1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingTower1", Plains_Pack_1Plugin.MWL_FulingTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingTower1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        
        Common.LocationManager.AddLocation(assetBundle, "MWL_PlainsArena1", Plains_Pack_1Plugin.MWL_PlainsArena1_Configuration, LocationConfigs.AllLocationConfigs["MWL_PlainsArena1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_PlainsFarm1", Plains_Pack_1Plugin.MWL_PlainsFarm1_Configuration, LocationConfigs.AllLocationConfigs["MWL_PlainsFarm1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_PlainsManor1", Plains_Pack_1Plugin.MWL_PlainsManor1_Configuration, LocationConfigs.AllLocationConfigs["MWL_PlainsManor1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_PlainsOracle1", Plains_Pack_1Plugin.MWL_PlainsOracle1_Configuration, LocationConfigs.AllLocationConfigs["MWL_PlainsOracle1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);

        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }

}