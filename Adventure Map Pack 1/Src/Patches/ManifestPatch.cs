using HarmonyLib;
using SoftReferenceableAssets;
using UnityEngine;

namespace Adventure_Map_Pack_1
{
    [HarmonyPatch(typeof(AssetBundleLoader), nameof(AssetBundleLoader.OnInitCompleted))]
    public static class AssetManifestDebugPatch
    {
        public static void Postfix(AssetBundleLoader __instance)
        {
            Debug.Log("[Adventure_Map_Pack_1] ===== SoftRef System Initialized =====");

            // Log all registered bundle names
            foreach (var pair in __instance.m_bundleNameToLoaderIndex)
            {
                Debug.Log("[Adventure_Map_Pack_1] Registered Bundle: " + pair.Key + " at index "+ pair.Value);
            }

            Debug.Log("[Adventure_Map_Pack_1] =====================================");
        }
    }
}