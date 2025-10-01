// using HarmonyLib;
// using UnityEngine;
// using SoftReferenceableAssets;
// using System.Collections.Generic;
// using System.Linq;
//
// public static class LocationAssetDebugger
// {
//     private static readonly Dictionary<string, AssetLoadingState> _locationStates = new Dictionary<string, AssetLoadingState>();
//     
//     private class AssetLoadingState
//     {
//         public string LocationName;
//         public bool IsCustomLocation;
//         public bool FirstSpawnAttempt = true;
//         public int SpawnAttempts = 0;
//         public bool PrefabLoadCalled = false;
//         public bool AssetAccessAttempted = false;
//         public bool WasVisible = false;
//         public LoadResult LastLoadResult;
//         public string LastError;
//     }
//
//     [HarmonyPatch(typeof(ZoneSystem), "PokeCanSpawnLocation")]
//     public static class ZoneSystem_PokeCanSpawnLocation_Debug
//     {
//         static void Prefix(ZoneSystem.ZoneLocation location, bool isFirstSpawn, out bool __state)
//         {
//             __state = IsCustomLocation(location);
//             
//             if (__state)
//             {
//                 var state = GetOrCreateState(location.m_prefab.Name);
//                 state.SpawnAttempts++;
//                 
//                 Debug.Log($"[LocationDebug] PokeCanSpawnLocation - {location.m_prefab.Name}");
//                 Debug.Log($"  - Spawn Attempt: {state.SpawnAttempts}");
//                 Debug.Log($"  - IsFirstSpawn: {isFirstSpawn}");
//                 Debug.Log($"  - Prefab IsValid: {location.m_prefab.IsValid}");
//                 Debug.Log($"  - Prefab IsLoading: {location.m_prefab.IsLoading}");
//                 Debug.Log($"  - Prefab IsLoaded: {location.m_prefab.IsLoaded}");
//             }
//         }
//         
//         static void Postfix(ZoneSystem.ZoneLocation location, bool isFirstSpawn, bool __result, bool __state)
//         {
//             if (__state)
//             {
//                 var state = GetOrCreateState(location.m_prefab.Name);
//                 Debug.Log($"[LocationDebug] PokeCanSpawnLocation Result - {location.m_prefab.Name}: {__result}");
//                 
//                 if (!__result)
//                 {
//                     Debug.Log($"  - Spawn BLOCKED - Asset not ready");
//                 }
//             }
//         }
//     }
//
//     [HarmonyPatch(typeof(ZoneSystem), "SpawnLocation")]
//     public static class ZoneSystem_SpawnLocation_Debug
//     {
//         static void Prefix(ZoneSystem.ZoneLocation location, int seed, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode)
//         {
//             if (IsCustomLocation(location))
//             {
//                 var state = GetOrCreateState(location.m_prefab.Name);
//                 
//                 Debug.Log($"[LocationDebug] SpawnLocation START - {location.m_prefab.Name}");
//                 Debug.Log($"  - Mode: {mode}");
//                 Debug.Log($"  - First Spawn: {state.FirstSpawnAttempt}");
//                 Debug.Log($"  - Prefab IsValid: {location.m_prefab.IsValid}");
//                 Debug.Log($"  - Prefab IsLoaded: {location.m_prefab.IsLoaded}");
//                 
//                 state.FirstSpawnAttempt = false;
//             }
//         }
//         
//         static void Postfix(ZoneSystem.ZoneLocation location, GameObject __result)
//         {
//             if (IsCustomLocation(location))
//             {
//                 var state = GetOrCreateState(location.m_prefab.Name);
//                 
//                 Debug.Log($"[LocationDebug] SpawnLocation END - {location.m_prefab.Name}");
//                 Debug.Log($"  - Result GameObject: {(__result != null ? "SUCCESS" : "NULL")}");
//                 
//                 if (__result != null)
//                 {
//                     // Check if the spawned objects are visible
//                     CheckVisibility(__result, state);
//                 }
//             }
//         }
//     }
//
//     [HarmonyPatch(typeof(SoftReference<GameObject>), "Load")]
//     public static class SoftReference_Load_Debug
//     {
//         static void Prefix(SoftReference<GameObject> __instance)
//         {
//             if (IsCustomLocationAsset(__instance))
//             {
//                 var state = GetOrCreateState(__instance.Name);
//                 state.PrefabLoadCalled = true;
//                 
//                 Debug.Log($"[LocationDebug] SoftReference.Load() CALLED - {__instance.Name}");
//                 Debug.Log($"  - IsValid: {__instance.IsValid}");
//                 Debug.Log($"  - IsLoading: {__instance.IsLoading}");
//                 Debug.Log($"  - IsLoaded: {__instance.IsLoaded}");
//             }
//         }
//         
//         static void Postfix(SoftReference<GameObject> __instance, LoadResult __result)
//         {
//             if (IsCustomLocationAsset(__instance))
//             {
//                 var state = GetOrCreateState(__instance.Name);
//                 state.LastLoadResult = __result;
//                 
//                 Debug.Log($"[LocationDebug] SoftReference.Load() RESULT - {__instance.Name}: {__result}");
//                 Debug.Log($"  - Post-Load IsLoaded: {__instance.IsLoaded}");
//                 
//                 if (__result != LoadResult.Succeeded)
//                 {
//                     state.LastError = $"Load failed with result: {__result}";
//                     Debug.LogError($"[LocationDebug] LOAD FAILED - {__instance.Name}: {__result}");
//                 }
//             }
//         }
//     }
//
//     [HarmonyPatch(typeof(SoftReference<GameObject>), "Asset", MethodType.Getter)]
//     public static class SoftReference_Asset_Debug
//     {
//         static void Prefix(SoftReference<GameObject> __instance)
//         {
//             if (IsCustomLocationAsset(__instance))
//             {
//                 var state = GetOrCreateState(__instance.Name);
//                 state.AssetAccessAttempted = true;
//                 
//                 Debug.Log($"[LocationDebug] SoftReference.Asset ACCESSED - {__instance.Name}");
//                 Debug.Log($"  - IsLoaded: {__instance.IsLoaded}");
//             }
//         }
//         
//         static void Postfix(SoftReference<GameObject> __instance, GameObject __result)
//         {
//             if (IsCustomLocationAsset(__instance))
//             {
//                 Debug.Log($"[LocationDebug] SoftReference.Asset RESULT - {__instance.Name}: {(__result != null ? "SUCCESS" : "NULL")}");
//                 
//                 if (__result != null)
//                 {
//                     // Log asset details
//                     LogAssetDetails(__result);
//                 }
//             }
//         }
//     }
//
//     [HarmonyPatch(typeof(SoftReferenceableAssets.Utils), "Instantiate", new[] { typeof(SoftReference<GameObject>), typeof(Vector3), typeof(Quaternion) })]
//     public static class Utils_Instantiate_Debug
//     {
//         static void Prefix(SoftReference<GameObject> original)
//         {
//             if (IsCustomLocationAsset(original))
//             {
//                 Debug.Log($"[LocationDebug] Utils.Instantiate() CALLED - {original.Name}");
//                 Debug.Log($"  - IsLoaded: {original.IsLoaded}");
//                 Debug.Log($"  - Asset null check: {(original.Asset != null)}");
//             }
//         }
//         
//         static void Postfix(SoftReference<GameObject> original, GameObject __result)
//         {
//             if (IsCustomLocationAsset(original))
//             {
//                 Debug.Log($"[LocationDebug] Utils.Instantiate() RESULT - {original.Name}: {(__result != null ? "SUCCESS" : "NULL")}");
//                 
//                 if (__result != null)
//                 {
//                     var state = GetOrCreateState(original.Name);
//                     CheckVisibility(__result, state);
//                 }
//             }
//         }
//     }
//
//     [HarmonyPatch(typeof(ZoneSystem.LocationPrefabLoadData), "OnPrefabLoaded")]
//     public static class LocationPrefabLoadData_OnPrefabLoaded_Debug
//     {
//         static void Prefix(AssetID assetID, LoadResult result)
//         {
//             var assetPath = SoftReferenceableAssets.Runtime.GetAssetPath(assetID);
//             Debug.Log($"[LocationDebug] LocationPrefabLoadData.OnPrefabLoaded - AssetID: {assetID}");
//             Debug.Log($"  - Asset Path: {assetPath}");
//             Debug.Log($"  - Load Result: {result}");
//         }
//     }
//
//     // Helper methods
//     private static bool IsCustomLocation(ZoneSystem.ZoneLocation location)
//     {
//         // Check if this is one of your custom locations
//         return Jotunn.Entities.CustomLocation.IsCustomLocation(location.m_prefabName);
//     }
//     
//     private static bool IsCustomLocationAsset(SoftReference<GameObject> softRef)
//     {
//         return Jotunn.Entities.CustomLocation.IsCustomLocation(softRef.Name);
//     }
//     
//     private static AssetLoadingState GetOrCreateState(string locationName)
//     {
//         if (!_locationStates.ContainsKey(locationName))
//         {
//             _locationStates[locationName] = new AssetLoadingState
//             {
//                 LocationName = locationName,
//                 IsCustomLocation = true
//             };
//         }
//         return _locationStates[locationName];
//     }
//     
//     private static void LogAssetDetails(GameObject asset)
//     {
//         Debug.Log($"[LocationDebug] Asset Details:");
//         Debug.Log($"  - Name: {asset.name}");
//         Debug.Log($"  - Active: {asset.activeInHierarchy}");
//         Debug.Log($"  - Components: {asset.GetComponents<Component>().Length}");
//         
//         // Check renderers
//         var renderers = asset.GetComponentsInChildren<Renderer>();
//         Debug.Log($"  - Renderers found: {renderers.Length}");
//         
//         foreach (var renderer in renderers.Take(5)) // Limit to first 5 to avoid spam
//         {
//             Debug.Log($"    - Renderer: {renderer.name}");
//             Debug.Log($"      - Enabled: {renderer.enabled}");
//             Debug.Log($"      - Materials: {renderer.materials?.Length ?? 0}");
//             
//             if (renderer.materials != null)
//             {
//                 foreach (var material in renderer.materials.Take(3)) // Limit materials
//                 {
//                     Debug.Log($"        - Material: {(material != null ? material.name : "NULL")}");
//                     if (material != null)
//                     {
//                         Debug.Log($"          - Shader: {(material.shader != null ? material.shader.name : "NULL")}");
//                     }
//                 }
//             }
//         }
//     }
//     
//     private static void CheckVisibility(GameObject spawnedObject, AssetLoadingState state)
//     {
//         var renderers = spawnedObject.GetComponentsInChildren<Renderer>();
//         bool hasVisibleRenderers = renderers.Any(r => r.enabled && r.gameObject.activeInHierarchy);
//         
//         state.WasVisible = hasVisibleRenderers;
//         
//         Debug.Log($"[LocationDebug] Visibility Check - {state.LocationName}:");
//         Debug.Log($"  - Total Renderers: {renderers.Length}");
//         Debug.Log($"  - Visible Renderers: {renderers.Count(r => r.enabled && r.gameObject.activeInHierarchy)}");
//         Debug.Log($"  - Has Visible Content: {hasVisibleRenderers}");
//         
//         if (!hasVisibleRenderers && renderers.Length > 0)
//         {
//             Debug.LogWarning($"[LocationDebug] INVISIBLE LOCATION DETECTED - {state.LocationName}");
//             
//             // Log first few renderers for debugging
//             foreach (var renderer in renderers.Take(3))
//             {
//                 Debug.Log($"  - Renderer {renderer.name}: enabled={renderer.enabled}, active={renderer.gameObject.activeInHierarchy}");
//                 Debug.Log($"    - Materials: {string.Join(", ", renderer.materials.Select(m => m?.name ?? "NULL"))}");
//             }
//         }
//     }
//     
//     // Call this method to get a summary of all location states
//     public static void LogLocationStates()
//     {
//         Debug.Log($"[LocationDebug] === LOCATION STATES SUMMARY ===");
//         
//         foreach (var kvp in _locationStates)
//         {
//             var state = kvp.Value;
//             Debug.Log($"Location: {state.LocationName}");
//             Debug.Log($"  - Spawn Attempts: {state.SpawnAttempts}");
//             Debug.Log($"  - Prefab Load Called: {state.PrefabLoadCalled}");
//             Debug.Log($"  - Asset Access Attempted: {state.AssetAccessAttempted}");
//             Debug.Log($"  - Was Visible: {state.WasVisible}");
//             Debug.Log($"  - Last Load Result: {state.LastLoadResult}");
//             if (!string.IsNullOrEmpty(state.LastError))
//             {
//                 Debug.Log($"  - Last Error: {state.LastError}");
//             }
//         }
//     }
// }