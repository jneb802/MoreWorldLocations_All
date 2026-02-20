using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Traders;

public class TraderItems
{
    public static ItemDrop.ItemData blacksmithStoneItemData_tier1;
    public static ItemDrop.ItemData blacksmithStoneItemData_tier2;
    public static ItemDrop.ItemData blacksmithStoneItemData_tier3;

    public static void CreateCustomItems()
    {
        var assetBundle = Prefabs.vendorsPrefabBundle;
        
        // Tier 1
        ItemConfig blacksmithStoneItemConfig_tier1 = new ItemConfig();
        CustomItem blacksmithStoneCustomItem_tier1 = new CustomItem(assetBundle, "MWL_blacksmithStone_tier1", fixReference: false, blacksmithStoneItemConfig_tier1);
        ItemDrop blacksmithStoneItemDrop_tier1 = blacksmithStoneCustomItem_tier1.ItemDrop;
        blacksmithStoneItemDrop_tier1.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        BlacksmithStone_SE blacksmithStoneEffect_tier1 = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStoneEffect_tier1.stoneTier = 1;
        blacksmithStoneItemDrop_tier1.m_itemData.m_shared.m_consumeStatusEffect = blacksmithStoneEffect_tier1;
        blacksmithStoneItemData_tier1 = blacksmithStoneCustomItem_tier1.ItemDrop.m_itemData;
        
        // Tier 2
        ItemConfig blacksmithStoneItemConfig_tier2 = new ItemConfig();
        CustomItem blacksmithStoneCustomItem_tier2 = new CustomItem(assetBundle, "MWL_blacksmithStone_tier2", fixReference: false, blacksmithStoneItemConfig_tier2);
        ItemDrop blacksmithStoneItemDrop_tier2 = blacksmithStoneCustomItem_tier2.ItemDrop;
        blacksmithStoneItemDrop_tier2.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        BlacksmithStone_SE blacksmithStoneEffect_tier2 = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStoneEffect_tier2.stoneTier = 2;
        blacksmithStoneItemDrop_tier2.m_itemData.m_shared.m_consumeStatusEffect = blacksmithStoneEffect_tier2;
        blacksmithStoneItemData_tier2 = blacksmithStoneCustomItem_tier2.ItemDrop.m_itemData;
        
        // Tier 3
        ItemConfig blacksmithStoneItemConfig_tier3 = new ItemConfig();
        CustomItem blacksmithStoneCustomItem_tier3 = new CustomItem(assetBundle, "MWL_blacksmithStone_tier3", fixReference: false, blacksmithStoneItemConfig_tier3);
        ItemDrop blacksmithStoneItemDrop_tier3 = blacksmithStoneCustomItem_tier3.ItemDrop;
        blacksmithStoneItemDrop_tier3.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        BlacksmithStone_SE blacksmithStoneEffect_tier3 = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStoneEffect_tier3.stoneTier = 3;
        blacksmithStoneItemDrop_tier3.m_itemData.m_shared.m_consumeStatusEffect = blacksmithStoneEffect_tier3;
        blacksmithStoneItemData_tier3 = blacksmithStoneCustomItem_tier3.ItemDrop.m_itemData;
        
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier1);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier2);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier3);
        
        // Skill Books
        BuildSkillBooks();
    }
    
    public static void BuildSkillBooks()
    {
        var assetBundle = Prefabs.vendorsPrefabBundle;
        
        CustomPrefab skillBookPrefab = new CustomPrefab(assetBundle, "MWL_skillTome", fixReference: false);
        PrefabManager.Instance.AddPrefab(skillBookPrefab);
        
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
        CustomItem customItem = new CustomItem(customItemName, "MWL_skillTome", bookConfig);
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
