using HarmonyLib;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RoomExtras : MonoBehaviour
{
    public int PlacementLimit = 0; // Allow room to be placed x times
    public string PlacementGroup = string.Empty; // Group this room with others for placement counts
    // TODO - Other placement adjustments?

    [HideInInspector]
    public static Dictionary<string, int> placements = new Dictionary<string, int>();
    private static string lastRoomPlaced = "";


    public static void ApplyPatches(Harmony harmony)
    {
        if (harmony == null) throw new ArgumentNullException(nameof(harmony));
        harmony.PatchAll(typeof(Patches));
    }

    private static class Patches
    {
        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.SetupAvailableRooms)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
        private static void OnDungeonGeneratorSetupAvailableRooms() => RoomExtras.OnDungeonGeneratorSetupAvailableRooms();

        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Generate), typeof(int), typeof(ZoneSystem.SpawnMode)), HarmonyPostfix]
        private static void OnDungeonGeneratorGenerate() => RoomExtras.OnDungeonGeneratorGenerate();

        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.PlaceOneRoom)), HarmonyPostfix]
        private static void OnDungeonGeneratorPlaceOneRoom(bool __result) => RoomExtras.OnDungeonGeneratorPlaceOneRoom(__result);

        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.PlaceRoom), typeof(RoomConnection), typeof(DungeonDB.RoomData), typeof(ZoneSystem.SpawnMode)), HarmonyPostfix]
        private static void OnDungeonGeneratorPlaceRoom(bool __result, DungeonDB.RoomData roomData) => RoomExtras.OnDungeonGeneratorPlaceRoom(__result, roomData);
    }

    private static void OnDungeonGeneratorSetupAvailableRooms()
    {
        // detect extras, if any, and initialize placement tally for them
        foreach (var roomData in DungeonGenerator.m_availableRooms)
        {
            if (roomData.m_prefab.IsLoaded && roomData.RoomInPrefab.TryGetComponent<RoomExtras>(out var extras) && extras?.PlacementLimit > 0)
            {
                if (!string.IsNullOrEmpty(extras.PlacementGroup))
                    RoomExtras.placements[extras.PlacementGroup] = 0;
                else
                    RoomExtras.placements[extras.name] = 0;
            }
        }
    }

    private static void OnDungeonGeneratorGenerate()
    {
        // reset placements after generation ends
        //List<string> reks = RoomExtras.placements.Keys.ToList();
        //RoomExtras.placements = new Dictionary<string, int>();
        //foreach (var key in reks)
        //    RoomExtras.placements[key] = 0;
        placements.Clear();
    }

    private static void OnDungeonGeneratorPlaceRoom(bool result, DungeonDB.RoomData roomData)
    {
        if (result)
            lastRoomPlaced = roomData.RoomInPrefab.name;
    }

    private static void OnDungeonGeneratorPlaceOneRoom(bool wasPlaced)
    {
        // Measure if room placements have reached limit.

        if (wasPlaced && !string.IsNullOrEmpty(lastRoomPlaced))
        {
            var roomData = DungeonGenerator.m_availableRooms.Where(r => string.Equals(lastRoomPlaced, r.RoomInPrefab.name)).FirstOrDefault();
            if (roomData.RoomInPrefab.TryGetComponent<RoomExtras>(out var extras))
            {
                int count = 0;
                if (!string.IsNullOrEmpty(extras.PlacementGroup) && RoomExtras.placements.ContainsKey(extras.PlacementGroup))
                {
                    count = RoomExtras.placements[extras.PlacementGroup] += 1;
                    if (extras.PlacementLimit != 0 && count >= extras.PlacementLimit)
                    {
                        Debug.Log($"[{nameof(RoomExtras)}] Removing group {extras.PlacementGroup}, limit reached {count}");
                        DungeonGenerator.m_tempRooms.RemoveAll(r => r.RoomInPrefab.TryGetComponent<RoomExtras>(out var re) && re.PlacementGroup == extras.PlacementGroup);
                        DungeonGenerator.m_availableRooms.RemoveAll(r => r.RoomInPrefab.TryGetComponent<RoomExtras>(out var re) && re.PlacementGroup == extras.PlacementGroup);
                    }
                }
                else if (RoomExtras.placements.ContainsKey(extras.name))
                {
                    count = RoomExtras.placements[extras.name] += 1;
                    if (extras.PlacementLimit != 0 && count >= extras.PlacementLimit)
                    {
                        Debug.Log($"[{nameof(RoomExtras)}] Removing {extras.name}, limit reached {count}");
                        DungeonGenerator.m_tempRooms.Remove(roomData);
                        DungeonGenerator.m_availableRooms.Remove(roomData);
                    }
                }
            }
        }
    }
}
