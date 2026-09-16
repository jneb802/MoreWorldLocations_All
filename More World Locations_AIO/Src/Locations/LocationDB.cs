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
        if (ServerOnlyMode.Enabled)
        {
            if (GenerationHold.RefuseFailedProcess())
                return;
            if (CatalogueSweep.State == CatalogueSweep.SweepState.Auditing || CatalogueSweep.RegistrationReady)
                return;
            // A failed attempt may have inserted only part of the list. The
            // world has not loaded yet; remove those definitions before retrying.
            if (ZoneSystem.instance != null)
            {
                foreach (string name in _registered)
                {
                    if (ZoneSystem.instance.m_locationsByHash.TryGetValue(name.GetStableHashCode(), out var prior)
                        && prior.m_prefab.Name == name)
                    {
                        ZoneSystem.instance.m_locationsByHash.Remove(name.GetStableHashCode());
                        ZoneSystem.instance.m_locations.Remove(prior);
                    }
                }
            }
        }
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
        //
        // It runs a name per frame and registers when it concludes; until then
        // nothing of ours is in the world's list and the engine holds the
        // world's location generation. With no frames to give (a test) it
        // concludes inside this call.
        if (ServerOnlyMode.Enabled)
        {
            ZoneManager.OnVanillaLocationsAvailable -= RegisterAll;
            if (!CatalogueSweep.BeginAudit(report => RegisterWhatTheAuditApproved(report, requested)))
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                    "A catalogue sweep is already running, so registration was not started; nothing is registered.");
            }
            return;
        }

        RegisterPacks();
        ZoneManager.OnVanillaLocationsAvailable -= RegisterAll;
    }

    /// <summary>
    /// The engine's late registration, for a location added after Jötunn has
    /// already injected its list into the world. Null where the doubles'
    /// Register puts a location straight into the world's list.
    /// </summary>
    public static System.Action<string>? LateRegistration { get; set; }

    private static void RegisterWhatTheAuditApproved(CatalogueReport? report, IReadOnlyCollection<string> requested)
    {
        if (report == null)
            throw new System.InvalidOperationException("No complete catalogue report is available for registration.");
        _auditApproved = ApprovedBy(report, requested);
        RegisterPacks();

        var logger = More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;
        logger.LogInfo(ServerOnlySelection.RegisteredNotice(_registered));
        foreach (string name in ServerOnlySelection.Unmatched(_approved, _registered))
            logger.LogWarning(
                $"Approved template '{name}' matched no location: it is either misspelled " +
                "or in a pack server-only mode excludes. Nothing was registered for it.");

        // Jötunn injected its list when the sweep started, which was empty;
        // what was registered since has to be put into the world by hand.
        foreach (string name in _registered)
        {
            try
            {
                LateRegistration?.Invoke(name);
                // In production the named definition is now in the world map;
                // fail here to exercise cleanup of partially registered worlds.
                StartupFaults.Current.AfterRegistration(name, message => logger.LogWarning(message));
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"'{name}' was approved and could not be put into the world: {ex.Message}", ex);
            }
        }

        // Enforcement closes the transaction HERE, where both halves have
        // provably happened. It used to sit on a ZoneSystem.SetupLocations
        // postfix, and a station run measured that hook running at the main
        // menu, before this method had been called at all -- so it found no
        // audit, withdrew the nothing that was registered, and reported a
        // failure that had not happened. A guard whose ordering is a guess
        // is a guard that reports on a world it has not seen.
        if (!CatalogueSweep.Enforce(report))
            throw new System.InvalidOperationException("The catalogue could not be enforced in the world's location map.");
        foreach (string name in _registered)
        {
            if (!ZoneSystem.instance.m_locationsByHash.TryGetValue(name.GetStableHashCode(), out var location)
                || location.m_prefab.Name != name
                || !ZoneSystem.instance.m_locations.Contains(location))
                throw new System.InvalidOperationException($"'{name}' was approved but is missing from the world's location map.");
        }
        ServerOnlySelection.SetRegistered(_registered);
    }

    private static void RegisterPacks()
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
    }

    /// <summary>The names a report lets this world register, or null when there is no report.</summary>
    private static HashSet<string>? ApprovedBy(CatalogueReport? report, IReadOnlyCollection<string> requested)
    {
        if (report == null)
            return null;
        var approved = new HashSet<string>(requested);
        foreach (CatalogueEntry entry in report.Entries)
        {
            if (entry.Registered)
                approved.Add(entry.Name);
        }
        return approved;
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
