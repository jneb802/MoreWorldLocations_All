using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// The last thing asked before a site's objects exist: can this ground be
/// served?
///
/// <para><b>The gap this closes.</b> The conversion runs from a postfix on
/// <c>PlaceLocations</c>, and finds its sites by reading location proxies —
/// which exist only once <c>SpawnLocation</c> has run and every networked child
/// already has a ZDO. So every refusal it could make came AFTER the buildings
/// were published, and a site the conversion would not serve stood on the
/// ground the client generates for itself: a hall half sunk into a hillside, a
/// stairway ending in mid air. A faithful site or no site.</para>
///
/// <para><b>What it is not.</b> Not a second conversion, and not a template
/// check. It runs the real <see cref="TerrainConversion"/> over a scratch copy
/// so that "can this be served" and "what does serving it produce" cannot
/// disagree, and it asks only about THIS placement —
/// <c>TemplatePolicy</c> already asked whether the build is serveable
/// anywhere.</para>
/// </summary>
public static class LocationSpawnGate
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    /// <summary>Placements refused, by site identity and reason, for the operator command.</summary>
    private static readonly Dictionary<string, SiteDecision> s_refused = new();

    /// <summary>Placements held because nothing could be established about them yet.</summary>
    private static readonly Dictionary<string, SiteDecision> s_held = new();

    internal static void Forget()
    {
        s_refused.Clear();
        s_held.Clear();
    }

    /// <summary>What the gate has refused and what it could not check, for <c>mwl_terrain</c>.</summary>
    public static string Status()
    {
        int waiting = ZoneReadinessBarrier.Holds.Count;
        int deferred = ZoneReadinessBarrier.Deferred.Count;
        if (s_refused.Count == 0 && s_held.Count == 0 && waiting == 0 && deferred == 0)
            return "No placement has been refused or held; every one was checked before it was placed.";

        var text = new System.Text.StringBuilder();
        text.Append(s_refused.Count).Append(" placement(s) refused before anything was placed:\n");
        foreach (KeyValuePair<string, SiteDecision> entry in Sorted(s_refused))
            text.Append("  ").Append(entry.Key).Append(" — ").Append(entry.Value.Code).Append(": ")
                .Append(entry.Value.Reason).Append('\n');

        if (s_held.Count > 0)
        {
            text.Append(s_held.Count).Append(" placement(s) held, waiting for ground that can be read:\n");
            foreach (KeyValuePair<string, SiteDecision> entry in Sorted(s_held))
                text.Append("  ").Append(entry.Key).Append(" — ").Append(entry.Value.Code).Append(": ")
                    .Append(entry.Value.Reason).Append('\n');
        }
        if (waiting > 0)
        {
            text.Append(waiting).Append(" zone(s) not generated yet, waiting for ground that can be read:\n");
            foreach (KeyValuePair<Vector2s, int> hold in ZoneReadinessBarrier.Holds)
                text.Append("  zone ").Append(hold.Key.x).Append(',').Append(hold.Key.y)
                    .Append(" — held ").Append(hold.Value).Append(" of ")
                    .Append(ZoneReadinessBarrier.MaxHolds).Append(" attempt(s)\n");
        }
        if (deferred > 0)
        {
            text.Append(deferred).Append(" zone(s) waiting on the height budget, not counted against readiness:\n");
            foreach (Vector2s zone in ZoneReadinessBarrier.Deferred)
                text.Append("  zone ").Append(zone.x).Append(',').Append(zone.y).Append('\n');
        }
        return text.ToString().TrimEnd('\n');
    }

    private static List<KeyValuePair<string, SiteDecision>> Sorted(Dictionary<string, SiteDecision> entries)
    {
        var sorted = new List<KeyValuePair<string, SiteDecision>>(entries);
        sorted.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        return sorted;
    }

    /// <summary>
    /// Whether this placement may be published.
    ///
    /// Anything not ours, and any location with no terrain of its own, passes
    /// without a question being asked: vanilla's own locations shape ground the
    /// client already builds, and converting those would sink every dolmen in
    /// the world a second time.
    /// </summary>
    internal static bool MayPublish(ZoneSystem.ZoneLocation location, Vector3 position, Quaternion rotation)
    {
        string name = location?.m_prefabName;
        if (string.IsNullOrEmpty(name) || !ServerOnlySelection.IsOurs(name))
            return true;

        Vector2s home = ZoneSystem.GetZone(position);
        string siteId = LocationTerrainReader.SiteId(name, position, home);

        // A placement the readiness barrier gave up on stays refused, whatever a
        // fresh look would now say: it was held as long as the bound allows, and
        // publishing it here would undo that.
        if (s_refused.TryGetValue(siteId, out SiteDecision already))
            return already.MayPublish;

        List<LocationTerrainOperation> operations =
            OperationsAt(location, name, position, rotation, out string unreadable);
        if (unreadable != null)
        {
            // Not "no terrain". A template that would not load says nothing
            // about what it does to the ground, and treating silence as "shapes
            // nothing" publishes exactly the site this check exists to hold.
            return Record(siteId, name, position, new SiteDecision(
                SiteVerdict.Undecided, SiteRefusalCodes.TemplateUnreadable, unreadable));
        }
        if (operations.Count == 0)
            return true;

        // Every zone this decision reads is pinned for as long as the decision
        // lasts. The builder hands its answer over once, so a site that touches
        // four zones must not have its first zone evicted by its fourth while it
        // is still deciding about the site as a whole.
        var pinned = new List<IDisposable>();
        try
        {
            s_pinning = pinned;
            return Record(siteId, name, position, SitePreflight.Decide(siteId, home, operations, GroundOf));
        }
        finally
        {
            s_pinning = null;
            foreach (IDisposable pin in pinned)
                pin.Dispose();
        }
    }

    /// <summary>
    /// Where a decision in progress collects its height pins. Null outside one,
    /// which makes GroundOf usable from anywhere without pinning by accident.
    /// </summary>
    [ThreadStatic]
    private static List<IDisposable>? s_pinning;

    /// <summary>
    /// Write the decision down and answer it. Nothing here decides anything; it
    /// exists so that every path out of the gate is on the record — a site that
    /// was never built appears in no ledger of work, and an operator would
    /// otherwise see a gap in the map with nothing anywhere saying why.
    /// </summary>
    private static bool Record(string siteId, string name, Vector3 position, SiteDecision decision)
    {
        if (decision.MayPublish)
            return true;

        Dictionary<string, SiteDecision> into =
            decision.Verdict == SiteVerdict.Refuse ? s_refused : s_held;
        if (!into.ContainsKey(siteId))
        {
            into[siteId] = decision;
            Log.LogWarning(
                $"{name} at {position.x:0},{position.z:0} was NOT placed ({decision.Code}): {decision.Reason} " +
                "Nothing of it exists, which is the point — a site whose ground cannot be shaped must not " +
                "leave its buildings behind.");
        }
        return false;
    }

    /// <summary>Give up on a placement permanently, so the zone stops being held for it.</summary>
    internal static void GiveUp(string siteId, SiteDecision decision)
    {
        s_held.Remove(siteId);
        if (!s_refused.ContainsKey(siteId))
            s_refused[siteId] = decision;
    }

    /// <summary>Whether this placement has already been given up on.</summary>
    internal static bool IsRefused(string siteId) => s_refused.ContainsKey(siteId);

    /// <summary>Record a check that threw, so a withheld site still has a reason attached.</summary>
    internal static void RecordCheckFailure(ZoneSystem.ZoneLocation location, Vector3 position, Exception ex)
    {
        string name = location?.m_prefabName ?? "(unknown)";
        string siteId = LocationTerrainReader.SiteId(name, position, ZoneSystem.GetZone(position));
        if (!s_held.ContainsKey(siteId))
        {
            s_held[siteId] = new SiteDecision(SiteVerdict.Undecided, SiteRefusalCodes.CheckFailed,
                $"the check itself failed with {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// The site's terrain in world space, from the template Jötunn resolved.
    ///
    /// The same arithmetic the conversion uses —
    /// <c>placement + rotation * localPosition</c>, read through
    /// <see cref="LocationTerrainReader"/> — because a second implementation of
    /// it would be a quiet source of a preflight that disagrees with the write
    /// it is supposed to be predicting.
    /// </summary>
    private static List<LocationTerrainOperation> OperationsAt(
        ZoneSystem.ZoneLocation location, string name, Vector3 position, Quaternion rotation,
        out string unreadable)
    {
        unreadable = null;

        // TerrainOf returns null when the template would not load and an empty
        // description when it loaded and has none. Those were one answer once,
        // and they are opposite answers: one is "this shapes no ground", the
        // other is "nobody knows what this does to the ground".
        TerrainTemplate? terrain = LocationTerrainPatch.TerrainOf(location, name);
        if (terrain == null)
        {
            unreadable = $"the template for '{name}' would not load, so what it does to the ground is unknown.";
            return new List<LocationTerrainOperation>();
        }

        // No asset is needed and none is held: the descriptors carry
        // root-relative positions, so the site's ground is worked out from
        // numbers rather than from a template that has to stay loaded.
        return terrain.OperationsAt(position, rotation);
    }

    /// <summary>
    /// What one zone can tell the preflight: what its compiler already holds,
    /// and the ground the client generates for itself.
    /// </summary>
    private static bool GroundOf(
        Vector2s zone, out TerrainZoneDeltas deltas, out TerrainConversion.VertexHeight baseHeightAt)
    {
        deltas = null;
        baseHeightAt = null;

        // A zone with no compiler holds no deltas -- which is an answer, not a
        // missing one. An existing compiler's deltas are what makes another
        // writer's ground visible.
        deltas = LocationTerrainBridge.HasSavedCompiler(zone)
            ? LocationTerrainBridge.AdoptSaved(zone, ZoneWidth, ZoneScale, out _, out _) ?? Empty(zone)
            : Empty(zone);
        if (deltas == null)
            return false;

        // The heightmap for a zone being generated carries its own build data;
        // for any other zone this comes from the builder, once, and is kept --
        // see LocationTerrainBridge.BaseHeights. Without the keeping, asking
        // here would consume the answer the conversion needs afterwards.
        LocationTerrainBridge.HeightOutcome outcome = LocationTerrainBridge.ReadGeneratedHeightAt(
            deltas, Heightmap.FindHeightmap(new Vector3(deltas.Origin.x, 0f, deltas.Origin.z)),
            out baseHeightAt, out string why, out IDisposable hold);

        // Every zone this decision reads stays held until the decision is made,
        // so a site touching four zones cannot lose its first to its fourth.
        if (hold != null)
        {
            if (s_pinning != null)
                s_pinning.Add(hold);
            else
                hold.Dispose();
        }
        if (outcome == LocationTerrainBridge.HeightOutcome.Deferred)
        {
            // By construction the readiness barrier reserved this room before
            // the zone was generated, so this is not expected; if it happens the
            // decision is Undecided and says why, rather than a site published
            // on ground nobody read.
            Log.LogWarning($"site check for zone {zone.x},{zone.y}: {why}");
        }
        return outcome == LocationTerrainBridge.HeightOutcome.Read;
    }

    /// <summary>
    /// The zone grid, from the one place that defines it.
    ///
    /// This used to be 32 here and 64 in the writer. The gate therefore built a
    /// 33×33 grid over the middle of a 65×65 zone, and every modifier in the
    /// outer half of a zone fell outside the vertices it was checking — so a cut
    /// the compiler could never hold was waved through, and the buildings went
    /// up on ground the conversion would later refuse.
    /// </summary>
    private const int ZoneWidth = TerrainZoneDeltas.ZoneWidth;
    private const float ZoneScale = TerrainZoneDeltas.ZoneScale;

    private static TerrainZoneDeltas Empty(Vector2s zone) =>
        new TerrainZoneDeltas(ZoneSystem.GetZonePos(zone), ZoneWidth, ZoneScale);
}

/// <summary>
/// Where the gate is driven from: the game's own spawn, before it instantiates
/// anything.
///
/// A prefix rather than anything cleverer, because the only moment at which a
/// site can be refused without leaving something behind is the moment before it
/// is built.
/// </summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
public static class LocationSpawnGatePatch
{
    private static bool Prefix(
        ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode,
        ref GameObject __result)
    {
        if (!ServerOnlyMode.Enabled)
            return true;
        // Ghost is a dedicated server generating for a peer; Full is a host
        // generating for itself. Client mode rebuilds from ZDOs that already
        // exist, and refusing there would delete a site somebody already has.
        if (mode != ZoneSystem.SpawnMode.Ghost && mode != ZoneSystem.SpawnMode.Full)
            return true;

        try
        {
            if (LocationSpawnGate.MayPublish(location, pos, rot))
                return true;
        }
        catch (Exception ex)
        {
            // A check that fails withholds. It used to publish, on the reasoning
            // that our own bug should not cost the player a location -- but the
            // cost of publishing is a building standing on ground the conversion
            // will refuse, which is the outcome the whole gate exists to
            // prevent. A missing location is visible and recoverable; a broken
            // one is neither.
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"The site preflight failed, so the location was NOT placed: {ex}");
            LocationSpawnGate.RecordCheckFailure(location, pos, ex);
        }

        __result = null;
        return false;
    }
}
