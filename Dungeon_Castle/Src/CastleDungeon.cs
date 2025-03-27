using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Common;
using Dungeon_Castle.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using Paths = BepInEx.Paths;

namespace Dungeon_Castle
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Dungeon_CastlePlugin : BaseUnityPlugin
    {
        internal const string ModName = "Dungeon_Castle";
        internal const string ModVersion = "1.0.0";
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
            
            MWD_CastleDungeon_Configuration =
                new LocationConfiguration(this.Config, "Castle", 40, "UndergroundRuinsCreatures1", "UndergroundRuinsLoot1","UndergroundRuinsPickables1");

            dungeonGameObject = assetBundle.LoadAsset<GameObject>("BFD_Exterior2");
            Rooms.RegisterTheme(dungeonGameObject, "CD_Castle");
            
            dungeonCastleYamlManager.ParseDefaultYamls();
            dungeonCastleYamlManager.ParseCustomYamls();
            dungeonCastleYamlManager.ParsePickableItemYaml("warpalicious.More_World_Locations");
            
            TranslationUtils.AddLocalizations();
            
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
            dungeonCastleYamlManager.BuildCreatureList(MWD_CastleDungeon_Configuration.CreatureYaml.Value, MWD_CastleDungeon_Configuration.CreatureList.Value);
            dungeonCastleYamlManager.BuildLootList(MWD_CastleDungeon_Configuration.LootYaml.Value, MWD_CastleDungeon_Configuration.LootList.Value);
            dungeonCastleYamlManager.BuildPickableList(MWD_CastleDungeon_Configuration.PickableItemYaml.Value, MWD_CastleDungeon_Configuration.PickableItemList.Value);
            
            PrefabManager.OnVanillaPrefabsAvailable -= BuildYamlLists;
        }
        
        public static LocationConfiguration MWD_CastleDungeon_Configuration;

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