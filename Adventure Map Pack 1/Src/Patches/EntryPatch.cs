
using System.IO;
using BepInEx;
using HarmonyLib;
using SoftReferenceableAssets;
using UnityEngine;

namespace Adventure_Map_Pack_1
{
    [HarmonyPatch(typeof(EntryPointSceneLoader), "Start")]
    public static class EntryPatch
    {
        public static void Prefix()
        {
            string manifestPath = Path.Combine(Paths.PluginPath, "warpalicious-Adventure_Map_Pack_1", "manifest_two");
            string manifestPath2 = Path.Combine(Paths.PluginPath, "warpalicious-Adventure_Map_Pack_1", "asset_bundle_manifest");

            Debug.Log("[Adventure_Map_Pack_1] Injecting custom manifest: " + manifestPath);
            Debug.Log("[Adventure_Map_Pack_1] Injecting custom manifest: " + manifestPath2);
            
            Runtime.AddManifest(manifestPath);
            Runtime.AddManifest(manifestPath2);
        }
    }
}