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

        // Raise the level directly. Vanilla Raise() zeroes the accumulator on levelup,
        // so banking multi-level XP into m_accumulator doesn't work — we'd lose all carry-over
        // and only gain one level. Preserve the player's existing accumulator progress.
        for (int i = 0; i < levelsToGrant; i++)
        {
            skill.m_level = Mathf.Clamp(skill.m_level + 1f, 0f, 100f);
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
