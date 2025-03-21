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
        internal const string ModVersion = "1.1.6";
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
            
        //     MWL_ForestForge1_Quantity_Config = config("1 - MWL_ForestForge1", "Spawn Quantity", 20,
        // "Amount of this location the game will attempt to place during world generation");
        //     MWL_ForestForge1_CreatureYaml_Config = config("1 - MWL_ForestForge1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_ForestForge1_CreatureList_Config = config("1 - MWL_ForestForge1", "Name of Creature List", "BlackforestCreatures3",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_ForestForge1_LootYaml_Config = config("1 - MWL_ForestForge1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_ForestForge1_LootList_Config = config("1 - MWL_ForestForge1", "Name of Loot List", "BlackforestLoot3",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        //
        //     MWL_ForestForge2_Quantity_Config = config("2 - MWL_ForestForge2", "Spawn Quantity", 20,
        //         "Amount of this location the game will attempt to place during world generation");
        //     MWL_ForestForge2_CreatureYaml_Config = config("2 - MWL_ForestForge2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_ForestForge2_CreatureList_Config = config("2 - MWL_ForestForge2", "Name of Creature List", "BlackforestCreatures3",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_ForestForge2_LootYaml_Config = config("2 - MWL_ForestForge2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_ForestForge2_LootList_Config = config("2 - MWL_ForestForge2", "Name of Loot List", "BlackforestLoot3",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        //
        //     MWL_ForestGreatHouse2_Quantity_Config = config("3 - MWL_ForestGreatHouse2", "Spawn Quantity", 20,
        //         "Amount of this location the game will attempt to place during world generation");
        //     MWL_ForestGreatHouse2_CreatureYaml_Config = config("3 - MWL_ForestGreatHouse2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_ForestGreatHouse2_CreatureList_Config = config("3 - MWL_ForestGreatHouse2", "Name of Creature List", "BlackforestCreatures3",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_ForestGreatHouse2_LootYaml_Config = config("3 - MWL_ForestGreatHouse2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_ForestGreatHouse2_LootList_Config = config("3 - MWL_ForestGreatHouse2", "Name of Loot List", "BlackforestLoot3",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        //
        //     MWL_ForestHouse2_Quantity_Config = config("4 - MWL_ForestHouse2", "Spawn Quantity", 20,
        //         "Amount of this location the game will attempt to place during world generation");
        //     MWL_ForestHouse2_CreatureYaml_Config = config("4 - MWL_ForestHouse2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_ForestHouse2_CreatureList_Config = config("4 - MWL_ForestHouse2", "Name of Creature List", "BlackforestCreatures3",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_ForestHouse2_LootYaml_Config = config("4 - MWL_ForestHouse2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_ForestHouse2_LootList_Config = config("4 - MWL_ForestHouse2", "Name of Loot List", "BlackforestLoot3",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        //
        //     MWL_ForestRuin1_Quantity_Config = config("5 - MWL_ForestRuin1", "Spawn Quantity", 20,
        //         "Amount of this location the game will attempt to place during world generation");
        //     MWL_ForestRuin1_CreatureYaml_Config = config("5 - MWL_ForestRuin1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_ForestRuin1_CreatureList_Config = config("5 - MWL_ForestRuin1", "Name of Creature List", "BlackforestCreatures3",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_ForestRuin1_LootYaml_Config = config("5 - MWL_ForestRuin1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_ForestRuin1_LootList_Config = config("5 - MWL_ForestRuin1", "Name of Loot List", "BlackforestLoot1",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        //
        //     MWL_ForestTower2_Quantity_Config = config("6 - MWL_ForestTower2", "Spawn Quantity", 20,
        //         "Amount of this location the game will attempt to place during world generation");
        //     MWL_ForestTower2_CreatureYaml_Config = config("6 - MWL_ForestTower2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_ForestTower2_CreatureList_Config = config("6 - MWL_ForestTower2", "Name of Creature List", "BlackforestCreatures1",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_ForestTower2_LootYaml_Config = config("6 - MWL_ForestTower2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_ForestTower2_LootList_Config = config("6 - MWL_ForestTower2", "Name of Loot List", "BlackforestLoot3",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        //
        //     MWL_ForestTower3_Quantity_Config = config("7 - MWL_ForestTower3", "Spawn Quantity", 20,
        //         "Amount of this location the game will attempt to place during world generation");
        //     MWL_ForestTower3_CreatureYaml_Config = config("7 - MWL_ForestTower3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_ForestTower3_CreatureList_Config = config("7 - MWL_ForestTower3", "Name of Creature List", "BlackforestCreatures1",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_ForestTower3_LootYaml_Config = config("7 - MWL_ForestTower3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_ForestTower3_LootList_Config = config("7 - MWL_ForestTower3", "Name of Loot List", "BlackforestLoot3",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        //
        //     MWL_MassGrave1_Quantity_Config = config("8 - MWL_MassGrave1", "Spawn Quantity", 15,
        //         "Amount of this location the game will attempt to place during world generation");
        //     MWL_MassGrave1_CreatureYaml_Config = config("8 - MWL_MassGrave1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_MassGrave1_CreatureList_Config = config("8 - MWL_MassGrave1", "Name of Creature List", "BlackforestCreatures3",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_MassGrave1_LootYaml_Config = config("8 - MWL_MassGrave1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_MassGrave1_LootList_Config = config("8 - MWL_MassGrave1", "Name of Loot List", "BlackforestLoot1",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        //
        //     MWL_StoneFormation1_Quantity_Config = config("9 - MWL_StoneFormation1", "Spawn Quantity", 15,
        //         "Amount of this location the game will attempt to place during world generation");
        //     MWL_StoneFormation1_CreatureYaml_Config = config("9 - MWL_StoneFormation1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
        //     MWL_StoneFormation1_CreatureList_Config = config("9 - MWL_StoneFormation1", "Name of Creature List", "BlackforestCreatures3",
        //         "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
        //     MWL_StoneFormation1_LootYaml_Config = config("9 - MWL_StoneFormation1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
        //         "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
        //     MWL_StoneFormation1_LootList_Config = config("9 - MWL_StoneFormation1", "Name of Loot List", "BlackforestLoot3",
        //         "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
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
        
        // public static ConfigEntry<int> MWL_ForestForge1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestForge1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestForge1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestForge1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestForge1_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_ForestForge2_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestForge2_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestForge2_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestForge2_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestForge2_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_ForestGreatHouse2_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestGreatHouse2_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestGreatHouse2_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestGreatHouse2_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestGreatHouse2_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_ForestHouse2_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestHouse2_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestHouse2_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestHouse2_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestHouse2_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_ForestRuin1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestRuin1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestRuin1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestRuin1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestRuin1_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_ForestTower2_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestTower2_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestTower2_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestTower2_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestTower2_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_ForestTower3_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestTower3_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestTower3_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestTower3_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestTower3_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_MassGrave1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_MassGrave1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_MassGrave1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_MassGrave1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_MassGrave1_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_StoneFormation1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneFormation1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_StoneFormation1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_StoneFormation1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_StoneFormation1_LootList_Config = null!;


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