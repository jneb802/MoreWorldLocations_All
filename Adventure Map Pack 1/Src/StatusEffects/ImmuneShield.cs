using Adventure_Map_Pack_1.Utils;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace Adventure_Map_Pack_1.StatusEffects;

public class ImmuneShield : SE_Shield
{
    
    public float m_absorbDamage = 9999f;
    public float ttl = 40f;
    
    public override void Setup(Character character)
    {
        base.Setup(character);
    }
    
}