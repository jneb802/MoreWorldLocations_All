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
using ServerSync;
using UnityEngine;
using Paths = BepInEx.Paths;

namespace Plains_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plains_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Plains_Pack_1";
        internal const string ModVersion = "1.1.1";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Plains_Pack_1Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

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

            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWL_GoblinFort1_QuantityConfig = config("1 - MWL_GoblinFort1", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_GoblinFort1_CreatureYamlConfig = config("1 - MWL_GoblinFort1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_GoblinFort1_CreatureListConfig = config("1 - MWL_GoblinFort1", "Name of Creature List", "PlainsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_GoblinFort1_LootYamlConfig = config("1 - MWL_GoblinFort1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_GoblinFort1_LootListConfig = config("1 - MWL_GoblinFort1", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_FulingRock1_QuantityConfig = config("2 - MWL_FulingRock1", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_FulingRock1_CreatureYamlConfig = config("2 - MWL_FulingRock1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FulingRock1_CreatureListConfig = config("2 - MWL_FulingRock1", "Name of Creature List", "PlainsCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FulingRock1_LootYamlConfig = config("2 - MWL_FulingRock1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FulingRock1_LootListConfig = config("2 - MWL_FulingRock1", "Name of Loot List", "PlainsLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_FulingVillage1_QuantityConfig = config("3 - MWL_FulingVillage1", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_FulingVillage1_CreatureYamlConfig = config("3 - MWL_FulingVillage1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FulingVillage1_CreatureListConfig = config("3 - MWL_FulingVillage1", "Name of Creature List", "PlainsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FulingVillage1_LootYamlConfig = config("3 - MWL_FulingVillage1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FulingVillage1_LootListConfig = config("3 - MWL_FulingVillage1", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_FulingVillage2_QuantityConfig = config("4 - MWL_FulingVillage2", "Spawn Quantity", 15, 
                "Amount of this location the game will attempt to place during world generation");
            MWL_FulingVillage2_CreatureYamlConfig = config("4 - MWL_FulingVillage2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FulingVillage2_CreatureListConfig = config("4 - MWL_FulingVillage2", "Name of Creature List", "PlainsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FulingVillage2_LootYamlConfig = config("4 - MWL_FulingVillage2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FulingVillage2_LootListConfig = config("4 - MWL_FulingVillage2", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_PlainsPillar1_QuantityConfig = config("5 - MWL_PlainsPillar1", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_PlainsPillar1_CreatureYamlConfig = config("5 - MWL_PlainsPillar1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_PlainsPillar1_CreatureListConfig = config("5 - MWL_PlainsPillar1", "Name of Creature List", "PlainsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_PlainsPillar1_LootYamlConfig = config("5 - MWL_PlainsPillar1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_PlainsPillar1_LootListConfig = config("5 - MWL_PlainsPillar1", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_FulingTemple1_QuantityConfig = config("6 - MWL_FulingTemple1", "Spawn Quantity", 15,
                "Amount of this location the game will attempt to place during world generation");
            MWL_FulingTemple1_CreatureYamlConfig = config("6 - MWL_FulingTemple1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FulingTemple1_CreatureListConfig = config("6 - MWL_FulingTemple1", "Name of Creature List", "PlainsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FulingTemple1_LootYamlConfig = config("6 - MWL_FulingTemple1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FulingTemple1_LootListConfig = config("6 - MWL_FulingTemple1", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_FulingTemple2_QuantityConfig = config("7 - MWL_FulingTemple2", "Spawn Quantity", 20,
                "Amount of this location the game will attempt to place during world generation");
            MWL_FulingTemple2_CreatureYamlConfig = config("7 - MWL_FulingTemple2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FulingTemple2_CreatureListConfig = config("7 - MWL_FulingTemple2", "Name of Creature List", "PlainsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FulingTemple2_LootYamlConfig = config("7 - MWL_FulingTemple2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FulingTemple2_LootListConfig = config("7 - MWL_FulingTemple2", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_FulingTemple3_QuantityConfig = config("8 - MWL_FulingTemple3", "Spawn Quantity", 20,
                "Amount of this location the game will attempt to place during world generation");
            MWL_FulingTemple3_CreatureYamlConfig = config("8 - MWL_FulingTemple3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FulingTemple3_CreatureListConfig = config("8 - MWL_FulingTemple3", "Name of Creature List", "PlainsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FulingTemple3_LootYamlConfig = config("8 - MWL_FulingTemple3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FulingTemple3_LootListConfig = config("8 - MWL_FulingTemple3", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_FulingWall1_QuantityConfig = config("9 - MWL_FulingWall1", "Spawn Quantity", 20,
                "Amount of this location the game will attempt to place during world generation");
            MWL_FulingWall1_CreatureYamlConfig = config("9 - MWL_FulingWall1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FulingWall1_CreatureListConfig = config("9 - MWL_FulingWall1", "Name of Creature List", "PlainsCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FulingWall1_LootYamlConfig = config("9 - MWL_FulingWall1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FulingWall1_LootListConfig = config("9 - MWL_FulingWall1", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_FulingTower1_QuantityConfig = config("10 - MWL_FulingTower1", "Spawn Quantity", 20,
                "Amount of this location the game will attempt to place during world generation");
            MWL_FulingTower1_CreatureYamlConfig = config("10 - MWL_FulingTower1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_FulingTower1_CreatureListConfig = config("10 - MWL_FulingTower1", "Name of Creature List", "PlainsCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_FulingTower1_LootYamlConfig = config("10 - MWL_FulingTower1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_FulingTower1_LootListConfig = config("10 - MWL_FulingTower1", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_GoblinCave1_QuantityConfig = config("10 - MWL_GoblinCave1", "Spawn Quantity", 20,
                "Amount of this location the game will attempt to place during world generation");
            MWL_GoblinCave1_CreatureYamlConfig = config("10 - MWL_GoblinCave1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_GoblinCave1_CreatureListConfig = config("10 - MWL_GoblinCave1", "Name of Creature List", "PlainsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_GoblinCave1_LootYamlConfig = config("10 - MWL_GoblinCave1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_GoblinCave1_LootListConfig = config("10 - MWL_GoblinCave1", "Name of Loot List", "PlainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
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
        
        public static ConfigEntry<int> MWL_GoblinFort1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_GoblinFort1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_GoblinFort1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_GoblinFort1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_GoblinFort1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_FulingRock1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingRock1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingRock1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingRock1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingRock1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_FulingVillage1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingVillage1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingVillage1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingVillage1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingVillage1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_FulingVillage2_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingVillage2_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingVillage2_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingVillage2_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingVillage2_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_PlainsPillar1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_PlainsPillar1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_PlainsPillar1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_PlainsPillar1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_PlainsPillar1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_FulingTemple1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingTemple1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingTemple1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingTemple1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingTemple1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_FulingTemple2_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingTemple2_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingTemple2_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingTemple2_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingTemple2_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_FulingTemple3_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingTemple3_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingTemple3_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingTemple3_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingTemple3_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_FulingWall1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingWall1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingWall1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingWall1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingWall1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_FulingTower1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingTower1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingTower1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_FulingTower1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_FulingTower1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_GoblinCave1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_GoblinCave1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_GoblinCave1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_GoblinCave1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_GoblinCave1_LootListConfig = null!;

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