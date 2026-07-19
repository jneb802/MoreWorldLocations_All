using System.Collections.Generic;
using System.Linq;
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
            if (ShipmentManager.Shipments.Count == 0)
            {
                args.Context.AddString("No shipments found");
                return;
            }
            foreach (Shipment shipment in ShipmentManager.Shipments.Values)
            {
                foreach (string? log in shipment.LogPrint())
                {
                    args.Context.AddString(log);
                }
            }
        });

        ConsoleCommand ports = new ConsoleCommand("mwl_ports", "pins port locations on map", args =>
        {
            if (!Minimap.instance || !ZNet.instance || !ZNet.instance.LocalPlayerIsAdminOrHost()) return;
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

        ConsoleCommand protectedHealthCheck = new ConsoleCommand("mwl_protected_health_check",
            "reports protected MWL location piece health near the player",
            args =>
            {
                if (!Player.m_localPlayer)
                {
                    args.Context.AddString("ERROR: No local player");
                    return;
                }

                float radius = args.Length > 1 && float.TryParse(args[1], out float parsedRadius) ? parsedRadius : 80f;
                Vector3 playerPosition = Player.m_localPlayer.transform.position;
                List<WearNTear> nearbyWorldPieces = WearNTear.GetAllInstances()
                    .Where(wearNTear => wearNTear != null)
                    .Where(wearNTear => Vector3.Distance(playerPosition, wearNTear.transform.position) <= radius)
                    .Where(wearNTear =>
                    {
                        Piece piece = wearNTear.GetComponent<Piece>();
                        return piece == null || !piece.IsPlacedByPlayer();
                    })
                    .ToList();

                List<WearNTear> protectedPieces = nearbyWorldPieces
                    .Where(ProtectedLocationWearNTearPatch.IsProtectedLocationPiece)
                    .ToList();

                (int worldHealth9999, List<string> worldSamples) = CountProtectedHealth(nearbyWorldPieces);
                (int protectedHealth9999, List<string> protectedSamples) = CountProtectedHealth(protectedPieces);

                args.Context.AddString(
                    $"MWL protected health check radius={radius:0.#} worldTotal={nearbyWorldPieces.Count} worldHealth9999={worldHealth9999} worldFailing={nearbyWorldPieces.Count - worldHealth9999} worldSamples={string.Join(",", worldSamples)} protectedTotal={protectedPieces.Count} protectedHealth9999={protectedHealth9999} protectedFailing={protectedPieces.Count - protectedHealth9999} protectedSamples={string.Join(",", protectedSamples)}");
            });
    }

    private static (int health9999, List<string> samples) CountProtectedHealth(List<WearNTear> pieces)
    {
        int health9999 = 0;
        List<string> samples = new List<string>();
        foreach (WearNTear wearNTear in pieces)
        {
            ZNetView view = wearNTear.GetComponent<ZNetView>();
            float health = view != null && view.IsValid()
                ? view.GetZDO().GetFloat(ZDOVars.s_health, wearNTear.m_health)
                : wearNTear.m_health;

            if (Mathf.Approximately(health, 9999f) && Mathf.Approximately(wearNTear.m_health, 9999f))
            {
                health9999++;
            }

            if (samples.Count >= 12) continue;

            string prefab = wearNTear.name.Replace("(Clone)", "").Trim();
            samples.Add($"{prefab}:{health:0.##}/{wearNTear.m_health:0.##}");
        }

        return (health9999, samples);
    }
}
