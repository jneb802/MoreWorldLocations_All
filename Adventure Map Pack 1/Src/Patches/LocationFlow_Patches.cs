// using System.Collections.Generic;
// using HarmonyLib;
// using UnityEngine;
//
// namespace Adventure_Map_Pack_1;
//
// public class LocationFlow_Patches
// {
//     [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
//     public static class ZoneSystem_SetupLocations_Patch
//     {
//         public static void Postfix(ZoneSystem __instance)
//         {
//             Debug.Log("ZoneSystem has called SetupLocations");
//         }
//     }
//     
//     [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
//     public static class ZoneSystem_SpawnLocation_Patch
//     {
//         public static void Postfix(ZoneSystem.ZoneLocation location, int seed, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects, GameObject __result)
//         {
//             Debug.Log("ZoneSystem has called SpawnLocation for location with name " + location.m_prefabName + " in zone: " + pos + ", SpawnMode " + mode);
//         }
//     }
//     
//     [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PokeCanSpawnLocation))]
//     public static class ZoneSystem_PokeCanSpawnLocation_Patch
//     {
//         public static void Postfix(ZoneSystem __instance, ZoneSystem.ZoneLocation location, bool isFirstSpawn)
//         {
//             Debug.Log("ZoneSystem has called PokeCanSpawnLocation for location with name: " + location.m_prefabName + ". isFirstSpawn:" + isFirstSpawn);
//         }
//     }
//     
//     [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnProxyLocation))]
//     public static class ZoneSystem_SpawnProxyLocation_Patch
//     {
//         public static void Postfix(ZoneSystem __instance, int hash, int seed, Vector3 pos, Quaternion rot)
//         {
//             ZoneSystem.ZoneLocation location = __instance.GetLocation(hash);
//             if (location != null)
//             {
//                 Debug.Log("ZoneSystem has called SpawnProxyLocation for location with name: " + location.m_prefabName + " in zone: " + pos);
//             }
//         }
//     }
//     
//     [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.CreateLocationProxy))]
//     public static class ZoneSystem_CreateLocationProxy_Patch
//     {
//         public static void Postfix(ZoneSystem __instance, ZoneSystem.ZoneLocation location, int seed, Vector3 pos, Quaternion rotation, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
//         {
//             Debug.Log("ZoneSystem has called CreateLocationProxy for location with name: " + location.m_prefabName + " in zone: " + pos + ", SpawnMode " + mode);
//         }
//     }
//     
//     // [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RegisterLocation))]
//     // public static class ZoneSystem_RegisterLocation_Patch
//     // {
//     //     public static void Postfix(ZoneSystem __instance, ZoneSystem.ZoneLocation location, Vector3 pos, bool generated)
//     //     {
//     //         Debug.Log("ZoneSystem has called RegisterLocation for location with name: " + location.m_name);
//     //         Debug.Log("RegisterLocation. generated: " + generated);
//     //     }
//     // }
//     
//     [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceLocations))]
//     public static class ZoneSystem_PlaceLocations_Patch
//     {
//         public static void Postfix(ZoneSystem __instance, Vector2i zoneID, Vector3 zoneCenterPos, Transform parent, Heightmap hmap, List<ZoneSystem.ClearArea> clearAreas, ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
//         {
//             Debug.Log("ZoneSystem has called PlaceLocations for zone with ID: " + zoneID + " with mode " + mode);
//         }
//     }
// }