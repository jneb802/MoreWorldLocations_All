using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Shrines;

public class ShrineDB
{
    public static Dictionary<string, ShrineConfig> shrinesConfigs;
    
    public static void BuildShrineConfigs()
    {
        shrinesConfigs = new Dictionary<string, ShrineConfig>
        {
            // Meadows
            { "shrineHealthMeadow", new ShrineConfig("shrineHealthMeadow", "Odin's Shrine of The Boar", StatusEffectDB.StatusEffects["MWL_SE_Boar"].StatusEffect, new Shrine.ShrineOffering("TrophyBoar", 2), Heightmap.Biome.Meadows) },
            { "shrineStamMeadow", new ShrineConfig("shrineStamMeadow", "Odin's Shrine of The Deer", StatusEffectDB.StatusEffects["MWL_SE_Deer"].StatusEffect, new Shrine.ShrineOffering("TrophyDeer", 2), Heightmap.Biome.Meadows) },
            { "shrineEitrMeadow", new ShrineConfig("shrineEitrMeadow", "Odin's Shrine of The Neck", StatusEffectDB.StatusEffects["MWL_SE_Neck"].StatusEffect, new Shrine.ShrineOffering("TrophyNeck", 2), Heightmap.Biome.Meadows) },

            // Black Forest
            { "shrineHealthBlackForest", new ShrineConfig("shrineHealthBlackForest", "Odin's Shrine of The Greydwarf", StatusEffectDB.StatusEffects["MWL_SE_Greydwarf"].StatusEffect, new Shrine.ShrineOffering("TrophyGreydwarf", 2), Heightmap.Biome.BlackForest) },
            { "shrineStamBlackForest", new ShrineConfig("shrineStamBlackForest", "Odin's Shrine of The Skeleton", StatusEffectDB.StatusEffects["MWL_SE_Skeleton"].StatusEffect, new Shrine.ShrineOffering("TrophySkeleton", 2), Heightmap.Biome.BlackForest) },
            { "shrineEitrBlackForest", new ShrineConfig("shrineEitrBlackForest", "Odin's Shrine of The Greydwarf Shaman", StatusEffectDB.StatusEffects["MWL_SE_GreydwarfShaman"].StatusEffect, new Shrine.ShrineOffering("TrophyGreydwarfShaman", 1), Heightmap.Biome.BlackForest) },
            { "shrineSkillBlackForest", new ShrineConfig("shrineSkillBlackForest", "Odin's Shrine of The Poison Skeleton", StatusEffectDB.StatusEffects["MWL_SE_SkeletonPoison"].StatusEffect, new Shrine.ShrineOffering("TrophySkeletonPoison", 1), Heightmap.Biome.BlackForest) },

            // Swamp
            { "shrineHealthSwamp", new ShrineConfig("shrineHealthSwamp", "Odin's Shrine of The Draugr", StatusEffectDB.StatusEffects["MWL_SE_Draugr"].StatusEffect, new Shrine.ShrineOffering("TrophyDraugr", 1), Heightmap.Biome.Swamp) },
            { "shrineStamSwamp", new ShrineConfig("shrineStamSwamp", "Odin's Shrine of The Leech", StatusEffectDB.StatusEffects["MWL_SE_Leech"].StatusEffect, new Shrine.ShrineOffering("TrophyLeech", 1), Heightmap.Biome.Swamp) },
            { "shrineEitrSwamp", new ShrineConfig("shrineEitrSwamp", "Odin's Shrine of The Wraith", StatusEffectDB.StatusEffects["MWL_SE_Wraith"].StatusEffect, new Shrine.ShrineOffering("TrophyWraith", 1), Heightmap.Biome.Swamp) },
            { "shrineSkillSwamp", new ShrineConfig("shrineSkillSwamp", "Odin's Shrine of The Abomination", StatusEffectDB.StatusEffects["MWL_SE_Abomination"].StatusEffect, new Shrine.ShrineOffering("TrophyAbomination", 1), Heightmap.Biome.Swamp) },

            // Mountains
            { "shrineHealthMountain", new ShrineConfig("shrineHealthMountain", "Odin's Shrine of The Wolf", StatusEffectDB.StatusEffects["MWL_SE_Wolf"].StatusEffect, new Shrine.ShrineOffering("TrophyWolf", 1), Heightmap.Biome.Mountain) },
            { "shrineStamMountain", new ShrineConfig("shrineStamMountain", "Odin's Shrine of The Hatchling", StatusEffectDB.StatusEffects["MWL_SE_Hatchling"].StatusEffect, new Shrine.ShrineOffering("TrophyHatchling", 1), Heightmap.Biome.Mountain) },
            { "shrineEitrMountain", new ShrineConfig("shrineEitrMountain", "Odin's Shrine of The Ulv", StatusEffectDB.StatusEffects["MWL_SE_Ulv"].StatusEffect, new Shrine.ShrineOffering("TrophyUlv", 1), Heightmap.Biome.Mountain) },
            { "shrineSkillMountain", new ShrineConfig("shrineSkillMountain", "Odin's Shrine of The Cultist", StatusEffectDB.StatusEffects["MWL_SE_Cultist"].StatusEffect, new Shrine.ShrineOffering("TrophyCultist", 1), Heightmap.Biome.Mountain) },

            // Plains
            { "shrineHealthPlains", new ShrineConfig("shrineHealthPlains", "Odin's Shrine of The Goblin", StatusEffectDB.StatusEffects["MWL_SE_Goblin"].StatusEffect, new Shrine.ShrineOffering("TrophyGoblin", 1), Heightmap.Biome.Plains) },
            { "shrineStamPlains", new ShrineConfig("shrineStamPlains", "Odin's Shrine of The Deathsquito", StatusEffectDB.StatusEffects["MWL_SE_Deathsquito"].StatusEffect, new Shrine.ShrineOffering("TrophyDeathsquito", 1), Heightmap.Biome.Plains) },
            { "shrineEitrPlains", new ShrineConfig("shrineEitrPlains", "Odin's Shrine of The Goblin Shaman", StatusEffectDB.StatusEffects["MWL_SE_GoblinShaman"].StatusEffect, new Shrine.ShrineOffering("TrophyGoblinShaman", 1), Heightmap.Biome.Plains) },
            { "shrineSkillPlains", new ShrineConfig("shrineSkillPlains", "Odin's Shrine of The Goblin Brute", StatusEffectDB.StatusEffects["MWL_SE_GoblinBrute"].StatusEffect, new Shrine.ShrineOffering("TrophyGoblinBrute", 1), Heightmap.Biome.Plains) },

            // Mistlands
            { "shrineHealthMistlands", new ShrineConfig("shrineHealthMistlands", "Odin's Shrine of The Seeker", StatusEffectDB.StatusEffects["MWL_SE_Seeker"].StatusEffect, new Shrine.ShrineOffering("TrophySeeker", 1), Heightmap.Biome.Mistlands) },
            { "shrineStamMistlands", new ShrineConfig("shrineStamMistlands", "Odin's Shrine of The Hare", StatusEffectDB.StatusEffects["MWL_SE_Hare"].StatusEffect, new Shrine.ShrineOffering("TrophyHare", 1), Heightmap.Biome.Mistlands) },
            { "shrineEitrMistlands", new ShrineConfig("shrineEitrMistlands", "Odin's Shrine of The Dvergr", StatusEffectDB.StatusEffects["MWL_SE_Dvergr"].StatusEffect, new Shrine.ShrineOffering("TrophyDvergr", 1), Heightmap.Biome.Mistlands) },
            { "shrineSkillMistlands", new ShrineConfig("shrineSkillMistlands", "Odin's Shrine of The Seeker Brute", StatusEffectDB.StatusEffects["MWL_SE_SeekerBrute"].StatusEffect, new Shrine.ShrineOffering("TrophySeekerBrute", 1), Heightmap.Biome.Mistlands) }
        };
        
        ItemManager.OnItemsRegistered -= ShrineDB.BuildShrineConfigs;
    }

    public static ShrineConfig GetRandomShrineConfig()
    {
        if (shrinesConfigs == null || shrinesConfigs.Count == 0)
        {
            Debug.Log("Failed to find shrine configs");
            return null;  
        }
        
        int randomIndex = UnityEngine.Random.Range(0, shrinesConfigs.Count);
        return shrinesConfigs.ElementAt(randomIndex).Value;
    }
    
    public static ShrineConfig GetRandomShrineConfig(Heightmap.Biome biome)
    {
        var biomeShrines = shrinesConfigs.Values.Where(s => s.biome == biome).ToList();
    
        if (biomeShrines.Count == 0) return null;
    
        return biomeShrines[UnityEngine.Random.Range(0, biomeShrines.Count)];
    }

    public static ShrineConfig GetShrineConfig(string shrineConfigName)
    {
        if (shrinesConfigs.TryGetValue(shrineConfigName, out ShrineConfig shrineConfig))
        {
            return shrineConfig;
        }
        
        Debug.LogWarning($"ShrineConfig '{shrineConfigName}' not found in ShrineDB");
        return null;
    }

    
}