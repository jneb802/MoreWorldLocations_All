using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Adventure_Map_Pack_1.Utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BlackForest_Pack_1;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using UnityEngine;
using Common;
using Jotunn.Managers;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

namespace Adventure_Map_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Adventure_Map_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Adventure_Map_Pack_1";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Adventure_Map_Pack_1Logger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);
        
        public static AssetBundle assetBundle;
        public static string bundleName = "adventuremap1";
        
        public static AssetBundle assetBundleTotem;
        public static string bundleNameTotem = "totems";
        
        public static YAMLManager adventuremap1YAMLmanager = new YAMLManager();
        
        public static void LoadAssetBundle()
        {
            assetBundle = AssetUtils.LoadAssetBundleFromResources(
                bundleName,
                Assembly.GetExecutingAssembly()
            );
            
            // assetBundleTotem = AssetUtils.LoadAssetBundleFromResources(
            //     bundleNameTotem,
            //     Assembly.GetExecutingAssembly()
            // );
            if (assetBundle == null)
            {
                WarpLogger.Logger.LogError("Failed to load asset bundle with name: " + bundleName);
            }
        }

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
        
            MWL_CastleCorner1_Configuration =
                new LocationConfiguration(this.Config, "CastleCorner1", 15, "SwampCreatures1", "SwampLoot1");
            MWL_ForestCamp1_Configuration =
                new LocationConfiguration(this.Config, "ForestCamp1", 20, "BlackforestCreatures1", "BlackforestLoot1");
            MWL_Misthut2_Configuration =
                new LocationConfiguration(this.Config, "Misthut2", 15, "MistCreatures1", "MistLoot1");
            MWL_MountainDvergrShrine1_Configuration =
                new LocationConfiguration(this.Config, "MountainDvergrShrine1", 15, "MistCreatures1", "MistLoot1");
            MWL_MountainShrine1_Configuration =
                new LocationConfiguration(this.Config, "MountainShrine1", 20, "MountainCreatures1", "MountainsLoot1");
            MWL_RuinedTower1_Configuration =
                new LocationConfiguration(this.Config, "RuinedTower1", 15, "BlackforestCreatures2", "BlackforestLoot1");
            MWL_TreeTowers1_Configuration =
                new LocationConfiguration(this.Config, "TreeTowers1", 20, "SwampCreatures1", "SwampLoot1");
            
    //         MWL_CastleCorner1_Quantity_Config = config("1 - MWL_CastleCorner1", "Spawn Quantity", 15,
    //             "Amount of this location the game will attempt to place during world generation");
    //         MWL_CastleCorner1_CreatureYaml_Config = config("1 - MWL_CastleCorner1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
    //         MWL_CastleCorner1_CreatureList_Config = config("1 - MWL_CastleCorner1", "Name of Creature List", "SwampCreatures1",
    //             "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
    //         MWL_CastleCorner1_LootYaml_Config = config("1 - MWL_CastleCorner1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
    //         MWL_CastleCorner1_LootList_Config = config("1 - MWL_CastleCorner1", "Name of Loot List", "SwampLoot1",
    //             "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
    //         
    //         MWL_ForestCamp1_Quantity_Config = config("2 - MWL_ForestCamp1", "Spawn Quantity", 20,
    // "Amount of this location the game will attempt to place during world generation");
    //         MWL_ForestCamp1_CreatureYaml_Config = config("2 - MWL_ForestCamp1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
    //         MWL_ForestCamp1_CreatureList_Config = config("2 - MWL_ForestCamp1", "Name of Creature List", "BlackforestCreatures1",
    //             "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
    //         MWL_ForestCamp1_LootYaml_Config = config("2 - MWL_ForestCamp1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
    //         MWL_ForestCamp1_LootList_Config = config("2 - MWL_ForestCamp1", "Name of Loot List", "BlackforestLoot1",
    //             "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
    //
    //         MWL_Misthut2_Quantity_Config = config("3 - MWL_Misthut2", "Spawn Quantity", 15,
    //             "Amount of this location the game will attempt to place during world generation");
    //         MWL_Misthut2_CreatureYaml_Config = config("3 - MWL_Misthut2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
    //         MWL_Misthut2_CreatureList_Config = config("3 - MWL_Misthut2", "Name of Creature List", "MistCreatures1",
    //             "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
    //         MWL_Misthut2_LootYaml_Config = config("3 - MWL_Misthut2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
    //         MWL_Misthut2_LootList_Config = config("3 - MWL_Misthut2", "Name of Loot List", "MistLoot1",
    //             "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
    //
    //         MWL_MountainDvergrShrine1_Quantity_Config = config("4 - MWL_MountainDvergrShrine1", "Spawn Quantity", 15,
    //             "Amount of this location the game will attempt to place during world generation");
    //         MWL_MountainDvergrShrine1_CreatureYaml_Config = config("4 - MWL_MountainDvergrShrine1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
    //         MWL_MountainDvergrShrine1_CreatureList_Config = config("4 - MWL_MountainDvergrShrine1", "Name of Creature List", "MistCreatures1",
    //             "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
    //         MWL_MountainDvergrShrine1_LootYaml_Config = config("4 - MWL_MountainDvergrShrine1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
    //         MWL_MountainDvergrShrine1_LootList_Config = config("4 - MWL_MountainDvergrShrine1", "Name of Loot List", "MistLoot1",
    //             "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
    //         
    //         MWL_MountainDvergrShrine2_Quantity_Config = config("4 - MWL_MountainDvergrShrine2", "Spawn Quantity", 15,
    //             "Amount of this location the game will attempt to place during world generation");
    //         MWL_MountainDvergrShrine2_CreatureYaml_Config = config("4 - MWL_MountainDvergrShrine2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
    //         MWL_MountainDvergrShrine2_CreatureList_Config = config("4 - MWL_MountainDvergrShrine2", "Name of Creature List", "MistCreatures1",
    //             "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
    //         MWL_MountainDvergrShrine2_LootYaml_Config = config("4 - MWL_MountainDvergrShrine2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
    //         MWL_MountainDvergrShrine2_LootList_Config = config("4 - MWL_MountainDvergrShrine2", "Name of Loot List", "MistLoot1",
    //             "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
    //
    //         MWL_MountainShrine1_Quantity_Config = config("5 - MWL_MountainShrine1", "Spawn Quantity", 10,
    //             "Amount of this location the game will attempt to place during world generation");
    //         MWL_MountainShrine1_CreatureYaml_Config = config("5 - MWL_MountainShrine1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
    //         MWL_MountainShrine1_CreatureList_Config = config("5 - MWL_MountainShrine1", "Name of Creature List", "MountainCreatures1",
    //             "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
    //         MWL_MountainShrine1_LootYaml_Config = config("5 - MWL_MountainShrine1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
    //         MWL_MountainShrine1_LootList_Config = config("5 - MWL_MountainShrine1", "Name of Loot List", "MountainsLoot1",
    //             "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
    //
    //         MWL_RuinedTower1_Quantity_Config = config("6 - MWL_RuinedTower1", "Spawn Quantity", 15,
    //             "Amount of this location the game will attempt to place during world generation");
    //         MWL_RuinedTower1_CreatureYaml_Config = config("6 - MWL_RuinedTower1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
    //         MWL_RuinedTower1_CreatureList_Config = config("6 - MWL_RuinedTower1", "Name of Creature List", "BlackforestCreatures2",
    //             "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
    //         MWL_RuinedTower1_LootYaml_Config = config("6 - MWL_RuinedTower1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
    //         MWL_RuinedTower1_LootList_Config = config("6 - MWL_RuinedTower1", "Name of Loot List", "BlackforestLoot1",
    //             "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
    //
    //         MWL_TreeTowers1_Quantity_Config = config("7 - MWL_TreeTowers1", "Spawn Quantity", 20,
    //             "Amount of this location the game will attempt to place during world generation");
    //         MWL_TreeTowers1_CreatureYaml_Config = config("7 - MWL_TreeTowers1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
    //         MWL_TreeTowers1_CreatureList_Config = config("7 - MWL_TreeTowers1", "Name of Creature List", "SwampCreatures1",
    //             "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
    //         MWL_TreeTowers1_LootYaml_Config = config("7 - MWL_TreeTowers1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
    //             "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
    //         MWL_TreeTowers1_LootList_Config = config("7 - MWL_TreeTowers1", "Name of Loot List", "SwampLoot1",
    //             "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            adventuremap1YAMLmanager.ParseDefaultYamls();
            adventuremap1YAMLmanager.ParseCustomYamls();
            
            // PrefabManager.OnVanillaPrefabsAvailable += PrefabUtils.CreateCustomPrefabs;
            // StatusEffectUtils.CreateCustomStatusEffects();
            
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
                Adventure_Map_Pack_1Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Adventure_Map_Pack_1Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Adventure_Map_Pack_1Logger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
        public static LocationConfiguration MWL_CastleCorner1_Configuration;
        public static LocationConfiguration MWL_ForestCamp1_Configuration;
        public static LocationConfiguration MWL_Misthut2_Configuration;
        public static LocationConfiguration MWL_MountainDvergrShrine1_Configuration;
        public static LocationConfiguration MWL_MountainShrine1_Configuration;
        public static LocationConfiguration MWL_RuinedTower1_Configuration;
        public static LocationConfiguration MWL_TreeTowers1_Configuration;
        
        // public static ConfigEntry<int> MWL_CastleCorner1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_CastleCorner1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_CastleCorner1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_CastleCorner1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_CastleCorner1_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_ForestCamp1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestCamp1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestCamp1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_ForestCamp1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_ForestCamp1_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_Misthut2_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_Misthut2_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_Misthut2_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_Misthut2_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_Misthut2_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_MountainDvergrShrine1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_MountainDvergrShrine1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_MountainDvergrShrine1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_MountainDvergrShrine1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_MountainDvergrShrine1_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_MountainDvergrShrine2_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_MountainDvergrShrine2_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_MountainDvergrShrine2_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_MountainDvergrShrine2_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_MountainDvergrShrine2_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_MountainShrine1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_MountainShrine1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_MountainShrine1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_MountainShrine1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_MountainShrine1_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_RuinedTower1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinedTower1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_RuinedTower1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_RuinedTower1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_RuinedTower1_LootList_Config = null!;
        //
        // public static ConfigEntry<int> MWL_TreeTowers1_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_TreeTowers1_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWL_TreeTowers1_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_TreeTowers1_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWL_TreeTowers1_LootList_Config = null!;
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