using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.Traders;

public class BlacksmithStone_SE : StatusEffect
{
    private bool shouldRemove = false;
    private Player? player;
    public int stoneTier = 1;
    
    public override void Setup(Character character)
    {
        base.Setup(character);
        player = character as Player;
        this.m_character.Message(this.m_startMessageType, "Using Blacksmith Stone to enhance an item...");
    }
    
    public bool EnhanceItem()
    {
        if (player != null)
        {
            Inventory inventory = player.GetInventory();
            ItemDrop.ItemData item = inventory.GetItemAt(0, 0);

            if (CanEnhanceItem(item, stoneTier))
            {
                item.m_quality += 1;
                player.Message(MessageHud.MessageType.Center, "Item: " + item.m_shared.m_name + " was enhanced");
                return true;
            }
            else
            {
                player.Message(MessageHud.MessageType.Center, "No suitable item found in top left corner of inventory.");
                return false;
            }
        }

        return false;
    }
    
    public static bool CheckItem(ItemDrop.ItemData item)
    {
        if (item == null || item.m_shared == null || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.None)
        {
            return false;
        }
        
        switch (item.m_shared.m_itemType)
        {
            case ItemDrop.ItemData.ItemType.OneHandedWeapon:
            case ItemDrop.ItemData.ItemType.Bow:
            case ItemDrop.ItemData.ItemType.Shield:
            case ItemDrop.ItemData.ItemType.Helmet:
            case ItemDrop.ItemData.ItemType.Chest:
            case ItemDrop.ItemData.ItemType.Legs:
            case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
            case ItemDrop.ItemData.ItemType.Torch:
            case ItemDrop.ItemData.ItemType.Shoulder:
            case ItemDrop.ItemData.ItemType.Tool:
            case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                return true;
            default:
                return false;
        }
    }

    public static bool isQualityCompatible(ItemDrop.ItemData item, int stoneTier)
    {
        return item.m_quality == item.m_shared.m_maxQuality + (stoneTier - 1);
    }

    public static bool CanEnhanceTopLeftItem(Player player, int stoneTier)
    {
        if (player == null)
        {
            return false;
        }

        Inventory inventory = player.GetInventory();
        return CanEnhanceItem(inventory.GetItemAt(0, 0), stoneTier);
    }

    public static bool CanEnhanceItem(ItemDrop.ItemData item, int stoneTier)
    {
        return CheckItem(item) && isQualityCompatible(item, stoneTier);
    }

    public static bool TryGetBlacksmithStone(ItemDrop.ItemData item, out BlacksmithStone_SE blacksmithStone)
    {
        BlacksmithStone_SE? matchedStone = item?.m_shared?.m_consumeStatusEffect as BlacksmithStone_SE;
        if (matchedStone == null)
        {
            blacksmithStone = null!;
            return false;
        }

        blacksmithStone = matchedStone;
        return true;
    }
    
    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);
        
        if (EnhanceItem())
        {
            shouldRemove = true;
        }
        else
        {
            if (player == null)
            {
                shouldRemove = true;
                return;
            }

            Inventory inventory = player.GetInventory();
            inventory.AddItem(GetBlacksmithStoneItemData().Clone());
            shouldRemove = true;
        }
    }

    public ItemDrop.ItemData GetBlacksmithStoneItemData()
    {
        switch (stoneTier)
        {
            case 1:
                return TraderItems.blacksmithStoneItemData_tier1;
            case 2:
                return TraderItems.blacksmithStoneItemData_tier2;
            case 3:
                return TraderItems.blacksmithStoneItemData_tier3;
            default:
                return TraderItems.blacksmithStoneItemData_tier1;
        }
    }
    
    public override bool IsDone()
    {
        return shouldRemove || base.IsDone();
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.CanConsumeItem))]
public static class BlacksmithStoneCanConsumeItemPatch
{
    private static bool Prefix(Player __instance, ItemDrop.ItemData item, ref bool __result)
    {
        if (!BlacksmithStone_SE.TryGetBlacksmithStone(item, out BlacksmithStone_SE blacksmithStone))
        {
            return true;
        }

        if (BlacksmithStone_SE.CanEnhanceTopLeftItem(__instance, blacksmithStone.stoneTier))
        {
            return true;
        }

        __instance.Message(MessageHud.MessageType.Center, "No suitable item found in top left corner of inventory.");
        __result = false;
        return false;
    }
}
