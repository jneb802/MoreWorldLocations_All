using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Managers;
using ServerSync;
using UnityEngine;
using Common;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

namespace BlackForest_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BlackForest_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "MWL_Blackforest_Pack_1";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        
        public static AssetBundle assetBundle;
        public static string bundleName = "blackforestPack1";

        public static void LoadAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources(
                bundleName,
                Assembly.GetExecutingAssembly()
            );
            if (assetBundle == null)
            {
                WarpLogger.Logger.LogError("Failed to load asset bundle with name: " + bundleName);
            }
        }

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            bool saveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet =
                false; // This and the variable above are used to prevent the config from saving on startup for each config entry. This is speeds up the startup process.

            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);


            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWL_RuinsArena2_Quantity_Config = config("2 - MWL_RuinsArena2", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsArena2_CreatureYaml_Config = config("2 - MWL_RuinsArena2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsArena2_CreatureList_Config = config("2 - MWL_RuinsArena2", "Name of Creature List", "BlackforestCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsArena2_LootYaml_Config = config("2 - MWL_RuinsArena2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsArena2_LootList_Config = config("2 - MWL_RuinsArena2", "Name of Loot List", "BlackforestLoot3",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_RuinsCastle1_Quantity_Config = config("3 - MWL_RuinsCastle1", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsCastle1_CreatureYaml_Config = config("3 - MWL_RuinsCastle1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsCastle1_CreatureList_Config = config("3 - MWL_RuinsCastle1", "Name of Creature List", "BlackforestCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsCastle1_LootYaml_Config = config("3 - MWL_RuinsCastle1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsCastle1_LootList_Config = config("3 - MWL_RuinsCastle1", "Name of Loot List", "BlackforestLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_RuinsCastle3_Quantity_Config = config("4 - MWL_RuinsCastle3", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsCastle3_CreatureYaml_Config = config("4 - MWL_RuinsCastle3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsCastle3_CreatureList_Config = config("4 - MWL_RuinsCastle3", "Name of Creature List", "BlackforestCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsCastle3_LootYaml_Config = config("4 - MWL_RuinsCastle3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsCastle3_LootList_Config = config("4 - MWL_RuinsCastle3", "Name of Loot List", "BlackforestLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_RuinsTower3_Quantity_Config = config("5 - MWL_RuinsTower3", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsTower3_CreatureYaml_Config = config("5 - MWL_RuinsTower3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsTower3_CreatureList_Config = config("5 - MWL_RuinsTower3", "Name of Creature List", "BlackforestCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsTower3_LootYaml_Config = config("5 - MWL_RuinsTower3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsTower3_LootList_Config = config("5 - MWL_RuinsTower3", "Name of Loot List", "BlackforestLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_RuinsTower8_Quantity_Config = config("6 - MWL_RuinsTower8", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsTower8_CreatureYaml_Config = config("6 - MWL_RuinsTower8", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsTower8_CreatureList_Config = config("6 - MWL_RuinsTower8", "Name of Creature List", "BlackforestCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsTower8_LootYaml_Config = config("6 - MWL_RuinsTower8", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsTower8_LootList_Config = config("6 - MWL_RuinsTower8", "Name of Loot List", "BlackforestLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_Tavern1_Quantity_Config = config("7 - MWL_Tavern1", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_Tavern1_CreatureYaml_Config = config("7 - MWL_Tavern1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_Tavern1_CreatureList_Config = config("7 - MWL_Tavern1", "Name of Creature List", "BlackforestCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_Tavern1_LootYaml_Config = config("7 - MWL_Tavern1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_Tavern1_LootList_Config = config("7 - MWL_Tavern1", "Name of Loot List", "WoodTavernLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_WoodTower1_Quantity_Config = config("8 - MWL_WoodTower1", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_WoodTower1_CreatureYaml_Config = config("8 - MWL_WoodTower1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_WoodTower1_CreatureList_Config = config("8 - MWL_WoodTower1", "Name of Creature List", "BlackforestCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_WoodTower1_LootYaml_Config = config("8 - MWL_WoodTower1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_WoodTower1_LootList_Config = config("8 - MWL_WoodTower1", "Name of Loot List", "BlackforestLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_WoodTower2_Quantity_Config = config("9 - MWL_WoodTower2", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_WoodTower2_CreatureYaml_Config = config("9 - MWL_WoodTower2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_WoodTower2_CreatureList_Config = config("9 - MWL_WoodTower2", "Name of Creature List", "BlackforestCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_WoodTower2_LootYaml_Config = config("9 - MWL_WoodTower2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_WoodTower2_LootList_Config = config("9 - MWL_WoodTower2", "Name of Loot List", "BlackforestLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_WoodTower3_Quantity_Config = config("10 - MWL_WoodTower3", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_WoodTower3_CreatureYaml_Config = config("10 - MWL_WoodTower3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_WoodTower3_CreatureList_Config = config("10 - MWL_WoodTower3", "Name of Creature List", "BlackforestCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_WoodTower3_LootYaml_Config = config("10 - MWL_WoodTower3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_WoodTower3_LootList_Config = config("10 - MWL_WoodTower3", "Name of Loot List", "BlackforestLoot3",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            YAMLManager.ParseDefaultYamls();
            YAMLManager.ParseCustomYamls();
            
            ZoneManager.OnVanillaLocationsAvailable += Locations.AddAllLocations;

            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
                Config.Save();
            }
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                WarpLogger.Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                WarpLogger.Logger.LogError($"There was an issue loading your {ConfigFileName}");
                WarpLogger.Logger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
        public static ConfigEntry<int> MWL_RuinsArena2_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsArena2_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsArena2_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsArena2_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsArena2_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_RuinsCastle1_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsCastle1_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsCastle1_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsCastle1_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsCastle1_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_RuinsCastle3_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsCastle3_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsCastle3_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsCastle3_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsCastle3_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_RuinsTower3_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsTower3_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsTower3_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsTower3_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsTower3_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_RuinsTower8_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsTower8_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsTower8_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsTower8_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsTower8_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_Tavern1_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Tavern1_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_Tavern1_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Tavern1_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_Tavern1_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_WoodTower1_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodTower1_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_WoodTower1_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodTower1_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_WoodTower1_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_WoodTower2_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodTower2_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_WoodTower2_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodTower2_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_WoodTower2_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_WoodTower3_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodTower3_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_WoodTower3_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodTower3_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_WoodTower3_LootList_Config = null!;

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }
    }
}