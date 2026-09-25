using System.Collections.Generic;
using Newtonsoft.Json;

namespace More_World_Locations_AIO;

public static class KnownPorts
{
    private const string CustomDataKey = "MWL_KnownPorts";
    private class SerializedGuid
    {
        private readonly List<string> GUIDs = new();
        public SerializedGuid(Player player)
        {
            if (!player.m_customData.TryGetValue(CustomDataKey, out string json)) return;
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                List<string>? data = JsonConvert.DeserializeObject<List<string>>(json);
                if (data is null)
                {
                    player.ResetKnownPorts();
                }
                else
                {
                    GUIDs = data;
                }
            }
            catch
            {
                player.ResetKnownPorts();
            }
        }

        public void Save(Player player)
        {
            player.m_customData[CustomDataKey] = ToJson();
        }
        private string ToJson() => JsonConvert.SerializeObject(GUIDs);
        
        public bool IsKnownPort(ShipmentManager.PortID portID) => GUIDs.Contains(portID.GUID);

        public void Add(ShipmentManager.PortID portID)
        {
            if (IsKnownPort(portID)) return;
            GUIDs.Add(portID.GUID);
        }
    }

    public static bool IsKnownPort(this Player player, ShipmentManager.PortID portID)
    {
        SerializedGuid knownPorts = new SerializedGuid(player);
        return knownPorts.IsKnownPort(portID);
    }

    public static void AddKnownPort(this Player player, ShipmentManager.PortID portID)
    {
        SerializedGuid knownPorts = new SerializedGuid(player);
        knownPorts.Add(portID);
        knownPorts.Save(player);
    }

    public static void ResetKnownPorts(this Player player) => player.m_customData.Remove(CustomDataKey);
}
