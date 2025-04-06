// using System;
// using System.Collections.Generic;
// using HarmonyLib;
// using UnityEngine;
//
// namespace Dungeon_Castle.Patches;
//
// public class ZnetScene_patch
// {
//     [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.CreateObjectsSorted))]
//     public class ZNetScene_CreateObjectsSorted_Patch
//     {
//         public static void Prefix(ZNetScene __instance, List<ZDO> currentNearObjects, int maxCreatedPerFrame, ref int created)
//         {
//             if (!ZoneSystem.instance.IsActiveAreaLoaded())
//                 return;
//             __instance.m_tempCurrentObjects2.Clear();
//             Vector3 referencePosition = ZNet.instance.GetReferencePosition();
//             foreach (ZDO currentNearObject in currentNearObjects)
//             {
//                 if (!currentNearObject.Created)
//                 {
//                     currentNearObject.m_tempSortValue = Utils.DistanceSqr(referencePosition, currentNearObject.GetPosition());
//                     __instance.m_tempCurrentObjects2.Add(currentNearObject);
//                 }
//             }
//             int num = Mathf.Max(__instance.m_tempCurrentObjects2.Count / 100, maxCreatedPerFrame);
//             __instance.m_tempCurrentObjects2.Sort(new Comparison<ZDO>(ZNetScene.ZDOCompare));
//             foreach (ZDO zdo in __instance.m_tempCurrentObjects2)
//             {
//                 if (ZoneSystem.instance.IsZoneReadyForType(zdo.GetSector(), zdo.Type))
//                 {
//                     if ((UnityEngine.Object) __instance.CreateObject(zdo) != (UnityEngine.Object) null)
//                     {
//                         ++created;
//                         if (created > num)
//                             break;
//                     }
//                     else if (ZNet.instance.IsServer())
//                     {
//                         zdo.SetOwner(ZDOMan.GetSessionID());
//                         ZLog.Log((object) ("Destroyed invalid predab ZDO:" + zdo.m_uid.ToString()));
//                         Debug.Log("Destroyed invalid predab ZDO with name:" + GetPrefabNameFromHash(zdo.m_prefab));
//                         ZDOMan.instance.DestroyZDO(zdo);
//                     }
//                 }
//             }
//
//             return;
//         }
//         
//         public static string GetPrefabNameFromHash(int prefabHash)
//         {
//             foreach (var kvp in ZNetScene.instance.m_namedPrefabs)
//             {
//                 if (kvp.Key == prefabHash)
//                 {
//                     return kvp.Value.name;
//                 }
//             }
//             return null; // Not found
//         }
//         
//         [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
//         [HarmonyAfter("*")]
//         public class ZNetScene_Awake_Patch
//         {
//             public static void Postfix(ZNetScene __instance)
//             {
//                 foreach (GameObject prefab in __instance.m_prefabs)
//                     Debug.Log("Adding prefab with hashcode: " + prefab.name.GetStableHashCode() + " and name: " + prefab.name );
//                 foreach (GameObject nonNetViewPrefab in __instance.m_nonNetViewPrefabs)
//                     Debug.Log("Adding prefab with hashcode: " + nonNetViewPrefab.name.GetStableHashCode() + " and name: " + nonNetViewPrefab.name );;
//             }
//         }
//     }
// }