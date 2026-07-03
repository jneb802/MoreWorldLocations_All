using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using More_World_Locations_AIO.Managers;

namespace More_World_Locations_AIO;

[HarmonyPatch(typeof(Location), nameof(Location.Awake))]
public static class PortLocationWearNTearPatch
{
    private const float PortPieceHealth = 9999f;

    private static readonly HashSet<string> PortLocations = new()
    {
        "MWL_Port1",
        "MWL_Port2",
        "MWL_Port3",
        "MWL_Port4",
        "MWL_Port5"
    };

    [UsedImplicitly]
    private static void Postfix(Location __instance)
    {
        if (__instance == null) return;

        string locationName = Helpers.GetNormalizedName(__instance.name);
        if (!PortLocations.Contains(locationName)) return;

        foreach (WearNTear wearNTear in __instance.GetComponentsInChildren<WearNTear>(true))
        {
            SetPortPieceHealthOnce(wearNTear);
        }
    }

    private static void SetPortPieceHealthOnce(WearNTear wearNTear)
    {
        if (wearNTear == null) return;
        if (wearNTear.m_health == PortPieceHealth) return;

        wearNTear.m_health = PortPieceHealth;

        ZNetView view = wearNTear.GetComponent<ZNetView>();
        if (view == null || !view.IsValid() || !view.IsOwner()) return;

        view.GetZDO().Set(ZDOVars.s_health, PortPieceHealth);
        view.InvokeRPC(ZNetView.Everybody, "RPC_HealthChanged", PortPieceHealth);
    }
}
