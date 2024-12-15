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
            "MWV_PlainsTavern1",
            LocationConfigs.MWV_PlainsTavern1,
            "PlainsTavernTrader1");
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
        assetBundle.Unload(false);
    }
}