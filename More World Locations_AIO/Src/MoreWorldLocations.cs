using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Common;
using HarmonyLib;
using Jotunn.Managers;
using More_World_Locations_AIO.Shipments;
using More_World_Locations_AIO.Shrines;
using More_World_Locations_AIO.Utils;
using More_World_Locations_AIO.Traders;
using More_World_Locations_AIO.Waystones;
using UnityEngine;

namespace More_World_Locations_AIO
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class More_World_Locations_AIOPlugin : BaseUnityPlugin
    {
        internal const string ModName = "More_World_Locations_AIO";
        internal const string ModVersion = "4.1.1";
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
            
            PortInit.Init(root);
            BepinexConfigs.BindFeatureConfigs();

            YAMLManager.ParseTraderYaml("warpalicious.More_World_Locations_TraderItems.yml", (Common.ConfigurationManager.Toggle)BepinexConfigs.UseCustomTraderConfigs.Value);
            YAMLManager.ParseDefaultYamls();
            YAMLManager.ParseCustomYamls((Common.ConfigurationManager.Toggle)BepinexConfigs.UseCustomLocationYAML.Value);

            UpgradeWorldCommands.AddUpgradeWorldCommands();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();

            Prefabs.LoadPrefabBundles();
            PortPrefabs.LoadPrefabBundles();
            
            // Trader setup
            MinimapTraderIcons.LoadIcons();
            MinimapTraderIcons.BuildLocationSpriteData();
            MWLLocalizations.Load(BepinexConfigs.UseCustomLocalization.Value);
            
            // AssetBundles.BuildCombinedManifest(
            //     Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", "Bundles"), 
            //     "full",
            //     AssetPaths.assetPathsInBundleFull
            // );   
            
            LocationQuantityManager.LoadOrMigrateConfigs(Config);
            
            PrefabManager.OnVanillaPrefabsAvailable += Initialize;
            ZoneManager.OnVanillaLocationsAvailable += LocationsNEW.AddAllLocations;

            ItemManager.OnItemsRegistered += StatusEffectDB.BuildStatusEffects;
            ItemManager.OnItemsRegistered += ShrineDB.BuildShrineConfigs;
            ItemManager.OnItemsRegistered += WaystoneDB.BuildWaystoneConfigs;

            if (saveOnSet)
            {
                BepinexConfigs.Config.SaveOnConfigSet = saveOnSet;
                BepinexConfigs.Config.Save();
            }

            Analytics.Init(Config, ModGUID, ModVersion);
        }
        
        // Add this method to ensure proper initialization order
        private void Initialize()
        {
            More_World_Locations_AIOLogger.LogInfo("Initializing LootDB and CreatureDB...");

            LootDB.InitializeLootTables();
            CreatureDB.InitializeCreatureLists();

            // Load YAML overrides if enabled
            if (BepinexConfigs.UseCustomLocationYAML.Value == PortInit.Toggle.On)
            {
                More_World_Locations_AIOLogger.LogInfo("Loading custom YAML configurations for creatures and loot...");
                string creatureYaml = YAMLManager.GetCreatureYamlContent((Common.ConfigurationManager.Toggle)BepinexConfigs.UseCustomLocationYAML.Value);
                string lootYaml = YAMLManager.GetLootYamlContent((Common.ConfigurationManager.Toggle)BepinexConfigs.UseCustomLocationYAML.Value);

                CreatureDB.LoadFromYAML(creatureYaml);
                LootDB.LoadFromYAML(lootYaml);
            }

            Prefabs.AddAllPrefabs();
            LocationCustomPrefabs.AddMarbleJail1Prefabs();
            LocationCustomPrefabs.AddMarbleCliffAltar1Prefabs();
            PortPrefabs.AddPortPrefabs();

            if (BepinexConfigs.EnableTraders.Value == PortInit.Toggle.On ||
                BepinexConfigs.EnableTrainers.Value == PortInit.Toggle.On)
            {
                TraderItems.CreateCustomItems();
            }

            if (BepinexConfigs.EnableTraders.Value == PortInit.Toggle.On)
            {
                TraderPrefabs.AddTraderPrefabs();
            }

            if (BepinexConfigs.EnableTrainers.Value == PortInit.Toggle.On)
            {
                TraderPrefabs.AddTrainerPrefabs();
            }

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