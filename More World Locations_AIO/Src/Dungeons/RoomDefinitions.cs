using Jotunn.Configs;

namespace More_World_Locations_AIO.Dungeons;

public static class RoomDefinitions
{
    private const string UndergroundRuinsTheme = "Underground Ruins";
    private const string UndergroundRuinsRoomPath = "Assets/WarpProjects/Dungeons/UndergroundRuins/Rooms/";

    public static readonly MWLRoom[] UndergroundRuins =
    {
        new() { Name = "BFD_MainEntrance2", AssetPath = UndergroundRuinsRoomPath + "BFD_MainEntrance2.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Entrance = true, Weight = 1.0f } },

        new() { Name = "BFD_Modular3", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular3.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_Modular5", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular5.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f, MinPlaceOrder = 2 } },

        new() { Name = "BFD_Modular6", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular6.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_Modular7", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular7.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_Modular9", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular9.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_Modular10", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular10.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_Modular11", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular11.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_Modular12", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular12.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_Modular13", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular13.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_Modular14", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular14.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_ModularElbow", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularElbow.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 1.0f } },

        new() { Name = "BFD_ModularEnd3", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularEnd3.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "BFD_ModularEnd4", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularEnd4.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "BFD_ModularEnd5", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularEnd5.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "BFD_ModularEnd6", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularEnd6.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "BFD_ModularEnd7", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularEnd7.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "BFD_ModularEnd8", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularEnd8.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "BFD_ModularEnd9", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularEnd9.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "BFD_ModularDivider1", AssetPath = UndergroundRuinsRoomPath + "BFD_ModularDivider1.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Entrance = false, Endcap = false, Divider = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "BFD_Stairwell3", AssetPath = UndergroundRuinsRoomPath + "BFD_Stairwell3.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 0.8f } },

        new() { Name = "BFD_Stairwell5", AssetPath = UndergroundRuinsRoomPath + "BFD_Stairwell5.prefab",
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 0.9f } },

        // PlacementLimit = 1: RoomExtras caps each at one placement per dungeon; Weight = 0
        // keeps them out of normal weighted picks so the limit mechanic decides placement.
        new() { Name = "BFD_Modular8_Puzzle", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular8_Puzzle.prefab",
            PlacementLimit = 1,
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 0f } },

        new() { Name = "BFD_Modular12_Solution", AssetPath = UndergroundRuinsRoomPath + "BFD_Modular12_Solution.prefab",
            PlacementLimit = 1,
            Config = new RoomConfig { ThemeName = UndergroundRuinsTheme, Weight = 0f } },
    };
}
