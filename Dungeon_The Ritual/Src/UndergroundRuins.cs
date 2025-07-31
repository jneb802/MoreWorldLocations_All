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

namespace Underground_Ruins
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Underground_RuinsPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Underground_Ruins";
        internal const string ModVersion = "1.0.9";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Dungeon_The_RitualLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);
        
        public static YAMLManager dungeonBFDYamlManager = new YAMLManager();
        
        public static AssetBundle assetBundle;
        public static string bundleName = "dungeonblackforest";
        public static GameObject dungeonGameObject;

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

            RoomExtras.ApplyPatches(_harmony);
                
            SetupWatcher();
            
            LoadAssetBundle();
            
            MWD_UndergroundRuins_Configuration =
                new LocationConfiguration(this.Config, "Underground Ruins", 60, "UndergroundRuinsCreatures1", "UndergroundRuinsLoot1","UndergroundRuinsPickables1");
            
            dungeonGameObject = assetBundle.LoadAsset<GameObject>("BFD_Exterior");
            Rooms.RegisterTheme(dungeonGameObject, "Underground Ruins");
            
            dungeonBFDYamlManager.ParseDefaultYamls();
            dungeonBFDYamlManager.ParseCustomYamls();
            dungeonBFDYamlManager.ParsePickableItemYaml("warpalicious.More_World_Locations");
            
            TranslationUtils.AddLocalizations();
            // Creatures.CreateShamanBoss();
            
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
            dungeonBFDYamlManager.BuildCreatureLists(MWD_UndergroundRuins_Configuration.CreatureYaml.Value, MWD_UndergroundRuins_Configuration.CreatureList.Value);
            dungeonBFDYamlManager.BuildLootList(MWD_UndergroundRuins_Configuration.LootYaml.Value, MWD_UndergroundRuins_Configuration.LootList.Value);
            dungeonBFDYamlManager.BuildPickableList(MWD_UndergroundRuins_Configuration.PickableItemYaml.Value, MWD_UndergroundRuins_Configuration.PickableItemList.Value);
            
            PrefabManager.OnVanillaPrefabsAvailable -= BuildYamlLists;
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
        
        public static LocationConfiguration MWD_UndergroundRuins_Configuration;
        
        // public static ConfigEntry<int> MWD_UndergroundRuins_Quantity_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWD_UndergroundRuins_CreatureYaml_Config = null!;
        // public static ConfigEntry<string> MWD_UndergroundRuins_CreatureList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWD_UndergroundRuins_LootYaml_Config = null!;
        // public static ConfigEntry<string> MWD_UndergroundRuins_LootList_Config = null!;
        // public static ConfigEntry<ConfigurationManager.Toggle> MWD_UndergroundRuins_PickableItemYaml_Config = null!;
        // public static ConfigEntry<string> MWD_UndergroundRuins_PickableItemList_Config = null!;

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Dungeon_The_RitualLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Dungeon_The_RitualLogger.LogError($"There was an issue loading your {ConfigFileName}");
                Dungeon_The_RitualLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

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