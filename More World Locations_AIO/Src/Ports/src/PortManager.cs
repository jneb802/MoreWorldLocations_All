using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Newtonsoft.Json;
using ServerSync;
using UnityEngine;

namespace More_World_Locations_AIO;

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocationsIfNeeded))]
public static class ZoneSystem_GenerateLocationsIfNeeded_Patch
{
    [UsedImplicitly]
    private static void Postfix()
    {
        if (PortManager.instance == null) return;
        // run our function just to make sure all locations are registered and ZNet is ready
        PortManager.instance.Invoke(nameof(PortManager.UpdatePortLocations), 10f);
    }
}
public class PortManager : MonoBehaviour
{
    private static CustomSyncedValue<string>? ServerSyncedPortLocations;

    private static PortLocations? locations;
    public static PortManager? instance;

    public void Awake()
    {
        instance = this;
        if (PortInit.configSync is ConfigSync configSync)
        {
            ServerSyncedPortLocations = new(configSync, "ServerPortLocations", "");
            ServerSyncedPortLocations.ValueChanged += () =>
            {
                if (!ZNet.instance || ZNet.instance.IsServer()) return;
                if (string.IsNullOrEmpty(ServerSyncedPortLocations.Value)) return;
                locations = new PortLocations(ServerSyncedPortLocations.Value);
            };
        }
    }

    public static List<PortLocation> GetPortLocations() => locations?.ports ?? new();

    public void UpdatePortLocations()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer() || !ZoneSystem.instance) return;
        Dictionary<Vector2i, ZoneSystem.LocationInstance>.ValueCollection? allLocations = ZoneSystem.instance.GetLocationList();
        List<ZoneSystem.LocationInstance> ports = allLocations.Where(location => location.m_location.m_group == "MWL_Ports").ToList();
        if (ports.Count == 0) return;
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"Registered {ports.Count} ports");
        locations = new PortLocations(ports);
        if (ServerSyncedPortLocations != null) ServerSyncedPortLocations.Value = locations.ToJson();
    }

    [Serializable]
    public class PortLocations
    {
        public List<PortLocation> ports = new();

        public PortLocations(List<ZoneSystem.LocationInstance> ports)
        {
            foreach (ZoneSystem.LocationInstance port in ports)
            {
                this.ports.Add(new PortLocation(port.m_location.m_prefabName, port.m_position, port.m_placed));
            }
        }

        public PortLocations(string json)
        {
            PortLocations? data = JsonConvert.DeserializeObject<PortLocations>(json);
            if (data == null) return;
            ports = data.ports;
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"Received {ports.Count} PortLocations from server");
        }
        
        public PortLocations(){}

        public string ToJson() => JsonConvert.SerializeObject(this);
    }

    [Serializable]
    public class PortLocation
    {
        public string PrefabName;
        public SerializedVector Position;
        public bool IsPlaced;

        public PortLocation(string prefabName, Vector3 position, bool isPlaced)
        {
            PrefabName = prefabName;
            Position = new SerializedVector(position);
            IsPlaced = isPlaced;
        }
    }

    [Serializable]
    public struct SerializedVector
    {
        public float x;
        public float y;
        public float z;

        public SerializedVector(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }
}