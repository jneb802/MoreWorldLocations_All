using UnityEngine;

namespace More_World_Locations_AIO.Traders;

public class SkillBook_SE : StatusEffect
{
    private bool shouldRemove = false;
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
            shouldRemove = true;
        }
    }
    
    public override bool IsDone()
    {
        return shouldRemove || base.IsDone();
    }
}
