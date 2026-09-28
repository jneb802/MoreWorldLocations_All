using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// Where <see cref="EmissionTrace"/> is attached to the game. Every hook
/// returns at once unless the switch named a template, and every hook body is
/// guarded: a failing trace is logged and the spawn proceeds untouched.
/// </summary>
public static class EmissionTracePatches
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    /// <summary>Read the switch once, at startup. Unset means no trace.</summary>
    public static void Install()
    {
        if (!ValidationSwitches.EmissionTraceRequested(out IReadOnlyCollection<string> names, out bool all))
            return;
        EmissionTrace.Configure(names, all);
        Log.LogWarning(
            $"{ValidationSwitches.EmissionTraceVariable} is set: recording selection, emission and destruction for " +
            (all ? "every template" : $"{names.Count} template(s) [{string.Join(", ", new List<string>(names).ToArray())}]") +
            ". Diagnostic output only.");
    }

    private static string Fingerprint(ZoneSystem.ZoneLocation location)
    {
        try
        {
            GameObject asset = location.m_prefab.Asset;
            TemplateFacts facts = TemplateFactsExtractor.Extract(
                location.m_prefabName, "", asset, stockPrefabOf: TemplateAssets.StockPrefabs);
            return TemplateFingerprint.Of(facts);
        }
        catch (Exception ex)
        {
            return "?(" + ex.GetType().Name + ")";
        }
    }

    private static void Guard(string where, Action body)
    {
        try
        {
            body();
        }
        catch (Exception ex)
        {
            Log.LogWarning($"{EmissionTrace.Tag} {where} could not record: {ex.GetType().Name}: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
    private static class SpawnPatch
    {
        private static void Prefix(ZoneSystem.ZoneLocation location, int seed, Vector3 pos, Quaternion rot,
            ZoneSystem.SpawnMode mode)
        {
            if (!EmissionTrace.Enabled || location == null)
                return;
            if (mode != ZoneSystem.SpawnMode.Ghost && mode != ZoneSystem.SpawnMode.Full)
                return;
            if (!EmissionTrace.Traces(location.m_prefabName))
                return;
            Guard("SpawnLocation prefix", () =>
            {
                location.m_prefab.Load();
                GameObject asset = location.m_prefab.Asset;
                ZNetView[] views = global::Utils.GetEnabledComponentsInChildren<ZNetView>(asset);
                EmissionTrace.BeginSite(location.m_prefabName, asset, seed, pos, rot, mode.ToString(),
                    Fingerprint(location), views);
            });
        }

        private static void Postfix()
        {
            if (!EmissionTrace.Enabled)
                return;
            Guard("SpawnLocation postfix", EmissionTrace.EndSite);
        }
    }

    [HarmonyPatch(typeof(RandomSpawn), nameof(RandomSpawn.Randomize))]
    private static class RandomSpawnPatch
    {
        private static void Postfix(RandomSpawn __instance)
        {
            if (!EmissionTrace.Enabled || EmissionTrace.CurrentSite == null)
                return;
            Guard("RandomSpawn.Randomize", () =>
            {
                GameObject go = __instance.gameObject;
                EmissionTrace.Randomised("RandomSpawn", go, go.activeSelf, EmissionTrace.NetworkedUnder(go));
            });
        }
    }

    [HarmonyPatch(typeof(RandomObject), nameof(RandomObject.Randomize))]
    private static class RandomObjectPatch
    {
        private static void Postfix(RandomObject __instance)
        {
            if (!EmissionTrace.Enabled || EmissionTrace.CurrentSite == null)
                return;
            Guard("RandomObject.Randomize", () =>
            {
                GameObject go = __instance.gameObject;
                EmissionTrace.Randomised("RandomObject", go, go.activeSelf, EmissionTrace.NetworkedUnder(go));
            });
        }
    }

    [HarmonyPatch(typeof(ZNetView), "Awake")]
    private static class AwakePatch
    {
        private static void Postfix(ZNetView __instance)
        {
            if (!EmissionTrace.Enabled || EmissionTrace.CurrentSite == null)
                return;
            Guard("ZNetView.Awake", () => EmissionTrace.Emitted(__instance, __instance.GetZDO()));
        }
    }

    /// <summary>The peer whose destroy request is being handled, for the X row.</summary>
    private static long s_destroySender;

    [HarmonyPatch(typeof(ZDOMan), "RPC_DestroyZDO")]
    private static class RemoteDestroyPatch
    {
        private static void Prefix(long sender) => s_destroySender = sender;
        private static void Postfix() => s_destroySender = 0;
    }

    [HarmonyPatch(typeof(ZDOMan), "HandleDestroyedZDO")]
    private static class HandleDestroyedPatch
    {
        private static void Prefix(ZDOID uid)
        {
            if (!EmissionTrace.Enabled || EmissionTrace.Watched == 0)
                return;
            Guard("HandleDestroyedZDO", () => EmissionTrace.Destroyed(uid,
                s_destroySender != 0 ? "peer " + s_destroySender : "handled locally"));
        }
    }

    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.DestroyZDO))]
    private static class LocalDestroyPatch
    {
        private static void Postfix(ZDO zdo)
        {
            if (!EmissionTrace.Enabled || EmissionTrace.Watched == 0 || zdo == null)
                return;
            Guard("DestroyZDO", () => EmissionTrace.Destroyed(zdo.m_uid, "this server, as owner"));
        }
    }
}
