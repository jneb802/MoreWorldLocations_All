using System.IO;
using System.Reflection;
using Common;
using HarmonyLib;
using Jotunn.Utils;
using SoftReferenceableAssets;
using UnityEngine;
using BepInEx;

namespace More_World_Locations_AIO;

public class AssetBundles
{
    public static AssetBundle assetBundle;
    public static string bundleName = "meadowspack1";
    
    [HarmonyPatch(typeof(EntryPointSceneLoader), "Start")]
    public static class EntryPatch
    {
        public static void Prefix()
        {
            string manifestPath = Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-Adventure_Map_Pack_1", "manifest_two");
            string manifestPath2 = Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-Adventure_Map_Pack_1", "asset_bundle_manifest");

            Debug.Log("[Adventure_Map_Pack_1] Injecting custom manifest: " + manifestPath);
            Debug.Log("[Adventure_Map_Pack_1] Injecting custom manifest: " + manifestPath2);
            
            Runtime.AddManifest(manifestPath);
            Runtime.AddManifest(manifestPath2);
        }
    }
    
    public static void LoadAssetBundles()
    {
        assetBundle = AssetUtils.LoadAssetBundleFromResources(
            bundleName,
            Assembly.GetExecutingAssembly()
        );
        if (assetBundle == null)
        {
            WarpLogger.Logger.LogError("Failed to load asset bundle with name: " + bundleName);
        }
    }
    
    
}