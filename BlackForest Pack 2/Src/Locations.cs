
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace BlackForest_Pack_2;

public class Locations
{
    public enum LocationPosition
    {
        Interior,
        Exterior
    }
    
    public static void AddAllLocations()
    {
        var assetBundle = BlackForest_Pack_2Plugin.assetBundle;

        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestForge1", BlackForest_Pack_2Plugin.MWL_ForestForge1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestForge1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestForge2", BlackForest_Pack_2Plugin.MWL_ForestForge2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestForge2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestGreatHouse2", BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestGreatHouse2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestHouse2", BlackForest_Pack_2Plugin.MWL_ForestHouse2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestHouse2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestRuin1", BlackForest_Pack_2Plugin.MWL_ForestRuin1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestRuin1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestTower2", BlackForest_Pack_2Plugin.MWL_ForestTower2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestTower2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestTower3", BlackForest_Pack_2Plugin.MWL_ForestTower3_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestTower3_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MassGrave1", BlackForest_Pack_2Plugin.MWL_MassGrave1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MassGrave1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneFormation1", BlackForest_Pack_2Plugin.MWL_StoneFormation1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneFormation1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_GuardTower1", BlackForest_Pack_2Plugin.MWL_GuardTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_GuardTower1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_RootRuins1", BlackForest_Pack_2Plugin.MWL_RootRuins1_Configuration, LocationConfigs.AllLocationConfigs["MWL_RootRuins1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_RootsTower1", BlackForest_Pack_2Plugin.MWL_RootsTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_RootsTower1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_RootsTower2", BlackForest_Pack_2Plugin.MWL_RootsTower2_Configuration, LocationConfigs.AllLocationConfigs["MWL_RootsTower2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestRuin2", BlackForest_Pack_2Plugin.MWL_ForestRuin2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestRuin2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestRuin3", BlackForest_Pack_2Plugin.MWL_ForestRuin3_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestRuin3_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestSkull1", BlackForest_Pack_2Plugin.MWL_ForestSkull1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestSkull1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestTower4", BlackForest_Pack_2Plugin.MWL_ForestTower4_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestTower4_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestTower5", BlackForest_Pack_2Plugin.MWL_ForestTower5_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestTower5_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);

        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestWarehouse1", BlackForest_Pack_2Plugin.MWL_ForestWarehouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestWarehouse1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        // Common.LocationManager.AddLocation(assetBundle, "MWL_ForestFort1", BlackForest_Pack_2Plugin.MWL_ForestFort1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestFort1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestStation1", BlackForest_Pack_2Plugin.MWL_ForestStation1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestStation1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestLodge1", BlackForest_Pack_2Plugin.MWL_ForestLodge1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestLodge1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestMine1", BlackForest_Pack_2Plugin.MWL_ForestMine1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestMine1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);

        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }

}