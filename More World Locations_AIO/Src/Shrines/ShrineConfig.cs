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
    
    public float healthRegenMultiplier;
    public List<float> healthRegenMultiplierValues = new List<float>() { 1.05f, 1.1f, 1.2f, };
    
    public float staminaRegenMultiplier;
    public List<float> staminaRegenMultiplierValues = new List<float>() { 1.05f, 1.1f, 1.2f, };
    
    public float eitrRegenMultiplier;
    public List<float> eitrRegenMultiplierValues = new List<float>() { 1.05f, 1.1f, 1.2f, };
    
    public float raiseSkillModifier;
    public List<float> raiseSkillModifierValues = new List<float>() { 1.05f, 1.1f, 1.2f, };
    
    public ShrineConfig(string internalName, string displayName, StatusEffect statusEffect, Shrine.ShrineOffering shrineOffering, Heightmap.Biome biome)
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
    
    public void RollRandomValues()
    {
        duration = durationValues[UnityEngine.Random.Range(0, durationValues.Count)];
        
        int effectCount = GetRandomEffectAmount();
        
        List<string> availableEffects = new List<string>
        {
            "healthRegen",
            "staminaRegen", 
            "eitrRegen",
            "raiseSkill"
        };
        
        for (int i = 0; i < effectCount && availableEffects.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableEffects.Count);
            string selectedEffect = availableEffects[randomIndex];
            availableEffects.RemoveAt(randomIndex); // Remove to prevent duplicates
            
            switch (selectedEffect)
            {
                case "healthRegen":
                    healthRegenMultiplier = healthRegenMultiplierValues[UnityEngine.Random.Range(0, healthRegenMultiplierValues.Count)];
                    break;
                case "staminaRegen":
                    staminaRegenMultiplier = staminaRegenMultiplierValues[UnityEngine.Random.Range(0, staminaRegenMultiplierValues.Count)];
                    break;
                case "eitrRegen":
                    eitrRegenMultiplier = eitrRegenMultiplierValues[UnityEngine.Random.Range(0, eitrRegenMultiplierValues.Count)];
                    break;
                case "raiseSkill":
                    raiseSkillModifier = raiseSkillModifierValues[UnityEngine.Random.Range(0, raiseSkillModifierValues.Count)];
                    break;
            }
        }
    }
    
    public int GetRandomEffectAmount()
    {
        if (weightedEffectCounts.Count == 0) 
            return 0;
        
        int totalWeight = weightedEffectCounts.Values.Sum();
        
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        
        int currentWeight = 0;
        foreach (var kvp in weightedEffectCounts)
        {
            currentWeight += kvp.Value;
            if (randomValue < currentWeight)
                return kvp.Key;
        }
        
        return weightedEffectCounts.Keys.First();
    }
    
}