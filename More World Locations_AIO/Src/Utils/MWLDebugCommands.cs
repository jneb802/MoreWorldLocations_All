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
        
        // Find and pin multiple locations on the map
        new Terminal.ConsoleCommand("findlocations", "Pin multiple locations on map. Usage: findlocations <name1> <name2> ...", args =>
        {
            if (ZoneSystem.instance == null)
            {
                args.Context.AddString("ZoneSystem not available");
                return;
            }
            
            if (Minimap.instance == null)
            {
                args.Context.AddString("Minimap not available");
                return;
            }
            
            if (args.Length < 2)
            {
                args.Context.AddString("Usage: findlocations <location_name1> <location_name2> ...");
                args.Context.AddString("  Use 'findlocations clear' to remove all pins");
                return;
            }
            
            // Handle clear command
            if (args[1].ToLower() == "clear")
            {
                int removed = 0;
                var pinsToRemove = Minimap.instance.m_pins
                    .Where(p => p.m_name.StartsWith("[MWL]"))
                    .ToList();
                foreach (var pin in pinsToRemove)
                {
                    Minimap.instance.RemovePin(pin);
                    removed++;
                }
                args.Context.AddString($"Removed {removed} location pins");
                return;
            }
            
            // Collect all search terms
            var searchNames = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                searchNames.Add(args[i].ToLower());
            }
            
            // Track counts per location type
            var counts = new Dictionary<string, int>();
            int totalPins = 0;
            
            // Find all matching instances and add pins
            foreach (var kvp in ZoneSystem.instance.m_locationInstances)
            {
                var inst = kvp.Value;
                if (inst.m_location?.m_prefabName == null) continue;
                
                string prefabName = inst.m_location.m_prefabName;
                string lowerName = prefabName.ToLower();
                
                // Check if this location matches any search term
                bool matches = searchNames.Any(search => lowerName.Contains(search));
                if (!matches) continue;
                
                // Add pin to map
                Minimap.instance.AddPin(
                    inst.m_position,
                    Minimap.PinType.Icon3,
                    $"[MWL] {prefabName}",
                    save: false,
                    isChecked: false
                );
                
                // Track count
                if (!counts.ContainsKey(prefabName))
                    counts[prefabName] = 0;
                counts[prefabName]++;
                totalPins++;
            }
            
            // Report results
            if (totalPins == 0)
            {
                args.Context.AddString($"No spawned instances found matching: {string.Join(", ", searchNames)}");
                return;
            }
            
            args.Context.AddString($"Added {totalPins} pins:");
            foreach (var kvp in counts.OrderBy(k => k.Key))
            {
                args.Context.AddString($"  {kvp.Key}: {kvp.Value}");
            }
            args.Context.AddString("Use 'findlocations clear' to remove pins");
            
        }, optionsFetcher: () => ZoneSystem.instance?.m_locations
            .Select(l => l.m_prefabName)
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .OrderBy(n => n)
            .ToList() ?? new List<string>());
    }
}