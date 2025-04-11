﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Common;
using Jotunn.Managers;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

namespace Mountains_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Mountains_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Mountains_Pack_1";
        internal const string ModVersion = "1.1.5";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Mountains_Pack_1Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        // Location Manager variables
        public Texture2D tex = null!;

        public enum Toggle
        {
            On = 1,
            Off = 0
        }
        
        public static AssetBundle assetBundle;
        public static string bundleName = "mountainspack1";
        
        public static YAMLManager MountainYAML = new YAMLManager();
        public static LocationManager MountainLocation = new LocationManager();

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
            
            MWL_StoneCastle1_Configuration =
                new LocationConfiguration(this.Config, "StoneCastle1", 5, "MountainsCreatures1", "MountainsLoot1");
            MWL_StoneFort1_Configuration =
                new LocationConfiguration(this.Config, "StoneFort1", 10, "MountainsCreatures1", "MountainsLoot1");
            MWL_StoneHall1_Configuration =
                new LocationConfiguration(this.Config, "StoneHall1", 10, "MountainsCreatures4", "MountainsLoot1");
            MWL_StoneTavern1_Configuration =
                new LocationConfiguration(this.Config, "StoneTavern1", 10, "MountainsCreatures1", "MountainsLoot1");
            MWL_StoneTower1_Configuration =
                new LocationConfiguration(this.Config, "StoneTower1", 10, "MountainsCreatures2", "MountainsLoot1");
            MWL_StoneTower2_Configuration =
                new LocationConfiguration(this.Config, "StoneTower2", 10, "MountainsCreatures2", "MountainsLoot1");
            MWL_WoodBarn1_Configuration =
                new LocationConfiguration(this.Config, "WoodBarn1", 10, "MountainsCreatures1", "MountainsLoot1");
            MWL_WoodFarm1_Configuration =
                new LocationConfiguration(this.Config, "WoodFarm1", 10, "MountainsCreatures3", "MountainsLoot1");
            MWL_WoodHouse1_Configuration =
                new LocationConfiguration(this.Config, "WoodHouse1", 5, "MountainsCreatures1", "MountainsLoot1");
            
            MWL_MountainAedicule1_Configuration = 
                new LocationConfiguration(this.Config, "MountainAedicule1", 10, "MountainsCreatures2", "MountainsLoot1");
            MWL_MountainPagota1_Configuration = 
                new LocationConfiguration(this.Config, "MountainPagota1", 10, "MountainsCreatures2", "MountainsLoot1");
            MWL_MountainTower1_Configuration = 
                new LocationConfiguration(this.Config, "MountainTower1", 10, "MountainsCreatures2", "MountainsLoot1");
            MWL_MountainTreasury1_Configuration = 
                new LocationConfiguration(this.Config, "MountainTreasury1", 10, "MountainsCreatures3", "MountainsLoot1");
            MWL_MountainHogan1_Configuration = 
                new LocationConfiguration(this.Config, "MountainHogan1", 10, "MountainsCreatures1", "MountainsLoot1");

            
            MountainYAML.ParseDefaultYamls();
            MountainYAML.ParseCustomYamls();
            
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
        
        public static LocationConfiguration MWL_StoneCastle1_Configuration;
        public static LocationConfiguration MWL_StoneFort1_Configuration;
        public static LocationConfiguration MWL_StoneHall1_Configuration;
        public static LocationConfiguration MWL_StoneTavern1_Configuration;
        public static LocationConfiguration MWL_StoneTower1_Configuration;
        public static LocationConfiguration MWL_StoneTower2_Configuration;
        public static LocationConfiguration MWL_WoodBarn1_Configuration;
        public static LocationConfiguration MWL_WoodFarm1_Configuration;
        public static LocationConfiguration MWL_WoodHouse1_Configuration;
        
        public static LocationConfiguration MWL_MountainAedicule1_Configuration;
        public static LocationConfiguration MWL_MountainPagota1_Configuration;
        public static LocationConfiguration MWL_MountainTower1_Configuration;
        public static LocationConfiguration MWL_MountainTreasury1_Configuration;
        public static LocationConfiguration MWL_MountainHogan1_Configuration;


        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Mountains_Pack_1Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Mountains_Pack_1Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Mountains_Pack_1Logger.LogError("Please check your config entries for spelling and format!");
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