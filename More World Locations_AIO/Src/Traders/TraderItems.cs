using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Traders;

public class TraderItems
{
    private const string BlacksmithStoneBasePrefab = "Ruby";
    private const string SkillBookBasePrefab = "CryptKey";

    public static ItemDrop.ItemData blacksmithStoneItemData_tier1;
    public static ItemDrop.ItemData blacksmithStoneItemData_tier2;
    public static ItemDrop.ItemData blacksmithStoneItemData_tier3;

    public static void CreateCustomItems()
    {
        CustomItem blacksmithStoneCustomItem_tier1 = CreateBlacksmithStone("MWL_blacksmithStone_tier1", 1);
        CustomItem blacksmithStoneCustomItem_tier2 = CreateBlacksmithStone("MWL_blacksmithStone_tier2", 2);
        CustomItem blacksmithStoneCustomItem_tier3 = CreateBlacksmithStone("MWL_blacksmithStone_tier3", 3);

        blacksmithStoneItemData_tier1 = blacksmithStoneCustomItem_tier1.ItemDrop.m_itemData;
        blacksmithStoneItemData_tier2 = blacksmithStoneCustomItem_tier2.ItemDrop.m_itemData;
        blacksmithStoneItemData_tier3 = blacksmithStoneCustomItem_tier3.ItemDrop.m_itemData;
        
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier1);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier2);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier3);
        
        // Skill Books
        BuildSkillBooks();
    }

    private static CustomItem CreateBlacksmithStone(string prefabName, int tier)
    {
        ItemConfig itemConfig = new ItemConfig
        {
            Name = $"$item_mwl_blacksmithstone_tier{tier}",
            Description = "$item_mwl_blacksmithstone_description",
            StackSize = 10,
        };

        CustomItem customItem = new CustomItem(prefabName, BlacksmithStoneBasePrefab, itemConfig);
        ItemDrop itemDrop = customItem.ItemDrop;
        itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;

        BlacksmithStone_SE blacksmithStoneEffect = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStoneEffect.stoneTier = tier;
        itemDrop.m_itemData.m_shared.m_consumeStatusEffect = blacksmithStoneEffect;

        return customItem;
    }
    
    public static void BuildSkillBooks()
    {
        foreach (Skills.SkillType skill in Enum.GetValues(typeof(Skills.SkillType)))
        {
            if (skill == Skills.SkillType.None || skill == Skills.SkillType.All) continue;

            for (int tier = 1; tier <= 3; tier++)
            {
                CreateSkillBook(skill, tier);
            }
        }
    }
    
    public static void CreateSkillBook(Skills.SkillType skill, int tier)
    {
        ItemConfig bookConfig = new ItemConfig();
        string customItemName = "MWL_skillBook_" + skill.ToString() + "_bookTier" + tier;
        CustomItem customItem = new CustomItem(customItemName, SkillBookBasePrefab, bookConfig);
        ItemDrop itemDrop = customItem.ItemDrop;
        itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        itemDrop.m_itemData.m_shared.m_maxStackSize = 10;
        itemDrop.m_itemData.m_shared.m_name = "$skill_" + skill.ToString().ToLower() + " $mwl_skillbook_tier" + tier;
        itemDrop.m_itemData.m_shared.m_description = "$mwl_skillbook_desc_tier" + tier;
        SkillBook_SE skillBook_SE = ScriptableObject.CreateInstance<SkillBook_SE>();
        skillBook_SE.name = "MWL_SkillBook_SE_" + skill + "_tier" + tier;
        skillBook_SE.m_name = "$se_skillBook";
        skillBook_SE.skillType = skill;
        skillBook_SE.bookTier = tier;
        itemDrop.m_itemData.m_shared.m_consumeStatusEffect = skillBook_SE;
        
        ItemManager.Instance.AddItem(customItem);
    }
}
