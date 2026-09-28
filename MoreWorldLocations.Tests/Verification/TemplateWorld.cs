using System;
using System.Collections.Generic;
using More_World_Locations_AIO;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>A foreign behaviour: a type from this assembly, which no client has.</summary>
public sealed class TestOnlyClientBehaviour : Component { }

/// <summary>A second one, so a fixture can tell two custom scripts apart.</summary>
public sealed class TestOnlySiteDriver : Component { }

/// <summary>
/// A world of templates to audit: an asset table, a stock prefab table, and the
/// location list the sweep enforces against.
///
/// <para>Registration here makes a location available immediately, which is the
/// BEST case for timing. That matters: if the audit cannot inspect an
/// unregistered template even when registration is instantaneous, no amount of
/// real asset latency would rescue it — so a failure under these doubles is a
/// failure about the code, not about the doubles.</para>
///
/// <para>What they cannot establish is when Harmony runs a hook, or what the
/// game does with the list afterwards. Those need a running world.</para>
/// </summary>
public sealed class TemplateWorld : IDisposable
{
    private readonly Dictionary<string, GameObject> _assets = new(StringComparer.Ordinal);
    private readonly Dictionary<string, GameObject> _stock = new(StringComparer.Ordinal);
    private readonly List<string> _opened = new();
    private readonly List<string> _released = new();

    public TemplateWorld()
    {
        ZoneSystem.instance = new ZoneSystem();
        ServerOnlyMode.Set(true);
        CatalogueSweep.Forget();
        CatalogueSweep.ScheduleRoutine = null;
        CatalogueSweep.ReleaseGeneration = null;
        TemplateAssets.Preloader = null;
        TemplateAssets.IsSettled = null;
        LocationDB.LateRegistration = null;
        ServerOnlySelection.SetRegistered(Array.Empty<string>());
        TemplateAssets.Source = name => _assets.TryGetValue(name, out GameObject asset)
            ? new Handle(this, name, asset)
            : null;
        TemplateAssets.StockPrefabs = name => _stock.TryGetValue(name, out GameObject prefab) ? prefab : null;
    }

    /// <summary>Templates opened by the audit, in order, so a test can say WHICH were inspected.</summary>
    public IReadOnlyList<string> Opened => _opened;

    /// <summary>Templates released again. A sweep that held every template would run a server out of memory.</summary>
    public IReadOnlyList<string> Released => _released;

    public TemplateWorld WithAsset(string name, GameObject template)
    {
        _assets[name] = template;
        return this;
    }

    /// <summary>A stock prefab to compare authored objects against.</summary>
    public TemplateWorld WithStock(string name, GameObject prefab)
    {
        _stock[name] = prefab;
        return this;
    }

    /// <summary>Every definition gets a plain, passing template unless a test replaces it.</summary>
    public TemplateWorld WithPlainAssetsForEveryDefinition()
    {
        foreach (MWLLocation location in LocationDB.All)
            _assets[location.Name] = Templates.Stock(location.Name);
        return this;
    }

    public GameObject Asset(string name) => _assets[name];

    public bool IsInWorld(string name) =>
        ZoneSystem.instance!.m_locationsByHash.ContainsKey(name.GetStableHashCode());

    /// <summary>Run the hook's body directly. Harmony's own timing is not modelled; see the type remarks.</summary>
    public static void RunEnforcementHook()
    {
        typeof(CatalogueSweepPatch)
            .GetMethod("Postfix", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, null);
    }

    public void Dispose()
    {
        TemplateAssets.Source = null;
        TemplateAssets.StockPrefabs = null;
        ZoneSystem.instance = null;
        ServerOnlyMode.Set(false);
        CatalogueSweep.Forget();
        CatalogueSweep.ScheduleRoutine = null;
        CatalogueSweep.ReleaseGeneration = null;
        TemplateAssets.Preloader = null;
        TemplateAssets.IsSettled = null;
        LocationDB.LateRegistration = null;
        BepInEx.Logging.ManualLogSource.ThrowOnNextInfo = false;
    }

    private sealed class Handle : ITemplateHandle
    {
        private readonly TemplateWorld _world;
        private readonly string _name;

        public Handle(TemplateWorld world, string name, GameObject asset)
        {
            _world = world;
            _name = name;
            Asset = asset;
            world._opened.Add(name);
        }

        public GameObject? Asset { get; }

        public void Dispose() => _world._released.Add(_name);
    }
}

/// <summary>Templates to judge, built by hand from the Unity doubles.</summary>
public static class Templates
{
    /// <summary>
    /// A template a stock client can build: one networked child of a vanilla
    /// prefab, with nothing hung underneath it.
    ///
    /// Everything else in these fixtures is this with one thing changed.
    /// </summary>
    public static GameObject Stock(string name = "Test_Template", string child = "wood_floor")
    {
        var root = new GameObject(name);
        root.Child(child).AddComponent<ZNetView>();
        return root;
    }

    /// <summary>A stock prefab with the given children, for use as a baseline.</summary>
    public static GameObject StockPrefab(string name, params string[] children)
    {
        var prefab = new GameObject(name);
        foreach (string child in children)
            prefab.Child(child);
        return prefab;
    }

    public static GameObject ChildOf(GameObject root, int index) => root.transform.GetChild(index).gameObject;
}
