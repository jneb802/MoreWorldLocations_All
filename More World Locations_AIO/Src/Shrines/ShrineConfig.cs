using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Shrines;

public class ShrineConfig
{
    public string internalName;
    public string displayName;
    public StatusEffect statusEffect;
    public Shrine.ShrineOffering shrineOffering;
    public Heightmap.Biome biome;

    // A weight list of effect counts.
    // The key is the number of effects a status effect has.
    // The value is the weight of selecting that amount of effects
    public Dictionary<int, int> weightedEffectCounts = new Dictionary<int, int>
    {
        { 1, 50 },  // 1 effect with weight 50
        { 2, 30 },  // 2 effects with weight 30
        { 3, 20 }   // 3 effects with weight 20
    };
    
    public int duration;

    public List<int> durationValues = new List<int>() { 300, 600, 1200, 1800 };
    
    public ShrineConfig(string internalName, string displayName, StatusEffect statusEffect, Shrine.ShrineOffering shrineOffering)
    {
        this.internalName = internalName;
        this.displayName = displayName;
        this.statusEffect = statusEffect;
        this.shrineOffering = shrineOffering;
        this.biome = biome;
    }
    
    public Shrine.ShrineOffering GetShrineOffering()
    {
        return shrineOffering;
    }
    
}