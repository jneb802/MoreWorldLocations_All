using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using More_World_Locations_AIO.Managers;
using UnityEngine;

namespace More_World_Locations_AIO;

[HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
public static class WearNTearPatch
{
    private static readonly HashSet<string> ProtectedPortIronPieces = new()
    {
        "iron_wall_1x1",
        "iron_wall_2x2",
        "iron_floor_2x2"
    };

    private static readonly HashSet<int> PortLocationHashes = new()
    {
        "MWL_Port1".GetStableHashCode(),
        "MWL_Port2".GetStableHashCode(),
        "MWL_Port3".GetStableHashCode(),
        "MWL_Port4".GetStableHashCode(),
        "MWL_Port5".GetStableHashCode()
    };

    [UsedImplicitly]
    private static bool Prefix(WearNTear __instance)
    {
        return !IsProtectedPortIronPiece(__instance);
    }

    private static bool IsProtectedPortIronPiece(WearNTear wearNTear)
    {
        if (wearNTear == null) return false;

        string pieceName = Helpers.GetNormalizedName(wearNTear.gameObject.name);
        if (!ProtectedPortIronPieces.Contains(pieceName)) return false;

        for (Transform? current = wearNTear.transform; current != null; current = current.parent)
        {
            LocationProxy proxy = current.GetComponent<LocationProxy>();
            if (proxy == null) continue;

            ZNetView view = proxy.GetComponent<ZNetView>();
            ZDO zdo = view != null ? view.GetZDO() : null;
            if (zdo == null) return false;

            int locationHash = zdo.GetInt(ZDOVars.s_location);
            return PortLocationHashes.Contains(locationHash);
        }

        return false;
    }
}
