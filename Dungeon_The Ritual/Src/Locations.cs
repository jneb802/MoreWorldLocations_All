using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Dungeon_The_Ritual;

public class Locations
{
    public static void AddAllLocations()
    {
        LocationManager.AddLocation(
            Dungeon_The_RitualPlugin.dungeonGameObject,
            LocationConfigs.MWD_TheRitual_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}