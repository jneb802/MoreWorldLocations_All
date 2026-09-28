using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// One site's terrain, as work for one zone.
///
/// A zone's compiler holds a zone's arrays, and a site near a boundary shapes
/// ground in more than one zone. Rather than queue the neighbours, each zone
/// asks — when it is generated — which sites reach into it, and writes their
/// share. That makes the order zones are generated in irrelevant, needs no
/// queue to survive a restart, and gives every zone the same answer however it
/// was reached.
/// </summary>
public readonly struct LocationTerrainWork
{
    /// <summary>The zone whose compiler this write goes into.</summary>
    public readonly Vector2s Zone;

    /// <summary>The location's name, for the record and the log.</summary>
    public readonly string LocationName;

    /// <summary>Where the location was placed.</summary>
    public readonly Vector3 Placement;

    /// <summary>
    /// ALL of the site's operations, not only the ones centred in this zone.
    /// They have to be simulated together: an operation levels ground that a
    /// later one smooths, so converting a subset would leave this zone's
    /// vertices at a height no machine ever draws.
    /// </summary>
    public readonly IReadOnlyList<LocationTerrainOperation> Operations;

    /// <summary>What this conversion is recorded under on the zone's compiler.</summary>
    public readonly string SiteId;

    public LocationTerrainWork(
        Vector2s zone, string locationName, Vector3 placement,
        IReadOnlyList<LocationTerrainOperation> operations)
    {
        Zone = zone;
        LocationName = locationName;
        Placement = placement;
        Operations = operations;
        SiteId = LocationTerrainReader.SiteId(locationName, placement, zone);
    }
}

/// <summary>
/// Which sites a zone owes terrain to.
///
/// Kept apart from the patch that calls it so the decision can be tested
/// without a world: the patch supplies the placed sites the game knows about,
/// and this says which of them reach the zone being generated.
/// </summary>
public static class LocationTerrainPlan
{
    /// <summary>
    /// The work <paramref name="zone"/> owes, given the sites near it.
    ///
    /// A site is included when any of its operations reaches this zone, which is
    /// asked of <see cref="LocationTerrainReader.ZonesTouched"/> rather than of
    /// the site's position: a modifier centred in the next zone still shapes
    /// vertices here, and a site whose own zone this is may reach no further.
    ///
    /// <para>A site with no operations is not work. Most templates have none,
    /// and a zone that owes nothing should touch no compiler at all — creating
    /// one to write zeros into would put a _TerrainCompiler ZDO in every zone
    /// the server generates.</para>
    /// </summary>
    public static List<LocationTerrainWork> For(
        Vector2s zone, IReadOnlyList<PlacedSite> sites)
    {
        var work = new List<LocationTerrainWork>();
        if (sites == null)
            return work;

        foreach (PlacedSite site in sites)
        {
            if (site.Operations == null || site.Operations.Count == 0)
                continue;
            if (!LocationTerrainReader.ZonesTouched(site.Operations).Contains(zone))
                continue;
            work.Add(new LocationTerrainWork(zone, site.LocationName, site.Placement, site.Operations));
        }

        // Ordered by name then placement so a zone generated twice plans the
        // same work in the same order, and a log of one run can be compared with
        // another's.
        work.Sort((a, b) =>
        {
            int byName = string.CompareOrdinal(a.LocationName, b.LocationName);
            if (byName != 0) return byName;
            if (a.Placement.x != b.Placement.x) return a.Placement.x.CompareTo(b.Placement.x);
            return a.Placement.z.CompareTo(b.Placement.z);
        });
        return work;
    }

    /// <summary>A location the world has placed, with its terrain read off the template.</summary>
    public readonly struct PlacedSite
    {
        public readonly string LocationName;
        public readonly Vector3 Placement;
        public readonly IReadOnlyList<LocationTerrainOperation> Operations;

        public PlacedSite(string locationName, Vector3 placement,
            IReadOnlyList<LocationTerrainOperation> operations)
        {
            LocationName = locationName;
            Placement = placement;
            Operations = operations;
        }
    }
}
