using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Writes a server-only location's terrain into the zone's compiler as the zone
/// is generated.
///
/// A location's <c>TerrainModifier</c>s live in the half of the template only a
/// client holding it builds, so a stock client stands on ground the site was not
/// placed for. The conversion turns them into the one thing a stock client does
/// receive — a persistent <c>TerrainComp</c>. This is where it is driven from.
///
/// <para><b>Where the placement comes from.</b> Not from a ledger: vanilla
/// already writes one <c>LocationProxy</c> per site, at exactly the position and
/// rotation the location was placed with, and saves it. Reading the proxy makes
/// the placement survive a restart for free and removes any question of the
/// mod's own record disagreeing with the world.</para>
///
/// <para><b>Why per zone, with no queue.</b> A zone's compiler holds a zone's
/// arrays, and a site near a boundary shapes ground in the next zone too. Each
/// zone, when it is generated, looks at the proxies in itself and its eight
/// neighbours and writes its own share of each. That is symmetric: whichever of
/// two neighbouring zones is generated second finds the other's site and writes
/// what it owes. Nothing has to be remembered between zones, and the order zones
/// are generated in — which on a dedicated server is whatever the players do —
/// stops mattering.</para>
///
/// <para>A site reaching further than one zone from its proxy would break that
/// symmetry, so it is reported rather than half-written. Nothing in MWL 5.0.9
/// comes close: the largest smooth radius in the catalogue is 14 m against a
/// 64 m zone.</para>
/// </summary>
public static class LocationTerrainWriter
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    /// <summary>
    /// Forget what was read from templates.
    ///
    /// One process can serve more than one world — a host leaves a world and
    /// joins another — and the cache is keyed by location name, which is not
    /// unique across worlds with different mod configurations. Cleared with the
    /// ZoneSystem, which is made once per world.
    /// </summary>
    internal static void Reset()
    {
        // A new world generates different ground, so heights kept from the
        // builder for the last one describe nowhere.
        LocationTerrainBridge.ForgetGeneratedHeights();
        Verification.TemplateFactsExtractor.ForgetStockSignatures();
        s_pendingWork.Clear();
        s_nextTick = 0f;
        LocationTerrainLedger.Reset();
    }

    /// <summary>Where the terrain stands; see <see cref="LocationTerrainLedger.Status"/>.</summary>
    public static string Status() => LocationTerrainLedger.Status();

    /// <summary>
    /// Convert everything zone <paramref name="zoneID"/> owes, having been
    /// handed the sites near it.
    ///
    /// The discovery of those sites is engine glue and lives in the patch; this
    /// is the dispatch, and it is the thing worth testing: which zones get
    /// written now, which are repaired behind them, what stays outstanding and
    /// what is a failure.
    /// </summary>
    internal static void WriteZone(Vector2s zoneID, Heightmap hmap, List<LocationTerrainPlan.PlacedSite> sites)
    {
        if (sites == null || sites.Count == 0)
        {
            RetryWaiting("zone generation");
            return;
        }
        foreach (LocationTerrainPlan.PlacedSite site in sites)
            Remember(site);

        // 1. What this zone owes, with its own heightmap in hand. This is the
        //    only path that gets the exact generated heights for free.
        Apply(zoneID, hmap, LocationTerrainPlan.For(zoneID, sites));

        // 2. What a NEIGHBOUR owes to a site discovered here. A zone's hook runs
        //    once: a zone generated before this site existed will never look at
        //    it again, so it is repaired now, through its saved compiler, or it
        //    is recorded as outstanding. A zone not yet generated needs nothing —
        //    its own hook will find this proxy, because the proxy is already
        //    saved.
        foreach (LocationTerrainPlan.PlacedSite site in sites)
        {
            foreach (Vector2s other in LocationTerrainReader.ZonesTouched(site.Operations))
            {
                if (other == zoneID || !IsGenerated(other))
                    continue;
                Reconcile(other, LocationTerrainPlan.For(other, new[] { site }));
            }
        }

        // 3. Anything still waiting gets another attempt while a zone is being
        //    generated, which is when compilers come alive and heights get built.
        RetryWaiting("zone generation");
    }

    private static bool IsGenerated(Vector2s zone) =>
        ZoneSystem.instance != null && ZoneSystem.instance.IsZoneGenerated(zone);

    /// <summary>
    /// A validation switch: make one named zone's FIRST write fail, once.
    ///
    /// Proving recovery in a running game needs a failure that happens on
    /// purpose and exactly once. The counter is per zone and is not reset with
    /// the ledger, so "the first write" stays the first even if the world is
    /// regenerated, and the switch strips out with the environment.
    /// </summary>
    private static bool FaultOnce(Vector2s zone)
    {
        if (s_faultZone == null)
        {
            s_faultZone = ValidationSwitches.FaultZoneOnce(out int fx, out int fz, out s_faultTimes)
                ? new Vector2s(fx, fz)
                : (Vector2s?)default;
            if (s_faultZone.HasValue)
                Log.LogWarning(
                    $"{ValidationSwitches.FaultZoneOnceVariable} is set: the first {s_faultTimes} " +
                    $"terrain write(s) for zone {s_faultZone.Value.x},{s_faultZone.Value.y} will be " +
                    "failed on purpose.");
        }
        if (!s_faultZone.HasValue || s_faultZone.Value != zone || s_faultFired >= s_faultTimes)
            return false;
        s_faultFired++;
        return true;
    }

    private static Vector2s? s_faultZone;
    private static int s_faultTimes = 1;
    private static int s_faultFired;

    /// <summary>Convert into a zone whose heightmap is in hand.</summary>
    private static void Apply(Vector2s zoneID, Heightmap hmap, List<LocationTerrainWork> work)
    {
        work = Outstanding(work);
        if (work.Count == 0)
            return;

        LocationTerrainBridge.Readiness readiness =
            LocationTerrainBridge.Acquire(hmap, zoneID, out TerrainComp compiler);
        if (readiness != LocationTerrainBridge.Readiness.Ready)
        {
            foreach (LocationTerrainWork item in work)
                Wait(item, $"the zone's compiler is {readiness}");
            return;
        }

        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(compiler);
        LocationTerrainBridge.HeightOutcome heights = LocationTerrainBridge.ReadGeneratedHeightAt(
            zone, hmap, out TerrainConversion.VertexHeight baseHeight, out string why, out IDisposable hold);
        if (heights != LocationTerrainBridge.HeightOutcome.Read)
        {
            foreach (LocationTerrainWork item in work)
                Postpone(item, heights, why);
            return;
        }
        // The heights are held for exactly as long as this write uses them.
        using (hold)
        {
            List<LocationTerrainWork> converted = Convert(zoneID, zone, baseHeight, work);
            if (converted.Count == 0)
                return;

            if (FaultOnce(zoneID))
            {
                foreach (LocationTerrainWork item in converted)
                    Wait(item, "a validation switch failed this write on purpose");
                return;
            }

            if (LocationTerrainBridge.WriteBack(compiler, zone, out string failure))
                foreach (LocationTerrainWork item in converted)
                    Done(item);
            else
                foreach (LocationTerrainWork item in converted)
                    Wait(item, "the compiler did not save: " + failure);
        }
    }

    /// <summary>
    /// Convert into a zone that is already generated and is not loaded here,
    /// through its saved compiler.
    /// </summary>
    private static void Reconcile(Vector2s zoneID, List<LocationTerrainWork> work)
    {
        work = Outstanding(work);
        if (work.Count == 0)
            return;

        Heightmap live = Heightmap.FindHeightmap(ZoneSystem.GetZonePos(zoneID));
        if (live != null)
        {
            // It is loaded after all; the ordinary path owns those arrays.
            Apply(zoneID, live, work);
            return;
        }

        TerrainZoneDeltas zone = LocationTerrainBridge.AdoptSaved(
            zoneID, ZoneWidth, ZoneScale, out TerrainBlob.Header header, out string problem);
        if (zone == null)
        {
            foreach (LocationTerrainWork item in work)
                Fail(item, "its saved terrain could not be read: " + problem);
            return;
        }
        LocationTerrainBridge.HeightOutcome heights = LocationTerrainBridge.ReadGeneratedHeightAt(
            zone, null, out TerrainConversion.VertexHeight baseHeight, out string why, out IDisposable hold);
        if (heights != LocationTerrainBridge.HeightOutcome.Read)
        {
            foreach (LocationTerrainWork item in work)
                Postpone(item, heights, why);
            return;
        }
        using (hold)
        {
            List<LocationTerrainWork> converted = Convert(zoneID, zone, baseHeight, work);
            if (converted.Count == 0)
                return;

            if (FaultOnce(zoneID))
            {
                foreach (LocationTerrainWork item in converted)
                    Wait(item, "a validation switch failed this write on purpose");
                return;
            }

            if (LocationTerrainBridge.WriteDetached(zoneID, zone, header, out string failure))
            {
                foreach (LocationTerrainWork item in converted)
                    Done(item, " (repaired through its saved compiler)");
            }
            else
            {
                foreach (LocationTerrainWork item in converted)
                    Wait(item, "the saved compiler did not take the write: " + failure);
            }
        }
    }

    /// <summary>
    /// A write that could not be tried: a wait when the ground is not built yet,
    /// a deferral — not an attempt — when this mode's own height budget had no
    /// room for it.
    /// </summary>
    private static void Postpone(LocationTerrainWork item, LocationTerrainBridge.HeightOutcome outcome, string why)
    {
        if (outcome == LocationTerrainBridge.HeightOutcome.Deferred)
            LocationTerrainLedger.Defer(item.SiteId, item.Zone, item.LocationName, why);
        else
            Wait(item, why);
    }

    /// <summary>
    /// Run the conversion for each piece of work into the shared zone. Returns
    /// the ones that changed the zone and therefore need the write to land; a
    /// refusal is recorded as a failure and never as a retry.
    /// </summary>
    private static List<LocationTerrainWork> Convert(
        Vector2s zoneID, TerrainZoneDeltas zone, TerrainConversion.VertexHeight baseHeight,
        List<LocationTerrainWork> work)
    {
        var converted = new List<LocationTerrainWork>();
        foreach (LocationTerrainWork item in work)
        {
            if (TerrainConversion.ApplyOnce(
                    item.SiteId, item.Operations, zone, baseHeight, out TerrainConversionResult result))
            {
                converted.Add(item);
                Log.LogInfo(
                    $"{item.LocationName} in zone {zoneID.x},{zoneID.y}: {result.VerticesChanged} vertices, " +
                    $"{result.TexelsPainted} texels, largest {result.LargestChange:0.00} m" +
                    (result.ContestedVertices.Count > 0
                        ? $", {result.ContestedVertices.Count} vertex/vertices already carried terrain"
                        : ""));
            }
            else if (result.Representable)
            {
                // Already in this zone's arrays: nothing to write, and the
                // ledger should say so rather than leaving it outstanding.
                Done(item, " (already carried by this zone)");
            }
            else
            {
                Fail(item,
                    $"the compiler cannot hold it: {result.BeyondCompilerRange.Count} vertex/vertices " +
                    $"beyond range (first {result.BeyondCompilerRange[0]}). Nothing was written.");
            }
        }
        return converted;
    }

    /// <summary>Work this process has not already finished.</summary>
    private static List<LocationTerrainWork> Outstanding(List<LocationTerrainWork> work) =>
        work.Where(item => !LocationTerrainLedger.IsDone(item.SiteId)).ToList();

    private static void Done(LocationTerrainWork item, string suffix = "")
    {
        LocationTerrainLedger.Record(item.SiteId, item.Zone, item.LocationName,
            LocationTerrainLedger.State.Done, "written" + suffix);
    }

    private static void Wait(LocationTerrainWork item, string reason)
    {
        LocationTerrainLedger.Entry entry = LocationTerrainLedger.Record(
            item.SiteId, item.Zone, item.LocationName, LocationTerrainLedger.State.Waiting, reason);
        if (entry.State == LocationTerrainLedger.State.Failed)
            Log.LogError($"{item.SiteId}: {entry.Reason}");
    }

    private static void Fail(LocationTerrainWork item, string reason)
    {
        LocationTerrainLedger.Record(item.SiteId, item.Zone, item.LocationName,
            LocationTerrainLedger.State.Failed, reason);
        Log.LogError($"{item.LocationName} in zone {item.Zone.x},{item.Zone.y}: {reason}");
    }

    /// <summary>
    /// Retry outstanding conversions on a clock, not only when a zone is
    /// generated.
    ///
    /// Zone generation is driven by where players are. If the LAST zone a site
    /// owes is waiting — its compiler belongs to someone else, or its heights
    /// are not built — and nobody moves far enough to generate another zone,
    /// nothing ever calls back and the site stays half shaped. The injected
    /// failure recovered on 15 Sep only because a neighbouring zone happened to
    /// generate straight afterwards; that is luck, not recovery.
    ///
    /// <paramref name="now"/> is the game's own clock in seconds. Work happens
    /// at most every <see cref="TickSeconds"/> and is bounded per tick, so an
    /// idle server does nothing measurable and a busy one cannot lose a frame
    /// to this.
    /// </summary>
    /// <returns>Whether this call did any work, for tests and the status line.</returns>
    public static bool Tick(float now)
    {
        if (!ServerOnlyMode.Enabled)
            return false;
        if (now < s_nextTick)
            return false;
        s_nextTick = now + ValidationSwitches.TickSeconds(TickSeconds);

        if (LocationTerrainLedger.Waiting().Count == 0)
            return false;
        RetryWaiting("the clock");
        return true;
    }

    /// <summary>How often the retry clock may do anything, in seconds.</summary>
    internal const float TickSeconds = 5f;

    private static float s_nextTick;

    /// <summary>
    /// Take up outstanding work again after a restart.
    ///
    /// The pending set lives in memory and a restart empties it, while the zones
    /// it concerned are already generated and will never run their own hook
    /// again. Without this, a site left half written by a shutdown stays half
    /// written for the life of the world.
    ///
    /// <para>The durable record is the one already on each zone's compiler, so
    /// nothing new is persisted: a (site, zone) is outstanding exactly when the
    /// zone is GENERATED and its saved compiler does not name the site. A zone
    /// that has never been generated is not outstanding — its own hook will find
    /// the proxy, which vanilla saved.</para>
    /// </summary>
    /// <param name="sites">
    /// The placed sites the caller found, which on a restart is every location
    /// proxy of ours in the world. Reading them is a pass over saved data; it
    /// generates nothing.
    /// </param>
    public static int Reseed(IReadOnlyList<LocationTerrainPlan.PlacedSite> sites)
    {
        if (sites == null)
            return 0;

        int outstanding = 0;
        foreach (LocationTerrainPlan.PlacedSite site in sites)
        {
            if (site.Operations == null || site.Operations.Count == 0)
                continue;
            Remember(site);
            foreach (Vector2s zone in LocationTerrainReader.ZonesTouched(site.Operations))
            {
                if (!IsGenerated(zone))
                    continue;
                foreach (LocationTerrainWork item in LocationTerrainPlan.For(zone, new[] { site }))
                {
                    if (CarriedByZone(item))
                    {
                        Done(item, " (already carried by this zone before this process started)");
                        continue;
                    }
                    Wait(item, "outstanding when this process started");
                    outstanding++;
                }
            }
        }
        if (outstanding > 0)
            Log.LogWarning(
                $"{outstanding} terrain conversion(s) were outstanding when this world loaded; " +
                "they will be retried.");
        return outstanding;
    }

    /// <summary>
    /// Whether the zone's saved compiler already names this conversion. This is
    /// the durable record — the same one <c>TerrainConversion.ApplyOnce</c>
    /// consults — so a restart asks the world rather than a memory of its own.
    /// </summary>
    private static bool CarriedByZone(LocationTerrainWork item)
    {
        TerrainZoneDeltas zone = LocationTerrainBridge.AdoptSaved(
            item.Zone, ZoneWidth, ZoneScale, out _, out string problem);
        return problem == null && zone != null && zone.HasApplied(item.SiteId);
    }

    /// <summary>
    /// Give every outstanding conversion another chance. Bounded per call so one
    /// pass cannot spend the frame, and each attempt is counted, so a wait that
    /// will never resolve becomes a reported failure rather than silence.
    /// </summary>
    private static void RetryWaiting(string driver)
    {
        IReadOnlyList<LocationTerrainLedger.Entry> waiting = LocationTerrainLedger.Waiting();
        int before = LocationTerrainLedger.All().Count(e => e.State == LocationTerrainLedger.State.Done);
        int budget = Math.Min(waiting.Count, RetriesPerZone);
        for (int i = 0; i < budget; i++)
        {
            LocationTerrainLedger.Entry entry = waiting[i];
            if (!s_pendingWork.TryGetValue(entry.SiteId, out LocationTerrainWork item))
                continue;
            Heightmap live = Heightmap.FindHeightmap(ZoneSystem.GetZonePos(entry.Zone));
            if (live != null)
                Apply(entry.Zone, live, new List<LocationTerrainWork> { item });
            else if (IsGenerated(entry.Zone))
                Reconcile(entry.Zone, new List<LocationTerrainWork> { item });
        }

        // Which driver finished something is the difference between "recovery
        // happens" and "recovery happens when a player moves". Only said when a
        // retry actually completed work.
        int after = LocationTerrainLedger.All().Count(e => e.State == LocationTerrainLedger.State.Done);
        if (after > before)
            Log.LogInfo($"[RETRY] {after - before} outstanding terrain conversion(s) completed, driven by {driver}.");
    }

    /// <summary>
    /// Keep the work behind every zone a site touches, so a retry has something
    /// to run even when the zone that discovered it is long finished.
    /// </summary>
    private static void Remember(LocationTerrainPlan.PlacedSite site)
    {
        foreach (Vector2s touched in LocationTerrainReader.ZonesTouched(site.Operations))
            foreach (LocationTerrainWork item in LocationTerrainPlan.For(touched, new[] { site }))
                s_pendingWork[item.SiteId] = item;
    }

    /// <summary>How many outstanding conversions one zone generation may retry.</summary>
    private const int RetriesPerZone = 4;

    /// <summary>The zone grid, from the one place that defines it.</summary>
    private const int ZoneWidth = TerrainZoneDeltas.ZoneWidth;
    private const float ZoneScale = TerrainZoneDeltas.ZoneScale;

    /// <summary>The work behind each outstanding ledger entry, so a retry has something to run.</summary>
    private static readonly Dictionary<string, LocationTerrainWork> s_pendingWork =
        new Dictionary<string, LocationTerrainWork>();

}
