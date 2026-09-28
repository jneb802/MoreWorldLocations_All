using System;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// When a site's terrain counts as finished.
///
/// A site's radius does not respect zone boundaries, and each zone's compiler is
/// written and saved separately. So one zone answering yes says nothing about
/// the other three, and a bake that opens a site's terrain to players on that
/// answer opens a site that is a quarter shaped. The ledger derives completion
/// from the outcomes it planned for, which is the invariant the plan asks for
/// and the one an empty queue does not give you.
/// </summary>
public class SiteTerrainLedgerTests
{
    private static readonly ZoneKey NorthWest = new(8, 12);
    private static readonly ZoneKey NorthEast = new(9, 12);
    private static readonly ZoneKey SouthWest = new(8, 11);
    private static readonly ZoneKey SouthEast = new(9, 11);

    private static SiteTerrainLedger AcrossFourZones()
    {
        SiteTerrainLedger ledger = new("MWL_RuinsWell1@1042,-318");
        foreach (ZoneKey zone in new[] { NorthWest, NorthEast, SouthWest, SouthEast })
            ledger.Require(zone, "Terrain1");
        return ledger;
    }

    [Fact]
    public void ASiteIsNotFinishedWhileOneZoneIsStillOwed()
    {
        SiteTerrainLedger ledger = AcrossFourZones();
        ledger.MarkWritten(NorthWest, "Terrain1");
        ledger.MarkWritten(NorthEast, "Terrain1");
        ledger.MarkWritten(SouthWest, "Terrain1");

        Assert.False(ledger.IsComplete);
        Assert.Equal(new[] { "Terrain1@9,11" }, ledger.Outstanding.ToArray());

        ledger.MarkWritten(SouthEast, "Terrain1");
        Assert.True(ledger.IsComplete);
        Assert.Empty(ledger.Outstanding);
    }

    [Fact]
    public void ASiteWithNothingPlannedIsNotFinished()
    {
        // The dangerous reading of "nothing outstanding". An empty plan means
        // planning has not run, and calling that done is how a site that was
        // never shaped gets a pass.
        SiteTerrainLedger ledger = new("MWL_Ruins1@0,0");
        Assert.False(ledger.IsComplete);
        Assert.Equal(0, ledger.RequiredCount);
    }

    [Fact]
    public void CountingWrittenWorkIsNotTheSameAsCountingTheRightWork()
    {
        // Four writes and four requirements, but not the same four. A ledger
        // that compared totals would call this finished; comparing identities is
        // what catches it.
        SiteTerrainLedger ledger = AcrossFourZones();
        ledger.MarkWritten(NorthWest, "Terrain1");
        ledger.MarkWritten(NorthWest, "Terrain1");
        ledger.MarkWritten(NorthEast, "Terrain1");

        Assert.Equal(4, ledger.RequiredCount);
        Assert.Equal(2, ledger.WrittenCount);
        Assert.False(ledger.IsComplete);
    }

    [Fact]
    public void WritingSomethingNobodyPlannedIsAnError()
    {
        // A write outside the planned footprint means the planner and the writer
        // disagree about where the site is, and a site that quietly grew is a
        // site whose reservation no longer covers it.
        SiteTerrainLedger ledger = AcrossFourZones();
        Assert.Throws<InvalidOperationException>(() => ledger.MarkWritten(new ZoneKey(50, 50), "Terrain1"));
        Assert.Throws<InvalidOperationException>(() => ledger.MarkWritten(NorthWest, "Terrain2"));
    }

    [Fact]
    public void TwoModifiersInOneZoneAreTwoPiecesOfWork()
    {
        SiteTerrainLedger ledger = new("MWL_RuinsArena1@0,0");
        ledger.Require(NorthWest, "Baselevel1");
        ledger.Require(NorthWest, "Baselevel2");

        ledger.MarkWritten(NorthWest, "Baselevel1");
        Assert.False(ledger.IsComplete);
        ledger.MarkWritten(NorthWest, "Baselevel2");
        Assert.True(ledger.IsComplete);
    }

    // --- the record has to outlive the process ----------------------------

    [Fact]
    public void APartlyWrittenSiteComesBackPartlyWritten()
    {
        SiteTerrainLedger ledger = AcrossFourZones();
        ledger.MarkWritten(NorthWest, "Terrain1");
        ledger.MarkWritten(SouthEast, "Terrain1");

        SiteTerrainLedger restored = SiteTerrainLedger.Deserialize(ledger.Serialize());

        Assert.Equal(ledger.SiteId, restored.SiteId);
        Assert.False(restored.IsComplete);
        Assert.Equal(ledger.Outstanding.ToArray(), restored.Outstanding.ToArray());
        Assert.True(restored.IsWritten(NorthWest, "Terrain1"));
        Assert.False(restored.IsWritten(NorthEast, "Terrain1"));
    }

    [Fact]
    public void AFinishedSiteComesBackFinishedAndOwesNothing()
    {
        SiteTerrainLedger ledger = AcrossFourZones();
        foreach (ZoneKey zone in new[] { NorthWest, NorthEast, SouthWest, SouthEast })
            ledger.MarkWritten(zone, "Terrain1");

        SiteTerrainLedger restored = SiteTerrainLedger.Deserialize(ledger.Serialize());
        Assert.True(restored.IsComplete);
        Assert.Empty(restored.Outstanding);
    }

    [Fact]
    public void TheSameStateAlwaysSerialisesToTheSameBytes()
    {
        // Declared in one order, written in another; the record must not depend
        // on either, or two identical worlds would compare as different.
        SiteTerrainLedger a = new("site");
        a.Require(SouthEast, "Terrain1");
        a.Require(NorthWest, "Terrain1");
        a.MarkWritten(SouthEast, "Terrain1");

        SiteTerrainLedger b = new("site");
        b.Require(NorthWest, "Terrain1");
        b.Require(SouthEast, "Terrain1");
        b.MarkWritten(SouthEast, "Terrain1");

        Assert.Equal(a.Serialize(), b.Serialize());
    }

    [Fact]
    public void ASavedRecordClaimingWorkItNeverPlannedIsRefused()
    {
        // A truncated or hand-edited record must fail loudly: read leniently and
        // a site becomes "finished" on the strength of a line nobody planned.
        string tampered = "site=MWL_Ruins1@0,0\nrequired=Terrain1@8,12\nwritten=Terrain1@8,12|Terrain1@9,12";
        Assert.Throws<FormatException>(() => SiteTerrainLedger.Deserialize(tampered));
    }

    [Fact]
    public void ARecordWithNoSiteIdentityIsRefused()
    {
        Assert.Throws<FormatException>(() =>
            SiteTerrainLedger.Deserialize("required=Terrain1@8,12\nwritten="));
        Assert.Throws<FormatException>(() => SiteTerrainLedger.Deserialize("nonsense"));
        Assert.Throws<ArgumentException>(() => SiteTerrainLedger.Deserialize(""));
    }

    [Fact]
    public void AnIdentityThatWouldCorruptTheRecordIsRefusedWhenItIsPlanned()
    {
        // Pairs are stored as "<operation>@<zone>", one per line. An operation
        // name carrying either separator would come back as a different pair,
        // and the site would owe work nobody could write.
        SiteTerrainLedger ledger = new("site");
        Assert.Throws<ArgumentException>(() => ledger.Require(NorthWest, "Terrain@1"));
        Assert.Throws<ArgumentException>(() => ledger.Require(NorthWest, "Terrain\n1"));
        Assert.Throws<ArgumentException>(() => ledger.Require(NorthWest, ""));
    }

    [Fact]
    public void ASiteNeedsAnIdentity()
    {
        Assert.Throws<ArgumentException>(() => new SiteTerrainLedger(""));
    }

    // --- zone keys --------------------------------------------------------

    [Fact]
    public void ZoneKeysCompareByValueAndRoundTrip()
    {
        // The ledger keys on these, so two references to zone 8,12 have to be
        // the same key and not two entries that both look right in a report.
        Assert.Equal(new ZoneKey(8, 12), new ZoneKey(8, 12));
        Assert.NotEqual(new ZoneKey(8, 12), new ZoneKey(12, 8));
        Assert.Equal(new ZoneKey(-3, 4), ZoneKey.Parse(new ZoneKey(-3, 4).ToString()));
        Assert.Throws<FormatException>(() => ZoneKey.Parse("8"));
        Assert.Throws<FormatException>(() => ZoneKey.Parse("eight,twelve"));
    }
}
