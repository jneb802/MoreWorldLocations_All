using System.Collections.Generic;
using UnityEngine;
using static Terminal;

namespace More_World_Locations_AIO;

public static class Commands
{
    private static readonly List<Minimap.PinData> tempPins = new();
    public static void Setup()
    {
        ConsoleCommand shipments = new("mwl_shipments", "list of all shipments", args =>
        {
            foreach (Shipment shipment in ShipmentManager.Shipments.Values)
            {
                foreach (string? log in shipment.LogPrint())
                {
                    Debug.Log(log);
                }
            }
        });

        ConsoleCommand ports = new ConsoleCommand("mwl_ports", "pins port locations on map", args =>
        {
            if (!Minimap.instance) return;
            foreach (Minimap.PinData? pin in tempPins)
            {
                Minimap.instance.RemovePin(pin);
            }
            tempPins.Clear();
            foreach (PortManager.PortLocation? port in PortManager.GetPortLocations())
            {
                Minimap.PinData? pin = Minimap.instance.AddPin(port.Position.ToVector3(), Minimap.PinType.Icon3, port.PrefabName, false, false);
                tempPins.Add(pin);
            }
        });

        ConsoleCommand clearPins = new ConsoleCommand("mwl_clear_ports", "removes port pins from map", args =>
        {
            if (!Minimap.instance) return;
            foreach (Minimap.PinData? pin in tempPins) Minimap.instance.RemovePin(pin);
            tempPins.Clear();
        });

        ConsoleCommand clearKnownPorts = new ConsoleCommand("mwl_clear_known_ports",
            "removes known ports from player save",
            args =>
            {
                if (!Player.m_localPlayer) return;
                Player.m_localPlayer.ResetKnownPorts();
            });
    }
}