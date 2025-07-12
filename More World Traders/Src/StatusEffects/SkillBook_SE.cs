using Mono.Security.X509;
using More_World_Traders.Utils;
using UnityEngine;

namespace More_World_Traders.StatusEffects;

public class SkillBook_SE : StatusEffect
{
    private bool shouldRemove = false;
    private int m_skillIncreaseAmount = 1;
    private Player player;
    public int bookTier = 1;
    public Skills.SkillType skillType;
    
    public override void Setup(Character character)
    {
        base.Setup(character);
        player = character as Player;
        this.m_character.Message(this.m_startMessageType, "Using Skill Book to raise skill...");
    }

    public bool RaiseSkill()
    {
        player.RaiseSkill(skillType);
        return true;
    }
    
    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);
        
        if (RaiseSkill())
        {
            shouldRemove = true;
        }
        else
        {
            Inventory inventory = player.GetInventory();
            inventory.AddItem(GetSkillItemData(), inventory.FindEmptySlot(true));
            shouldRemove = true;
        }
    }
    
    public ItemDrop.ItemData GetSkillItemData()
    {
        switch (bookTier)
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