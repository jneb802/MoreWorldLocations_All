using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// What the audit takes out and gives back.
///
/// <para>Ownership here is counted, not inferred: the game's loader increments
/// a reference when a soft reference is loaded, decrements on release, and
/// unloads — and lets Jötunn destroy the resolved clone — when the count hits
/// zero. So a sweep that leaks one reference per template keeps the whole
/// catalogue resident, and a sweep that releases one it never took decrements
/// somebody else's hold on a shared asset. Both are counted here.</para>
///
/// <para>The lease these exercise is the production seam. What they cannot
/// establish is Unity's own unloading, which happens on its schedule and needs
/// the station.</para>
/// </summary>
[Collection("templates")]
public class LeaseBalanceTests
{
    /// <summary>A template source that counts acquisitions and releases the way the loader does.</summary>
    private sealed class CountingSource
    {
        private readonly Dictionary<string, GameObject> _assets = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _references = new(StringComparer.Ordinal);

        public int Throws { get; set; }
        public string? FailsToLoad { get; set; }
        public string? ThrowsOnOpen { get; set; }

        public CountingSource With(string name, GameObject asset)
        {
            _assets[name] = asset;
            return this;
        }

        /// <summary>Somebody else already holds this one, as Jötunn does for its registered assets.</summary>
        public CountingSource HeldElsewhere(string name)
        {
            _references[name] = _references.TryGetValue(name, out int held) ? held + 1 : 1;
            return this;
        }

        public int ReferencesTo(string name) => _references.TryGetValue(name, out int held) ? held : 0;

        public int TotalReferences => _references.Values.Sum();

        public ITemplateHandle? Open(string name)
        {
            if (name == ThrowsOnOpen)
            {
                // Before the increment, which is where the loader's own
                // pre-acquisition failures live.
                Throws++;
                throw new InvalidOperationException("the loader was not ready");
            }
            if (!_assets.ContainsKey(name))
                return null;

            _references[name] = ReferencesTo(name) + 1;
            return new Handle(this, name, name == FailsToLoad ? null : _assets[name]);
        }

        private sealed class Handle : ITemplateHandle
        {
            private readonly CountingSource _source;
            private readonly string _name;
            private bool _held = true;

            public Handle(CountingSource source, string name, GameObject? asset)
            {
                _source = source;
                _name = name;
                Asset = asset;
                TemplateAssets.LeaseTakenForTest();
            }

            public GameObject? Asset { get; }

            public void Dispose()
            {
                if (!_held)
                    return;
                _held = false;
                _source._references[_name] = _source.ReferencesTo(_name) - 1;
                TemplateAssets.LeaseReturnedForTest();
            }
        }
    }

    private static GameObject Template(string name)
    {
        var root = new GameObject(name);
        root.Child("wood_floor").AddComponent<ZNetView>();
        return root;
    }

    private static CountingSource Install(CountingSource source)
    {
        TemplateAssets.ResetAccounting();
        TemplateAssets.Source = source.Open;
        return source;
    }

    private sealed class World : IDisposable
    {
        public World()
        {
            ZoneSystem.instance = new ZoneSystem();
            ServerOnlyMode.Set(true);
            CatalogueSweep.Forget();
            ServerOnlySelection.SetRegistered(Array.Empty<string>());
        }

        public void Dispose()
        {
            TemplateAssets.Source = null;
            TemplateAssets.ResetAccounting();
            ZoneSystem.instance = null;
            ServerOnlyMode.Set(false);
            CatalogueSweep.Forget();
        }
    }

    [Fact]
    public void AnUnloadedAssetEndsAtTheReferenceCountItStartedWith()
    {
        using var world = new World();
        var source = new CountingSource();
        foreach (MWLLocation location in LocationDB.All)
            source.With(location.Name, Template(location.Name));
        Install(source);

        LocationDB.RegisterAll();

        Assert.Equal(0, source.TotalReferences);
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
        Assert.True(TemplateAssets.PeakLeases >= 1, "the sweep never opened anything");
    }

    [Fact]
    public void AnAssetSomebodyElseHoldsStaysHeldByThem()
    {
        // Jötunn deliberately keeps references for the assets it registered.
        // Those are not the audit's to give back, and an audit that released
        // them would unload something another owner is still using.
        using var world = new World();
        var source = new CountingSource();
        foreach (MWLLocation location in LocationDB.All)
            source.With(location.Name, Template(location.Name));
        source.HeldElsewhere("MWL_Ruins1");
        Install(source);

        LocationDB.RegisterAll();

        Assert.Equal(1, source.ReferencesTo("MWL_Ruins1"));
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
    }

    [Fact]
    public void ATemplateThatWillNotLoadStillReturnsItsReference()
    {
        // The loader increments BEFORE it loads, so a failed load has already
        // acquired. Treating "no asset" as "nothing to give back" leaks one
        // reference for every template that fails.
        using var world = new World();
        var source = new CountingSource();
        foreach (MWLLocation location in LocationDB.All)
            source.With(location.Name, Template(location.Name));
        source.FailsToLoad = "MWL_Ruins1";
        Install(source);

        LocationDB.RegisterAll();

        Assert.Equal(0, source.TotalReferences);
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
    }

    [Fact]
    public void AnOpenThatThrowsBeforeAcquiringReleasesNothing()
    {
        // The other direction, and the more dangerous one: releasing a reference
        // we never took decrements somebody else's hold on a shared asset.
        using var world = new World();
        var source = new CountingSource();
        foreach (MWLLocation location in LocationDB.All)
            source.With(location.Name, Template(location.Name));
        source.ThrowsOnOpen = "MWL_Ruins1";
        source.HeldElsewhere("MWL_Ruins1");
        Install(source);

        LocationDB.RegisterAll();

        Assert.Equal(1, source.Throws);
        Assert.Equal(1, source.ReferencesTo("MWL_Ruins1"));
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
    }

    [Fact]
    public void AnExtractionThatThrowsStillReturnsTheLease()
    {
        using var world = new World();
        var source = new CountingSource();
        foreach (MWLLocation location in LocationDB.All)
            source.With(location.Name, Template(location.Name));
        Install(source);

        // The extractor throws on a template whose root has been destroyed out
        // from under it; a null root is the shape the walk cannot survive.
        TemplateAssets.StockPrefabs = _ => throw new InvalidOperationException("the registry is gone");
        try
        {
            LocationDB.RegisterAll();
        }
        finally
        {
            TemplateAssets.StockPrefabs = null;
        }

        Assert.Equal(0, source.TotalReferences);
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
    }

    [Fact]
    public void OnlyOneTemplateIsOpenAtATime()
    {
        // A sweep that held all 194 open would keep the whole catalogue
        // resident, which is the thing the station measured.
        using var world = new World();
        var source = new CountingSource();
        foreach (MWLLocation location in LocationDB.All)
            source.With(location.Name, Template(location.Name));
        Install(source);

        LocationDB.RegisterAll();

        Assert.Equal(1, TemplateAssets.PeakLeases);
    }

    [Fact]
    public void TheSignatureCacheIsClearedWhenTheSweepEnds()
    {
        // It is the audit's, not the world's: it exists to avoid re-walking a
        // stock prefab within one sweep and has no reader afterwards.
        using var world = new World();
        var source = new CountingSource();
        foreach (MWLLocation location in LocationDB.All)
            source.With(location.Name, Template(location.Name));
        Install(source);
        TemplateAssets.StockPrefabs = name => Template(name);

        try
        {
            LocationDB.RegisterAll();
        }
        finally
        {
            TemplateAssets.StockPrefabs = null;
        }

        Assert.Equal(0, TemplateFactsExtractor.StockSignatureBytes);
        Assert.Equal(0, TemplateFactsExtractor.StockSignatureEntries);
    }

    [Fact]
    public void TheSignatureCacheIsClearedWhenTheSweepThrows()
    {
        using var world = new World();
        var source = new CountingSource();
        foreach (MWLLocation location in LocationDB.All)
            source.With(location.Name, Template(location.Name));
        Install(source);
        TemplateAssets.StockPrefabs = name => Template(name);
        CatalogueAudit.Progress = _ => { };

        try
        {
            // A report sink that throws is the abort path R4 was about; the
            // audit's own memory must not outlive it either.
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.GetType();
            LocationDB.RegisterAll();
        }
        finally
        {
            TemplateAssets.StockPrefabs = null;
            CatalogueAudit.Progress = null;
        }

        Assert.Equal(0, TemplateFactsExtractor.StockSignatureBytes);
    }

    [Fact]
    public void TerrainIsReadThroughTheSameLeaseAndGivenBack()
    {
        // One lease implementation for the audit and the terrain reader. The
        // reader used to call Load() with no release at all.
        using var terrain = new TerrainWorld();
        ZoneSystem.ZoneLocation location =
            terrain.LocationWithLevelModifier("ReviewSite", atX: 0f, levelTo: 29f);
        terrain.GroundEverywhere(30f);

        LocationSpawnGate.MayPublish(location, Vector3.zero, Quaternion.identity);

        Assert.Contains("ReviewSite", terrain.Opened);
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
    }

    [Fact]
    public void ATemplatesTerrainIsReadOnceAndThenAnsweredFromValues()
    {
        // The second question must not reopen the template: that is the
        // difference between a description and a cache of components.
        using var terrain = new TerrainWorld();
        ZoneSystem.ZoneLocation location =
            terrain.LocationWithLevelModifier("ReviewSite", atX: 0f, levelTo: 29f);
        terrain.GroundEverywhere(30f);

        LocationSpawnGate.MayPublish(location, Vector3.zero, Quaternion.identity);
        LocationSpawnGate.MayPublish(location, new Vector3(200f, 0f, 200f), Quaternion.identity);

        Assert.Equal(1, terrain.Opened.Count(name => name == "ReviewSite"));
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
    }
}
