﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Common;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Managers;
using Jotunn.Utils;
using LocalizationManager;
using ServerSync;
using UnityEngine;
using Paths = BepInEx.Paths;

namespace Underground_Ruins
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Underground_RuinsPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Underground_Ruins";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Dungeon_The_RitualLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        
        public static YAMLManager dungeonBFDYamlManager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "dungeonblackforest";
        public static GameObject dungeonGameObject;

        // Location Manager variables
        public Texture2D tex = null!;

        // Use only if you need them
        //private Sprite mySprite = null!;
        //private SpriteRenderer sr = null!;

        public enum Toggle
        {
            On = 1,
            Off = 0
        }
        
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

        public void Awake()
        {
            // Uncomment the line below to use the LocalizationManager for localizing your mod.
            //Localizer.Load(); // Use this to initialize the LocalizationManager (for more information on LocalizationManager, see the LocalizationManager documentation https://github.com/blaxxun-boop/LocalizationManager#example-project).
            bool saveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet =
                false; // This and the variable above are used to prevent the config from saving on startup for each config entry. This is speeds up the startup process.

            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);

            RoomExtras.ApplyPatches(_harmony);
                
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWD_UndergroundRuins_Quantity_Config = config("1 - Underground Ruins", "Spawn Quantity", 30,
                "Amount of attempts world generation will try to place dungeon exterior during world generation");
            MWD_UndergroundRuins_CreatureYaml_Config = config("1 - Underground Ruins", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, dungeon will select creatures from list in the custom YAML file in BepinEx config folder");
            MWD_UndergroundRuins_CreatureList_Config = config("1 - Underground Ruins", "Name of Creature List", "UndergroundRuinsCreatures1",
                "The name of the creature list to use from YAML file");
            MWD_UndergroundRuins_LootYaml_Config = config("1 - Underground Ruins", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, dungeon will select loot from list in the custom YAML file in BepinEx config folder");
            MWD_UndergroundRuins_LootList_Config = config("1 - Underground Ruins", "Name of Loot List", "UndergroundRuinsLoot1",
                "The name of the loot list to use from YAML file");
            MWD_UndergroundRuins_PickableItemYaml_Config = config("1 - Underground Ruins", "Use Custom PickableItem YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, dungeon will select loot from list in the custom YAML file in BepinEx config folder");
            MWD_UndergroundRuins_PickableItemList_Config = config("1 - Underground Ruins", "Name of PickableItem List", "UndergroundRuinsPickables1",
                "The name of the loot list to use from YAML file");
            
            dungeonGameObject = assetBundle.LoadAsset<GameObject>("BFD_Exterior");
            Rooms.RegisterTheme(dungeonGameObject, "Underground Ruins");
            
            dungeonBFDYamlManager.ParseDefaultYamls();
            dungeonBFDYamlManager.ParseCustomYamls();
            dungeonBFDYamlManager.ParsePickableItemYaml("warpalicious.More_World_Locations");

            dungeonBFDYamlManager.BuildCreatureList(MWD_UndergroundRuins_CreatureYaml_Config.Value, MWD_UndergroundRuins_CreatureList_Config.Value);
            dungeonBFDYamlManager.BuildLootList(MWD_UndergroundRuins_LootYaml_Config.Value, MWD_UndergroundRuins_LootList_Config.Value);
            dungeonBFDYamlManager.BuildPickableList(MWD_UndergroundRuins_PickableItemYaml_Config.Value, MWD_UndergroundRuins_PickableItemList_Config.Value);
            
            TranslationUtils.AddLocalizations();
            // Creatures.CreateShamanBoss();
            
            PrefabManager.OnVanillaPrefabsAvailable += CustomPrefabs.RegisterKitPrefabs;
            ZoneManager.OnVanillaLocationsAvailable += Locations.AddAllLocations;
            DungeonManager.OnVanillaRoomsAvailable += Rooms.AddAllRooms;

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
        
        public static ConfigEntry<int> MWD_UndergroundRuins_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWD_UndergroundRuins_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWD_UndergroundRuins_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWD_UndergroundRuins_LootYaml_Config = null!;
        public static ConfigEntry<string> MWD_UndergroundRuins_LootList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWD_UndergroundRuins_PickableItemYaml_Config = null!;
        public static ConfigEntry<string> MWD_UndergroundRuins_PickableItemList_Config = null!;

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Dungeon_The_RitualLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Dungeon_The_RitualLogger.LogError($"There was an issue loading your {ConfigFileName}");
                Dungeon_The_RitualLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

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

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        }

        #endregion
    }

    public static class KeyboardExtensions
    {
        public static bool IsKeyDown(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) &&
                   shortcut.Modifiers.All(Input.GetKey);
        }

        public static bool IsKeyHeld(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) &&
                   shortcut.Modifiers.All(Input.GetKey);
        }
    }
}