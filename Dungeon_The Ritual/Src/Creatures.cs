using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace Dungeon_The_Ritual;

public class Creatures
{
    public static void CreateShamanBoss()
    {
        GameObject shamaPrefab = Dungeon_The_RitualPlugin.assetBundle.LoadAsset<GameObject>("Thornskar_boss_ru");
        
        CreatureConfig shamanBossConfig = new CreatureConfig();
        shamanBossConfig.Name = "$enemy_thornskar";
        shamanBossConfig.Faction = Character.Faction.Boss;
        
        Jotunn.Managers.CreatureManager.Instance.AddCreature(new CustomCreature(shamaPrefab, true, shamanBossConfig));
    }
}