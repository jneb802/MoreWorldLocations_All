using System;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

[HarmonyPatch(typeof(Terminal), "InitTerminal")]
public static class ShowPortGuiConsolePatch
{
    [HarmonyPostfix]
    static void Postfix()
    {
        // Add showshipments command to toggle the shipments GUI
        new Terminal.ConsoleCommand("showport", "toggles the port GUI on/off", (Terminal.ConsoleEvent)(args =>
        {
            try
            {
                PortUI.ToggleShipmentsUI(args.Context);
            }
            catch (Exception ex)
            {
                args.Context?.AddString($"Error toggling shipments UI: {ex.Message}");
                Debug.LogError($"Error in showport command: {ex}");
            }
        }));
    }
    }
