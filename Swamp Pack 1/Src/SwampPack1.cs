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
using UnityEngine;
using Common;
using Jotunn.Managers;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

namespace Swamp_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Swamp_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Swamp_Pack_1";
        internal const string ModVersion = "1.1.3";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Swamp_Pack_1Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

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
        
        public static YAMLManager swampYAMLmanager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "swamppack1";

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

            MWL_GuckPit1_Configuration =
                new LocationConfiguration(this.Config, "GuckPit1", 15, "SwampCreatures1", "SwampLoot1");
            MWL_SwampAltar1_Configuration =
                new LocationConfiguration(this.Config, "SwampAltar1", 15, "SwampCreatures3", "SwampLoot1");
            MWL_SwampAltar2_Configuration =
                new LocationConfiguration(this.Config, "SwampAltar2", 10, "SwampCreatures3", "SwampLoot2");
            MWL_SwampAltar3_Configuration =
                new LocationConfiguration(this.Config, "SwampAltar3", 10, "SwampCreatures3", "SwampLoot2");
            MWL_SwampAltar4_Configuration =
                new LocationConfiguration(this.Config, "SwampAltar4", 10, "SwampCreatures2", "SwampLoot2");
            MWL_SwampCastle2_Configuration =
                new LocationConfiguration(this.Config, "SwampCastle2", 10, "SwampCreatures2", "SwampLoot3");
            MWL_SwampGrave1_Configuration =
                new LocationConfiguration(this.Config, "SwampGrave1", 25, "SwampCreatures2", "SwampLoot1");
            MWL_SwampHouse1_Configuration =
                new LocationConfiguration(this.Config, "SwampHouse1", 20, "SwampCreatures3", "SwampLoot1");
            MWL_SwampRuin1_Configuration =
                new LocationConfiguration(this.Config, "SwampRuin1", 25, "SwampCreatures3", "SwampLoot1");
            MWL_SwampTower1_Configuration =
                new LocationConfiguration(this.Config, "SwampTower1", 20, "SwampCreatures3", "SwampLoot1");
            MWL_SwampTower2_Configuration =
                new LocationConfiguration(this.Config, "SwampTower2", 25, "SwampCreatures4", "SwampLoot3");
            MWL_SwampTower3_Configuration =
                new LocationConfiguration(this.Config, "SwampTower3", 25, "SwampCreatures2", "SwampLoot1");
            MWL_SwampWell1_Configuration =
                new LocationConfiguration(this.Config, "SwampWell1", 20, "SwampCreatures1", "SwampLoot1");
            //
            // MWL_GuckPit1_QuantityConfig = config("1 - MWL_GuckPit1", "Spawn Quantity", 15,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_GuckPit1_CreatureYamlConfig = config("1 - MWL_GuckPit1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_GuckPit1_CreatureListConfig = config("1 - MWL_GuckPit1", "Name of Creature List", "SwampCreatures1",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_GuckPit1_LootYamlConfig = config("1 - MWL_GuckPit1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_GuckPit1_LootListConfig = config("1 - MWL_GuckPit1", "Name of Loot List", "SwampLoot1",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampAltar1_QuantityConfig = config("2 - MWL_SwampAltar1", "Spawn Quantity", 15,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampAltar1_CreatureYamlConfig = config("2 - MWL_SwampAltar1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampAltar1_CreatureListConfig = config("2 - MWL_SwampAltar1", "Name of Creature List", "SwampCreatures3",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampAltar1_LootYamlConfig = config("2 - MWL_SwampAltar1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampAltar1_LootListConfig = config("2 - MWL_SwampAltar1", "Name of Loot List", "SwampLoot1",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampAltar2_QuantityConfig = config("3 - MWL_SwampAltar2", "Spawn Quantity", 10,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampAltar2_CreatureYamlConfig = config("3 - MWL_SwampAltar2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampAltar2_CreatureListConfig = config("3 - MWL_SwampAltar2", "Name of Creature List", "SwampCreatures3",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampAltar2_LootYamlConfig = config("3 - MWL_SwampAltar2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampAltar2_LootListConfig = config("3 - MWL_SwampAltar2", "Name of Loot List", "SwampLoot2",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampAltar3_QuantityConfig = config("4 - MWL_SwampAltar3", "Spawn Quantity", 10,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampAltar3_CreatureYamlConfig = config("4 - MWL_SwampAltar3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampAltar3_CreatureListConfig = config("4 - MWL_SwampAltar3", "Name of Creature List", "SwampCreatures3",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampAltar3_LootYamlConfig = config("4 - MWL_SwampAltar3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampAltar3_LootListConfig = config("4 - MWL_SwampAltar3", "Name of Loot List", "SwampLoot2",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampAltar4_QuantityConfig = config("5 - MWL_SwampAltar4", "Spawn Quantity", 10,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampAltar4_CreatureYamlConfig = config("5 - MWL_SwampAltar4", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampAltar4_CreatureListConfig = config("5 - MWL_SwampAltar4", "Name of Creature List", "SwampCreatures2",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampAltar4_LootYamlConfig = config("5 - MWL_SwampAltar4", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampAltar4_LootListConfig = config("5 - MWL_SwampAltar4", "Name of Loot List", "SwampLoot2",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // /*MWL_SwampCastle1_QuantityConfig = config("6 -  MWL_SwampCastle1", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampCastle1_CreatureYamlConfig = config("6 -  MWL_SwampCastle1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampCastle1_CreatureListConfig = config("6 -  MWL_SwampCastle1", "Name of Creature List", "SwampCreatures2",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampCastle1_LootYamlConfig = config("6 -  MWL_SwampCastle1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampCastle1_LootListConfig = config("6 -  MWL_SwampCastle1", "Name of Loot List", "SwampLoot3",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");*/
            //
            // MWL_SwampCastle2_QuantityConfig = config("7 -  MWL_SwampCastle2", "Spawn Quantity", 10,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampCastle2_CreatureYamlConfig = config("7 -  MWL_SwampCastle2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampCastle2_CreatureListConfig = config("7 -  MWL_SwampCastle2", "Name of Creature List", "SwampCreatures2",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampCastle2_LootYamlConfig = config("7 -  MWL_SwampCastle2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampCastle2_LootListConfig = config("7 -  MWL_SwampCastle2", "Name of Loot List", "SwampLoot3",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // /*MWL_SwampChurch1_QuantityConfig = config("8 -  MWL_SwampChurch1", "Spawn Quantity", 5,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampChurch1_CreatureYamlConfig = config("8 -  MWL_SwampChurch1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampChurch1_CreatureListConfig = config("8 -  MWL_SwampChurch1", "Name of Creature List", "SwampCreatures2",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampChurch1_LootYamlConfig = config("8 -  MWL_SwampChurch1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampChurch1_LootListConfig = config("8 -  MWL_SwampChurch1", "Name of Loot List", "SwampLoot2",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");*/
            //
            // MWL_SwampGrave1_QuantityConfig = config("9 -  MWL_SwampGrave1", "Spawn Quantity", 25,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampGrave1_CreatureYamlConfig = config("9 -  MWL_SwampGrave1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampGrave1_CreatureListConfig = config("9 -  MWL_SwampGrave1", "Name of Creature List", "SwampCreatures2",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampGrave1_LootYamlConfig = config("9 -  MWL_SwampGrave1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampGrave1_LootListConfig = config("9 -  MWL_SwampGrave1", "Name of Loot List", "SwampLoot1",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampHouse1_QuantityConfig = config("10 -  MWL_SwampHouse1", "Spawn Quantity", 20,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampHouse1_CreatureYamlConfig = config("10 -  MWL_SwampHouse1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampHouse1_CreatureListConfig = config("10 -  MWL_SwampHouse1", "Name of Creature List", "SwampCreatures3",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampHouse1_LootYamlConfig = config("10 -  MWL_SwampHouse1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampHouse1_LootListConfig = config("10 -  MWL_SwampHouse1", "Name of Loot List", "SwampLoot1",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampRuin1_QuantityConfig = config("11 -  MWL_SwampRuin1", "Spawn Quantity", 25,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampRuin1_CreatureYamlConfig = config("11 -  MWL_SwampRuin1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampRuin1_CreatureListConfig = config("11 -  MWL_SwampRuin1", "Name of Creature List", "SwampCreatures3",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampRuin1_LootYamlConfig = config("11 -  MWL_SwampRuin1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampRuin1_LootListConfig = config("11 -  MWL_SwampRuin1", "Name of Loot List", "SwampLoot1",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampTower1_QuantityConfig = config("12 -  MWL_SwampTower1", "Spawn Quantity", 20,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampTower1_CreatureYamlConfig = config("12 -  MWL_SwampTower1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampTower1_CreatureListConfig = config("12 -  MWL_SwampTower1", "Name of Creature List", "SwampCreatures3",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampTower1_LootYamlConfig = config("12 -  MWL_SwampTower1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampTower1_LootListConfig = config("12 -  MWL_SwampTower1", "Name of Loot List", "SwampLoot1",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampTower2_QuantityConfig = config("13 -  MWL_SwampTower2", "Spawn Quantity", 25,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampTower2_CreatureYamlConfig = config("13 -  MWL_SwampTower2", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampTower2_CreatureListConfig = config("13 -  MWL_SwampTower2", "Name of Creature List", "SwampCreatures4",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampTower2_LootYamlConfig = config("13 -  MWL_SwampTower2", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampTower2_LootListConfig = config("13 -  MWL_SwampTower2", "Name of Loot List", "SwampLoot3",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampTower3_QuantityConfig = config("14 -  MWL_SwampTower3", "Spawn Quantity", 25,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampTower3_CreatureYamlConfig = config("14 -  MWL_SwampTower3", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampTower3_CreatureListConfig = config("14 -  MWL_SwampTower3", "Name of Creature List", "SwampCreatures2",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampTower3_LootYamlConfig = config("14 -  MWL_SwampTower3", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampTower3_LootListConfig = config("14 -  MWL_SwampTower3", "Name of Loot List", "SwampLoot1",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            //
            // MWL_SwampWell1_QuantityConfig = config("15 -  MWL_SwampWell1", "Spawn Quantity", 20,
            //     "Amount of this location the game will attempt to place during world generation");
            // MWL_SwampWell1_CreatureYamlConfig = config("15 -  MWL_SwampWell1", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will spawn default creatures. When On, location will select creatures from list in the warpalicious.More_World_Locations_CreatureLists.yml file in BepinEx config folder");
            // MWL_SwampWell1_CreatureListConfig = config("15 -  MWL_SwampWell1", "Name of Creature List", "SwampCreatures1",
            //     "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");
            // MWL_SwampWell1_LootYamlConfig = config("15 -  MWL_SwampWell1", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off,
            //     "When Off, location will use default loot. When On, location will select loot from list in the warpalicious.More_World_Locations_LootLists.yml file in BepinEx config folder");
            // MWL_SwampWell1_LootListConfig = config("15 -  MWL_SwampWell1", "Name of Loot List", "SwampLoot1",
            //     "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            swampYAMLmanager.ParseDefaultYamls();
            swampYAMLmanager.ParseCustomYamls();
            
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
        
        public static LocationConfiguration MWL_GuckPit1_Configuration;
        public static LocationConfiguration MWL_SwampAltar1_Configuration;
        public static LocationConfiguration MWL_SwampAltar2_Configuration;
        public static LocationConfiguration MWL_SwampAltar3_Configuration;
        public static LocationConfiguration MWL_SwampAltar4_Configuration;
        public static LocationConfiguration MWL_SwampCastle2_Configuration;
        public static LocationConfiguration MWL_SwampGrave1_Configuration;
        public static LocationConfiguration MWL_SwampHouse1_Configuration;
        public static LocationConfiguration MWL_SwampRuin1_Configuration;
        public static LocationConfiguration MWL_SwampTower1_Configuration;
        public static LocationConfiguration MWL_SwampTower2_Configuration;
        public static LocationConfiguration MWL_SwampTower3_Configuration;
        public static LocationConfiguration MWL_SwampWell1_Configuration;
        
        // public static ConfigEntry<int> MWL_GuckPit1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_GuckPit1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_GuckPit1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_GuckPit1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_GuckPit1_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampAltar1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampAltar1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampAltar1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampAltar1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampAltar1_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampAltar2_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampAltar2_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampAltar2_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampAltar2_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampAltar2_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampAltar3_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampAltar3_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampAltar3_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampAltar3_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampAltar3_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampAltar4_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampAltar4_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampAltar4_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampAltar4_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampAltar4_LootListConfig = null!;
        //
        // /*public static ConfigEntry<int> MWL_SwampCastle1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampCastle1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampCastle1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampCastle1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampCastle1_LootListConfig = null!;*/
        //
        // public static ConfigEntry<int> MWL_SwampCastle2_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampCastle2_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampCastle2_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampCastle2_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampCastle2_LootListConfig = null!;
        //
        // /*public static ConfigEntry<int> MWL_SwampChurch1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampChurch1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampChurch1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampChurch1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampChurch1_LootListConfig = null!;*/
        //
        // public static ConfigEntry<int> MWL_SwampGrave1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampGrave1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampGrave1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampGrave1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampGrave1_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampHouse1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampHouse1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampHouse1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampHouse1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampHouse1_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampRuin1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampRuin1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampRuin1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampRuin1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampRuin1_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampTower1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampTower1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampTower1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampTower1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampTower1_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampTower2_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampTower2_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampTower2_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampTower2_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampTower2_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampTower3_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampTower3_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampTower3_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampTower3_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampTower3_LootListConfig = null!;
        //
        // public static ConfigEntry<int> MWL_SwampWell1_QuantityConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampWell1_CreatureYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampWell1_CreatureListConfig = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWL_SwampWell1_LootYamlConfig = null!;
        // public static ConfigEntry<string> MWL_SwampWell1_LootListConfig = null!;

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Swamp_Pack_1Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Swamp_Pack_1Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Swamp_Pack_1Logger.LogError("Please check your config entries for spelling and format!");
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