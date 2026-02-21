using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Utils;
using YamlDotNet.Serialization;

namespace More_World_Locations_AIO;

public static class LocationQuantityManager
{
    private const int CurrentVersion = 1;

    private static readonly string YamlFilePath = Path.Combine(
        BepInEx.Paths.ConfigPath,
        "warpalicious.More_World_Locations_LocationConfigs.yml");

    private static Dictionary<string, int> _quantities = new();

    private static readonly string[] BiomeOrder =
    {
        "Meadows", "BlackForest", "Swamp", "Mountains", "Plains",
        "Mistlands", "Ashlands", "Ports", "Traders", "Trainers"
    };

    private static readonly Dictionary<string, (string Biome, int DefaultQuantity)> LocationDefaults = new()
    {
        // Meadows
        { "MWL_DeerShrine1", ("Meadows", 10) },
        { "MWL_DeerShrine2", ("Meadows", 10) },
        { "MWL_MaypoleHut1", ("Meadows", 10) },
        { "MWL_MeadowsBarn1", ("Meadows", 5) },
        { "MWL_MeadowsFarm1", ("Meadows", 10) },
        { "MWL_MeadowsHouse2", ("Meadows", 20) },
        { "MWL_MeadowsLighthouse1", ("Meadows", 10) },
        { "MWL_MeadowsRuin1", ("Meadows", 5) },
        { "MWL_MeadowsSawmill1", ("Meadows", 10) },
        { "MWL_MeadowsTavern1", ("Meadows", 10) },
        { "MWL_MeadowsTomb4", ("Meadows", 10) },
        { "MWL_MeadowsTower1", ("Meadows", 15) },
        { "MWL_MeadowsWall1", ("Meadows", 10) },
        { "MWL_OakHut1", ("Meadows", 15) },
        { "MWL_Ruins1", ("Meadows", 5) },
        { "MWL_Ruins2", ("Meadows", 10) },
        { "MWL_Ruins3", ("Meadows", 25) },
        { "MWL_Ruins6", ("Meadows", 5) },
        { "MWL_Ruins7", ("Meadows", 2) },
        { "MWL_Ruins8", ("Meadows", 5) },
        { "MWL_RuinsArena1", ("Meadows", 25) },
        { "MWL_RuinsArena3", ("Meadows", 25) },
        { "MWL_RuinsChurch1", ("Meadows", 25) },
        { "MWL_RuinsWell1", ("Meadows", 5) },
        { "MWL_SmallHouse1", ("Meadows", 20) },

        // BlackForest
        { "MWL_CoastTower1", ("BlackForest", 15) },
        { "MWL_ForestCamp1", ("BlackForest", 20) },
        { "MWL_ForestForge1", ("BlackForest", 20) },
        { "MWL_ForestForge2", ("BlackForest", 20) },
        { "MWL_ForestGreatHouse2", ("BlackForest", 20) },
        { "MWL_ForestGrove1", ("BlackForest", 15) },
        { "MWL_ForestHouse2", ("BlackForest", 20) },
        { "MWL_ForestPillar1", ("BlackForest", 15) },
        { "MWL_ForestRuin1", ("BlackForest", 20) },
        { "MWL_ForestRuin2", ("BlackForest", 15) },
        { "MWL_ForestRuin3", ("BlackForest", 15) },
        { "MWL_ForestSkull1", ("BlackForest", 15) },
        { "MWL_ForestTower2", ("BlackForest", 20) },
        { "MWL_ForestTower3", ("BlackForest", 20) },
        { "MWL_ForestTower4", ("BlackForest", 15) },
        { "MWL_ForestTower5", ("BlackForest", 15) },
        { "MWL_GuardTower1", ("BlackForest", 5) },
        { "MWL_MassGrave1", ("BlackForest", 15) },
        { "MWL_RockShrine1", ("BlackForest", 15) },
        { "MWL_RootRuins1", ("BlackForest", 15) },
        { "MWL_RootsTower1", ("BlackForest", 20) },
        { "MWL_RootsTower2", ("BlackForest", 10) },
        { "MWL_RuinedRootTower5", ("BlackForest", 10) },
        { "MWL_RuinedTower1", ("BlackForest", 15) },
        { "MWL_RuinsArena2", ("BlackForest", 5) },
        { "MWL_RuinsCastle1", ("BlackForest", 15) },
        { "MWL_RuinsCastle3", ("BlackForest", 5) },
        { "MWL_RuinsTower3", ("BlackForest", 15) },
        { "MWL_RuinsTower6", ("BlackForest", 15) },
        { "MWL_RuinsTower8", ("BlackForest", 10) },
        { "MWL_StoneFormation1", ("BlackForest", 15) },
        { "MWL_StoneOutlook1", ("BlackForest", 10) },
        { "MWL_Tavern1", ("BlackForest", 15) },
        { "MWL_WoodTower1", ("BlackForest", 10) },
        { "MWL_WoodTower2", ("BlackForest", 10) },
        { "MWL_WoodTower3", ("BlackForest", 10) },

        // Swamp
        { "MWL_AbandonedHouse1", ("Swamp", 15) },
        { "MWL_Belmont1", ("Swamp", 5) },
        { "MWL_CastleCorner1", ("Swamp", 15) },
        { "MWL_FortBakkarhalt1", ("Swamp", 5) },
        { "MWL_GuckPit1", ("Swamp", 15) },
        { "MWL_Shipyard1", ("Swamp", 20) },
        { "MWL_StoneCircle1", ("Swamp", 10) },
        { "MWL_SwampAltar1", ("Swamp", 15) },
        { "MWL_SwampAltar2", ("Swamp", 10) },
        { "MWL_SwampAltar3", ("Swamp", 10) },
        { "MWL_SwampAltar4", ("Swamp", 10) },
        { "MWL_SwampBrokenTower1", ("Swamp", 15) },
        { "MWL_SwampBrokenTower3", ("Swamp", 10) },
        { "MWL_SwampCastle2", ("Swamp", 10) },
        { "MWL_SwampCourtyard1", ("Swamp", 5) },
        { "MWL_SwampGrave1", ("Swamp", 25) },
        { "MWL_SwampHouse1", ("Swamp", 20) },
        { "MWL_SwampRuin1", ("Swamp", 25) },
        { "MWL_SwampTemple1", ("Swamp", 10) },
        { "MWL_SwampTower1", ("Swamp", 20) },
        { "MWL_SwampTower2", ("Swamp", 25) },
        { "MWL_SwampTower3", ("Swamp", 25) },
        { "MWL_SwampWell1", ("Swamp", 20) },
        { "MWL_Treehouse1", ("Swamp", 20) },
        { "MWL_TreeTowers1", ("Swamp", 20) },

        // Mountains
        { "MWL_MountainCultShrine1", ("Mountains", 15) },
        { "MWL_MountainDvergrShrine1", ("Mountains", 15) },
        { "MWL_MountainDvergrShrine2", ("Mountains", 15) },
        { "MWL_MountainOverlook1", ("Mountains", 15) },
        { "MWL_MountainShrine1", ("Mountains", 20) },
        { "MWL_RuinsChurch2", ("Mountains", 15) },
        { "MWL_StoneAltar1", ("Mountains", 15) },
        { "MWL_StoneCastle1", ("Mountains", 5) },
        { "MWL_StoneFort1", ("Mountains", 10) },
        { "MWL_StoneHall1", ("Mountains", 10) },
        { "MWL_StoneTavern1", ("Mountains", 10) },
        { "MWL_StoneTower1", ("Mountains", 10) },
        { "MWL_StoneTower2", ("Mountains", 10) },
        { "MWL_TempleShrine1", ("Mountains", 5) },
        { "MWL_WoodBarn1", ("Mountains", 10) },
        { "MWL_WoodFarm1", ("Mountains", 10) },
        { "MWL_WoodHouse1", ("Mountains", 5) },

        // Plains
        { "MWL_FulingRock1", ("Plains", 15) },
        { "MWL_FulingTemple1", ("Plains", 15) },
        { "MWL_FulingTemple2", ("Plains", 10) },
        { "MWL_FulingTemple3", ("Plains", 20) },
        { "MWL_FulingTemple4", ("Plains", 10) },
        { "MWL_FulingTempleBroken1", ("Plains", 10) },
        { "MWL_FulingTower1", ("Plains", 20) },
        { "MWL_FulingVillage1", ("Plains", 15) },
        { "MWL_FulingVillage2", ("Plains", 15) },
        { "MWL_FulingWall1", ("Plains", 20) },
        { "MWL_GoblinFort1", ("Plains", 10) },
        { "MWL_PlainsPillar1", ("Plains", 15) },
        { "MWL_RockGarden1", ("Plains", 15) },

        // Mistlands
        { "MWL_AncientShrine1", ("Mistlands", 15) },
        { "MWL_DvergrEitrSingularity1", ("Mistlands", 25) },
        { "MWL_DvergrHouse1", ("Mistlands", 20) },
        { "MWL_DvergrHouseWood1", ("Mistlands", 10) },
        { "MWL_DvergrHouseWood2", ("Mistlands", 10) },
        { "MWL_DvergrKnowledgeExtractor1", ("Mistlands", 15) },
        { "MWL_MarbleCage1", ("Mistlands", 10) },
        { "MWL_MarbleCliffAltar1", ("Mistlands", 10) },
        { "MWL_MarbleHome1", ("Mistlands", 10) },
        { "MWL_MarbleHome2", ("Mistlands", 10) },
        { "MWL_MarbleJail1", ("Mistlands", 10) },
        { "MWL_MistFort2", ("Mistlands", 20) },
        { "MWL_MistHut1", ("Mistlands", 25) },
        { "MWL_Misthut2", ("Mistlands", 15) },
        { "MWL_MistPond1", ("Mistlands", 10) },
        { "MWL_MistShrine1", ("Mistlands", 15) },
        { "MWL_MistTower1", ("Mistlands", 20) },
        { "MWL_MistTower2", ("Mistlands", 25) },
        { "MWL_MistWall1", ("Mistlands", 15) },
        { "MWL_MistWorkshop1", ("Mistlands", 25) },
        { "MWL_SecretRoom1", ("Mistlands", 15) },

        // Ashlands
        { "MWL_AshlandsFort1", ("Ashlands", 5) },
        { "MWL_AshlandsFort2", ("Ashlands", 5) },
        { "MWL_AshlandsFort3", ("Ashlands", 5) },

        // Ports
        { "MWL_Port1", ("Ports", 5) },
        { "MWL_Port2", ("Ports", 5) },
        { "MWL_Port3", ("Ports", 5) },
        { "MWL_Port4", ("Ports", 5) },

        // Traders
        { "MWL_BlackForestBlacksmith1", ("Traders", 5) },
        { "MWL_BlackForestBlacksmith2", ("Traders", 5) },
        { "MWL_MistlandsBlacksmith1", ("Traders", 5) },
        { "MWL_MountainsBlacksmith1", ("Traders", 5) },
        { "MWL_OceanTavern1", ("Traders", 5) },
        { "MWL_PlainsCamp1", ("Traders", 5) },
        { "MWL_PlainsTavern1", ("Traders", 5) },

        // Trainers
        { "MWL_MeadowsTrainer1", ("Trainers", 5) },
        { "MWL_MistTrainer1", ("Trainers", 5) },
        { "MWL_PlainsTrainer1", ("Trainers", 5) },
        { "MWL_SwampTrainer1", ("Trainers", 5) },
    };

    // Exact BepInEx section names from the old GenerateConfigs() method.
    // Only these sections should be cleaned up during migration.
    private static readonly HashSet<string> OldBepInExSections = new()
    {
        // Sections that used MWL_ prefix
        "MWL_Ruins1", "MWL_Ruins2", "MWL_Ruins3", "MWL_Ruins6", "MWL_Ruins7", "MWL_Ruins8",
        "MWL_RuinsArena1", "MWL_RuinsArena3", "MWL_RuinsChurch1", "MWL_RuinsWell1",
        "MWL_DeerShrine1", "MWL_DeerShrine2", "MWL_MeadowsBarn1", "MWL_MeadowsHouse2",
        "MWL_MeadowsRuin1", "MWL_MeadowsTomb4", "MWL_MeadowsTower1", "MWL_OakHut1",
        "MWL_SmallHouse1", "MWL_MeadowsFarm1", "MWL_MeadowsLighthouse1", "MWL_MeadowsSawmill1",
        "MWL_MeadowsWall1", "MWL_MeadowsTavern1",
        "MWL_RuinsArena2", "MWL_RuinsCastle1", "MWL_RuinsCastle3", "MWL_RuinsTower3",
        "MWL_RuinsTower8", "MWL_Tavern1", "MWL_WoodTower1", "MWL_WoodTower2", "MWL_WoodTower3",
        "MWL_RuinsTower6",
        "MWL_AshlandsFort1", "MWL_AshlandsFort2", "MWL_AshlandsFort3",
        "MWL_PlainsTavern1", "MWL_PlainsCamp1", "MWL_BlackForestBlacksmith1",
        "MWL_BlackForestBlacksmith2", "MWL_MountainsBlacksmith1", "MWL_MistlandsBlacksmith1",
        "MWL_OceanTavern1",
        "MWL_MeadowsTrainer1", "MWL_SwampTrainer1", "MWL_PlainsTrainer1", "MWL_MistTrainer1",

        // Sections that did NOT use MWL_ prefix
        "MaypoleHut1",
        "ForestForge1", "ForestForge2", "ForestGreatHouse2", "ForestHouse2", "ForestRuin1",
        "ForestTower2", "ForestTower3", "MassGrave1", "StoneFormation1", "GuardTower1",
        "RootRuins1", "RootsTower1", "RootsTower2", "RuinedRootTower5", "StoneOutlook1",
        "ForestRuin2", "ForestRuin3", "ForestSkull1", "ForestTower4", "ForestTower5",
        "ForestPillar1", "CoastTower1", "ForestGrove1", "RockShrine1",
        "GuckPit1", "SwampAltar1", "SwampAltar2", "SwampAltar3", "SwampAltar4", "SwampCastle2",
        "SwampGrave1", "SwampHouse1", "SwampRuin1", "SwampTower1", "SwampTower2", "SwampTower3",
        "SwampWell1", "AbandonedHouse1", "Treehouse1", "Shipyard1", "FortBakkarhalt1", "Belmont1",
        "SwampCourtyard1", "SwampBrokenTower1", "SwampBrokenTower3", "StoneCircle1", "SwampTemple1",
        "StoneCastle1", "StoneFort1", "StoneHall1", "StoneTavern1", "StoneTower1", "StoneTower2",
        "WoodBarn1", "WoodFarm1", "WoodHouse1", "TempleShrine1", "StoneAltar1",
        "GoblinFort1", "FulingRock1", "FulingVillage1", "FulingVillage2", "PlainsPillar1",
        "FulingTemple1", "FulingTemple2", "FulingTemple3", "FulingTempleBroken1", "FulingTemple4",
        "FulingWall1", "FulingTower1", "RockGarden1",
        "MistFort2", "SecretRoom1", "MistWorkshop1", "MistTower1", "MistWall1", "MistTower2",
        "MistHut1", "DvergrEitrSingularity1", "DvergrHouse1", "DvergrHouseWood1", "DvergrHouseWood2",
        "MarbleJail1", "MarbleHome1", "MarbleHome2", "MarbleCliffAltar1", "MistPond1", "MarbleCage1",
        "DvergrKnowledgeExtractor1", "AncientShrine1", "MistShrine1",
        "CastleCorner1", "ForestCamp1", "Misthut2", "MountainDvergrShrine1", "MountainDvergrShrine2",
        "MountainOverlook1", "MountainCultShrine1", "RuinsChurch2", "MountainShrine1",
        "RuinedTower1", "TreeTowers1",
        "Port1", "Port2", "Port3", "Port4",
    };

    /// <summary>
    /// Strips the numbered prefix added by Jotunn's BindConfigInOrder
    /// (e.g. "1 - MWL_Ruins1" -> "MWL_Ruins1", "100 - FulingTemple2" -> "FulingTemple2").
    /// </summary>
    private static string StripSectionPrefix(string section)
    {
        return Regex.Replace(section, @"^\d+\s*-\s*", "");
    }

    public static void LoadOrMigrateConfigs(ConfigFile config)
    {
        // Always start with defaults
        _quantities = LocationDefaults.ToDictionary(kv => kv.Key, kv => kv.Value.DefaultQuantity);

        if (BepinexConfigs.UseCustomLocationYAML.Value == PortInit.Toggle.Off)
            return;

        string defaultYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_LocationConfigs.yml");

        if (!File.Exists(YamlFilePath))
        {
            // Check for old BepInEx entries to migrate
            var orphanedEntries = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries")
                ?.GetValue(config) as Dictionary<ConfigDefinition, string>;

            bool hasOldEntries = orphanedEntries != null &&
                orphanedEntries.Any(e => e.Key.Key == "Spawn Quantity" &&
                    OldBepInExSections.Contains(StripSectionPrefix(e.Key.Section)));

            if (hasOldEntries)
            {
                MigrateFromBepInEx(orphanedEntries);
                ClearOrphanedEntries(config, orphanedEntries);
                WriteYamlFile();
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                    "Migrated location quantities from BepInEx config to YAML.");
            }
            else
            {
                File.WriteAllText(YamlFilePath, defaultYamlContent);
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                    $"Auto-extracted default location config to: {YamlFilePath}");
            }
        }

        LoadFromYaml();
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
            $"Location quantity config loaded from: {YamlFilePath}");
    }

    public static int GetQuantity(string locationName)
    {
        if (_quantities.TryGetValue(locationName, out int qty))
            return qty;

        if (LocationDefaults.TryGetValue(locationName, out var def))
            return def.DefaultQuantity;

        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
            $"No quantity config found for location: {locationName}");
        return 0;
    }

    private static void MigrateFromBepInEx(Dictionary<ConfigDefinition, string> orphanedEntries)
    {
        if (orphanedEntries == null || orphanedEntries.Count == 0)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                "No orphaned BepInEx entries found. Using default quantities.");
            return;
        }

        int migratedCount = 0;
        foreach (var entry in orphanedEntries)
        {
            if (entry.Key.Key != "Spawn Quantity") continue;

            string sectionName = StripSectionPrefix(entry.Key.Section);
            string locationName = sectionName.StartsWith("MWL_") ? sectionName : "MWL_" + sectionName;

            if (_quantities.ContainsKey(locationName) && int.TryParse(entry.Value, out int value))
            {
                _quantities[locationName] = value;
                migratedCount++;
            }
        }

        if (migratedCount > 0)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                $"Migrated {migratedCount} location quantities from BepInEx config to YAML.");
        }
    }

    private static void ClearOrphanedEntries(ConfigFile config, Dictionary<ConfigDefinition, string> orphanedEntries)
    {
        if (orphanedEntries == null) return;

        var toRemove = orphanedEntries.Keys
            .Where(k => OldBepInExSections.Contains(StripSectionPrefix(k.Section)))
            .ToList();

        foreach (var key in toRemove)
            orphanedEntries.Remove(key);

        if (toRemove.Count > 0)
        {
            config.Save();
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                $"Cleared {toRemove.Count} orphaned BepInEx config entries.");
        }
    }

    private static void LoadFromYaml()
    {
        try
        {
            string yamlContent = File.ReadAllText(YamlFilePath);
            var deserializer = new DeserializerBuilder().Build();
            var yamlData = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

            if (yamlData == null) return;

            // Check version
            int version = 1;
            if (yamlData.TryGetValue("version", out object versionObj))
                int.TryParse(versionObj?.ToString(), out version);

            if (version > CurrentVersion)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                    $"YAML config version {version} is newer than expected {CurrentVersion}. " +
                    "Loading what we can — some fields may be ignored.");
            }

            // Future: if (version < CurrentVersion) RunMigrations(version, yamlData);

            // Build a flat set of all location names found in the YAML
            var yamlLocationNames = new HashSet<string>();

            // Override defaults with user values
            foreach (var kvp in yamlData)
            {
                if (kvp.Key == "version") continue;
                if (kvp.Value is Dictionary<object, object> biomeLocations)
                {
                    foreach (var loc in biomeLocations)
                    {
                        string locName = loc.Key.ToString();
                        yamlLocationNames.Add(locName);

                        if (int.TryParse(loc.Value?.ToString(), out int qty))
                        {
                            _quantities[locName] = qty;
                        }

                        if (!LocationDefaults.ContainsKey(locName))
                        {
                            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                                $"Unknown location in YAML config: {locName} (in section {kvp.Key}). " +
                                "Check for typos — this entry will be ignored.");
                        }
                    }
                }
            }

            // Check if new locations need to be added to the YAML
            bool hasNewLocations = LocationDefaults.Keys.Any(name => !yamlLocationNames.Contains(name));

            if (hasNewLocations)
            {
                WriteYamlFile();
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                    "Updated YAML config with new location entries.");
            }
        }
        catch (Exception ex)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"Error loading YAML config: {ex.Message}. Using defaults.");
        }
    }

    private static void WriteYamlFile()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# More World Locations AIO - Location Quantity Configuration");
        sb.AppendLine("# Quantity = number of this location the game will attempt to place during world generation");
        sb.AppendLine("# Set to 0 to disable a location");
        sb.AppendLine();
        sb.AppendLine("version: 1");

        foreach (var biome in BiomeOrder)
        {
            sb.AppendLine();
            sb.AppendLine($"{biome}:");

            var locations = LocationDefaults
                .Where(kv => kv.Value.Biome == biome)
                .OrderBy(kv => kv.Key)
                .ToList();

            foreach (var loc in locations)
            {
                int qty = _quantities.TryGetValue(loc.Key, out int q) ? q : loc.Value.DefaultQuantity;
                sb.AppendLine($"  {loc.Key}: {qty}");
            }
        }

        File.WriteAllText(YamlFilePath, sb.ToString());
    }
}
