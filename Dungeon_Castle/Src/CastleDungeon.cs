using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Common;
using Forbidden_Catacombs.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Extensions;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using Paths = BepInEx.Paths;

namespace Forbidden_Catacombs
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Forbidden_CatacombsPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Forbidden_Catacombs";
        internal const string ModVersion = "1.0.2";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Dungeon_CastleLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        // Location Manager variables
        public Texture2D tex = null!;
        
        public static YAMLManager dungeonCastleYamlManager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "dungeoncastle";
        public static GameObject dungeonGameObject;

        public enum Toggle
        {
            On = 1,
            Off = 0
        }
        
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

            MWD_CastleDungeon_Quantity = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Spawn Quantity", 40, 
                "Amount of this dungeon the game will attempt to place during world generation");
            
            MWD_CastleDungeon_CreatureYaml = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off, 
                "When Off, location will spawn default creatures. When On, both upper and lower dungeon sections will select creatures from the list in the warpalicious.More_World_Locations_CreatureLists.yml file in the BepInEx config folder");
            MWD_CastleDungeon_CreatureListUpper = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Name of Creature List for Upper Section", "CatacombCreatures1", 
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            MWD_CastleDungeon_CreatureListLower = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Name of Creature List for Lower Section", "CatacombCreatures2", 
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWD_CastleDungeon_LootYaml = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off, 
                "When Off, location will use default loot. When On, both upper and lower dungeon sections will select loot from the list in the warpalicious.More_World_Locations_LootLists.yml file in the BepInEx config folder");
            MWD_CastleDungeon_LootListUpper = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Name of Loot List for Upper Section", "CatacombLoot1", 
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            MWD_CastleDungeon_LootListLower = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Name of Loot List for Lower Section", "CatacombLoot2", 
                "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
            
            MWD_CastleDungeon_PickableYaml = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Use Custom PickableItem YAML file", ConfigurationManager.Toggle.Off, 
                "When Off, location will use default loot. When On, dungeon will select loot from list in the custom YAML file in BepinEx config folder");
            MWD_CastleDungeon_PickableList = ConfigFileExtensions.BindConfigInOrder(this.Config, "Forbidden_Catacombs", "Name of PickableItem List", "CatacombPickables1", 
                "The name of the loot list to use from YAML file");
            
            dungeonGameObject = assetBundle.LoadAsset<GameObject>("CD_Exterior1");
            Rooms.RegisterTheme(dungeonGameObject, "CD_Catacomb");
            
            dungeonCastleYamlManager.ParseDefaultYamls();
            dungeonCastleYamlManager.ParseCustomYamls();
            dungeonCastleYamlManager.ParsePickableItemYaml("warpalicious.More_World_Locations");
            
            Utils.TranslationUtils.AddLocalizations();
            
            PrefabManager.OnVanillaPrefabsAvailable += BuildYamlLists;
            PrefabManager.OnVanillaPrefabsAvailable += CustomPrefabs.RegisterKitPrefabs;
            ZoneManager.OnVanillaLocationsAvailable += Locations.AddAllLocations;
            DungeonManager.OnVanillaRoomsAvailable += Rooms.AddAllRooms;
            
            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
                Config.Save();
            }
        }
        
        public void BuildYamlLists()
        {
            dungeonCastleYamlManager.BuildCreatureLists(MWD_CastleDungeon_CreatureYaml.Value, MWD_CastleDungeon_CreatureListUpper.Value);
            dungeonCastleYamlManager.BuildCreatureLists(MWD_CastleDungeon_CreatureYaml.Value, MWD_CastleDungeon_CreatureListLower.Value);
            dungeonCastleYamlManager.BuildLootList(MWD_CastleDungeon_LootYaml.Value, MWD_CastleDungeon_LootListUpper.Value);
            dungeonCastleYamlManager.BuildLootList(MWD_CastleDungeon_LootYaml.Value, MWD_CastleDungeon_LootListLower.Value);
            dungeonCastleYamlManager.BuildPickableList(MWD_CastleDungeon_PickableYaml.Value, MWD_CastleDungeon_PickableList.Value);
            
            PrefabManager.OnVanillaPrefabsAvailable -= BuildYamlLists;
        }
        
        public static ConfigEntry<int> MWD_CastleDungeon_Quantity;
        public static ConfigEntry<ConfigurationManager.Toggle> MWD_CastleDungeon_CreatureYaml;
        public static ConfigEntry<string> MWD_CastleDungeon_CreatureListUpper;
        public static ConfigEntry<string> MWD_CastleDungeon_CreatureListLower;
        public static ConfigEntry<ConfigurationManager.Toggle> MWD_CastleDungeon_LootYaml;
        public static ConfigEntry<string> MWD_CastleDungeon_LootListUpper;
        public static ConfigEntry<string> MWD_CastleDungeon_LootListLower;
        public static ConfigEntry<ConfigurationManager.Toggle> MWD_CastleDungeon_PickableYaml;
        public static ConfigEntry<string> MWD_CastleDungeon_PickableList;

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
                Dungeon_CastleLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Dungeon_CastleLogger.LogError($"There was an issue loading your {ConfigFileName}");
                Dungeon_CastleLogger.LogError("Please check your config entries for spelling and format!");
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