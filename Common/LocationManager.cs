
using System;
using System.Collections.Generic;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using More_World_Locations_AIO.Utils;
using SoftReferenceableAssets;
using UnityEngine;

namespace Common;

public class LocationManager
{
    public enum LocationPosition
    {
        Interior,
        Exterior
    }
    
    public static LocationManager instance;
    public static LocationManager Instance => instance ??= new LocationManager();
    
    // Original method used by most mods
    // Added check for new loot list version
    // public static void AddLocation(AssetBundle assetBundle, string locationName, string creatureYAMLContent, string creatureListName, int creatureCount, string lootYAMLContent, string lootListName, LocationConfig locationConfig)
    // {
    //     var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
    //     GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
    //     
    //     CreatureManager.SetupCreatures(LocationPosition.Exterior,creatureListName,jotunnLocationContainer,creatureCount,creatureYAMLContent);
    //     
    //     if (LootManager.isLootListVersion2(lootYAMLContent))
    //     {
    //         List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(lootListName, lootYAMLContent);
    //         List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
    //         LootManager.SetupChestLoot(locationChestContainers,dropDataList);
    //     }
    //     else
    //     {
    //         List<string> lootList = LootManager.CreateLootList(lootListName, lootYAMLContent);
    //         List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
    //         LootManager.SetupChestLoot(locationChestContainers,lootList);   
    //         var locationDropOnDestroyedLoot = LootManager.GetLocationsDropOnDestroyeds(jotunnLocationContainer,LocationPosition.Exterior);
    //         if (locationDropOnDestroyedLoot != null)
    //         {
    //             LootManager.SetupDropOnDestroyedLoot(locationDropOnDestroyedLoot,lootList);
    //         }
    //     }
    //     CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
    //     
    //     ZoneManager.Instance.AddCustomLocation(customLocation);
    // }
    
    // Alternate method that doesn't use creatureCount
    // Alternate method to check for new loot list version
    public static void AddLocation(AssetBundle assetBundle, string locationName, string creatureYAMLContent, string creatureListName, string lootYAMLContent, string lootListName, LocationConfig locationConfig)
    {
        GameObject locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
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
    
    // No creature count
    // Includes new lootList versions
    // Accept just locationConfiguration
    public static void AddLocation(AssetBundle assetBundle, string locationName, LocationConfiguration locationConfiguration, LocationConfig locationConfig, YAMLManager yamlManager)
    {
        var locationGameObject = assetBundle.LoadAsset<GameObject>(locationName);
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(locationGameObject);
        
        CreatureManager.SetupCreatures(locationConfiguration.CreatureList.Value,jotunnLocationContainer,yamlManager.GetCreatureYamlContent(locationConfiguration.CreatureYaml.Value));

        if (LootManager.isLootListVersion2(yamlManager.GetLootYamlContent(locationConfiguration.LootYaml.Value)))
        {
            List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(locationConfiguration.LootList.Value, yamlManager.GetLootYamlContent(locationConfiguration.LootYaml.Value));
            List<Container> locationChestContainers = LootManager.GetLocationsContainers(jotunnLocationContainer);
            LootManager.SetupChestLoot(locationChestContainers,dropDataList);
        }
        else
        {
            List<string> lootList = LootManager.CreateLootList(locationConfiguration.LootList.Value, yamlManager.GetLootYamlContent(locationConfiguration.LootYaml.Value));
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

    
    
    
    // New Soft Reference method
    public static void AddLocation(string locationName, LocationConfiguration locationConfiguration, LocationConfig locationConfig, YAMLManager yamlManager)
    {
        SoftReference<GameObject> softReferencePrefab = Jotunn.Managers.AssetManager.Instance.GetSoftReference<GameObject>(locationName);
        
        Jotunn.Managers.AssetManager.Instance.ResolveMocksOnLoad(
            softReferencePrefab.m_assetID,
            null,
            resolvedObj =>
            {
                CreatureDB.SetupCreatures(
                    LocationCreatureMapping.GetCreatureListForLocation(locationName),
                    resolvedObj as GameObject);
            });
        
        Jotunn.Managers.AssetManager.Instance.ResolveMocksOnLoad(
            softReferencePrefab.m_assetID,
            null,
            resolvedObj => 
            {
                LootDB.SetupLoot(
                    locationConfig.Biome,
                    resolvedObj as GameObject);
            });
    
        CustomLocation customLocation = new 
            CustomLocation(
                softReferencePrefab,
                true,
                locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }

    
    
}