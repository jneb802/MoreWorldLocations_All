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
        internal const string ModVersion = "1.2.3";
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
            MWL_AbandonedHouse1_Configuration =
                new LocationConfiguration(this.Config, "AbandonedHouse1", 15, "SwampCreatures3", "SwampLoot1");
            MWL_Treehouse1_Configuration =
                new LocationConfiguration(this.Config, "Treehouse1", 20, "SwampCreatures1", "SwampLoot1");
            MWL_Shipyard1_Configuration =
                new LocationConfiguration(this.Config, "Shipyard1", 20, "SwampCreatures2", "SwampLoot1");
            MWL_FortBakkarhalt1_Configuration =
                new LocationConfiguration(this.Config, "FortBakkarhalt1", 5, "SwampCreatures2", "SwampLoot1");
            MWL_Belmont1_Configuration =
                new LocationConfiguration(this.Config, "Belmont1", 5, "SwampCreatures2", "SwampLoot1");
            
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
        
        public static LocationConfiguration MWL_AbandonedHouse1_Configuration;
        public static LocationConfiguration MWL_Treehouse1_Configuration;
        public static LocationConfiguration MWL_Shipyard1_Configuration;
        public static LocationConfiguration MWL_FortBakkarhalt1_Configuration;
        public static LocationConfiguration MWL_Belmont1_Configuration;
        
        public static LocationConfiguration MWL_SwampComplex1_Configuration;
        public static LocationConfiguration MWL_SwampSanctuary1_Configuration;
        public static LocationConfiguration MWL_SwampShrine1_Configuration;
        public static LocationConfiguration MWL_SwampStrongHold1_Configuration;
        public static LocationConfiguration MWL_SwampTemple1_Configuration;
        
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