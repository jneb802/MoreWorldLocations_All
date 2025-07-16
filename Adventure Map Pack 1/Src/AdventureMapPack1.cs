using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Adventure_Map_Pack_1.Utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using UnityEngine;
using Common;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SoftReferenceableAssets;
using AssetManager = Common.AssetManager;
using Paths = BepInEx.Paths;

namespace Adventure_Map_Pack_1
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Adventure_Map_Pack_1Plugin : BaseUnityPlugin
    {
        internal const string ModName = "Adventure_Map_Pack_1";
        internal const string ModVersion = "1.0.2";
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
        public static string bundlePath = Path.Combine(Paths.PluginPath, "warpalicious-Adventure_Map_Pack_1", bundleName);
        
        public static AssetBundle assetBundleTotem;
        public static string bundleNameTotem = "totems";
        
        public static YAMLManager adventuremap1YAMLmanager = new YAMLManager();
        
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
            
            // LoadAssetBundle();
        
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
            
            adventuremap1YAMLmanager.ParseDefaultYamls();
            adventuremap1YAMLmanager.ParseCustomYamls();

            // BuildManifest();
            
            // PrefabManager.OnVanillaPrefabsAvailable += PrefabUtils.CreateCustomPrefabs;
            // StatusEffectUtils.CreateCustomStatusEffects();
            
            ZoneManager.OnVanillaLocationsAvailable += AddLocation;

            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
                Config.Save();
            }
        }

        public string bundle = "assetBundleTest";
        public string[] assetPathsInBundle = new[]
        {
            "Assets/WarpProjects/LocationTesting/Location1.prefab",
        };
        
        public void BuildManifest(string bundleName, string[] assetPathsInBundle)
        {
            string manifestPath = Path.Combine(Paths.PluginPath, "assetBundleManifest");
            string bundleRelativeDir = ".";
            
            SoftReferenceableAssets.AssetBundleManifest manifest = new SoftReferenceableAssets.AssetBundleManifest(bundleRelativeDir);

            foreach (string assetPath in assetPathsInBundle)
            {
                AssetID assetId = Jotunn.Managers.AssetManager.GenerateAssetID(assetPath);
                var location = new AssetLocation(bundleName, assetPath);
                manifest.AddAssetLocation(assetId, location);
            }
            
            manifest.AddBundleDependencies(bundleName, System.Array.Empty<string>());
            
            manifest.SerializeToDisk(manifestPath, SerializationFormat.Text);
        }

        public void AddLocation()
        {

            SoftReference<GameObject> location1SoftRefPrefab = Jotunn.Managers.AssetManager.Instance.GetSoftReference<GameObject>("Location1");
            SoftReference<GameObject> mountainDvergrShrine2SoftRefPrefab = Jotunn.Managers.AssetManager.Instance.GetSoftReference<GameObject>("MWL_MountainDvergrShrine2");
            
            CustomLocation customLocation = new 
                CustomLocation(
                    location1SoftRefPrefab,
                    true,
                    LocationConfigs.MWL_MountainDvergrShrine1_Config);
            
            CustomLocation customLocation2 = new 
                CustomLocation(
                    mountainDvergrShrine2SoftRefPrefab,
                    true,
                    LocationConfigs.MWL_MountainDvergrShrine1_Config);
        
            ZoneManager.Instance.AddCustomLocation(customLocation);
            ZoneManager.Instance.AddCustomLocation(customLocation2);
            
            ZoneManager.OnVanillaLocationsAvailable -= AddLocation;
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