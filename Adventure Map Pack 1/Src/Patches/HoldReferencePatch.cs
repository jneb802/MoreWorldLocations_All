// using System;
// using System.Collections.Generic;
// using HarmonyLib;
// using SoftReferenceableAssets;
// using UnityEngine;
//
// namespace Adventure_Map_Pack_1.Patches;
//
// public class HoldReferencePatch
// {
//     [HarmonyPatch(typeof(SoftReferenceableAssets.AssetLoader), nameof(SoftReferenceableAssets.AssetLoader.HoldReference))]
//     public class AssetLoader_HoldReference_Patch
//     {
//         public static void Prefix(SoftReferenceableAssets.AssetLoader __instance)
//         {
//             Debug.Log("[Patch] AssetLoader.HoldReference invoked.");
//
//             // ReferenceCounter is a struct, so check using .Equals(default)
//             if (__instance.m_referenceCounter.Equals(default(SoftReferenceableAssets.ReferenceCounter)))
//             {
//                 Debug.LogError("[Patch] m_referenceCounter is uninitialized (default struct value)!");
//             }
//             else
//             {
//                 Debug.Log($"[Patch] m_referenceCounter initialized. Value: {__instance.m_referenceCounter.Value}");
//             }
//
//
//             var loader = SoftReferenceableAssets.AssetBundleLoader.Instance;
//             if (loader == null)
//             {
//                 Debug.LogError("[Patch] AssetBundleLoader.Instance is null!");
//                 return;
//             }
//
//             if (loader.m_bundleLoaders == null)
//             {
//                 Debug.LogError("[Patch] m_bundleLoaders is null!");
//                 return;
//             }
//
//             if (__instance.m_bundleLoaderIndex < 0 || __instance.m_bundleLoaderIndex >= loader.m_bundleLoaders.Length)
//             {
//                 Debug.LogError($"[Patch] Invalid m_bundleLoaderIndex: {__instance.m_bundleLoaderIndex} (bundleLoaders.Length: {loader.m_bundleLoaders.Length})");
//                 return;
//             }
//
//             var bundleLoader = loader.m_bundleLoaders[__instance.m_bundleLoaderIndex];
//             if (bundleLoader.BundleLoaderIndices == null)
//             {
//                 Debug.LogError("[Patch] BundleLoaderIndices is null!");
//             }
//             else
//             {
//                 Debug.Log($"[Patch] BundleLoaderIndices count: {bundleLoader.BundleLoaderIndices.Length}");
//             }
//         }
//     }
// }