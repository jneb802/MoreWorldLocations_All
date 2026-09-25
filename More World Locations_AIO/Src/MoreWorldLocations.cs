using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Common;
using HarmonyLib;
using Jotunn.Managers;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.Shipments;
using More_World_Locations_AIO.Shrines;
using More_World_Locations_AIO.Utils;
using More_World_Locations_AIO.Dungeons;
using More_World_Locations_AIO.Dungeons.Packs;
using More_World_Locations_AIO.Traders;
using More_World_Locations_AIO.Waystones;
using UnityEngine;

namespace More_World_Locations_AIO
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class More_World_Locations_AIOPlugin : BaseUnityPlugin
    {
        internal const string ModName = "More_World_Locations_AIO";
        internal const string ModVersion = "5.1.2";
        internal const string Author = "warpalicious";
        internal const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        
        public static readonly ManualLogSource More_World_Locations_AIOLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);
        
        public static YAMLManager YAMLManager = new YAMLManager();
        
        public static GameObject root = null!;
        
        private static readonly System.Version MinJotunnVersion = new System.Version(2, 28, 0);

        private const string MoreWorldTradersGUID = "warpalicious.More_World_Traders";

        public void Start()
        {
            if (Chainloader.PluginInfos.ContainsKey(MoreWorldTradersGUID))
            {
                More_World_Locations_AIOLogger.LogError(
                    "More World Traders (warpalicious.More_World_Traders) is loaded alongside More World Locations AIO. " +
                    "Traders are fully integrated into More World Locations AIO as of version 4.0.0. " +
                    "Please remove the More World Traders mod.");
            }
        }

        public void Awake()
        {
            Analytics.Init(Config, ModGUID, ModVersion);

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

            // Read once, here, and let every other decision ask the flag. This
            // runs long before ZNet exists, so no peer can connect against a
            // mode that is still unset.
            ServerOnlyMode.Set(BepinexConfigs.ServerOnly.Value == PortInit.Toggle.On);
            if (ServerOnlyMode.Enabled)
            {
                // Before RegisterAll runs, because the audit it performs has to
                // be able to open a template that is not registered -- which is
                // every template it has not yet approved.
                More_World_Locations_AIO.ServerOnly.Verification.GameTemplateAssets.Install();
                More_World_Locations_AIOLogger.LogInfo(
                    "Server-only mode: clients without More World Locations are admitted, " +
                    "and only locations this run's audit approves are registered.");
            }
            
            YAMLManager.ParseTraderYaml("warpalicious.More_World_Locations_TraderItems.yml", (ConfigurationManager.Toggle)BepinexConfigs.UseCustomTraderConfigs.Value);

            UpgradeWorldCommands.AddUpgradeWorldCommands();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            if (ServerOnlyMode.Enabled)
            {
                // Before any template is resolved: the audit opens every
                // template in the catalogue, and an unguarded resolution of one
                // with an unresolvable mock moves objects into vanilla prefabs.
                More_World_Locations_AIO.ServerOnly.Verification.MockReferenceGuardPatch.Install(_harmony);
                GenerationHold.Install();
                // Jötunn injects its list into the world when the sweep starts;
                // what the sweep approves is put into the world through Jötunn's
                // own late path, which prepares the location the same way.
                LocationDB.LateRegistration = name =>
                {
                    Jotunn.Entities.CustomLocation custom = ZoneManager.Instance.GetCustomLocation(name);
                    ZoneManager.Instance.RegisterLocationInZoneSystem(custom.ZoneLocation);
                };
            }
            SetupWatcher();

            Prefabs.LoadPrefabBundles();
            PortPrefabs.LoadPrefabBundles();
            
            // Trader setup
            MinimapTraderIcons.LoadIcons();
            MinimapTraderIcons.BuildLocationSpriteData();

            // Uncomment for one local game run to regenerate assetBundleManifest_full.
            // AssetBundles.BuildCombinedManifest(
            //     Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", "Bundles"),
            //     "full",
            //     LocationDB.GetAllAssetPaths().Concat(RoomDB.GetAllAssetPaths()).ToArray()
            // );

            LocationQuantityManager.LoadOrMigrateConfigs(Config);
            
            PrefabManager.OnVanillaPrefabsAvailable += Initialize;
            ZoneManager.OnVanillaLocationsAvailable += LocationDB.RegisterAll;
            DungeonManager.OnVanillaRoomsAvailable += RoomDB.RegisterAll;

            ItemManager.OnItemsRegistered += StatusEffectDB.BuildStatusEffects;
            ItemManager.OnItemsRegistered += ShrineDB.BuildShrineConfigs;
            ItemManager.OnItemsRegistered += WaystoneDB.BuildWaystoneConfigs;

            try
            {
                Assembly jotunnAssembly = typeof(Jotunn.Main).Assembly;
                System.Version jotunnVersion = jotunnAssembly.GetName().Version;
                if (jotunnVersion.CompareTo(MinJotunnVersion) < 0)
                {
                    More_World_Locations_AIOLogger.LogError(
                        $"More World Locations requires Jotunn {MinJotunnVersion} or newer, but found {jotunnVersion}. " +
                        "Please update Jotunn or the text in the mod will be broken!");
                }

                MWLLocalizations.Load(BepinexConfigs.UseCustomLocalization.Value);
            }
            catch (Exception ex)
            {
                More_World_Locations_AIOLogger.LogError(
                    $"Failed to load YAML localizations: {ex.Message}. " +
                    "Localized text will show as raw tokens. Please update Jotunn to 2.28.0 or newer.");
            }
            
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
            
            Prefabs.AddAllPrefabs();
            DungeonPackDB.RegisterAll();
            LocationCustomPrefabs.AddMarbleJail1Prefabs();
            LocationCustomPrefabs.AddMarbleCliffAltar1Prefabs();
            LocationCustomPrefabs.AddPortRunestonePrefabs();
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
