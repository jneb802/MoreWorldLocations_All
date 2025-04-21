using HarmonyLib;
using Jotunn;
using UnityEngine;

namespace Underground_Ruins;

public class DungeonGenerator_Patch
{
    [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Load))]
    public class DungeonGenerator_Awake_Patch
    {
        public static void Prefix(DungeonGenerator __instance)
        {
            if (__instance.gameObject.name == "DG_BlackForestDungeon(Clone)")
            {
                __instance.gameObject.AddComponent<AttachPuzzle>();
            }
        }
    }
    
    [HarmonyPatch(typeof(DungeonGenerator), "Spawn")]
    public class DungeonGenerator_Spawn_Patch
    {
        public static void Postfix(DungeonGenerator __instance)
        {
            AttachPuzzle attachPuzzle = __instance.GetComponent<AttachPuzzle>();
            if (attachPuzzle != null)
            {
                attachPuzzle.StartCoroutine(attachPuzzle.DelayedPuzzleInit(__instance.transform.position));
            }
        }
    }
}