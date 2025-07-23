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
            // Vanilla Forsaken powers (Extended versions)
            { "shrineEikthyr",   new ShrineConfig("shrineEikthyr", "Odin's Shrine of Eikthyr",   StatusEffectDB.StatusEffects["MWL_GP_Eikthyr"].StatusEffect,   new Shrine.ShrineOffering("TrophyEikthyr", 1)) },
            { "shrineElder",     new ShrineConfig("shrineElder", "Odin's Shrine of The Elder", StatusEffectDB.StatusEffects["MWL_GP_TheElder"].StatusEffect,   new Shrine.ShrineOffering("TrophyTheElder", 1)) },
            { "shrineBonemass",  new ShrineConfig("shrineBonemass", "Odin's Shrine of Bonemass",  StatusEffectDB.StatusEffects["MWL_GP_Bonemass"].StatusEffect,  new Shrine.ShrineOffering("TrophyBonemass", 1)) },
            { "shrineModer",     new ShrineConfig("shrineModer", "Odin's Shrine of Moder",     StatusEffectDB.StatusEffects["MWL_GP_Moder"].StatusEffect,     new Shrine.ShrineOffering("TrophyDragonQueen", 1)) },
            { "shrineYagluth",   new ShrineConfig("shrineYagluth", "Odin's Shrine of Yagluth",   StatusEffectDB.StatusEffects["MWL_GP_Yagluth"].StatusEffect,   new Shrine.ShrineOffering("TrophyGoblinKing", 1)) },
            { "shrineQueen",     new ShrineConfig("shrineQueen", "Odin's Shrine of The Queen", StatusEffectDB.StatusEffects["MWL_GP_Queen"].StatusEffect,     new Shrine.ShrineOffering("TrophySeekerQueen", 1)) },

            // Biome-specific Health Regen
            { "shrineHealthMeadow", new ShrineConfig("shrineHealthMeadow", "Odin's Shrine of The Boar", StatusEffectDB.StatusEffects["MWL_SE_Meadows_IncreaseHealthRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyBoar", 2)) },
            { "shrineHealthBlackForest", new ShrineConfig("shrineHealthBlackForest", "Odin's Shrine of The Greydwarf", StatusEffectDB.StatusEffects["MWL_SE_Blackforest_IncreaseHealthRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyGreydwarf", 2)) },
            { "shrineHealthSwamp", new ShrineConfig("shrineHealthSwamp", "Odin's Shrine of The Draugr", StatusEffectDB.StatusEffects["MWL_SE_Swamp_IncreaseHealthRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyDraugr", 1)) },
            { "shrineHealthMountain", new ShrineConfig("shrineHealthMountain", "Odin's Shrine of The Wolf", StatusEffectDB.StatusEffects["MWL_SE_Mountains_IncreaseHealthRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyWolf", 1)) },
            { "shrineHealthPlains", new ShrineConfig("shrineHealthPlains", "Odin's Shrine of The Goblin", StatusEffectDB.StatusEffects["MWL_SE_Plains_IncreaseHealthRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyGoblin", 1)) },
            { "shrineHealthMistlands", new ShrineConfig("shrineHealthMistlands", "Odin's Shrine of The Seeker", StatusEffectDB.StatusEffects["MWL_SE_Mistlands_IncreaseHealthRegen"].StatusEffect, new Shrine.ShrineOffering("TrophySeeker", 1)) },

            // Biome-specific Stamina Regen
            { "shrineStamMeadow", new ShrineConfig("shrineStamMeadow", "Odin's Shrine of The Deer", StatusEffectDB.StatusEffects["MWL_SE_Meadows_IncreaseStamRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyDeer", 2)) },
            { "shrineStamBlackForest", new ShrineConfig("shrineStamBlackForest", "Odin's Shrine of The Skeleton", StatusEffectDB.StatusEffects["MWL_SE_Blackforest_IncreaseStamRegen"].StatusEffect, new Shrine.ShrineOffering("TrophySkeleton", 2)) },
            { "shrineStamSwamp", new ShrineConfig("shrineStamSwamp", "Odin's Shrine of The Leech", StatusEffectDB.StatusEffects["MWL_SE_Swamp_IncreaseStamRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyLeech", 1)) },
            { "shrineStamMountain", new ShrineConfig("shrineStamMountain", "Odin's Shrine of The Hatchling", StatusEffectDB.StatusEffects["MWL_SE_Mountains_IncreaseStamRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyHatchling", 1)) },
            { "shrineStamPlains", new ShrineConfig("shrineStamPlains", "Odin's Shrine of The Deathsquito", StatusEffectDB.StatusEffects["MWL_SE_Plains_IncreaseStamRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyDeathsquito", 1)) },
            { "shrineStamMistlands", new ShrineConfig("shrineStamMistlands", "Odin's Shrine of The Hare", StatusEffectDB.StatusEffects["MWL_SE_Mistlands_IncreaseStamRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyHare", 1)) },

            // Biome-specific Eitr Regen
            { "shrineEitrMeadow", new ShrineConfig("shrineEitrMeadow", "Odin's Shrine of The Neck", StatusEffectDB.StatusEffects["MWL_SE_Meadows_IncreaseEitrRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyNeck", 2)) },
            { "shrineEitrBlackForest", new ShrineConfig("shrineEitrBlackForest", "Odin's Shrine of The Greydwarf Shaman", StatusEffectDB.StatusEffects["MWL_SE_Blackforest_IncreaseEitrRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyGreydwarfShaman", 1)) },
            { "shrineEitrSwamp", new ShrineConfig("shrineEitrSwamp", "Odin's Shrine of The Wraith", StatusEffectDB.StatusEffects["MWL_SE_Swamp_IncreaseEitrRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyWraith", 1)) },
            { "shrineEitrMountain", new ShrineConfig("shrineEitrMountain", "Odin's Shrine of The Ulv", StatusEffectDB.StatusEffects["MWL_SE_Mountains_IncreaseEitrRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyUlv", 1)) },
            { "shrineEitrPlains", new ShrineConfig("shrineEitrPlains", "Odin's Shrine of The Goblin Shaman", StatusEffectDB.StatusEffects["MWL_SE_Plains_IncreaseEitrRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyGoblinShaman", 1)) },
            { "shrineEitrMistlands", new ShrineConfig("shrineEitrMistlands", "Odin's Shrine of The Dvergr", StatusEffectDB.StatusEffects["MWL_SE_Mistlands_IncreaseEitrRegen"].StatusEffect, new Shrine.ShrineOffering("TrophyDvergr", 1)) },

            // Biome-specific Skill Gain
            { "shrineSkillBlackForest", new ShrineConfig("shrineSkillBlackForest", "Odin's Shrine of The Poison Skeleton", StatusEffectDB.StatusEffects["MWL_SE_Blackforest_IncreaseSkillGain"].StatusEffect, new Shrine.ShrineOffering("TrophySkeletonPoison", 1)) },
            { "shrineSkillSwamp", new ShrineConfig("shrineSkillSwamp", "Odin's Shrine of The Abomination", StatusEffectDB.StatusEffects["MWL_SE_Swamp_IncreaseSkillGain"].StatusEffect, new Shrine.ShrineOffering("TrophyAbomination", 1)) },
            { "shrineSkillMountain", new ShrineConfig("shrineSkillMountain", "Odin's Shrine of The Cultist", StatusEffectDB.StatusEffects["MWL_SE_Mountains_IncreaseSkillGain"].StatusEffect, new Shrine.ShrineOffering("TrophyCultist", 1)) },
            { "shrineSkillPlains", new ShrineConfig("shrineSkillPlains", "Odin's Shrine of The Goblin Brute", StatusEffectDB.StatusEffects["MWL_SE_Plains_IncreaseSkillGain"].StatusEffect, new Shrine.ShrineOffering("TrophyGoblinBrute", 1)) },
            { "shrineSkillMistlands", new ShrineConfig("shrineSkillMistlands", "Odin's Shrine of The Seeker Brute", StatusEffectDB.StatusEffects["MWL_SE_Mistlands_IncreaseSkillGain"].StatusEffect, new Shrine.ShrineOffering("TrophySeekerBrute", 1)) }
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