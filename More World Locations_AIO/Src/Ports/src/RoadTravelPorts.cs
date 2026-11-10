using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO;

/// <summary>Supported integration for a separate road travel service. Shipping and sea travel keep their own UI.</summary>
public static class RoadTravelPorts
{
    public const int ApiVersion = 1;
    internal static bool IsPortLocation(ZoneSystem.ZoneLocation location)
    {
        string name = location.m_prefab.Name;
        return location.m_group == "MWL_Ports" || name == "MWL_PortLocation" || name == "MWL_DragonPort1" || name == "MWL_PortStone1";
    }
    private static ZDOMan? manager;
    private static bool seeded;
    private static readonly Dictionary<ZDOID, ZDO> known = new();
    private static void EnsureManager()
    {
        if (manager == ZDOMan.instance) return;
        manager = ZDOMan.instance; known.Clear(); seeded = false;
    }
    private static void Observe(ZDO port)
    {
        int prefab = port.GetPrefab();
        if (prefab != "MWL_PortTrader".GetStableHashCode() && prefab != "PortTrader".GetStableHashCode()) return;
        EnsureManager(); known[port.m_uid] = port;
    }

    public static List<ZDO> GetPorts()
    {
        EnsureManager();
        if (!seeded && manager != null)
        {
            foreach (ZDO port in ShipmentManager.GetPorts()) known[port.m_uid] = port;
            seeded = true;
        }
        List<ZDO> result = new();
        foreach (ZDOID id in new List<ZDOID>(known.Keys))
        {
            ZDO? port = manager?.GetZDO(id);
            if (port == null) known.Remove(id);
            else result.Add(port);
        }
        return result;
    }

    /// <summary>Resolves the shipping trader to its actual port location, not to a nearby road.</summary>
    public static bool TryGetLocation(ZDO port, out Vector3 center)
    {
        center = default;
        if (port == null || ZoneSystem.instance == null || string.IsNullOrEmpty(port.GetString("PortGUID"))) return false;
        int prefab = port.GetPrefab();
        if (prefab != "MWL_PortTrader".GetStableHashCode() && prefab != "PortTrader".GetStableHashCode()) return false;
        ZoneSystem.LocationInstance? selected = null;
        float best = float.MaxValue;
        if (ZNet.instance != null && !ZNet.instance.IsServer())
        {
            // Dedicated clients receive the port locations through MWL's existing synchronization.
            foreach (PortManager.PortLocation location in PortManager.GetPortLocations())
            {
                Vector3 position = new(location.Position.x, location.Position.y, location.Position.z);
                ZoneSystem.ZoneLocation definition = ZoneSystem.instance.GetLocation(location.PrefabName);
                if (definition == null || !IsPortLocation(definition)) continue;
                float distance = global::Utils.DistanceXZ(position, port.GetPosition());
                if (distance > Mathf.Max(10f, definition.m_exteriorRadius) || distance >= best) continue;
                best = distance; center = position;
            }
            return best < float.MaxValue;
        }
        foreach (ZoneSystem.LocationInstance location in ZoneSystem.instance.GetLocationList())
        {
            if (!IsPortLocation(location.m_location)) continue;
            float distance = global::Utils.DistanceXZ(location.m_position, port.GetPosition());
            if (distance > Mathf.Max(10f, location.m_location.m_exteriorRadius) || distance >= best) continue;
            selected = location; best = distance;
        }
        if (selected == null) return false;
        center = selected.Value.m_position;
        return true;
    }

    public static bool CanTravel(Player player) => player != null && player.IsTeleportable(allowAllItems: false);

    [HarmonyPatch(typeof(ZDO), nameof(ZDO.SetPrefab))]
    private static class NewPort
    {
        private static void Postfix(ZDO __instance) => Observe(__instance);
    }
    [HarmonyPatch(typeof(ZDO), nameof(ZDO.Deserialize))]
    private static class ReceivedPort
    {
        private static void Postfix(ZDO __instance) => Observe(__instance);
    }
}
