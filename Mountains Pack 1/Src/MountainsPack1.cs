using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using ServerSync;
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
        internal const string ModVersion = "1.0.0";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Mountains_Pack_1Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

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

            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWL_StoneCastle1_QuantityConfig = config("1 - MWL_StoneCastle1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            MWL_StoneCastle1_CreatureYamlConfig = config("1 - MWL_StoneCastle1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_StoneCastle1_CreatureListConfig = config("1 - MWL_StoneCastle1", "Name of Creature List", "MountainsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_StoneCastle1_LootYamlConfig = config("1 - MWL_StoneCastle1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_StoneCastle1_LootListConfig = config("1 - MWL_StoneCastle1", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_StoneFort1_QuantityConfig = config("2 - MWL_StoneFort1", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_StoneFort1_CreatureYamlConfig = config("2 - MWL_StoneFort1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_StoneFort1_CreatureListConfig = config("2 - MWL_StoneFort1", "Name of Creature List", "MountainsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_StoneFort1_LootYamlConfig = config("2 - MWL_StoneFort1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_StoneFort1_LootListConfig = config("2 - MWL_StoneFort1", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_StoneHall1_QuantityConfig = config("3 - MWL_StoneHall1", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_StoneHall1_CreatureYamlConfig = config("3 - MWL_StoneHall1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_StoneHall1_CreatureListConfig = config("3 - MWL_StoneHall1", "Name of Creature List", "MountainsCreatures4",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_StoneHall1_LootYamlConfig = config("3 - MWL_StoneHall1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_StoneHall1_LootListConfig = config("3 - MWL_StoneHall1", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_StoneTavern1_QuantityConfig = config("4 - MWL_StoneTavern1", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_StoneTavern1_CreatureYamlConfig = config("4 - MWL_StoneTavern1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_StoneTavern1_CreatureListConfig = config("4 - MWL_StoneTavern1", "Name of Creature List", "MountainsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_StoneTavern1_LootYamlConfig = config("4 - MWL_StoneTavern1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_StoneTavern1_LootListConfig = config("4 - MWL_StoneTavern1", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_StoneTower1_QuantityConfig = config("5 - MWL_StoneTower1", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_StoneTower1_CreatureYamlConfig = config("5 - MWL_StoneTower1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_StoneTower1_CreatureListConfig = config("5 - MWL_StoneTower1", "Name of Creature List", "MountainsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_StoneTower1_LootYamlConfig = config("5 - MWL_StoneTower1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_StoneTower1_LootListConfig = config("5 - MWL_StoneTower1", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_StoneTower2_QuantityConfig = config("6 - MWL_StoneTower2", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_StoneTower2_CreatureYamlConfig = config("6 - MWL_StoneTower2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_StoneTower2_CreatureListConfig = config("6 - MWL_StoneTower2", "Name of Creature List", "MountainsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_StoneTower2_LootYamlConfig = config("6 - MWL_StoneTower2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_StoneTower2_LootListConfig = config("6 - MWL_StoneTower2", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_WoodBarn1_QuantityConfig = config("7 - MWL_WoodBarn1", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_WoodBarn1_CreatureYamlConfig = config("7 - MWL_WoodBarn1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_WoodBarn1_CreatureListConfig = config("7 - MWL_WoodBarn1", "Name of Creature List", "MountainsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_WoodBarn1_LootYamlConfig = config("7 - MWL_WoodBarn1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_WoodBarn1_LootListConfig = config("7 - MWL_WoodBarn1", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_WoodFarm1_QuantityConfig = config("8 - MWL_WoodFarm1", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_WoodFarm1_CreatureYamlConfig = config("8 - MWL_WoodFarm1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_WoodFarm1_CreatureListConfig = config("8 - MWL_WoodFarm1", "Name of Creature List", "MountainsCreatures3",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_WoodFarm1_LootYamlConfig = config("8 - MWL_WoodFarm1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_WoodFarm1_LootListConfig = config("8 - MWL_WoodFarm1", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_WoodHouse1_QuantityConfig = config("9 - MWL_WoodHouse1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            MWL_WoodHouse1_CreatureYamlConfig = config("9 - MWL_WoodHouse1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_WoodHouse1_CreatureListConfig = config("9 - MWL_WoodHouse1", "Name of Creature List", "MountainsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_WoodHouse1_LootYamlConfig = config("9 - MWL_WoodHouse1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_WoodHouse1_LootListConfig = config("9 - MWL_WoodHouse1", "Name of Loot List", "MountainsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
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
        
        public static ConfigEntry<int> MWL_StoneCastle1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneCastle1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneCastle1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneCastle1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneCastle1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_StoneFort1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneFort1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneFort1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneFort1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneFort1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_StoneHall1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneHall1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneHall1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneHall1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneHall1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_StoneTavern1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneTavern1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneTavern1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneTavern1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneTavern1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_StoneTower1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneTower1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneTower1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneTower1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneTower1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_StoneTower2_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneTower2_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneTower2_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneTower2_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_StoneTower2_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_WoodBarn1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodBarn1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_WoodBarn1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodBarn1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_WoodBarn1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_WoodFarm1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodFarm1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_WoodFarm1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodFarm1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_WoodFarm1_LootListConfig = null!;
        
        public static ConfigEntry<int> MWL_WoodHouse1_QuantityConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodHouse1_CreatureYamlConfig = null!;
        public static ConfigEntry<string> MWL_WoodHouse1_CreatureListConfig = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_WoodHouse1_LootYamlConfig = null!;
        public static ConfigEntry<string> MWL_WoodHouse1_LootListConfig = null!;

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