using System;
using System.Collections.Generic;
using Jotunn.Entities;

namespace More_World_Traders.Utils;

public class TranslationUtils
{
    public static CustomLocalization Localization;

    public static void AddLocalizations()
    {
        Localization = Jotunn.Managers.LocalizationManager.Instance.GetLocalization();
    
        Localization.AddTranslation("English", new Dictionary<string, string>
        {
            {"$se_blacksmithStone", "Blacksmith Stone"},
            {"$se_skillBook", "Skill Book"},
            {"$item_mwl_blacksmithstone_tier1", "Blacksmith Stone (1)"},
            {"$item_mwl_blacksmithstone_tier2", "Blacksmith Stone (2)"},
            {"$item_mwl_blacksmithstone_tier3", "Blacksmith Stone (3)"},
            {"$item_mwl_blacksmithstone_description", "Upgrades an armor or weapon past max quality. Place an armor or weapon in top left cell in inventory and consume this item."},
        });
    }

    public static void AddSkillBookLocalization(Skills.SkillType skill, int tier)
    {
        Localization = Jotunn.Managers.LocalizationManager.Instance.GetLocalization();
    
        Localization.AddTranslation("English", new Dictionary<string, string>
        {
            {"$item_mwl_skillBook_" + skill.ToString() + "_bookTier" + tier, skill.ToString() + " Skill Book " + "(" + tier + ")"},
            {"$item_mwl_skillBook_description_" + skill.ToString() + "_bookTier" + tier, "A skill book to raise " + skill + " skill"},
        });
    }
    
    public static void BuildSkillBookLocalizations()
    {
        foreach (Skills.SkillType skill in Enum.GetValues(typeof(Skills.SkillType)))
        {
            for (int tier = 1; tier <= 3; tier++)
            {
                AddSkillBookLocalization(skill, tier);
            }
        }
    }

}