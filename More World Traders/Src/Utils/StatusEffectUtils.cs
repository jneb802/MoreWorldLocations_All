using Jotunn.Entities;
using Jotunn.Managers;
using More_World_Traders.StatusEffects;
using UnityEngine;

namespace More_World_Traders.Utils;

public class StatusEffectUtils
{
    public static void CreateCustomStatusEffects()
    {
        BlacksmithStone_SE blacksmithStone = ScriptableObject.CreateInstance<BlacksmithStone_SE>();
        blacksmithStone.name = "BlacksmithStoneStatusEffect";
        blacksmithStone.m_name = "$se_blacksmithStone";
        CustomStatusEffect blacksmithStoneSE = new CustomStatusEffect(blacksmithStone, false);
        
        SkillBook_SE skillBook_SE = ScriptableObject.CreateInstance<SkillBook_SE>();
        skillBook_SE.name = "SkillBookStatusEffect";
        skillBook_SE.m_name = "$se_skillBook";
        CustomStatusEffect skillBookSE = new CustomStatusEffect(skillBook_SE, false);

        ItemManager.Instance.AddStatusEffect(blacksmithStoneSE);
        ItemManager.Instance.AddStatusEffect(skillBookSE);
    }
}