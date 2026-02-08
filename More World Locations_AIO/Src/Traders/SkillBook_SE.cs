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
        ApplySkillBook();
    }

    private void ApplySkillBook()
    {
        if (player == null)
        {
            shouldRemove = true;
            return;
        }

        Skills.Skill skill = player.GetSkills().GetSkill(skillType);

        int levelsToGrant = bookTier switch
        {
            1 => 1,
            2 => 3,
            3 => 5,
            _ => 1,
        };

        int currentLevel = (int)skill.m_level;
        levelsToGrant = Mathf.Min(levelsToGrant, 100 - currentLevel);

        if (levelsToGrant <= 0)
        {
            player.Message(MessageHud.MessageType.Center, "$skill_" + skillType.ToString().ToLower() + " is already at max level");
            shouldRemove = true;
            return;
        }

        // Calculate total XP needed to gain the desired number of levels from current level
        float totalXP = 0f;
        for (int i = 0; i < levelsToGrant; i++)
        {
            float levelForCalc = currentLevel + i;
            totalXP += Mathf.Pow(Mathf.Floor(levelForCalc + 1f), 1.5f) * 0.5f + 0.5f;
        }

        // Subtract any existing accumulator progress (player already earned partial XP toward next level)
        totalXP -= skill.m_accumulator;
        if (totalXP < 0f) totalXP = 0f;

        // Add the XP to the accumulator
        skill.m_accumulator += totalXP;

        // Level up loop - matches vanilla Raise() pattern
        while (skill.m_level < 100f)
        {
            float required = Mathf.Pow(Mathf.Floor(skill.m_level + 1f), 1.5f) * 0.5f + 0.5f;
            if (skill.m_accumulator < required)
                break;

            skill.m_level += 1f;
            skill.m_level = Mathf.Clamp(skill.m_level, 0f, 100f);
            skill.m_accumulator = 0f;
            player.OnSkillLevelup(skillType, skill.m_level);
        }

        player.Message(MessageHud.MessageType.Center,
            "$skill_" + skillType.ToString().ToLower() + " increased to " + (int)skill.m_level,
            0, skill.m_info?.m_icon);

        shouldRemove = true;
    }

    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);
    }

    public override bool IsDone()
    {
        return shouldRemove || base.IsDone();
    }
}
