using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using More_World_Traders.StatusEffects;
using UnityEngine;

namespace More_World_Traders.Utils;

public class PrefabUtils
{
    public static ItemDrop.ItemData blacksmithStoneItemData_tier1;
    public static ItemDrop.ItemData blacksmithStoneItemData_tier2;
    public static ItemDrop.ItemData blacksmithStoneItemData_tier3;
    
    public static Sprite anvilSprite;
    public static Sprite tankardSprite;
    public static Sprite coinSprite;

    public static Minimap.LocationSpriteData blackForestBlacksmith1Icon;
    public static Minimap.LocationSpriteData blackForestBlacksmith2Icon;
    public static Minimap.LocationSpriteData mountainsBlacksmith1Icon;
    public static Minimap.LocationSpriteData mistlandsBlacksmith1Icon;
    public static Minimap.LocationSpriteData plainsTavern1Icon;
    public static Minimap.LocationSpriteData oceanTavern1Icon;
    public static Minimap.LocationSpriteData plainsCamp1Icon;

    public static void LoadIcons()
    {
        anvilSprite = More_World_TradersPlugin.assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Anvil.png");
        tankardSprite = More_World_TradersPlugin.assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Tankard.png");
        coinSprite = More_World_TradersPlugin.assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Coin.png");
    }

    public static void BuildLocationSpriteData()
    {
        blackForestBlacksmith1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_BlackForestBlacksmith1",
            m_icon = anvilSprite
        };

        blackForestBlacksmith2Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_BlackForestBlacksmith2",
            m_icon = anvilSprite
        };

        mountainsBlacksmith1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_MountainsBlacksmith1",
            m_icon = anvilSprite
        };

        mistlandsBlacksmith1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_MistlandsBlacksmith1",
            m_icon = anvilSprite
        };

        plainsTavern1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_PlainsTavern1",
            m_icon = tankardSprite
        };
        
        oceanTavern1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_OceanTavern1",
            m_icon = tankardSprite
        };

        plainsCamp1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_PlainsCamp1",
            m_icon = coinSprite
        };
    }
    
    public static void CreateCustomItems()
    {
        // Tier 1
        ItemConfig blacksmithStoneItemConfig_tier1 = new ItemConfig();
        CustomItem blacksmithStoneCustomItem_tier1 = new CustomItem(More_World_TradersPlugin.assetBundle, "MWL_blacksmithStone_tier1", fixReference: false, blacksmithStoneItemConfig_tier1);
        ItemDrop blacksmithStoneItemDrop_tier1 = blacksmithStoneCustomItem_tier1.ItemDrop;
        blacksmithStoneItemDrop_tier1.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        BlacksmithStone_SE blacksmithStoneEffect_tier1 = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStoneEffect_tier1.stoneTier = 1;
        blacksmithStoneItemDrop_tier1.m_itemData.m_shared.m_consumeStatusEffect = blacksmithStoneEffect_tier1;
        blacksmithStoneItemData_tier1 = blacksmithStoneCustomItem_tier1.ItemDrop.m_itemData;
        
        // Tier 2
        ItemConfig blacksmithStoneItemConfig_tier2 = new ItemConfig();
        CustomItem blacksmithStoneCustomItem_tier2 = new CustomItem(More_World_TradersPlugin.assetBundle, "MWL_blacksmithStone_tier2", fixReference: false, blacksmithStoneItemConfig_tier2);
        ItemDrop blacksmithStoneItemDrop_tier2 = blacksmithStoneCustomItem_tier2.ItemDrop;
        blacksmithStoneItemDrop_tier2.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        BlacksmithStone_SE blacksmithStoneEffect_tier2 = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStoneEffect_tier2.stoneTier = 2; // Set Tier 2
        blacksmithStoneItemDrop_tier2.m_itemData.m_shared.m_consumeStatusEffect = blacksmithStoneEffect_tier2;
        blacksmithStoneItemData_tier2 = blacksmithStoneCustomItem_tier2.ItemDrop.m_itemData;
        
        // Tier 3
        ItemConfig blacksmithStoneItemConfig_tier3 = new ItemConfig();
        CustomItem blacksmithStoneCustomItem_tier3 = new CustomItem(More_World_TradersPlugin.assetBundle, "MWL_blacksmithStone_tier3", fixReference: false, blacksmithStoneItemConfig_tier3);
        ItemDrop blacksmithStoneItemDrop_tier3 = blacksmithStoneCustomItem_tier3.ItemDrop;
        blacksmithStoneItemDrop_tier3.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
        BlacksmithStone_SE blacksmithStoneEffect_tier3 = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStoneEffect_tier3.stoneTier = 3; // Set Tier 3
        blacksmithStoneItemDrop_tier3.m_itemData.m_shared.m_consumeStatusEffect = blacksmithStoneEffect_tier3;
        blacksmithStoneItemData_tier3 = blacksmithStoneCustomItem_tier3.ItemDrop.m_itemData;
        
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier1);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier2);
        ItemManager.Instance.AddItem(blacksmithStoneCustomItem_tier3);
    }


    // public static void CreateCustomItems()
    // {
    //     GameObject shaman_heal_aoe = PrefabManager.Instance.GetPrefab("shaman_heal_aoe");
    //     GameObject modifier_shaman_heal_aoe =
    //         PrefabManager.Instance.CreateClonedPrefab("healCustomPrefab", shaman_heal_aoe);
    //     CustomPrefab healCustomPrefab = new CustomPrefab(modifier_shaman_heal_aoe, false);
    //     Aoe aoe = healCustomPrefab.Prefab.GetComponent<Aoe>();
    //     aoe.m_statusEffect = "";
    //
    //     // This is the mistle prefab. I modiify it to remove monsterAI.
    //     GameObject mistile = PrefabManager.Instance.GetPrefab("Mistile");
    //     GameObject modifier_mistile = PrefabManager.Instance.CreateClonedPrefab("mistleCustomPrefab", mistile);
    //     CustomPrefab mistleCustomPrefab = new CustomPrefab(modifier_mistile, false);
    //     Character mistileCharacter = mistleCustomPrefab.Prefab.GetComponent<Humanoid>();
    //     mistileCharacter.m_speed = 0f;
    //     mistileCharacter.m_flyFastSpeed = 0f;
    //     mistileCharacter.m_flySlowSpeed = 0f;
    //     mistileCharacter.m_name = "$modifier_mistile";
    //     mistleCustomPrefab.Prefab.GetComponent<CharacterTimedDestruction>().m_timeoutMin = 2;
    //     mistleCustomPrefab.Prefab.GetComponent<CharacterTimedDestruction>().m_timeoutMax = 4;
    //
    //     PrefabManager.Instance.AddPrefab(healCustomPrefab);
    //     PrefabManager.Instance.AddPrefab(mistleCustomPrefab);
    //     leechDeathVFX = PrefabManager.Instance.GetPrefab("vfx_leech_death");
    //     leechDeathSFX = PrefabManager.Instance.GetPrefab("sfx_leech_death");
    //
    //     PrefabManager.OnVanillaPrefabsAvailable -= CreateCustomPrefabs;
    // }
}