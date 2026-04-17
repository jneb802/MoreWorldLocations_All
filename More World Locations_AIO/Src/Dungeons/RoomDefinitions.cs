using Jotunn.Configs;

namespace More_World_Locations_AIO.Dungeons;

public static class RoomDefinitions
{
    public static readonly MWLRoom[] UndergroundRuins =
    {
        
        new() { Name = "BFD_MainEntrance2", AssetPath = "Assets/WarpProjects/Dungeons/UndergroundRuins/Rooms/BFD_MainEntrance2.prefab",
            Config = new RoomConfig { ThemeName = "Underground Ruins", Entrance = true, Weight = 1.0f } },

        new() { Name = "BFD_Modular3", AssetPath = "Assets/WarpProjects/Dungeons/UndergroundRuins/Rooms/BFD_Modular3.prefab",
            Config = new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
    };
}
