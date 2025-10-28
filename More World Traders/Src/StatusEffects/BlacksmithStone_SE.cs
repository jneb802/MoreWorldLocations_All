using More_World_Traders.Utils;
using System;

namespace More_World_Traders.StatusEffects;

public class BlacksmithStone_SE : StatusEffect
{
    private bool shouldRemove = false;
    private int m_qualityIncreaseAmount = 1;
    private Player? player; // Fix: Make 'player' nullable to resolve CS8618, ensure null checks are in place
    public int stoneTier = 1;
    
    public override void Setup(Character character)
    {
        base.Setup(character);
        player = character as Player ?? throw new InvalidCastException("Character is not a Player.");
        this.m_character.Message(this.m_startMessageType, "Using Blacksmith Stone to enhance an item...");
    }
    
    public bool EnhanceItem()
    {
        if (player != null)
        {
            Inventory inventory = player.GetInventory();
            ItemDrop.ItemData item = inventory.GetItemAt(0, 0);
            
            if (CheckItem(item) && isQualityCompatible(item))
            {
                item.m_quality += (int)m_qualityIncreaseAmount;
                player.Message(MessageHud.MessageType.TopLeft, "Item: " + item.m_shared.m_name + " was enhanced");
                return true;
            }
            else
            {
                player.Message(MessageHud.MessageType.TopLeft, "No suitable item found in top left corner of inventory.");
                return false;
            }
        }

        return false;
    }
    
    public bool CheckItem(ItemDrop.ItemData item)
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

    public bool isQualityCompatible(ItemDrop.ItemData item)
    {
        // Shield max level is 3
        if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
        {
            if (item.m_quality == stoneTier + 2)
            {
                return true;
            } 
        }
        else
        {
            if (item.m_quality == stoneTier + 3)
            {
                return true;
            }  
        }
        return false;
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
            if (player != null)
            {
                Inventory inventory = player.GetInventory();
                inventory.AddItem(GetBlacksmithStoneItemData(), inventory.FindEmptySlot(true));
                shouldRemove = true;
            }
        }
    }

    public ItemDrop.ItemData GetBlacksmithStoneItemData()
    {
        switch (stoneTier)
        {
            case 1:
                return PrefabUtils.blacksmithStoneItemData_tier1;
            case 2:
                return PrefabUtils.blacksmithStoneItemData_tier2;
            case 3:
                return PrefabUtils.blacksmithStoneItemData_tier3;
            default:
                return PrefabUtils.blacksmithStoneItemData_tier1;
        }
    }
    
    public override bool IsDone()
    {
        return shouldRemove || base.IsDone();
    }
}