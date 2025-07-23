using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Shrines;

public class Shrine : MonoBehaviour, Interactable, Hoverable
{
    public ShrineConfig shrineConfig;
    
    public bool hasBeenUsedOnce = false;

    public ZNetView znetView;

    public Heightmap.Biome biome = Heightmap.Biome.Meadows;
    
    public const string ShrineConfigKey = "MWL_ShrineConfigName";
    
    public void Awake()
    {
        znetView = GetComponent<ZNetView>();
        ZDO zdo = znetView.GetZDO();
        if (znetView.IsOwner())
        {
            if (!zdo.GetString(ShrineConfigKey, out string storedName) || string.IsNullOrEmpty(storedName))
            {
                shrineConfig = ShrineDB.GetRandomShrineConfig();
                zdo.Set(ShrineConfigKey, shrineConfig.internalName);
                Debug.Log($"Shrine: Set random config '{shrineConfig.internalName}' to ZDO");
            }
        }
        string configName = zdo.GetString(ShrineConfigKey, "");
        shrineConfig = ShrineDB.GetShrineConfig(configName);
        Debug.Log("Shrine Awake: Loaded shrine config '" + shrineConfig.internalName + "'");
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (CheckUserInventory(user))
        {
            Debug.Log("User has the requirements, adding status effect");
            user.GetSEMan().AddStatusEffect(shrineConfig.statusEffect);
            List<Character> characters = Character.GetAllCharacters();
            foreach (Character character in characters)
            {
                character.GetSEMan().AddStatusEffect(shrineConfig.statusEffect);
            }
            return true;
        }

        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return true;
    }

    public string GetHoverText()
    {
        ShrineOffering offering = shrineConfig.shrineOffering;
        
        string hoverText;
        string itemName = offering.offeringItem.m_shared.m_name;
        int quantity = offering.itemQuantity;
        string effectDetails = "";
        if (shrineConfig.statusEffect is SE_Stats stats)
        {
            effectDetails = ShrineUtils.GetEffectDescription(stats);
            float durationMinutes = stats.m_ttl / 60f;
            string durationText = durationMinutes > 0f ? $"Duration: <color=orange>{durationMinutes:0.#} min</color>\n" : "";
            
            hoverText = Localization.instance.Localize(
                $"{shrineConfig.displayName}\n" +
                $"[<color=yellow><b>$KEY_Use</b></color>] Sacrifice {quantity}x <color=orange>{itemName}</color>\n" +
                $"{durationText}" +
                $"{effectDetails}");
            return hoverText;
        }

        hoverText = Localization.instance.Localize(
            $"{shrineConfig.displayName}\n" +
            $"[<color=yellow><b>$KEY_Use</b></color>] Sacrifice {quantity}x <color=orange>{itemName}</color>\n");

        return hoverText;
    }

    public string GetHoverName()
    {
        return Localization.instance.Localize(this.name);
    }

    public bool CheckUserInventory(Humanoid user)
    {
        if (!hasBeenUsedOnce)
        {
            return true;   
        }
        
        bool hasRequiredItems = false;
        foreach (var item in user.GetInventory().GetAllItems())
        {
            Debug.Log(item.m_shared.m_name);
        }
        
        ShrineOffering offering = shrineConfig.shrineOffering;
        
        Debug.Log(offering.itemQuantity + " " + offering.offeringItem.m_shared.m_name);
        
        ItemDrop.ItemData? match = user.GetInventory().GetAllItems()
            .FirstOrDefault(i => i.m_shared.m_name == offering.offeringItem.m_shared.m_name);

        if (match != null && user.GetInventory().RemoveItem(match, offering.itemQuantity))
        {
            user.ShowRemovedMessage(match, offering.itemQuantity);
            hasRequiredItems = true;
        }
        
        return hasRequiredItems;
    }

    public class ShrineOffering
    {
        public ItemDrop.ItemData offeringItem;

        public int itemQuantity;

        public ShrineOffering(string itemName, int quantity)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(itemName);
            if (prefab == null)
            {
                Debug.LogError($"ShrineOffering: prefab '{itemName}' not found");
                return;
            }

            ItemDrop drop = prefab.GetComponent<ItemDrop>();
            if (drop == null)
            {
                Debug.LogError($"ShrineOffering: prefab '{itemName}' has no ItemDrop component");
                return;
            }

            offeringItem = drop.m_itemData;

            itemQuantity = quantity;
        }
    }
}