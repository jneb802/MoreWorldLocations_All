using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Where the terrain conversion is driven from: a zone finishing generation.
///
/// This half is engine glue and is deliberately thin -- find the location
/// proxies near the zone, read their templates' modifiers, hand them to
/// <see cref="LocationTerrainWriter.WriteZone"/>. The decisions all live there,
/// where they can be tested without a world.
///
/// <para><b>Where the placement comes from.</b> Not from a ledger of our own:
/// <c>ZoneSystem.PlaceLocations</c> computes the position and rotation locally
/// and keeps neither -- <c>LocationInstance</c> stores only the pre-ground-data
/// position and a placed flag. Vanilla does save the <c>LocationProxy</c>, at
/// exactly the transform the location was placed with, so reading it makes the
/// placement survive a restart for free and removes any chance of our record
/// disagreeing with the world.</para>
/// </summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceLocations))]
public static class LocationTerrainPatch
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    /// <summary>Modifiers read off a resolved template, in template order, once per name.</summary>
    private static readonly Dictionary<string, List<TerrainModifier>> s_templateModifiers = new();

    /// <summary>Sites already reported as reaching too far, so the log says it once.</summary>
    private static readonly HashSet<string> s_reportedOutOfReach = new();

    /// <summary>A new world reads its own templates; see LocationTerrainWriter.Reset.</summary>
    internal static void Forget()
    {
        s_templateModifiers.Clear();
        s_reportedOutOfReach.Clear();
    }

    private static void Postfix(Vector2s zoneID, Heightmap hmap, ZoneSystem.SpawnMode mode)
    {
        if (!ServerOnlyMode.Enabled || hmap == null)
            return;
        // Ghost is a dedicated server generating for a peer; Full is a host
        // generating for itself. Client mode rebuilds from ZDOs and must not
        // write.
        if (mode != ZoneSystem.SpawnMode.Ghost && mode != ZoneSystem.SpawnMode.Full)
            return;

        try
        {
            LocationTerrainWriter.WriteZone(zoneID, hmap, SitesNear(zoneID));
        }
        catch (System.Exception ex)
        {
            // One zone's failure is one zone's. Letting it out of here would
            // stop the game generating the world.
            Log.LogError($"Terrain for zone {zoneID.x},{zoneID.y} failed: {ex}");
        }
    }


    /// <summary>
    /// The sites whose terrain could reach this zone: the location proxies in it
    /// and its eight neighbours, read from saved data.
    /// </summary>
    private static List<LocationTerrainPlan.PlacedSite> SitesNear(Vector2s zoneID)
    {
        var sites = new List<LocationTerrainPlan.PlacedSite>();
        if (ZDOMan.instance == null || ZoneSystem.instance == null)
            return sites;

        var visited = new HashSet<ZoneSystem.SectorIndex>();
        var zdos = new List<ZDO>();
        for (int x = zoneID.x - 1; x <= zoneID.x + 1; x++)
            for (int y = zoneID.y - 1; y <= zoneID.y + 1; y++)
                ZDOMan.instance.FindObjects(new Vector2s(x, y), zdos, visited);

        foreach (ZDO zdo in zdos)
        {
            if (zdo.GetPrefab() != LocationProxyPrefab)
                continue;
            int locationHash = zdo.GetInt(ZDOVars.s_location, 0);
            if (locationHash == 0)
                continue;
            if (!ZoneSystem.instance.m_locationsByHash.TryGetValue(locationHash, out ZoneSystem.ZoneLocation location))
                continue;

            string name = location.m_prefabName;
            // Only ours. A vanilla location's modifiers are in a template the
            // stock client HAS, so it shapes that ground itself; converting them
            // as well would apply the deltas on top and sink the site twice.
            if (!ServerOnlySelection.IsOurs(name))
                continue;

            List<TerrainModifier> modifiers = ModifiersOf(location, name);
            if (modifiers == null || modifiers.Count == 0)
                continue;

            Vector3 placement = zdo.GetPosition();
            Quaternion rotation = zdo.GetRotation();
            GameObject asset = location.m_prefab.Asset;
            if (asset == null)
                continue;

            List<LocationTerrainOperation> operations = LocationTerrainReader.Operations(
                modifiers,
                modifier => placement + rotation * asset.transform.InverseTransformPoint(modifier.transform.position));
            if (operations.Count == 0)
                continue;

            if (ReachesTooFar(zdo, operations, name, placement))
                continue;

            sites.Add(new LocationTerrainPlan.PlacedSite(name, placement, operations));
        }
        return sites;
    }

    /// <summary>
    /// Whether a site reaches beyond the ring of zones this scheme can see.
    ///
    /// The per-zone rule works because two neighbouring zones each find the
    /// other's proxy. A site reaching two zones away would be found by the near
    /// neighbour and not by the far one, and the far one would keep the ground
    /// the client generated — a step in the middle of the site. It is reported
    /// and skipped entirely rather than written where it happens to be seen.
    /// </summary>
    private static bool ReachesTooFar(
        ZDO zdo, IReadOnlyList<LocationTerrainOperation> operations, string name, Vector3 placement)
    {
        Vector2s home = zdo.GetSector();
        foreach (Vector2s touched in LocationTerrainReader.ZonesTouched(operations))
        {
            if (Mathf.Abs(touched.x - home.x) <= 1 && Mathf.Abs(touched.y - home.y) <= 1)
                continue;
            string id = LocationTerrainReader.SiteId(name, placement, home);
            if (s_reportedOutOfReach.Add(id))
                Log.LogError(
                    $"{name} at {placement.x:0},{placement.z:0} shapes ground in zone " +
                    $"{touched.x},{touched.y}, more than one zone from its own {home.x},{home.y}. " +
                    "No part of it was converted: writing only the zones that can see it would leave " +
                    "a step across the site.");
            return true;
        }
        return false;
    }

    /// <summary>
    /// A template's terrain modifiers, in the order they appear in it.
    ///
    /// <c>Utils.GetEnabledComponentsInChildren</c> is what vanilla itself uses to
    /// read a location's children, so the set here is the set the game would
    /// instantiate on a client that has the template — including the rule that a
    /// child under an inactive parent does not count.
    /// </summary>
    private static List<TerrainModifier> ModifiersOf(ZoneSystem.ZoneLocation location, string name)
    {
        if (s_templateModifiers.TryGetValue(name, out List<TerrainModifier> cached))
            return cached;

        GameObject asset = location.m_prefab.Asset;
        if (asset == null)
            return null;

        var modifiers = new List<TerrainModifier>(global::Utils.GetEnabledComponentsInChildren<TerrainModifier>(asset));
        s_templateModifiers[name] = modifiers;
        if (modifiers.Count > 0)
            Log.LogInfo($"{name}: {modifiers.Count} terrain modifier(s) to convert for stock clients.");
        return modifiers;
    }

    private static readonly int LocationProxyPrefab = "LocationProxy".GetStableHashCode();

}

/// <summary>A new world: forget the templates read for the last one.</summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Awake))]
public static class LocationTerrainWriterReset
{
    private static void Postfix()
    {
        LocationTerrainWriter.Reset();
        LocationTerrainPatch.Forget();
    }
}
