using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// A zone waits until the ground its location needs can be read.
///
/// <para><b>Why a barrier and not a refusal.</b> The spawn gate can refuse a
/// site it has decided against, and that is right. It cannot refuse one it has
/// not been able to look at: the generator may simply not have reached a
/// neighbouring zone yet, and deleting a location for that would be losing
/// content to a scheduling detail. Nor can it publish one, which is the gap
/// this whole mechanism exists to close. So the placement is held.</para>
///
/// <para><b>Where it is held, and why there.</b> Vanilla's own
/// <c>SpawnZone</c> already begins with two of these: it returns false when
/// <c>HeightmapBuilder.IsTerrainReady</c> says no, and again when
/// <c>PokeCanSpawnLocation</c> says no. In both cases the zone is not marked
/// generated, and <c>CreateGhostZones</c> tries it again on the next frame
/// while a player is near. This is a third one in exactly that shape — no
/// second placement engine, no rotation recomputed, nothing remembered that
/// the game does not already remember.</para>
///
/// <para><b>Rotation is not needed here, and that matters.</b> Readiness is
/// "can every zone this site could reach be read", and the set of zones it
/// COULD reach is the same at every rotation: the template's own furthest
/// modifier, taken from the placement. Asking the question rotation-free is
/// what lets it be asked before <c>PlaceLocations</c> computes the
/// rotation.</para>
///
/// <para><b>Bounded.</b> A zone held for ever is a world that never finishes
/// generating, so after <see cref="MaxHolds"/> attempts the placement is given
/// up: refused by identity and never published, and the zone generates without
/// it.</para>
/// </summary>
public static class ZoneReadinessBarrier
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    /// <summary>
    /// How many times one zone may be held before its location is given up.
    ///
    /// Generous, because each hold costs one frame's attempt at one zone and the
    /// thing being waited for -- a neighbour's terrain -- is queued by the
    /// asking. Small enough that a zone whose ground is never going to be
    /// readable does not stall the world.
    /// </summary>
    public const int MaxHolds = 30;

    private static readonly Dictionary<Vector2s, int> s_holds = new();

    internal static void Forget() => s_holds.Clear();

    /// <summary>Zones currently waiting, for the operator command.</summary>
    public static IReadOnlyDictionary<Vector2s, int> Holds => s_holds;

    /// <summary>
    /// Whether this zone may be generated now.
    ///
    /// False holds the zone exactly as vanilla's own readiness checks do: not
    /// generated, tried again next frame.
    /// </summary>
    internal static bool MayGenerate(Vector2s zoneID, ZoneSystem.SpawnMode mode)
    {
        if (!ServerOnlyMode.Enabled || ZoneSystem.instance == null)
            return true;
        // Client mode rebuilds from ZDOs that already exist; holding there would
        // withhold a site somebody already has.
        if (mode != ZoneSystem.SpawnMode.Ghost && mode != ZoneSystem.SpawnMode.Full)
            return true;

        if (!ZoneSystem.instance.m_locationInstances.TryGetValue(zoneID, out ZoneSystem.LocationInstance instance))
            return true;
        if (instance.m_placed)
            return true;

        ZoneSystem.ZoneLocation location = instance.m_location;
        string name = location?.m_prefabName;
        if (string.IsNullOrEmpty(name) || !ServerOnlySelection.IsOurs(name))
            return true;

        Vector2s home = ZoneSystem.GetZone(instance.m_position);
        string siteId = LocationTerrainReader.SiteId(name, instance.m_position, home);
        if (LocationSpawnGate.IsRefused(siteId))
            return true;   // already given up on; the gate withholds the site itself

        if (Readable(location, name, instance.m_position, out string why))
        {
            s_holds.Remove(zoneID);
            return true;
        }

        s_holds.TryGetValue(zoneID, out int held);
        held++;
        s_holds[zoneID] = held;

        if (held < MaxHolds)
        {
            if (held == 1)
                Log.LogInfo($"Zone {zoneID.x},{zoneID.y} waits before generating: {why}");
            return false;
        }

        s_holds.Remove(zoneID);
        var decision = new SiteDecision(SiteVerdict.Refuse, SiteRefusalCodes.NeverReadable,
            $"held {MaxHolds} times and the ground never became readable — {why} " +
            "The placement is given up rather than held for ever, and nothing of it is placed.");
        LocationSpawnGate.GiveUp(siteId, decision);
        Log.LogWarning($"{name} at {instance.m_position.x:0},{instance.m_position.z:0}: {decision.Reason}");
        return true;
    }

    /// <summary>
    /// Whether every zone this placement could reach has ground that can be
    /// read.
    ///
    /// <c>IsTerrainReady</c> rather than a fetch: it queues the build and
    /// answers whether it is done, which is the same question SpawnZone asks
    /// about its own zone, and it consumes nothing.
    /// </summary>
    private static bool Readable(
        ZoneSystem.ZoneLocation location, string name, Vector3 placement, out string why)
    {
        why = null;

        TerrainTemplate? terrain = LocationTerrainPatch.TerrainOf(location, name);
        if (terrain == null)
        {
            why = $"the template for '{name}' has not loaded yet, so what it does to the ground is unknown.";
            return false;
        }
        if (terrain.Modifiers.Count == 0)
            return true;

        // Rotation-free, from the description's own bound: a placement's
        // rotation turns a point about the origin and cannot move it further
        // from the origin, which is what lets readiness be asked before
        // PlaceLocations has chosen one.
        float reach = terrain.Reach;
        if (HeightmapBuilder.instance == null || WorldGenerator.instance == null)
        {
            why = "the terrain builder is not running, so no ground can be read.";
            return false;
        }

        Vector2s min = ZoneSystem.GetZone(new Vector3(placement.x - reach, 0f, placement.z - reach));
        Vector2s max = ZoneSystem.GetZone(new Vector3(placement.x + reach, 0f, placement.z + reach));
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(x, y));
                if (HeightmapBuilder.instance.IsTerrainReady(
                        centre, TerrainZoneDeltas.ZoneWidth, TerrainZoneDeltas.ZoneScale, false, WorldGenerator.instance))
                    continue;
                why = $"the ground for zone {x},{y}, which '{name}' could reach, is not built yet.";
                return false;
            }
        }
        return true;
    }

}

/// <summary>
/// Where the barrier sits: in front of the game's own zone generation, beside
/// vanilla's two readiness checks and behaving the same way.
/// </summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnZone))]
public static class ZoneReadinessBarrierPatch
{
    private static bool Prefix(Vector2s zoneID, ZoneSystem.SpawnMode mode, ref GameObject root, ref bool __result)
    {
        try
        {
            if (ZoneReadinessBarrier.MayGenerate(zoneID, mode))
                return true;
        }
        catch (Exception ex)
        {
            // A barrier that throws lets the zone through: holding every zone on
            // our own bug would stop the world generating entirely, and the
            // spawn gate still stands between the failure and a published site.
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"The zone readiness check failed and zone {zoneID.x},{zoneID.y} was allowed through: {ex}");
            return true;
        }

        root = null;
        __result = false;
        return false;
    }
}
