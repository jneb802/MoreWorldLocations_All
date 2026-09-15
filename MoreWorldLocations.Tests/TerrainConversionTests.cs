using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Whether a client without the mod ends up standing on the ground the author
/// drew.
///
/// The review of 9657742 made the point these are written around: copying
/// <c>TerrainComp</c>'s arithmetic reproduces a hoe, not a location. A location's
/// modifiers run through <c>Heightmap.ApplyModifiers</c>, which walks them over a
/// mutating height array — so a modifier's smooth pass reads ground its own level
/// pass has already flattened, and a second modifier reads the first one's
/// result. The compiler's passes both read the height from before the operation.
/// The two agree on a level-only modifier and disagree on every other kind, and
/// MWL uses the other kinds.
///
/// So the expected numbers below are worked out by hand from
/// <c>Heightmap.LevelTerrain</c> and <c>SmoothTerrain2</c> in the decompiled 1.0
/// assembly, not read back from the converter. Where a number is derived, the
/// derivation is written next to it.
/// </summary>
public class TerrainConversionTests
{
    private const int Width = 32;
    private const float Scale = 1f;

    /// <summary>The centre vertex of a zone whose origin is the world origin.</summary>
    private const int Centre = 16;

    private static TerrainZoneDeltas Zone() =>
        new TerrainZoneDeltas(new Vector3(0f, 0f, 0f), Width, Scale);

    private static Vector3 At(float x, float height, float z) => new Vector3(x, height, z);

    /// <summary>Ground the client generates for itself: flat, so the arithmetic is checkable by hand.</summary>
    private static TerrainConversion.VertexHeight Flat(float height) => (x, y) => height;

    /// <summary>Ground that rises eastwards, for the cases where flat would hide a mistake.</summary>
    private static TerrainConversion.VertexHeight Slope() => (x, y) => 20f + x * 0.25f;

    /// <summary>
    /// What the client's terrain ends up as: the height it generated, plus the
    /// compiler's two deltas, clamped against the generated height — exactly
    /// <c>TerrainComp.ApplyToHeightmap</c>.
    /// </summary>
    private static float AsReceived(TerrainZoneDeltas zone, TerrainConversion.VertexHeight generated, int x, int y)
    {
        int i = zone.Index(x, y);
        float own = generated(x, y);
        if (zone.LevelDelta[i] == 0f && zone.SmoothDelta[i] == 0f)
            return own;
        return Mathf.Clamp(own + zone.LevelDelta[i] + zone.SmoothDelta[i], own - 8f, own + 8f);
    }

    private static IReadOnlyList<LocationTerrainOperation> One(LocationTerrainOperation op) => new[] { op };

    // --- the case that separates the authored path from the compiler's -----

    [Fact]
    public void LevelAndSmoothTogetherLeaveTheLevelledGroundExactlyAtTheTarget()
    {
        // MWL_Ruins1's one modifier is level r=3 AND smooth r=3, and this is
        // where the old approach was wrong. Live: level sets the ground to 25,
        // then smooth lerps from 25 towards 25 and changes nothing. Copying the
        // compiler instead adds a smooth delta computed from the ORIGINAL 30, and
        // the middle of the ruin ends up below its own floor.
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.VertexHeight ground = Flat(30f);

        TerrainConversion.Convert(
            One(new LocationTerrainOperation(
                At(0f, 25f, 0f),
                level: true, levelRadius: 3f, square: true,
                smooth: true, smoothRadius: 3f, smoothPower: 4f)),
            zone, ground);

        for (int y = Centre - 3; y <= Centre + 3; y++)
            for (int x = Centre - 3; x <= Centre + 3; x++)
                Assert.Equal(25f, AsReceived(zone, ground, x, y), 3);
    }

    [Fact]
    public void SmoothingReachesBeyondTheLevelledGroundByHandCheckedAmounts()
    {
        // Flat 30, levelled to 25 within 3 m, smoothed within 6 m with power 3.
        // Heightmap.SmoothTerrain2: n = d/r, w = 1 - n^3, h = lerp(h, 25, w).
        //   d = 4 -> n = 0.6667, n^3 = 0.2963, w = 0.7037, h = 30 - 5*0.7037 = 26.481
        //   d = 6 -> n = 1,      w = 0,        h = 30
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.VertexHeight ground = Flat(30f);

        TerrainConversion.Convert(
            One(new LocationTerrainOperation(
                At(0f, 25f, 0f),
                level: true, levelRadius: 3f, square: true,
                smooth: true, smoothRadius: 6f, smoothPower: 3f)),
            zone, ground);

        Assert.Equal(25f, AsReceived(zone, ground, Centre, Centre), 3);
        Assert.Equal(26.481f, AsReceived(zone, ground, Centre + 4, Centre), 2);
        Assert.Equal(30f, AsReceived(zone, ground, Centre + 6, Centre), 3);
    }

    [Fact]
    public void ALaterModifierOverridesAnEarlierOneWhereTheyOverlap()
    {
        // ApplyModifiers walks the instances in order over the same array, so two
        // modifiers of one site are not independent. Converting them one at a
        // time against the untouched ground -- which is what a per-modifier
        // conversion would do -- loses this entirely.
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.VertexHeight ground = Flat(30f);

        LocationTerrainOperation first = new(
            At(0f, 25f, 0f), level: true, levelRadius: 5f, square: true, sortOrder: 0);
        LocationTerrainOperation second = new(
            At(0f, 27f, 0f), level: true, levelRadius: 2f, square: true, sortOrder: 1);

        TerrainConversion.Convert(new[] { first, second }, zone, ground);

        Assert.Equal(27f, AsReceived(zone, ground, Centre, Centre), 3);      // both, second wins
        Assert.Equal(25f, AsReceived(zone, ground, Centre + 4, Centre), 3);  // first only
        Assert.Equal(30f, AsReceived(zone, ground, Centre + 6, Centre), 3);  // neither
    }

    [Fact]
    public void TheSortOrderDecidesWhichModifierWinsAndNotTheOrderTheyArrivedIn()
    {
        // Vanilla sorts by m_sortOrder before applying. Handing them over in the
        // other order must not change the ground, or the site depends on how the
        // template happened to be walked.
        TerrainConversion.VertexHeight ground = Flat(30f);
        LocationTerrainOperation low = new(
            At(0f, 25f, 0f), level: true, levelRadius: 5f, square: true, sortOrder: 0);
        LocationTerrainOperation high = new(
            At(0f, 27f, 0f), level: true, levelRadius: 2f, square: true, sortOrder: 1);

        TerrainZoneDeltas asGiven = Zone();
        TerrainConversion.Convert(new[] { low, high }, asGiven, ground);

        TerrainZoneDeltas reversed = Zone();
        TerrainConversion.Convert(new[] { high, low }, reversed, ground);

        Assert.Equal(asGiven.LevelDelta, reversed.LevelDelta);
        Assert.Equal(27f, AsReceived(reversed, ground, Centre, Centre), 3);
    }

    [Fact]
    public void ModifiersWithTheSameSortOrderKeepTheOrderTheTemplateGaveThem()
    {
        // Vanilla breaks a sort-order tie by creation order, which for a
        // location's children is their order in the template. Letting the sort
        // pick would make the ground depend on something nobody wrote down.
        TerrainConversion.VertexHeight ground = Flat(30f);
        LocationTerrainOperation first = new(At(0f, 25f, 0f), level: true, levelRadius: 3f, square: true);
        LocationTerrainOperation second = new(At(0f, 27f, 0f), level: true, levelRadius: 3f, square: true);

        TerrainZoneDeltas oneWay = Zone();
        TerrainConversion.Convert(new[] { first, second }, oneWay, ground);
        Assert.Equal(27f, AsReceived(oneWay, ground, Centre, Centre), 3);

        TerrainZoneDeltas otherWay = Zone();
        TerrainConversion.Convert(new[] { second, first }, otherWay, ground);
        Assert.Equal(25f, AsReceived(otherWay, ground, Centre, Centre), 3);
    }

    // --- the ground the client lands on ------------------------------------

    [Fact]
    public void WhatTheClientReceivesIsTheAuthoredGroundVertexForVertex()
    {
        // The whole claim, stated over the whole grid and on ground that is not
        // flat: after the deltas are applied the way the game applies them, the
        // client stands exactly where the author drew.
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.VertexHeight ground = Slope();

        IReadOnlyList<LocationTerrainOperation> site = new[]
        {
            new LocationTerrainOperation(At(-4f, 24f, 0f), level: true, levelRadius: 4f, square: true,
                smooth: true, smoothRadius: 7f, smoothPower: 3f, sortOrder: 0),
            new LocationTerrainOperation(At(3f, 26f, 2f), level: true, levelRadius: 3f, sortOrder: 1),
        };

        float[] authored = TerrainConversion.AuthoredHeights(site, zone, ground);
        TerrainConversion.Convert(site, zone, ground);

        for (int y = 0; y < zone.Pitch; y++)
            for (int x = 0; x < zone.Pitch; x++)
                Assert.Equal(authored[zone.Index(x, y)], AsReceived(zone, ground, x, y), 3);
    }

    [Fact]
    public void GroundTheSiteDoesNotReachIsLeftWhereTheWorldGeneratorPutIt()
    {
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.VertexHeight ground = Slope();

        TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 25f, 0f), level: true, levelRadius: 3f, square: true)),
            zone, ground);

        Assert.False(zone.ModifiedHeight[zone.Index(Centre + 5, Centre)]);
        Assert.Equal(ground(Centre + 5, Centre), AsReceived(zone, ground, Centre + 5, Centre), 4);
    }

    [Fact]
    public void ARoundFootprintDoesNotTouchItsCorners()
    {
        // square=false is a circle in vanilla, and MWL_RuinsWell1 uses one.
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 25f, 0f), level: true, levelRadius: 4f, square: false)),
            zone, Flat(30f));

        Assert.True(zone.ModifiedHeight[zone.Index(Centre + 4, Centre)],
            "the rim on the axis is inside a radius-4 circle");
        Assert.False(zone.ModifiedHeight[zone.Index(Centre + 4, Centre + 4)],
            "the corner of the box is outside it");
    }

    [Fact]
    public void TheLevelOffsetSinksTheGroundBelowTheModifier()
    {
        // MWL_RuinsWell1 levels to Position + up * -2. Drop the offset and the
        // well sits flush with the meadow, two metres above its own floor.
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.VertexHeight ground = Flat(30f);
        TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 30f, 0f), level: true, levelRadius: 4f, levelOffset: -2f)),
            zone, ground);

        Assert.Equal(28f, AsReceived(zone, ground, Centre, Centre), 3);
    }

    // --- what the compiler cannot hold -------------------------------------

    [Fact]
    public void ACutDeeperThanTheCompilerCanHoldIsReportedRatherThanApproximated()
    {
        // Heightmap.LevelTerrain sets a location's ground absolutely with no
        // limit; a compiler delta is clamped to +/-8 m. A template that needs 20
        // cannot be served as authored, and saying so is the difference between
        // excluding it and shipping a ruin sitting twelve metres in the air.
        TerrainZoneDeltas zone = Zone();
        TerrainConversionResult result = TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 30f, 0f), level: true, levelRadius: 3f, square: true)),
            zone, Flat(50f));

        Assert.False(result.Representable);
        Assert.NotEmpty(result.BeyondCompilerRange);
        Assert.Contains("needs -20.0 m", result.BeyondCompilerRange[0]);
        Assert.Equal(-20f, result.LargestChange, 3);
    }

    [Fact]
    public void AnOrdinaryCutIsWithinRangeAndSaysSo()
    {
        TerrainZoneDeltas zone = Zone();
        TerrainConversionResult result = TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 28f, 0f), level: true, levelRadius: 3f, square: true)),
            zone, Flat(30f));

        Assert.True(result.Representable);
        Assert.Empty(result.BeyondCompilerRange);
        Assert.Equal(-2f, result.LargestChange, 3);
        Assert.Equal(7 * 7, result.VerticesChanged);
    }

    [Fact]
    public void PreviewAnswersTheSameQuestionWithoutWritingAnything()
    {
        // How a template is judged before anything is committed to a world.
        TerrainZoneDeltas zone = Zone();
        TerrainConversionResult preview = TerrainConversion.Preview(
            One(new LocationTerrainOperation(At(0f, 30f, 0f), level: true, levelRadius: 3f, square: true)),
            zone, Flat(50f));

        Assert.False(preview.Representable);
        Assert.All(zone.ModifiedHeight, Assert.False);
        Assert.Empty(zone.AppliedOperations);
    }

    // --- two writers on one vertex ----------------------------------------

    [Fact]
    public void GroundAnotherWriterHadAlreadyMovedIsReportedAsContested()
    {
        // A road and a site wanting the same vertex is a planning question, not
        // something to average. The site's ground wins and the collision is
        // named, so it can be seen rather than discovered by standing on it.
        TerrainZoneDeltas zone = Zone();
        zone.LevelDelta[zone.Index(Centre, Centre)] = 1.5f;
        zone.ModifiedHeight[zone.Index(Centre, Centre)] = true;

        TerrainConversion.VertexHeight ground = Flat(30f);
        TerrainConversionResult result = TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 28f, 0f), level: true, levelRadius: 3f, square: true)),
            zone, ground);

        Assert.Contains(Centre + "," + Centre, result.ContestedVertices);
        Assert.Equal(28f, AsReceived(zone, ground, Centre, Centre), 3);
    }

    [Fact]
    public void AZoneNobodyElseTouchedHasNothingContested()
    {
        TerrainConversionResult result = TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 28f, 0f), level: true, levelRadius: 3f, square: true)),
            Zone(), Flat(30f));

        Assert.Empty(result.ContestedVertices);
    }

    // --- paint -------------------------------------------------------------

    [Fact]
    public void PaintingMovesTheMaskTowardsTheGamesOwnColour()
    {
        TerrainZoneDeltas zone = Zone();
        TerrainConversionResult result = TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 30f, 0f), paint: true, paintRadius: 3f,
                paintType: TerrainModifier.PaintType.Dirt)),
            zone, Flat(30f));

        TerrainConversion.WorldToVertexMask(zone, At(-0.5f, 30f, -0.5f), out int cx, out int cy);
        int centre = zone.Index(cx, cy);
        Assert.True(zone.ModifiedPaint[centre]);
        Assert.Equal(Heightmap.m_paintMaskDirt.r, zone.PaintMask[centre].r, 3);
        Assert.True(result.TexelsPainted > 0);
        Assert.Equal(0, result.VerticesChanged);
    }

    [Fact]
    public void PaintingKeepsTheExistingAlphaUnlessItIsClearingVegetation()
    {
        // The alpha channel is the cleared-vegetation channel. Dirt must not
        // quietly clear the grass; ClearVegetation must.
        TerrainZoneDeltas dirt = Zone();
        for (int i = 0; i < dirt.PaintMask.Length; i++) dirt.PaintMask[i] = new Color(0f, 0f, 0f, 1f);
        TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 30f, 0f), paint: true, paintRadius: 3f,
                paintType: TerrainModifier.PaintType.Dirt)),
            dirt, Flat(30f));

        TerrainZoneDeltas cleared = Zone();
        for (int i = 0; i < cleared.PaintMask.Length; i++) cleared.PaintMask[i] = new Color(0f, 0f, 0f, 1f);
        TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 30f, 0f), paint: true, paintRadius: 3f,
                paintType: TerrainModifier.PaintType.ClearVegetation)),
            cleared, Flat(30f));

        TerrainConversion.WorldToVertexMask(dirt, At(-0.5f, 30f, -0.5f), out int cx, out int cy);
        int centre = dirt.Index(cx, cy);
        Assert.Equal(1f, dirt.PaintMask[centre].a, 3);
        Assert.True(cleared.PaintMask[centre].a < 1f, "clearing vegetation has to move the alpha");
    }

    [Fact]
    public void PaintUsesTheMaskMappingAndNotTheHeightMapping()
    {
        // On a zone the two mappings agree, so a conversion using the wrong one
        // would look right and no test on a zone could tell. Checked on an odd
        // width, where they part company.
        const int odd = 33;
        TerrainZoneDeltas zone = new TerrainZoneDeltas(new Vector3(0f, 0f, 0f), odd, Scale);

        // A radius under one vertex, so the painted block is 3x3 and its centre
        // says which mapping was used; a single texel would not, since the two
        // candidate centres are only one apart.
        TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(0f, 20f, 0f), paint: true, paintRadius: 0.4f,
                paintType: TerrainModifier.PaintType.Dirt)),
            zone, Flat(20f));

        Vector3 offsetPos = At(-0.5f, 20f, -0.5f);
        TerrainConversion.WorldToVertexMask(zone, offsetPos, out int mx, out int my);
        TerrainConversion.WorldToVertex(zone, offsetPos, out int vx, out int vy);
        Assert.NotEqual((mx, my), (vx, vy));

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

    [Fact]
    public void ThePaintGridAgreesWithTheHeightGridOnAZoneAndNotInGeneral()
    {
        // WorldToVertex adds m_width/2 outside the floor; WorldToVertexMask adds
        // (m_width+1)/2 inside it. For an even width the integer division makes
        // both halves the same number. Pinned in both directions so the
        // conversion is not resting on a coincidence nobody wrote down.
        Vector3 pos = At(0.2f, 0f, 3.7f);

        TerrainZoneDeltas even = new TerrainZoneDeltas(new Vector3(0f, 0f, 0f), 32, Scale);
        TerrainConversion.WorldToVertex(even, pos, out int vx, out int vy);
        TerrainConversion.WorldToVertexMask(even, pos, out int mx, out int my);
        Assert.Equal((vx, vy), (mx, my));

        TerrainZoneDeltas odd = new TerrainZoneDeltas(new Vector3(0f, 0f, 0f), 33, Scale);
        TerrainConversion.WorldToVertex(odd, pos, out int ovx, out int ovy);
        TerrainConversion.WorldToVertexMask(odd, pos, out int omx, out int omy);
        Assert.NotEqual((ovx, ovy), (omx, omy));
    }

    // --- what the operation claims to reach --------------------------------

    [Fact]
    public void TheRadiusIsTheLargestEnabledPart()
    {
        // The planner uses this to decide which zones a site touches. Counting a
        // disabled part would bake zones nothing writes to; missing an enabled
        // one would leave a site half-shaped at a boundary.
        LocationTerrainOperation op = new(
            At(0f, 20f, 0f),
            level: true, levelRadius: 3f,
            smooth: false, smoothRadius: 40f,
            paint: true, paintRadius: 6f);
        Assert.Equal(6f, op.Radius, 4);
    }

    [Fact]
    public void AnOperationThatDoesNothingTouchesNothing()
    {
        TerrainZoneDeltas zone = Zone();
        TerrainConversionResult result =
            TerrainConversion.Convert(One(new LocationTerrainOperation(At(0f, 25f, 0f))), zone, Slope());

        Assert.All(zone.ModifiedHeight, Assert.False);
        Assert.All(zone.ModifiedPaint, Assert.False);
        Assert.Equal(0, result.VerticesChanged);
        Assert.Equal(0, result.TexelsPainted);
        Assert.True(result.Representable);
    }

    [Fact]
    public void ASiteWithNoModifiersLeavesTheZoneAlone()
    {
        TerrainZoneDeltas zone = Zone();
        TerrainConversionResult result =
            TerrainConversion.Convert(new List<LocationTerrainOperation>(), zone, Slope());

        Assert.Equal(0, result.VerticesChanged);
        Assert.All(zone.ModifiedHeight, Assert.False);
    }

    // --- the zone boundary -------------------------------------------------

    [Fact]
    public void AnOperationOutsideTheZoneWritesNothingIntoIt()
    {
        // A site near a boundary is converted once per affected zone with the
        // same modifiers; a zone it does not reach must come away untouched
        // rather than with a stripe along its edge.
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(200f, 25f, 200f), level: true, levelRadius: 4f, square: true)),
            zone, Slope());

        Assert.All(zone.ModifiedHeight, Assert.False);
    }

    [Fact]
    public void AnOperationStraddlingTheEdgeWritesOnlyTheVerticesInside()
    {
        // Half in, half out: vanilla's bounds check drops the outside half rather
        // than wrapping it to the far side of the grid.
        TerrainZoneDeltas zone = Zone();
        float edgeX = -(Width / 2) * Scale;
        TerrainConversionResult result = TerrainConversion.Convert(
            One(new LocationTerrainOperation(At(edgeX, 25f, 0f), level: true, levelRadius: 3f, square: true)),
            zone, Slope());

        Assert.InRange(result.VerticesChanged, 1, 7 * 7 - 1);
        TerrainConversion.WorldToVertexMask(zone, At(edgeX, 25f, 0f), out int cx, out int cy);
        Assert.Equal(0, cx);
        Assert.True(zone.ModifiedHeight[zone.Index(cx, cy)], "the centre vertex is inside");
    }

    // --- writing it once ---------------------------------------------------

    [Fact]
    public void ApplyOnceWritesTheSiteAndThenRefusesToWriteItAgain()
    {
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.VertexHeight ground = Flat(30f);
        LocationTerrainOperation op = new(At(0f, 30f, 0f), level: true, levelRadius: 3f, levelOffset: -2f,
            square: true, paint: true, paintRadius: 3f);

        Assert.True(TerrainConversion.ApplyOnce("MWL_RuinsWell1@8,12", One(op), zone, ground, out _));
        Assert.False(TerrainConversion.ApplyOnce("MWL_RuinsWell1@8,12", One(op), zone, ground, out _));
        Assert.Equal(28f, AsReceived(zone, ground, Centre, Centre), 3);
    }

    [Fact]
    public void WritingTheSameSiteTwiceWouldNotMoveTheGroundEvenIfItGotThrough()
    {
        // Heights are stated absolutely, so a repeat lands on the same ground
        // rather than sinking the site twice -- the failure the previous
        // accumulating conversion had. The completion record is still what stops
        // the repeat, because paint is a lerp and a second pass leaves a
        // different mask; this is the second lock, not the first.
        TerrainConversion.VertexHeight ground = Flat(30f);
        LocationTerrainOperation op = new(At(0f, 30f, 0f), level: true, levelRadius: 3f,
            levelOffset: -2f, square: true);

        TerrainZoneDeltas once = Zone();
        TerrainConversion.Convert(One(op), once, ground);

        TerrainZoneDeltas twice = Zone();
        TerrainConversion.Convert(One(op), twice, ground);
        TerrainConversion.Convert(One(op), twice, ground);

        Assert.Equal(once.LevelDelta, twice.LevelDelta);
        Assert.Equal(28f, AsReceived(twice, ground, Centre, Centre), 3);
    }

    [Fact]
    public void ApplyOnceRefusesAConversionWithNoIdentity()
    {
        Assert.Throws<ArgumentException>(() =>
            TerrainConversion.ApplyOnce("", One(new LocationTerrainOperation(At(0f, 30f, 0f))),
                Zone(), Flat(30f), out _));
    }

    [Fact]
    public void TwoSitesInOneZoneAreTwoConversions()
    {
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.VertexHeight ground = Flat(30f);

        Assert.True(TerrainConversion.ApplyOnce("siteA@8,12",
            One(new LocationTerrainOperation(At(-6f, 28f, 0f), level: true, levelRadius: 2f, square: true)),
            zone, ground, out _));
        Assert.True(TerrainConversion.ApplyOnce("siteB@8,12",
            One(new LocationTerrainOperation(At(6f, 27f, 0f), level: true, levelRadius: 2f, square: true)),
            zone, ground, out _));

        Assert.Equal(28f, AsReceived(zone, ground, Centre - 6, Centre), 3);
        Assert.Equal(27f, AsReceived(zone, ground, Centre + 6, Centre), 3);
        Assert.Equal(2, zone.AppliedOperations.Count());
    }
}
