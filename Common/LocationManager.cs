
using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Common;

public class LocationManager
{
    public enum LocationPosition
    {
        Interior,
        Exterior
    }
    
    public static void AddLocation(AssetBundle assetBundle, string locationName, string creatureYAMLContent, string creatureListName, int creatureCount, string lootYAMLContent, string lootListName, LocationConfig locationConfig)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        CreatureManager.SetupCreatures(LocationPosition.Exterior,creatureListName,jotunnLocationContainer,creatureCount,creatureYAMLContent);
        
        var lootList = LootManager.CreateLootList(lootListName, lootYAMLContent);
        var locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer,LocationPosition.Exterior);
        LootManager.SetupChestLoot(locationChestContainers,lootList);
        var locationDropOnDestroyedLoot = LootManager.GetLocationsDropOnDestroyeds(jotunnLocationContainer,LocationPosition.Exterior);
        if (locationDropOnDestroyedLoot != null)
        {
            LootManager.SetupDropOnDestroyedLoot(locationDropOnDestroyedLoot,lootList);
        }
        
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
    
    public static void AddLocation(AssetBundle assetBundle, string locationName, LocationConfig locationConfig)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
    
    public static void AddLocation(GameObject locationGameObject, LocationConfig locationConfig)
    {
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
}