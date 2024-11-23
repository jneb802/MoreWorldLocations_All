using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using ServerSync;
using UnityEngine;
using Common;
using Jotunn;
using Jotunn.Managers;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

namespace Mistlands_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Mistlands_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Mistlands_Pack_1";
        internal const string ModVersion = "1.0.2";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Mistlands_Pack_1Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

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
        
        public static YAMLManager mistlandsYAMLManager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "mistlandspack1";

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
        
        public static List<string> mistlandsLocationsList = new List<string>();

        public static void CreateLocationConfigs(List<string> locationList)
        {
            
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
            SetupWatcher();

            LoadAssetBundle();
            
            MWL_MistFort2_QuantityConfig = config("1 - MWL_MistFort2", "Spawn Quantity", 20,
                "Amount of this location the game will attempt to place during world generation");
            MWL_MistFort2_CreatureYamlConfig = config("1 - MWL_MistFort2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_MistFort2_CreatureListConfig = config("1 - MWL_MistFort2", "Name of Creature List", "MistCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_MistFort2_LootYamlConfig = config("1 - MWL_MistFort2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_MistFort2_LootListConfig = config("1 - MWL_MistFort2", "Name of Loot List", "MistLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_SecretRoom1_QuantityConfig = config("2 - MWL_SecretRoom1", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_SecretRoom1_CreatureYamlConfig = config("2 - MWL_SecretRoom1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_SecretRoom1_CreatureListConfig = config("2 - MWL_SecretRoom1", "Name of Creature List", "MistCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_SecretRoom1_LootYamlConfig = config("2 - MWL_SecretRoom1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_SecretRoom1_LootListConfig = config("2 - MWL_SecretRoom1", "Name of Loot List", "MistLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_MistWorkshop1_QuantityConfig = config("3 - MWL_MistWorkshop1", "Spawn Quantity", 25,
                "Amount of this location the game will attempt to place during world generation");
            MWL_MistWorkshop1_CreatureYamlConfig = config("3 - MWL_MistWorkshop1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_MistWorkshop1_CreatureListConfig = config("3 - MWL_MistWorkshop1", "Name of Creature List", "MistCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_MistWorkshop1_LootYamlConfig = config("3 - MWL_MistWorkshop1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_MistWorkshop1_LootListConfig = config("3 - MWL_MistWorkshop1", "Name of Loot List", "MistLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_MistTower1_QuantityConfig = config("4 - MWL_MistTower1", "Spawn Quantity", 20,
                "Amount of this location the game will attempt to place during world generation");
            MWL_MistTower1_CreatureYamlConfig = config("4 - MWL_MistTower1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_MistTower1_CreatureListConfig = config("4 - MWL_MistTower1", "Name of Creature List", "MistCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_MistTower1_LootYamlConfig = config("4 - MWL_MistTower1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_MistTower1_LootListConfig = config("4 - MWL_MistTower1", "Name of Loot List", "MistLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_MistWall1_QuantityConfig = config("5 - MWL_MistWall1", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_MistWall1_CreatureYamlConfig = config("5 - MWL_MistWall1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_MistWall1_CreatureListConfig = config("5 - MWL_MistWall1", "Name of Creature List", "MistCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_MistWall1_LootYamlConfig = config("5 - MWL_MistWall1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_MistWall1_LootListConfig = config("5 - MWL_MistWall1", "Name of Loot List", "MistLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_MistTower2_QuantityConfig = config("6 - MWL_MistTower2", "Spawn Quantity", 25,
                "Amount of this location the game will attempt to place during world generation");
            MWL_MistTower2_CreatureYamlConfig = config("6 - MWL_MistTower2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_MistTower2_CreatureListConfig = config("6 - MWL_MistTower2", "Name of Creature List", "MistCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_MistTower2_LootYamlConfig = config("6 - MWL_MistTower2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_MistTower2_LootListConfig = config("6 - MWL_MistTower2", "Name of Loot List", "MistLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_MistHut1_QuantityConfig = config("7 - MWL_MistHut1", "Spawn Quantity", 25,
                "Amount of this location the game will attempt to place during world generation");
            MWL_MistHut1_CreatureYamlConfig = config("7 - MWL_MistHut1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_MistHut1_CreatureListConfig = config("7 - MWL_MistHut1", "Name of Creature List", "MistCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_MistHut1_LootYamlConfig = config("7 - MWL_MistHut1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_MistHut1_LootListConfig = config("7 - MWL_MistHut1", "Name of Loot List", "MistLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
                        
            mistlandsYAMLManager.ParseDefaultYamls();
            mistlandsYAMLManager.ParseCustomYamls();
            
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
        
        public static ConfigEntry<int> MWL_MistFort2_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistFort2_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistFort2_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistFort2_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistFort2_LootListConfig = null!;
                
        public static ConfigEntry<int> MWL_SecretRoom1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_SecretRoom1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_SecretRoom1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_SecretRoom1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_SecretRoom1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_MistWorkshop1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistWorkshop1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistWorkshop1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistWorkshop1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistWorkshop1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_MistTower1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistTower1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistTower1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistTower1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistTower1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_MistWall1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistWall1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistWall1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle>MWL_MistWall1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistWall1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_MistTower2_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistTower2_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistTower2_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistTower2_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistTower2_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_MistHut1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistHut1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistHut1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MistHut1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_MistHut1_LootListConfig = null!;

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Mistlands_Pack_1Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Mistlands_Pack_1Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Mistlands_Pack_1Logger.LogError("Please check your config entries for spelling and format!");
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