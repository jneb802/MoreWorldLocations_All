
using System;
using System.Collections.Generic;
using Jotunn;
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
    
    
    // Original method used by most mods
    // Added check for new loot list version
    public static void AddLocation(AssetBundle assetBundle, string locationName, string creatureYAMLContent, string creatureListName, int creatureCount, string lootYAMLContent, string lootListName, LocationConfig locationConfig)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        CreatureManager.SetupCreatures(LocationPosition.Exterior,creatureListName,jotunnLocationContainer,creatureCount,creatureYAMLContent);
        
        if (LootManager.isLootListVersion2(lootYAMLContent))
        {
            List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(lootListName, lootYAMLContent);
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,dropDataList);
        }
        else
        {
            List<string> lootList = LootManager.CreateLootList(lootListName, lootYAMLContent);
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,lootList);   
            var locationDropOnDestroyedLoot = LootManager.GetLocationsDropOnDestroyeds(jotunnLocationContainer,LocationPosition.Exterior);
            if (locationDropOnDestroyedLoot != null)
            {
                LootManager.SetupDropOnDestroyedLoot(locationDropOnDestroyedLoot,lootList);
            }
        }
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
    
    // Alternate method that doesn't use creatureCount
    // Alternate method to check for new loot list version
    public static void AddLocation(AssetBundle assetBundle, string locationName, string creatureYAMLContent, string creatureListName, string lootYAMLContent, string lootListName, LocationConfig locationConfig)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        CreatureManager.SetupCreatures(creatureListName,jotunnLocationContainer,creatureYAMLContent);

        if (LootManager.isLootListVersion2(lootYAMLContent))
        {
            List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(lootListName, lootYAMLContent);
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,dropDataList);
        }
        else
        {
            List<string> lootList = LootManager.CreateLootList(lootListName, lootYAMLContent);
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,lootList);   
        }
        
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
    
    // Simplified method to just accept location without loot or creature modifications
    public static void AddLocation(AssetBundle assetBundle, string locationName, LocationConfig locationConfig)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
    
    // Simplified method to just accept location: Loot: Yes, Creature: No
    public static void AddLocation(AssetBundle assetBundle, string locationName, string lootYAMLContent, string lootListName, LocationConfig locationConfig)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        if (LootManager.isLootListVersion2(lootYAMLContent))
        {
            List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(lootListName, lootYAMLContent);
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,dropDataList);
        }
        else
        {
            List<string> lootList = LootManager.CreateLootList(lootListName, lootYAMLContent);
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,lootList);   
        }
        
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
    
    // Modified to accept a location gameObject instead of asset bundle
    public static void AddLocation(GameObject locationGameObject, LocationConfig locationConfig)
    {
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
    
    public static void AddEventLocation(AssetBundle assetBundle, string locationName, string creatureYAMLContent, string creatureListName, string lootYAMLContent, string lootListName, LocationConfig locationConfig)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        CreatureManager.SetupCreatures(creatureListName,jotunnLocationContainer,creatureYAMLContent);

        if (LootManager.isLootListVersion2(lootYAMLContent))
        {
            List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(lootListName, lootYAMLContent);
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,dropDataList);
        }
        else
        {
            List<string> lootList = LootManager.CreateLootList(lootListName, lootYAMLContent);
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,lootList);
        }
        
        GameObject eventStone = jotunnLocationContainer.FindDeepChild("MWL_EventStone").gameObject;
        if (!eventStone)
        {
            WarpLogger.Logger.LogDebug("Failed to find EventStone");
        }
        
        eventStone.GetComponent<EffectArea>().m_statusEffect = "GoblinShaman_shield";
        
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
}