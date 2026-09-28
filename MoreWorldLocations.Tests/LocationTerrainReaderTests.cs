using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Turning a template's terrain modifiers into values.
///
/// Everything downstream is arithmetic over those values, so a field read from
/// the wrong place here is ground in the wrong place with nothing further along
/// able to notice. The mapping is therefore asserted field by field rather than
/// through a round trip.
/// </summary>
public class LocationTerrainReaderTests
{
    private static TerrainModifier Modifier(Action<TerrainModifier> configure)
    {
        var modifier = new TerrainModifier();
        configure(modifier);
        return modifier;
    }

    /// <summary>MWL_RuinsWell1's only modifier, from the offline audit.</summary>
    private static TerrainModifier RuinsWell1() => Modifier(m =>
    {
        m.m_level = true;
        m.m_levelRadius = 4f;
        m.m_levelOffset = -2f;
        m.m_square = false;
        m.m_smooth = false;
        m.m_smoothRadius = 2f;
        m.m_paintCleared = false;
        m.m_paintRadius = 2f;
    });

    [Fact]
    public void EverySettingIsCarriedAcross()
    {
        TerrainModifier modifier = Modifier(m =>
        {
            m.m_sortOrder = 7;
            m.m_level = true;
            m.m_levelRadius = 3.5f;
            m.m_levelOffset = -2f;
            m.m_square = true;
            m.m_smooth = true;
            m.m_smoothRadius = 6.25f;
            m.m_smoothPower = 4f;
            m.m_paintCleared = true;
            m.m_paintRadius = 3f;
            m.m_paintStrength = 0.5f;
            m.m_paintType = TerrainModifier.PaintType.Paved;
            m.m_paintHeightCheck = true;
        });

        LocationTerrainOperation op = LocationTerrainReader.Operation(modifier, new Vector3(10f, 5f, -20f));

        Assert.Equal(new Vector3(10f, 5f, -20f), op.Position);
        Assert.Equal(7, op.SortOrder);
        Assert.True(op.Level);
        Assert.Equal(3.5f, op.LevelRadius);
        Assert.Equal(-2f, op.LevelOffset);
        Assert.True(op.Square);
        Assert.True(op.Smooth);
        Assert.Equal(6.25f, op.SmoothRadius);
        Assert.Equal(4f, op.SmoothPower);
        Assert.True(op.Paint);
        Assert.Equal(3f, op.PaintRadius);
        Assert.Equal(0.5f, op.PaintStrength);
        Assert.Equal(TerrainModifier.PaintType.Paved, op.PaintType);
        Assert.True(op.PaintHeightCheck);
    }

    [Fact]
    public void PaintMeansPaintCleared()
    {
        // Heightmap.ApplyModifier gates the paint pass on m_paintCleared, not on
        // the paint type -- a modifier with m_paintType Dirt and m_paintCleared
        // false paints nothing. Reading the type as the switch would paint a
        // ring of dirt around every levelled site.
        Assert.False(LocationTerrainReader.Operation(
            Modifier(m => { m.m_paintCleared = false; m.m_paintType = TerrainModifier.PaintType.Dirt; }),
            Vector3.zero).Paint);

        Assert.True(LocationTerrainReader.Operation(
            Modifier(m => m.m_paintCleared = true), Vector3.zero).Paint);
    }

    [Fact]
    public void RuinsWell1ReadsAsTheAuditDescribesIt()
    {
        // The offline audit is the expected value: level radius 4, round,
        // offset -2, no smooth, no paint. If the reader and the auditor
        // disagree, one of them is reading the asset wrong.
        LocationTerrainOperation op = LocationTerrainReader.Operation(RuinsWell1(), new Vector3(100f, 30f, 50f));

        Assert.True(op.Level);
        Assert.Equal(4f, op.LevelRadius);
        Assert.Equal(-2f, op.LevelOffset);
        Assert.False(op.Square);
        Assert.False(op.Smooth);
        Assert.False(op.Paint);
        Assert.Equal(4f, op.Radius);
    }

    [Fact]
    public void ADisabledModifierIsNotAnOperation()
    {
        // ApplyModifiers tests item.enabled before doing anything, so a disabled
        // modifier shapes nothing in game and must shape nothing here.
        var operations = LocationTerrainReader.Operations(
            new[] { Modifier(m => { m.m_level = true; m.enabled = false; }) },
            _ => Vector3.zero);

        Assert.Empty(operations);
    }

    [Fact]
    public void AModifierThatUsesTheCompilerIsLeftToTheCompiler()
    {
        // m_useTerrainCompiler means the game writes this one through a
        // persistent TerrainComp itself. Converting it as well would apply the
        // same shaping twice, which is the exact failure this whole conversion
        // exists to avoid on a modded client.
        var operations = LocationTerrainReader.Operations(
            new[] { Modifier(m => { m.m_level = true; m.m_useTerrainCompiler = true; }) },
            _ => Vector3.zero);

        Assert.Empty(operations);
    }

    [Fact]
    public void TemplateOrderIsKept()
    {
        // Vanilla's tie-break after m_sortOrder is creation time, which for a
        // location's children instantiated together is template order. The
        // conversion's own sort is stable, so this is the order it will apply.
        var modifiers = new[]
        {
            Modifier(m => { m.m_level = true; m.m_levelRadius = 1f; }),
            Modifier(m => { m.m_level = true; m.m_levelRadius = 2f; }),
            Modifier(m => { m.m_level = true; m.m_levelRadius = 3f; }),
        };

        var operations = LocationTerrainReader.Operations(modifiers, _ => Vector3.zero);

        Assert.Equal(new[] { 1f, 2f, 3f }, operations.Select(o => o.LevelRadius));
    }

    [Fact]
    public void TheWorldPositionIsTheCallersToCompute()
    {
        // The reader never rotates anything: the caller hands in the position
        // the game's own transform maths produced. A second implementation of
        // that is ground in the wrong place that nothing downstream can catch.
        var placed = new Vector3(-976f, 32f, -1193f);
        var operations = LocationTerrainReader.Operations(
            new[] { RuinsWell1() }, _ => placed);

        Assert.Equal(placed, operations.Single().Position);
    }

    // ---- which zones a site reaches ---------------------------------------

    [Fact]
    public void AnOperationWellInsideOneZoneTouchesThatZoneAlone()
    {
        var op = new LocationTerrainOperation(ZoneSystem.GetZonePos(new Vector2s(3, -4)), level: true, levelRadius: 4f);

        Assert.Equal(new[] { new Vector2s(3, -4) }, LocationTerrainReader.ZonesTouched(new[] { op }));
    }

    [Fact]
    public void AnOperationAtAZoneCornerTouchesFourZones()
    {
        // Leaving a zone out is how a levelled site gets a step down its edge:
        // the compiler holds one zone's arrays, so the neighbour keeps the
        // ground the client generated.
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(0, 0));
        var corner = new Vector3(centre.x + ZoneSystem.ZoneSize / 2f, 0f, centre.z + ZoneSystem.ZoneSize / 2f);
        var op = new LocationTerrainOperation(corner, level: true, levelRadius: 4f);

        var zones = LocationTerrainReader.ZonesTouched(new[] { op });

        Assert.Equal(4, zones.Count);
        Assert.Contains(new Vector2s(0, 0), zones);
        Assert.Contains(new Vector2s(1, 1), zones);
    }

    [Fact]
    public void ItIsTheRadiusThatDecides()
    {
        // A modifier whose centre is comfortably inside a zone still reaches the
        // next one when its radius is large enough -- MWL_ForestSkull1's smooth
        // radius is 14 m. Taking the zone from the position alone would miss it.
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(0, 0));
        var nearEdge = new Vector3(centre.x + ZoneSystem.ZoneSize / 2f - 5f, 0f, centre.z);

        var small = LocationTerrainReader.ZonesTouched(
            new[] { new LocationTerrainOperation(nearEdge, level: true, levelRadius: 3f) });
        var large = LocationTerrainReader.ZonesTouched(
            new[] { new LocationTerrainOperation(nearEdge, smooth: true, smoothRadius: 14f) });

        Assert.Single(small);
        Assert.Equal(2, large.Count);
    }

    [Fact]
    public void TheZoneListIsOrderedSoTwoRunsAgree()
    {
        Vector3 centre = ZoneSystem.GetZonePos(new Vector2s(0, 0));
        var corner = new Vector3(centre.x + ZoneSystem.ZoneSize / 2f, 0f, centre.z + ZoneSystem.ZoneSize / 2f);
        var ops = new[] { new LocationTerrainOperation(corner, level: true, levelRadius: 6f) };

        Assert.Equal(LocationTerrainReader.ZonesTouched(ops), LocationTerrainReader.ZonesTouched(ops));
        Assert.Equal(
            LocationTerrainReader.ZonesTouched(ops).OrderBy(z => z.x).ThenBy(z => z.y),
            LocationTerrainReader.ZonesTouched(ops));
    }

    // ---- the identity a conversion is recorded under -----------------------

    [Fact]
    public void TwoSitesOfOneTemplateInOneZoneAreDifferentSites()
    {
        var zone = new Vector2s(3, -4);
        string a = LocationTerrainReader.SiteId("MWL_RuinsWell1", new Vector3(100f, 30f, 50f), zone);
        string b = LocationTerrainReader.SiteId("MWL_RuinsWell1", new Vector3(130f, 30f, 50f), zone);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void OneSiteInTwoZonesIsTwoConversions()
    {
        // A zone's compiler holds a zone's arrays; the same site spilling into
        // the next zone is a separate write and a separate record.
        var placement = new Vector3(100f, 30f, 50f);
        Assert.NotEqual(
            LocationTerrainReader.SiteId("MWL_Ruins1", placement, new Vector2s(1, 0)),
            LocationTerrainReader.SiteId("MWL_Ruins1", placement, new Vector2s(2, 0)));
    }

    [Fact]
    public void TheSiteIdSurvivesBeingRecomputed()
    {
        // It is written into a ZDO and read back after a restart, so it has to
        // come from the world and not from anything allocated at runtime.
        var placement = new Vector3(-976.25f, 32f, -1193.75f);
        Assert.Equal(
            LocationTerrainReader.SiteId("MWL_WoodTower2", placement, new Vector2s(-15, -19)),
            LocationTerrainReader.SiteId("MWL_WoodTower2", placement, new Vector2s(-15, -19)));
    }
}
