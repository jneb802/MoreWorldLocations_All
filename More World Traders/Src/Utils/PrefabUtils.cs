using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using More_World_Traders.StatusEffects;
using UnityEngine;

namespace More_World_Traders.Utils;

public class PrefabUtils
{
    public static ItemDrop.ItemData blacksmithStoneItemData;
    
    public static void CreateCustomItems()
    {
        ItemConfig blacksmithStoneItemConfig = new ItemConfig();
        CustomItem blacksmithStoneCustomItem = new CustomItem(More_World_TradersPlugin.assetBundle, "MWL_blacksmithStone", fixReference: false, blacksmithStoneItemConfig);
        ItemDrop blacksmithStoneItemDrop = blacksmithStoneCustomItem.ItemDrop;
        blacksmithStoneItemDrop.m_itemData.m_shared.m_consumeStatusEffect = ScriptableObject.CreateInstance<BlacksmithStone_SE>();;

        blacksmithStoneItemData = blacksmithStoneCustomItem.ItemDrop.m_itemData;
            
        PrefabManager.OnVanillaPrefabsAvailable -= CreateCustomItems;
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