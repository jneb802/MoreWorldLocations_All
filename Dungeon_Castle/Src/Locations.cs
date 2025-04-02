using Common;
using Jotunn.Managers;

namespace Forbidden_Catacombs;

public class Locations
{
    public static void AddAllLocations()
    {
        LocationManager.AddLocation(
            Forbidden_CatacombsPlugin.dungeonGameObject,
            LocationConfigs.MWD_Castle_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}