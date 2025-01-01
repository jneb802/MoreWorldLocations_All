using HarmonyLib;
using Jotunn;
using UnityEngine;

namespace Underground_Ruins;

public class DungeonGenerator_Patch
{
    [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Load))]
    public class DungeonGenerator_Awake_Patch
    {
        public static void Prefix(DungeonGenerator __instance)
        {
            if (__instance.gameObject.name == "DG_BlackForestDungeon(Clone)")
            {
                // Debug.Log("Dungeon calling Load, first add attach puzzle to dungeon generator");
                __instance.gameObject.AddComponent<AttachPuzzle>();
            }
            else
            {
                // Debug.Log("Failed to find dungeon generator with name DG_BlackForestDungeon(Clone)");
            }
        }
    }
    
    [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Generate), typeof(int), typeof(ZoneSystem.SpawnMode))]
    public class DungeonGenerator_Generate_Patch
    {
        public static void Postfix(DungeonGenerator __instance)
        {
            if (__instance.gameObject.name == "DG_BlackForestDungeon(Clone)")
            {
                if (__instance == null)
                {
                    return;
                }
                // Debug.Log("Dungeon generator finished calling Generate, now call AttachPuzzle.Initialize");
                __instance.gameObject.GetComponent<AttachPuzzle>().Initialize();
            }
            else
            {
                // Debug.Log("Failed to find dungeon generator with name DG_BlackForestDungeon(Clone)");
            }
        }
    }
    
    // [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.PlaceRoom), 
    //     typeof(DungeonDB.RoomData), 
    //     typeof(Vector3), 
    //     typeof(Quaternion), 
    //     typeof(RoomConnection), 
    //     typeof(ZoneSystem.SpawnMode))]
    // public class DungeonGenerator_PlaceRoom_Patch
    // {
    //     public static void Postfix(DungeonGenerator __instance, DungeonDB.RoomData roomData, Vector3 pos)
    //     {
    //         if (__instance.gameObject.name == "DG_BlackForestDungeon(Clone)")
    //         {
    //             if (__instance == null)
    //             {
    //                 return;
    //             }
    //             
    //             Debug.Log("Attempting to change position of boss room");
    //             Debug.Log("Room prefab asset name is: " + roomData.m_prefab.Name);
    //             Debug.Log("RoomInPrefab name is: " + roomData.RoomInPrefab);
    //
    //             if (roomData.m_prefab.Asset.name == "BFD_Modular17_Boss")
    //             {
    //                 Debug.Log("Room name matches");
    //                 GameObject bossRoom = roomData.m_prefab.Asset.FindDeepChild("BossRoom").gameObject;
    //                 if (bossRoom != null)
    //                 {
    //                     Debug.Log("Found boss room in gameobject");
    //                     Debug.Log("Boss room starting position is: " + bossRoom.transform.position);
    //                     Vector2i zone = ZoneSystem.GetZone(__instance.gameObject.transform.position);
    //                     Vector3 zoneCenterPos = ZoneSystem.GetZonePos(zone);
    //                     zoneCenterPos.y = 5500;
    //                     bossRoom.transform.position = zoneCenterPos;
    //                     Debug.Log("Boss room updated position is: " + bossRoom.transform.position);
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             Debug.Log("Failed to find dungeon generator with name DG_BlackForestDungeon(Clone)");
    //         }
    //     }
    // }
}