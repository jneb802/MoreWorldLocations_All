using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Configs;
using Jotunn.Entities;
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
        SphereCollider collider = areaObject.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = areaObject.AddComponent<SphereCollider>();
        }

        collider.radius = radius;
        collider.isTrigger = true;

        EffectArea effectArea = areaObject.GetComponent<EffectArea>();
        if (effectArea == null)
        {
            effectArea = areaObject.AddComponent<EffectArea>();
        }

        effectArea.m_type = EffectArea.Type.None;
        effectArea.m_statusEffect = StatusEffectName;
        effectArea.m_playerOnly = true;

        ProtectedLocationPeacefulAreaApplier areaApplier = areaObject.GetComponent<ProtectedLocationPeacefulAreaApplier>();
        if (areaApplier == null)
        {
            areaApplier = areaObject.AddComponent<ProtectedLocationPeacefulAreaApplier>();
        }

        areaApplier.Initialize(radius);
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
}

public sealed class ProtectedLocationPeacefulAreaApplier : MonoBehaviour
{
    private const float ApplyInterval = 0.5f;

    private float _radiusSqrXZ;
    private float _nextApplyTime;
    private int _statusEffectHash;
    private bool _loggedStatusActive;
    private bool _loggedMissingStatusEffect;

    public void Initialize(float radius)
    {
        _radiusSqrXZ = radius * radius;
        _statusEffectHash = ProtectedLocationPeacefulArea.StatusEffectName.GetStableHashCode();
    }

    private void FixedUpdate()
    {
        EnsureInitialized();

        if (_statusEffectHash == 0 || _radiusSqrXZ <= 0f || Time.time < _nextApplyTime)
        {
            return;
        }

        _nextApplyTime = Time.time + ApplyInterval;

        Player player = Player.m_localPlayer;
        if (player == null)
        {
            return;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3 areaPosition = transform.position;
        float deltaX = playerPosition.x - areaPosition.x;
        float deltaZ = playerPosition.z - areaPosition.z;
        float distanceSqrXZ = deltaX * deltaX + deltaZ * deltaZ;
        if (distanceSqrXZ > _radiusSqrXZ)
        {
            return;
        }

        StatusEffect? peacefulStatusEffect = GetPeacefulStatusEffect();
        if (peacefulStatusEffect == null)
        {
            if (!_loggedMissingStatusEffect)
            {
                _loggedMissingStatusEffect = true;
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                    $"Peaceful status effect {ProtectedLocationPeacefulArea.StatusEffectName} is unavailable.");
            }

            return;
        }

        SEMan seMan = player.GetSEMan();
        StatusEffect? statusEffect = seMan.AddStatusEffect(peacefulStatusEffect, true);
        if (!_loggedStatusActive && (statusEffect != null || seMan.HaveStatusEffect(_statusEffectHash)))
        {
            _loggedStatusActive = true;
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug(
                $"Peaceful status active for {player.name} inside {name}.");
        }
    }

    private static StatusEffect? GetPeacefulStatusEffect()
    {
        if (StatusEffectDB.StatusEffects.TryGetValue(
                ProtectedLocationPeacefulArea.StatusEffectName,
                out CustomStatusEffect customStatusEffect))
        {
            return customStatusEffect.StatusEffect;
        }

        return ObjectDB.instance != null ? ObjectDB.instance.GetStatusEffect(ProtectedLocationPeacefulArea.StatusEffectName.GetStableHashCode()) : null;
    }

    private void EnsureInitialized()
    {
        if (_statusEffectHash == 0)
        {
            _statusEffectHash = ProtectedLocationPeacefulArea.StatusEffectName.GetStableHashCode();
        }

        if (_radiusSqrXZ > 0f)
        {
            return;
        }

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            Initialize(sphereCollider.radius);
        }
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
