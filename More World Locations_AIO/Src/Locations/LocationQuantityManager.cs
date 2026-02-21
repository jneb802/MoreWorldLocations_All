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
    private static Dictionary<string, string> _biomeByLocation = new();
    private static bool _defaultsLoaded;

    private static readonly string[] BiomeOrder =
    {
        "Meadows", "BlackForest", "Swamp", "Mountains", "Plains",
        "Mistlands", "Ashlands", "Ports", "Traders", "Trainers"
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
        // Always initialize from embedded YAML — single source of truth for defaults
        _quantities = new Dictionary<string, int>();
        _biomeByLocation = new Dictionary<string, string>();
        _defaultsLoaded = false;
        EnsureDefaultsLoaded();

        // Check for old BepInEx orphaned entries
        var orphanedEntries = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries")
            ?.GetValue(config) as Dictionary<ConfigDefinition, string>;

        bool hasOldEntries = orphanedEntries != null &&
            orphanedEntries.Any(e => e.Key.Key == "Spawn Quantity" &&
                OldBepInExSections.Contains(StripSectionPrefix(e.Key.Section)));

        if (BepinexConfigs.UseCustomLocationYAML.Value == PortInit.Toggle.Off)
        {
            // Still clean up old entries even when custom YAML is disabled
            if (hasOldEntries)
                ClearOrphanedEntries(config, orphanedEntries);
            return;
        }

        if (!File.Exists(YamlFilePath))
        {
            if (hasOldEntries)
            {
                // Migrate values before clearing the orphaned entries
                MigrateFromBepInEx(orphanedEntries);
                ClearOrphanedEntries(config, orphanedEntries);
                WriteYamlFile();
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                    "Migrated location quantities from BepInEx config to YAML.");
            }
            else
            {
                string defaultYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_LocationConfigs.yml");
                File.WriteAllText(YamlFilePath, defaultYamlContent);
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                    $"Auto-extracted default location config to: {YamlFilePath}");
            }
        }
        else if (hasOldEntries)
        {
            // YAML already exists but old entries still linger — clean them up
            ClearOrphanedEntries(config, orphanedEntries);
        }

        LoadFromYaml();
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
            $"Location quantity config loaded from: {YamlFilePath}");
    }

    private static void ParseQuantitiesFromContent(string yamlContent)
    {
        if (string.IsNullOrEmpty(yamlContent)) return;

        var deserializer = new DeserializerBuilder().Build();
        var yamlData = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

        if (yamlData == null) return;

        foreach (var kvp in yamlData)
        {
            if (kvp.Key == "version") continue;
            if (kvp.Value is Dictionary<object, object> biomeLocations)
            {
                foreach (var loc in biomeLocations)
                {
                    string locName = loc.Key.ToString();
                    if (int.TryParse(loc.Value?.ToString(), out int qty))
                        _quantities[locName] = qty;
                    _biomeByLocation[locName] = kvp.Key;
                }
            }
        }
    }

    private static void EnsureDefaultsLoaded()
    {
        if (_defaultsLoaded) return;
        _defaultsLoaded = true;
        string defaultYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_LocationConfigs.yml");
        ParseQuantitiesFromContent(defaultYamlContent);
    }

    public static int GetQuantity(string locationName)
    {
        if (!_defaultsLoaded)
            EnsureDefaultsLoaded();
        return _quantities.TryGetValue(locationName, out int qty) ? qty : 0;
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

                        if (!_biomeByLocation.ContainsKey(locName))
                        {
                            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                                $"Unknown location in YAML config: {locName} (in section {kvp.Key}). " +
                                "Check for typos — this entry will be ignored.");
                        }
                    }
                }
            }

            // Check if new locations need to be added to the YAML
            bool hasNewLocations = _biomeByLocation.Keys.Any(name => !yamlLocationNames.Contains(name));

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

            var locations = _biomeByLocation
                .Where(kv => kv.Value == biome)
                .OrderBy(kv => kv.Key)
                .ToList();

            foreach (var loc in locations)
            {
                int qty = _quantities.TryGetValue(loc.Key, out int q) ? q : 0;
                sb.AppendLine($"  {loc.Key}: {qty}");
            }
        }

        File.WriteAllText(YamlFilePath, sb.ToString());
    }
}
