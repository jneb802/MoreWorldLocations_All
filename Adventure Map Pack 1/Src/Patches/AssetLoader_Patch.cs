// using HarmonyLib;
// using SoftReferenceableAssets;
//
// namespace Adventure_Map_Pack_1.Patches;
//
// public class AssetLoader_Patch
// {
//     [HarmonyPatch(typeof(AssetLoader), "LoadAssetFromBundleAsync")]
//     public class PreventLoadAssetFromBundleCrash
//     {
//         public static bool Prefix(ref AssetLoader __instance)
//         {
//             if (__instance.Asset != null)
//             {
//                 // asset already set, just succeed
//                 __instance.InvokeCallbacks(LoadResult.Succeeded); // might need a patch-access method
//                 return false; // skip original method
//             }
//
//             return true; // proceed as normal
//         }
//     }
// }