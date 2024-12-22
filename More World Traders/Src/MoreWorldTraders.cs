using System;
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
using More_World_Traders.Utils;
using ServerSync;
using UnityEngine;
using Paths = BepInEx.Paths;

namespace More_World_Traders
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class More_World_TradersPlugin : BaseUnityPlugin
    {
        internal const string ModName = "More_World_Traders";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource More_World_TradersLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

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
        
        public static YAMLManager moreWorldTradersYAMLManager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "moreworldvendors";

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
            
            LoadAssetBundle();
            
            PrefabUtils.LoadIcons();
            PrefabUtils.BuildLocationSpriteData();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            MWL_PlainsTavern1_QuantityConfig = config("1 - MWV_PlainsTavern1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            
            MWL_PlainsCamp1_QuantityConfig = config("2 - MWV_PlainsCamp1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            
            MWL_BlackForestBlacksmith1_QuantityConfig = config("3 - MWL_BlackForestBlacksmith1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");

            MWL_BlackForestBlacksmith2_QuantityConfig = config("4 - MWL_BlackForestBlacksmith2", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");

            MWL_MountainsBlacksmith1_QuantityConfig = config("5 - MWL_MountainsBlacksmith1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            
            MWL_MistlandsBlacksmith1_QuantityConfig = config("6 - MWL_MistlandsCamp1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");

            
            moreWorldTradersYAMLManager.ParseTraderYaml("warpalicious.More_World_Traders_MoreWorldTraders.yml");
            
            TranslationUtils.AddLocalizations();
            StatusEffectUtils.CreateCustomStatusEffects();
            PrefabUtils.CreateCustomItems();
            ZoneManager.OnVanillaLocationsAvailable += Locations.AddAllLocations;
            
            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
                Config.Save();
            }
        }
        
        public static ConfigEntry<int> MWL_PlainsTavern1_QuantityConfig = null!;
        public static ConfigEntry<int> MWL_PlainsCamp1_QuantityConfig = null!;
        public static ConfigEntry<int> MWL_BlackForestBlacksmith1_QuantityConfig = null!;
        public static ConfigEntry<int> MWL_BlackForestBlacksmith2_QuantityConfig = null!;
        public static ConfigEntry<int> MWL_MountainsBlacksmith1_QuantityConfig = null!;
        public static ConfigEntry<int> MWL_MistlandsBlacksmith1_QuantityConfig = null!;
        
        

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
                More_World_TradersLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                More_World_TradersLogger.LogError($"There was an issue loading your {ConfigFileName}");
                More_World_TradersLogger.LogError("Please check your config entries for spelling and format!");
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