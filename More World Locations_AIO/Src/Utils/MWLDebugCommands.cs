using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        new Terminal.ConsoleCommand("mwl_location_diagnostics", "Report MWL location proxy and terrain modifier state. Usage: mwl_location_diagnostics <name>", args =>
        {
            if (args.Length < 2)
            {
                args.Context.AddString("Usage: mwl_location_diagnostics <location_name>");
                return;
            }

            string searchName = args[1];
            string lowerSearchName = searchName.ToLowerInvariant();
            ZoneSystem zoneSystem = ZoneSystem.instance;

            if (zoneSystem == null)
            {
                args.Context.AddString("ZoneSystem not available");
                return;
            }

            List<ZoneSystem.ZoneLocation> zoneLocations = zoneSystem.m_locations
                .Where(location => location.m_prefabName != null && location.m_prefabName.ToLowerInvariant().Contains(lowerSearchName))
                .OrderBy(location => location.m_prefabName)
                .ToList();

            args.Context.AddString($"[MWLDiag] ZoneLocations matching '{searchName}': {zoneLocations.Count}");
            foreach (ZoneSystem.ZoneLocation zoneLocation in zoneLocations)
            {
                args.Context.AddString(
                    $"[MWLDiag] zone name={zoneLocation.m_prefabName} clearArea={zoneLocation.m_clearArea} exteriorRadius={zoneLocation.m_exteriorRadius:0.00} prefabValid={zoneLocation.m_prefab.IsValid} prefabLoaded={zoneLocation.m_prefab.IsLoaded}");
            }

            List<ZoneSystem.LocationInstance> instances = zoneSystem.m_locationInstances.Values
                .Where(instance => instance.m_location?.m_prefabName != null && instance.m_location.m_prefabName.ToLowerInvariant().Contains(lowerSearchName))
                .OrderBy(instance => instance.m_location.m_prefabName)
                .ThenBy(instance => instance.m_position.x)
                .ThenBy(instance => instance.m_position.z)
                .ToList();

            args.Context.AddString($"[MWLDiag] LocationInstances matching '{searchName}': {instances.Count}");
            foreach (ZoneSystem.LocationInstance instance in instances)
            {
                args.Context.AddString(
                    $"[MWLDiag] instance name={instance.m_location.m_prefabName} pos={FormatVector(instance.m_position)} placed={instance.m_placed}");
            }

            List<Vector3> instancePositions = instances
                .Select(instance => instance.m_position)
                .ToList();

            List<LocationProxy> proxies = Object.FindObjectsByType<LocationProxy>(FindObjectsSortMode.None)
                .Where(proxy => proxy != null && MatchesNameOrNearInstance(proxy.gameObject, lowerSearchName, instancePositions, 128f))
                .OrderBy(proxy => proxy.name)
                .ToList();

            args.Context.AddString($"[MWLDiag] Active LocationProxy objects matching '{searchName}' by name or nearby instance: {proxies.Count}");
            foreach (LocationProxy proxy in proxies)
            {
                ReportProxy(args, proxy);
            }

            List<GameObject> sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(IsLoadedSceneObject)
                .Where(gameObject => gameObject.name.ToLowerInvariant().Contains(lowerSearchName))
                .OrderBy(gameObject => gameObject.name)
                .Take(25)
                .ToList();

            args.Context.AddString($"[MWLDiag] Loaded scene objects matching '{searchName}' (first 25): {sceneObjects.Count}");
            foreach (GameObject gameObject in sceneObjects)
            {
                TerrainModifier[] terrainModifiers = gameObject.GetComponentsInChildren<TerrainModifier>(true);
                ZNetView[] zNetViews = gameObject.GetComponentsInChildren<ZNetView>(true);
                args.Context.AddString(
                    $"[MWLDiag] object name={gameObject.name} activeSelf={gameObject.activeSelf} activeInHierarchy={gameObject.activeInHierarchy} pos={FormatVector(gameObject.transform.position)} terrainModifiers={terrainModifiers.Length} activeTerrainModifiers={terrainModifiers.Count(IsActiveTerrainModifier)} znetViews={zNetViews.Length} activeZNetViews={zNetViews.Count(view => view != null && view.isActiveAndEnabled)}");

                foreach (TerrainModifier terrainModifier in terrainModifiers)
                {
                    ReportTerrainModifier(args, terrainModifier);
                }
            }
        }, optionsFetcher: () => ZoneSystem.instance?.m_locations
            .Select(l => l.m_prefabName)
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .OrderBy(n => n)
            .ToList() ?? new List<string>());
    }

    private static void ReportProxy(Terminal.ConsoleEventArgs args, LocationProxy proxy)
    {
        TerrainModifier[] terrainModifiers = proxy.GetComponentsInChildren<TerrainModifier>(true);
        ZNetView[] zNetViews = proxy.GetComponentsInChildren<ZNetView>(true);

        args.Context.AddString(
            $"[MWLDiag] proxy name={proxy.name} activeSelf={proxy.gameObject.activeSelf} activeInHierarchy={proxy.gameObject.activeInHierarchy} pos={FormatVector(proxy.transform.position)} terrainModifiers={terrainModifiers.Length} activeTerrainModifiers={terrainModifiers.Count(IsActiveTerrainModifier)} znetViews={zNetViews.Length} activeZNetViews={zNetViews.Count(view => view != null && view.isActiveAndEnabled)}");

        foreach (TerrainModifier terrainModifier in terrainModifiers)
        {
            ReportTerrainModifier(args, terrainModifier);
        }
    }

    private static void ReportTerrainModifier(Terminal.ConsoleEventArgs args, TerrainModifier terrainModifier)
    {
        if (terrainModifier == null)
        {
            return;
        }

        args.Context.AddString(
            $"[MWLDiag] terrain name={terrainModifier.name} activeSelf={terrainModifier.gameObject.activeSelf} activeInHierarchy={terrainModifier.gameObject.activeInHierarchy} enabled={terrainModifier.enabled} level={terrainModifier.m_level} levelOffset={terrainModifier.m_levelOffset:0.00} levelRadius={terrainModifier.m_levelRadius:0.00} smooth={terrainModifier.m_smooth} smoothRadius={terrainModifier.m_smoothRadius:0.00} pos={FormatVector(terrainModifier.transform.position)}");
    }

    private static bool IsActiveTerrainModifier(TerrainModifier terrainModifier)
    {
        return terrainModifier != null && terrainModifier.enabled && terrainModifier.gameObject.activeInHierarchy;
    }

    private static bool IsLoadedSceneObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        Scene scene = gameObject.scene;
        return scene.IsValid() && scene.isLoaded;
    }

    private static bool MatchesNameOrNearInstance(GameObject gameObject, string lowerSearchName, List<Vector3> instancePositions, float maxDistance)
    {
        if (gameObject.name.ToLowerInvariant().Contains(lowerSearchName))
        {
            return true;
        }

        Vector3 position = gameObject.transform.position;
        return instancePositions.Any(instancePosition => Vector3.Distance(position, instancePosition) <= maxDistance);
    }

    private static string FormatVector(Vector3 vector)
    {
        return $"({vector.x:0.00},{vector.y:0.00},{vector.z:0.00})";
    }
}
