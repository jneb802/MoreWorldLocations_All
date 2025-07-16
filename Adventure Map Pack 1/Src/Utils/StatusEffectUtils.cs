using Adventure_Map_Pack_1.StatusEffects;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Adventure_Map_Pack_1.Utils;

public class StatusEffectUtils
{
    public static void CreateCustomStatusEffects()
    {
        ImmuneShield immuneShield = ScriptableObject.CreateInstance<ImmuneShield>();
        immuneShield.name = "ImmuneShieldStatusEffect";
        immuneShield.m_name = "$se_ImmuneShield";
        immuneShield.m_breakEffects = PrefabUtils.immuneShieldBreak;
        immuneShield.m_hitEffects = PrefabUtils.immuneShieldHit;
        immuneShield.m_startEffects = PrefabUtils.immuneShieldBubble;
        CustomStatusEffect immuneShieldSE = new CustomStatusEffect(immuneShield, false);

        ItemManager.Instance.AddStatusEffect(immuneShieldSE);
    }
}