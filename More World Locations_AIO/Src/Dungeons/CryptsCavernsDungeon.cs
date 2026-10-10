using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons;

public static class CryptsCavernsDungeon
{
    // Working theme shared by the room set and the temporary generator.
    public const string ThemeName = "MWL_CryptsCaverns";
    public const string TestExteriorName = "MWL_CryptsCaverns_Test";
    public const string TestGeneratorName = "MWL_DG_CryptsCaverns_Test";
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

    public static void RegisterTestExterior()
    {
        ZoneManager.OnVanillaLocationsAvailable -= RegisterTestExterior;

        foreach (MWLRoom room in Rooms)
        {
            if (!AssetManager.Instance.GetSoftReference<GameObject>(room.Name).IsValid)
            {
                return;
            }
        }

        CustomLocation exterior = ZoneManager.Instance.CreateClonedLocation(TestExteriorName, "Crypt4");
        // Manual test placement only. Do not add this temporary entrance to world generation.
        exterior.ZoneLocation.m_quantity = 0;
        exterior.ZoneLocation.m_prioritized = false;

        DungeonGenerator generator = exterior.Prefab.GetComponentInChildren<DungeonGenerator>(true);
        if (generator == null)
        {
            ZoneManager.Instance.DestroyCustomLocation(TestExteriorName);
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                "Crypt4 test exterior has no dungeon generator.");
            return;
        }

        // Exclude the original crypt room pool and its required-room names.
        // ZoneManager registers this child as its own network prefab. A unique name
        // prevents prefab-based spawners from substituting the vanilla generator.
        generator.gameObject.name = TestGeneratorName;
        generator.m_themes = Room.Theme.None;
        generator.m_requiredRooms.Clear();
        generator.m_minRequiredRooms = 0;
        DungeonManager.Instance.RegisterDungeonTheme(generator.gameObject, ThemeName);

        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
            $"Registered manual test exterior {TestExteriorName} from Crypt4 with theme {ThemeName}.");
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
