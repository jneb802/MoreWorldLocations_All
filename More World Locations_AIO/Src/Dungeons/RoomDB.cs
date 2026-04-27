using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Managers;

namespace More_World_Locations_AIO.Dungeons;

public static class RoomDB
{
    public static readonly MWLRoom[] All;

    private static readonly Dictionary<string, MWLRoom> _byName;

    static RoomDB()
    {
        All = RoomDefinitions.UndergroundRuins
            .Concat(RoomDefinitions.ForbiddenCatacombs)
            .ToArray();

        _byName = All.ToDictionary(r => r.Name);
    }

    public static void RegisterAll()
    {
        Register(RoomDefinitions.UndergroundRuins);
        Register(RoomDefinitions.ForbiddenCatacombs);

        DungeonManager.OnVanillaRoomsAvailable -= RegisterAll;
    }

    public static MWLRoom GetRoom(string name)
    {
        return _byName.TryGetValue(name, out MWLRoom room) ? room : null;
    }

    public static RoomConfig GetRoomConfig(string name)
    {
        return _byName.TryGetValue(name, out MWLRoom room) ? room.Config : null;
    }

    public static string[] GetRoomNames(string themeName)
    {
        return All
            .Where(r => r.Config.ThemeName == themeName)
            .Select(r => r.Name)
            .ToArray();
    }
    
    public static string[] GetAllAssetPaths()
    {
        return All.Select(l => l.AssetPath).ToArray();
    }

    private static void Register(MWLRoom[] rooms)
    {
        foreach (MWLRoom room in rooms)
            room.Register();
    }
}
