using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// What the audit and the terrain reader hold, and what they give back.
///
/// <para>A sweep over 194 templates that leaks one reference each keeps the
/// whole catalogue resident, and the server measured on the station passed five
/// gigabytes before it was stopped. The three owners are a template's asset, a
/// zone's generated heights and a prefab's signature text; each has a bound
/// here, and each gets asked whether removing that bound is noticed.</para>
/// </summary>
public class ByteBudgetCacheTests
{
    private static ByteBudgetCache<string, string> Cache(long budget) =>
        new ByteBudgetCache<string, string>(budget, v => v.Length, StringComparer.Ordinal);

    private static string OfBytes(int bytes) => new string('x', bytes);

    [Fact]
    public void WhatIsHeldIsCountedInBytes()
    {
        ByteBudgetCache<string, string> cache = Cache(100);

        Assert.True(cache.Put("a", OfBytes(40)));
        Assert.True(cache.Put("b", OfBytes(30)));

        Assert.Equal(70, cache.Bytes);
        Assert.Equal(2, cache.Count);
    }

    [Fact]
    public void GoingOverTheBudgetEvictsTheLeastRecentlyUsed()
    {
        // A count is not a memory bound: the two things cached here are 16 kB
        // and anything-at-all. Bytes are what a run can be held to.
        ByteBudgetCache<string, string> cache = Cache(100);
        cache.Put("old", OfBytes(40));
        cache.Put("recent", OfBytes(40));
        cache.Peek("old");                      // touching makes 'recent' the older one

        cache.Put("new", OfBytes(40));

        Assert.True(cache.Bytes <= cache.BudgetBytes);
        Assert.NotNull(cache.Peek("old"));
        Assert.Null(cache.Peek("recent"));
    }

    [Fact]
    public void SomethingInUseIsNeverEvicted()
    {
        // The builder hands a zone's heights over once. If the entry could be
        // dropped between the preflight reading it and the write reading it
        // again, the second read would find nothing and convert against nothing.
        ByteBudgetCache<string, string> cache = Cache(100);
        cache.Put("pinned", OfBytes(60));

        using (ByteBudgetCache<string, string>.Lease lease = cache.Pin("pinned")!)
        {
            cache.Put("wants-room", OfBytes(60));

            Assert.NotNull(cache.Peek("pinned"));
            Assert.Equal(60, lease.Value.Length);
        }

        // And once it is no longer in use it becomes evictable like anything else.
        cache.Put("now-there-is-room", OfBytes(60));
        Assert.Null(cache.Peek("pinned"));
    }

    [Fact]
    public void AValueLargerThanTheWholeBudgetIsUsedAndNotKept()
    {
        // Admitting it would evict everything else and still be over, which is a
        // cache that has stopped being one. The caller still has the value.
        ByteBudgetCache<string, string> cache = Cache(100);
        cache.Put("small", OfBytes(50));

        Assert.False(cache.Put("enormous", OfBytes(500)));

        Assert.Null(cache.Peek("enormous"));
        Assert.NotNull(cache.Peek("small"));
        Assert.Equal(50, cache.Bytes);
    }

    [Fact]
    public void WhenEverythingLeftIsInUseTheNewValueIsNotKeptRatherThanOverflowing()
    {
        ByteBudgetCache<string, string> cache = Cache(100);
        cache.Put("a", OfBytes(60));

        using (cache.Pin("a")!)
        {
            Assert.False(cache.Put("b", OfBytes(60)));
        }

        Assert.True(cache.Bytes <= cache.BudgetBytes);
    }

    [Fact]
    public void ReleasingALeaseTwiceDoesNotUnpinSomebodyElsesUse()
    {
        ByteBudgetCache<string, string> cache = Cache(100);
        cache.Put("a", OfBytes(60));

        ByteBudgetCache<string, string>.Lease first = cache.Pin("a")!;
        ByteBudgetCache<string, string>.Lease second = cache.Pin("a")!;
        first.Dispose();
        first.Dispose();

        // The second use still holds it.
        cache.Put("b", OfBytes(60));
        Assert.NotNull(cache.Peek("a"));

        second.Dispose();
        cache.Put("c", OfBytes(60));
        Assert.Null(cache.Peek("a"));
    }

    [Fact]
    public void AnAdoptedBufferCountsUntilItsLeaseReturns()
    {
        // The gap the review named: a builder result the cache refused was live
        // and outside the reported bytes. Adopted, it is counted while it is
        // used and dropped when its user is done, with no key to find it by.
        ByteBudgetCache<string, string> cache = Cache(100);

        ByteBudgetCache<string, string>.Lease lease = cache.Adopt(OfBytes(40));

        Assert.Equal(40, cache.Bytes);
        Assert.Equal(1, cache.Retired);
        Assert.Equal(0, cache.Count);
        Assert.Null(cache.Peek("anything"));
        lease.Dispose();
        lease.Dispose();
        Assert.Equal(0, cache.Bytes);
        Assert.Equal(0, cache.Retired);
    }

    [Fact]
    public void ReservingRoomEvictsWhatIsNotInUseAndRefusesWhatIs()
    {
        ByteBudgetCache<string, string> cache = Cache(100);
        cache.Put("idle", OfBytes(60));
        cache.Put("busy", OfBytes(30));
        using ByteBudgetCache<string, string>.Lease pin = cache.Pin("busy")!;

        // Room is made from what nobody is reading.
        Assert.True(cache.TryReserve(50));
        Assert.Null(cache.Peek("idle"));
        Assert.Equal(30, cache.Bytes);

        // Everything left is in use: no, and nothing was touched to say so.
        Assert.False(cache.TryReserve(80));
        Assert.Equal(30, cache.Bytes);
        Assert.NotNull(cache.Peek("busy"));
        // Larger than the whole budget can never fit.
        Assert.False(cache.TryReserve(101));
    }

    [Fact]
    public void ClearingDropsEverythingAndTheAccounting()
    {
        ByteBudgetCache<string, string> cache = Cache(100);
        cache.Put("a", OfBytes(40));

        cache.Clear();

        Assert.Equal(0, cache.Bytes);
        Assert.Equal(0, cache.Count);
    }
}

/// <summary>
/// A template's terrain, read once and then held as numbers.
///
/// The modifiers used to be cached as components for the life of the world. A
/// cached component keeps its GameObject, which keeps the template, which keeps
/// the bundle — so the release that would have unloaded it could never take
/// effect, and every template ever read stayed resident.
/// </summary>
public class TerrainTemplateTests
{
    private static GameObject TemplateWithModifier(float x, float levelOffset, int sortOrder = 0)
    {
        var root = new GameObject("Site");
        GameObject node = root.Child("terrain");
        node.transform.position = new Vector3(x, 0f, 0f);
        TerrainModifier modifier = node.AddComponent<TerrainModifier>();
        modifier.m_level = true;
        modifier.m_levelRadius = 4f;
        modifier.m_levelOffset = levelOffset;
        modifier.m_smooth = false;
        modifier.m_paintCleared = false;
        modifier.m_sortOrder = sortOrder;
        return root;
    }

    [Fact]
    public void ADescriptionHoldsNoUnityObject()
    {
        // The whole point. Anything here that referenced a component would pin
        // the template behind it for as long as the description lived.
        TerrainTemplate terrain = TerrainTemplate.Read(TemplateWithModifier(10f, -2f));

        Assert.All(
            typeof(TerrainModifierDescriptor).GetProperties(),
            property => Assert.False(
                typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType)
                || property.PropertyType.Name is "GameObject" or "Transform" or "TerrainModifier",
                $"{property.Name} is a Unity reference and would pin the template"));
        Assert.Single(terrain.Modifiers);
    }

    [Fact]
    public void OperationsAreTheSameAfterTheTemplateIsGone()
    {
        // Read once, then answer forever: a site's ground has to be workable out
        // long after its template has been released.
        GameObject template = TemplateWithModifier(10f, -2f);
        TerrainTemplate terrain = TerrainTemplate.Read(template);

        List<LocationTerrainOperation> before =
            terrain.OperationsAt(new Vector3(100f, 0f, 200f), Quaternion.identity);
        template.transform.GetChild(0).position = new Vector3(999f, 0f, 0f);   // the template moves on
        List<LocationTerrainOperation> after =
            terrain.OperationsAt(new Vector3(100f, 0f, 200f), Quaternion.identity);

        Assert.Equal(before[0].Position.x, after[0].Position.x);
        Assert.Equal(110f, after[0].Position.x);
        Assert.Equal(200f, after[0].Position.z);
        Assert.Equal(-2f, after[0].LevelOffset);
    }

    [Fact]
    public void APlacementsRotationTurnsTheGroundWithIt()
    {
        TerrainTemplate terrain = TerrainTemplate.Read(TemplateWithModifier(10f, -2f));

        List<LocationTerrainOperation> turned =
            terrain.OperationsAt(Vector3.zero, Quaternion.Euler(0f, 90f, 0f));

        // A quarter turn puts a point ten metres east ten metres south.
        Assert.Equal(0f, turned[0].Position.x, 3);
        Assert.Equal(-10f, turned[0].Position.z, 3);
    }

    [Fact]
    public void ReachIsTheSameAtEveryRotation()
    {
        // What lets readiness be asked before PlaceLocations has chosen a
        // rotation: turning a point about the origin cannot move it further from
        // the origin.
        TerrainTemplate terrain = TerrainTemplate.Read(TemplateWithModifier(10f, -2f));

        Assert.Equal(14f, terrain.Reach, 3);   // 10 m out, 4 m radius
    }

    [Fact]
    public void TheAuthoredOrderSurvivesTheDescription()
    {
        // Each modifier reads what the one before it left, so the order is the
        // ground and not the bookkeeping.
        var root = new GameObject("Site");
        foreach ((string name, int order) in new[] { ("a", 2), ("b", 1) })
        {
            GameObject node = root.Child(name);
            TerrainModifier modifier = node.AddComponent<TerrainModifier>();
            modifier.m_level = true;
            modifier.m_levelRadius = 2f;
            modifier.m_sortOrder = order;
            modifier.m_smooth = false;
            modifier.m_paintCleared = false;
        }

        TerrainTemplate terrain = TerrainTemplate.Read(root);
        List<LocationTerrainOperation> operations = terrain.OperationsAt(Vector3.zero, Quaternion.identity);

        Assert.Equal(new[] { 2, 1 }, operations.Select(o => o.SortOrder));
        Assert.Equal(
            new[] { 1, 2 },
            TerrainModifierOrder.Apply(operations, _ => false, o => o.SortOrder).Select(o => o.SortOrder));
    }

    [Fact]
    public void AModifierTheGameWouldSkipIsSkippedHereToo()
    {
        var root = new GameObject("Site");
        TerrainModifier compiled = root.Child("already").AddComponent<TerrainModifier>();
        compiled.m_level = true;
        compiled.m_useTerrainCompiler = true;

        TerrainTemplate terrain = TerrainTemplate.Read(root);

        Assert.Single(terrain.Modifiers);
        Assert.Empty(terrain.OperationsAt(Vector3.zero, Quaternion.identity));
        Assert.Equal(0f, terrain.Reach);
    }
}

/// <summary>
/// The two accounting defects a review found with reproductions against the
/// production cache, ported so they run with the rest of the suite.
///
/// Both are the same mistake: memory that is still alive stops being counted.
/// A budget that forgets what a lease is still holding is not a budget.
/// </summary>
public class RetiredAllocationTests
{
    private static ByteBudgetCache<string, byte[]> Cache(long budget) =>
        new ByteBudgetCache<string, byte[]>(budget, value => value.Length);

    [Fact]
    public void ReplacingAPinnedKeyCannotHideTheOldAllocation()
    {
        // Eight bytes of budget, eight bytes pinned. Replacing the key used to
        // subtract the old entry's bytes while the lease still held the array,
        // so sixteen bytes were alive and the cache reported eight.
        ByteBudgetCache<string, byte[]> cache = Cache(8);
        cache.Put("a", new byte[8]);
        using ByteBudgetCache<string, byte[]>.Lease lease = cache.Pin("a")!;

        bool admitted = cache.Put("a", new byte[8]);
        long retained = lease.Value.Length + (admitted ? cache.Peek("a")!.Length : 0);

        Assert.True(retained <= cache.BudgetBytes, $"retained {retained} over a budget of {cache.BudgetBytes}");
        Assert.True(cache.Bytes >= retained, $"reported {cache.Bytes} while {retained} is alive");
    }

    [Fact]
    public void ClearCannotReportZeroWhileALeaseStillHoldsAnArray()
    {
        // And it is during teardown that something is most likely to be
        // mid-operation, so this is the worst moment to start lying.
        ByteBudgetCache<string, byte[]> cache = Cache(8);
        cache.Put("a", new byte[8]);
        using ByteBudgetCache<string, byte[]>.Lease lease = cache.Pin("a")!;

        cache.Clear();

        Assert.True(cache.Bytes >= lease.Value.Length,
            $"lease holds {lease.Value.Length} and the cache reports {cache.Bytes}");
        Assert.Equal(1, cache.Retired);
    }

    [Fact]
    public void ARetiredAllocationStopsCountingWhenItsLastLeaseReturns()
    {
        ByteBudgetCache<string, byte[]> cache = Cache(8);
        cache.Put("a", new byte[8]);
        ByteBudgetCache<string, byte[]>.Lease lease = cache.Pin("a")!;
        cache.Clear();

        lease.Dispose();

        Assert.Equal(0, cache.Bytes);
        Assert.Equal(0, cache.Retired);
    }

    [Fact]
    public void AnUnpinnedEntryIsDroppedOutrightRatherThanRetired()
    {
        // Retiring is for memory somebody still holds. Everything else goes.
        ByteBudgetCache<string, byte[]> cache = Cache(16);
        cache.Put("a", new byte[8]);

        cache.Clear();

        Assert.Equal(0, cache.Bytes);
        Assert.Equal(0, cache.Retired);
    }
}
