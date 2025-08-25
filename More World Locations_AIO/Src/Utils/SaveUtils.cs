using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using More_World_Locations_AIO.Shipments;

namespace More_World_Locations_AIO.Utils
{
    public static class SaveUtils
    {
        private const string MOD_DATA_FILENAME = "MoreWorldLocations_Ports";
        private const int MOD_DATA_VERSION = 1;
        private static string GetModDataPath()
        {
            if (ZNet.m_world == null) return null;
            string dir = Path.GetDirectoryName(ZNet.m_world.GetMetaPath());
            string worldName = ZNet.m_world.m_name ?? "unknown";
            return Path.Combine(dir, $"{worldName}_{MOD_DATA_FILENAME}.json");
        }

        // --- SAVE ---
        public static void SavePortData()
        {
            Debug.Log("SaveUtils.SavePortData()");
            try
            {
                if (ZNet.instance?.GetWorldUID() == 0 || ZNet.m_world == null)
                {
                    Debug.LogWarning("SaveUtils: No valid world loaded, skipping save");
                    return;
                }

                string modDataPath = GetModDataPath();
                if (string.IsNullOrEmpty(modDataPath))
                {
                    Debug.LogWarning("SaveUtils: Could not resolve save path");
                    return;
                }

                var allPorts = PortDB.Instance.allPorts;
                var list = new List<PortKvp>(allPorts.Count);
                foreach (var kv in allPorts)
                    list.Add(new PortKvp { key = kv.Key, value = kv.Value });

                var saveData = new PortSaveData
                {
                    worldUID = ZNet.instance.GetWorldUID().ToString(),
                    worldName = ZNet.m_world.m_name ?? "unknown",
                    modVersion = MOD_DATA_VERSION,
                    savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    portCount = allPorts.Count,
                    ports = list
                };

                string json = JsonUtility.ToJson(saveData, true);

                Directory.CreateDirectory(Path.GetDirectoryName(modDataPath));
                File.WriteAllText(modDataPath, json);

                Debug.Log($"SaveUtils: Saved {allPorts.Count} ports to {Path.GetFullPath(modDataPath)}");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveUtils: Failed to save port data - {e.Message}");
            }
        }

        // --- LOAD ---
        public static void LoadPortData()
        {
            LogAllPaths();
            Debug.Log("SaveUtils.LoadPortData");
            try
            {
                if (ZNet.instance?.GetWorldUID() == 0 || ZNet.m_world == null)
                {
                    Debug.LogWarning("SaveUtils: No valid world loaded, skipping load");
                    return;
                }

                string modDataPath = GetModDataPath();
                if (string.IsNullOrEmpty(modDataPath))
                {
                    Debug.LogWarning("SaveUtils: Could not resolve load path");
                    return;
                }

                Debug.Log($"SaveUtils: Looking for port data at {Path.GetFullPath(modDataPath)}");

                if (!File.Exists(modDataPath))
                {
                    Debug.Log("SaveUtils: No existing port save found, using defaults");
                    return;
                }

                string json = File.ReadAllText(modDataPath);
                var saveData = JsonUtility.FromJson<PortSaveData>(json);
                if (saveData == null)
                {
                    Debug.LogError("SaveUtils: Failed to parse JSON");
                    return;
                }

                var dict = PortDB.Instance.allPorts;
                dict.Clear();
                if (saveData.ports != null)
                {
                    foreach (var kv in saveData.ports)
                        if (!string.IsNullOrEmpty(kv.key) && kv.value != null)
                            dict[kv.key] = kv.value;
                }

                Debug.Log($"SaveUtils: Loaded {dict.Count} ports from {Path.GetFullPath(modDataPath)}");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveUtils: Failed to load port data - {e.Message}");
            }
        }
        
        
        public static void LogAllPaths()
        {
            try
            {
                Debug.Log("==== SaveUtils Path Debug ====");

                if (ZNet.m_world == null)
                {
                    Debug.LogWarning("No world loaded, cannot log paths");
                    return;
                }

                Debug.Log($"World name: {ZNet.m_world.m_name}");
                Debug.Log($"World UID: {ZNet.instance?.GetWorldUID()}");

                // Core Valheim paths
                Debug.Log($"ZNet.m_world.GetMetaPath(): {ZNet.m_world.GetMetaPath()}");
                Debug.Log($"ZNet.m_world.GetDBPath():   {ZNet.m_world.GetDBPath()}");

                // Utils-based path
                string utilsPath = global::Utils.GetSaveDataPath(ZNet.m_world.m_fileSource);
                Debug.Log($"Utils.GetSaveDataPath():    {utilsPath}");
                
                string utilsPath1 = global::Utils.GetSaveDataPath(FileHelpers.FileSource.Cloud);
                Debug.Log($"Utils.GetSaveDataPath() cloud:    {utilsPath1}");
                
                string utilsPath2 = global::Utils.GetSaveDataPath(FileHelpers.FileSource.Local);
                Debug.Log($"Utils.GetSaveDataPath() cloud:    {utilsPath2}");

                // Our custom mod file path
                string modDataPath = Path.Combine(
                    Path.GetDirectoryName(ZNet.m_world.GetMetaPath()) ?? "",
                    $"{ZNet.m_world.m_name}_MoreWorldLocations_Ports.json"
                );
                Debug.Log($"ModDataPath (next to .fwl): {modDataPath}");

                // Application base
                Debug.Log($"Application.persistentDataPath: {Application.persistentDataPath}");

                Debug.Log("==== End Path Debug ====");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveUtilsDebug.LogAllPaths failed: {e.Message}");
            }
        }
        
    }

    // --- support types ---
    [Serializable]
    public class PortKvp { public string key; public Port value; }

    [Serializable]
    public class PortSaveData
    {
        public string worldUID;
        public string worldName;
        public int modVersion;
        public string savedAt;
        public int portCount;
        public List<PortKvp> ports;
    }
}