using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons;

public class RoomExtras : MonoBehaviour
{
    public int PlacementLimit = 0;
    public string PlacementGroup = string.Empty;

    [HideInInspector]
    public static Dictionary<string, int> placements = new Dictionary<string, int>();

    private static string lastRoomPlaced = string.Empty;

    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.SetupAvailableRooms))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void OnDungeonGeneratorSetupAvailableRooms() => RoomExtras.OnDungeonGeneratorSetupAvailableRooms();

        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Generate), typeof(int), typeof(ZoneSystem.SpawnMode))]
        [HarmonyPostfix]
        private static void OnDungeonGeneratorGenerate() => RoomExtras.OnDungeonGeneratorGenerate();

        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.PlaceOneRoom))]
        [HarmonyPostfix]
        private static void OnDungeonGeneratorPlaceOneRoom(bool __result) => RoomExtras.OnDungeonGeneratorPlaceOneRoom(__result);

        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.PlaceRoom), typeof(RoomConnection), typeof(DungeonDB.RoomData), typeof(ZoneSystem.SpawnMode))]
        [HarmonyPostfix]
        private static void OnDungeonGeneratorPlaceRoom(bool __result, DungeonDB.RoomData roomData) => RoomExtras.OnDungeonGeneratorPlaceRoom(__result, roomData);
    }

    private static void OnDungeonGeneratorSetupAvailableRooms()
    {
        foreach (DungeonDB.RoomData roomData in DungeonGenerator.m_availableRooms)
        {
            if (roomData.m_prefab.IsLoaded
                && roomData.RoomInPrefab.TryGetComponent(out RoomExtras extras)
                && extras?.PlacementLimit > 0)
            {
                string key = !string.IsNullOrEmpty(extras.PlacementGroup) ? extras.PlacementGroup : extras.name;
                placements[key] = 0;
            }
        }
    }

    private static void OnDungeonGeneratorGenerate()
    {
        placements.Clear();
    }

    private static void OnDungeonGeneratorPlaceRoom(bool result, DungeonDB.RoomData roomData)
    {
        if (result)
            lastRoomPlaced = roomData.RoomInPrefab.name;
    }

    private static void OnDungeonGeneratorPlaceOneRoom(bool wasPlaced)
    {
        if (!wasPlaced || string.IsNullOrEmpty(lastRoomPlaced)) return;

        DungeonDB.RoomData roomData = DungeonGenerator.m_availableRooms
            .FirstOrDefault(r => string.Equals(lastRoomPlaced, r.RoomInPrefab.name));

        if (roomData == null || !roomData.RoomInPrefab.TryGetComponent(out RoomExtras extras)) return;
        if (extras.PlacementLimit == 0) return;

        if (!string.IsNullOrEmpty(extras.PlacementGroup) && placements.ContainsKey(extras.PlacementGroup))
        {
            int count = placements[extras.PlacementGroup] += 1;
            if (count >= extras.PlacementLimit)
            {
                Debug.Log($"[{nameof(RoomExtras)}] Removing group {extras.PlacementGroup}, limit reached {count}");
                DungeonGenerator.m_tempRooms.RemoveAll(r => r.RoomInPrefab.TryGetComponent(out RoomExtras re) && re.PlacementGroup == extras.PlacementGroup);
                DungeonGenerator.m_availableRooms.RemoveAll(r => r.RoomInPrefab.TryGetComponent(out RoomExtras re) && re.PlacementGroup == extras.PlacementGroup);
            }
        }
        else if (placements.ContainsKey(extras.name))
        {
            int count = placements[extras.name] += 1;
            if (count >= extras.PlacementLimit)
            {
                Debug.Log($"[{nameof(RoomExtras)}] Removing {extras.name}, limit reached {count}");
                DungeonGenerator.m_tempRooms.Remove(roomData);
                DungeonGenerator.m_availableRooms.Remove(roomData);
            }
        }
    }
}
