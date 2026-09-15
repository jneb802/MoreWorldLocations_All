using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Which conversions have already been written here, by the caller's own
    /// identifier.
    ///
    /// Sorted, because it is serialised: a set's iteration order is not a thing
    /// to let decide what a saved world says.
    /// </summary>
    private readonly SortedSet<string> _applied = new SortedSet<string>(StringComparer.Ordinal);

    public int Pitch => Width + 1;

    public bool HasApplied(string operationId) => _applied.Contains(operationId);

    /// <summary>The conversions written here, in a stable order.</summary>
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
    /// Two callers. <see cref="TerrainConversion.ApplyOnce(string, IReadOnlyList{LocationTerrainOperation}, TerrainZoneDeltas, TerrainConversion.VertexHeight, out TerrainConversionResult)"/>
    /// publishes its scratch state here once the conversion has returned; and the
    /// runtime adapter loads a live compiler's arrays into a fresh zone after a
    /// restart, where the completion record comes from elsewhere and must not be
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
    /// restarts mid-bake and forgets which sites it wrote would paint them
    /// again, and paint is a lerp towards a colour, so a second pass leaves a
    /// different mask. The compiler state itself is persisted by the game in the
    /// TerrainComp's ZDO; these identities are persisted beside it by the caller.
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

/// <summary>What a conversion did, and whether the compiler could hold it.</summary>
public sealed class TerrainConversionResult
{
    /// <summary>Vertices whose height the site changes.</summary>
    public int VerticesChanged { get; internal set; }

    /// <summary>Mask texels the site paints.</summary>
    public int TexelsPainted { get; internal set; }

    /// <summary>The largest height change any vertex needs, in metres, signed.</summary>
    public float LargestChange { get; internal set; }

    /// <summary>
    /// Vertices the compiler cannot express, as "x,y needs -11.2 m". A compiler
    /// delta is clamped to ±8 m and the authored path is not, so a template that
    /// cuts deeper than that cannot be served to a stock client as authored.
    /// </summary>
    public IReadOnlyList<string> BeyondCompilerRange { get; internal set; } = new List<string>();

    /// <summary>
    /// Vertices another writer had already moved — a road, or an overlapping
    /// site. The site's own ground wins, so this is reported rather than
    /// silently merged: two writers on one vertex is a planning question.
    /// </summary>
    public IReadOnlyList<string> ContestedVertices { get; internal set; } = new List<string>();

    /// <summary>
    /// True when every vertex the site wants is within the compiler's range. A
    /// false here is a template to exclude, not an approximation to accept.
    /// </summary>
    public bool Representable => BeyondCompilerRange.Count == 0;
}

/// <summary>
/// Turns a location's <c>TerrainModifier</c> children into terrain a client
/// without the mod receives.
///
/// <para><b>What this is not.</b> An earlier version mirrored <c>TerrainComp</c>'s
/// own arithmetic — the level/smooth accumulation a hoe drives. That reproduces
/// the hoe, not the author, and the two differ for exactly the modifiers MWL
/// uses. A location's modifier is applied by <c>Heightmap.ApplyModifiers</c>,
/// which runs each modifier over the heights array in turn: its smooth pass
/// reads ground its own level pass has already flattened, and a second modifier
/// reads the first one's result. The compiler's two passes both read the height
/// from before the operation. On a modifier with both level and smooth —
/// <c>MWL_Ruins1</c> has one — copying the compiler leaves a non-zero smooth
/// delta on top of ground that was already levelled, and the site ends up
/// somewhere the author never put it.</para>
///
/// <para><b>What this is.</b> The authored path is simulated exactly, in
/// vanilla's order, over a scratch copy of the heights the client generates for
/// itself. That result is the ground the author drew. It is then expressed as
/// the one thing a stock client can receive: a level delta per vertex equal to
/// authored minus generated, with the smooth delta cleared. The client's final
/// height is generated + level + smooth
/// (<c>TerrainComp.ApplyToHeightmap</c>), so it lands on the authored height by
/// construction rather than by resemblance.</para>
///
/// <para><b>The contract for <c>baseHeightAt</c>.</b> It returns the height the
/// CLIENT generates for that vertex on its own — <c>Heightmap.GetHeight</c>
/// before any compiler delta, relative to the heightmap origin. It is never
/// progressively updated between modifiers: the simulation keeps its own scratch
/// heights for that, so the sequencing is not each caller's problem to get
/// right.</para>
///
/// <para><b>Clamps are a representability question, not a rounding one.</b>
/// <c>Heightmap.LevelTerrain</c> sets a location's height absolutely with no
/// limit; a compiler delta is clamped to ±8 m, and clamped again against the
/// base height when applied. A modifier that cuts deeper cannot be served as
/// authored, and the result says so instead of writing eight metres and calling
/// it done.</para>
///
/// <para><b>A client that has the mod is displaced twice</b>, because
/// <c>ApplyModifiers</c> adds the compiler's deltas on top of the live
/// instance's work. That is why server-only mode admits stock clients only.</para>
/// </summary>
public static class TerrainConversion
{
    /// <summary>Vanilla clamps an accumulated compiler level delta to ±8 m.</summary>
    public const float MaxLevelDelta = 8f;

    /// <summary>Vanilla clamps an accumulated compiler smooth delta to ±1 m.</summary>
    public const float MaxSmoothDelta = 1f;

    /// <summary>
    /// The height the client generates for a vertex of this zone on its own,
    /// relative to the heightmap origin — <c>Heightmap.GetHeight(x, y)</c>
    /// before any compiler delta. Returns 0 outside the grid, like the game.
    /// </summary>
    public delegate float VertexHeight(int x, int y);

    // ---- the authored path ------------------------------------------------

    /// <summary>
    /// The ground these modifiers draw, simulated the way
    /// <c>Heightmap.ApplyModifiers</c> draws it: each modifier in turn over a
    /// mutating height array, level then smooth, each pass reading what the
    /// previous one left.
    /// </summary>
    /// <param name="operations">
    /// Vanilla sorts by <c>m_sortOrder</c> and breaks ties by creation order,
    /// which for a location's children is the order they appear in the template.
    /// The caller supplies that order and ties keep it, because a sort that
    /// silently decides its own ties is a selector nobody wrote.
    /// </param>
    public static float[] AuthoredHeights(
        IReadOnlyList<LocationTerrainOperation> operations, TerrainZoneDeltas zone, VertexHeight baseHeightAt)
    {
        if (operations == null) throw new ArgumentNullException(nameof(operations));
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        if (baseHeightAt == null) throw new ArgumentNullException(nameof(baseHeightAt));

        float[] heights = new float[zone.Pitch * zone.Pitch];
        for (int y = 0; y < zone.Pitch; y++)
            for (int x = 0; x < zone.Pitch; x++)
                heights[zone.Index(x, y)] = baseHeightAt(x, y);

        foreach (LocationTerrainOperation op in InVanillaOrder(operations))
        {
            // Heightmap.ApplyModifier: level, then smooth, over the same array.
            if (op.Level)
                LiveLevel(zone, heights, Raise(op.Position, op.LevelOffset), op.LevelRadius, op.Square);
            if (op.Smooth)
                LiveSmooth(zone, heights, Raise(op.Position, op.LevelOffset), op.SmoothRadius, op.SmoothPower);
        }

        return heights;
    }

    /// <summary>
    /// Vanilla's order: <c>m_sortOrder</c> ascending, ties left exactly as the
    /// caller gave them. LINQ's OrderBy is documented stable, so a tie keeps the
    /// template's own order rather than letting the sort pick.
    /// </summary>
    private static IEnumerable<LocationTerrainOperation> InVanillaOrder(
        IReadOnlyList<LocationTerrainOperation> operations) =>
        operations.OrderBy(op => op.SortOrder);

    /// <summary>
    /// <c>Heightmap.LevelTerrain</c> for a non-player modifier: sets the height,
    /// with no clamp at all.
    /// </summary>
    private static void LiveLevel(
        TerrainZoneDeltas zone, float[] heights, Vector3 worldPos, float radius, bool square)
    {
        // The live level pass addresses the grid through WorldToVertexMask where
        // the compiler's pass uses WorldToVertex. On an even width -- a zone's 32
        // -- the two are the same mapping; this follows the live one because this
        // is the live path.
        WorldToVertexMask(zone, worldPos, out int centreX, out int centreY);
        float target = worldPos.y - zone.Origin.y;
        float radiusInVertices = radius / zone.Scale;
        int reach = Mathf.CeilToInt(radiusInVertices);

        for (int y = centreY - reach; y <= centreY + reach; y++)
        {
            for (int x = centreX - reach; x <= centreX + reach; x++)
            {
                if (!square && Distance(centreX, centreY, x, y) > radiusInVertices) continue;
                if (!zone.Inside(x, y)) continue;
                heights[zone.Index(x, y)] = target;
            }
        }
    }

    /// <summary>
    /// <c>Heightmap.SmoothTerrain2</c> for a non-player modifier: lerp each
    /// vertex towards the modifier's height by 1 − (distance/radius)^power,
    /// reading and writing the same array, so it sees what level just did.
    /// </summary>
    private static void LiveSmooth(
        TerrainZoneDeltas zone, float[] heights, Vector3 worldPos, float radius, float power)
    {
        WorldToVertex(zone, worldPos, out int centreX, out int centreY);
        float target = worldPos.y - zone.Origin.y;
        float radiusInVertices = radius / zone.Scale;
        int reach = Mathf.CeilToInt(radiusInVertices);

        for (int y = centreY - reach; y <= centreY + reach; y++)
        {
            for (int x = centreX - reach; x <= centreX + reach; x++)
            {
                float distance = Distance(centreX, centreY, x, y);
                if (distance > radiusInVertices || !zone.Inside(x, y)) continue;

                float normalised = distance / radiusInVertices;
                normalised = power == 3f
                    ? normalised * normalised * normalised
                    : Mathf.Pow(normalised, power);

                int index = zone.Index(x, y);
                heights[index] = Mathf.Lerp(heights[index], target, 1f - normalised);
            }
        }
    }

    /// <summary>
    /// <c>Heightmap.PaintCleared</c> for each modifier in turn, over a mask that
    /// starts as the one the zone already carries.
    /// </summary>
    private static int AuthoredPaint(
        IReadOnlyList<LocationTerrainOperation> operations, TerrainZoneDeltas zone,
        float[] authoredHeights, Color[] mask, bool[] painted)
    {
        foreach (LocationTerrainOperation op in InVanillaOrder(operations))
        {
            if (!op.Paint) continue;

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
                    if (!zone.Inside(x, y)) continue;
                    int index = zone.Index(x, y);
                    if (op.PaintHeightCheck && authoredHeights[index] > modifierHeight) continue;

                    float distance = Distance(centreX, centreY, x, y);
                    float weight = 1f - Mathf.Clamp01(distance / radiusInVertices);
                    weight = Mathf.Pow(weight, 0.1f) * op.PaintStrength;

                    Color current = mask[index];
                    float alpha = current.a;
                    Color result = Color.Lerp(current, target, weight);

                    // Vanilla keeps the existing alpha except when clearing
                    // vegetation, where the alpha channel IS the clearing.
                    if (op.PaintType != TerrainModifier.PaintType.ClearVegetation)
                        result.a = alpha;

                    mask[index] = result;
                    painted[index] = true;
                }
            }
        }

        int count = 0;
        foreach (bool wasPainted in painted) if (wasPainted) count++;
        return count;
    }

    // ---- expressing it as compiler deltas ---------------------------------

    /// <summary>
    /// What the compiler would have to hold for a client to land on the authored
    /// ground, without writing anything. Use it to decide whether a template is
    /// servable before committing to it.
    /// </summary>
    public static TerrainConversionResult Preview(
        IReadOnlyList<LocationTerrainOperation> operations, TerrainZoneDeltas zone, VertexHeight baseHeightAt)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        return Convert(operations, zone.Clone(), baseHeightAt);
    }

    /// <summary>
    /// Write the deltas into <paramref name="zone"/> and report what happened.
    /// Prefer <see cref="ApplyOnce(string, IReadOnlyList{LocationTerrainOperation}, TerrainZoneDeltas, VertexHeight, out TerrainConversionResult)"/>,
    /// which is atomic and refuses a repeat.
    /// </summary>
    public static TerrainConversionResult Convert(
        IReadOnlyList<LocationTerrainOperation> operations, TerrainZoneDeltas zone, VertexHeight baseHeightAt)
    {
        float[] authored = AuthoredHeights(operations, zone, baseHeightAt);

        TerrainConversionResult result = new TerrainConversionResult();
        List<string> beyond = new List<string>();
        List<string> contested = new List<string>();
        float largest = 0f;
        int changed = 0;

        for (int y = 0; y < zone.Pitch; y++)
        {
            for (int x = 0; x < zone.Pitch; x++)
            {
                int index = zone.Index(x, y);
                float required = authored[index] - baseHeightAt(x, y);
                if (Mathf.Abs(required) < 1e-5f)
                    continue;

                if (zone.ModifiedHeight[index]
                    && (zone.LevelDelta[index] != 0f || zone.SmoothDelta[index] != 0f))
                    contested.Add(x + "," + y);

                if (Mathf.Abs(required) > MaxLevelDelta)
                    beyond.Add(x + "," + y + " needs " + required.ToString("0.0") + " m");

                // Stated absolutely rather than added to whatever was there, so
                // the client lands on the authored height and a second pass lands
                // on the same one.
                zone.LevelDelta[index] = Mathf.Clamp(required, -MaxLevelDelta, MaxLevelDelta);
                zone.SmoothDelta[index] = 0f;
                zone.ModifiedHeight[index] = true;
                changed++;
                if (Mathf.Abs(required) > Mathf.Abs(largest)) largest = required;
            }
        }

        result.VerticesChanged = changed;
        result.LargestChange = largest;
        result.BeyondCompilerRange = beyond;
        result.ContestedVertices = contested;
        result.TexelsPainted = AuthoredPaint(operations, zone, authored, zone.PaintMask, zone.ModifiedPaint);
        return result;
    }

    /// <summary>
    /// Convert a site's modifiers into this zone unless that conversion has
    /// already been written here.
    /// </summary>
    /// <param name="operationId">
    /// Stable identity for this site's terrain in this zone — the site's id and
    /// the zone's coordinates. All of a site's modifiers in one zone are ONE
    /// conversion, because they have to be simulated together: two of them
    /// converted separately would each see the untouched ground, and the second
    /// would undo the first.
    /// </param>
    /// <remarks>
    /// All of it or none of it. The conversion runs on a copy of the zone and the
    /// copy is published only once it has returned, so a height read that throws
    /// part of the way through leaves the caller's terrain, paint and completion
    /// record exactly as they were and the site can be retried. Recording
    /// completion first — which this did — turns one failed read into a site that
    /// is marked done, is not done, and has some of its vertices moved.
    /// </remarks>
    public static bool ApplyOnce(
        string operationId, IReadOnlyList<LocationTerrainOperation> operations,
        TerrainZoneDeltas zone, VertexHeight baseHeightAt, out TerrainConversionResult result)
    {
        if (string.IsNullOrEmpty(operationId))
            throw new ArgumentException("a conversion needs an identity to be applied once", nameof(operationId));
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        if (baseHeightAt == null) throw new ArgumentNullException(nameof(baseHeightAt));

        if (zone.HasApplied(operationId))
        {
            result = new TerrainConversionResult();
            return false;
        }

        TerrainZoneDeltas scratch = zone.Clone();
        result = Convert(operations, scratch, baseHeightAt);

        zone.AdoptTerrainFrom(scratch);
        zone.RecordApplied(operationId);
        return true;
    }

    /// <summary>One modifier, for callers that do not need the report.</summary>
    public static bool ApplyOnce(
        string operationId, LocationTerrainOperation operation,
        TerrainZoneDeltas zone, VertexHeight baseHeightAt) =>
        ApplyOnce(operationId, new[] { operation }, zone, baseHeightAt, out _);

    // ---- shared geometry --------------------------------------------------

    private static Vector3 Raise(Vector3 position, float offset) =>
        new Vector3(position.x, position.y + offset, position.z);

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
    /// <c>Heightmap.WorldToVertexMask</c>, which the live level pass and the
    /// paint pass use.
    ///
    /// It is written differently — the half is (width+1)/2 and it is added
    /// inside the floor — but for an even width the integer division makes it the
    /// same half, so on a zone heightmap (m_width 32) the two mappings agree
    /// exactly. Kept separate because it is the call vanilla makes, so the
    /// conversion does not quietly depend on that coincidence holding for some
    /// other width.
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
