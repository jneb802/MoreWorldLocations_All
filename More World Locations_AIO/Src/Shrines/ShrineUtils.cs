using System.Text;
using UnityEngine;

namespace More_World_Locations_AIO.Shrines;

public static class ShrineUtils
{
    public static string GetEffectDescription(SE_Stats effect)
    {
        if (effect == null) return "";

        var sb = new StringBuilder();

        void AddStat(string label, float value, string suffix = "")
        {
            if (Mathf.Abs(value) > 0.01f)
            {
                string sign = value > 0 ? "+" : "";
                sb.AppendLine($"{label}: {sign}{value:0.#}{suffix}");
            }
        }

        void AddPercent(string label, float value)
        {
            if (Mathf.Abs(value) > 0.01f)
            {
                string sign = value > 0 ? "+" : "";
                sb.AppendLine($"{label}: {sign}{value * 100f:0.#}%");
            }
        }

        void AddMultiplierPercent(string label, float multiplier)
        {
            if (Mathf.Abs(multiplier - 1f) > 0.01f)
            {
                float percentChange = (multiplier - 1f) * 100f;
                string sign = percentChange > 0 ? "+" : "";
                sb.AppendLine($"{label}: {sign}{percentChange:0.#}%");
            }
        }

        AddStat("HP per tick", effect.m_healthPerTick);
        AddStat("Health over time", effect.m_healthOverTime);
        AddStat("Stamina over time", effect.m_staminaOverTime);
        AddStat("Eitr over time", effect.m_eitrOverTime);
        AddMultiplierPercent("Health regen", effect.m_healthRegenMultiplier);
        AddMultiplierPercent("Stamina regen", effect.m_staminaRegenMultiplier);
        AddMultiplierPercent("Eitr regen", effect.m_eitrRegenMultiplier);
        AddStat("Carry weight", effect.m_addMaxCarryWeight, " kg");
        AddPercent("Speed", effect.m_speedModifier);
        AddPercent("Stealth", effect.m_stealthModifier);
        AddPercent("Noise", effect.m_noiseModifier);
        AddPercent("Fall damage", effect.m_fallDamageModifier);
        AddStat("Max fall speed", effect.m_maxMaxFallSpeed, " m/s");

        AddPercent("Run stamina drain", effect.m_runStaminaDrainModifier);
        AddPercent("Jump stamina use", effect.m_jumpStaminaUseModifier);
        AddPercent("Attack stamina use", effect.m_attackStaminaUseModifier);
        AddPercent("Block stamina use", effect.m_blockStaminaUseModifier);
        AddPercent("Dodge stamina use", effect.m_dodgeStaminaUseModifier);
        AddPercent("Swim stamina use", effect.m_swimStaminaUseModifier);
        AddPercent("Sneak stamina use", effect.m_sneakStaminaUseModifier);
        AddPercent("Run stamina use", effect.m_runStaminaUseModifier);

        if (effect.m_skillLevel != Skills.SkillType.None)
            sb.AppendLine($"Skill: {effect.m_skillLevel} +{effect.m_skillLevelModifier:0.#}");

        if (effect.m_skillLevel2 != Skills.SkillType.None)
            sb.AppendLine($"Skill: {effect.m_skillLevel2} +{effect.m_skillLevelModifier2:0.#}");

        if (effect.m_mods != null && effect.m_mods.Count > 0)
            sb.AppendLine("Damage mods: " + SE_Stats.GetDamageModifiersTooltipString(effect.m_mods).Replace("\n", " "));

        return sb.ToString().TrimEnd();
    }
}