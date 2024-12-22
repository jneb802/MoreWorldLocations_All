using HarmonyLib;
using More_World_Traders.Utils;
using UnityEngine;
    
namespace More_World_Traders;

public class Minimap_Patch
{
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
    public static class Minimap_Awake_Patch
    {
        public static void Postfix(Minimap __instance)
        {
            Debug.Log("Starting Minimap add icons patch");
            if (__instance.GetLocationIcon(PrefabUtils.blackForestBlacksmith1Icon.m_name) == null)
            {
                Debug.Log("Adding icon for blacksmith1");
                __instance.m_locationIcons.Add(PrefabUtils.blackForestBlacksmith1Icon);
            }
            if (__instance.GetLocationIcon(PrefabUtils.blackForestBlacksmith2Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(PrefabUtils.blackForestBlacksmith2Icon);
            }
            if (__instance.GetLocationIcon(PrefabUtils.mountainsBlacksmith1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(PrefabUtils.mountainsBlacksmith1Icon);
            }
            if (__instance.GetLocationIcon(PrefabUtils.mistlandsBlacksmith1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(PrefabUtils.mistlandsBlacksmith1Icon);
            }
            if (__instance.GetLocationIcon(PrefabUtils.plainsTavern1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(PrefabUtils.plainsTavern1Icon);
            }
            if (__instance.GetLocationIcon(PrefabUtils.plainsCamp1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(PrefabUtils.plainsCamp1Icon);
            }
        }
    }
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetLocationIcon))]
    public static class Minimap_GetLocationIcon_Patch
    {
        public static void Postfix(Minimap __instance)
        {
            foreach (Minimap.LocationSpriteData locationIcon in __instance.m_locationIcons)
            {
                Debug.Log("LocationIcons contains icon with name: " + locationIcon.m_name);
            }
        }
    }
}