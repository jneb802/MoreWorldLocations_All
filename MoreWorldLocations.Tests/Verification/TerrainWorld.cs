using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
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
    }
}
