using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Dungeon_Mountain;

public class Locations
{
    public static void AddAllLocations()
    {
        LocationManager.AddLocation(
            Dungeon_MountainPlugin.dungeonGameObject,
            LocationConfigs.MWD_MountainDungeon_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}