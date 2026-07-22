using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using More_World_Locations_AIO.Managers;
using UnityEngine;

namespace More_World_Locations_AIO;

public static class ProtectedLocationWearNTearPatch
{
    private const float ProtectedPieceHealth = 9999f;

    private static readonly HashSet<string> ProtectedLocations = LocationDefinitions.Ports
        .Concat(LocationDefinitions.Traders)
        .Concat(LocationDefinitions.Trainers)
        .Select(location => Helpers.GetNormalizedName(location.Name))
        .ToHashSet();

    private static readonly HashSet<string> ProtectedLocationGroups = new()
    {
        "MWL_Ports",
        "MWL_Trader"
    };

    internal static void SetProtectedPieceHealth(WearNTear wearNTear)
    {
        if (wearNTear == null) return;

        ZNetView view = wearNTear.GetComponent<ZNetView>();
        if (view == null || !view.IsValid())
        {
            wearNTear.m_health = ProtectedPieceHealth;
            return;
        }

        float currentZdoHealth = view.GetZDO().GetFloat(ZDOVars.s_health, wearNTear.m_health);
        if (Mathf.Approximately(wearNTear.m_health, ProtectedPieceHealth) &&
            Mathf.Approximately(currentZdoHealth, ProtectedPieceHealth))
        {
            return;
        }

        wearNTear.m_health = ProtectedPieceHealth;
        if (!view.IsOwner()) return;

        view.GetZDO().Set(ZDOVars.s_health, ProtectedPieceHealth);
        view.InvokeRPC(ZNetView.Everybody, "RPC_HealthChanged", ProtectedPieceHealth);
    }

    internal static void ProtectLocationRoot(GameObject locationRoot)
    {
        if (locationRoot == null) return;

        foreach (WearNTear wearNTear in locationRoot.GetComponentsInChildren<WearNTear>(true))
        {
            SetProtectedPieceHealth(wearNTear);
        }
    }

    internal static bool IsProtectedLocationName(ZoneSystem.ZoneLocation location)
    {
        if (location == null) return false;
        if (IsProtectedLocationName(location.m_name)) return true;
        if (IsProtectedLocationName(location.m_prefabName)) return true;
        if (ProtectedLocationGroups.Contains(location.m_group)) return true;

        return location.m_prefab != null && IsProtectedLocationName(location.m_prefab.Name);
    }

    internal static bool IsProtectedLocationName(string locationName)
    {
        return !string.IsNullOrEmpty(locationName) &&
               ProtectedLocations.Contains(Helpers.GetNormalizedName(locationName));
    }
}

[HarmonyPatch(typeof(ZoneSystem), "SpawnLocation")]
public static class ProtectedLocationSpawnPatch
{
    [UsedImplicitly]
    private static void Prefix(ZoneSystem.ZoneLocation location, out bool __state)
    {
        __state = ProtectedLocationWearNTearPatch.IsProtectedLocationName(location);
    }

    [UsedImplicitly]
    private static void Postfix(bool __state, GameObject __result)
    {
        if (!__state)
        {
            return;
        }

        ProtectedLocationWearNTearPatch.ProtectLocationRoot(__result);
    }
}

[HarmonyPatch(typeof(Location), nameof(Location.Awake))]
public static class ProtectedLocationAwakePatch
{
    [UsedImplicitly]
    private static void Postfix(Location __instance)
    {
        if (__instance == null)
        {
            return;
        }

        if (!ProtectedLocationWearNTearPatch.IsProtectedLocationName(__instance.name))
        {
            return;
        }

        ProtectedLocationWearNTearPatch.ProtectLocationRoot(__instance.gameObject);
    }
}
