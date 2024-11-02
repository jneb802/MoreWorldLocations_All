using HarmonyLib;
using UnityEngine;

namespace Dungeon_The_Ritual;

public class DungeonGenerator_Patch
{
    [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Load))]
    public class DungeonGenerator_Awake_Patch
    {
        public static void Prefix(DungeonGenerator __instance)
        {
            if (__instance.gameObject.name == "DG_BlackForestDungeon(Clone)")
            {
                Debug.Log("Dungeon calling Load, first add attach puzzle to dungeon generator");
                __instance.gameObject.AddComponent<AttachPuzzle>();
            }
            else
            {
                Debug.Log("Failed to find dungeon generator with name DG_BlackForestDungeon(Clone)");
            }
        }
    }
    
    [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Generate), typeof(int), typeof(ZoneSystem.SpawnMode))]
    public class DungeonGenerator_Generate_Patch
    {
        public static void Postfix(DungeonGenerator __instance)
        {
            if (__instance.gameObject.name == "DG_BlackForestDungeon(Clone)")
            {
                if (__instance == null)
                {
                    return;
                }
                Debug.Log("Dungeon generator finished calling Generate, now call AttachPuzzle.Initialize");
                __instance.gameObject.GetComponent<AttachPuzzle>().Initialize();
            }
            else
            {
                Debug.Log("Failed to find dungeon generator with name DG_BlackForestDungeon(Clone)");
            }
        }
    }
}