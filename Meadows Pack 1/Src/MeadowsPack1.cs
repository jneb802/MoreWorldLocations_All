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
using Jotunn.Utils;
using UnityEngine;
using Paths = BepInEx.Paths;
using Common;

namespace Meadows_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[BepInDependency("kg.marketplace", BepInDependency.DependencyFlags.SoftDependency)]
    public class Meadows_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Meadows_Pack_1";
        internal const string ModVersion = "1.1.1";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        
        public static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static YAMLManager meadowsYAMLManager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "meadowspack1";

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
            bool saveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet = false; // This and the variable above are used to prevent the config from saving on startup for each config entry. This is speeds up the startup process.
            
            //_serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            //_ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();

            LoadAssetBundle();
            
            MWL_Ruins1_Quantity_Config = config("1 - MWL_Ruins1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            MWL_Ruins1_CreatureYaml_Config = config("1 - MWL_Ruins1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_Ruins1_CreatureList_Config = config("1 - MWL_Ruins1", "Name of Creature List", "MeadowsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_Ruins1_LootYaml_Config = config("1 - MWL_Ruins1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_Ruins1_LootList_Config = config("1 - MWL_Ruins1", "Name of Loot List", "MeadowsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_Ruins2_Quantity_Config = config("2 - MWL_Ruins2", "Spawn Quantity", 10,
                "Amount of this location the game will attempt to place during world generation");
            MWL_Ruins2_CreatureYaml_Config = config("2 - MWL_Ruins2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_Ruins2_CreatureList_Config = config("2 - MWL_Ruins2", "Name of Creature List", "MeadowsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_Ruins2_LootYaml_Config = config("2 - MWL_Ruins2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_Ruins2_LootList_Config = config("2 - MWL_Ruins2", "Name of Loot List", "MeadowsLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_Ruins3_Quantity_Config = config("3 - MWL_Ruins3", "Spawn Quantity", 25,
                "Amount of this location the game will attempt to place during world generation");
            MWL_Ruins3_CreatureYaml_Config = config("3 - MWL_Ruins3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_Ruins3_CreatureList_Config = config("3 - MWL_Ruins3", "Name of Creature List", "MeadowsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_Ruins3_LootYaml_Config = config("3 - MWL_Ruins3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_Ruins3_LootList_Config = config("3 - MWL_Ruins3", "Name of Loot List", "MeadowsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            
            MWL_Ruins6_Quantity_Config = config("4 - MWL_Ruins6", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            MWL_Ruins6_CreatureYaml_Config = config("4 - MWL_Ruins6", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_Ruins6_CreatureList_Config = config("4 - MWL_Ruins6", "Name of Creature List", "MeadowsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_Ruins6_LootYaml_Config = config("4 - MWL_Ruins6", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_Ruins6_LootList_Config = config("4 - MWL_Ruins6", "Name of Loot List", "MeadowsLoot3",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");

            MWL_Ruins7_Quantity_Config = config("5 - MWL_Ruins7", "Spawn Quantity", 2,
                "Amount of this location the game will attempt to place during world generation");
            MWL_Ruins7_CreatureYaml_Config = config("5 - MWL_Ruins7", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_Ruins7_CreatureList_Config = config("5 - MWL_Ruins7", "Name of Creature List", "MeadowsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_Ruins7_LootYaml_Config = config("5 - MWL_Ruins7", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_Ruins7_LootList_Config = config("5 - MWL_Ruins7", "Name of Loot List", "MeadowsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_Ruins8_Quantity_Config = config("6 - MWL_Ruins8", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            MWL_Ruins8_CreatureYaml_Config = config("6 - MWL_Ruins8", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_Ruins8_CreatureList_Config = config("6 - MWL_Ruins8", "Name of Creature List", "MeadowsCreatures2",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_Ruins8_LootYaml_Config = config("6 - MWL_Ruins8", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_Ruins8_LootList_Config = config("6 - MWL_Ruins8", "Name of Loot List", "MeadowsLoot3",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");

            MWL_RuinsArena1_Quantity_Config = config("7 - MWL_RuinsArena1", "Spawn Quantity", 25,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsArena1_CreatureYaml_Config = config("7 - MWL_RuinsArena1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsArena1_CreatureList_Config = config("7 - MWL_RuinsArena1", "Name of Creature List", "MeadowsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsArena1_LootYaml_Config = config("7 - MWL_RuinsArena1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsArena1_LootList_Config = config("7 - MWL_RuinsArena1", "Name of Loot List", "MeadowsLoot3",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_RuinsArena3_Quantity_Config = config("8 - MWL_RuinsArena3", "Spawn Quantity", 25,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsArena3_CreatureYaml_Config = config("8 - MWL_RuinsArena3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsArena3_CreatureList_Config = config("8 - MWL_RuinsArena3", "Name of Creature List", "MeadowsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsArena3_LootYaml_Config = config("8 - MWL_RuinsArena3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsArena3_LootList_Config = config("8 - MWL_RuinsArena3", "Name of Loot List", "MeadowsLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");

            MWL_RuinsChurch1_Quantity_Config = config("9 - MWL_RuinsChurch1", "Spawn Quantity", 25,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsChurch1_CreatureYaml_Config = config("9 - MWL_RuinsChurch1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsChurch1_CreatureList_Config = config("9 - MWL_RuinsChurch1", "Name of Creature List", "MeadowsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsChurch1_LootYaml_Config = config("9 - MWL_RuinsChurch1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsChurch1_LootList_Config = config("9 - MWL_RuinsChurch1", "Name of Loot List", "MeadowsLoot2",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWL_RuinsWell1_Quantity_Config = config("10 - MWL_RuinsWell1", "Spawn Quantity", 5,
                "Amount of this location the game will attempt to place during world generation");
            MWL_RuinsWell1_CreatureYaml_Config = config("10 - MWL_RuinsWell1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            MWL_RuinsWell1_CreatureList_Config = config("10 - MWL_RuinsWell1", "Name of Creature List", "MeadowsCreatures1",
                "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            MWL_RuinsWell1_LootYaml_Config = config("10 - MWL_RuinsWell1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
                "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            MWL_RuinsWell1_LootList_Config = config("10 - MWL_RuinsWell1", "Name of Loot List", "MeadowsLoot1",
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            /*UseCustomLocationCreatureListYAML = config("2 - Custom Location YAML files",
                "Use Custom Location Creature List", Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will spawn creatures from the warpalicious." + ModName + " file in BepinEx config folder");
            UseCustomLocationLootListYAML = config("2 - Custom Location YAML files",
                "Use Custom Location Loot List", Toggle.Off,
                "When Off, location will use default loot. When On, location will use custom loot list from the warpalicious." + ModName + " file in BepinEx config folder");*/
            
            meadowsYAMLManager.ParseDefaultYamls();
            meadowsYAMLManager.ParseCustomYamls();
            
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
        
        public static ConfigEntry<int> MWL_Ruins1_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins1_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins1_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins1_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins1_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_Ruins2_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins2_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins2_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins2_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins2_LootList_Config = null!;
        
        
        public static ConfigEntry<int> MWL_Ruins3_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins3_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins3_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins3_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins3_LootList_Config = null!;
        
        
        public static ConfigEntry<int> MWL_Ruins6_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins6_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins6_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins6_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins6_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_Ruins7_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins7_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins7_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins7_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins7_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_Ruins8_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins8_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins8_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_Ruins8_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_Ruins8_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_RuinsArena1_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsArena1_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsArena1_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsArena1_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsArena1_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_RuinsArena3_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsArena3_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsArena3_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsArena3_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsArena3_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_RuinsChurch1_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsChurch1_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsChurch1_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsChurch1_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsChurch1_LootList_Config = null!;
        
        public static ConfigEntry<int> MWL_RuinsWell1_Quantity_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsWell1_CreatureYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsWell1_CreatureList_Config = null!;
        public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinsWell1_LootYaml_Config = null!;
        public static ConfigEntry<string> MWL_RuinsWell1_LootList_Config = null!;
        
        
        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Logger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
        /*public enum Toggle
        {
            On = 1,
            Off = 0 
        }*/

        //private static ConfigEntry<Toggle> _serverConfigLocked = null!;

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

           //SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
           //syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

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
        
    }
}