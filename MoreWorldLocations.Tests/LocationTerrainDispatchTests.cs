using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Which zones get written, which get repaired behind them, and what is left
/// outstanding.
///
/// These drive <c>LocationTerrainWriter.WriteZone</c> — the production dispatch,
/// with the real ledger, the real bridge and the real conversion. Only the
/// discovery of proxies is supplied by the test, because that half is engine
/// glue.
///
/// The failure they exist for: a zone's generation hook runs once. A zone
/// generated before a neighbouring site existed never looks at it again, and the
/// version this replaces logged and returned — indistinguishable from success to
/// everything downstream, leaving a site shaped on one side of a zone boundary
/// and not the other.
/// </summary>
public class LocationTerrainDispatchTests : IDisposable
{
    private static readonly Vector2s A = new(0, 0);
    private static readonly Vector2s B = new(1, 0);

    private readonly SyntheticWorld _world = new();
    private readonly HeightmapBuilder _builder = new();

    public LocationTerrainDispatchTests()
    {
        ZDOMan.instance = new ZDOMan();
        WorldGenerator.instance = _world;
        HeightmapBuilder.instance = _builder;
        ZoneSystem.instance = new ZoneSystem();
        Heightmap.Registered = null;
        Heightmap.Loaded.Clear();
        LocationTerrainWriter.Reset();
        _builder.Build(A, _world);
        _builder.Build(B, _world);
    }

    public void Dispose()
    {
        ZDOMan.instance = null;
        WorldGenerator.instance = null;
        HeightmapBuilder.instance = null;
        ZoneSystem.instance = null;
        Heightmap.Registered = null;
        Heightmap.Loaded.Clear();
        LocationTerrainWriter.Reset();
    }

    /// <summary>Load a zone the way generation does: a heightmap with its build data and a compiler.</summary>
    private Heightmap Load(Vector2s zone, bool withCompiler = true)
    {
        Heightmap hm = Heightmap.CreateForZone(zone, width: 64, withCompiler: withCompiler);
        hm.m_buildData = _builder.Built.TryGetValue(zone, out var built) ? built : null;
        Heightmap.Loaded[zone] = hm;
        return hm;
    }

    private void Unload(Vector2s zone) => Heightmap.Loaded.Remove(zone);

    private void Generated(Vector2s zone) => ZoneSystem.instance.Generated.Add(zone);

    /// <summary>
    /// A site on the A/B boundary: a level operation whose radius reaches into
    /// both zones, placed on the ground so the cut is one the compiler can hold.
    /// </summary>
    private LocationTerrainPlan.PlacedSite BoundarySite()
    {
        Vector3 centreA = ZoneSystem.GetZonePos(A);
        float x = centreA.x + ZoneSystem.ZoneSize / 2f - 1f;
        float z = centreA.z;
        var at = new Vector3(x, _world.GetHeight(x, z), z);
        var op = new LocationTerrainOperation(at, level: true, levelRadius: 6f, levelOffset: -1f);
        Assert.Equal(new[] { A, B }, LocationTerrainReader.ZonesTouched(new[] { op }));
        return new LocationTerrainPlan.PlacedSite("MWL_Boundary", at, new[] { op });
    }

    private static float[] TerrainOf(Vector2s zone)
    {
        TerrainZoneDeltas deltas = LocationTerrainBridge.AdoptSaved(zone, 64, 1f, out _, out string problem);
        Assert.Null(problem);
        Assert.NotNull(deltas);
        return deltas.LevelDelta;
    }

    private static IReadOnlyList<string> AppliedIn(Vector2s zone)
    {
        TerrainZoneDeltas deltas = LocationTerrainBridge.AdoptSaved(zone, 64, 1f, out _, out _);
        return deltas.AppliedOperations.OrderBy(s => s).ToList();
    }

    // ---- the two orderings -------------------------------------------------

    [Fact]
    public void AZoneGeneratedBeforeTheSiteExistedIsRepairedWhenItIsDiscovered()
    {
        // A first, with nothing in it. Then the site appears and B is generated.
        // A's hook will never run again, so if B's generation does not repair A,
        // the ground steps down the boundary and nothing says so.
        Heightmap hmA = Load(A);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite>());
        Generated(A);
        Unload(A);

        Heightmap hmB = Load(B);
        LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Generated(B);

        Assert.Empty(LocationTerrainLedger.Failures());
        Assert.Empty(LocationTerrainLedger.Waiting());
        Assert.Contains(TerrainOf(A), d => d != 0f);
        Assert.Contains(TerrainOf(B), d => d != 0f);
    }

    [Fact]
    public void TheOtherOrderReachesTheSameGround()
    {
        // B with the site first, then A. A's own hook finds the saved proxy --
        // the caller supplies it -- and writes its share.
        Heightmap hmB = Load(B);
        LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Generated(B);
        Unload(B);

        Heightmap hmA = Load(A);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Generated(A);

        Assert.Empty(LocationTerrainLedger.Failures());
        Assert.Empty(LocationTerrainLedger.Waiting());
        Assert.Contains(TerrainOf(A), d => d != 0f);
        Assert.Contains(TerrainOf(B), d => d != 0f);
    }

    [Fact]
    public void BothOrderingsConvergeOnTheSameIdentitiesAndTheSameTerrain()
    {
        float[] aFirstA, aFirstB, bFirstA, bFirstB;
        IReadOnlyList<string> idsA, idsB;

        Heightmap hmA = Load(A);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite>());
        Generated(A);
        Unload(A);
        Heightmap hmB = Load(B);
        LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Generated(B);
        aFirstA = TerrainOf(A); aFirstB = TerrainOf(B);
        idsA = AppliedIn(A); idsB = AppliedIn(B);

        // A second world, generated the other way round.
        Dispose();
        ZDOMan.instance = new ZDOMan();
        WorldGenerator.instance = _world;
        HeightmapBuilder.instance = _builder;
        ZoneSystem.instance = new ZoneSystem();
        LocationTerrainWriter.Reset();

        hmB = Load(B);
        LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Generated(B);
        Unload(B);
        hmA = Load(A);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Generated(A);
        bFirstA = TerrainOf(A); bFirstB = TerrainOf(B);

        Assert.Equal(aFirstA, bFirstA);
        Assert.Equal(aFirstB, bFirstB);
        Assert.Equal(idsA, AppliedIn(A));
        Assert.Equal(idsB, AppliedIn(B));
    }

    [Fact]
    public void ARestartBetweenTheTwoZonesDoesNotLoseOrRepeatTheWrite()
    {
        Heightmap hmB = Load(B);
        LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Generated(B);
        float[] before = TerrainOf(B);
        IReadOnlyList<string> idsBefore = AppliedIn(B);
        Unload(B);

        // The process restarts: the in-memory ledger is gone, the world is not.
        LocationTerrainWriter.Reset();
        Assert.Empty(LocationTerrainLedger.All());

        Heightmap hmA = Load(A);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Generated(A);

        // B keeps exactly what it had -- the completion record came back from
        // its own compiler -- and A is written now.
        Assert.Equal(before, TerrainOf(B));
        Assert.Equal(idsBefore, AppliedIn(B));
        Assert.Contains(TerrainOf(A), d => d != 0f);
        Assert.Empty(LocationTerrainLedger.Failures());
    }

    // ---- refusals that must stay outstanding -------------------------------

    [Fact]
    public void AZoneWhoseSavedCompilerIsNotAliveYetWaitsAndIsWrittenLater()
    {
        // Asking for a compiler now would make a second one, and the two destroy
        // each other on every load. Waiting is not success and must not be
        // recorded as one.
        Heightmap hmA = Load(A, withCompiler: false);
        ZDOMan.instance.CreateNewZDO(hmA.transform.position, LocationTerrainBridge.TerrainCompilerPrefab);

        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });

        LocationTerrainLedger.Entry waiting = Assert.Single(
            LocationTerrainLedger.Waiting().Where(e => e.Zone == A));
        Assert.Contains("WaitForSavedCompiler", waiting.Reason);
        Assert.DoesNotContain(LocationTerrainLedger.All(),
            e => e.Zone == A && e.State == LocationTerrainLedger.State.Done);
    }

    [Fact]
    public void ACompilerOwnedElsewhereWaitsAndIsWrittenWhenItIsReleased()
    {
        Heightmap hmA = Load(A);
        hmA.m_terrainComp!.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);

        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        Assert.Contains(LocationTerrainLedger.Waiting(), e => e.Zone == A);
        Assert.All(TerrainOf(A), d => Assert.Equal(0f, d));

        // The owner goes away; the next zone generation retries what is waiting.
        hmA.m_terrainComp.m_nview.GetZDO().SetOwner(0L);
        Heightmap hmB = Load(B);
        LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite>());

        Assert.DoesNotContain(LocationTerrainLedger.Waiting(), e => e.Zone == A);
        Assert.Contains(TerrainOf(A), d => d != 0f);
    }

    [Fact]
    public void ASaveThatWritesNothingIsNotSuccess()
    {
        // The game's own Save returns without a word in some states. Treating
        // "TCData is not null" as success is how a completed identity gets paired
        // with terrain this write never made.
        Heightmap hmA = Load(A);
        hmA.m_terrainComp!.SaveFails = true;

        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });

        Assert.Contains(LocationTerrainLedger.Waiting(), e => e.Zone == A);
        Assert.DoesNotContain(LocationTerrainLedger.All(),
            e => e.Zone == A && e.State == LocationTerrainLedger.State.Done);
        Assert.Empty(AppliedIn(A));
    }

    [Fact]
    public void AnExistingBlobFromSomebodyElseDoesNotCountAsThisWrite()
    {
        // The zone already carries terrain -- a player's digging. A save that
        // does nothing leaves that blob in place; the gate is equality with what
        // this write intended, not "there are bytes".
        Heightmap hmA = Load(A);
        TerrainComp compiler = hmA.m_terrainComp!;
        compiler.m_modifiedHeight[5] = true;
        compiler.m_levelDelta[5] = 3f;
        compiler.Save();
        Assert.NotNull(compiler.m_nview.GetZDO().GetByteArray(ZDOVars.s_TCData));

        compiler.SaveFails = true;
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });

        Assert.DoesNotContain(LocationTerrainLedger.All(),
            e => e.Zone == A && e.State == LocationTerrainLedger.State.Done);
        Assert.Equal(3f, TerrainOf(A)[5]);
    }

    [Fact]
    public void AWaitThatNeverResolvesBecomesAReportedFailure()
    {
        // Unbounded waiting is how a site that will never be written stays
        // invisible. After a bounded number of attempts it is a failure, and it
        // keeps the reason it was waiting for.
        Heightmap hmA = Load(A);
        hmA.m_terrainComp!.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);
        var site = BoundarySite();

        for (int i = 0; i < LocationTerrainLedger.MaxAttempts + 2; i++)
            LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { site });

        LocationTerrainLedger.Entry failure = Assert.Single(
            LocationTerrainLedger.Failures().Where(e => e.Zone == A));
        Assert.Contains("gave up", failure.Reason);
        Assert.Contains("OwnedElsewhere", failure.Reason);
        Assert.Contains("FAILED", LocationTerrainLedger.Status());
    }

    [Fact]
    public void ASiteTheCompilerCannotHoldIsAFailureAndNotARetry()
    {
        Vector3 centre = ZoneSystem.GetZonePos(A);
        var at = new Vector3(centre.x, _world.GetHeight(centre.x, centre.z), centre.z);
        var site = new LocationTerrainPlan.PlacedSite("MWL_TooDeep", at,
            new[] { new LocationTerrainOperation(at, level: true, levelRadius: 4f, levelOffset: -40f) });

        Heightmap hmA = Load(A);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { site });

        LocationTerrainLedger.Entry failure = Assert.Single(LocationTerrainLedger.Failures());
        Assert.Contains("beyond range", failure.Reason);
        Assert.Empty(LocationTerrainLedger.Waiting());
        Assert.All(TerrainOf(A), d => Assert.Equal(0f, d));
    }

    [Fact]
    public void OneZoneThatCannotBeWrittenDoesNotStopAHealthyOne()
    {
        // Two separate sites. One zone's compiler is held by another peer; the
        // other zone still has to complete, and the report has to show both.
        Vector3 centreA = ZoneSystem.GetZonePos(A);
        Vector3 centreB = ZoneSystem.GetZonePos(B);
        var inA = new Vector3(centreA.x, _world.GetHeight(centreA.x, centreA.z), centreA.z);
        var inB = new Vector3(centreB.x, _world.GetHeight(centreB.x, centreB.z), centreB.z);
        var siteA = new LocationTerrainPlan.PlacedSite("MWL_InA", inA,
            new[] { new LocationTerrainOperation(inA, level: true, levelRadius: 4f, levelOffset: -1f) });
        var siteB = new LocationTerrainPlan.PlacedSite("MWL_InB", inB,
            new[] { new LocationTerrainOperation(inB, level: true, levelRadius: 4f, levelOffset: -1f) });

        Heightmap hmA = Load(A);
        hmA.m_terrainComp!.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { siteA });

        Heightmap hmB = Load(B);
        LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { siteB });

        Assert.Contains(TerrainOf(B), d => d != 0f);
        Assert.Contains(LocationTerrainLedger.Waiting(), e => e.Zone == A);
        Assert.Contains("1 written", LocationTerrainLedger.Status());
    }

    [Fact]
    public void ExactlyOneCompilerPerZoneAfterARepair()
    {
        // The repair writes through the zone's saved compiler. A second one would
        // destroy the first on the next load.
        Heightmap hmA = Load(A);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite>());
        Generated(A);
        Unload(A);

        Heightmap hmB = Load(B);
        LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });

        LocationTerrainBridge.SavedCompiler(A, out int inA);
        LocationTerrainBridge.SavedCompiler(B, out int inB);
        Assert.Equal(1, inA);
        Assert.Equal(1, inB);
    }

    // ---- recovery without another zone being generated ---------------------

    [Fact]
    public void TheLastZoneIsFinishedWithoutAnyFurtherZoneBeingGenerated()
    {
        // The gap this closes: retries used to run only from a zone's generation
        // hook. If the LAST zone a site owes is waiting -- its compiler belongs
        // to someone else, its heights are not built -- and nobody moves far
        // enough to generate another zone, nothing ever calls back. The injected
        // failure on 15 Sep recovered only because a neighbour happened to
        // generate next; that is luck, not recovery.
        ServerOnlyMode.Set(true);
        try
        {
            Heightmap hmA = Load(A);
            hmA.m_terrainComp!.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);
            LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
            Assert.Contains(LocationTerrainLedger.Waiting(), e => e.Zone == A);

            // Nothing else generates. The blocker clears and the clock alone
            // has to finish the work.
            hmA.m_terrainComp.m_nview.GetZDO().SetOwner(0L);
            Assert.True(LocationTerrainWriter.Tick(1000f));

            Assert.Empty(LocationTerrainLedger.Waiting());
            Assert.Contains(TerrainOf(A), d => d != 0f);
        }
        finally { ServerOnlyMode.Set(false); }
    }

    [Fact]
    public void TheClockIsBoundedAndSilentWhenThereIsNothingToDo()
    {
        ServerOnlyMode.Set(true);
        try
        {
            // Nothing outstanding: no work, however often it is called.
            Assert.False(LocationTerrainWriter.Tick(1000f));

            Heightmap hmA = Load(A);
            hmA.m_terrainComp!.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);
            LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });

            // Something outstanding, but the clock has just run: not again yet.
            Assert.True(LocationTerrainWriter.Tick(2000f));
            Assert.False(LocationTerrainWriter.Tick(2000f + LocationTerrainWriter.TickSeconds / 2f));
            Assert.True(LocationTerrainWriter.Tick(2000f + LocationTerrainWriter.TickSeconds));
        }
        finally { ServerOnlyMode.Set(false); }
    }

    [Fact]
    public void TheClockDoesNothingOutsideServerOnlyMode()
    {
        Heightmap hmA = Load(A);
        hmA.m_terrainComp!.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);
        ServerOnlyMode.Set(true);
        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });
        ServerOnlyMode.Set(false);

        Assert.False(LocationTerrainWriter.Tick(9999f));
        Assert.NotEmpty(LocationTerrainLedger.Waiting());
    }

    [Fact]
    public void WorkOutstandingAtAShutdownIsTakenUpAgainAfterTheRestart()
    {
        // The pending set is memory and a restart empties it, while the zones it
        // concerned are already generated and will never run their own hook
        // again. Without the reseed, a site left half written by a shutdown
        // stays half written for the life of the world.
        ServerOnlyMode.Set(true);
        try
        {
            var site = BoundarySite();

            // B is written; A is generated but its compiler is held, so A waits.
            Heightmap hmB = Load(B);
            LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { site });
            Generated(B);
            Heightmap hmA = Load(A);
            hmA.m_terrainComp!.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);
            Generated(A);
            LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { site });
            Assert.Contains(LocationTerrainLedger.Waiting(), e => e.Zone == A);
            float[] bBefore = TerrainOf(B);

            // Restart: the ledger and the pending set are gone, the world is not.
            LocationTerrainWriter.Reset();
            Assert.Empty(LocationTerrainLedger.All());
            hmA.m_terrainComp.m_nview.GetZDO().SetOwner(0L);

            int outstanding = LocationTerrainWriter.Reseed(new[] { site });

            // B is recognised as already carried -- from its own compiler, not
            // from a memory of ours -- and only A is outstanding.
            Assert.Equal(1, outstanding);
            Assert.Contains(LocationTerrainLedger.All(),
                e => e.Zone == B && e.State == LocationTerrainLedger.State.Done);
            Assert.Contains(LocationTerrainLedger.Waiting(), e => e.Zone == A);

            Assert.True(LocationTerrainWriter.Tick(5000f));
            Assert.Empty(LocationTerrainLedger.Waiting());
            Assert.Contains(TerrainOf(A), d => d != 0f);
            Assert.Equal(bBefore, TerrainOf(B));
        }
        finally { ServerOnlyMode.Set(false); }
    }

    [Fact]
    public void AFinishedSiteIsNotPlannedAgainAfterARestart()
    {
        ServerOnlyMode.Set(true);
        try
        {
            var site = BoundarySite();
            Heightmap hmA = Load(A);
            Heightmap hmB = Load(B);
            LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { site });
            Generated(A);
            LocationTerrainWriter.WriteZone(B, hmB, new List<LocationTerrainPlan.PlacedSite> { site });
            Generated(B);
            Assert.Empty(LocationTerrainLedger.Waiting());

            LocationTerrainWriter.Reset();
            Assert.Equal(0, LocationTerrainWriter.Reseed(new[] { site }));
            Assert.Empty(LocationTerrainLedger.Waiting());
            Assert.Empty(LocationTerrainLedger.Failures());
        }
        finally { ServerOnlyMode.Set(false); }
    }

    [Fact]
    public void AZoneThatWasNeverGeneratedIsNotOutstanding()
    {
        // Its own hook will find the proxy vanilla saved. Treating it as
        // outstanding would write terrain for a zone the world has not made.
        ServerOnlyMode.Set(true);
        try
        {
            Assert.Equal(0, LocationTerrainWriter.Reseed(new[] { BoundarySite() }));
            Assert.Empty(LocationTerrainLedger.All());
        }
        finally { ServerOnlyMode.Set(false); }
    }

    [Fact]
    public void ARepairRefusesToWriteBehindALiveCompiler()
    {
        // A loaded zone's live compiler owns those arrays and would overwrite a
        // detached write on its next save.
        Heightmap hmA = Load(A);
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(hmA.m_terrainComp!);

        Assert.False(LocationTerrainBridge.WriteDetached(A, zone, TerrainBlob.Header.Fresh, out string failure));
        Assert.Contains("live compiler", failure);
    }

    [Fact]
    public void AZoneWithNoGeneratedHeightsWaitsRatherThanConvertingAgainstZero()
    {
        _builder.Built.Remove(A);
        Heightmap hmA = Load(A);
        hmA.m_buildData = null;

        LocationTerrainWriter.WriteZone(A, hmA, new List<LocationTerrainPlan.PlacedSite> { BoundarySite() });

        Assert.Contains(LocationTerrainLedger.Waiting(),
            e => e.Zone == A && e.Reason.Contains("heights"));
        Assert.All(TerrainOf(A), d => Assert.Equal(0f, d));
    }
}
