﻿using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Common;
using HarmonyLib;
using Jotunn.Managers;
using More_World_Locations_AIO.Shipments;
using More_World_Locations_AIO.Shrines;
using More_World_Locations_AIO.Utils;
using More_World_Locations_AIO.Waystones;
using UnityEngine;

namespace More_World_Locations_AIO
{
    [BepInIncompatibility("hyleanlegend.RuneMagic")]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class More_World_Locations_AIOPlugin : BaseUnityPlugin
    {
        internal const string ModName = "More_World_Locations_AIO";
        internal const string ModVersion = "2.0.4";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        
        public static readonly ManualLogSource More_World_Locations_AIOLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);
        
        public static YAMLManager YAMLManager = new YAMLManager();
        
        public static GameObject root = null!;
        
        //public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        
        public void Awake()
        {
            BepinexConfigs.Config = Config;
            bool saveOnSet = BepinexConfigs.Config.SaveOnConfigSet;
            BepinexConfigs.Config.SaveOnConfigSet =
                false;
            
            // create a root object to contain all clones, necessary to hold reference to -int game objects
            root = new GameObject("root");
            DontDestroyOnLoad(root);
            root.SetActive(false);
            
            //PortInit.Init(root);
            
            UpgradeWorldCommands.AddUpgradeWorldCommands();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();

            // Prefabs.LoadPrefabBundles();
            // PortPrefabs.LoadPrefabBundles();
            // PortPrefabs.AddPortUIPrefabs();
            
            // AssetBundles.BuildManifest(AssetBundles.bundle1, AssetBundles.assetPathsInBundle1, "1");
            // AssetBundles.BuildManifest(AssetBundles.bundle2, AssetBundles.assetPathsInBundle2, "2");
            // AssetBundles.BuildManifest(AssetBundles.bundle3, AssetBundles.assetPathsInBundle3, "3");
            // AssetBundles.BuildManifest(AssetBundles.bundle4, 
            //     Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", "moreworldlocations_assetbundle_4.manifest"),
            //     AssetBundles.assetPathsInBundle4, 
            //     "4");
            
            // AssetBundles.BuildCombinedManifest(
            //     Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", "Bundles"), 
            //     "full",
            //     AssetPaths.assetPathsInBundleFull
            // );   
            
            // YAMLManager.ParseDefaultYamls();
            // YAMLManager.ParseCustomYamls();
            
            BepinexConfigs.GenerateConfigs(Config);
            
            // PrefabManager.OnVanillaPrefabsAvailable += YAMLManager.BuildCreatureLists;
            // PrefabManager.OnVanillaPrefabsAvailable += YAMLManager.BuildLootLists;

            PrefabManager.OnVanillaPrefabsAvailable += Initialize;
            ZoneManager.OnVanillaLocationsAvailable += LocationsNEW.AddAllLocations;
            // ZoneManager.OnVanillaLocationsAvailable += PortPrefabs.AddPortLocation;

            ItemManager.OnItemsRegistered += StatusEffectDB.BuildStatusEffects;
            ItemManager.OnItemsRegistered += ShrineDB.BuildShrineConfigs;
            ItemManager.OnItemsRegistered += WaystoneDB.BuildWaystoneConfigs;

            if (saveOnSet)
            {
                BepinexConfigs.Config.SaveOnConfigSet = saveOnSet;
                BepinexConfigs.Config.Save();
            }
        }
        
        // Add this method to ensure proper initialization order
        private void Initialize()
        {
            More_World_Locations_AIOLogger.LogInfo("Initializing LootDB and CreatureDB...");
    
            LootDB.InitializeLootTables();
            CreatureDB.InitializeCreatureLists();
            Prefabs.LoadPrefabBundles();
            Prefabs.AddAllPrefabs();
            // PortPrefabs.AddPortPrefabs();
    
            More_World_Locations_AIOLogger.LogInfo("LootDB and CreatureDB initialized successfully.");

            PrefabManager.OnVanillaPrefabsAvailable -= Initialize;
        }

        private void OnDestroy()
        {
            BepinexConfigs.Config.Save();
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
                More_World_Locations_AIOLogger.LogDebug("ReadConfigValues called");
                BepinexConfigs.Config.Reload();
            }
            catch
            {
                More_World_Locations_AIOLogger.LogError($"There was an issue loading your {ConfigFileName}");
                More_World_Locations_AIOLogger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
    }
}