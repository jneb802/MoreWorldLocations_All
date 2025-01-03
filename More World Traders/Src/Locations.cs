using Common;
using Jotunn.Managers;
using More_World_Traders;
using UnityEngine;

namespace More_World_Traders;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = More_World_TradersPlugin.assetBundle;
        AssetBundle assetBundleAshlands = More_World_TradersPlugin.assetBundleAshlands;
        
        LocationManager.AddLocation(assetBundle,
            "MWL_PlainsTavern1",
            LocationConfigs.MWL_PlainsTavern1);
        
        LocationManager.AddLocation(assetBundleAshlands,
            "MWL_OceanTavern1",
            LocationConfigs.MWL_OceanTavern1);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_PlainsCamp1",
            LocationConfigs.MWL_PlainsCamp1);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_BlackForestBlacksmith1",
            LocationConfigs.MWL_BlackForestBlacksmith1);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_BlackForestBlacksmith2",
            LocationConfigs.MWL_BlackForestBlacksmith2);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MountainsBlacksmith1",
            LocationConfigs.MWL_MountainsBlacksmith1);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_MistlandsBlacksmith1",
            LocationConfigs.MWL_MistlandsBlacksmith1);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}