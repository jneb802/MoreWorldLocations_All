using System.Collections.Generic;
using System.Reflection;
using Common;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using More_World_Locations_AIO.Shrines;
using More_World_Locations_AIO.Utils;
using More_World_Locations_AIO.Waystones;
using UnityEngine;

namespace More_World_Locations_AIO;

public class Prefabs
{
    public static AssetBundle prefabBundle_1;
    public static AssetBundle prefabBundle_2;
    public static AssetBundle prefabBundle_3;
    
    public static void LoadPrefabBundles()
    {
        prefabBundle_1 = AssetUtils.LoadAssetBundleFromResources(
            "moreworldlocations_prefabs_1",
            Assembly.GetExecutingAssembly());
        
        prefabBundle_2 = AssetUtils.LoadAssetBundleFromResources(
            "moreworldlocations_prefabs_2",
            Assembly.GetExecutingAssembly());
        
        prefabBundle_3 = AssetUtils.LoadAssetBundleFromResources(
            "moreworldlocations_prefabs_3",
            Assembly.GetExecutingAssembly());
    }

    public static void AddAllPrefabs()
    {
        GameObject[] gameObjects1 = prefabBundle_1.LoadAllAssets<GameObject>();
        GameObject[] gameObjects2 = prefabBundle_2.LoadAllAssets<GameObject>();
        GameObject[] gameObjects3 = prefabBundle_3.LoadAllAssets<GameObject>();
        
        AddPrefabsFromBundle(gameObjects1);
        AddPrefabsFromBundle(gameObjects2);
        AddPrefabsFromBundle(gameObjects3);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllPrefabs;
    }

    public static void AddPrefabsFromBundle(GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            if (PrefabManager.Instance.GetPrefab(gameObject.name) != null)
            {
                // Debug.Log("Prefab with name "+ gameObject.name + " is already in ObjectDB");
            
                PrefabManager.Instance.RemovePrefab(gameObject.name);
            }
            
            // If the prefab is a loot_chest then it needs to mock references
            if (gameObject.GetComponent<Container>() != null)
            {
                AddContainerPrefab(gameObject);
            }
            else if (gameObject.GetComponent<CreatureSpawner>() != null)
            {
                AddSpawnerPrefab(gameObject);
            }
            else if (gameObject.name.Equals("MWL_Shrine"))
            {
                CustomPrefab customPrefab = new CustomPrefab(gameObject, true);
                customPrefab.Prefab.AddComponent<Shrine>();
                PrefabManager.Instance.AddPrefab(customPrefab); 
            }
            else if (gameObject.name.Equals("MWL_Waystone"))
            {
                CustomPrefab customPrefab = new CustomPrefab(gameObject, true);
                customPrefab.Prefab.AddComponent<Waystone>();
                PrefabManager.Instance.AddPrefab(customPrefab); 
            }
            
            // else if (gameObject.name.Equals("MWL_Totem_wood_8"))
            // {
            //     CustomPrefab customPrefab = new CustomPrefab(gameObject, true);
            //     customPrefab.Prefab.AddComponent<TotemSpawner>();
            //     PrefabManager.Instance.AddPrefab(customPrefab); 
            // }
        }
    }

    public static void AddContainerPrefab(GameObject prefab)
    {
        int index = prefab.name.IndexOf("_loot");
        string locationName = index > 0 ? prefab.name.Substring(0, index) : prefab.name;
        
        // Get the LocationConfig to determine biome
        LocationConfig locationConfig = LocationConfigs.GetLocationConfig(locationName);
        if (locationConfig == null)
        {
            Debug.LogWarning($"Prefabs: Could not find LocationConfig for {locationName}");
            return;
        }
        
        // Get the appropriate loot table for the biome
        DropTable lootTable = LootDB.GetLootTable(locationConfig.Biome);
        if (lootTable == null)
        {
            Debug.LogWarning($"Prefabs: Could not find loot table for biome {locationConfig.Biome}");
            return;
        }
        
        CustomPrefab customPrefab = new CustomPrefab(prefab, true);
        Container container = customPrefab.Prefab.GetComponent<Container>();
        
        // Set the container's default items to our biome-specific loot table
        container.m_defaultItems.m_drops = lootTable.m_drops;
        
        // Debug.Log($"Prefabs: Set loot table for {prefab.name} using biome {locationConfig.Biome}");
        
        PrefabManager.Instance.AddPrefab(customPrefab); 
    }
    
    public static void AddSpawnerPrefab(GameObject prefab)
    {
        int index = prefab.name.IndexOf("_Spawner");
        string locationName = index > 0 ? prefab.name.Substring(0, index) : prefab.name;
        
        string creatureListName = LocationCreatureMapping.GetCreatureListForLocation(locationName);
        if (string.IsNullOrEmpty(creatureListName))
        {
            Debug.LogWarning($"Prefabs: Could not find creature list mapping for {locationName}");
            return;
        }
        
        GameObject randomCreature = CreatureDB.GetRandomCreatureFromList(creatureListName);
        if (randomCreature == null)
        {
            Debug.LogWarning($"Prefabs: Could not get random creature from list {creatureListName} for {locationName}");
            return;
        }
    
        CustomPrefab customPrefab = new CustomPrefab(prefab, false);
        CreatureSpawner spawner = customPrefab.Prefab.GetComponent<CreatureSpawner>();
        
        spawner.m_creaturePrefab = randomCreature;
    
        // Debug.Log($"Prefabs: Set creature {randomCreature.name} from list {creatureListName} for spawner {prefab.name}");
    
        PrefabManager.Instance.AddPrefab(customPrefab); 
    }
    
    
}