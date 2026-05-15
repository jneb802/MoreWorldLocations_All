using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using More_World_Locations_AIO.Traders;
using UnityEngine;

namespace More_World_Locations_AIO;

public static class KnownPorts
{
    private const string CustomDataKey = "MWL_KnownPorts";
    private const string IconCustomDataKey = "MWL_KnownPortIcons";
    private static SerializedGuid? localKnownPorts; // cache player known ports
    private static SerializedKnownPortIcons? localKnownPortIcons; // cache player discovered port icons
    private static readonly List<Minimap.PinData> PortMapPins = new();
    
    [HarmonyPatch(typeof(Player), nameof(Player.Load))]
    private static class Player_Load_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Player __instance)
        {
            localKnownPorts = new SerializedGuid(__instance, CustomDataKey);
            if (!BepinexConfigs.UseIndividualPortIcons)
            {
                localKnownPortIcons = null;
                ClearMapPins();
                return;
            }

            localKnownPortIcons = new SerializedKnownPortIcons(__instance, IconCustomDataKey);
            RebuildMapPins();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Save))]
    private static class Player_Save_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Player __instance)
        {
            GetKnownPorts(__instance).Save(__instance);
            if (!BepinexConfigs.UseIndividualPortIcons) return;

            GetKnownPortIcons(__instance).Save(__instance);
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
    private static class Minimap_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            RebuildMapPins();
        }
    }

    private class SerializedGuid
    {
        private readonly List<string> GUIDs = new();
        private readonly string? DataKey;

        public SerializedGuid(Player player, string? dataKey)
        {
            DataKey = dataKey;
            if (DataKey == null) return;
            if (!player.m_customData.TryGetValue(DataKey, out string json)) return;
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                Load(json);
            }
            catch
            {
                player.ResetKnownPorts();
            }
        }

        private void Load(string json)
        {
            JToken token = JToken.Parse(json);
            if (token.Type != JTokenType.Array) return;

            // Handles the short-lived object format used while port icons shared this field.
            if (token.First?.Type == JTokenType.Object)
            {
                List<KnownPortData>? data = token.ToObject<List<KnownPortData>>();
                if (data != null) GUIDs.AddRange(data
                    .Where(port => !string.IsNullOrEmpty(port.GUID))
                    .Select(port => port.GUID));
                return;
            }

            List<string>? guids = token.ToObject<List<string>>();
            if (guids == null) return;
            GUIDs.AddRange(guids.Where(guid => !string.IsNullOrEmpty(guid)));
        }

        public void Save(Player player)
        {
            if (DataKey == null) return;
            player.m_customData[DataKey] = ToJson();
        }

        public bool UsesKey(string? dataKey) => DataKey == dataKey;

        private string ToJson() => JsonConvert.SerializeObject(GUIDs);
        
        public bool IsKnownPort(ShipmentManager.PortID portID) => GUIDs.Contains(portID.GUID);

        public void Add(ShipmentManager.PortID portID)
        {
            if (IsKnownPort(portID)) return;
            GUIDs.Add(portID.GUID);
        }
    }

    private class SerializedKnownPortIcons
    {
        private readonly List<KnownPortData> Ports = new();
        private readonly string? DataKey;

        public SerializedKnownPortIcons(Player player, string? dataKey)
        {
            DataKey = dataKey;
            if (DataKey == null) return;
            if (!player.m_customData.TryGetValue(DataKey, out string json)) return;
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                List<KnownPortData>? data = JsonConvert.DeserializeObject<List<KnownPortData>>(json);
                if (data != null) Ports.AddRange(data.Where(port => !string.IsNullOrEmpty(port.GUID)));
            }
            catch
            {
                player.m_customData.Remove(DataKey);
            }
        }

        public void Save(Player player)
        {
            if (DataKey == null) return;
            player.m_customData[DataKey] = JsonConvert.SerializeObject(Ports);
        }

        public bool UsesKey(string? dataKey) => DataKey == dataKey;

        public void AddIcon(ShipmentManager.PortID portID, Vector3 position)
        {
            KnownPortData? existing = Ports.FirstOrDefault(port => port.GUID == portID.GUID);
            if (existing == null)
            {
                Ports.Add(new KnownPortData(portID, position));
                return;
            }

            existing.Name = portID.Name;
            existing.Position = new PortManager.SerializedVector(position);
            existing.HasPosition = true;
        }

        public List<KnownPortData> GetPortsWithPositions() => Ports
            .Where(port => port.HasPosition)
            .ToList();
    }

    private class KnownPortData
    {
        public string GUID = "";
        public string Name = "";
        public PortManager.SerializedVector Position;
        public bool HasPosition;

        public KnownPortData() {}

        public KnownPortData(ShipmentManager.PortID portID, Vector3 position)
        {
            GUID = portID.GUID;
            Name = portID.Name;
            Position = new PortManager.SerializedVector(position);
            HasPosition = true;
        }
    }

    public static bool IsKnownPort(this Player player, ShipmentManager.PortID portID)
    {
        localKnownPorts = GetKnownPorts(player);
        return localKnownPorts.IsKnownPort(portID);
    }

    public static void AddKnownPort(this Player player, ShipmentManager.PortID portID)
    {
        localKnownPorts = GetKnownPorts(player);
        localKnownPorts.Add(portID);
    }

    public static void AddDiscoveredPortIcon(this Player player, ShipmentManager.PortID portID, Vector3 position)
    {
        if (!BepinexConfigs.UseIndividualPortIcons) return;

        localKnownPortIcons = GetKnownPortIcons(player);
        localKnownPortIcons.AddIcon(portID, position);
        RebuildMapPins();
    }

    public static void ResetKnownPorts(this Player player)
    {
        player.m_customData.Remove(CustomDataKey);
        player.m_customData.Remove(IconCustomDataKey);
        localKnownPorts = new SerializedGuid(player, CustomDataKey);
        localKnownPortIcons = new SerializedKnownPortIcons(player, IconCustomDataKey);
        ClearMapPins();
    }

    private static SerializedGuid GetKnownPorts(Player player)
    {
        if (localKnownPorts == null || !localKnownPorts.UsesKey(CustomDataKey))
        {
            localKnownPorts = new SerializedGuid(player, CustomDataKey);
        }
        return localKnownPorts;
    }

    private static SerializedKnownPortIcons GetKnownPortIcons(Player player)
    {
        if (!BepinexConfigs.UseIndividualPortIcons)
        {
            localKnownPortIcons = null;
            ClearMapPins();
            return new SerializedKnownPortIcons(player, null);
        }

        if (localKnownPortIcons == null || !localKnownPortIcons.UsesKey(IconCustomDataKey))
        {
            localKnownPortIcons = new SerializedKnownPortIcons(player, IconCustomDataKey);
        }
        return localKnownPortIcons;
    }

    private static void RebuildMapPins()
    {
        if (Minimap.instance == null || Player.m_localPlayer == null) return;
        if (MinimapTraderIcons.achorSprite == null) return;
        if (!BepinexConfigs.UseIndividualPortIcons)
        {
            ClearMapPins();
            return;
        }

        ClearMapPins();

        foreach (KnownPortData port in GetKnownPortIcons(Player.m_localPlayer).GetPortsWithPositions())
        {
            Minimap.PinData pin = Minimap.instance.AddPin(
                port.Position.ToVector3(),
                Minimap.PinType.None,
                port.Name,
                false,
                false);
            pin.m_icon = MinimapTraderIcons.achorSprite;
            pin.m_doubleSize = true;
            PortMapPins.Add(pin);
        }
    }

    private static void ClearMapPins()
    {
        if (Minimap.instance == null) return;

        foreach (Minimap.PinData pin in PortMapPins.Where(pin => pin != null).ToList())
        {
            Minimap.instance.RemovePin(pin);
        }
        PortMapPins.Clear();
    }
}
