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


namespace Meadows_Pack_2
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Meadows_Pack_2Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Meadows_Pack_2";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Meadows_Pack_2Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        

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
        
        public static YAMLManager meadows2YAMLManager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "meadowspack2";

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
                false; // This and the variable above are used to prevent the config from saving on startup for each config entry. This is speeds up the startup process

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWL_DeerShrine1_Configuration =
                new LocationConfiguration(this.Config, "MWL_DeerShrine1", 5, "MeadowsCreatures1", "MeadowsLoot1");
            MWL_DeerShrine2_Configuration =
                new LocationConfiguration(this.Config, "MWL_DeerShrine2", 5, "MeadowsCreatures1", "MeadowsLoot1");
            MWL_MeadowsBarn1_Configuration =
                new LocationConfiguration(this.Config, "MWL_MeadowsBarn1", 15, "MeadowsCreatures3", "MeadowsLoot1");
            MWL_MeadowsHouse2_Configuration =
                new LocationConfiguration(this.Config, "MWL_MeadowsHouse2", 5, "MeadowsCreatures2", "MeadowsLoot3");
            MWL_MeadowsRuin1_Configuration =
                new LocationConfiguration(this.Config, "MWL_MeadowsRuin1", 5, "MeadowsCreatures1", "MeadowsLoot1");
            MWL_MeadowsTomb1_Configuration =
                new LocationConfiguration(this.Config, "MWL_MeadowsTomb1", 5, "MeadowsCreatures1", "MeadowsLoot1");
            MWL_MeadowsTomb4_Configuration =
                new LocationConfiguration(this.Config, "MWL_MeadowsTomb4", 5, "MeadowsCreatures1", "MeadowsLoot1");
            MWL_MeadowsTower1_Configuration =
                new LocationConfiguration(this.Config, "MWL_MeadowsTower1", 5, "MeadowsCreatures1", "MeadowsLoot1");
            MWL_OakHut1_Configuration =
                new LocationConfiguration(this.Config, "MWL_OakHut1", 5, "MeadowsCreatures1", "MeadowsLoot1");
            MWL_SmallHouse1_Configuration =
                new LocationConfiguration(this.Config, "MWL_SmallHouse1", 5, "MeadowsCreatures1", "MeadowsLoot1");
            
            meadows2YAMLManager.ParseDefaultYamls();
            meadows2YAMLManager.ParseCustomYamls();
            
            ZoneManager.OnVanillaLocationsAvailable += Locations.AddAllLocations;
            
            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
                Config.Save();
            }
        }
        
        public static LocationConfiguration MWL_DeerShrine1_Configuration;
        public static LocationConfiguration MWL_DeerShrine2_Configuration;
        public static LocationConfiguration MWL_MeadowsBarn1_Configuration;
        public static LocationConfiguration MWL_MeadowsHouse2_Configuration;
        public static LocationConfiguration MWL_MeadowsRuin1_Configuration;
        public static LocationConfiguration MWL_MeadowsTomb1_Configuration;
        public static LocationConfiguration MWL_MeadowsTomb4_Configuration;
        public static LocationConfiguration MWL_MeadowsTower1_Configuration;
        public static LocationConfiguration MWL_OakHut1_Configuration;
        public static LocationConfiguration MWL_SmallHouse1_Configuration;

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
                Meadows_Pack_2Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Meadows_Pack_2Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Meadows_Pack_2Logger.LogError("Please check your config entries for spelling and format!");
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