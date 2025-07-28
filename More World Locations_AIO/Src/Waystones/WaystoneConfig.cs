using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Waystones;

public class WaystoneConfig
{
    public string internalName;
    public string displayName;
    public WaystoneType waystoneType;
    public Heightmap.Biome biome;

    public string locationInternalName;
    public string locationDisplayName;
    public string vegetationInternalName;
    public string vegetationDisplayName;
    public int mapRevealRadius;
    
    public WaystoneConfig(string internalName, string displayName, string objectInternalName, string objectDisplayName, Heightmap.Biome biome, WaystoneType waystoneType)
    {
        this.internalName = internalName;
        this.displayName = displayName;
        
        this.biome = biome;
        this.waystoneType = waystoneType;

        if (waystoneType == WaystoneType.location)
        {
            this.locationInternalName = objectInternalName;
            this.locationDisplayName = objectDisplayName;
        }
        
        if (waystoneType == WaystoneType.vegetation)
        {
            this.vegetationDisplayName = objectInternalName;
            this.vegetationDisplayName = objectDisplayName;
        }
    }

    public WaystoneConfig(string internalName, string displayName, int mapRevealRadius)
    {
        this.internalName = internalName;
        this.displayName = displayName;
        this.mapRevealRadius = mapRevealRadius;
        this.waystoneType = WaystoneType.mapReveal;
        this.biome = Heightmap.Biome.All;
    }

    public enum WaystoneType
    {
        location,
        mapReveal,
        vegetation
    }
}