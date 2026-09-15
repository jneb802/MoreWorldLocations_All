using System;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// The delta arrays of one zone's terrain compiler, addressable without a
/// <c>TerrainComp</c> behind them.
///
/// Vanilla's <c>TerrainComp</c> keeps five arrays over a (width+1)² vertex grid
/// and writes them into its ZDO, which is what makes a terrain change something
/// a client receives rather than something it re-derives. This is the same grid,
/// separated from the MonoBehaviour so the conversion can be run and checked
/// without a scene; the runtime adapter copies these into the real compiler.
/// </summary>
public sealed class TerrainZoneDeltas
{
    /// <summary>Heightmap.m_width: 32 for a zone, so 33 vertices a side.</summary>
    public readonly int Width;

    /// <summary>Metres per vertex (Heightmap.m_scale, 1 for a zone).</summary>
    public readonly float Scale;

    /// <summary>The heightmap's transform position: vertex heights are relative to it.</summary>
    public readonly Vector3 Origin;

    public readonly float[] LevelDelta;
    public readonly float[] SmoothDelta;
    public readonly bool[] ModifiedHeight;
    public readonly Color[] PaintMask;
    public readonly bool[] ModifiedPaint;

    /// <summary>
    /// Which operations have already been written here, by the caller's own
    /// identifier. Levelling accumulates a difference against the client's
    /// unmodified height, so applying one operation twice displaces the ground
    /// twice -- see <see cref="TerrainConversion.ApplyOnce"/>.
    ///
    /// Sorted, because it is serialised: a set's iteration order is not a thing
    /// to let decide what a saved world says.
    /// </summary>
    private readonly SortedSet<string> _applied = new SortedSet<string>(StringComparer.Ordinal);

    public int Pitch => Width + 1;

    public bool HasApplied(string operationId) => _applied.Contains(operationId);

    /// <summary>The operations written here, in a stable order.</summary>
    public IEnumerable<string> AppliedOperations => _applied;

    internal bool RecordApplied(string operationId) => _applied.Add(operationId);

    public TerrainZoneDeltas(Vector3 origin, int width = 32, float scale = 1f)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (scale <= 0f) throw new ArgumentOutOfRangeException(nameof(scale));

        Origin = origin;
        Width = width;
        Scale = scale;

        int vertices = (width + 1) * (width + 1);
        LevelDelta = new float[vertices];
        SmoothDelta = new float[vertices];
        ModifiedHeight = new bool[vertices];
        PaintMask = new Color[vertices];
        ModifiedPaint = new bool[vertices];
    }

    public int Index(int x, int y) => y * Pitch + x;

    public bool Inside(int x, int y) => x >= 0 && y >= 0 && x < Pitch && y < Pitch;

    /// <summary>
    /// A copy carrying the same terrain, paint and completion record. Used as
    /// scratch state: a conversion that fails part way through is discarded with
    /// the copy rather than left half-written over the caller's arrays.
    /// </summary>
    public TerrainZoneDeltas Clone()
    {
        TerrainZoneDeltas copy = new TerrainZoneDeltas(Origin, Width, Scale);
        Array.Copy(LevelDelta, copy.LevelDelta, LevelDelta.Length);
        Array.Copy(SmoothDelta, copy.SmoothDelta, SmoothDelta.Length);
        Array.Copy(ModifiedHeight, copy.ModifiedHeight, ModifiedHeight.Length);
        Array.Copy(PaintMask, copy.PaintMask, PaintMask.Length);
        Array.Copy(ModifiedPaint, copy.ModifiedPaint, ModifiedPaint.Length);
        foreach (string applied in _applied)
            copy._applied.Add(applied);
        return copy;
    }

    /// <summary>
    /// Take another zone's terrain and paint, leaving this one's completion
    /// record alone.
    ///
    /// Two callers. <see cref="TerrainConversion.ApplyOnce"/> publishes its
    /// scratch state here once the conversion has returned; and the runtime
    /// adapter loads a live compiler's arrays into a fresh zone after a restart,
    /// where the completion record comes from elsewhere and must not be
    /// overwritten by whatever the other object happens to hold.
    /// </summary>
    public void AdoptTerrainFrom(TerrainZoneDeltas other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        if (other.Width != Width || other.Scale != Scale)
            throw new ArgumentException("zones of different shapes cannot be merged", nameof(other));

        Array.Copy(other.LevelDelta, LevelDelta, LevelDelta.Length);
        Array.Copy(other.SmoothDelta, SmoothDelta, SmoothDelta.Length);
        Array.Copy(other.ModifiedHeight, ModifiedHeight, ModifiedHeight.Length);
        Array.Copy(other.PaintMask, PaintMask, PaintMask.Length);
        Array.Copy(other.ModifiedPaint, ModifiedPaint, ModifiedPaint.Length);
    }

    /// <summary>
    /// Rebuild the completion record from what a saved world carries.
    ///
    /// The record has to outlive the process, not just the object: a server that
    /// restarts mid-bake and forgets which operations it wrote would write them
    /// again, and a second write sinks the site twice. The compiler state itself
    /// is persisted by the game in the TerrainComp's ZDO; these identities are
    /// persisted beside it by the caller.
    /// </summary>
    public void RestoreApplied(IEnumerable<string> operationIds)
    {
        if (operationIds == null) throw new ArgumentNullException(nameof(operationIds));
        foreach (string id in operationIds)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("a saved completion record contains an empty identity", nameof(operationIds));
            _applied.Add(id);
        }
    }

    /// <summary>
    /// The completion record as one string, for storing beside the compiler.
    /// Newline-separated and sorted, so the same set always produces the same
    /// bytes and a diff of two saves means the sets differ.
    /// </summary>
    public string SerializeApplied() => string.Join("\n", _applied);

    /// <summary>The inverse of <see cref="SerializeApplied"/>; empty means nothing was written.</summary>
    public void DeserializeApplied(string serialized)
    {
        if (string.IsNullOrEmpty(serialized))
            return;
        RestoreApplied(serialized.Split('\n'));
    }
}

/// <summary>
/// Reproduces a location's <c>TerrainModifier</c> as persistent compiler deltas.
///
/// The arithmetic is vanilla's, taken from <c>TerrainComp.LevelTerrain</c>,
/// <c>SmoothTerrain</c> and <c>Heightmap.PaintCleared</c> rather than invented:
/// levelling accumulates the difference between the target height and the
/// vertex's current height and absorbs any smoothing already there, smoothing
/// lerps towards the modifier's own height by a power of the normalised
/// distance, and painting lerps the mask towards the paint colour by
/// distance^0.1 × strength. The clamps are vanilla's too -- ±8 m on level, ±1 m
/// on smooth -- and they matter: a modifier that would cut more than 8 m is
/// silently limited by the game as well.
///
/// <para><b>A client that has the mod sees the change twice.</b>
/// <c>Heightmap.ApplyModifiers</c> applies the live <c>TerrainModifier</c>
/// instances to the heights and then adds the compiler's deltas on top
/// (<c>TerrainComp.ApplyToHeightmap</c>). A stock client has no live instance
/// and gets the shaping once, which is the point; a client running the same
/// build of MWL builds the proxy half, gets the instance, and is displaced
/// twice. That has to be decided and measured before a modded client is
/// supported on a server-only world -- it is not a thing to guess at.</para>
/// </summary>
public static class TerrainConversion
{
    /// <summary>Vanilla clamps accumulated levelling to ±8 m.</summary>
    public const float MaxLevelDelta = 8f;

    /// <summary>Vanilla clamps accumulated smoothing to ±1 m.</summary>
    public const float MaxSmoothDelta = 1f;

    /// <summary>
    /// Height of a vertex in the zone's grid, relative to the heightmap origin
    /// — <c>Heightmap.GetHeight(x, y)</c>, which returns 0 outside the grid.
    /// </summary>
    public delegate float VertexHeight(int x, int y);

    /// <summary>
    /// Apply one operation to one zone's deltas. Only the parts of the
    /// operation that reach into this zone have any effect, so a site straddling
    /// a boundary is handled by calling this once per affected zone with the
    /// same operation.
    /// </summary>
    public static void Apply(LocationTerrainOperation op, TerrainZoneDeltas zone, VertexHeight heightAt)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        if (heightAt == null) throw new ArgumentNullException(nameof(heightAt));

        // Vanilla's order, and it is not interchangeable: levelling zeroes the
        // smoothing already at a vertex, so smoothing first would be thrown away.
        if (op.Level)
            LevelTerrain(zone, heightAt, Raise(op.Position, op.LevelOffset), op.LevelRadius, op.Square);

        if (op.Smooth)
            SmoothTerrain(zone, heightAt, Raise(op.Position, op.LevelOffset), op.SmoothRadius, op.SmoothPower);

        if (op.Paint)
            PaintCleared(zone, heightAt, op);
    }

    /// <summary>
    /// Apply an operation to a zone unless that same operation has already been
    /// written there, and say which happened.
    ///
    /// This is the entry point a bake should use. <see cref="Apply"/> is
    /// vanilla's arithmetic and is deliberately not idempotent: the level delta
    /// is the difference between the target and the height the client generates
    /// for itself, which does not change, so writing it twice moves the ground
    /// twice. In the game a hoe does not have this problem, because by the time
    /// a second operation runs the heightmap has already been rebuilt with the
    /// first; a bake computing against generated heights has no such rebuild
    /// between passes.
    /// </summary>
    /// <param name="operationId">
    /// Stable identity for this modifier within this site -- the site's id and
    /// the modifier's path in the template. Two different modifiers must not
    /// share one, or the second is silently dropped.
    /// </param>
    /// <returns>True if it was written, false if it was already there.</returns>
    /// <remarks>
    /// All of it or none of it. The conversion runs on a copy of the zone and
    /// the copy is published only once it has returned, so a read that throws
    /// part of the way through leaves the caller's terrain, paint and completion
    /// record exactly as they were and the operation can be retried. Recording
    /// completion first -- which this did -- turns one failed read into a site
    /// that is marked done, is not done, and has some of its vertices moved.
    /// </remarks>
    public static bool ApplyOnce(
        string operationId, LocationTerrainOperation op, TerrainZoneDeltas zone, VertexHeight heightAt)
    {
        if (string.IsNullOrEmpty(operationId))
            throw new ArgumentException("an operation needs an identity to be applied once", nameof(operationId));
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        if (heightAt == null) throw new ArgumentNullException(nameof(heightAt));

        if (zone.HasApplied(operationId))
            return false;

        TerrainZoneDeltas scratch = zone.Clone();
        Apply(op, scratch, heightAt);

        zone.AdoptTerrainFrom(scratch);
        zone.RecordApplied(operationId);
        return true;
    }

    private static Vector3 Raise(Vector3 position, float offset) =>
        new Vector3(position.x, position.y + offset, position.z);

    /// <summary>
    /// <c>TerrainComp.LevelTerrain</c>: bring every vertex in range to the
    /// modifier's height. The delta is against the vertex's CURRENT height, so
    /// what is stored is "how far this vertex has to move", which is exactly
    /// what a client can apply to terrain it generated itself.
    /// </summary>
    private static void LevelTerrain(
        TerrainZoneDeltas zone, VertexHeight heightAt, Vector3 worldPos, float radius, bool square)
    {
        WorldToVertex(zone, worldPos, out int centreX, out int centreY);
        float targetHeight = worldPos.y - zone.Origin.y;
        float radiusInVertices = radius / zone.Scale;
        int reach = Mathf.CeilToInt(radiusInVertices);

        for (int y = centreY - reach; y <= centreY + reach; y++)
        {
            for (int x = centreX - reach; x <= centreX + reach; x++)
            {
                if (!square && Distance(centreX, centreY, x, y) > radiusInVertices)
                    continue;
                if (!zone.Inside(x, y))
                    continue;

                int index = zone.Index(x, y);
                float delta = targetHeight - heightAt(x, y);

                // Smoothing already at this vertex is absorbed rather than added
                // to: levelling wins over an earlier smooth in vanilla.
                delta += zone.SmoothDelta[index];
                zone.SmoothDelta[index] = 0f;

                zone.LevelDelta[index] = Mathf.Clamp(
                    zone.LevelDelta[index] + delta, -MaxLevelDelta, MaxLevelDelta);
                zone.ModifiedHeight[index] = true;
            }
        }
    }

    /// <summary>
    /// <c>TerrainComp.SmoothTerrain</c>: lerp each vertex towards the modifier's
    /// height, weighted by 1 − (distance/radius)^power, so the effect fades to
    /// nothing at the rim. Always circular, whatever <c>Square</c> says —
    /// vanilla's smooth has no square branch.
    /// </summary>
    private static void SmoothTerrain(
        TerrainZoneDeltas zone, VertexHeight heightAt, Vector3 worldPos, float radius, float power)
    {
        WorldToVertex(zone, worldPos, out int centreX, out int centreY);
        float targetHeight = worldPos.y - zone.Origin.y;
        float radiusInVertices = radius / zone.Scale;
        int reach = Mathf.CeilToInt(radiusInVertices);

        for (int y = centreY - reach; y <= centreY + reach; y++)
        {
            for (int x = centreX - reach; x <= centreX + reach; x++)
            {
                float distance = Distance(centreX, centreY, x, y);
                if (distance > radiusInVertices || !zone.Inside(x, y))
                    continue;

                float normalised = distance / radiusInVertices;
                normalised = power == 3f
                    ? normalised * normalised * normalised
                    : Mathf.Pow(normalised, power);

                float height = heightAt(x, y);
                float delta = Mathf.Lerp(height, targetHeight, 1f - normalised) - height;

                int index = zone.Index(x, y);
                zone.SmoothDelta[index] = Mathf.Clamp(
                    zone.SmoothDelta[index] + delta, -MaxSmoothDelta, MaxSmoothDelta);
                zone.ModifiedHeight[index] = true;
            }
        }
    }

    /// <summary>
    /// <c>Heightmap.PaintCleared</c> — the overload a location's modifier reaches,
    /// not the hoe's. The half-vertex offset and the mask grid are vanilla's:
    /// the paint mask is addressed by <c>WorldToVertexMask</c>, which is not the
    /// same mapping as the height grid's.
    /// </summary>
    private static void PaintCleared(TerrainZoneDeltas zone, VertexHeight heightAt, LocationTerrainOperation op)
    {
        Vector3 worldPos = new Vector3(op.Position.x - 0.5f, op.Position.y, op.Position.z - 0.5f);
        float modifierHeight = worldPos.y - zone.Origin.y;
        WorldToVertexMask(zone, worldPos, out int centreX, out int centreY);

        float radiusInVertices = op.PaintRadius / zone.Scale;
        int reach = Mathf.CeilToInt(radiusInVertices);
        Color target = PaintColour(op.PaintType);

        for (int y = centreY - reach; y <= centreY + reach; y++)
        {
            for (int x = centreX - reach; x <= centreX + reach; x++)
            {
                if (!zone.Inside(x, y))
                    continue;
                if (op.PaintHeightCheck && heightAt(x, y) > modifierHeight)
                    continue;

                float distance = Distance(centreX, centreY, x, y);
                float weight = 1f - Mathf.Clamp01(distance / radiusInVertices);
                weight = Mathf.Pow(weight, 0.1f) * op.PaintStrength;

                int index = zone.Index(x, y);
                Color current = zone.PaintMask[index];
                float alpha = current.a;
                Color painted = Color.Lerp(current, target, weight);

                // Vanilla keeps the existing alpha except when clearing
                // vegetation, where the alpha channel IS the clearing.
                if (op.PaintType != TerrainModifier.PaintType.ClearVegetation)
                    painted.a = alpha;

                zone.PaintMask[index] = painted;
                zone.ModifiedPaint[index] = true;
            }
        }
    }

    private static Color PaintColour(TerrainModifier.PaintType type)
    {
        switch (type)
        {
            case TerrainModifier.PaintType.Dirt: return Heightmap.m_paintMaskDirt;
            case TerrainModifier.PaintType.Cultivate: return Heightmap.m_paintMaskCultivated;
            case TerrainModifier.PaintType.Paved: return Heightmap.m_paintMaskPaved;
            case TerrainModifier.PaintType.Reset: return Heightmap.m_paintMaskNothing;
            case TerrainModifier.PaintType.ClearVegetation: return Heightmap.m_paintMaskClearVegetation;
            case TerrainModifier.PaintType.DeepSnow: return Heightmap.m_paintMaskDeepSnow;
            default: return Heightmap.m_paintMaskNothing;
        }
    }

    /// <summary><c>Heightmap.WorldToVertex</c>.</summary>
    public static void WorldToVertex(TerrainZoneDeltas zone, Vector3 worldPos, out int x, out int y)
    {
        float localX = worldPos.x - zone.Origin.x;
        float localZ = worldPos.z - zone.Origin.z;
        int half = zone.Width / 2;
        x = Mathf.FloorToInt(localX / zone.Scale + 0.5f) + half;
        y = Mathf.FloorToInt(localZ / zone.Scale + 0.5f) + half;
    }

    /// <summary>
    /// <c>Heightmap.WorldToVertexMask</c>, which the paint pass uses where the
    /// height passes use <see cref="WorldToVertex"/>.
    ///
    /// It is written differently -- the half is (width+1)/2 and it is added
    /// inside the floor -- but for an even width the integer division makes it
    /// the same half, so on a zone heightmap (m_width 32) the two mappings
    /// agree exactly. Kept separate because it is the call vanilla makes, so
    /// the conversion does not quietly depend on that coincidence holding for
    /// some other width.
    /// </summary>
    public static void WorldToVertexMask(TerrainZoneDeltas zone, Vector3 worldPos, out int x, out int y)
    {
        float localX = worldPos.x - zone.Origin.x;
        float localZ = worldPos.z - zone.Origin.z;
        int half = (zone.Width + 1) / 2;
        x = Mathf.FloorToInt(localX / zone.Scale + 0.5f + half);
        y = Mathf.FloorToInt(localZ / zone.Scale + 0.5f + half);
    }

    private static float Distance(int ax, int ay, int bx, int by)
    {
        float dx = ax - bx;
        float dy = ay - by;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
