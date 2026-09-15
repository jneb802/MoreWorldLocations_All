using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// What a failed or repeated terrain write must not leave behind.
///
/// The review of 9657742 found <c>ApplyOnce</c> recording completion before
/// doing the work: one exception from a height read produced a site that was
/// marked done, was not done, and had some of its vertices already moved, with
/// the retry refusing to touch it. Its two regressions live in
/// <c>ReviewTerrainFailureTests</c>; these are the controls around them —
/// that the fix preserves terrain which was already there rather than only
/// terrain that happened to be zero, and that a retry after a failure gives the
/// same answer as a clean first run.
/// </summary>
public class TerrainFailureTests
{
    private static TerrainZoneDeltas Zone() => new TerrainZoneDeltas(new Vector3(0f, 0f, 0f));

    private static LocationTerrainOperation Sink() => new(
        new Vector3(0f, 30f, 0f), level: true, levelRadius: 3f, levelOffset: -2f, square: true,
        paint: true, paintRadius: 3f, paintType: TerrainModifier.PaintType.Dirt);

    private static float Flat(int x, int y) => 30f;

    /// <summary>A height reader that throws on the nth call, counting from one.</summary>
    private static TerrainConversion.VertexHeight ThrowsOnCall(int n)
    {
        int calls = 0;
        return (x, y) =>
        {
            if (++calls == n) throw new InvalidOperationException("synthetic read failure");
            return 30f;
        };
    }

    [Fact]
    public void AFailedConversionLeavesTerrainThatWasAlreadyThereExactlyAsItWas()
    {
        // The review's own regression starts from empty arrays, where "restored"
        // and "zeroed" look the same. A zone that already carries a road, or an
        // earlier modifier of the same site, must come back byte for byte.
        TerrainZoneDeltas zone = Zone();
        TerrainConversion.ApplyOnce("site/first", Sink(), zone, Flat);

        float[] levelBefore = (float[])zone.LevelDelta.Clone();
        float[] smoothBefore = (float[])zone.SmoothDelta.Clone();
        bool[] heightBefore = (bool[])zone.ModifiedHeight.Clone();
        Color[] paintBefore = (Color[])zone.PaintMask.Clone();
        bool[] paintedBefore = (bool[])zone.ModifiedPaint.Clone();

        Assert.Throws<InvalidOperationException>(() =>
            TerrainConversion.ApplyOnce("site/second", Sink(), zone, ThrowsOnCall(3)));

        Assert.Equal(levelBefore, zone.LevelDelta);
        Assert.Equal(smoothBefore, zone.SmoothDelta);
        Assert.Equal(heightBefore, zone.ModifiedHeight);
        Assert.Equal(paintedBefore, zone.ModifiedPaint);
        for (int i = 0; i < paintBefore.Length; i++)
        {
            Assert.Equal(paintBefore[i].r, zone.PaintMask[i].r, 6);
            Assert.Equal(paintBefore[i].g, zone.PaintMask[i].g, 6);
            Assert.Equal(paintBefore[i].b, zone.PaintMask[i].b, 6);
            Assert.Equal(paintBefore[i].a, zone.PaintMask[i].a, 6);
        }
        Assert.False(zone.HasApplied("site/second"));
    }

    [Fact]
    public void ARetryAfterAFailureLandsWhereACleanRunWouldHave()
    {
        // The point of not recording completion on failure: the retry has to be
        // able to finish the job, and finish it in the same place. If the failed
        // attempt had left a single vertex behind, this would come out low.
        TerrainZoneDeltas failedThenRetried = Zone();
        Assert.Throws<InvalidOperationException>(() =>
            TerrainConversion.ApplyOnce("site/terrain", Sink(), failedThenRetried, ThrowsOnCall(5)));
        Assert.True(TerrainConversion.ApplyOnce("site/terrain", Sink(), failedThenRetried, Flat));

        TerrainZoneDeltas clean = Zone();
        Assert.True(TerrainConversion.ApplyOnce("site/terrain", Sink(), clean, Flat));

        Assert.Equal(clean.LevelDelta, failedThenRetried.LevelDelta);
        Assert.Equal(clean.SmoothDelta, failedThenRetried.SmoothDelta);
        Assert.Equal(clean.ModifiedHeight, failedThenRetried.ModifiedHeight);
    }

    [Fact]
    public void AFailureOnTheVeryFirstReadIsTreatedNoDifferently()
    {
        TerrainZoneDeltas zone = Zone();
        Assert.Throws<InvalidOperationException>(() =>
            TerrainConversion.ApplyOnce("site/terrain", Sink(), zone, ThrowsOnCall(1)));

        Assert.False(zone.HasApplied("site/terrain"));
        Assert.All(zone.ModifiedHeight, Assert.False);
        Assert.All(zone.ModifiedPaint, Assert.False);
    }

    [Fact]
    public void AFailureLateInTheConversionDoesNotKeepTheHeightsThatSucceeded()
    {
        // Heights are written before paint. Publishing per pass rather than per
        // conversion would leave the ground cut and the mask untouched, which is
        // a state no retry can reason about.
        TerrainZoneDeltas zone = Zone();

        // Count the reads a whole conversion makes, then fail on the last one --
        // after the authored heights have been worked out and the deltas written
        // into the scratch zone.
        int reads = 0;
        TerrainConversion.Convert(new[] { Sink() }, Zone(), (x, y) => { reads++; return 30f; });
        Assert.True(reads > 1, "the conversion has to read more than once for this to mean anything");

        Assert.Throws<InvalidOperationException>(() =>
            TerrainConversion.ApplyOnce("site/terrain", Sink(), zone, ThrowsOnCall(reads)));

        Assert.All(zone.ModifiedHeight, Assert.False);
        Assert.All(zone.ModifiedPaint, Assert.False);
        Assert.False(zone.HasApplied("site/terrain"));
    }

    [Fact]
    public void ApplyOnceRefusesAHeightReaderThatIsNotThere()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TerrainConversion.ApplyOnce("site/terrain", Sink(), Zone(), null!));
    }

    // --- completion has to outlive the process ----------------------------

    [Fact]
    public void ARestoredCompletionRecordStillRefusesTheRepeat()
    {
        // The in-memory set proves repeat protection on one object. A server
        // that restarts mid-bake gets a new object, and the record has to come
        // back with it or the site is written a second time and sinks twice.
        TerrainZoneDeltas before = Zone();
        Assert.True(TerrainConversion.ApplyOnce("MWL_RuinsWell1@8,12/Terrain1", Sink(), before, Flat));
        string saved = before.SerializeApplied();

        // What a restart looks like: the compiler's own arrays come back from
        // the game's save, the completion record from ours.
        TerrainZoneDeltas afterRestart = Zone();
        afterRestart.AdoptTerrainFrom(before);
        afterRestart.DeserializeApplied(saved);

        Assert.True(afterRestart.HasApplied("MWL_RuinsWell1@8,12/Terrain1"));
        Assert.False(TerrainConversion.ApplyOnce("MWL_RuinsWell1@8,12/Terrain1", Sink(), afterRestart, Flat));
        Assert.Equal(before.LevelDelta, afterRestart.LevelDelta);
    }

    [Fact]
    public void ACompletionRecordThatWasNotSavedLetsTheSiteBePaintedTwice()
    {
        // The negative half of the one above, stated so the consequence of
        // dropping the record is on the record rather than assumed.
        //
        // Heights survive a repeat: they are stated absolutely, so a second pass
        // lands on the same ground. Paint does not. Heightmap.PaintCleared lerps
        // the mask TOWARDS a colour, so running it again moves the mask further
        // and the site comes out a different shade of dirt than the author drew.
        // That is what the record is for now that heights are safe.
        TerrainZoneDeltas before = Zone();
        TerrainConversion.ApplyOnce("site/terrain", Sink(), before, Flat);

        TerrainZoneDeltas afterRestart = Zone();
        afterRestart.AdoptTerrainFrom(before);
        // ...and no DeserializeApplied.

        Assert.True(TerrainConversion.ApplyOnce("site/terrain", Sink(), afterRestart, Flat));

        TerrainConversion.WorldToVertex(afterRestart, new Vector3(0f, 30f, 0f), out int cx, out int cy);
        int centre = afterRestart.Index(cx, cy);
        Assert.Equal(before.LevelDelta[centre], afterRestart.LevelDelta[centre], 3);

        // Away from the centre, where the weight is below 1 and the lerp has not
        // already arrived: at the centre itself the first pass saturates and a
        // second changes nothing, which would make this pass for the wrong
        // reason.
        TerrainConversion.WorldToVertexMask(afterRestart, new Vector3(-0.5f, 30f, -0.5f), out int mx, out int my);
        int mask = afterRestart.Index(mx + 2, my);
        Assert.True(before.PaintMask[mask].r < 1f, "the sample has to be a texel the lerp had not finished");
        Assert.True(afterRestart.PaintMask[mask].r > before.PaintMask[mask].r,
            "a second pass moves the mask further towards the paint colour");
    }

    [Fact]
    public void TheCompletionRecordRoundTripsAndIsOrderedTheSameWayEveryTime()
    {
        TerrainZoneDeltas zone = Zone();
        foreach (string id in new[] { "b/one", "a/two", "c/three" })
            TerrainConversion.ApplyOnce(id, Sink(), zone, Flat);

        string saved = zone.SerializeApplied();
        Assert.Equal("a/two\nb/one\nc/three", saved);

        TerrainZoneDeltas restored = Zone();
        restored.DeserializeApplied(saved);
        Assert.Equal(zone.AppliedOperations.ToArray(), restored.AppliedOperations.ToArray());
    }

    [Fact]
    public void AnEmptyCompletionRecordReadsAsNothingWritten()
    {
        TerrainZoneDeltas zone = Zone();
        zone.DeserializeApplied("");
        zone.DeserializeApplied(null!);
        Assert.Empty(zone.AppliedOperations);
    }

    [Fact]
    public void ASavedRecordWithAnEmptyIdentityIsRefusedRatherThanRead()
    {
        // A blank identity would match nothing and hide the real one. Better a
        // loud read failure than a site quietly eligible for a second write.
        TerrainZoneDeltas zone = Zone();
        Assert.Throws<ArgumentException>(() => zone.RestoreApplied(new[] { "site/terrain", "" }));
    }
}
