using Mono.Security.X509;
using More_World_Traders.Utils;
using UnityEngine;

namespace More_World_Traders.StatusEffects;

public class BlacksmithStone_SE : StatusEffect
{
    private bool shouldRemove = false;
    private int m_qualityIncreaseAmount = 1;
    private Player player;
    
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

            if (item != null)
            {
                item.m_quality += (int)m_qualityIncreaseAmount;
                player.Message(MessageHud.MessageType.TopLeft, "Item: + " + item.m_shared.m_name + " was enhanced");
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
    
    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);
        
        if (EnhanceItem())
        {
            Inventory inventory = player.GetInventory();
            inventory.RemoveItem(PrefabUtils.blacksmithStoneItemData, 1);
            shouldRemove = true;
        }
        else
        {
            shouldRemove = true;
        }
    }
    
    public override bool IsDone()
    {
        return shouldRemove || base.IsDone();
    }
}