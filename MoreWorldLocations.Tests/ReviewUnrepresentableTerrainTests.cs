using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

public class ReviewUnrepresentableTerrainTests
{
    [Fact]
    public void AnUnrepresentableSiteIsNotPublishedOrMarkedComplete()
    {
        var zone = new TerrainZoneDeltas(new Vector3(0f,0f,0f));
        var operation = new LocationTerrainOperation(
            new Vector3(0f,10f,0f), level: true, levelRadius: 1f, square: true);
        bool written = TerrainConversion.ApplyOnce("site@0,0", new[] { operation },
            zone, (x,y) => 30f, out var result);
        Assert.False(result.Representable); // actual -20 m cut exceeds the compiler's range
        // Preview is optional at this API; the authoritative commit must enforce rejection.
        Assert.All(zone.LevelDelta, value => Assert.Equal(0f, value));
        Assert.All(zone.SmoothDelta, value => Assert.Equal(0f, value));
        Assert.All(zone.ModifiedHeight, value => Assert.False(value));
        Assert.False(zone.HasApplied("site@0,0"));
        Assert.False(written);
    }
}
