using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using SoftReferenceableAssets;
using UnityEngine;

namespace More_World_Locations_AIO.Managers;

[PublicAPI]
public static class PrefabManager
{
    internal static List<GameObject> PrefabsToRegister = new();
    internal static Dictionary<string, Blueprint> Blueprints = new();
    internal static Dictionary<string, BlueprintLocation> BlueprintLocations = new();
    internal static List<Clone> Clones = new();
    
    // static constructor, gets called first time PrefabManager is accessed
    static PrefabManager()
    {
        Harmony harmony = new("org.bepinex.helpers.MWL_Manager");
        harmony.Patch(AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.Awake)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_FejdStartup))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(AssetBundleLoader), nameof(AssetBundleLoader.OnInitCompleted)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(SoftAssetLoader), nameof(SoftAssetLoader.Patch_AssetBundleLoader_OnInitCompleted))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNetScene), nameof(ZNetScene.Awake)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_ZNetScene_Awake))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(Minimap), nameof(Minimap.Awake)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LocationManager), nameof(LocationManager.Patch_Minimap_Awake))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations)), prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LocationManager), nameof(LocationManager.Patch_SetupLocations))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(Location), nameof(Location.Awake)), prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LocationManager), nameof(LocationManager.Patch_Location_Awake))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(Localization), nameof(Localization.LoadCSV)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LocalizeKey), nameof(LocalizeKey.AddLocalizedKeys))));
    }

    public static void RegisterPrefab(GameObject? prefab)
    {
        if (prefab == null) return;
        PrefabsToRegister.Add(prefab);
    }
    public static void RegisterPrefab(string assetBundleName, string prefabName) => RegisterPrefab(AssetBundleManager.LoadAsset<GameObject>(assetBundleName, prefabName));
    public static void RegisterPrefab(AssetBundle assetBundle, string prefabName) =>  RegisterPrefab(assetBundle.LoadAsset<GameObject>(prefabName));

    [HarmonyPriority(Priority.VeryHigh)]
    internal static void Patch_ZNetScene_Awake(ZNetScene __instance)
    {
        foreach (GameObject prefab in PrefabsToRegister)
        {
            if (!prefab.GetComponent<ZNetView>()) continue; // make sure asset even needs to be registered to ZNetScene
            __instance.m_prefabs.Add(prefab);
        }
    }

    internal static void Patch_FejdStartup(FejdStartup __instance)
    {
        Helpers._ZNetScene = __instance.m_objectDBPrefab.GetComponent<ZNetScene>();
        Helpers._ObjectDB = __instance.m_objectDBPrefab.GetComponent<ObjectDB>();
        foreach(var clone in Clones) clone.Create();
        foreach(var location in LocationManager.CustomLocation.locations.Values) location.Setup();
        foreach(var blueprint in Blueprints.Values) blueprint.Create();
        foreach(var location in BlueprintLocations.Values) location.Create();
        SoftAssetLoader.AddBlueprintSoftReferences();
    }
}