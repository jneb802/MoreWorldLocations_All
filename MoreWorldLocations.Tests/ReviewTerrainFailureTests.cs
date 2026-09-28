using System;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

public class ReviewTerrainFailureTests
{
    private static LocationTerrainOperation Operation() => new(
        new Vector3(0f, 28f, 0f), level: true, levelRadius: 1f, square: true);

    [Fact]
    public void AConversionThatThrowsBeforeWritingIsNotRecordedAsApplied()
    {
        var zone = new TerrainZoneDeltas(new Vector3(0f, 0f, 0f));
        Assert.Throws<InvalidOperationException>(() => TerrainConversion.ApplyOnce(
            "site/modifier", Operation(), zone,
            (x, y) => throw new InvalidOperationException("synthetic read failure")));
        Assert.False(zone.HasApplied("site/modifier"));
        Assert.True(TerrainConversion.ApplyOnce("site/modifier", Operation(), zone, (x,y) => 30f));
        Assert.Equal(-2f, zone.LevelDelta[zone.Index(16,16)]);
    }

    [Fact]
    public void APartialConversionFailureLeavesTheSuppliedTerrainUnchanged()
    {
        var zone = new TerrainZoneDeltas(new Vector3(0f, 0f, 0f));
        int calls = 0;
        Assert.Throws<InvalidOperationException>(() => TerrainConversion.ApplyOnce(
            "site/modifier", Operation(), zone, (x,y) => {
                if (++calls == 2) throw new InvalidOperationException("mid-operation read failure");
                return 30f;
            }));
        // Merely moving RecordApplied below Apply does not satisfy this:
        // retrying partially changed arrays would apply the first vertex twice.
        Assert.All(zone.LevelDelta, value => Assert.Equal(0f, value));
        Assert.All(zone.SmoothDelta, value => Assert.Equal(0f, value));
        Assert.All(zone.ModifiedHeight, value => Assert.False(value));
    }
}
