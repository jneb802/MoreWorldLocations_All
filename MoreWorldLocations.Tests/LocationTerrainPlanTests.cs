using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Which sites a zone owes terrain to.
///
/// The failure this guards against is a site that spans two zones and gets its
/// shaping in only one of them: the ground steps down along the zone boundary,
/// the site looks finished, and the ledger says it is done.
/// </summary>
public class LocationTerrainPlanTests
{
    private static LocationTerrainPlan.PlacedSite Site(
        string name, Vector3 at, params LocationTerrainOperation[] operations) =>
        new(name, at, operations);

    private static LocationTerrainOperation Level(Vector3 at, float radius) =>
        new(at, level: true, levelRadius: radius);

    [Fact]
    public void AZoneOwesTheSitesThatReachIt()
    {
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(4, 4));
        var sites = new[] { Site("MWL_RuinsWell1", centre, Level(centre, 4f)) };

        var work = LocationTerrainPlan.For(new Vector2s(4, 4), sites);

        Assert.Single(work);
        Assert.Equal("MWL_RuinsWell1", work[0].LocationName);
        Assert.Equal(new Vector2s(4, 4), work[0].Zone);
    }

    [Fact]
    public void AZoneOwesNothingToASiteThatDoesNotReachIt()
    {
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(4, 4));
        var sites = new[] { Site("MWL_RuinsWell1", centre, Level(centre, 4f)) };

        Assert.Empty(LocationTerrainPlan.For(new Vector2s(5, 4), sites));
    }

    [Fact]
    public void ASiteOnABoundaryIsOwedByBothZones()
    {
        // This is the whole reason the plan is per zone. The site is one site;
        // the conversion is two, because two compilers hold the ground.
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(0, 0));
        var edge = new Vector3(centre.x + ZoneSystem.ZoneSize / 2f - 1f, 30f, centre.z);
        var sites = new[] { Site("MWL_Ruins1", edge, Level(edge, 6f)) };

        var here = LocationTerrainPlan.For(new Vector2s(0, 0), sites);
        var next = LocationTerrainPlan.For(new Vector2s(1, 0), sites);

        Assert.Single(here);
        Assert.Single(next);
        Assert.NotEqual(here[0].SiteId, next[0].SiteId);
    }

    [Fact]
    public void EachZoneGetsTheWholeSitesOperations()
    {
        // Not only the operations centred in it: they are simulated together,
        // because one levels ground a later one smooths. A subset would leave
        // this zone's vertices at a height no machine ever draws.
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(0, 0));
        var edge = new Vector3(centre.x + ZoneSystem.ZoneSize / 2f - 1f, 30f, centre.z);
        var sites = new[]
        {
            Site("MWL_Ruins1", edge,
                Level(edge, 6f),
                new LocationTerrainOperation(edge, smooth: true, smoothRadius: 6f, smoothPower: 4f)),
        };

        Assert.Equal(2, LocationTerrainPlan.For(new Vector2s(1, 0), sites)[0].Operations.Count);
    }

    [Fact]
    public void ASiteWithNoTerrainIsNotWork()
    {
        // Most templates have no modifier at all. A zone that owes nothing must
        // touch no compiler: creating one to write zeros would put a
        // _TerrainCompiler ZDO in every zone the server generates.
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(4, 4));

        Assert.Empty(LocationTerrainPlan.For(new Vector2s(4, 4),
            new[] { Site("MWL_WoodTower2", centre) }));
        Assert.Empty(LocationTerrainPlan.For(new Vector2s(4, 4),
            new[] { new LocationTerrainPlan.PlacedSite("MWL_WoodTower2", centre, null) }));
    }

    [Fact]
    public void NoSitesIsNoWorkRatherThanAnException()
    {
        Assert.Empty(LocationTerrainPlan.For(new Vector2s(0, 0), null));
        Assert.Empty(LocationTerrainPlan.For(new Vector2s(0, 0), new LocationTerrainPlan.PlacedSite[0]));
    }

    [Fact]
    public void TwoSitesOfOneTemplateInOneZoneAreTwoPiecesOfWork()
    {
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(4, 4));
        var a = new Vector3(centre.x - 12f, 30f, centre.z);
        var b = new Vector3(centre.x + 12f, 30f, centre.z);
        var sites = new[]
        {
            Site("MWL_RuinsWell1", a, Level(a, 4f)),
            Site("MWL_RuinsWell1", b, Level(b, 4f)),
        };

        var work = LocationTerrainPlan.For(new Vector2s(4, 4), sites);

        Assert.Equal(2, work.Count);
        Assert.Equal(2, work.Select(w => w.SiteId).Distinct().Count());
    }

    [Fact]
    public void ThePlanIsOrderedSoTwoRunsAgree()
    {
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(4, 4));
        var a = new Vector3(centre.x - 12f, 30f, centre.z);
        var b = new Vector3(centre.x + 12f, 30f, centre.z);
        var forwards = new[] { Site("MWL_RuinsWell1", a, Level(a, 4f)), Site("MWL_Ruins1", b, Level(b, 4f)) };
        var backwards = forwards.Reverse().ToArray();

        Assert.Equal(
            LocationTerrainPlan.For(new Vector2s(4, 4), forwards).Select(w => w.SiteId),
            LocationTerrainPlan.For(new Vector2s(4, 4), backwards).Select(w => w.SiteId));
    }

    [Fact]
    public void TheSiteIdNamesTheSiteAndTheZone()
    {
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(4, 4));
        var work = LocationTerrainPlan.For(new Vector2s(4, 4),
            new[] { Site("MWL_RuinsWell1", centre, Level(centre, 4f)) });

        Assert.Contains("MWL_RuinsWell1", work[0].SiteId);
        Assert.Contains("4,4", work[0].SiteId);
    }
}
