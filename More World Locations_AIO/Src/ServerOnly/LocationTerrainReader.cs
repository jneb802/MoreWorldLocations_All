using System;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// A placed location's terrain modifiers, as values.
///
/// <see cref="TerrainConversion"/> is arithmetic over
/// <see cref="LocationTerrainOperation"/>; this is where those operations come
/// from. It reads the <c>TerrainModifier</c> components of the template Jötunn
/// has already resolved, which is the only place the real settings exist: the
/// bundle holds mocks, and a dedicated server never instantiates the proxy half
/// where the modifiers live, so there is nothing in the scene to read.
///
/// The rotation is deliberately not recomputed here. A caller hands in each
/// modifier's world position, which it gets from the game's own
/// <c>placementPosition + placementRotation * localPosition</c>; a second
/// implementation of that would be a quiet source of ground in the wrong place.
/// What this owns is the field-by-field mapping and the question of which zones
/// an operation reaches, both of which are testable without a scene.
/// </summary>
public static class LocationTerrainReader
{
    /// <summary>
    /// One modifier as a value, at the world position it was placed.
    ///
    /// <c>m_paintCleared</c> is the flag vanilla's <c>ApplyModifier</c> gates
    /// painting on, so it is what <see cref="LocationTerrainOperation.Paint"/>
    /// means — not the paint type, which is meaningful only when it is set.
    /// </summary>
    public static LocationTerrainOperation Operation(TerrainModifier modifier, Vector3 worldPosition)
    {
        if (modifier == null) throw new ArgumentNullException(nameof(modifier));
        return new LocationTerrainOperation(
            worldPosition,
            level: modifier.m_level,
            levelRadius: modifier.m_levelRadius,
            levelOffset: modifier.m_levelOffset,
            square: modifier.m_square,
            smooth: modifier.m_smooth,
            smoothRadius: modifier.m_smoothRadius,
            smoothPower: modifier.m_smoothPower,
            paint: modifier.m_paintCleared,
            paintRadius: modifier.m_paintRadius,
            paintStrength: modifier.m_paintStrength,
            paintType: modifier.m_paintType,
            paintHeightCheck: modifier.m_paintHeightCheck,
            sortOrder: modifier.m_sortOrder);
    }

    /// <summary>
    /// Every modifier of one placed location, in the order vanilla would apply
    /// them.
    /// </summary>
    /// <param name="modifiers">
    /// The template's <c>TerrainModifier</c> children, in template order. That
    /// order is vanilla's tie-break: <c>TerrainModifier.SortByModifiers</c> goes
    /// by <c>m_playerModifiction</c>, then <c>m_sortOrder</c>, then creation
    /// time — and for a location's children, instantiated together, creation
    /// time is the order they appear in the template.
    /// </param>
    /// <param name="worldPositionOf">
    /// Where each modifier ends up once the location is placed. The caller
    /// computes it with the game's own transform maths.
    /// </param>
    /// <remarks>
    /// A modifier that the game itself would skip is skipped here for the same
    /// reason and no other: <c>ApplyModifiers</c> tests <c>item.enabled</c>, and
    /// a modifier with <c>m_useTerrainCompiler</c> already writes through a
    /// compiler, so converting it would shape the ground twice.
    /// </remarks>
    public static List<LocationTerrainOperation> Operations(
        IReadOnlyList<TerrainModifier> modifiers, Func<TerrainModifier, Vector3> worldPositionOf)
    {
        if (modifiers == null) throw new ArgumentNullException(nameof(modifiers));
        if (worldPositionOf == null) throw new ArgumentNullException(nameof(worldPositionOf));

        var operations = new List<LocationTerrainOperation>(modifiers.Count);
        foreach (TerrainModifier modifier in modifiers)
        {
            if (modifier == null || !modifier.enabled || modifier.m_useTerrainCompiler)
                continue;
            operations.Add(Operation(modifier, worldPositionOf(modifier)));
        }
        return operations;
    }

    /// <summary>
    /// The zones these operations reach, in a fixed order.
    ///
    /// A site is converted zone by zone, because a zone's terrain compiler holds
    /// one zone's arrays. An operation near a boundary reaches into the next
    /// zone, and leaving that zone out is how a levelled site gets a step down
    /// its edge — so the bound is taken from each operation's own radius rather
    /// than from the location's position.
    ///
    /// <para>All of a site's operations are handed to EVERY zone they could
    /// touch, not just the ones centred there: they have to be simulated
    /// together (see <c>TerrainConversion.ApplyOnce</c>), and an operation
    /// centred in the next zone still shapes vertices in this one.</para>
    /// </summary>
    public static List<Vector2s> ZonesTouched(IReadOnlyList<LocationTerrainOperation> operations)
    {
        if (operations == null) throw new ArgumentNullException(nameof(operations));

        var zones = new List<Vector2s>();
        var seen = new HashSet<Vector2s>();
        foreach (LocationTerrainOperation op in operations)
        {
            float r = op.Radius;
            Vector2s min = ZoneSystem.GetZone(new Vector3(op.Position.x - r, 0f, op.Position.z - r));
            Vector2s max = ZoneSystem.GetZone(new Vector3(op.Position.x + r, 0f, op.Position.z + r));
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var zone = new Vector2s(x, y);
                    if (seen.Add(zone))
                        zones.Add(zone);
                }
            }
        }
        zones.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        return zones;
    }

    /// <summary>
    /// The identity a conversion is recorded under: this site's terrain in this
    /// zone.
    ///
    /// It has to survive a restart and name one thing, so it is built from the
    /// location's name and its placed position rather than from anything
    /// allocated at runtime. Two locations of the same template in one zone are
    /// different sites and get different ids.
    /// </summary>
    public static string SiteId(string locationName, Vector3 placement, Vector2s zone) =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0}@{1:F1},{2:F1}#{3},{4}", locationName, placement.x, placement.z, zone.x, zone.y);
}
