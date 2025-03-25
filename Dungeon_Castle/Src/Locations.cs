using Common;
using Jotunn.Managers;

namespace Dungeon_Castle;

public class Locations
{
    public static void AddAllLocations()
    {
        LocationManager.AddLocation(
            Dungeon_CastlePlugin.dungeonGameObject,
            LocationConfigs.MWD_Castle_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}