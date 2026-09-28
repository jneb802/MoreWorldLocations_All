using System;
using System.Collections.Generic;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// What a client without the mod can build.
///
/// Everything the policy says about identity rests on this file being complete
/// and being the STOCK game's. A registry that silently lost rows would block
/// templates that are fine; one taken from a modded server would pass templates
/// that are not. Both failures look like a working check.
/// </summary>
public class StockPrefabRegistryTests
{
    [Fact]
    public void ANameInTheSnapshotIsKnownAndOneOutsideItIsNot()
    {
        StockPrefabRegistry registry = StockPrefabRegistry.FromNames(new[] { "wood_floor", "Greydwarf" });

        Assert.True(registry.Has("wood_floor"));
        Assert.False(registry.Has("MWL_Shrine"));
        Assert.Equal("wood_floor", registry.ResolvesTo("wood_floor"));
        Assert.Null(registry.ResolvesTo("MWL_Shrine"));
    }

    [Fact]
    public void LookupIsCaseSensitiveBecauseTheGamesHashIs()
    {
        // MWL_MaypoleHut1 and MWL_MayPoleHut1 are two different lookups, and
        // folding them would answer a question nobody asked.
        StockPrefabRegistry registry = StockPrefabRegistry.FromNames(new[] { "wood_floor" });

        Assert.True(registry.Has("wood_floor"));
        Assert.False(registry.Has("Wood_Floor"));
    }

    [Fact]
    public void AMalformedLineIsRefusedRatherThanSkipped()
    {
        // A registry that dropped unparsable lines would shrink without saying
        // so, and a smaller registry blocks templates that are fine.
        Assert.Throws<FormatException>(() => StockPrefabRegistry.Parse("#buildid x\nwood_floor\n"));
        Assert.Throws<FormatException>(() => StockPrefabRegistry.Parse("#buildid x\nwood_floor\tnot-a-number\n"));
    }

    [Fact]
    public void ADuplicateNameIsRefusedBecauseWhicheverWonWouldDecideVerdicts()
    {
        Assert.Throws<FormatException>(() =>
            StockPrefabRegistry.Parse("#buildid x\nwood_floor\t1\nwood_floor\t2\n"));
    }

    [Fact]
    public void CommentsAndBlankLinesAreNotRows()
    {
        StockPrefabRegistry registry = StockPrefabRegistry.Parse(
            "# a note\n\n#buildid 123\n\nwood_floor\t" + "wood_floor".GetStableHashCode() + "\n");

        Assert.Equal(1, registry.Count);
        Assert.Equal("123", registry.GameBuildId);
    }

    [Fact]
    public void AnEmptyRegistryMissesEverythingRatherThanThrowing()
    {
        // This is the fallback when the snapshot does not load, and the policy
        // turns it into "unresolved" rather than "blocked". It has to behave.
        Assert.Equal(0, StockPrefabRegistry.Empty.Count);
        Assert.False(StockPrefabRegistry.Empty.Has("wood_floor"));
        Assert.Null(StockPrefabRegistry.Empty.ResolvesTo("wood_floor"));
    }

    // ---- the shipped snapshot, not a fixture -------------------------------

    [Fact]
    public void TheShippedSnapshotLoadsAndNamesItsGameBuild()
    {
        StockPrefabRegistry registry = VerificationData.StockPrefabs;

        Assert.Empty(VerificationData.LoadProblems);
        Assert.Equal("25253764", registry.GameBuildId);
        Assert.Equal(4644, registry.Count);
    }

    [Fact]
    public void EveryHashInTheShippedSnapshotIsTheHashOfItsOwnName()
    {
        // The snapshot was produced by another tool. If its hashes and this
        // code's hashing disagreed, every identity check would be answering a
        // different question from the client's, and nothing else in the suite
        // would notice.
        string? text = VerificationData.ReadResource(VerificationData.StockPrefabsResource);
        Assert.NotNull(text);

        var wrong = new List<string>();
        int rows = 0;
        foreach (string raw in text!.Split('\n'))
        {
            string line = raw.Trim('\r');
            if (line.Length == 0 || line[0] == '#')
                continue;
            rows++;
            string[] parts = line.Split('\t');
            if (parts[0].GetStableHashCode() != int.Parse(parts[1]))
                wrong.Add(parts[0]);
        }

        Assert.Equal(4644, rows);
        Assert.Empty(wrong);
    }

    [Fact]
    public void ThePrefabsTheModAddsAreAbsentFromTheStockSnapshot()
    {
        // Measured, not assumed, and it is the single fact that decides the
        // largest group of exclusions in the catalogue: MWL_Shrine appears in 38
        // templates and MWL_Waystone in 14. They are registered by the mod from
        // its own bundle with its own MonoBehaviours attached (Prefabs.cs), so a
        // client without the mod has nothing to build.
        StockPrefabRegistry registry = VerificationData.StockPrefabs;

        Assert.False(registry.Has("MWL_Shrine"));
        Assert.False(registry.Has("MWL_Waystone"));
        // MD_Kit_widestone is a clone of vanilla 'widestone' with its
        // Destructible removed, registered under a name of its own
        // (Prefabs.MakeMDKitPrefabs). The original IS stock; the clone is not,
        // and it is the clone's name that goes into the ZDO. One template uses
        // it 135 times, so a single unsupported name can decide a whole build.
        Assert.False(registry.Has("MD_Kit_widestone"));
        Assert.True(registry.Has("widestone"));

        // And the check is not vacuous: ordinary vanilla names resolve.
        Assert.True(registry.Has("Greydwarf"));
        Assert.True(registry.Has("piece_chest_wood"));
    }

    [Fact]
    public void TheSnapshotHoldsNetworkedPrefabsOnlyAndThatIsWhatIdentityMeans()
    {
        // ZNetScene.m_prefabs is the list a ZDO's hash is resolved against, so
        // it is the right authority for "can the client build this ZDO" -- and
        // the wrong one for scenery. 'Stoneblock', 'Vines' and 'SacredPillar'
        // are the game's own assets and are absent here, because they carry no
        // ZNetView: they are building blocks, not networked objects.
        //
        // That matters to the verdict, not just the wording. An offline pass
        // that only compared names called all three "unknown prefab"; at runtime
        // one of them is either part of a stock prefab the client builds anyway,
        // or an object in the half of the template a client without the mod
        // never builds. Those are different findings with different fixes, and
        // only the extractor -- which can see whether the object is networked --
        // can tell them apart.
        StockPrefabRegistry registry = VerificationData.StockPrefabs;

        Assert.False(registry.Has("Stoneblock"));
        Assert.False(registry.Has("Vines"));
        Assert.False(registry.Has("SacredPillar"));
        // Not vacuous: vanilla pieces that ARE networked resolve, including ones
        // from the same dvergr kit as pieces that are not.
        Assert.True(registry.Has("Greydwarf"));
        Assert.True(registry.Has("dvergrtown_wood_stake"));
        Assert.False(registry.Has("dvergrtown_metal_wall_2x2"));
    }
}
