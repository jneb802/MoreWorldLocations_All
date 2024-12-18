using Common;
using Jotunn.Managers;
using More_World_Traders;

namespace More_World_Traders;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = More_World_TradersPlugin.assetBundle;
        
        LocationManager.AddLocation(assetBundle,
            "MWL_PlainsTavern1",
            LocationConfigs.MWV_PlainsTavern1);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_PlainsCamp1",
            LocationConfigs.MWV_PlainsCamp1);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}