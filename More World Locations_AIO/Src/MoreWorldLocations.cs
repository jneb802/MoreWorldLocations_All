﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace More_World_Locations_AIO
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class More_World_Locations_AIOPlugin : BaseUnityPlugin
    {
        internal const string ModName = "More_World_Locations_AIO";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        
        public static readonly ManualLogSource More_World_Locations_AIOLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            BepinexConfigs.Config = Config;
            bool saveOnSet = BepinexConfigs.Config.SaveOnConfigSet;
            BepinexConfigs.Config.SaveOnConfigSet =
                false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();

            if (saveOnSet)
            {
                BepinexConfigs.Config.SaveOnConfigSet = saveOnSet;
                BepinexConfigs.Config.Save();
            }
        }

        private void OnDestroy()
        {
            BepinexConfigs.Config.Save();
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
                More_World_Locations_AIOLogger.LogDebug("ReadConfigValues called");
                BepinexConfigs.Config.Reload();
            }
            catch
            {
                More_World_Locations_AIOLogger.LogError($"There was an issue loading your {ConfigFileName}");
                More_World_Locations_AIOLogger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
    }
}