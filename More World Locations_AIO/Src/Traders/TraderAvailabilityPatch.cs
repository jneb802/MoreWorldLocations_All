using System.Collections.Generic;
using HarmonyLib;

namespace More_World_Locations_AIO.Traders;

public class TraderAvailabilityPatch
{
    private static readonly Dictionary<string, string> NotRequiredKeys = new();

    public static void Register(string prefabName, string notRequiredKey)
    {
        NotRequiredKeys[prefabName] = notRequiredKey;
    }

    [HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
    public static class Trader_GetAvailableItems_Patch
    {
        public static void Postfix(ref List<Trader.TradeItem> __result)
        {
            if (ZoneSystem.instance == null) return;

            __result.RemoveAll(item =>
            {
                if (item.m_prefab == null) return false;
                string name = item.m_prefab.gameObject.name;
                return NotRequiredKeys.TryGetValue(name, out string notKey) &&
                       !string.IsNullOrEmpty(notKey) &&
                       ZoneSystem.instance.GetGlobalKey(notKey);
            });
        }
    }
}
