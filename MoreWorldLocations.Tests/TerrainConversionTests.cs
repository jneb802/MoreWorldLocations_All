using System;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Turning a location's TerrainModifier into something a client without the mod
/// receives.
///
/// The failure being guarded against is not a crash. It is a ruin standing on
/// ground that was never flattened, because the flattening lived in a component
/// the client never got — visible only to someone standing there. So these
/// tests check the ground the client would end up on, vertex by vertex, against
/// the arithmetic vanilla itself uses.
/// </summary>
public class TerrainConversionTests
{
    private const int Width = 32;
    private const float Scale = 1f;

    /// <summary>
    /// A zone of rolling ground, the way a real one arrives: heights relative to
    /// the heightmap origin, varying across the grid so a levelling operation
    /// has something to level. Deterministic, so a failure is reproducible.
    /// </summary>
    private sealed class Ground
    {
        private readonly float[] _heights = new float[(Width + 1) * (Width + 1)];

        public Ground(Func<int, int, float> shape)
        {
            for (int y = 0; y <= Width; y++)
                for (int x = 0; x <= Width; x++)
                    _heights[y * (Width + 1) + x] = shape(x, y);
        }

        /// <summary>Heightmap.GetHeight: 0 outside the grid, like the game.</summary>
        public float At(int x, int y)
        {
            if (x < 0 || y < 0 || x > Width || y > Width) return 0f;
            return _heights[y * (Width + 1) + x];
        }

        /// <summary>
        /// What the client's terrain ends up as: its own generated height plus
        /// the compiler's deltas, exactly as TerrainComp.ApplyToHeightmap adds
        /// them (and with vanilla's ±8 m clamp against the base height).
        /// </summary>
        public float AsReceived(TerrainZoneDeltas zone, int x, int y)
        {
            int i = zone.Index(x, y);
            float height = At(x, y);
            if (zone.LevelDelta[i] == 0f && zone.SmoothDelta[i] == 0f)
                return height;
            return Mathf.Clamp(height + zone.LevelDelta[i] + zone.SmoothDelta[i], height - 8f, height + 8f);
        }
    }

    private static Ground Slope() => new Ground((x, y) => 20f + x * 0.25f + y * 0.1f);

    private static TerrainZoneDeltas Zone() =>
        new TerrainZoneDeltas(new Vector3(0f, 0f, 0f), Width, Scale);

    /// <summary>World position of the grid centre vertex, so a radius fits inside.</summary>
    private static Vector3 Centre(float height) => new Vector3(0f, height, 0f);

    // --- levelling -------------------------------------------------------

    [Fact]
    public void LevellingBringsEveryVertexInRangeToTheModifiersHeight()
    {
        // MWL_Ruins1's modifier: level, radius 3, square. Against a slope, the
        // 7x7 block around the centre must come out flat at the target.
        Ground ground = Slope();
        TerrainZoneDeltas zone = Zone();
        LocationTerrainOperation op = new(Centre(25f), level: true, levelRadius: 3f, square: true);

        TerrainConversion.Apply(op, zone, ground.At);

        TerrainConversion.WorldToVertex(zone, Centre(25f), out int cx, out int cy);
        for (int y = cy - 3; y <= cy + 3; y++)
            for (int x = cx - 3; x <= cx + 3; x++)
                Assert.Equal(25f, ground.AsReceived(zone, x, y), 3);
    }

    [Fact]
    public void LevellingLeavesGroundOutsideTheRadiusAlone()
    {
        // The site's footprint has to stop where the audit says it stops; a
        // modifier that quietly reshapes the next ridge would move a road.
        Ground ground = Slope();
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(25f), level: true, levelRadius: 3f, square: true),
            zone, ground.At);

        TerrainConversion.WorldToVertex(zone, Centre(25f), out int cx, out int cy);
        Assert.Equal(ground.At(cx + 5, cy), ground.AsReceived(zone, cx + 5, cy), 4);
        Assert.False(zone.ModifiedHeight[zone.Index(cx + 5, cy)]);
    }

    [Fact]
    public void ARoundFootprintDoesNotTouchItsCorners()
    {
        // square=false is a circle in vanilla, and MWL_RuinsWell1 uses one. The
        // corner of the bounding box is outside it.
        Ground ground = Slope();
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(25f), level: true, levelRadius: 4f, square: false),
            zone, ground.At);

        TerrainConversion.WorldToVertex(zone, Centre(25f), out int cx, out int cy);
        Assert.True(zone.ModifiedHeight[zone.Index(cx + 4, cy)], "the rim on the axis is inside a radius-4 circle");
        Assert.False(zone.ModifiedHeight[zone.Index(cx + 4, cy + 4)], "the corner of the box is outside it");
    }

    [Fact]
    public void TheLevelOffsetSinksTheGroundBelowTheModifier()
    {
        // MWL_RuinsWell1 levels to Position + up * -2. If the offset were
        // dropped the well would sit flush with the meadow instead of in a dip,
        // and the structure around it would be two metres out.
        Ground ground = new Ground((x, y) => 30f);
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(30f), level: true, levelRadius: 4f, levelOffset: -2f),
            zone, ground.At);

        TerrainConversion.WorldToVertex(zone, Centre(30f), out int cx, out int cy);
        Assert.Equal(28f, ground.AsReceived(zone, cx, cy), 3);
    }

    [Fact]
    public void LevellingIsClampedTheWayVanillaClampsIt()
    {
        // A 20 m cut is not a 20 m cut: TerrainComp clamps accumulated levelling
        // to +/-8 m, so the server and the client agree on a limited change
        // rather than the server believing in one the client cannot represent.
        Ground ground = new Ground((x, y) => 50f);
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(30f), level: true, levelRadius: 3f, square: true),
            zone, ground.At);

        TerrainConversion.WorldToVertex(zone, Centre(30f), out int cx, out int cy);
        Assert.Equal(-TerrainConversion.MaxLevelDelta, zone.LevelDelta[zone.Index(cx, cy)], 4);
        Assert.Equal(42f, ground.AsReceived(zone, cx, cy), 3);
    }

    // --- smoothing -------------------------------------------------------

    [Fact]
    public void SmoothingPullsTowardsTheModifierAndFadesToNothingAtTheRim()
    {
        Ground ground = new Ground((x, y) => 20f);
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(21f), smooth: true, smoothRadius: 4f, smoothPower: 3f),
            zone, ground.At);

        TerrainConversion.WorldToVertex(zone, Centre(21f), out int cx, out int cy);
        int centre = zone.Index(cx, cy);

        // At the centre the distance is 0, so the lerp is the whole way.
        Assert.Equal(1f, zone.SmoothDelta[centre], 3);
        // At the rim it is 0, so nothing moves even though the vertex is marked.
        Assert.Equal(0f, zone.SmoothDelta[zone.Index(cx + 4, cy)], 3);
        // In between, something less than the whole way.
        float half = zone.SmoothDelta[zone.Index(cx + 2, cy)];
        Assert.InRange(half, 0.01f, 0.99f);
    }

    [Fact]
    public void SmoothingIsClampedToOneMetre()
    {
        Ground ground = new Ground((x, y) => 20f);
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(40f), smooth: true, smoothRadius: 4f, smoothPower: 3f),
            zone, ground.At);

        TerrainConversion.WorldToVertex(zone, Centre(40f), out int cx, out int cy);
        Assert.Equal(TerrainConversion.MaxSmoothDelta, zone.SmoothDelta[zone.Index(cx, cy)], 4);
    }

    [Fact]
    public void LevellingAbsorbsSmoothingRatherThanAddingToIt()
    {
        // Vanilla's LevelTerrain takes the smooth delta into its own and zeroes
        // it. Applying them the other way round, or adding both, leaves the
        // client a metre off from what the author shaped.
        Ground ground = new Ground((x, y) => 20f);
        TerrainZoneDeltas zone = Zone();

        TerrainConversion.Apply(
            new LocationTerrainOperation(
                Centre(25f),
                level: true, levelRadius: 3f, square: true,
                smooth: true, smoothRadius: 3f, smoothPower: 3f),
            zone, ground.At);

        TerrainConversion.WorldToVertex(zone, Centre(25f), out int cx, out int cy);
        int centre = zone.Index(cx, cy);

        // Level ran first and left nothing for smooth to absorb, so smooth's own
        // contribution is still there -- but the level delta is the plain
        // difference, not the difference plus a metre.
        Assert.Equal(5f, zone.LevelDelta[centre], 3);
    }

    [Fact]
    public void WritingTheSameOperationTwiceDisplacesTheGroundTwice()
    {
        // Vanilla's arithmetic is not idempotent here, and it matters. The level
        // delta is the difference between the target and the height the CLIENT
        // generates for itself, which does not change between passes; a hoe in
        // the game escapes this only because the heightmap is rebuilt with the
        // first operation before the second runs. A bake has no such rebuild, so
        // a site written twice sinks twice as far. Pinned rather than hoped for.
        Ground ground = new Ground((x, y) => 30f);
        LocationTerrainOperation op = new(Centre(30f), level: true, levelRadius: 3f, levelOffset: -2f, square: true);

        TerrainZoneDeltas once = Zone();
        TerrainConversion.Apply(op, once, ground.At);

        TerrainZoneDeltas twice = Zone();
        TerrainConversion.Apply(op, twice, ground.At);
        TerrainConversion.Apply(op, twice, ground.At);

        TerrainConversion.WorldToVertex(once, Centre(30f), out int cx, out int cy);
        Assert.Equal(28f, ground.AsReceived(once, cx, cy), 3);
        Assert.Equal(26f, ground.AsReceived(twice, cx, cy), 3);
    }

    [Fact]
    public void ApplyOnceRefusesToWriteTheSameOperationAgain()
    {
        // Which is why a bake goes through ApplyOnce: re-running a site's
        // terrain after a restart, a retry or a second pass over the same zone
        // must leave the ground where it already is.
        Ground ground = new Ground((x, y) => 30f);
        TerrainZoneDeltas zone = Zone();
        LocationTerrainOperation op = new(Centre(30f), level: true, levelRadius: 3f, levelOffset: -2f, square: true);

        Assert.True(TerrainConversion.ApplyOnce("site7/Terrain1", op, zone, ground.At));
        Assert.False(TerrainConversion.ApplyOnce("site7/Terrain1", op, zone, ground.At));
        Assert.True(zone.HasApplied("site7/Terrain1"));

        TerrainConversion.WorldToVertex(zone, Centre(30f), out int cx, out int cy);
        Assert.Equal(28f, ground.AsReceived(zone, cx, cy), 3);
    }

    [Fact]
    public void ApplyOnceStillWritesADifferentOperationInTheSameZone()
    {
        // Two modifiers of one site, or two sites in one zone, are different
        // work: the guard is per operation, not per zone.
        Ground ground = new Ground((x, y) => 30f);
        TerrainZoneDeltas zone = Zone();
        LocationTerrainOperation op = new(Centre(30f), level: true, levelRadius: 3f, levelOffset: -1f, square: true);

        Assert.True(TerrainConversion.ApplyOnce("site7/Terrain1", op, zone, ground.At));
        Assert.True(TerrainConversion.ApplyOnce("site7/Terrain2", op, zone, ground.At));

        TerrainConversion.WorldToVertex(zone, Centre(30f), out int cx, out int cy);
        Assert.Equal(28f, ground.AsReceived(zone, cx, cy), 3);
    }

    [Fact]
    public void ApplyOnceRefusesAnOperationWithNoIdentity()
    {
        Ground ground = new Ground((x, y) => 30f);
        Assert.Throws<ArgumentException>(() =>
            TerrainConversion.ApplyOnce("", new LocationTerrainOperation(Centre(30f)), Zone(), ground.At));
    }

    // --- paint -----------------------------------------------------------

    [Fact]
    public void PaintingMovesTheMaskTowardsTheGamesOwnColour()
    {
        Ground ground = new Ground((x, y) => 20f);
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(
            new LocationTerrainOperation(
                Centre(20f), paint: true, paintRadius: 3f,
                paintType: TerrainModifier.PaintType.Dirt),
            zone, ground.At);

        TerrainConversion.WorldToVertexMask(zone, new Vector3(-0.5f, 20f, -0.5f), out int cx, out int cy);
        int centre = zone.Index(cx, cy);
        Assert.True(zone.ModifiedPaint[centre]);
        Assert.Equal(Heightmap.m_paintMaskDirt.r, zone.PaintMask[centre].r, 3);
    }

    [Fact]
    public void PaintingKeepsTheExistingAlphaUnlessItIsClearingVegetation()
    {
        // The alpha channel is the cleared-vegetation channel. Dirt must not
        // quietly clear the grass; ClearVegetation must.
        Ground ground = new Ground((x, y) => 20f);

        TerrainZoneDeltas dirt = Zone();
        for (int i = 0; i < dirt.PaintMask.Length; i++) dirt.PaintMask[i] = new Color(0f, 0f, 0f, 1f);
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(20f), paint: true, paintRadius: 3f,
                paintType: TerrainModifier.PaintType.Dirt),
            dirt, ground.At);

        TerrainZoneDeltas cleared = Zone();
        for (int i = 0; i < cleared.PaintMask.Length; i++) cleared.PaintMask[i] = new Color(0f, 0f, 0f, 1f);
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(20f), paint: true, paintRadius: 3f,
                paintType: TerrainModifier.PaintType.ClearVegetation),
            cleared, ground.At);

        TerrainConversion.WorldToVertexMask(dirt, new Vector3(-0.5f, 20f, -0.5f), out int cx, out int cy);
        int centre = dirt.Index(cx, cy);
        Assert.Equal(1f, dirt.PaintMask[centre].a, 3);
        Assert.True(cleared.PaintMask[centre].a < 1f, "clearing vegetation has to move the alpha");
    }

    [Fact]
    public void ThePaintGridAgreesWithTheHeightGridOnAZoneAndNotInGeneral()
    {
        // Heightmap has two mappings, and they are written differently:
        // WorldToVertex adds m_width/2 outside the floor, WorldToVertexMask adds
        // (m_width+1)/2 inside it. For an even width the integer division makes
        // both halves the same number and the two agree exactly -- which is the
        // case on a zone heightmap, m_width 32. They part company on an odd
        // width. Pinned in both directions so the conversion is not resting on a
        // coincidence nobody wrote down.
        Vector3 pos = new Vector3(0.2f, 0f, 3.7f);

        TerrainZoneDeltas even = new TerrainZoneDeltas(new Vector3(0f, 0f, 0f), 32, Scale);
        TerrainConversion.WorldToVertex(even, pos, out int vx, out int vy);
        TerrainConversion.WorldToVertexMask(even, pos, out int mx, out int my);
        Assert.Equal((vx, vy), (mx, my));

        TerrainZoneDeltas odd = new TerrainZoneDeltas(new Vector3(0f, 0f, 0f), 33, Scale);
        TerrainConversion.WorldToVertex(odd, pos, out int ovx, out int ovy);
        TerrainConversion.WorldToVertexMask(odd, pos, out int omx, out int omy);
        Assert.NotEqual((ovx, ovy), (omx, omy));
    }

    [Fact]
    public void PaintUsesTheMaskMappingAndNotTheHeightMapping()
    {
        // On a zone the two mappings agree, so a conversion that used the wrong
        // one would look right and no test on a zone could tell. Checked on an
        // odd width, where they part company, so the choice is actually
        // defended rather than assumed.
        const int odd = 33;
        Ground flat = new Ground((x, y) => 20f);
        TerrainZoneDeltas zone = new TerrainZoneDeltas(new Vector3(0f, 0f, 0f), odd, Scale);

        // A radius under one vertex, so exactly one texel is painted and the
        // test can say which.
        TerrainConversion.Apply(
            new LocationTerrainOperation(Centre(20f), paint: true, paintRadius: 0.4f,
                paintType: TerrainModifier.PaintType.Dirt),
            zone, flat.At);

        Vector3 offsetPos = new Vector3(-0.5f, 20f, -0.5f);
        TerrainConversion.WorldToVertexMask(zone, offsetPos, out int mx, out int my);
        TerrainConversion.WorldToVertex(zone, offsetPos, out int vx, out int vy);
        Assert.NotEqual((mx, my), (vx, vy));

        // The scanned block is centred on the mapping the conversion used, so
        // compare the block's centre rather than a single texel: at radius 0.4
        // the block is 3x3 and the two candidate centres are one apart, so both
        // would show a painted texel either way.
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        for (int y = 0; y < zone.Pitch; y++)
            for (int x = 0; x < zone.Pitch; x++)
                if (zone.ModifiedPaint[zone.Index(x, y)])
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }

        Assert.Equal((mx, my), ((minX + maxX) / 2, (minY + maxY) / 2));
    }

    // --- what the operation claims to reach -------------------------------

    [Fact]
    public void TheRadiusIsTheLargestEnabledPart()
    {
        // The planner uses this to decide which zones a site touches. Counting a
        // disabled part would bake zones nothing writes to; missing an enabled
        // one would leave a site half-shaped at a zone boundary.
        LocationTerrainOperation op = new(
            Centre(20f),
            level: true, levelRadius: 3f,
            smooth: false, smoothRadius: 40f,
            paint: true, paintRadius: 6f);
        Assert.Equal(6f, op.Radius, 4);
    }

    [Fact]
    public void AnOperationThatDoesNothingTouchesNothing()
    {
        Ground ground = Slope();
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(new LocationTerrainOperation(Centre(25f)), zone, ground.At);

        Assert.All(zone.ModifiedHeight, Assert.False);
        Assert.All(zone.ModifiedPaint, Assert.False);
        Assert.Equal(0f, op0Sum(zone.LevelDelta), 5);
        Assert.Equal(0f, op0Sum(zone.SmoothDelta), 5);
    }

    private static float op0Sum(float[] values)
    {
        float total = 0f;
        foreach (float v in values) total += Mathf.Abs(v);
        return total;
    }

    // --- the zone boundary ------------------------------------------------

    [Fact]
    public void AnOperationOutsideTheZoneWritesNothingIntoIt()
    {
        // A site near a boundary is applied once per affected zone with the same
        // operation; the zone that it does not reach must come away untouched
        // rather than with a stripe along its edge.
        Ground ground = Slope();
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Apply(
            new LocationTerrainOperation(new Vector3(200f, 25f, 200f), level: true, levelRadius: 4f, square: true),
            zone, ground.At);

        Assert.All(zone.ModifiedHeight, Assert.False);
    }

    [Fact]
    public void AnOperationStraddlingTheEdgeWritesOnlyTheVerticesInside()
    {
        // Half in, half out: vanilla's bounds check drops the outside half
        // rather than wrapping it to the far side of the grid.
        Ground ground = Slope();
        TerrainZoneDeltas zone = Zone();
        // The grid's -x edge in world terms: origin.x - width/2 * scale.
        float edgeX = -(Width / 2) * Scale;
        TerrainConversion.Apply(
            new LocationTerrainOperation(new Vector3(edgeX, 25f, 0f), level: true, levelRadius: 3f, square: true),
            zone, ground.At);

        int touched = 0;
        foreach (bool modified in zone.ModifiedHeight) if (modified) touched++;

        Assert.InRange(touched, 1, 7 * 7 - 1);
        TerrainConversion.WorldToVertex(zone, new Vector3(edgeX, 25f, 0f), out int cx, out int cy);
        Assert.Equal(0, cx);
        Assert.True(zone.ModifiedHeight[zone.Index(cx, cy)], "the centre vertex is inside");
    }
}
