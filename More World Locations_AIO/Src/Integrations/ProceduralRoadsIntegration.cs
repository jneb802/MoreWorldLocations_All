using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using Jotunn.Configs;

namespace More_World_Locations_AIO;

public static class ProceduralRoadsIntegration
{
    private const string ProceduralRoadsGuid = "warpalicious.ProceduralRoads";
    private const string ProceduralRoadsApiTypeName = "ProceduralRoads.ProceduralRoadsAPI";

    public static void RegisterRoadLocations()
    {
        if (BepinexConfigs.EnableProceduralRoadsIntegration.Value == PortInit.Toggle.Off)
            return;

        MethodInfo? registerLocation = GetRegisterLocationMethod();
        if (registerLocation == null)
            return;

        HashSet<string> registeredNames = new HashSet<string>(StringComparer.Ordinal);
        int registeredCount = 0;

        registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Meadows, 65);
        registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.BlackForest, 65);
        registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Swamp, 65);
        registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Mountains, 65);
        registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Plains, 65);
        registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Mistlands, 65);
        registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Ashlands, 65);
        registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Dungeons, 50, true);

        if (BepinexConfigs.EnableTraders.Value != PortInit.Toggle.Off)
            registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Traders, 65, true);

        if (BepinexConfigs.EnableTrainers.Value != PortInit.Toggle.Off)
            registeredCount += RegisterPack(registerLocation, registeredNames, LocationDefinitions.Trainers, 55, true);

        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
            $"Registered {registeredCount} More World Locations landmarks with Procedural Roads.");
    }

    private static MethodInfo? GetRegisterLocationMethod()
    {
        if (!Chainloader.PluginInfos.TryGetValue(ProceduralRoadsGuid, out BepInEx.PluginInfo pluginInfo))
            return null;

        Assembly assembly = pluginInfo.Instance.GetType().Assembly;
        Type? apiType = assembly.GetType(ProceduralRoadsApiTypeName);
        MethodInfo? method = apiType?.GetMethod(
            "RegisterLocation",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(int) },
            null);

        if (method == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                "Procedural Roads is installed, but its location registration API was not found.");
        }

        return method;
    }

    private static int RegisterPack(
        MethodInfo registerLocation,
        HashSet<string> registeredNames,
        IEnumerable<MWLLocation> locations,
        int defaultPriority,
        bool includeEntirePack = false)
    {
        int count = 0;
        foreach (MWLLocation location in locations)
        {
            if (ContainsAny(location.Name, "Ocean"))
                continue;

            if (!includeEntirePack && !IsRoadLandmark(location))
                continue;

            if (!registeredNames.Add(location.Name))
                continue;

            int priority = GetPriority(location, defaultPriority);
            registerLocation.Invoke(null, new object[] { location.Name, priority });
            count++;
        }

        return count;
    }

    private static bool IsRoadLandmark(MWLLocation location)
    {
        string name = location.Name;
        string group = location.Config?.Group ?? string.Empty;

        return ContainsAny(name, "Altar", "Blacksmith", "Camp", "Castle", "Farm", "Forge", "Fort", "GreatHouse",
                "Lighthouse", "Sawmill", "Shrine", "Tavern", "Temple", "Tower", "Trainer",
                "Village", "Waystone")
            || ContainsAny(group, "Camp", "Fort", "Forge", "House_large", "Large",
                "MWL_Trader", "Shrine", "Structure_large", "Temple", "Tower", "Village");
    }

    private static int GetPriority(MWLLocation location, int defaultPriority)
    {
        string name = location.Name;
        string group = location.Config?.Group ?? string.Empty;

        if (ContainsAny(group, "MWL_Trader") || ContainsAny(name, "Blacksmith", "Tavern"))
            return 65;

        if (ContainsAny(name, "Trainer"))
            return 55;

        if (ContainsAny(name, "Shrine", "Waystone", "Temple", "Fort", "Village") ||
            ContainsAny(group, "Shrine", "Temple", "Fort", "Village"))
            return 55;

        if (ContainsAny(name, "Castle", "GreatHouse", "Lighthouse") ||
            ContainsAny(group, "House_large", "Structure_large", "Large"))
            return 50;

        if (ContainsAny(name, "Camp", "Farm", "Forge", "Sawmill", "Tower") ||
            ContainsAny(group, "Camp", "Forge", "Tower"))
            return 45;

        return defaultPriority;
    }

    private static bool ContainsAny(string source, params string[] values)
    {
        foreach (string value in values)
        {
            if (source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }
}
