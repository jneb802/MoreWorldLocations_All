using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using Common;
using Jotunn.Managers;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

namespace Valheim_NPCs_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Valheim_NPCs_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "NPC_Locations_Pack_1";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static YAMLManager NPCPackYAMLManager = new YAMLManager();

        public static readonly ManualLogSource Valheim_NPCs_Pack_1Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

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
        
        public static AssetBundle assetBundle;
        public static string bundleName = "npcspack1";

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
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWL_FenrisMaw_QuantityConfig = config("1 - MWL_FenrisMaw", "Spawn Quantity", 1,
                "Amount of this location the game will attempt to place during world generation");
            MWL_FenrisMaw_CreatureYamlConfig = config("1 - MWL_FenrisMaw", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FenrisMaw_CreatureListConfig = config("1 - MWL_FenrisMaw", "Name of Creature List", "SwampCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FenrisMaw_LootYamlConfig = config("1 - MWL_FenrisMaw", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FenrisMaw_LootListConfig = config("1 - MWL_FenrisMaw", "Name of Loot List", "SwampLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_MarketplaceLarge1_QuantityConfig = config("2 - MWL_MarketplaceLarge1", "Spawn Quantity", 1,
                "Amount of this location the game will attempt to place during world generation");
            MWL_MarketplaceLarge1_CreatureYamlConfig = config("2 - MWL_MarketplaceLarge1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_MarketplaceLarge1_CreatureListConfig = config("2 - MWL_MarketplaceLarge1", "Name of Creature List", "SwampCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_MarketplaceLarge1_LootYamlConfig = config("2 - MWL_MarketplaceLarge1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_MarketplaceLarge1_LootListConfig = config("2 - MWL_MarketplaceLarge1", "Name of Loot List", "SwampLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_MarketSmall1_QuantityConfig = config("3 - MWL_MarketSmall1", "Spawn Quantity", 2,
                "Amount of this location the game will attempt to place during world generation");
            MWL_MarketSmall1_CreatureYamlConfig = config("3 - MWL_MarketSmall1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_MarketSmall1_CreatureListConfig = config("3 - MWL_MarketSmall1", "Name of Creature List", "SwampCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_MarketSmall1_LootYamlConfig = config("3 - MWL_MarketSmall1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_MarketSmall1_LootListConfig = config("3 - MWL_MarketSmall1", "Name of Loot List", "SwampLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_ShopSmall1_QuantityConfig = config("4 - MWL_ShopSmall1", "Spawn Quantity", 2,
                "Amount of this location the game will attempt to place during world generation");
            MWL_ShopSmall1_CreatureYamlConfig = config("4 - MWL_ShopSmall1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_ShopSmall1_CreatureListConfig = config("4 - MWL_ShopSmall1", "Name of Creature List", "SwampCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_ShopSmall1_LootYamlConfig = config("4 - MWL_ShopSmall1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_ShopSmall1_LootListConfig = config("4 - MWL_ShopSmall1", "Name of Loot List", "SwampLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_ThirstyRavenInn_QuantityConfig = config("5 - MWL_ThirstyRavenInn", "Spawn Quantity", 1,
                "Amount of this location the game will attempt to place during world generation");
            MWL_ThirstyRavenInn_CreatureYamlConfig = config("5 - MWL_ThirstyRavenInn", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_ThirstyRavenInn_CreatureListConfig = config("5 - MWL_ThirstyRavenInn", "Name of Creature List", "SwampCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_ThirstyRavenInn_LootYamlConfig = config("5 - MWL_ThirstyRavenInn", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_ThirstyRavenInn_LootListConfig = config("5 - MWL_ThirstyRavenInn", "Name of Loot List", "SwampLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");

            NPCPackYAMLManager.ParseDefaultYamls();
            NPCPackYAMLManager.ParseCustomYamls();
            
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
        
        public static ConfigEntry<int> MWL_FenrisMaw_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FenrisMaw_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FenrisMaw_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FenrisMaw_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FenrisMaw_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_MarketplaceLarge1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MarketplaceLarge1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_MarketplaceLarge1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MarketplaceLarge1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_MarketplaceLarge1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_MarketSmall1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MarketSmall1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_MarketSmall1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_MarketSmall1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_MarketSmall1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_ShopSmall1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_ShopSmall1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_ShopSmall1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_ShopSmall1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_ShopSmall1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_ThirstyRavenInn_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_ThirstyRavenInn_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_ThirstyRavenInn_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_ThirstyRavenInn_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_ThirstyRavenInn_LootListConfig = null!;

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Valheim_NPCs_Pack_1Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Valheim_NPCs_Pack_1Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Valheim_NPCs_Pack_1Logger.LogError("Please check your config entries for spelling and format!");
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