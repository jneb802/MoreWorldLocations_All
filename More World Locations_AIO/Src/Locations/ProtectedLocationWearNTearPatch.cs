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
    private const float MinimumProtectedRadius = 96f;
    private static int protectedLocationSpawnDepth;
    private static int protectedLocationBoundsFrame = -1;

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

    private static readonly List<ProtectedLocationBounds> ProtectedLocationBoundsCache = new();

    internal static bool IsSpawningProtectedLocation => protectedLocationSpawnDepth > 0;

    internal static bool EnterProtectedLocationSpawn(ZoneSystem.ZoneLocation location)
    {
        if (!IsProtectedLocationName(location))
        {
            return false;
        }

        protectedLocationSpawnDepth++;
        return true;
    }

    internal static void ExitProtectedLocationSpawn()
    {
        protectedLocationSpawnDepth = Mathf.Max(0, protectedLocationSpawnDepth - 1);
    }

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

    internal static bool IsProtectedLocationPiece(WearNTear wearNTear)
    {
        if (wearNTear == null) return false;

        Piece piece = wearNTear.GetComponent<Piece>();
        if (piece != null && piece.IsPlacedByPlayer()) return false;

        ZoneSystem zoneSystem = ZoneSystem.instance;
        if (zoneSystem == null) return false;

        RefreshProtectedLocationBounds(zoneSystem);

        Vector3 position = wearNTear.transform.position;
        foreach (ProtectedLocationBounds locationBounds in ProtectedLocationBoundsCache)
        {
            if (global::Utils.DistanceXZ(position, locationBounds.Position) <= locationBounds.Radius) return true;
        }

        return false;
    }

    private static void RefreshProtectedLocationBounds(ZoneSystem zoneSystem)
    {
        if (protectedLocationBoundsFrame == Time.frameCount)
        {
            return;
        }

        protectedLocationBoundsFrame = Time.frameCount;
        ProtectedLocationBoundsCache.Clear();

        foreach (ZoneSystem.LocationInstance locationInstance in zoneSystem.GetLocationList())
        {
            if (!IsProtectedLocationName(locationInstance.m_location)) continue;

            ProtectedLocationBoundsCache.Add(new ProtectedLocationBounds
            {
                Position = locationInstance.m_position,
                Radius = GetProtectedLocationRadius(locationInstance.m_location)
            });
        }
    }

    private static float GetProtectedLocationRadius(ZoneSystem.ZoneLocation location)
    {
        float radius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
        return Mathf.Max(radius, MinimumProtectedRadius);
    }

    private static bool IsProtectedLocationName(ZoneSystem.ZoneLocation location)
    {
        if (location == null) return false;
        if (IsProtectedLocationName(location.m_name)) return true;
        if (IsProtectedLocationName(location.m_prefabName)) return true;
        if (ProtectedLocationGroups.Contains(location.m_group)) return true;

        return location.m_prefab != null && IsProtectedLocationName(location.m_prefab.Name);
    }

    private static bool IsProtectedLocationName(string locationName)
    {
        return !string.IsNullOrEmpty(locationName) &&
               ProtectedLocations.Contains(Helpers.GetNormalizedName(locationName));
    }

    private struct ProtectedLocationBounds
    {
        public Vector3 Position;
        public float Radius;
    }
}

[HarmonyPatch(typeof(ZoneSystem), "SpawnLocation")]
public static class ProtectedLocationSpawnPatch
{
    [UsedImplicitly]
    private static void Prefix(ZoneSystem.ZoneLocation location, out bool __state)
    {
        __state = ProtectedLocationWearNTearPatch.EnterProtectedLocationSpawn(location);
    }

    [UsedImplicitly]
    private static void Finalizer(bool __state)
    {
        if (!__state)
        {
            return;
        }

        ProtectedLocationWearNTearPatch.ExitProtectedLocationSpawn();
    }
}

[HarmonyPatch(typeof(WearNTear), "Awake")]
public static class ProtectedLocationWearNTearAwakePatch
{
    [UsedImplicitly]
    private static void Postfix(WearNTear __instance)
    {
        if (!ProtectedLocationWearNTearPatch.IsSpawningProtectedLocation &&
            !ProtectedLocationWearNTearPatch.IsProtectedLocationPiece(__instance))
        {
            return;
        }

        ProtectedLocationWearNTearPatch.SetProtectedPieceHealth(__instance);
    }
}
