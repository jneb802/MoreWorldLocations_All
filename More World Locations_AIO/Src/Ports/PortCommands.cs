using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class PortCommands
{
    [HarmonyPatch(typeof(Terminal), "InitTerminal")]
    public static class ShowShipmentsConsoleCommandPatch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            new Terminal.ConsoleCommand(
                "showshipments",
                "Logs 'shown' in the console.",
                (Terminal.ConsoleEvent)(args =>
                {
                    // let the player know we kicked off the request
                    args.Context?.AddString("Requesting shipments from server...");

                    // this will no-op to local refresh if we're host/SP (per your code)
                    ShipmentManager.ClientRequestFullSync();

                    // Optional: also log to Unity console for debugging
                    Debug.Log("showshipments: ClientRequestFullSync() invoked");
                })
            );
        }
    }
}