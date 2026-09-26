using System.Collections.Generic;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// What the site terrain conversion does to road terrain already in the same
/// zone's compiler, and what it leaves alone.
///
/// The other writer is ProceduralRoads' server-side terrain write. The two mods
/// build into separate assemblies and neither references the other, so the road
/// is represented here by the state it leaves in the compiler — which is all
/// this mod can see: an absolute <c>m_levelDelta</c> from the world generator's
/// height, a zeroed <c>m_smoothDelta</c>, <c>m_modifiedHeight</c> set, and the
/// paved colour blended into <c>m_paintMask</c> with <c>m_modifiedPaint</c> set,
/// along the road's vertices. That is what <c>RoadTerrainModifier</c> writes
/// (Roads, Src/Roads/RoadTerrainModifier.cs), and it is the same shape this mod
/// writes. So these are tests about a shared compiler, not tests of Roads; the
/// mirror pair — the road writer over site terrain — lives in that mod's suite
/// as MwlCoexistenceTests.
/// </summary>
public class RoadsCoexistenceTests
{
    private const int Width = 32;
    private const float Scale = 1f;
    private const int Centre = 16;

    private static TerrainZoneDeltas Zone() => new TerrainZoneDeltas(Vector3.zero, Width, Scale);

    /// <summary>Flat ground, so every number below is checkable by hand.</summary>
    private static TerrainConversion.VertexHeight Flat(float height) => (x, y) => height;

    /// <summary>
    /// A road's terrain as it sits in the compiler: a strip of vertices at
    /// y = <paramref name="row"/>, each stating an absolute delta and painted
    /// paved. Returns the indices written.
    /// </summary>
    private static List<int> WriteRoad(TerrainZoneDeltas zone, int row, float delta)
    {
        var written = new List<int>();
        for (int x = 0; x < zone.Pitch; x++)
        {
            for (int y = row - 2; y <= row + 2; y++)
            {
                if (!zone.Inside(x, y)) continue;
                int i = zone.Index(x, y);
                zone.LevelDelta[i] = delta;
                zone.SmoothDelta[i] = 0f;
                zone.ModifiedHeight[i] = true;
                zone.PaintMask[i] = Heightmap.m_paintMaskPaved;
                zone.ModifiedPaint[i] = true;
                written.Add(i);
            }
        }
        return written;
    }

    /// <summary>A site that levels a disc to its own height: one ordinary modifier.</summary>
    private static IReadOnlyList<LocationTerrainOperation> Site(float x, float z, float height, float radius) =>
        new[]
        {
            new LocationTerrainOperation(
                new Vector3(x, height, z), level: true, levelRadius: radius,
                paint: true, paintRadius: radius, paintStrength: 1f,
                paintType: TerrainModifier.PaintType.Dirt)
        };

    // ---- disjoint footprints in one zone ---------------------------------

    /// <summary>
    /// A road along one edge of the zone and a site near the other. The
    /// conversion states deltas only at the vertices its own modifiers reach,
    /// so the road's ground and paint come through untouched.
    /// </summary>
    [Fact]
    public void ConversionKeepsRoadTerrainItDoesNotReach()
    {
        TerrainZoneDeltas zone = Zone();
        List<int> road = WriteRoad(zone, row: 4, delta: 1.5f);

        Assert.True(TerrainConversion.ApplyOnce(
            "site@0,0", Site(0f, 10f, height: 22f, radius: 4f), zone, Flat(20f), out var result));
        Assert.True(result.VerticesChanged > 0, "the site wrote nothing, so this proves nothing");

        foreach (int i in road)
        {
            Assert.True(zone.ModifiedHeight[i], $"road vertex {i} lost its modified flag");
            Assert.Equal(1.5f, zone.LevelDelta[i]);
            Assert.Equal(0f, zone.SmoothDelta[i]);
            Assert.Equal(Heightmap.m_paintMaskPaved.b, zone.PaintMask[i].b);
            Assert.True(zone.ModifiedPaint[i], $"road texel {i} lost its painted flag");
        }
        Assert.Empty(result.ContestedVertices);
    }

    // ---- the footprints intersect ----------------------------------------

    /// <summary>
    /// Where the two overlap, the site's own ground wins and the conversion
    /// NAMES the vertices it took: that is what <c>ContestedVertices</c> is for.
    /// Recorded rather than endorsed — the result is still <c>Representable</c>,
    /// so <c>ApplyOnce</c> publishes it and the caller has to read the report.
    /// </summary>
    [Fact]
    public void ConversionReportsTheVerticesItTakesFromTheRoadAndStillWritesThem()
    {
        TerrainZoneDeltas zone = Zone();
        WriteRoad(zone, row: Centre, delta: 1.5f);

        Assert.True(TerrainConversion.ApplyOnce(
            "site@0,0", Site(0f, 0f, height: 22f, radius: 4f), zone, Flat(20f), out var result));

        Assert.NotEmpty(result.ContestedVertices);
        Assert.Contains(Centre + "," + Centre, result.ContestedVertices);

        // The site's height, not the road's, at the shared centre vertex.
        int i = zone.Index(Centre, Centre);
        Assert.Equal(2f, zone.LevelDelta[i], 3);

        // And it is servable, so nothing refuses it: contention is a report.
        Assert.True(result.Representable);
    }

    /// <summary>
    /// The gap that a height-only check leaves. A site whose modifier only
    /// paints, over a road's paved texels, changes the road's paint and reports
    /// no contention at all — <c>ContestedVertices</c> is derived from the
    /// height arrays. Pinned so the omission is a known one rather than a
    /// surprise at a shared site.
    /// </summary>
    [Fact]
    public void PaintOverlapIsNotReportedAsContention()
    {
        TerrainZoneDeltas zone = Zone();
        WriteRoad(zone, row: Centre, delta: 0f);
        // Level deltas of 0 with the flag set: paint without height, so nothing
        // in the height arrays can be contested.
        int i = zone.Index(Centre, Centre);
        Color paved = zone.PaintMask[i];

        var paintOnly = new[]
        {
            new LocationTerrainOperation(
                new Vector3(0f, 20f, 0f), paint: true, paintRadius: 4f, paintStrength: 1f,
                paintType: TerrainModifier.PaintType.Dirt)
        };
        Assert.True(TerrainConversion.ApplyOnce("paint@0,0", paintOnly, zone, Flat(20f), out var result));

        Assert.NotEqual(paved.r, zone.PaintMask[i].r);
        Assert.Empty(result.ContestedVertices);
    }

    /// <summary>
    /// <c>TexelsPainted</c> counts every painted texel in the ZONE, not the
    /// site's own: a road's texels are already flagged when the site arrives, so
    /// the number in the writer's log is larger than what the site did. A
    /// diagnostic, not a decision — but a diagnostic that would be misread at a
    /// shared site, so it is pinned.
    /// </summary>
    [Fact]
    public void TexelsPaintedCountsTheWholeZoneNotTheSite()
    {
        TerrainZoneDeltas alone = Zone();
        Assert.True(TerrainConversion.ApplyOnce(
            "site@0,0", Site(0f, 0f, 22f, 4f), alone, Flat(20f), out var solo));

        TerrainZoneDeltas shared = Zone();
        List<int> road = WriteRoad(shared, row: 4, delta: 1.5f);
        Assert.True(TerrainConversion.ApplyOnce(
            "site@0,0", Site(0f, 0f, 22f, 4f), shared, Flat(20f), out var withRoad));

        Assert.True(withRoad.TexelsPainted > solo.TexelsPainted,
            "the road's texels are not being counted, so this test is stale");
        Assert.Equal(solo.TexelsPainted + road.Count, withRoad.TexelsPainted);
    }

    // ---- repeats ----------------------------------------------------------

    /// <summary>
    /// A site already written into this zone is not written again, and the road
    /// that arrived in between is left exactly as it is. This is the case that
    /// makes a restart safe: the completion record is per site, so a road laid
    /// after the site does not make the site look outstanding.
    /// </summary>
    [Fact]
    public void ARepeatIsInertAndLeavesRoadTerrainAlone()
    {
        TerrainZoneDeltas zone = Zone();
        Assert.True(TerrainConversion.ApplyOnce(
            "site@0,0", Site(0f, 0f, 22f, 4f), zone, Flat(20f), out _));

        // The road arrives afterwards, across the site.
        List<int> road = WriteRoad(zone, row: Centre, delta: 1.5f);

        Assert.False(TerrainConversion.ApplyOnce(
            "site@0,0", Site(0f, 0f, 22f, 4f), zone, Flat(20f), out var again));
        Assert.True(again.Representable, "an inert repeat must not look like a refusal");
        Assert.Equal(0, again.VerticesChanged);

        foreach (int i in road)
        {
            Assert.Equal(1.5f, zone.LevelDelta[i]);
            Assert.Equal(Heightmap.m_paintMaskPaved.b, zone.PaintMask[i].b);
        }
    }

    // ---- through the wire -------------------------------------------------

    /// <summary>
    /// Both writers' work survives the serialization the compiler's ZDO uses.
    /// The blob is what a stock client actually receives, so a zone that holds
    /// both has to come back holding both.
    /// </summary>
    [Fact]
    public void BothWritersSurviveTheCompilerRoundTrip()
    {
        TerrainZoneDeltas zone = Zone();
        List<int> road = WriteRoad(zone, row: 4, delta: 1.5f);
        Assert.True(TerrainConversion.ApplyOnce(
            "site@0,0", Site(0f, 10f, 22f, 4f), zone, Flat(20f), out _));

        var siteVertices = new List<int>();
        for (int i = 0; i < zone.ModifiedHeight.Length; i++)
            if (zone.ModifiedHeight[i] && !road.Contains(i))
                siteVertices.Add(i);
        Assert.NotEmpty(siteVertices);

        byte[] blob = TerrainBlob.Encode(zone, TerrainBlob.Header.Fresh);
        TerrainZoneDeltas back = Zone();
        Assert.True(TerrainBlob.TryDecode(blob, back, out _, out string problem), problem);

        foreach (int i in road)
        {
            Assert.True(back.ModifiedHeight[i]);
            Assert.Equal(zone.LevelDelta[i], back.LevelDelta[i]);
            Assert.Equal(zone.PaintMask[i].b, back.PaintMask[i].b);
        }
        foreach (int i in siteVertices)
        {
            Assert.True(back.ModifiedHeight[i]);
            Assert.Equal(zone.LevelDelta[i], back.LevelDelta[i]);
        }
    }
}
