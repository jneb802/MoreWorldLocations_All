using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Whether a placement may be published, asked before anything of it exists.
///
/// <para>The failure this closes is the one an operator sees and cannot
/// explain: a hall standing on a hillside it was supposed to be cut into,
/// because the conversion refused the ground AFTER the buildings had already
/// been sent to every client. A faithful site or no site; a stairway two metres
/// above the grass is not a third option.</para>
///
/// <para>These pin the decision, not the hook. Whether Harmony runs the prefix
/// where it is supposed to, and what the game does with a location it did not
/// spawn, needs a running world.</para>
/// </summary>
public class SitePreflightTests
{
    private const int Width = 32;
    private const float Scale = 1f;

    /// <summary>
    /// A zone's deltas, the way the conversion tests build them: 33 vertices a
    /// side around the zone's own position.
    /// </summary>
    private static TerrainZoneDeltas Zone(int x = 0, int z = 0) =>
        new TerrainZoneDeltas(ZoneSystem.GetZonePos(new Vector2s(x, z)), Width, Scale);

    private static TerrainConversion.VertexHeight Flat(float height) => (x, y) => height;

    /// <summary>One levelling operation at a point, at the given depth below the ground.</summary>
    private static LocationTerrainOperation Level(Vector3 at, float level, float radius = 6f) =>
        new LocationTerrainOperation(at, level: true, levelRadius: radius, levelOffset: level,
            square: true, smooth: false, smoothRadius: 0f, smoothPower: 0f,
            paint: false, paintRadius: 0f, paintStrength: 0f,
            paintType: TerrainModifier.PaintType.Dirt, paintHeightCheck: false, sortOrder: 0);

    /// <summary>Ground that answers for every zone, at one height.</summary>
    private static SitePreflight.ZoneGround Ground(float height, params Vector2s[] silent)
    {
        var quiet = new HashSet<Vector2s>(silent);
        return (Vector2s zone, out TerrainZoneDeltas deltas, out TerrainConversion.VertexHeight baseHeightAt) =>
        {
            if (quiet.Contains(zone))
            {
                deltas = null;
                baseHeightAt = null;
                return false;
            }
            deltas = Zone(zone.x, zone.y);
            baseHeightAt = Flat(height);
            return true;
        };
    }

    [Fact]
    public void ASiteWhoseGroundCanBeCutIsServed()
    {
        var operations = new[] { Level(new Vector3(0f, 0f, 0f), 30f) };

        SiteDecision decision = SitePreflight.Decide("site", new Vector2s(0, 0), operations, Ground(31f));

        Assert.Equal(SiteVerdict.Serve, decision.Verdict);
        Assert.True(decision.MayPublish);
    }

    [Fact]
    public void ALocationWithNoTerrainIsNeverRefused()
    {
        // Most templates are this, and asking about them would be asking about
        // ground nobody shapes.
        SiteDecision decision = SitePreflight.Decide(
            "site", new Vector2s(0, 0), new LocationTerrainOperation[0], Ground(30f));

        Assert.Equal(SiteVerdict.Serve, decision.Verdict);
    }

    [Fact]
    public void ASiteReachingTwoZonesAwayIsRefusedBeforeItIsPlaced()
    {
        // The per-zone scheme sees a site's own zone and its eight neighbours.
        // A site reaching further would have the near zones shaped and the far
        // one left as the client generated it -- a step through the middle of
        // the building.
        var operations = new[] { Level(new Vector3(0f, 0f, 0f), 30f, radius: 200f) };

        SiteDecision decision = SitePreflight.Decide("site", new Vector2s(0, 0), operations, Ground(31f));

        Assert.Equal(SiteVerdict.Refuse, decision.Verdict);
        Assert.Equal(SiteRefusalCodes.OutOfReach, decision.Code);
        Assert.False(decision.MayPublish);
    }

    [Fact]
    public void ACutDeeperThanACompilerCanHoldIsRefusedBeforeItIsPlaced()
    {
        // A compiler delta is clamped to ±8 m and the authored path is not, so
        // the site would stand on ground the author never drew. Before this
        // check the buildings were already published when that was discovered.
        var operations = new[] { Level(new Vector3(0f, 0f, 0f), 10f) };

        SiteDecision decision = SitePreflight.Decide("site", new Vector2s(0, 0), operations, Ground(40f));

        Assert.Equal(SiteVerdict.Refuse, decision.Verdict);
        Assert.Equal(SiteRefusalCodes.Unrepresentable, decision.Code);
        // And it says which vertex and how far, not merely that it cannot.
        Assert.Contains("m", decision.Reason);
    }

    [Fact]
    public void GroundAnotherWriterHasAlreadyMovedIsRefusedBeforeItIsPlaced()
    {
        // Two writers on one vertex is a planning question -- a road through the
        // site, or an overlapping location -- and merging them quietly is how a
        // site ends up somewhere neither writer meant.
        TerrainZoneDeltas contested = Zone();
        TerrainConversion.ApplyOnce("a-road", new[] { Level(new Vector3(0f, 0f, 0f), 28f) }, contested, Flat(30f), out _);

        SitePreflight.ZoneGround ground =
            (Vector2s zone, out TerrainZoneDeltas deltas, out TerrainConversion.VertexHeight baseHeightAt) =>
            {
                deltas = contested;
                baseHeightAt = Flat(30f);
                return true;
            };

        SiteDecision decision = SitePreflight.Decide(
            "site", new Vector2s(0, 0), new[] { Level(new Vector3(0f, 0f, 0f), 29f) }, ground);

        Assert.Equal(SiteVerdict.Refuse, decision.Verdict);
        Assert.Equal(SiteRefusalCodes.Contested, decision.Code);
    }
    [Fact]
    public void GroundThatCannotBeReadIsUndecidedAndNothingIsPublished()
    {
        // This used to publish, on the reasoning that "we could not look" is not
        // "the ground is wrong" and refusing would delete locations for a
        // scheduling detail. Both halves true, conclusion wrong: if the ground
        // later turns out to be contested or beyond the compiler's range, the
        // buildings are already standing on it and the whole gap is back. The
        // placement is HELD instead -- see ZoneReadinessBarrier -- so it is
        // neither published nor deleted.
        //
        // Zone 0,0 runs to x = 32, so this straddles the edge into zone 1,0.
        var operations = new[] { Level(new Vector3(30f, 0f, 0f), 30f, radius: 8f) };

        SiteDecision decision = SitePreflight.Decide(
            "site", new Vector2s(0, 0), operations, Ground(31f, new Vector2s(1, 0)));

        Assert.Equal(SiteVerdict.Undecided, decision.Verdict);
        Assert.Equal(SiteRefusalCodes.GroundUnknown, decision.Code);
        Assert.False(decision.MayPublish);
        Assert.Contains("not placed", decision.Reason);
    }

    [Fact]
    public void ASiteWhoseZoneCarriesAnUnreadableCompilerIsNotPublished()
    {
        // A zone whose saved compiler will not decode is the opposite of an
        // empty zone: it says there IS something written there and nobody can
        // read it. Converting over that lands on another writer's ground.
        SitePreflight.ZoneGround unreadable =
            (Vector2s zone, out TerrainZoneDeltas deltas, out TerrainConversion.VertexHeight baseHeightAt) =>
            {
                deltas = null!;
                baseHeightAt = Flat(31f);
                return true;
            };

        SiteDecision decision = SitePreflight.Decide(
            "site", new Vector2s(0, 0), new[] { Level(new Vector3(0f, 0f, 0f), 30f) }, unreadable);

        Assert.Equal(SiteVerdict.Undecided, decision.Verdict);
        Assert.False(decision.MayPublish);
    }

    [Fact]
    public void TheDecisionDoesNotWriteIntoTheZoneItWasAskedAbout()
    {
        // The preflight runs the real conversion so that "can this be served"
        // and "what does serving it produce" cannot disagree. It must not leave
        // the answer behind: the write happens later, through the ledger, and a
        // zone that already recorded this site would skip it.
        TerrainZoneDeltas zone = Zone();
        SitePreflight.ZoneGround ground =
            (Vector2s _, out TerrainZoneDeltas deltas, out TerrainConversion.VertexHeight baseHeightAt) =>
            {
                deltas = zone;
                baseHeightAt = Flat(31f);
                return true;
            };

        SitePreflight.Decide("site", new Vector2s(0, 0),
            new[] { Level(new Vector3(0f, 0f, 0f), 30f) }, ground);

        Assert.False(zone.HasApplied("site"));
        Assert.All(zone.ModifiedHeight, modified => Assert.False(modified));
    }

    [Fact]
    public void EveryZoneTheSiteTouchesIsAsked()
    {
        // A site is served or refused whole. Checking only the zone it is
        // centred in is how a levelled courtyard gets a step down one edge.
        var asked = new List<Vector2s>();
        SitePreflight.ZoneGround ground =
            (Vector2s zone, out TerrainZoneDeltas deltas, out TerrainConversion.VertexHeight baseHeightAt) =>
            {
                asked.Add(zone);
                deltas = Zone(zone.x, zone.y);
                baseHeightAt = Flat(31f);
                return true;
            };

        // Zone 0,0 runs to x = 32 and z = 32, so this reaches into three others.
        SitePreflight.Decide("site", new Vector2s(0, 0),
            new[] { Level(new Vector3(30f, 0f, 30f), 30f, radius: 8f) }, ground);

        Assert.True(asked.Count > 1, $"only {asked.Count} zone(s) asked about a site that straddles a corner");
        Assert.Equal(asked.Distinct().Count(), asked.Count);
    }

    [Fact]
    public void ARefusalNamesTheSiteSpecificCauseAndNotTheTemplate()
    {
        // The line the design turns on: this template converts perfectly well,
        // and THIS hillside cannot take it. Reporting it as a template defect
        // would withhold every other site of the same build.
        var operations = new[] { Level(new Vector3(0f, 0f, 0f), 10f) };

        SiteDecision decision = SitePreflight.Decide("MWL_Hall@100,200#1,3", new Vector2s(0, 0), operations, Ground(40f));

        Assert.StartsWith("site_", decision.Code);
    }
}
