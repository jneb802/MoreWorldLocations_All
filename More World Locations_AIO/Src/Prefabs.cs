using System.Collections.Generic;
using System.Reflection;
using Common;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using More_World_Locations_AIO.Shrines;
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
        }
    }

    public static void AddContainerPrefab(GameObject prefab)
    {
        int index = prefab.name.IndexOf("_loot");
        string locationName = index > 0 ? prefab.name.Substring(0, index) : prefab.name;
        
        LocationConfiguration locationConfiguration = BepinexConfigs.bepinexConfigs[locationName + "_Configuration"];
        List<DropTable.DropData> lootList =
            More_World_Locations_AIOPlugin.YAMLManager.lootListDictionary[locationConfiguration.LootList.Value];
        
        CustomPrefab customPrefab = new CustomPrefab(prefab, true);
        Container container = customPrefab.Prefab.GetComponent<Container>();
        container.m_defaultItems.m_drops = lootList;
        
        PrefabManager.Instance.AddPrefab(customPrefab); 
    }
    
    public static void AddSpawnerPrefab(GameObject prefab)
    {
        int index = prefab.name.IndexOf("_Spawner");
        string locationName = index > 0 ? prefab.name.Substring(0, index) : prefab.name;
        
        LocationConfiguration locationConfiguration = BepinexConfigs.bepinexConfigs[locationName + "_Configuration"];
        List<GameObject> creatureList =
            More_World_Locations_AIOPlugin.YAMLManager.creatureListDictionary[locationConfiguration.CreatureList.Value];
        
        CustomPrefab customPrefab = new CustomPrefab(prefab, false);
        Common.CreatureManager_v2.AddCreatureToSpawner(customPrefab.Prefab, creatureList);
        PrefabManager.Instance.AddPrefab(customPrefab); 
    }
    
    
}