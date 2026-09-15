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

    /// <summary>Placements published without the ground having been readable.</summary>
    private static readonly Dictionary<string, SiteDecision> s_unchecked = new();

    internal static void Forget()
    {
        s_refused.Clear();
        s_unchecked.Clear();
    }

    /// <summary>What the gate has refused and what it could not check, for <c>mwl_terrain</c>.</summary>
    public static string Status()
    {
        if (s_refused.Count == 0 && s_unchecked.Count == 0)
            return "No placement has been refused, and every one was checked before it was placed.";

        var text = new System.Text.StringBuilder();
        text.Append(s_refused.Count).Append(" placement(s) refused before anything was placed:\n");
        foreach (KeyValuePair<string, SiteDecision> entry in Sorted(s_refused))
            text.Append("  ").Append(entry.Key).Append(" — ").Append(entry.Value.Code).Append(": ")
                .Append(entry.Value.Reason).Append('\n');

        if (s_unchecked.Count > 0)
        {
            text.Append(s_unchecked.Count).Append(" placed without the ground being readable (NOT a pass):\n");
            foreach (KeyValuePair<string, SiteDecision> entry in Sorted(s_unchecked))
                text.Append("  ").Append(entry.Key).Append(" — ").Append(entry.Value.Reason).Append('\n');
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

        List<LocationTerrainOperation> operations = OperationsAt(location, name, position, rotation);
        if (operations == null || operations.Count == 0)
            return true;

        Vector2s home = ZoneSystem.GetZone(position);
        string siteId = LocationTerrainReader.SiteId(name, position, home);
        SiteDecision decision = SitePreflight.Decide(siteId, home, operations, GroundOf);

        switch (decision.Verdict)
        {
            case SiteVerdict.Refuse:
                if (!s_refused.ContainsKey(siteId))
                {
                    s_refused[siteId] = decision;
                    Log.LogWarning(
                        $"{name} at {position.x:0},{position.z:0} was NOT placed: {decision.Reason} " +
                        "Nothing of it exists, which is the point — a site that cannot have its ground " +
                        "shaped must not leave its buildings behind.");
                }
                return false;

            case SiteVerdict.Undecided:
                if (!s_unchecked.ContainsKey(siteId))
                {
                    s_unchecked[siteId] = decision;
                    Log.LogWarning($"{name} at {position.x:0},{position.z:0} was placed UNCHECKED: {decision.Reason}");
                }
                return true;

            default:
                return true;
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
        ZoneSystem.ZoneLocation location, string name, Vector3 position, Quaternion rotation)
    {
        List<TerrainModifier> modifiers = LocationTerrainPatch.ModifiersOf(location, name);
        if (modifiers == null || modifiers.Count == 0)
            return null;

        GameObject asset = location.m_prefab.Asset;
        if (asset == null)
            return null;

        return LocationTerrainReader.Operations(
            modifiers,
            modifier => position + rotation * asset.transform.InverseTransformPoint(modifier.transform.position));
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
        return LocationTerrainBridge.TryGeneratedHeightAt(deltas, Heightmap.FindHeightmap(
            new Vector3(deltas.Origin.x, 0f, deltas.Origin.z)), out baseHeightAt, out _);
    }

    /// <summary>Heightmap.m_width for a zone, and its metres per vertex; the same values the compiler holds.</summary>
    private const int ZoneWidth = 32;
    private const float ZoneScale = 1f;

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
            // A gate that throws must not become a gate that refuses. The site
            // is placed, exactly as it would have been before this existed, and
            // the failure is loud.
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"The site preflight failed and the location was placed unchecked: {ex}");
            return true;
        }

        __result = null;
        return false;
    }
}
