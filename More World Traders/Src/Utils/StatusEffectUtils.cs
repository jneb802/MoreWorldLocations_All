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

        ItemManager.Instance.AddStatusEffect(blacksmithStoneSE);
    }
}