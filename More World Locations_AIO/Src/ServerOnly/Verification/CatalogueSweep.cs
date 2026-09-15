using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The engine half of the sweep: find each template, hand it to the extractor,
/// and act on what the audit decides.
///
/// Deliberately thin, like <see cref="LocationTerrainPatch"/>. Everything that
/// decides anything is in <see cref="CatalogueAudit"/>,
/// <see cref="TemplatePolicy"/> and <see cref="ApprovedSelection"/>, all of
/// which are tested without a world. What lives here is the part that cannot
/// be: where a resolved template comes from, and how to take a location back
/// out of the world before it is placed.
/// </summary>
public static class CatalogueSweep
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    private static bool s_swept;

    internal static void Forget()
    {
        s_swept = false;
        CatalogueAudit.Forget();
    }

    /// <summary>
    /// Judge every name the catalogue declares, and take back any registered
    /// location this run does not stand behind.
    ///
    /// Runs once per world, after the locations are set up and before any is
    /// placed.
    /// </summary>
    public static void RunOnce()
    {
        if (s_swept || ZoneSystem.instance == null)
            return;
        s_swept = true;

        CatalogueReport report = CatalogueAudit.Run(
            Subjects(),
            FactsOf,
            VerificationData.StockPrefabs,
            VerificationData.ApprovedSelection,
            ServerOnlyAllowlist.ExcludedPacks);

        Announce(report);
        Withdraw(report);
    }

    /// <summary>
    /// Every name in the catalogue, in the packs' declared order.
    ///
    /// The whole catalogue and not the registered subset: a report that only
    /// covered what was registered could not answer "why is this location
    /// missing", which is the only question anybody brings to it.
    /// </summary>
    private static IEnumerable<CatalogueSubject> Subjects()
    {
        foreach (MWLLocation location in LocationDB.All)
        {
            yield return new CatalogueSubject(
                location.Name,
                PackOf(location.Name),
                sourceDeclared: true,
                interiorPrefabName: location.InteriorPrefabName ?? "",
                dungeonTheme: location.DungeonTheme ?? "");
        }
    }

    /// <summary>
    /// One template's facts, read after Jötunn has resolved it.
    ///
    /// <para>The template is a soft reference and its asset is null until
    /// something loads it. <c>ZoneSystem.SpawnLocation</c> loads it the same
    /// way, and so does the terrain conversion, so this is the game's own path
    /// rather than a second one that could disagree.</para>
    ///
    /// <para>The reference is released afterwards. A sweep over the whole
    /// catalogue holds 190-odd templates otherwise, and a dedicated server that
    /// ran out of memory checking its locations would be a poor trade.</para>
    /// </summary>
    private static TemplateFacts FactsOf(CatalogueSubject subject)
    {
        if (!ZoneSystem.instance.m_locationsByHash.TryGetValue(
                subject.Name.GetStableHashCode(), out ZoneSystem.ZoneLocation location))
        {
            return TemplateFacts.Unreadable(subject.Name, subject.Pack,
                "no ZoneLocation is registered under this exact name, so nothing would be placed for it. " +
                "Names are case-sensitive: a definition and an asset that differ only in capitalisation are two names.");
        }

        bool loadedHere = location.m_prefab.Asset == null;
        if (loadedHere)
            location.m_prefab.Load();

        try
        {
            return TemplateFactsExtractor.Extract(
                subject.Name, subject.Pack, location.m_prefab.Asset,
                subject.InteriorPrefabName, subject.DungeonTheme);
        }
        finally
        {
            // Only what this sweep loaded. Releasing a template the game had
            // already loaded for its own reasons would pull it out from under
            // whatever asked for it.
            if (loadedHere)
                location.m_prefab.Release();
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

    private static void Announce(CatalogueReport report)
    {
        foreach (string line in report.Summary().Split('\n'))
            Log.LogInfo(line);

        foreach (string name in report.ApprovedButNotRegistered())
        {
            // The one case nothing else in a run would mention: the shipped file
            // says this location is in the world and the world does not have it.
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

    /// <summary>
    /// Take back a location the selection approved and this run does not stand
    /// behind.
    ///
    /// <para>The case being guarded against is content that changed under an
    /// approval, so the file is the thing that is wrong and the run is the thing
    /// that is right. Removing it here — before generation — means a world built
    /// from this build never places it. A world that already has it keeps it;
    /// nothing is deleted out of a save, and the log says which sites are
    /// affected instead.</para>
    /// </summary>
    private static void Withdraw(CatalogueReport report)
    {
        foreach (string name in report.ApprovedButNotRegistered())
        {
            int hash = name.GetStableHashCode();
            if (!ZoneSystem.instance.m_locationsByHash.TryGetValue(hash, out ZoneSystem.ZoneLocation location))
                continue;

            ZoneSystem.instance.m_locationsByHash.Remove(hash);
            ZoneSystem.instance.m_locations.Remove(location);
            ServerOnlySelection.Forget(name);
            Log.LogWarning(
                $"{name} was registered and has been withdrawn before any was placed. " +
                "A world already carrying it keeps what it has: nothing is removed from a save.");
        }
    }
}

/// <summary>
/// Where the sweep is driven from.
///
/// <para><c>ZoneSystem.SetupLocations</c> is the game's own "the location list
/// is now complete" moment, and it is where Jötunn adds MWL's locations as
/// well, so the sweep declares that it runs after Jötunn rather than hoping. It
/// is before <c>GenerateLocationsIfNeeded</c>, which is what makes withdrawing
/// a location mean something.</para>
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
            CatalogueSweep.RunOnce();
        }
        catch (Exception ex)
        {
            // An audit that throws must not become an empty world that looks
            // successful. Nothing is withdrawn, the failure is loud, and the
            // shipped selection stands as it was.
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"The server-only catalogue sweep failed and nothing was withdrawn: {ex}");
        }
    }
}

/// <summary>A new world sweeps again: the templates are reloaded and may resolve differently.</summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Awake))]
public static class CatalogueSweepReset
{
    private static void Prefix() => CatalogueSweep.Forget();
}
