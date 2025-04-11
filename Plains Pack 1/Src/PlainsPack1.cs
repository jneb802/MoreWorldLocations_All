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

namespace Plains_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plains_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Plains_Pack_1";
        internal const string ModVersion = "1.1.3";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Plains_Pack_1Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

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
        
        public static YAMLManager plainsYAMLmanager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "plainspack1";

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

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWL_GoblinFort1_Configuration =
                new LocationConfiguration(this.Config, "GoblinFort1", 10, "PlainsCreatures2", "PlainsLoot1");
            MWL_FulingRock1_Configuration =
                new LocationConfiguration(this.Config, "FulingRock1", 15, "PlainsCreatures3", "PlainsLoot2");
            MWL_FulingVillage1_Configuration =
                new LocationConfiguration(this.Config, "FulingVillage1", 15, "PlainsCreatures3", "PlainsLoot1");
            MWL_FulingVillage2_Configuration =
                new LocationConfiguration(this.Config, "FulingVillage2", 15, "PlainsCreatures2", "PlainsLoot1");
            MWL_PlainsPillar1_Configuration =
                new LocationConfiguration(this.Config, "PlainsPillar1", 15, "PlainsCreatures1", "PlainsLoot1");
            MWL_FulingTemple1_Configuration =
                new LocationConfiguration(this.Config, "FulingTemple1", 15, "PlainsCreatures2", "PlainsLoot1");
            MWL_FulingTemple2_Configuration =
                new LocationConfiguration(this.Config, "FulingTemple2", 20, "PlainsCreatures1", "PlainsLoot1");
            MWL_FulingTemple3_Configuration =
                new LocationConfiguration(this.Config, "FulingTemple3", 20, "PlainsCreatures1", "PlainsLoot1");
            MWL_FulingWall1_Configuration =
                new LocationConfiguration(this.Config, "FulingWall1", 20, "PlainsCreatures3", "PlainsLoot1");
            MWL_FulingTower1_Configuration =
                new LocationConfiguration(this.Config, "FulingTower1", 20, "PlainsCreatures3", "PlainsLoot1");
            MWL_GoblinCave1_Configuration =
                new LocationConfiguration(this.Config, "GoblinCave1", 20, "PlainsCreatures2", "PlainsLoot1");
            
            MWL_PlainsArena1_Configuration = 
                new LocationConfiguration(this.Config, "PlainsArena1", 10, "PlainsCreatures2", "PlainsLoot1");
            MWL_PlainsFarm1_Configuration = 
                new LocationConfiguration(this.Config, "PlainsFarm1", 10, "PlainsCreatures1", "PlainsLoot1");
            MWL_PlainsManor1_Configuration = 
                new LocationConfiguration(this.Config, "PlainsManor1", 10, "PlainsCreatures3", "PlainsLoot1");
            MWL_PlainsOracle1_Configuration = 
                new LocationConfiguration(this.Config, "PlainsOracle1", 10, "PlainsCreatures2", "PlainsLoot1");
            
            plainsYAMLmanager.ParseDefaultYamls();
            plainsYAMLmanager.ParseCustomYamls();
            
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
        
        public static LocationConfiguration MWL_GoblinFort1_Configuration;
        public static LocationConfiguration MWL_FulingRock1_Configuration;
        public static LocationConfiguration MWL_FulingVillage1_Configuration;
        public static LocationConfiguration MWL_FulingVillage2_Configuration;
        public static LocationConfiguration MWL_PlainsPillar1_Configuration;
        public static LocationConfiguration MWL_FulingTemple1_Configuration;
        public static LocationConfiguration MWL_FulingTemple2_Configuration;
        public static LocationConfiguration MWL_FulingTemple3_Configuration;
        public static LocationConfiguration MWL_FulingWall1_Configuration;
        public static LocationConfiguration MWL_FulingTower1_Configuration;
        public static LocationConfiguration MWL_GoblinCave1_Configuration;
        
        public static LocationConfiguration MWL_PlainsArena1_Configuration;
        public static LocationConfiguration MWL_PlainsFarm1_Configuration;
        public static LocationConfiguration MWL_PlainsManor1_Configuration;
        public static LocationConfiguration MWL_PlainsOracle1_Configuration;


        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Plains_Pack_1Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Plains_Pack_1Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Plains_Pack_1Logger.LogError("Please check your config entries for spelling and format!");
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