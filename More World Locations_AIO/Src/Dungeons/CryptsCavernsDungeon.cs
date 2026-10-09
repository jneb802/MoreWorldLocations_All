using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons;

public static class CryptsCavernsDungeon
{
    // Working theme name. The future generator and exterior must use the same value.
    public const string ThemeName = "MWL_CryptsCaverns";
    public const string RoomAssetPath = "Assets/WarpProjects/CryptRecovery/Rooms/";

    public static readonly MWLRoom[] Rooms =
    {
        CreateRoom("MCRoomEntrance_ru"),
        CreateRoom("MCRoom27_ru"),
        CreateRoom("MCRoom26_ru"),
        CreateRoom("MCRoom24_ru"),
        CreateRoom("MCRoom23_ru"),
        CreateRoom("MCRoom22_ru"),
        CreateRoom("MCRoom19_ru"),
        CreateRoom("MCRoom18_ru"),
        CreateRoom("MCRoom17_ru"),
        CreateRoom("MCRoom16_ru"),
        CreateRoom("MCRoom15_ru"),
        CreateRoom("MCRoom14_ru"),
        CreateRoom("MCRoom13_ru"),
        CreateRoom("MCRoom12_ru"),
        CreateRoom("MCRoom11_ru"),
        CreateRoom("MCRoom10_ru"),
        CreateRoom("MCRoom9_ru"),
        CreateRoom("MCRoom8_ru"),
        CreateRoom("MCRoom7_ru"),
        CreateRoom("MCRoom4_ru"),
        CreateRoom("MCRoom3_ru"),
        CreateRoom("MC_EndCap_ru"),
    };

    public static void RegisterRoomsIfAvailable()
    {
        // Room bundles are still being authored. Do not register a partial room set.
        // This checks manifest references without loading the room prefabs into memory.
        List<string> missingRooms = new List<string>();
        foreach (MWLRoom room in Rooms)
        {
            if (!AssetManager.Instance.GetSoftReference<GameObject>(room.Name).IsValid)
            {
                missingRooms.Add(room.Name);
            }
        }

        if (missingRooms.Count == Rooms.Length)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                "Crypts & Caverns rooms are not installed; skipping room registration.");
            return;
        }

        if (missingRooms.Count > 0)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                $"Crypts & Caverns room assets are incomplete; skipping all rooms. Missing: {string.Join(", ", missingRooms)}");
            return;
        }

        foreach (MWLRoom room in Rooms)
        {
            room.Register();
        }
    }

    private static MWLRoom CreateRoom(string name)
    {
        return new MWLRoom
        {
            Name = name,
            AssetPath = RoomAssetPath + name + ".prefab",
            // Preserve the Room settings authored in Unity, including entrance/endcap
            // flags, weights, placement order and enabled state.
            Config = new RoomConfig { ThemeName = ThemeName },
        };
    }
}
