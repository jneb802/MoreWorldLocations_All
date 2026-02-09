using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Sealstone;

public class SealstoneItems
{
    public static void CreateItems()
    {
        for (int tier = 1; tier <= 3; tier++)
        {
            CreateSealKey(tier);
            CreateSealKeyPickable(tier);
        }

        AddLocalizations();
    }

    private static void CreateSealKey(int tier)
    {
        string keyName = "MWL_SealKey_Tier" + tier;
        GameObject keyPrefab = PrefabManager.Instance.CreateClonedPrefab(keyName, "CryptKey");
        if (keyPrefab == null)
        {
            Debug.LogWarning($"SealstoneItems: Could not clone CryptKey for {keyName}");
            return;
        }

        ItemDrop itemDrop = keyPrefab.GetComponent<ItemDrop>();
        itemDrop.m_itemData.m_shared.m_name = "$item_mwl_sealkey_tier" + tier;
        itemDrop.m_itemData.m_shared.m_description = "$item_mwl_sealkey_tier" + tier + "_desc";
        itemDrop.m_itemData.m_shared.m_maxStackSize = 5;
        itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Material;

        CustomItem customItem = new CustomItem(keyPrefab, false);
        ItemManager.Instance.AddItem(customItem);
    }

    private static void CreateSealKeyPickable(int tier)
    {
        string pickableName = "MWL_SealKey_Tier" + tier + "_Pickable";
        GameObject pickablePrefab = PrefabManager.Instance.CreateClonedPrefab(pickableName, "Pickable_Item");
        if (pickablePrefab == null)
        {
            Debug.LogWarning($"SealstoneItems: Could not clone Pickable_Item for {pickableName}");
            return;
        }

        PickableItem pickable = pickablePrefab.GetComponent<PickableItem>();
        pickable.m_randomItemPrefabs = System.Array.Empty<PickableItem.RandomItem>();

        // The m_itemPrefab will be set after item registration resolves the prefab.
        // We need to defer this to after all items are registered.
        string keyName = "MWL_SealKey_Tier" + tier;
        PrefabManager.OnPrefabsRegistered += () =>
        {
            GameObject keyPrefab = PrefabManager.Instance.GetPrefab(keyName);
            if (keyPrefab != null)
            {
                pickable.m_itemPrefab = keyPrefab.GetComponent<ItemDrop>();
                pickable.m_stack = 1;
            }
        };

        CustomPrefab customPrefab = new CustomPrefab(pickablePrefab, false);
        PrefabManager.Instance.AddPrefab(customPrefab);
    }

    private static void AddLocalizations()
    {
        var localization = LocalizationManager.Instance.GetLocalization();

        localization.AddTranslation("English", new Dictionary<string, string>
        {
            { "$item_mwl_sealkey_tier1", "Seal Key - Tier 1" },
            { "$item_mwl_sealkey_tier1_desc", "A runic key that emanates faint energy. It can unseal Tier 1 gates." },
            { "$item_mwl_sealkey_tier2", "Seal Key - Tier 2" },
            { "$item_mwl_sealkey_tier2_desc", "A runic key pulsing with moderate power. It can unseal Tier 2 gates." },
            { "$item_mwl_sealkey_tier3", "Seal Key - Tier 3" },
            { "$item_mwl_sealkey_tier3_desc", "A runic key surging with ancient energy. It can unseal Tier 3 gates." },
        });
    }
}
