using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Managers;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;

namespace More_World_Locations_AIO;

public static class LocationDB
{
    public static readonly MWLLocation[] All;

    private static readonly Dictionary<string, MWLLocation> _byName;

    /// <summary>What this process treats as approved (see <see cref="ServerOnlySelection"/>).</summary>
    private static HashSet<string> _approved = new HashSet<string>();

    /// <summary>The names actually registered, so the run can say so and name what it did not.</summary>
    private static readonly HashSet<string> _registered = new HashSet<string>();

    /// <summary>
    /// What the audit will let this world register. Null when the audit did not
    /// complete, which registers nothing rather than falling back to a list
    /// nobody checked this run.
    /// </summary>
    private static HashSet<string>? _auditApproved;

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
        _auditApproved = null;

        string? notice = ServerOnlySelection.ValidationNotice(requested);
        if (notice != null)
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(notice);

        // The audit runs BEFORE anything is registered, and opens every template
        // whether or not it is a candidate. That order is the whole point: while
        // the audit found its templates through the registered list, a template
        // had to be approved before it could be inspected, so the tool could
        // only ever confirm what it already approved. Iterating the catalogue
        // changed the number of rows in the report and not the set of templates
        // actually opened.
        if (ServerOnlyMode.Enabled)
            _auditApproved = AuditBeforeRegistering(requested);

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

    /// <summary>
    /// Judge the catalogue and return the names that may be registered, or null
    /// when the audit could not be completed.
    ///
    /// <para>Null is not "register the shipped selection". It is the case where
    /// the check did not happen, and <see cref="Register"/> registers nothing at
    /// all: an empty world with a loud reason is the right direction for a
    /// verification failure to fail. A validation run's requested names are the
    /// one exception, because somebody is deliberately about to watch them.</para>
    /// </summary>
    private static HashSet<string>? AuditBeforeRegistering(IReadOnlyCollection<string> requested)
    {
        try
        {
            CatalogueReport report = CatalogueSweep.Audit();
            var approved = new HashSet<string>(requested);
            foreach (CatalogueEntry entry in report.Entries)
            {
                if (entry.Registered)
                    approved.Add(entry.Name);
            }
            return approved;
        }
        catch (System.Exception ex)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"The server-only catalogue audit failed, so nothing is registered: {ex}");
            return null;
        }
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
        // Server-only mode registers what THIS RUN's audit approved, and nothing
        // else. The filter runs here, before Register, because turning features
        // off afterwards does not describe a vanilla subset: this method
        // registers the Dungeons pack whatever the port and trader toggles say.
        //
        // _auditApproved rather than _approved: the shipped selection records
        // that an audit passed once, and this run's audit is what decides now.
        // Null means the audit did not complete, and nothing is registered.
        IEnumerable<MWLLocation> registering = ServerOnlyMode.Enabled
            ? (_auditApproved == null
                ? new MWLLocation[0]
                : ServerOnlyAllowlist.Filter(packName, pack, _auditApproved))
            : pack;

        foreach (MWLLocation loc in registering)
        {
            loc.Register();
            _registered.Add(loc.Name);
        }
    }
}
