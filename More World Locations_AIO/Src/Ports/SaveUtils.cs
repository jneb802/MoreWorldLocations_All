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
        private static string GetWorldSavePath()
        {
            if (ZNet.m_world == null)
            {
                Debug.LogWarning("SaveUtils.GetWorldSavePath: Tried to get world save path but ZNet.m_world is null");
                return null;
            }
            
            string baseDir = World.GetWorldSavePath(ZNet.m_world.m_fileSource);
            string worldFile = ZNet.m_world.m_fileName;
            string fileName = $"{worldFile}_{MOD_DATA_FILENAME}_{MOD_DATA_VERSION}.json";
            
            Debug.Log($"SaveUtils.GetWorldSavePath: WorldSave path is {Path.Combine(baseDir, fileName)}");
            return Path.Combine(baseDir, fileName);
        }

        public static void SavePortData()
        {
            Debug.Log("SaveUtils.SavePortData()");
            string modDataPath = GetWorldSavePath();
            
            var allPorts = PortDB.Instance.allPorts;
            List<PortKvp> list = new List<PortKvp>(allPorts.Count);
            
            foreach (var kvp in allPorts)
            {
                PortDB.PortData portData = new PortDB.PortData();
                portData.m_portID = kvp.Key;
                portData.m_localizationKey = kvp.Value.localizationKey;
                portData.m_worldPosition = kvp.Value.worldPosition;
                
                list.Add(new PortKvp { key = kvp.Key, value = portData }); 
            }
            
            PortSaveData saveData = new PortSaveData
            {
                worldUID = ZNet.instance.GetWorldUID(),
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
        
        public static void LoadPortData()
        {
            Debug.Log("SaveUtils.LoadPortData()");
            string modDataPath = GetWorldSavePath();

            if (string.IsNullOrEmpty(modDataPath) || !File.Exists(modDataPath))
            {
                Debug.LogWarning("SaveUtils.LoadPortData: No save file found");
                return;
            }

            try
            {
                string json = File.ReadAllText(modDataPath);
                var saveData = JsonUtility.FromJson<PortSaveData>(json);

                var dict = PortDB.Instance.allPortData;
                dict.Clear();

                if (saveData.ports != null)
                {
                    foreach (var kv in saveData.ports)
                    {
                        if (kv == null || string.IsNullOrEmpty(kv.key) || kv.value == null)
                            continue;

                        dict[kv.key] = kv.value;
                    }
                }

                Debug.Log($"SaveUtils: Loaded {dict.Count} ports from {Path.GetFullPath(modDataPath)}");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveUtils.LoadPortData: Failed to load ports: {e}");
            }
        }
    }
    
    [Serializable]
    public class PortKvp { public string key; public PortDB.PortData value; }

    [Serializable]
    public class PortSaveData
    {
        public long worldUID;
        public string worldName;
        public int modVersion;
        public string savedAt;
        public int portCount;
        public List<PortKvp> ports;
    }
}