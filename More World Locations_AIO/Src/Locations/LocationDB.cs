using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Managers;
using More_World_Locations_AIO.ServerOnly;

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
            .Concat(LocationDefinitions.Dungeons)
            .ToArray();

        _byName = All.ToDictionary(l => l.Name);
    }

    public static void RegisterAll()
    {
        Register("Meadows", LocationDefinitions.Meadows);
        Register("BlackForest", LocationDefinitions.BlackForest);
        Register("Swamp", LocationDefinitions.Swamp);
        Register("Mountains", LocationDefinitions.Mountains);
        Register("Plains", LocationDefinitions.Plains);
        Register("Mistlands", LocationDefinitions.Mistlands);
        Register("Ashlands", LocationDefinitions.Ashlands);
        Register("Dungeons", LocationDefinitions.Dungeons);

        if (PortInit.EnablePortLocations.Value != PortInit.Toggle.Off)
            Register("Ports", LocationDefinitions.Ports);

        if (BepinexConfigs.EnableTraders.Value != PortInit.Toggle.Off)
            Register("Traders", LocationDefinitions.Traders);

        if (BepinexConfigs.EnableTrainers.Value != PortInit.Toggle.Off)
            Register("Trainers", LocationDefinitions.Trainers);

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

    private static void Register(string packName, MWLLocation[] pack)
    {
        // Server-only mode registers the audited subset and nothing else. The
        // filter runs here, before Register, because turning features off
        // afterwards does not describe a vanilla subset: this method registers
        // the Dungeons pack whatever the port and trader toggles say.
        IEnumerable<MWLLocation> registering = ServerOnlyMode.Enabled
            ? ServerOnlyAllowlist.Filter(packName, pack)
            : pack;

        foreach (MWLLocation loc in registering)
            loc.Register();
    }
}
