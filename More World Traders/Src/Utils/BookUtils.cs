using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using More_World_Traders.StatusEffects;
using UnityEngine;

namespace More_World_Traders.Utils;

public class BookUtils
{
    public static void BuildSkillBooks()
    {
        CustomPrefab skillBookPrefab = new CustomPrefab(More_World_TradersPlugin.assetBundle,
            "MWL_skillTome", fixReference: false);
        PrefabManager.Instance.AddPrefab(skillBookPrefab);
        
        foreach (Skills.SkillType skill in Enum.GetValues(typeof(Skills.SkillType)))
        {
            for (int tier = 1; tier <= 3; tier++)
            {
                Debug.Log("built skill for skill " + skill.ToString() + " tier: " + tier);
                CreateSkillBook(skill, tier);
            }
        }
    }
    
    public static void CreateSkillBook(Skills.SkillType skill, int tier)
    {
        ItemConfig bookConfig = new ItemConfig();
        string customItemName = "MWL_skillBook_" + skill.ToString() + "_bookTier" + tier;
        CustomItem customItem = new CustomItem(customItemName, "MWL_skillTome", bookConfig);
        ItemDrop itemDrop = customItem.ItemDrop;
        itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        itemDrop.m_itemData.m_shared.m_name = "$item_mwl_skillBook_" + skill.ToString() + "_bookTier" + tier;
        itemDrop.m_itemData.m_shared.m_description = "$item_mwl_skillBook_description_" + skill.ToString() + "_bookTier" + tier;
        SkillBook_SE skillBook_SE = ScriptableObject.CreateInstance<SkillBook_SE>();
        skillBook_SE.skillType = skill;
        skillBook_SE.bookTier = tier;
        itemDrop.m_itemData.m_shared.m_consumeStatusEffect = skillBook_SE;
        
        ItemManager.Instance.AddItem(customItem);
    }
    
}