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
    
    /// <summary>
    /// Gets a default fallback waystone config to prevent null references
    /// </summary>
    private static WaystoneConfig GetDefaultWaystoneConfig()
    {
        return new WaystoneConfig("waystoneDefault", "Odin's Waystone", 200);
    }
    
    public static WaystoneConfig GetRandomWaystoneConfig(Heightmap.Biome biome)
    {
        // Safety check: ensure dictionary is initialized
        if (waystonesConfigs == null || waystonesConfigs.Count == 0)
        {
            Debug.LogWarning("WaystoneDB not initialized! Building configs now.");
            BuildWaystoneConfigs();
        }
        
        var biomeWaystones = waystonesConfigs.Values.Where(s => s.biome == biome).ToList();
        
        if (waystonesConfigs.TryGetValue("waystoneRadiusSmall", out var small))
            biomeWaystones.Add(small);
        if (waystonesConfigs.TryGetValue("waystoneRadiusMedium", out var medium))
            biomeWaystones.Add(medium);
        if (waystonesConfigs.TryGetValue("waystoneRadiusLarge", out var large))
            biomeWaystones.Add(large);
    
        if (biomeWaystones.Count == 0)
        {
            Debug.LogWarning($"No waystone configs found for biome {biome}, using default.");
            return GetDefaultWaystoneConfig();
        }
    
        return biomeWaystones[UnityEngine.Random.Range(0, biomeWaystones.Count)];
    }

    public static WaystoneConfig GetWaystoneConfig(string waystoneConfigName)
    {
        // Safety check: ensure dictionary is initialized
        if (waystonesConfigs == null || waystonesConfigs.Count == 0)
        {
            Debug.LogWarning("WaystoneDB not initialized! Building configs now.");
            BuildWaystoneConfigs();
        }
        
        // Handle empty or null config names
        if (string.IsNullOrEmpty(waystoneConfigName))
        {
            Debug.LogWarning("Empty waystone config name, using default.");
            return GetDefaultWaystoneConfig();
        }
        
        if (waystonesConfigs.TryGetValue(waystoneConfigName, out WaystoneConfig waystoneConfig))
        {
            return waystoneConfig;
        }
        
        Debug.LogWarning($"WaystoneConfig '{waystoneConfigName}' not found in WaystoneDB, using default.");
        return GetDefaultWaystoneConfig();
    }

    
}