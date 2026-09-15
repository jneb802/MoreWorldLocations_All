using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The engine half of the audit: open each template, hand it to the extractor,
/// and make sure the world ends up holding exactly what the audit approved.
///
/// <para>Thin on purpose, like <see cref="LocationTerrainPatch"/>. Everything
/// that decides anything is in <see cref="CatalogueAudit"/>,
/// <see cref="TemplatePolicy"/> and <see cref="ApprovedSelection"/>, all tested
/// without a world. What lives here is the part that cannot be: where a
/// template comes from, and how to take a location out of the world before it
/// is placed.</para>
///
/// <para><b>Two steps, and the order matters.</b> The audit runs at
/// registration, before any location is added, so an unapproved template is
/// never registered in the first place. The sweep then runs again when the
/// game's location list is complete and withdraws anything the audit did not
/// approve — because registration and the world's list are two different things
/// and only the second one places buildings.</para>
/// </summary>
public static class CatalogueSweep
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    internal static void Forget() => CatalogueAudit.Forget();

    /// <summary>
    /// Judge the whole catalogue, and answer which names may be registered.
    ///
    /// Called by <c>LocationDB.RegisterAll</c> before it registers anything.
    /// Nothing here consults the registered set, which is the point: a template
    /// has to be openable without first being approved, or the audit can only
    /// ever confirm what it already approves.
    /// </summary>
    public static CatalogueReport Audit()
    {
        CatalogueAudit.Progress = line => Log.LogInfo(line);
        Observe(() => SweepStarted?.Invoke());
        CatalogueReport report;
        try
        {
            report = CatalogueAudit.Run(
                Subjects(),
                FactsOf,
                VerificationData.StockPrefabs,
                VerificationData.ApprovedSelection,
                ServerOnlyAllowlist.ExcludedPacks);
        }
        finally
        {
            // The signature cache is the AUDIT's, not the world's: it exists to
            // avoid re-walking a stock prefab within one sweep and has no reader
            // afterwards. Cleared however the sweep ended — finished, cancelled
            // or thrown — because "we did not get to the end" is exactly when
            // nobody is left to clear it.
            TemplateFactsExtractor.ForgetStockSignatures();
            CatalogueAudit.Progress = null;
        }

        // Said before registration rather than after, so that a world that comes
        // up wrong has its explanation above the symptom in the log.
        Announce(report);
        Observe(() => SweepFinished?.Invoke());
        return report;
    }

    /// <summary>
    /// Audit if this world has not been audited yet, then make its location list
    /// agree with the result.
    ///
    /// The two steps are separate in the ordinary path — the audit runs at
    /// registration, before anything is registered, and enforcement runs when
    /// the game's location list is complete — and this is for a caller that
    /// wants both and does not care which has happened.
    /// </summary>
    public static bool RunOnce()
    {
        if (CatalogueAudit.Report == null)
            Audit();
        return Enforce();
    }

    /// <summary>
    /// Make the world's location list agree with the audit, once the list is
    /// complete.
    ///
    /// <para>Returns false when it could not, which is the case that used to
    /// silently approve everything: the state saying "already swept" was set
    /// before the work, and an exception then left the unverified names exactly
    /// where they were.</para>
    /// </summary>
    public static bool Enforce()
    {
        // No "already done" flag. It existed to avoid repeating the work, and
        // the work is a dictionary lookup per location; what it actually bought
        // was a second pass that silently did nothing, so a location added after
        // registration would never be looked at again. Running twice is free and
        // catching a late addition is not.
        if (ZoneSystem.instance == null)
            return false;

        CatalogueReport? report = CatalogueAudit.Report;
        if (report == null)
        {
            // Nothing judged this world yet. Withdraw whatever is actually
            // there, which is the honest action, and do NOT mark enforcement
            // done: registration may still be to come. A station run measured
            // this hook firing at the main menu, before RegisterAll had run at
            // all, where "the audit never happened" was true and meant nothing.
            return WithdrawAll("no audit has judged this world yet") == 0;
        }

        var approved = new HashSet<string>(StringComparer.Ordinal);
        foreach (CatalogueEntry entry in report.Entries)
        {
            if (entry.Registered)
                approved.Add(entry.Name);
        }

        foreach (MWLLocation location in LocationDB.All)
        {
            if (approved.Contains(location.Name))
                continue;
            if (Withdraw(location.Name))
            {
                CatalogueEntry? entry = report.Find(location.Name);
                Log.LogWarning(
                    $"{location.Name} was in the world's location list and the audit does not stand behind it; " +
                    "it has been withdrawn before anything was placed. " +
                    (entry == null ? "It is not in the catalogue report at all." : entry.Decision.Reason) +
                    " A world already carrying it keeps what it has: nothing is removed from a save.");
            }
        }

        return true;
    }

    /// <summary>
    /// What this mode is holding, in the terms a memory run is judged in.
    ///
    /// One line, from the process itself, so that an external sampler's private
    /// bytes and this mode's own accounting can be read against each other. A
    /// sampler alone cannot say which owner grew; this alone cannot see the
    /// allocator.
    /// </summary>
    public static string MemoryStatus() =>
        $"leases held {TemplateAssets.OutstandingLeases} (peak {TemplateAssets.PeakLeases}); " +
        $"signatures {TemplateFactsExtractor.StockSignatureBytes / 1024} KiB in " +
        $"{TemplateFactsExtractor.StockSignatureEntries} entr(ies) of " +
        $"{TemplateFactsExtractor.StockSignatureBudgetBytes / 1024 / 1024} MiB; " +
        $"heights {LocationTerrainBridge.GeneratedHeightBytes / 1024} KiB in " +
        $"{LocationTerrainBridge.GeneratedHeightEntries} zone(s) " +
        $"({LocationTerrainBridge.GeneratedHeightPins} in use, " +
        $"{LocationTerrainBridge.GeneratedHeightAdopted} adopted outside the table) of " +
        $"{LocationTerrainBridge.GeneratedHeightBudgetBytes / 1024 / 1024} MiB; " +
        $"managed heap {System.GC.GetTotalMemory(false) / 1024 / 1024} MiB; " +
        MockReferenceGuard.Status();

    /// <summary>
    /// Judge the catalogue again in this process.
    ///
    /// <para>Only a measurement uses it. A sweep runs once per world, so the
    /// question "does the second one retain more than the first" cannot be asked
    /// at all without a way to ask for another — and restarting between sweeps
    /// is exactly what would hide an owner that grows.</para>
    /// </summary>
    public static CatalogueReport Resweep()
    {
        CatalogueReport report = Audit();
        Enforce();
        return report;
    }

    /// <summary>
    /// Every name a report has to account for: the catalogue's known names and
    /// every definition this build declares, reconciled by identity.
    ///
    /// <para>The union, not either one. A name in the list with no definition is
    /// an asset nothing places, which is a real answer somebody needs; a
    /// definition the list has never heard of is a build added since, which has
    /// to be judged and not skipped.</para>
    /// </summary>
    internal static IEnumerable<CatalogueSubject> Subjects()
    {
        var byName = new Dictionary<string, MWLLocation>(StringComparer.Ordinal);
        foreach (MWLLocation location in LocationDB.All)
            byName[location.Name] = location;

        var emitted = new HashSet<string>(StringComparer.Ordinal);

        // The definitions first, in the packs' declared order, so that the
        // report's order matches the order registration walks.
        foreach (MWLLocation location in LocationDB.All)
        {
            emitted.Add(location.Name);
            yield return new CatalogueSubject(
                location.Name,
                PackOf(location.Name),
                sourceDeclared: true,
                interiorPrefabName: location.InteriorPrefabName ?? "",
                dungeonTheme: location.DungeonTheme ?? "");
        }

        // Then the names the catalogue knows about that no definition declares.
        foreach (CatalogueName known in VerificationData.CatalogueNames.All)
        {
            if (!emitted.Add(known.Name))
                continue;
            yield return new CatalogueSubject(known.Name, known.Pack, sourceDeclared: false);
        }
    }

    /// <summary>
    /// One template's facts, read after Jötunn has resolved it and compared
    /// against the stock prefabs the client would build.
    ///
    /// The handle is released as soon as the walk is done; see
    /// <see cref="ITemplateHandle"/>.
    /// </summary>
    private static TemplateFacts FactsOf(CatalogueSubject subject)
    {
        if (!TemplateAssets.CanLoad)
        {
            return TemplateFacts.Unreadable(subject.Name, subject.Pack,
                "this process has no way to load templates, so nothing was inspected. " +
                "Every verdict in this report is unresolved for that reason and none of them is about a template.");
        }

        TemplateFacts facts;
        using (ITemplateHandle? handle = TemplateAssets.Open(subject.Name))
        {
            if (handle?.Asset == null)
            {
                return TemplateFacts.Unreadable(subject.Name, subject.Pack,
                    "no asset loaded under this exact name. Names are case-sensitive: a definition and an asset that " +
                    "differ only in capitalisation are two names, and one of them places nothing.");
            }

            facts = TemplateFactsExtractor.Extract(
                subject.Name, subject.Pack, handle.Asset,
                subject.InteriorPrefabName, subject.DungeonTheme,
                TemplateAssets.StockPrefabs, TemplateAssets.BaselineProvenance);
            Observe(() => TemplateRead?.Invoke(subject.Name, handle.Asset));
        }
        Observe(() => TemplateReleased?.Invoke(subject.Name));
        return facts;
    }

    /// <summary>
    /// Where a lifecycle trace watches the sweep from. Set only by a validation
    /// switch; null in a shipped run.
    ///
    /// <para>The three moments a trace needs and cannot get from outside: a
    /// template while the sweep still holds it, the same template the instant
    /// it has been given back, and the end of the sweep. They observe and never
    /// decide: an observer that throws is swallowed the same way a log sink that
    /// throws is, because R4 was exactly a caller that only wanted a message
    /// deciding what the world contains.</para>
    /// </summary>
    public static Action<string, GameObject?>? TemplateRead { get; set; }

    /// <summary>See <see cref="TemplateRead"/>.</summary>
    public static Action<string>? TemplateReleased { get; set; }

    /// <summary>See <see cref="TemplateRead"/>.</summary>
    public static Action? SweepFinished { get; set; }

    /// <summary>See <see cref="TemplateRead"/>: before the first template is opened.</summary>
    public static Action? SweepStarted { get; set; }

    private static void Observe(Action observer)
    {
        try
        {
            observer();
        }
        catch (Exception ex)
        {
            try
            {
                Log.LogWarning($"a lifecycle observer threw and was ignored: {ex.GetType().Name}: {ex.Message}");
            }
            catch
            {
                // The observer is not allowed to change anything, including by failing.
            }
        }
    }

    /// <summary>Which pack a name came from, for scope exclusion.</summary>
    private static string PackOf(string name)
    {
        foreach (KeyValuePair<string, MWLLocation[]> pack in Packs)
        {
            foreach (MWLLocation location in pack.Value)
            {
                if (string.Equals(location.Name, name, StringComparison.Ordinal))
                    return pack.Key;
            }
        }
        return "";
    }

    private static readonly KeyValuePair<string, MWLLocation[]>[] Packs =
    {
        new KeyValuePair<string, MWLLocation[]>("Meadows", LocationDefinitions.Meadows),
        new KeyValuePair<string, MWLLocation[]>("BlackForest", LocationDefinitions.BlackForest),
        new KeyValuePair<string, MWLLocation[]>("Swamp", LocationDefinitions.Swamp),
        new KeyValuePair<string, MWLLocation[]>("Mountains", LocationDefinitions.Mountains),
        new KeyValuePair<string, MWLLocation[]>("Plains", LocationDefinitions.Plains),
        new KeyValuePair<string, MWLLocation[]>("Mistlands", LocationDefinitions.Mistlands),
        new KeyValuePair<string, MWLLocation[]>("Ashlands", LocationDefinitions.Ashlands),
        new KeyValuePair<string, MWLLocation[]>("Ports", LocationDefinitions.Ports),
        new KeyValuePair<string, MWLLocation[]>("Traders", LocationDefinitions.Traders),
        new KeyValuePair<string, MWLLocation[]>("Trainers", LocationDefinitions.Trainers),
        new KeyValuePair<string, MWLLocation[]>("Dungeons", LocationDefinitions.Dungeons),
    };

    /// <summary>
    /// Take one location out of the world's list. True when it was there.
    ///
    /// Nothing is removed from a save: a world that already carries the location
    /// keeps what it has, and this only stops another being placed.
    /// </summary>
    private static bool Withdraw(string name)
    {
        int hash = name.GetStableHashCode();
        if (!ZoneSystem.instance.m_locationsByHash.TryGetValue(hash, out ZoneSystem.ZoneLocation location))
            return false;

        ZoneSystem.instance.m_locationsByHash.Remove(hash);
        ZoneSystem.instance.m_locations.Remove(location);
        ServerOnlySelection.Forget(name);
        return true;
    }

    /// <summary>
    /// Every MWL location out of the world, for the case where the audit did not
    /// happen at all.
    ///
    /// The alternative is falling back to the shipped selection, which is the
    /// very set of unverified names the guard exists to check. An empty world
    /// with a loud reason is the right way for a verification failure to fail.
    /// </summary>
    private static int WithdrawAll(string why)
    {
        int withdrawn = 0;
        foreach (MWLLocation location in LocationDB.All)
        {
            if (Withdraw(location.Name))
                withdrawn++;
        }
        // Loud only when something was actually taken out. Nothing registered is
        // not a failure; it is the ordinary state before registration runs, and
        // an error there teaches an operator to ignore the one that matters.
        if (withdrawn > 0)
        {
            Log.LogError(
                $"Server-only mode withdrew {withdrawn} location(s) from this world: {why}. " +
                "Nothing MWL registers is placed until a sweep has judged it.");
        }
        return withdrawn;
    }

    private static void Announce(CatalogueReport report)
    {
        // Every line is guarded. Reporting is not allowed to decide whether a
        // rejected template stays registered, and an exception on the way out of
        // a log call is exactly how it used to.
        try
        {
            foreach (string line in report.Summary().Split('\n'))
                Log.LogInfo(line);

            foreach (string name in report.ApprovedButNotRegistered())
            {
                // The one case nothing else in a run would mention: the shipped
                // file says this location is in the world and it is not.
                Log.LogWarning($"{name} is approved and was NOT registered: {report.Find(name)!.Decision.Reason}");
            }

            IReadOnlyList<string> unbound = VerificationData.ApprovedSelection.Unbound;
            if (unbound.Count > 0)
            {
                Log.LogWarning(
                    $"{unbound.Count} approval(s) are not bound to the template they were granted for — " +
                    string.Join(", ", new List<string>(unbound).ToArray()) +
                    ". They were watched in game before this build could fingerprint a template; " +
                    "this run's own evaluation is the only check standing behind them.");
            }

            foreach (string problem in VerificationData.LoadProblems)
                Log.LogError($"verification data: {problem}");
        }
        catch (Exception ex)
        {
            try
            {
                Log.LogError($"The catalogue report could not be written out: {ex.GetType().Name}: {ex.Message}");
            }
            catch
            {
                // A logger that cannot log its own failure has nothing left to
                // tell us, and it still must not change what is registered.
            }
        }
    }
}

/// <summary>
/// Where enforcement is driven from.
///
/// <para><c>ZoneSystem.SetupLocations</c> is the game's own "the location list
/// is now complete" moment, and it is where Jötunn adds MWL's locations as
/// well, so this declares that it runs after Jötunn rather than hoping. It is
/// before <c>GenerateLocationsIfNeeded</c>, which is what makes withdrawing a
/// location mean something.</para>
///
/// <para>The audit itself has already run, at registration. This is the step
/// that makes the world agree with it.</para>
/// </summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
[HarmonyAfter("com.jotunn.jotunn")]
public static class CatalogueSweepPatch
{
    private static void Postfix()
    {
        if (!ServerOnlyMode.Enabled)
            return;

        try
        {
            CatalogueSweep.Enforce();
        }
        catch (Exception ex)
        {
            // A verification failure must not become approval. Nothing MWL
            // registered is left in the world, and the failure is loud.
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"Server-only verification failed while enforcing the catalogue: {ex}");
            try
            {
                foreach (MWLLocation location in LocationDB.All)
                {
                    int hash = location.Name.GetStableHashCode();
                    if (ZoneSystem.instance != null
                        && ZoneSystem.instance.m_locationsByHash.TryGetValue(hash, out ZoneSystem.ZoneLocation zone))
                    {
                        ZoneSystem.instance.m_locationsByHash.Remove(hash);
                        ZoneSystem.instance.m_locations.Remove(zone);
                        ServerOnlySelection.Forget(location.Name);
                    }
                }
            }
            catch (Exception second)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                    $"and the locations could not be withdrawn either: {second}");
            }
        }
    }
}

/// <summary>A new world audits again: the templates are reloaded and may resolve differently.</summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Awake))]
public static class CatalogueSweepReset
{
    private static void Prefix() => CatalogueSweep.Forget();
}
