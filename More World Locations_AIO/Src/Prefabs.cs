using System.Collections.Generic;
using System.Reflection;
using Common;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace More_World_Locations_AIO;

public class Prefabs
{
    public static AssetBundle prefabBundle_2;
    
    
    
    public static void LoadPrefabBundles()
    {
        prefabBundle_2 = AssetUtils.LoadAssetBundleFromResources(
            "moreworldlocations_prefabs_2",
            Assembly.GetExecutingAssembly());
    }

    public static void AddAllPrefabs()
    {
        GameObject[] gameObjects = prefabBundle_2.LoadAllAssets<GameObject>();

        foreach (GameObject gameObject in gameObjects)
        {
            // If the prefab is a loot_chest then it needs to mock references
            if (gameObject.GetComponent<Container>() != null)
            {
                CustomPrefab prefab = new CustomPrefab(gameObject, true);
                PrefabManager.Instance.AddPrefab(prefab);
            }
            else
            {
                AddSpawnerPrefab(gameObject);
            }
        }
    }

    public static void AddContainerPrefab(GameObject prefab)
    {
        int index = prefab.name.IndexOf("_Spawner");
        string locationName = index > 0 ? prefab.name.Substring(0, index) : prefab.name;
        
        LocationConfiguration locationConfiguration = BepinexConfigs.LocationConfigs[locationName + "_Configuration"];
        List<GameObject> lootList =
            More_World_Locations_AIOPlugin.YAMLManager.creatureListDictionary[locationConfiguration.LootList.Value];
        
        CustomPrefab customPrefab = new CustomPrefab(prefab, false);
        Common.CreatureManager_v2.AddCreatureToSpawner(customPrefab.Prefab, lootList);
        PrefabManager.Instance.AddPrefab(customPrefab); 
    }
    
    public static void AddSpawnerPrefab(GameObject prefab)
    {
        int index = prefab.name.IndexOf("_Spawner");
        string locationName = index > 0 ? prefab.name.Substring(0, index) : prefab.name;
        
        LocationConfiguration locationConfiguration = BepinexConfigs.LocationConfigs[locationName + "_Configuration"];
        List<GameObject> creatureList =
            More_World_Locations_AIOPlugin.YAMLManager.creatureListDictionary[locationConfiguration.CreatureList.Value];
        
        CustomPrefab customPrefab = new CustomPrefab(prefab, false);
        Common.CreatureManager_v2.AddCreatureToSpawner(customPrefab.Prefab, creatureList);
        PrefabManager.Instance.AddPrefab(customPrefab); 
    }
    
}