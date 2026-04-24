using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using More_World_Locations_AIO.Shipments;
using More_World_Locations_AIO.Shrines;
using More_World_Locations_AIO.Utils;
using More_World_Locations_AIO.Waystones;
using UnityEngine;
using YamlDotNet.Serialization;

namespace More_World_Locations_AIO;

public class Prefabs
{
    public static AssetBundle prefabBundle_1;
    public static AssetBundle prefabBundle_2;
    public static AssetBundle prefabBundle_3;
    public static AssetBundle vendorsPrefabBundle;
    public static AssetBundle vendorNpcBundle;
    public static AssetBundle dungeonBlackforest;

    private const string BfdChestPrefabName = "BFD_chest_loot";
    private const string BfdTentaSpawnerPrefabName = "BFD_Kit_Spawner_TentaRoot";
    private const string BfdTentaSpawnerChestPrefabName = "BFD_chest_spawner1";
    private const string BfdTentaCreatureName = "TentaRoot";
    
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
        
        vendorsPrefabBundle = AssetUtils.LoadAssetBundleFromResources(
            "moreworldvendors",
            Assembly.GetExecutingAssembly());
        
        vendorNpcBundle = AssetUtils.LoadAssetBundleFromResources(
            "vendornpc",
            Assembly.GetExecutingAssembly());
        
        dungeonBlackforest = AssetUtils.LoadAssetBundleFromResources(
            "dungeonblackforest",
            Assembly.GetExecutingAssembly());
    }

    public static void AddAllPrefabs()
    {
        GameObject[] gameObjects1 = prefabBundle_1.LoadAllAssets<GameObject>();
        GameObject[] gameObjects2 = prefabBundle_2.LoadAllAssets<GameObject>();
        GameObject[] gameObjects3 = prefabBundle_3.LoadAllAssets<GameObject>();
        GameObject[] dungeonBlackforestGameObjects = dungeonBlackforest.LoadAllAssets<GameObject>();
            
        AddPrefabsFromBundle(gameObjects1);
        AddPrefabsFromBundle(gameObjects2);
        AddPrefabsFromBundle(gameObjects3);
        
        // Underground Ruins
        AddBlackForestDungeonPrefabs(dungeonBlackforestGameObjects);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllPrefabs;
    }
    
    private static readonly string[] BfdKitVanillaBaseNames =
    {
        "dirtfloor",
        "Stoneblock",
        "StoneblockSmall",
        "StonePillar",
        "MineRock_Copper"
    };

    public static void AddBlackForestDungeonPrefabs(GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            // Rooms and the dungeon exterior register through the soft-reference path.
            if (gameObject.GetComponent<Room>() != null) continue;
            if (gameObject.GetComponent<Location>() != null) continue;

            // These prefabs are intentionally created by cloning vanilla prefabs below.
            if (IsClonedBfdPrefab(gameObject.name)) continue;

            if (PrefabManager.Instance.GetPrefab(gameObject.name) != null)
            {
                PrefabManager.Instance.RemovePrefab(gameObject.name);
            }

            CustomPrefab customPrefab = new CustomPrefab(gameObject, true);
            PrefabManager.Instance.AddPrefab(customPrefab);
        }
    }
    
    public static void AddBFDKitPrefabs()
    {
        foreach (string vanillaName in BfdKitVanillaBaseNames)
        {
            string kitName = "BFD_Kit_" + vanillaName;

            GameObject cloned = CreateOrReplaceClonedPrefab(kitName, vanillaName);
            if (cloned == null)
            {
                continue;
            }

            CustomPrefab customPrefab = new CustomPrefab(cloned, fixReference: false);
            PrefabManager.Instance.AddPrefab(customPrefab);
        }

        GameObject chestCloned = CreateOrReplaceClonedPrefab(BfdChestPrefabName, "TreasureChest_fCrypt");
        if (chestCloned != null)
        {
            CustomPrefab chestClonedCustomPrefab = new CustomPrefab(chestCloned, fixReference: false);
            PrefabManager.Instance.AddPrefab(chestClonedCustomPrefab);
        }

        GameObject tentaSpawnerCloned = CreateOrReplaceClonedPrefab(BfdTentaSpawnerPrefabName, "Spawner_Skeleton");
        if (tentaSpawnerCloned != null)
        {
            CreatureSpawner creatureSpawner = tentaSpawnerCloned.GetComponent<CreatureSpawner>();
            GameObject tentaRootPrefab = PrefabManager.Cache.GetPrefab<GameObject>(BfdTentaCreatureName);

            if (creatureSpawner == null)
            {
                Debug.LogWarning($"Prefabs: {BfdTentaSpawnerPrefabName} clone is missing CreatureSpawner");
            }
            else if (tentaRootPrefab == null)
            {
                Debug.LogWarning($"Prefabs: Could not find creature prefab {BfdTentaCreatureName}");
            }
            else
            {
                creatureSpawner.m_creaturePrefab = tentaRootPrefab;
                CustomPrefab tentaSpawnerClonedCustomPrefab = new CustomPrefab(tentaSpawnerCloned, fixReference: false);
                PrefabManager.Instance.AddPrefab(tentaSpawnerClonedCustomPrefab);
            }
        }

        GameObject tentaCloned = CreateOrReplaceClonedPrefab(BfdTentaSpawnerChestPrefabName, "crypt_skeleton_chest");
        if (tentaCloned != null)
        {
            EggHatch eggHatch = tentaCloned.GetComponent<EggHatch>();
            if (eggHatch == null)
            {
                Debug.LogWarning($"Prefabs: {BfdTentaSpawnerChestPrefabName} clone is missing EggHatch");
            }
            else if (tentaSpawnerCloned == null)
            {
                Debug.LogWarning($"Prefabs: Could not assign {BfdTentaSpawnerPrefabName} to {BfdTentaSpawnerChestPrefabName}");
            }
            else
            {
                eggHatch.m_spawnPrefab = tentaSpawnerCloned;
                CustomPrefab tentaClonedCustomPrefab = new CustomPrefab(tentaCloned, fixReference: false);
                PrefabManager.Instance.AddPrefab(tentaClonedCustomPrefab);
            }
        }
    }

    private static GameObject CreateOrReplaceClonedPrefab(string prefabName, string vanillaPrefabName)
    {
        if (PrefabManager.Instance.GetPrefab(prefabName) != null)
        {
            PrefabManager.Instance.RemovePrefab(prefabName);
        }

        GameObject cloned = PrefabManager.Instance.CreateClonedPrefab(prefabName, vanillaPrefabName);
        if (cloned == null)
        {
            Debug.LogWarning($"Prefabs: Could not clone vanilla '{vanillaPrefabName}' for {prefabName}");
        }

        return cloned;
    }

    private static bool IsBfdKitPrefab(string name)
    {
        foreach (string vanillaName in BfdKitVanillaBaseNames)
        {
            if (name == "BFD_Kit_" + vanillaName) return true;
        }

        return false;
    }

    private static bool IsClonedBfdPrefab(string name)
    {
        return name == BfdChestPrefabName
               || name == BfdTentaSpawnerPrefabName
               || name == BfdTentaSpawnerChestPrefabName
               || IsBfdKitPrefab(name);
    }

    public static void AddBFDContainerPrefab(GameObject prefab)
    {
        GameObject sourceChest = PrefabManager.Cache.GetPrefab<GameObject>("TreasureChest_fCrypt");
        if (sourceChest == null)
        {
            Debug.LogWarning($"Prefabs: Could not find TreasureChest_fCrypt source for {prefab.name}");
            return;
        }

        Container sourceContainer = sourceChest.GetComponent<Container>();
        if (sourceContainer == null || sourceContainer.m_defaultItems == null)
        {
            Debug.LogWarning($"Prefabs: TreasureChest_fCrypt has no Container.m_defaultItems");
            return;
        }

        CustomPrefab customPrefab = new CustomPrefab(prefab, true);
        Container container = customPrefab.Prefab.GetComponent<Container>();
        container.m_defaultItems.m_drops = sourceContainer.m_defaultItems.m_drops;

        PrefabManager.Instance.AddPrefab(customPrefab);
    }
    
    // EggHatch.m_spawnPrefab points to a prefab that carries a CreatureSpawner; swap that
    // spawner's creature so hatched eggs in this room spawn the chosen creature.
    private static void SwapEggHatchCreature(GameObject prefab, string creatureName)
    {
        EggHatch eggHatch = prefab.GetComponent<EggHatch>();
        
        GameObject creaturePrefab = PrefabManager.Cache.GetPrefab<GameObject>(creatureName);
        if (creaturePrefab == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"Could not find creature prefab '{creatureName}' for EggHatch swap");
            return;
        }
        
        eggHatch.m_spawnPrefab = creaturePrefab;
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
        LocationConfig locationConfig = LocationDB.GetLocationConfig(locationName);
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

    public static GameObject AddDoorPrefab(string doorName, string vanillaDoorName)
    {
        GameObject doorPrefab = PrefabManager.Instance.CreateClonedPrefab(doorName, vanillaDoorName);
        if (doorPrefab == null)
        {
            Debug.LogWarning($"Prefabs: Could not create cloned prefab for {doorName}");
            return null;
        }
        
        CustomPrefab customPrefab = new CustomPrefab(doorPrefab, false);
        PrefabManager.Instance.AddPrefab(customPrefab); 
        return doorPrefab;
    }

    public static GameObject AddKeyPrefab(string keyName, string vanillaKeyName)
    {
        GameObject keyPrefab = PrefabManager.Instance.CreateClonedPrefab(keyName, vanillaKeyName);
        if (keyPrefab == null)
        {
            Debug.LogWarning($"Prefabs: Could not create cloned prefab for {keyName}");
            return null;
        }
        
        // Items must be registered via ItemManager (not just PrefabManager) so they appear
        // in ObjectDB.m_items. Without this, dropping the item fails because m_dropPrefab is null.
        CustomItem customItem = new CustomItem(keyPrefab, false);
        ItemManager.Instance.AddItem(customItem); 
        return keyPrefab;
    }

    public static GameObject AddRuneStonePrefab(string runeStoneName, string vanillaRuneStoneName)
    {
        GameObject runeStonePrefab = PrefabManager.Instance.CreateClonedPrefab(runeStoneName, vanillaRuneStoneName);
        if (runeStonePrefab == null)
        {
            Debug.LogWarning($"Prefabs: Could not create cloned prefab for {runeStoneName}");
            return null;
        }
    
        CustomPrefab customPrefab = new CustomPrefab(runeStonePrefab, false);
        PrefabManager.Instance.AddPrefab(customPrefab); 
        return runeStonePrefab;
    }

    public static GameObject AddPickableItemPrefab(string pickableItemName, string vanillaPickableItemName)
    {
        GameObject pickableItemPrefab = PrefabManager.Instance.CreateClonedPrefab(pickableItemName, vanillaPickableItemName);
        if (pickableItemPrefab == null)
        {
            Debug.LogWarning($"Prefabs: Could not create cloned prefab for {pickableItemName}");
            return null;
        }

        CustomPrefab customPrefab = new CustomPrefab(pickableItemPrefab, false);
        PickableItem pickableItem = customPrefab.Prefab.GetComponent<PickableItem>();
        pickableItem.m_randomItemPrefabs = System.Array.Empty<PickableItem.RandomItem>();
        PrefabManager.Instance.AddPrefab(customPrefab); 
        return pickableItemPrefab;
    }

}
