using System.Reflection;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using More_World_Locations_AIO.Shrines;
using More_World_Locations_AIO.Waystones;
using UnityEngine;

namespace More_World_Locations_AIO;

public class Prefabs
{
    public static AssetBundle vendorsPrefabBundle;
    public static AssetBundle vendorNpcBundle;
    public static AssetBundle portIconBundle;
    public static AssetBundle dungeonBlackforest;
    public static AssetBundle dungeonCastle;
    public static AssetBundle mockPlaceHoldersBundle;

    public static GameObject[] mockPlaceHoldersGameObjects;

    public static void LoadPrefabBundles()
    {
        mockPlaceHoldersBundle = AssetUtils.LoadAssetBundleFromResources(
            "mockplaceholders",
            Assembly.GetExecutingAssembly());

        vendorsPrefabBundle = AssetUtils.LoadAssetBundleFromResources(
            "moreworldvendors",
            Assembly.GetExecutingAssembly());

        vendorNpcBundle = AssetUtils.LoadAssetBundleFromResources(
            "vendornpc",
            Assembly.GetExecutingAssembly());

        portIconBundle = AssetUtils.LoadAssetBundleFromResources(
            "porticon",
            Assembly.GetExecutingAssembly());

        dungeonBlackforest = AssetUtils.LoadAssetBundleFromResources(
            "dungeonblackforest",
            Assembly.GetExecutingAssembly());

        dungeonCastle = AssetUtils.LoadAssetBundleFromResources(
            "dungeoncastle",
            Assembly.GetExecutingAssembly());
    }

    public static void AddAllPrefabs()
    {
        mockPlaceHoldersGameObjects = mockPlaceHoldersBundle.LoadAllAssets<GameObject>();

        AddWardPrefab<Shrine>("MWL_Shrine");
        AddWardPrefab<Waystone>("MWL_Waystone");
        MakeMDKitPrefabs();
    }

    private static void AddWardPrefab<T>(string prefabName) where T : MonoBehaviour
    {
        GameObject prefab = PrefabManager.Instance.CreateClonedPrefab(prefabName, "guard_stone");
        if (prefab == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"Could not clone guard_stone for {prefabName}");
            return;
        }

        // Configure the inactive clone before any ward behavior can run.
        Object.DestroyImmediate(prefab.GetComponent<PrivateArea>());
        RemoveChild(prefab, "AreaMarker");
        RemoveChild(prefab, "PlayerBase");
        prefab.transform.localPosition = Vector3.zero;

        // The bundled prefabs kept the ward visuals active without ward protection.
        Transform wayEffect = prefab.transform.Find("WayEffect");
        if (wayEffect != null)
            wayEffect.gameObject.SetActive(true);

        RandomSpawn randomSpawn = prefab.AddComponent<RandomSpawn>();
        randomSpawn.m_chanceToSpawn = 5f;

        prefab.AddComponent<T>();
        PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, fixReference: false));
    }

    private static void RemoveChild(GameObject prefab, string childName)
    {
        Transform child = prefab.transform.Find(childName);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
    }

    public static void MakeMDKitPrefabs()
    {
        string widestoneName = "widestone";
        string widestoneKitName = "MD_Kit_widestone";

        GameObject wideStoneCloned = PrefabManager.Instance.CreateClonedPrefab(widestoneKitName, widestoneName);
        Object.DestroyImmediate(wideStoneCloned.GetComponent<Destructible>());
        CustomPrefab customPrefab = new CustomPrefab(wideStoneCloned, fixReference: false);
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
