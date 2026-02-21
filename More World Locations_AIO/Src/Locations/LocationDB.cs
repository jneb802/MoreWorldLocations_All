using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Managers;

namespace More_World_Locations_AIO;

public static class LocationDB
{
    public static readonly MWLLocation[] All;

    private static readonly Dictionary<string, MWLLocation> _byName;

    static LocationDB()
    {
        All = LocationDefinitions.Meadows
            .Concat(LocationDefinitions.BlackForest)
            .Concat(LocationDefinitions.Swamp)
            .Concat(LocationDefinitions.Mountains)
            .Concat(LocationDefinitions.Plains)
            .Concat(LocationDefinitions.Mistlands)
            .Concat(LocationDefinitions.Ashlands)
            .Concat(LocationDefinitions.Ports)
            .Concat(LocationDefinitions.Traders)
            .Concat(LocationDefinitions.Trainers)
            .ToArray();

        _byName = All.ToDictionary(l => l.Name);
    }

    public static void RegisterAll()
    {
        Register(LocationDefinitions.Meadows);
        Register(LocationDefinitions.BlackForest);
        Register(LocationDefinitions.Swamp);
        Register(LocationDefinitions.Mountains);
        Register(LocationDefinitions.Plains);
        Register(LocationDefinitions.Mistlands);
        Register(LocationDefinitions.Ashlands);

        if (PortInit.EnablePortLocations.Value != PortInit.Toggle.Off)
            Register(LocationDefinitions.Ports);

        if (BepinexConfigs.EnableTraders.Value == PortInit.Toggle.On)
            Register(LocationDefinitions.Traders);

        if (BepinexConfigs.EnableTrainers.Value == PortInit.Toggle.On)
            Register(LocationDefinitions.Trainers);

        ZoneManager.OnVanillaLocationsAvailable -= RegisterAll;
    }

    public static MWLLocation GetLocation(string name)
    {
        return _byName.TryGetValue(name, out var loc) ? loc : null;
    }

    public static LocationConfig GetLocationConfig(string name)
    {
        return _byName.TryGetValue(name, out var loc) ? loc.Config : null;
    }

    public static string[] GetAllAssetPaths()
    {
        return All.Select(l => l.AssetPath).ToArray();
    }

    public static string[] GetLocationNames(Heightmap.Biome biome)
    {
        return All
            .Where(l => l.Config.Biome == biome)
            .Select(l => l.Name)
            .ToArray();
    }

    private static void Register(MWLLocation[] pack)
    {
        foreach (var loc in pack)
            loc.Register();
    }
}
