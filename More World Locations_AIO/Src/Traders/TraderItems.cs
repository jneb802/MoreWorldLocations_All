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
    public static ItemDrop.ItemData blacksmithStoneItemData_tier4;

    public static void CreateCustomItems()
    {
        AssetBundle assetBundle = Prefabs.vendorsPrefabBundle;
        
        CustomItem blacksmithStoneCustomItem_tier1 = CreateBundledBlacksmithStone(assetBundle, "MWL_blacksmithStone_tier1", 1, out blacksmithStoneItemData_tier1);
        CustomItem blacksmithStoneCustomItem_tier2 = CreateBundledBlacksmithStone(assetBundle, "MWL_blacksmithStone_tier2", 2, out blacksmithStoneItemData_tier2);
        CustomItem blacksmithStoneCustomItem_tier3 = CreateBundledBlacksmithStone(assetBundle, "MWL_blacksmithStone_tier3", 3, out blacksmithStoneItemData_tier3);
        CustomItem blacksmithStoneCustomItem_tier4 = CreateBundledBlacksmithStone(assetBundle, "MWL_blacksmithStone_tier4", 4, out blacksmithStoneItemData_tier4);
        
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier1);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier2);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier3);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier4);
        
        // Skill Books
        BuildSkillBooks();
    }

    private static CustomItem CreateBundledBlacksmithStone(AssetBundle assetBundle, string prefabName, int stoneTier, out ItemDrop.ItemData itemData)
    {
        ItemConfig itemConfig = new ItemConfig();
        CustomItem customItem = new CustomItem(assetBundle, prefabName, fixReference: false, itemConfig);
        ConfigureBlacksmithStone(customItem.ItemDrop, stoneTier);
        itemData = customItem.ItemDrop.m_itemData;
        return customItem;
    }

    private static void ConfigureBlacksmithStone(ItemDrop itemDrop, int stoneTier)
    {
        itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        BlacksmithStone_SE blacksmithStoneEffect = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStoneEffect.stoneTier = stoneTier;
        itemDrop.m_itemData.m_shared.m_consumeStatusEffect = blacksmithStoneEffect;
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
