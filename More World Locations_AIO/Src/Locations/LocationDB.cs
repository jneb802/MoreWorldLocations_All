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

    /// <summary>What this process treats as approved (see <see cref="ServerOnlySelection"/>).</summary>
    private static HashSet<string> _approved = new HashSet<string>();

    /// <summary>The names actually registered, so the run can say so and name what it did not.</summary>
    private static readonly HashSet<string> _registered = new HashSet<string>();

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
        IReadOnlyCollection<string> requested = ServerOnlyMode.Enabled
            ? ValidationSwitches.ApprovedForValidation()
            : new HashSet<string>();
        _approved = ServerOnlySelection.Compose(
            ServerOnlyAllowlist.Approved, requested, ServerOnlyMode.Enabled);
        _registered.Clear();

        string? notice = ServerOnlySelection.ValidationNotice(requested);
        if (notice != null)
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(notice);

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

        if (ServerOnlyMode.Enabled)
        {
            // The runtime has to be able to tell an MWL location from a vanilla
            // one: the terrain conversion applies to ours and must not touch
            // theirs, which a stock client builds for itself.
            ServerOnlySelection.SetRegistered(_registered);

            var logger = More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;
            logger.LogInfo(ServerOnlySelection.RegisteredNotice(_registered));
            foreach (string name in ServerOnlySelection.Unmatched(_approved, _registered))
                logger.LogWarning(
                    $"Approved template '{name}' matched no location: it is either misspelled " +
                    "or in a pack server-only mode excludes. Nothing was registered for it.");
        }

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
            ? ServerOnlyAllowlist.Filter(packName, pack, _approved)
            : pack;

        foreach (MWLLocation loc in registering)
        {
            loc.Register();
            _registered.Add(loc.Name);
        }
    }
}
