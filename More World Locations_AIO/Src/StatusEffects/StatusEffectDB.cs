using System;
using System.Collections.Generic;
using System.ComponentModel;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Shrines;

public class StatusEffectDB
{
    public static Dictionary<string, CustomStatusEffect> StatusEffects = new Dictionary<string, CustomStatusEffect>();
    
    public static void BuildStatusEffects()
    {
        EffectList startEffects =  ObjectDB.instance.GetStatusEffect("GP_Eikthyr".GetStableHashCode()).m_startEffects;
        
        // CreateClonedStatusEffect("GP_Eikthyr", "MWL_GP_Eikthyr", 1800f);
        // CreateClonedStatusEffect("GP_TheElder", "MWL_GP_TheElder", 1800f);
        // CreateClonedStatusEffect("GP_Bonemass", "MWL_GP_Bonemass", 1800f);
        // CreateClonedStatusEffect("GP_Moder", "MWL_GP_Moder", 1800f);
        // CreateClonedStatusEffect("GP_Yagluth", "MWL_GP_Yagluth", 1800f);
        // CreateClonedStatusEffect("GP_Queen", "MWL_GP_Queen", 1800f);
        // CreateClonedStatusEffect("GP_Fader", "MWL_GP_Fader", 1800f);
        
        var increaseHealthRegen = new (string internalName, string displayName, string trophyName)[]
        {
            ("MWL_SE_Boar", "Blessing of the Boar", "TrophyBoar"),
            ("MWL_SE_Greydwarf", "Blessing of the Greydwarf", "TrophyGreydwarf"),
            ("MWL_SE_Draugr", "Blessing of the Draugr", "TrophyDraugr"),
            ("MWL_SE_Wolf", "Blessing of the Wolf", "TrophyWolf"),
            ("MWL_SE_Goblin", "Blessing of the Fuling", "TrophyGoblin"),
            ("MWL_SE_Seeker", "Blessing of the Seeker", "TrophySeeker"),
            // ("MWL_SE_Ashlands_IncreaseHealthRegen", "Blessing of the Charred", "TrophyCharred"),
        };

        foreach (var (internalName, displayName, trophyName) in increaseHealthRegen)
        {
            CreateCustomStatusEffect(internalName, displayName, trophyName, se =>
            {
                // se.m_healthRegenMultiplier = 1.1f;
                // se.m_ttl = 1800f;
                se.m_startEffects = startEffects;
            });
        }
        
        var increaseStamRegen = new (string internalName, string displayName, string trophyName)[]
        {
            ("MWL_SE_Deer", "Blessing of the Deer", "TrophyDeer"),
            ("MWL_SE_Skeleton", "Blessing of the Skeleton", "TrophySkeleton"),
            ("MWL_SE_Leech", "Blessing of the Leech", "TrophyLeech"),
            ("MWL_SE_Hatchling", "Blessing of the Hatchling", "TrophyHatchling"),
            ("MWL_SE_Deathsquito", "Blessing of the Deathsquito", "TrophyDeathSquito"),
            ("MWL_SE_Hare", "Blessing of the Hare", "TrophyHare"),
            // ("MWL_SE_Ashlands_IncreaseStamRegen", "Blessing of the Lox", "TrophyLox"),
        };

        foreach (var (internalName, displayName, trophyName) in increaseStamRegen)
        {
            CreateCustomStatusEffect(internalName, displayName, trophyName, se =>
            {
                se.m_staminaRegenMultiplier = 1.1f;
                se.m_ttl = 1800f;
                se.m_startEffects = startEffects;
            });
        }
        
        var increaseEitrRegen = new (string internalName, string displayName, string trophyName)[]
        {
            ("MWL_SE_Neck", "Blessing of the Neck", "TrophyNeck"),
            ("MWL_SE_GreydwarfShaman", "Blessing of the Greydwarf Shaman", "TrophyGreydwarfShaman"),
            ("MWL_SE_Wraith", "Blessing of the Wraith", "TrophyWraith"),
            ("MWL_SE_Ulv", "Blessing of the Ulv", "TrophyUlv"),
            ("MWL_SE_GoblinShaman", "Blessing of the Fuling Shaman", "TrophyGoblinShaman"),
            ("MWL_SE_Dvergr", "Blessing of the Dvergr", "TrophyDvergr"),
            // ("MWL_SE_Ashlands_IncreaseEitrRegen", "Blessing of the Lox", "TrophyLox"),
        };

        foreach (var (internalName, displayName, trophyName) in increaseEitrRegen)
        {
            CreateCustomStatusEffect(internalName, displayName, trophyName, se =>
            {
                se.m_eitrRegenMultiplier = 1.1f;
                se.m_ttl = 1800f;
                se.m_startEffects = startEffects;
            });
        }
        
        var increaseSkillGain = new (string internalName, string displayName, string trophyName)[]
        {
            // ("MWL_SE_Meadows_IncreaseSkillGain", "Blessing of the Neck", "TrophyNeck"),
            ("MWL_SE_SkeletonPoison", "Blessing of the Poison Skeleton", "TrophySkeletonPoison"),
            ("MWL_SE_Abomination", "Blessing of the Abomination", "TrophyAbomination"),
            ("MWL_SE_Cultist", "Blessing of the Cultist", "TrophyCultist"),
            ("MWL_SE_GoblinBrute", "Blessing of the Fuling Berserker", "TrophyGoblinBrute"),
            ("MWL_SE_SeekerBrute", "Blessing of the Seeker Soldier", "TrophySeekerBrute"),
            // ("MWL_SE_Ashlands_IncreaseSkillGain", "Blessing of the Lox", "TrophyLox"),
        };

        foreach (var (internalName, displayName, trophyName) in increaseSkillGain)
        {
            CreateCustomStatusEffect(internalName, displayName, trophyName, se =>
            {
                se.m_raiseSkill = Skills.SkillType.All;
                se.m_raiseSkillModifier = 1.1f;
                se.m_ttl = 1800f;
                se.m_startEffects = startEffects;
            });
        }
        
        ItemManager.OnItemsRegistered -= BuildStatusEffects;
    }

    public static void CreateClonedStatusEffect(string statusEffectName, string newStatusEffectName, float duration)
    {
        StatusEffect original = ObjectDB.instance.GetStatusEffect(statusEffectName.GetStableHashCode());
        if (original == null)
        {
            Debug.LogError("Could not find: " + statusEffectName);
            return;
        }

        StatusEffect clone = UnityEngine.Object.Instantiate(original);
        clone.name = newStatusEffectName;
        clone.m_ttl = duration;
        
        CustomStatusEffect customSE = new CustomStatusEffect(clone, fixReference: false);
        ItemManager.Instance.AddStatusEffect(customSE);
        
        if (!StatusEffects.ContainsKey(newStatusEffectName))
        {
            StatusEffects.Add(newStatusEffectName, customSE);
            Debug.Log("Adding custom status effect with name " + newStatusEffectName);
        }
    }
    
    private static void CreateCustomStatusEffect(string internalName, string displayName, string spriteName, Action<SE_Stats> configure)
    {
        if (ObjectDB.instance.m_StatusEffects.Exists(se => se.name == internalName)) return;

        SE_Stats se = ScriptableObject.CreateInstance<SE_Stats>();
        se.name = internalName;
        se.m_name = displayName;
        se.m_icon = GUIManager.Instance.GetSprite(spriteName);

        configure(se);

        CustomStatusEffect customSE = new CustomStatusEffect(se, fixReference: false);
        ItemManager.Instance.AddStatusEffect(customSE);
        
        if (!StatusEffects.ContainsKey(internalName))
        {
            StatusEffects.Add(internalName, customSE);
            Debug.Log("Adding custom status effect with name " + internalName);
        }
    }
}