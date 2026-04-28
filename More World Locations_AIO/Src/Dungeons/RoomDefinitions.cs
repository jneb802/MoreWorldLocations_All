using Jotunn.Configs;

namespace More_World_Locations_AIO.Dungeons;

public static class RoomDefinitions
{
    private const string UndergroundRuinsTheme = "Underground Ruins";
    private const string UndergroundRuinsRoomPath = "Assets/WarpProjects/Dungeons/UndergroundRuins/Rooms/";
    private const string ForbiddenCatacombsTheme = "CD_Catacomb";
    private const string ForbiddenCatacombsRoomPath = "Assets/WarpProjects/Dungeons/ForbiddenCatacombs/Rooms/";

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

    public static readonly MWLRoom[] ForbiddenCatacombs =
    {
        new() { Name = "CD_Entrance2", AssetPath = ForbiddenCatacombsRoomPath + "CD_Entrance2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Entrance = true, Weight = 1.0f } },

        new() { Name = "CD_Room1", AssetPath = ForbiddenCatacombsRoomPath + "CD_Room1.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Room2", AssetPath = ForbiddenCatacombsRoomPath + "CD_Room2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Room3", AssetPath = ForbiddenCatacombsRoomPath + "CD_Room3.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Room4", AssetPath = ForbiddenCatacombsRoomPath + "CD_Room4.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_RoomBig1", AssetPath = ForbiddenCatacombsRoomPath + "CD_RoomBig1.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_RoomBig2", AssetPath = ForbiddenCatacombsRoomPath + "CD_RoomBig2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_RoomBig3", AssetPath = ForbiddenCatacombsRoomPath + "CD_RoomBig3.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Hallway1", AssetPath = ForbiddenCatacombsRoomPath + "CD_Hallway1.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Hallway2", AssetPath = ForbiddenCatacombsRoomPath + "CD_Hallway2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Hallway3", AssetPath = ForbiddenCatacombsRoomPath + "CD_Hallway3.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Endcap1", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap1.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap2", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap4", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap4.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap5", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap5.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap6", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap6.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap7", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap7.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap8", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap8.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap9", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap9.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap10", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap10.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap11", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap11.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap12", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap12.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap13", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap13.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Endcap14", AssetPath = ForbiddenCatacombsRoomPath + "CD_Endcap14.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Lower_Hallway1", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Hallway1.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.5f } },

        new() { Name = "CD_Lower_Hallway2", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Hallway2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.5f } },

        new() { Name = "CD_Lower_Hallway3", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Hallway3.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.5f } },

        new() { Name = "CD_Lower_Hallway4", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Hallway4.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.5f } },

        new() { Name = "CD_Lower_Hallway5", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Hallway5.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.5f } },

        new() { Name = "CD_Lower_Room1", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Room1.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Lower_Room2", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Room2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Lower_Room3", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Room3.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Lower_Cell1", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Cell1.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Lower_Cell2", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Cell2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Lower_Cell3", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Cell3.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Lower_Cell4", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Cell4.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Lower_Cell5", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Cell5.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Weight = 1.0f } },

        new() { Name = "CD_Lower_Endcap1", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Endcap1.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Lower_Endcap2", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Endcap2.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Lower_Endcap3", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Endcap3.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Lower_Endcap4", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Endcap4.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },

        new() { Name = "CD_Lower_Endcap5", AssetPath = ForbiddenCatacombsRoomPath + "CD_Lower_Endcap5.prefab",
            Config = new RoomConfig { ThemeName = ForbiddenCatacombsTheme, Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
    };
}
