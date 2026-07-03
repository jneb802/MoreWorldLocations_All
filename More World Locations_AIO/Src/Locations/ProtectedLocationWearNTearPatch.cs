using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using More_World_Locations_AIO.Managers;

namespace More_World_Locations_AIO;

[HarmonyPatch(typeof(Location), nameof(Location.Awake))]
public static class ProtectedLocationWearNTearPatch
{
    private const float ProtectedPieceHealth = 9999f;

    private static readonly HashSet<string> ProtectedLocations = LocationDefinitions.Ports
        .Concat(LocationDefinitions.Traders)
        .Select(location => Helpers.GetNormalizedName(location.Name))
        .ToHashSet();

    [UsedImplicitly]
    private static void Postfix(Location __instance)
    {
        if (__instance == null) return;

        string locationName = Helpers.GetNormalizedName(__instance.name);
        if (!ProtectedLocations.Contains(locationName)) return;

        foreach (WearNTear wearNTear in __instance.GetComponentsInChildren<WearNTear>(true))
        {
            SetProtectedPieceHealthOnce(wearNTear);
        }
    }

    private static void SetProtectedPieceHealthOnce(WearNTear wearNTear)
    {
        if (wearNTear == null) return;
        if (wearNTear.m_health == ProtectedPieceHealth) return;

        wearNTear.m_health = ProtectedPieceHealth;

        ZNetView view = wearNTear.GetComponent<ZNetView>();
        if (view == null || !view.IsValid() || !view.IsOwner()) return;

        view.GetZDO().Set(ZDOVars.s_health, ProtectedPieceHealth);
        view.InvokeRPC(ZNetView.Everybody, "RPC_HealthChanged", ProtectedPieceHealth);
    }
}
