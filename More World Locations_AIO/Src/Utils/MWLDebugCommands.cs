using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public static class MWLCommands
{
    static void Postfix()
    {
        // Teleport to a location instance
        new Terminal.ConsoleCommand("gotolocation", "Teleport to a location. Usage: gotolocation <name> [index]", args =>
        {
            if (ZoneSystem.instance == null)
            {
                args.Context.AddString("ZoneSystem not available");
                return;
            }
            
            if (Player.m_localPlayer == null)
            {
                args.Context.AddString("No local player");
                return;
            }
            
            if (args.Length < 2)
            {
                args.Context.AddString("Usage: gotolocation <location_name> [index]");
                args.Context.AddString("  index: which instance to go to (default: closest)");
                return;
            }
            
            string searchName = args[1].ToLower();
            int targetIndex = -1;
            if (args.Length >= 3 && int.TryParse(args[2], out int parsed))
            {
                targetIndex = parsed;
            }
            
            var playerPos = Player.m_localPlayer.transform.position;
            
            // Find all spawned instances matching this name
            var instances = new List<(ZoneSystem.LocationInstance inst, float distance)>();
            foreach (var kvp in ZoneSystem.instance.m_locationInstances)
            {
                var inst = kvp.Value;
                if (inst.m_location?.m_prefabName?.ToLower().Contains(searchName) == true)
                {
                    float dist = Vector3.Distance(playerPos, inst.m_position);
                    instances.Add((inst, dist));
                }
            }
            
            if (instances.Count == 0)
            {
                bool locationExists = ZoneSystem.instance.m_locations.Any(
                    l => l.m_prefabName.ToLower().Contains(searchName));
                
                if (locationExists)
                    args.Context.AddString($"Location '{args[1]}' exists but no instances have spawned yet");
                else
                    args.Context.AddString($"No location found matching '{args[1]}'");
                return;
            }
            
            // Sort by distance
            instances.Sort((a, b) => a.distance.CompareTo(b.distance));
            
            // Pick target
            ZoneSystem.LocationInstance target;
            if (targetIndex >= 0)
            {
                if (targetIndex >= instances.Count)
                {
                    args.Context.AddString($"Index {targetIndex} out of range. Only {instances.Count} instances exist.");
                    return;
                }
                target = instances[targetIndex].inst;
            }
            else
            {
                target = instances[0].inst;
            }
            
            Vector3 teleportPos = target.m_position + Vector3.up * 2f;
            args.Context.AddString($"Teleporting to {target.m_location.m_prefabName} ({(targetIndex >= 0 ? targetIndex : 0)}/{instances.Count})");
            Player.m_localPlayer.TeleportTo(teleportPos, Player.m_localPlayer.transform.rotation, true);
            
        }, optionsFetcher: () => ZoneSystem.instance?.m_locations
            .Select(l => l.m_prefabName)
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .OrderBy(n => n)
            .ToList() ?? new List<string>());
    }
}