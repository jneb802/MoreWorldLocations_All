using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// A world with ground in it: the terrain builder, a location placed in a zone,
/// and the registration the gate and the barrier read.
///
/// <para>What it models is the data those two decide from — which zones have
/// built ground, where a location is, what its template does to the terrain. It
/// does not model Unity's lifecycle, Harmony's timing, vanilla's placement
/// bookkeeping or a client's physics, and a test that needed any of those would
/// be testing the double.</para>
/// </summary>
public sealed class TerrainWorld : IDisposable
{
    public TerrainWorld()
    {
        ZoneSystem.instance = new ZoneSystem();
        ZDOMan.instance = new ZDOMan();
        World = new SyntheticWorld();
        WorldGenerator.instance = World;
        Builder = new HeightmapBuilder();
        HeightmapBuilder.instance = Builder;
        Heightmap.Registered = null;
        Heightmap.Loaded.Clear();
        ServerOnlyMode.Set(true);
        LocationTerrainWriter.Reset();
        LocationTerrainPatch.Forget();
        LocationSpawnGate.Forget();
        ZoneReadinessBarrier.Forget();
        ServerOnlySelection.SetRegistered(Array.Empty<string>());
        TemplateAssets.ResetAccounting();
        // Templates reach the terrain reader through the same lease the audit
        // uses. That is the point of the shared seam: a test that handed the
        // reader a GameObject directly would not exercise the ownership at all.
        TemplateAssets.Source = name => _assets.TryGetValue(name, out GameObject asset)
            ? new Lease(this, name, asset)
            : null;
    }

    private readonly Dictionary<string, GameObject> _assets = new(StringComparer.Ordinal);
    private readonly List<string> _opened = new();

    /// <summary>Templates opened through the lease, in order.</summary>
    public IReadOnlyList<string> Opened => _opened;

    private sealed class Lease : ITemplateHandle
    {
        private readonly TerrainWorld _world;
        private bool _held = true;

        public Lease(TerrainWorld world, string name, GameObject asset)
        {
            _world = world;
            Asset = asset;
            world._opened.Add(name);
            TemplateAssets.LeaseTakenForTest();
        }

        public GameObject? Asset { get; }

        public void Dispose()
        {
            if (!_held)
                return;
            _held = false;
            TemplateAssets.LeaseReturnedForTest();
        }
    }

    public SyntheticWorld World { get; }
    public HeightmapBuilder Builder { get; }

    /// <summary>Build ground at one height for the zones a site near the origin can reach.</summary>
    public void GroundEverywhere(float height)
    {
        World.FlatHeight = height;
        for (int x = -2; x <= 2; x++)
            for (int y = -2; y <= 2; y++)
                Builder.Build(new Vector2s(x, y), World, TerrainZoneDeltas.ZoneWidth);
    }

    /// <summary>
    /// A registered location whose template levels the ground at one point.
    /// </summary>
    public ZoneSystem.ZoneLocation LocationWithLevelModifier(
        string name, float atX, float levelTo, float radius = 2f)
    {
        var asset = new GameObject(name);
        GameObject node = asset.Child("terrain");
        node.transform.position = new Vector3(atX, 0f, 0f);
        TerrainModifier modifier = node.AddComponent<TerrainModifier>();
        modifier.m_level = true;
        modifier.m_levelRadius = radius;
        modifier.m_levelOffset = levelTo;
        modifier.m_smooth = false;
        modifier.m_paintCleared = false;

        var location = new ZoneSystem.ZoneLocation { m_prefabName = name };
        location.m_prefab.Name = name;
        location.m_prefab.Asset = asset;
        _assets[name] = asset;
        ServerOnlySelection.SetRegistered(new[] { name });
        return location;
    }

    /// <summary>A registered location whose template shapes no ground at all. Most templates.</summary>
    public ZoneSystem.ZoneLocation LocationWithoutTerrain(string name)
    {
        var asset = new GameObject(name);
        var location = new ZoneSystem.ZoneLocation { m_prefabName = name };
        location.m_prefab.Name = name;
        location.m_prefab.Asset = asset;
        _assets[name] = asset;
        ServerOnlySelection.SetRegistered(new[] { name });
        return location;
    }

    /// <summary>The same, placed in a zone the way vanilla records a placement.</summary>
    public ZoneSystem.ZoneLocation PlaceInstance(string name, Vector2s zone, float atX, float levelTo)
    {
        ZoneSystem.ZoneLocation location = LocationWithLevelModifier(name, atX, levelTo);
        ZoneSystem.instance!.m_locationInstances[zone] = new ZoneSystem.LocationInstance
        {
            m_location = location,
            m_position = ZoneSystem.GetZonePos(zone),
            m_placed = false,
        };
        return location;
    }

    public ZoneSystem.LocationInstance Instance(string name) =>
        ZoneSystem.instance!.m_locationInstances.Values.First(i => i.m_location.m_prefabName == name);

    public void Dispose()
    {
        ZoneSystem.instance = null;
        ZDOMan.instance = null;
        WorldGenerator.instance = null;
        HeightmapBuilder.instance = null;
        Heightmap.Registered = null;
        Heightmap.Loaded.Clear();
        ServerOnlyMode.Set(false);
        LocationTerrainWriter.Reset();
        LocationTerrainPatch.Forget();
        LocationSpawnGate.Forget();
        ZoneReadinessBarrier.Forget();
        TemplateAssets.Source = null;
    }
}
