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
        internal const string ModVersion = "1.0.9";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource More_World_TradersLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

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
        
        public static AssetBundle assetBundleAshlands;
        public static string bundleNameAshlands = "moreworldvendorsashlands";

        public static void LoadAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources(
                bundleName,
                Assembly.GetExecutingAssembly()
            );
            
            assetBundleAshlands = AssetUtils.LoadAssetBundleFromResources(
                bundleNameAshlands,
                Assembly.GetExecutingAssembly()
            );
            
            if (assetBundle == null)
            {
                WarpLogger.Logger.LogError("Failed to load asset bundle with name: " + bundleName);
            }
            
            if (assetBundleAshlands == null)
            {
                WarpLogger.Logger.LogError("Failed to load asset bundle with name: " + bundleNameAshlands);
            }
        }

        public void Awake()
        {
            // Uncomment the line below to use the LocalizationManager for localizing your mod.
            //Localizer.Load(); // Use this to initialize the LocalizationManager (for more information on LocalizationManager, see the LocalizationManager documentation https://github.com/blaxxun-boop/LocalizationManager#example-project).
            bool saveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet =
                false; // This and the variable above are used to prevent the config from saving on startup for each config entry. This is speeds up the startup process.
            
            LoadAssetBundle();
            
            PrefabUtils.LoadIcons();
            PrefabUtils.BuildLocationSpriteData();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            MWL_PlainsTavern1_Configuration =
                new LocationConfiguration(this.Config, "PlainsTavern1", 5);
            MWL_PlainsCamp1_Configuration =
                new LocationConfiguration(this.Config, "PlainsCamp1", 5);
            MWL_BlackForestBlacksmith1_Configuration =
                new LocationConfiguration(this.Config, "BlackForestBlacksmith1", 5);
            MWL_BlackForestBlacksmith2_Configuration =
                new LocationConfiguration(this.Config, "BlackForestBlacksmith2", 5);
            MWL_MountainsBlacksmith1_Configuration =
                new LocationConfiguration(this.Config, "MountainsBlacksmith1", 5);
            MWL_MistlandsBlacksmith1_Configuration =
                new LocationConfiguration(this.Config, "MistlandsBlacksmith1", 5);
            MWL_OceanTavern1_Configuration =
                new LocationConfiguration(this.Config, "OceanTavern1", 5);
            
            
            // MWL_PlainsTavern1_QuantityConfig = config("1 - MWV_PlainsTavern1", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");
            //
            // MWL_PlainsCamp1_QuantityConfig = config("2 - MWV_PlainsCamp1", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");
            //
            // MWL_BlackForestBlacksmith1_QuantityConfig = config("3 - MWL_BlackForestBlacksmith1", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");
            //
            // MWL_BlackForestBlacksmith2_QuantityConfig = config("4 - MWL_BlackForestBlacksmith2", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");
            //
            // MWL_MountainsBlacksmith1_QuantityConfig = config("5 - MWL_MountainsBlacksmith1", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");
            //
            // MWL_MistlandsBlacksmith1_QuantityConfig = config("6 - MWL_MistlandsCamp1", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");
            //
            // MWL_OceanTavern1_QuantityConfig = config("7 - MWL_OceanTavern1", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");

            
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
        
        public static LocationConfiguration MWL_PlainsTavern1_Configuration;
        public static LocationConfiguration MWL_PlainsCamp1_Configuration;
        public static LocationConfiguration MWL_BlackForestBlacksmith1_Configuration;
        public static LocationConfiguration MWL_BlackForestBlacksmith2_Configuration;
        public static LocationConfiguration MWL_MountainsBlacksmith1_Configuration;
        public static LocationConfiguration MWL_MistlandsBlacksmith1_Configuration;
        public static LocationConfiguration MWL_OceanTavern1_Configuration;
        
        // public static ConfigEntry<int> MWL_PlainsTavern1_QuantityConfig = null!;
        // public static ConfigEntry<int> MWL_PlainsCamp1_QuantityConfig = null!;
        // public static ConfigEntry<int> MWL_BlackForestBlacksmith1_QuantityConfig = null!;
        // public static ConfigEntry<int> MWL_BlackForestBlacksmith2_QuantityConfig = null!;
        // public static ConfigEntry<int> MWL_MountainsBlacksmith1_QuantityConfig = null!;
        // public static ConfigEntry<int> MWL_MistlandsBlacksmith1_QuantityConfig = null!;
        // public static ConfigEntry<int> MWL_OceanTavern1_QuantityConfig = null!;
        
        

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