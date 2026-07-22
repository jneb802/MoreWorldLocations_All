using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Configs;
using Jotunn.Managers;
using More_World_Locations_AIO.Managers;
using More_World_Locations_AIO.Shrines;
using SoftReferenceableAssets;
using UnityEngine;

namespace More_World_Locations_AIO;

public static class ProtectedLocationPeacefulArea
{
    internal const string StatusEffectName = "MWL_SE_Peaceful";
    private const string AreaObjectName = "MWL_PeacefulArea";
    private const float AreaHeight = 200f;

    private static readonly FieldInfo StatusEffectHashField = AccessTools.Field(typeof(EffectArea), "m_statusEffectHash");

    private static readonly HashSet<string> ProtectedLocations = LocationDefinitions.Ports
        .Concat(LocationDefinitions.Traders)
        .Concat(LocationDefinitions.Trainers)
        .Select(location => Helpers.GetNormalizedName(location.Name))
        .ToHashSet();

    public static void RegisterPrefabPatch(string locationName, LocationConfig locationConfig)
    {
        if (!IsProtectedLocationName(locationName))
        {
            return;
        }

        SoftReference<GameObject> prefabReference = AssetManager.Instance.GetSoftReference<GameObject>(locationName);
        AssetManager.Instance.ResolveMocksOnLoad(
            prefabReference.m_assetID,
            null,
            resolvedObj =>
            {
                if (resolvedObj is GameObject locationRoot)
                {
                    AddPeacefulArea(locationRoot, locationConfig);
                }
            });
    }

    internal static void AddPeacefulArea(GameObject locationRoot, LocationConfig? locationConfig = null)
    {
        if (locationRoot == null)
        {
            return;
        }

        float radius = GetPeacefulRadius(locationRoot, locationConfig);
        Transform existingArea = locationRoot.transform.Find(AreaObjectName);
        if (existingArea != null)
        {
            ConfigurePeacefulArea(existingArea.gameObject, radius);
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug(
                $"Configured Peaceful effect area on {Helpers.GetNormalizedName(locationRoot.name)} with radius {radius:0.#}.");
            return;
        }

        GameObject areaObject = new(AreaObjectName);
        areaObject.SetActive(false);
        areaObject.transform.SetParent(locationRoot.transform, false);
        areaObject.transform.localPosition = Vector3.zero;
        areaObject.transform.localRotation = Quaternion.identity;
        areaObject.transform.localScale = Vector3.one;

        ConfigurePeacefulArea(areaObject, radius);

        areaObject.SetActive(true);
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug(
            $"Added Peaceful effect area to {Helpers.GetNormalizedName(locationRoot.name)} with radius {radius:0.#}.");
    }

    private static void ConfigurePeacefulArea(GameObject areaObject, float radius)
    {
        CapsuleCollider collider = areaObject.GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = areaObject.AddComponent<CapsuleCollider>();
        }

        collider.radius = radius;
        collider.height = Mathf.Max(AreaHeight, radius * 2f);
        collider.direction = 1;
        collider.center = Vector3.zero;
        collider.isTrigger = true;

        EffectArea effectArea = areaObject.GetComponent<EffectArea>();
        if (effectArea == null)
        {
            effectArea = areaObject.AddComponent<EffectArea>();
        }

        effectArea.m_type = EffectArea.Type.None;
        effectArea.m_statusEffect = StatusEffectName;
        effectArea.m_playerOnly = true;
        SetStatusEffectHash(effectArea);
    }

    internal static bool HasPeacefulArea(GameObject locationRoot)
    {
        return locationRoot != null && locationRoot.transform.Find(AreaObjectName) != null;
    }

    internal static bool IsProtectedLocationName(string locationName)
    {
        return !string.IsNullOrEmpty(locationName) &&
               ProtectedLocations.Contains(Helpers.GetNormalizedName(locationName));
    }

    private static float GetPeacefulRadius(GameObject locationRoot, LocationConfig? locationConfig)
    {
        if (locationConfig != null && locationConfig.ExteriorRadius > 0f)
        {
            return locationConfig.ExteriorRadius;
        }

        Location location = locationRoot.GetComponent<Location>();
        return location != null ? Mathf.Max(location.m_exteriorRadius, 0f) : 0f;
    }

    private static void SetStatusEffectHash(EffectArea effectArea)
    {
        StatusEffectHashField?.SetValue(effectArea, StatusEffectName.GetStableHashCode());
    }
}

[HarmonyPatch(typeof(Location), nameof(Location.Awake))]
public static class ProtectedLocationPeacefulAwakePatch
{
    [UsedImplicitly]
    private static void Postfix(Location __instance)
    {
        if (__instance == null)
        {
            return;
        }

        bool isProtectedLocation = ProtectedLocationPeacefulArea.IsProtectedLocationName(__instance.name);
        if (!isProtectedLocation && !ProtectedLocationPeacefulArea.HasPeacefulArea(__instance.gameObject))
        {
            return;
        }

        ProtectedLocationPeacefulArea.AddPeacefulArea(__instance.gameObject);
    }
}
