using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using More_World_Locations_AIO.Managers;
using UnityEngine;

namespace More_World_Locations_AIO;

[HarmonyPatch(typeof(Location), nameof(Location.Awake))]
public static class ProtectedLocationWearNTearPatch
{
    private const float ProtectedPieceHealth = 9999f;
    private const float MinimumProtectedRadius = 96f;

    private static readonly HashSet<string> ProtectedLocations = LocationDefinitions.Ports
        .Concat(LocationDefinitions.Traders)
        .Concat(LocationDefinitions.Trainers)
        .Select(location => Helpers.GetNormalizedName(location.Name))
        .ToHashSet();

    private static readonly string[] ProtectedAnchorPrefabs = LocationDefinitions.Traders
        .Select(location => location.Name + "_Vendor")
        .Concat(LocationDefinitions.Trainers.Select(location => location.Name + "_Trainer"))
        .ToArray();

    private static readonly List<ZDO> AnchorZdos = new();
    private static readonly List<Vector3> ProtectedAnchorPositions = new();
    private static int protectedAnchorPositionsFrame = -1;

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

    internal static void SetProtectedPieceHealthOnce(WearNTear wearNTear)
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

        Vector3 position = wearNTear.transform.position;
        foreach (ZoneSystem.LocationInstance locationInstance in zoneSystem.GetLocationList())
        {
            if (!IsProtectedLocationName(locationInstance.m_location)) continue;

            float radius = Mathf.Max(locationInstance.m_location.m_exteriorRadius, locationInstance.m_location.m_interiorRadius);
            radius = Mathf.Max(radius, MinimumProtectedRadius);
            if (global::Utils.DistanceXZ(position, locationInstance.m_position) <= radius) return true;
        }

        return false;
    }

    private static bool IsProtectedLocationName(ZoneSystem.ZoneLocation location)
    {
        if (ProtectedLocations.Contains(Helpers.GetNormalizedName(location.m_name))) return true;
        if (ProtectedLocations.Contains(Helpers.GetNormalizedName(location.m_prefabName))) return true;

        string prefabName = location.m_prefab.Name;
        return !string.IsNullOrEmpty(prefabName) && ProtectedLocations.Contains(Helpers.GetNormalizedName(prefabName));
    }

    internal static bool IsNearProtectedAnchor(Vector3 position)
    {
        if (ZDOMan.instance == null) return false;

        RefreshProtectedAnchorPositions();
        foreach (Vector3 anchorPosition in ProtectedAnchorPositions)
        {
            if (global::Utils.DistanceXZ(position, anchorPosition) <= MinimumProtectedRadius) return true;
        }

        return false;
    }

    private static void RefreshProtectedAnchorPositions()
    {
        if (protectedAnchorPositionsFrame == Time.frameCount) return;

        protectedAnchorPositionsFrame = Time.frameCount;
        ProtectedAnchorPositions.Clear();
        foreach (ZDO port in ShipmentManager.GetPorts())
        {
            ProtectedAnchorPositions.Add(port.GetPosition());
        }

        foreach (string anchorPrefab in ProtectedAnchorPrefabs)
        {
            AnchorZdos.Clear();
            int index = 0;
            while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(anchorPrefab, AnchorZdos, ref index))
            {
            }

            foreach (ZDO anchor in AnchorZdos)
            {
                ProtectedAnchorPositions.Add(anchor.GetPosition());
            }
        }
    }
}

[HarmonyPatch(typeof(WearNTear), "Awake")]
public static class ProtectedLocationWearNTearAwakePatch
{
    [UsedImplicitly]
    private static void Postfix(WearNTear __instance)
    {
        __instance.StartCoroutine(SetProtectedHealthWhenAnchorsAreReady(__instance));
    }

    private static IEnumerator SetProtectedHealthWhenAnchorsAreReady(WearNTear wearNTear)
    {
        float[] delays = { 0f, 2f, 8f };
        foreach (float delay in delays)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            if (wearNTear == null) yield break;
            if (!ProtectedLocationWearNTearPatch.IsProtectedLocationPiece(wearNTear) &&
                !ProtectedLocationWearNTearPatch.IsNearProtectedAnchor(wearNTear.transform.position))
            {
                continue;
            }

            ProtectedLocationWearNTearPatch.SetProtectedPieceHealthOnce(wearNTear);
            yield break;
        }
    }
}
