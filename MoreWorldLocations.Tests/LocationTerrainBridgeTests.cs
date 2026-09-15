using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Carrying a conversion across into the game's terrain compiler.
///
/// Three things here can go wrong quietly. The arrays can be aliased, so a
/// refused conversion still reaches the ground. The compiler can be asked for
/// before the saved one has come alive, which makes a second compiler and the
/// two destroy each other on every load. And the base height can be read from
/// the heightmap, which already carries the deltas being replaced, so the site
/// sinks a little further on every pass.
/// </summary>
public class LocationTerrainBridgeTests : IDisposable
{
    private readonly Vector2s _zone = new(2, -3);
    private readonly SyntheticWorld _world = new();

    private readonly HeightmapBuilder _builder = new();

    public LocationTerrainBridgeTests()
    {
        ZDOMan.instance = new ZDOMan();
        WorldGenerator.instance = _world;
        HeightmapBuilder.instance = _builder;
        ZoneSystem.instance = new ZoneSystem();
        Heightmap.Registered = null;
        // The bridge keeps the heights the builder hands over, because the
        // builder only hands them over once and two callers need them: the site
        // preflight before a site is published, and the conversion afterwards.
        // Kept state is per world, and each test is its own world.
        LocationTerrainBridge.ForgetGeneratedHeights();
    }

    public void Dispose()
    {
        ZDOMan.instance = null;
        WorldGenerator.instance = null;
        HeightmapBuilder.instance = null;
        ZoneSystem.instance = null;
        Heightmap.Registered = null;
    }

    /// <summary>
    /// A zone as the game hands it over during generation: a heightmap whose
    /// build data is already there, because that array is what the client will
    /// use and the bridge is required to read it rather than re-derive it.
    /// </summary>
    private Heightmap Zone(bool withCompiler = true, bool built = true)
    {
        Heightmap hm = Heightmap.CreateForZone(_zone, width: 64, withCompiler: withCompiler);
        if (built)
            hm.m_buildData = _builder.Build(_zone, _world);
        Heightmap.Registered = hm;
        return hm;
    }

    /// <summary>
    /// MWL_RuinsWell1's modifier, at the centre of the zone and ON the ground.
    ///
    /// The y matters: vanilla levels TO <c>position + up * levelOffset</c>, so a
    /// modifier placed at y = 0 over ground at 48 m asks for a fifty-metre cut
    /// and is refused. A location is placed at the ground height, so the test
    /// puts it there.
    /// </summary>
    private LocationTerrainOperation[] Well(Heightmap hm)
    {
        Vector3 centre = hm.transform.position;
        var onGround = new Vector3(centre.x, _world.GetHeight(centre.x, centre.z), centre.z);
        return new[] { new LocationTerrainOperation(onGround, level: true, levelRadius: 4f, levelOffset: -2f) };
    }

    // ---- acquiring the zone's compiler ------------------------------------

    [Fact]
    public void AZoneWithNoCompilerGetsOne()
    {
        Heightmap hm = Zone(withCompiler: false);

        Assert.Equal(LocationTerrainBridge.Readiness.Ready,
            LocationTerrainBridge.Acquire(hm, _zone, out TerrainComp compiler));
        Assert.NotNull(compiler);
        Assert.True(compiler.m_nview.IsOwner());
    }

    [Fact]
    public void ASavedCompilerThatIsNotAliveYetIsWaitedFor()
    {
        // The game creates the saved compiler from its ZDO only after the zone
        // has loaded. Asking for one now makes a second, and the two destroy
        // each other on every load -- the roads work was bitten by this twice.
        Heightmap hm = Zone(withCompiler: false);
        ZDOMan.instance.CreateNewZDO(hm.transform.position, LocationTerrainBridge.TerrainCompilerPrefab);

        Assert.Equal(LocationTerrainBridge.Readiness.WaitForSavedCompiler,
            LocationTerrainBridge.Acquire(hm, _zone, out TerrainComp compiler));
        Assert.Null(compiler);
        Assert.Null(hm.m_terrainComp);
    }

    [Fact]
    public void ACompilerOwnedByAnotherPeerIsTheirsToWrite()
    {
        Heightmap hm = Zone();
        hm.m_terrainComp.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);

        Assert.Equal(LocationTerrainBridge.Readiness.OwnedElsewhere,
            LocationTerrainBridge.Acquire(hm, _zone, out TerrainComp compiler));
        Assert.Null(compiler);
    }

    [Fact]
    public void AnUnownedCompilerIsClaimed()
    {
        Heightmap hm = Zone();
        hm.m_terrainComp.m_nview.GetZDO().SetOwner(0L);

        Assert.Equal(LocationTerrainBridge.Readiness.Ready,
            LocationTerrainBridge.Acquire(hm, _zone, out TerrainComp compiler));
        Assert.True(compiler.m_nview.IsOwner());
    }

    [Fact]
    public void NoHeightmapIsNotAnException()
    {
        Assert.Equal(LocationTerrainBridge.Readiness.NoHeightmap,
            LocationTerrainBridge.Acquire(null, _zone, out TerrainComp compiler));
        Assert.Null(compiler);
    }

    // ---- adopting and writing back ----------------------------------------

    [Fact]
    public void AdoptingCopiesTheArraysRatherThanSharingThem()
    {
        // The conversion runs on the adopted zone and is published only if it is
        // servable. Sharing the arrays would put a refused conversion into the
        // ground anyway, which is the failure ApplyOnce exists to prevent.
        Heightmap hm = Zone();
        TerrainComp compiler = hm.m_terrainComp;
        compiler.m_levelDelta[0] = 3f;

        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(compiler);
        Assert.Equal(3f, zone.LevelDelta[0]);

        zone.LevelDelta[0] = -5f;
        Assert.Equal(3f, compiler.m_levelDelta[0]);
    }

    [Fact]
    public void AdoptingTakesTheZonesGeometryFromTheCompiler()
    {
        Heightmap hm = Zone();
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(hm.m_terrainComp);

        Assert.Equal(hm.m_terrainComp.m_width, zone.Width);
        Assert.Equal(hm.m_scale, zone.Scale);
        Assert.Equal(hm.transform.position, zone.Origin);
    }

    [Fact]
    public void WritingBackReachesTheCompilerAndItsZdo()
    {
        Heightmap hm = Zone();
        TerrainComp compiler = hm.m_terrainComp;
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(compiler);

        Assert.True(TerrainConversion.ApplyOnce(
            "site#1", Well(hm), zone, LocationTerrainBridge.GeneratedHeightAt(zone, hm), out var result));
        Assert.True(result.VerticesChanged > 0);

        Assert.True(LocationTerrainBridge.WriteBack(compiler, zone));
        Assert.Equal(zone.LevelDelta.Take(zone.Pitch * zone.Pitch), compiler.m_levelDelta.Take(zone.Pitch * zone.Pitch));
        Assert.Equal(1, compiler.SaveCount);
        Assert.Equal(1, hm.PokeCount);
        Assert.NotNull(compiler.m_nview.GetZDO().GetByteArray(ZDOVars.s_TCData));
    }

    [Fact]
    public void WritingBackRefusesWhenThisPeerDoesNotOwnTheCompiler()
    {
        // The game's own Save returns without a word in that case, so a caller
        // that assumed success would record a site as written that is not.
        Heightmap hm = Zone();
        TerrainComp compiler = hm.m_terrainComp;
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(compiler);
        compiler.m_nview.GetZDO().SetOwner(ZDOMan.instance.m_sessionID + 1);

        Assert.False(LocationTerrainBridge.WriteBack(compiler, zone));
        Assert.Equal(0, compiler.SaveCount);
    }

    [Fact]
    public void AnUninitialisedCompilerIsRefusedRatherThanPartlyRead()
    {
        Heightmap hm = Zone();
        hm.m_terrainComp.m_levelDelta = new float[4];

        Assert.Throws<ArgumentException>(() => LocationTerrainBridge.Adopt(hm.m_terrainComp));
    }

    // ---- the record that outlives the process ------------------------------

    [Fact]
    public void AWrittenSiteIsRecordedOnTheCompilerAndComesBack()
    {
        Heightmap hm = Zone();
        TerrainComp compiler = hm.m_terrainComp;

        TerrainZoneDeltas first = LocationTerrainBridge.Adopt(compiler);
        Assert.True(TerrainConversion.ApplyOnce(
            "MWL_RuinsWell1@100,50#2,-3", Well(hm), first,
            LocationTerrainBridge.GeneratedHeightAt(first, hm), out _));
        Assert.True(LocationTerrainBridge.WriteBack(compiler, first));

        // A fresh adopt is what a restart does: the compiler comes back from its
        // ZDO and the mod asks it again what it already carries.
        TerrainZoneDeltas reloaded = LocationTerrainBridge.Adopt(compiler);
        Assert.True(reloaded.HasApplied("MWL_RuinsWell1@100,50#2,-3"));

        Assert.False(TerrainConversion.ApplyOnce(
            "MWL_RuinsWell1@100,50#2,-3", Well(hm), reloaded,
            LocationTerrainBridge.GeneratedHeightAt(reloaded, hm), out var again));
        Assert.True(again.Representable);   // already written, not refused
    }

    [Fact]
    public void AnUntouchedCompilerCarriesNoSites()
    {
        Heightmap hm = Zone();
        Assert.Empty(LocationTerrainBridge.Adopt(hm.m_terrainComp).AppliedOperations);
    }

    // ---- the base height ---------------------------------------------------

    [Fact]
    public void TheBaseHeightIsTheHeightmapsOwnBuildData()
    {
        // The contract on TerrainConversion.VertexHeight is the height the
        // client generates for ITSELF, before any compiler delta -- and that is
        // the builder's array, which the heightmap already holds. Re-deriving it
        // from WorldGenerator.GetHeight is a different function: the builder
        // takes the biome at the zone's four CORNERS and blends when they
        // disagree, while GetHeight looks the biome up per point.
        Heightmap hm = Zone();
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(hm.m_terrainComp);
        var heightAt = LocationTerrainBridge.GeneratedHeightAt(zone, hm);

        for (int y = 0; y < zone.Pitch; y += 17)
            for (int x = 0; x < zone.Pitch; x += 17)
                Assert.Equal(hm.m_buildData!.m_baseHeights[y * zone.Pitch + x], heightAt(x, y));

        // And it does not go and ask the builder again when the heightmap has it.
        Assert.Equal(0, _builder.SyncRequests);
    }

    [Fact]
    public void ThereIsNoConversionWithoutGeneratedHeights()
    {
        // Converting against zero writes the site into ground nobody generates
        // and records it as done. A zone whose heights are not built is a zone
        // to leave alone.
        Heightmap hm = Zone(built: false);
        _builder.Built.Clear();
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(hm.m_terrainComp);

        Assert.False(LocationTerrainBridge.TryGeneratedHeightAt(zone, hm, out _, out string why));
        Assert.Contains("nobody generates", why);
        Assert.Throws<InvalidOperationException>(() => LocationTerrainBridge.GeneratedHeightAt(zone, hm));
    }

    [Fact]
    public void WithNoHeightmapTheBuilderIsAsked()
    {
        // The repair path for an already-generated zone has no heightmap at all.
        Heightmap hm = Zone();
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(hm.m_terrainComp);

        var heightAt = LocationTerrainBridge.GeneratedHeightAt(zone, null);
        Assert.Equal(hm.m_buildData!.m_baseHeights[0], heightAt(0, 0));
        Assert.True(_builder.SyncRequests > 0);
    }

    [Fact]
    public void TheGeneratedHeightsAreFetchedOnceBecauseAskingConsumesThem()
    {
        // Found in game, not by a test. HeightmapBuilder hands a ready entry out
        // and REMOVES it (RequestTerrain: m_ready.RemoveAt), so a "can I?" call
        // followed by a "do it" call consumes the data in the first and finds
        // nothing in the second. On the server that threw on every retry, the
        // exception was caught per zone, the attempt was never counted, and the
        // site sat outstanding at attempt 11 for ever.
        Heightmap hm = Zone();
        hm.m_buildData = null;                  // force the builder path
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(hm.m_terrainComp!);

        Assert.True(LocationTerrainBridge.TryGeneratedHeightAt(
            zone, null, out TerrainConversion.VertexHeight height, out _));
        Assert.Equal(1, _builder.SyncRequests);
        Assert.False(float.IsNaN(height(0, 0)));
    }

    [Fact]
    public void AHeightArrayOfTheWrongWidthIsRefused()
    {
        Heightmap hm = Zone();
        hm.m_buildData!.m_baseHeights = hm.m_buildData.m_baseHeights.Take(16).ToList();
        _builder.Built.Clear();
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(hm.m_terrainComp);

        Assert.Throws<InvalidOperationException>(() => LocationTerrainBridge.GeneratedHeightAt(zone, hm));
    }

    [Fact]
    public void GroundThatAlreadyCarriesADeltaDoesNotMoveTheSite()
    {
        // The compiler's arrays are not always zero: a player's digging, or
        // another mod's roads, get there first. The base height has to stay the
        // height the CLIENT generates, so the site lands on the authored ground
        // whatever the zone was already carrying. Reading a height that has the
        // existing delta folded in is the quiet version of this bug -- the site
        // comes out displaced by exactly what was there before, and nothing
        // downstream can tell.
        Heightmap hm = Zone();
        TerrainComp compiler = hm.m_terrainComp;

        TerrainZoneDeltas clean = LocationTerrainBridge.Adopt(compiler);
        Assert.True(TerrainConversion.ApplyOnce(
            "clean", Well(hm), clean, LocationTerrainBridge.GeneratedHeightAt(clean, hm), out _));

        // Someone raised this zone by three metres before the site arrived.
        for (int i = 0; i < compiler.m_levelDelta.Length; i++)
        {
            compiler.m_levelDelta[i] = 3f;
            compiler.m_modifiedHeight[i] = true;
        }

        TerrainZoneDeltas overDug = LocationTerrainBridge.Adopt(compiler);
        Assert.True(TerrainConversion.ApplyOnce(
            "over-dug", Well(hm), overDug, LocationTerrainBridge.GeneratedHeightAt(overDug, hm), out var result));

        // Inside the site's own footprint the deltas are the same, not three
        // metres smaller. Outside it the three metres are still there: the
        // conversion states the ground it is responsible for and does not erase
        // terrain it did not draw.
        int changed = 0, kept = 0;
        for (int i = 0; i < clean.LevelDelta.Length; i++)
        {
            if (clean.LevelDelta[i] != 0f)
            {
                Assert.Equal(clean.LevelDelta[i], overDug.LevelDelta[i], 4);
                changed++;
            }
            else
            {
                Assert.Equal(3f, overDug.LevelDelta[i]);
                kept++;
            }
        }
        Assert.True(changed > 0, "the site changed nothing, so the test proves nothing");
        Assert.True(kept > 0, "nothing was left outside the footprint to check");
        Assert.NotEmpty(result.ContestedVertices);
    }

    [Fact]
    public void WritingTheSameSiteTwiceLandsOnTheSameGround()
    {
        // The conversion states an absolute height rather than adding to what is
        // there, so even a forced second pass has to leave the same deltas. If
        // the base height came from the heightmap instead, the site would sink
        // by its own depth each time.
        Heightmap hm = Zone();
        TerrainComp compiler = hm.m_terrainComp;

        TerrainZoneDeltas first = LocationTerrainBridge.Adopt(compiler);
        TerrainConversion.ApplyOnce("a", Well(hm), first, LocationTerrainBridge.GeneratedHeightAt(first, hm), out _);
        LocationTerrainBridge.WriteBack(compiler, first);
        float[] afterOne = (float[])compiler.m_levelDelta.Clone();

        TerrainZoneDeltas second = LocationTerrainBridge.Adopt(compiler);
        TerrainConversion.ApplyOnce("b", Well(hm), second, LocationTerrainBridge.GeneratedHeightAt(second, hm), out _);
        LocationTerrainBridge.WriteBack(compiler, second);

        Assert.Equal(afterOne, compiler.m_levelDelta);
    }

    [Fact]
    public void ARefusedConversionLeavesTheCompilerAlone()
    {
        // A cut deeper than the compiler can hold is a template to exclude, not
        // a thing to write eight metres of and call done.
        Heightmap hm = Zone();
        TerrainComp compiler = hm.m_terrainComp;
        TerrainZoneDeltas zone = LocationTerrainBridge.Adopt(compiler);

        Vector3 centre = hm.transform.position;
        var onGround = new Vector3(centre.x, _world.GetHeight(centre.x, centre.z), centre.z);
        var tooDeep = new[]
        {
            new LocationTerrainOperation(onGround, level: true, levelRadius: 4f, levelOffset: -40f),
        };

        Assert.False(TerrainConversion.ApplyOnce(
            "deep", tooDeep, zone, LocationTerrainBridge.GeneratedHeightAt(zone, hm), out var result));
        Assert.False(result.Representable);
        Assert.NotEmpty(result.BeyondCompilerRange);
        Assert.All(zone.LevelDelta, d => Assert.Equal(0f, d));
        Assert.All(compiler.m_levelDelta, d => Assert.Equal(0f, d));
        Assert.Equal(0, compiler.SaveCount);
    }
}
