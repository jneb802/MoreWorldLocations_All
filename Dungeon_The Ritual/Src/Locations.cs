using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Common;

namespace Underground_Ruins;

public class Locations
{
    public static void AddAllLocations()
    {
        LocationManager.AddLocation(
            Underground_RuinsPlugin.dungeonGameObject,
            LocationConfigs.MWD_TheRitual_Config);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
}