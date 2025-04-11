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
using UnityEngine;
using Common;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

namespace BlackForest_Pack_2
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class BlackForest_Pack_2Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Blackforest_Pack_2";
        internal const string ModVersion = "1.1.8";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        
        public static readonly ManualLogSource BlackForest_Pack_2Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        
        public static AssetBundle assetBundle;
        public static string bundleName = "blackforestpack2";
        
        public static YAMLManager blackforest2YAMLmanager = new YAMLManager();

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


            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWL_ForestForge1_Configuration =
                new LocationConfiguration(this.Config, "ForestForge1", 20, "BlackforestCreatures3", "BlackforestLoot3");
            MWL_ForestForge2_Configuration =
                new LocationConfiguration(this.Config, "ForestForge2", 20, "BlackforestCreatures3", "BlackforestLoot3");
            MWL_ForestGreatHouse2_Configuration =
                new LocationConfiguration(this.Config, "ForestGreatHouse2", 20, "BlackforestCreatures3", "BlackforestLoot3");
            MWL_ForestHouse2_Configuration =
                new LocationConfiguration(this.Config, "ForestHouse2", 20, "BlackforestCreatures3", "BlackforestLoot3");
            MWL_ForestRuin1_Configuration =
                new LocationConfiguration(this.Config, "ForestRuin1", 20, "BlackforestCreatures3", "BlackforestLoot1");
            MWL_ForestTower2_Configuration =
                new LocationConfiguration(this.Config, "ForestTower2", 20, "BlackforestCreatures1", "BlackforestLoot3");
            MWL_ForestTower3_Configuration =
                new LocationConfiguration(this.Config, "ForestTower3", 20, "BlackforestCreatures1", "BlackforestLoot3");
            MWL_MassGrave1_Configuration =
                new LocationConfiguration(this.Config, "MassGrave1", 15, "BlackforestCreatures3", "BlackforestLoot1");
            MWL_StoneFormation1_Configuration =
                new LocationConfiguration(this.Config, "StoneFormation1", 15, "BlackforestCreatures3", "BlackforestLoot3");
            
            MWL_GuardTower1_Configuration =
                new LocationConfiguration(this.Config, "GuardTower1", 5, "BlackforestCreatures1", "BlackforestLoot3");
            MWL_RootRuins1_Configuration =
                new LocationConfiguration(this.Config, "RootRuins1", 15, "BlackforestCreatures3", "BlackforestLoot1");
            MWL_RootsTower1_Configuration =
                new LocationConfiguration(this.Config, "RootsTower1", 20, "BlackforestCreatures3", "BlackforestLoot2");
            MWL_RootsTower2_Configuration =
                new LocationConfiguration(this.Config, "RootsTower2", 10, "BlackforestCreatures3", "BlackforestLoot2");
            MWL_StoneOutlook1_Configuration =
                new LocationConfiguration(this.Config, "StoneOutlook1", 10, "BlackforestCreatures3", "BlackforestLoot1");
        
            MWL_ForestRuin2_Configuration =
                new LocationConfiguration(this.Config, "ForestRuin2", 15, "BlackforestCreatures2", "BlackforestLoot3");
            MWL_ForestRuin3_Configuration =
                new LocationConfiguration(this.Config, "ForestRuin3", 15, "BlackforestCreatures3", "BlackforestLoot1");
            MWL_ForestSkull1_Configuration =
                new LocationConfiguration(this.Config, "ForestSkull1", 15, "BlackforestCreatures1", "BlackforestLoot3");
            MWL_ForestTower4_Configuration =
                new LocationConfiguration(this.Config, "ForestTower4", 15, "BlackforestCreatures3", "BlackforestLoot2");
            MWL_ForestTower5_Configuration =
                new LocationConfiguration(this.Config, "ForestTower5", 15, "BlackforestCreatures3", "BlackforestLoot1");
            
            MWL_ForestWarehouse1_Configuration =
                new LocationConfiguration(this.Config, "MWL_ForestWarehouse1", 10, "BlackforestCreatures2", "BlackforestLoot2");
            MWL_ForestFort1_Configuration =
                new LocationConfiguration(this.Config, "MWL_ForestFort1", 10, "BlackforestCreatures3", "BlackforestLoot3");
            MWL_ForestStation1_Configuration =
                new LocationConfiguration(this.Config, "MWL_ForestStation1", 10, "BlackforestCreatures1", "BlackforestLoot1");
            MWL_ForestLodge1_Configuration =
                new LocationConfiguration(this.Config, "MWL_ForestLodge1", 10, "BlackforestCreatures2", "BlackforestLoot2");
            MWL_ForestMine1_Configuration =
                new LocationConfiguration(this.Config, "MWL_ForestMine1", 10, "BlackforestCreatures3", "BlackforestLoot3");
            
            blackforest2YAMLmanager.ParseDefaultYamls();
            blackforest2YAMLmanager.ParseCustomYamls();
            
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
        
        public static LocationConfiguration MWL_ForestForge1_Configuration;
        public static LocationConfiguration MWL_ForestForge2_Configuration;
        public static LocationConfiguration MWL_ForestGreatHouse2_Configuration;
        public static LocationConfiguration MWL_ForestHouse2_Configuration;
        public static LocationConfiguration MWL_ForestRuin1_Configuration;
        public static LocationConfiguration MWL_ForestTower2_Configuration;
        public static LocationConfiguration MWL_ForestTower3_Configuration;
        public static LocationConfiguration MWL_MassGrave1_Configuration;
        public static LocationConfiguration MWL_StoneFormation1_Configuration;
        
        public static LocationConfiguration MWL_GuardTower1_Configuration;
        public static LocationConfiguration MWL_RootRuins1_Configuration;
        public static LocationConfiguration MWL_RootsTower1_Configuration;
        public static LocationConfiguration MWL_RootsTower2_Configuration;
        public static LocationConfiguration MWL_StoneOutlook1_Configuration;
        
        public static LocationConfiguration MWL_ForestRuin2_Configuration;
        public static LocationConfiguration MWL_ForestRuin3_Configuration;
        public static LocationConfiguration MWL_ForestSkull1_Configuration;
        public static LocationConfiguration MWL_ForestTower4_Configuration;
        public static LocationConfiguration MWL_ForestTower5_Configuration;
        
        public static LocationConfiguration MWL_ForestWarehouse1_Configuration;
        public static LocationConfiguration MWL_ForestFort1_Configuration;
        public static LocationConfiguration MWL_ForestStation1_Configuration;
        public static LocationConfiguration MWL_ForestLodge1_Configuration;
        public static LocationConfiguration MWL_ForestMine1_Configuration;


        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }
    }
}