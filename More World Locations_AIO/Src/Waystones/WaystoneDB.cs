using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Waystones;

public class WaystoneDB
{
    public static Dictionary<string, WaystoneConfig> waystonesConfigs;
    
    public static void BuildWaystoneConfigs()
    {
        waystonesConfigs = new Dictionary<string, WaystoneConfig>
        {

            // Mountain Silver
            // { "waystoneMountainSilver", new WaystoneConfig("waystoneMountainSilver", "Odin's Waystone of Silver", "silvervein", "Silver vein", Heightmap.Biome.Mountain, WaystoneConfig.WaystoneType.vegetation) },

            // Swamp crypt
            { "waystoneSwampCrypt", new WaystoneConfig("waystoneSwampCrypt", "Odin's Waystone of The Dragur", "SunkenCrypt4","Sunken Crypt", Heightmap.Biome.Swamp, WaystoneConfig.WaystoneType.location) },

            // Moutain Frostcave
            { "waystoneMountainFrostcave", new WaystoneConfig("waystoneMountainFrostcave", "Odin's Waystone of The Ulv", "MountainCave02", "Frost Cave", Heightmap.Biome.Mountain, WaystoneConfig.WaystoneType.location) },

            // Mistlands Infested Mines
            { "waystoneMistlandsInfestedMine1", new WaystoneConfig("waystoneMistlandsInfestedMine1", "Odin's Waystone of The Seeker", "Mistlands_DvergrTownEntrance1","Infested Mine", Heightmap.Biome.Mistlands, WaystoneConfig.WaystoneType.location) },
            { "waystoneMistlandsInfestedMine2", new WaystoneConfig("waystoneMistlandsInfestedMine2", "Odin's Waystone of The Seeker", "Mistlands_DvergrTownEntrance2", "Infested Mine",Heightmap.Biome.Mistlands, WaystoneConfig.WaystoneType.location) },

            // Reval radius waystones
            { "waystoneRadiusSmall", new WaystoneConfig("waystoneRadiusSmall", "Odin's Waystone of Far Sight", 200) },
            { "waystoneRadiusMedium", new WaystoneConfig("waystoneRadiusMedium", "Odin's Waystone of Far Sight", 400) },
            { "waystoneRadiusLarge", new WaystoneConfig("waystoneRadiusLarge", "Odin's Waystone of Far Sight", 800) },

        };
        
        ItemManager.OnItemsRegistered -= WaystoneDB.BuildWaystoneConfigs;
    }
    
    public static WaystoneConfig GetRandomWaystoneConfig(Heightmap.Biome biome)
    {
        var biomeWaystones = waystonesConfigs.Values.Where(s => s.biome == biome).ToList();
        
        if (waystonesConfigs.TryGetValue("waystoneRadiusSmall", out var small))
            biomeWaystones.Add(small);
        if (waystonesConfigs.TryGetValue("waystoneRadiusMedium", out var medium))
            biomeWaystones.Add(medium);
        if (waystonesConfigs.TryGetValue("waystoneRadiusLarge", out var large))
            biomeWaystones.Add(large);
    
        if (biomeWaystones.Count == 0) return null;
    
        return biomeWaystones[UnityEngine.Random.Range(0, biomeWaystones.Count)];
    }

    public static WaystoneConfig GetWaystoneConfig(string waystoneConfigName)
    {
        if (waystonesConfigs.TryGetValue(waystoneConfigName, out WaystoneConfig waystoneConfig))
        {
            return waystoneConfig;
        }
        
        Debug.LogWarning($"WaystoneConfig '{waystoneConfigName}' not found in WaystoneDB");
        return null;
    }

    
}