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
using UnityEngine;
using Paths = BepInEx.Paths;

namespace AshlandsPack1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class AshlandsPack1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "AshlandsPack1";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource AshlandsPack1Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        
        public static AssetBundle assetBundle;
        public static string bundleName = "ashlandspack1";
        
        public static YAMLManager ashlandsYAMLManager = new YAMLManager();

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

        public void Awake()
        {
            // Uncomment the line below to use the LocalizationManager for localizing your mod.
            //Localizer.Load(); // Use this to initialize the LocalizationManager (for more information on LocalizationManager, see the LocalizationManager documentation https://github.com/blaxxun-boop/LocalizationManager#example-project).
            bool saveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet =
                false; // This and the variable above are used to prevent the config from saving on startup for each config entry. This is speeds up the startup process.

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            LoadAssetBundle();

            MWL_AshlandsFort1_Configuration =
                new LocationConfiguration(this.Config, "MWL_AshlandsFort1", 5, "AshlandsCreatures1", "AshlandsLoot1");
            MWL_AshlandsFort2_Configuration =
                new LocationConfiguration(this.Config, "MWL_AshlandsFort2", 5, "AshlandsCreatures1", "AshlandsLoot1");
            MWL_AshlandsFort3_Configuration =
                new LocationConfiguration(this.Config, "MWL_AshlandsFort3", 5, "AshlandsCreatures1", "AshlandsLoot1");
            
            ashlandsYAMLManager.ParseDefaultYamls();
            ashlandsYAMLManager.ParseCustomYamls();
            
            ZoneManager.OnVanillaLocationsAvailable += Locations.AddAllLocations;

            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
                Config.Save();
            }
        }
        
        public static LocationConfiguration MWL_AshlandsFort1_Configuration;
        public static LocationConfiguration MWL_AshlandsFort2_Configuration;
        public static LocationConfiguration MWL_AshlandsFort3_Configuration;

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
                AshlandsPack1Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                AshlandsPack1Logger.LogError($"There was an issue loading your {ConfigFileName}");
                AshlandsPack1Logger.LogError("Please check your config entries for spelling and format!");
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