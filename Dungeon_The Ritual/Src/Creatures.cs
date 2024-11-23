using System.Reflection;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Utils;
using UnityEngine;

namespace Dungeon_The_Ritual;

public class Creatures
{
    public static void CreateShamanBoss()
    {
        AssetBundle creatureAssetBundle = AssetUtils.LoadAssetBundleFromResources(
            "put the name of your asset bundle here inside the quotes no spaces",
            Assembly.GetExecutingAssembly());
        
        GameObject shamaPrefab = creatureAssetBundle.LoadAsset<GameObject>("Thornskar_boss_ru");
        
        CreatureConfig shamanBossConfig = new CreatureConfig();
        shamanBossConfig.Name = "$enemy_thornskar";
        shamanBossConfig.Faction = Character.Faction.Boss;
        
        Jotunn.Managers.CreatureManager.Instance.AddCreature(new CustomCreature(shamaPrefab, true, shamanBossConfig));
    }
}