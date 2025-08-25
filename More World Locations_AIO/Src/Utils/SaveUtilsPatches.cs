using System;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.Utils
{
    /// <summary>
    /// Patch to save port data when world save starts
    /// </summary>
    [HarmonyPatch(typeof(ZNet),  nameof(ZNet.SaveWorld))]
    public class SaveUtils_ZNet_SaveWorld_Patch
    {
        public static void Prefix(bool sync)
        {
            Debug.Log("SaveUtils: SaveWorld");
            Debug.Log($"SaveUtils: SaveWorld - ZNet.instance: {ZNet.instance != null}");
            Debug.Log($"SaveUtils: SaveWorld - ZNet.m_world: {ZNet.m_world != null}");
            Debug.Log($"SaveUtils: SaveWorld - IsServer: {ZNet.instance?.IsServer()}");
            Debug.Log($"SaveUtils: SaveWorld - LoadError: {ZNet.m_loadError}");
            try
            {
                Debug.Log("SaveUtils: World save detected, saving port data...");
                SaveUtils.SavePortData();
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveUtils: Error in SaveWorld_Prefix - {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Patch to save port data when world save starts
    /// </summary>
    [HarmonyPatch(typeof(ZNet), "SaveWorldThread")]
    public class SaveUtils_ZNet_SaveWorldThread_Patch
    {
        public static void Prefix()
        {
            Debug.Log("SaveUtils: SaveWorldThread");
            Debug.Log($"SaveUtils: SaveWorldThread - ZNet.instance: {ZNet.instance != null}");
            Debug.Log($"SaveUtils: SaveWorldThread - ZNet.m_world: {ZNet.m_world != null}");
            Debug.Log($"SaveUtils: SaveWorldThread - IsServer: {ZNet.instance?.IsServer()}");
            Debug.Log($"SaveUtils: SaveWorldThread - LoadError: {ZNet.m_loadError}");
            try
            {
                Debug.Log("SaveUtils: World save detected, saving port data...");
                SaveUtils.SavePortData();
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveUtils: Error in SaveWorldThread_Prefix - {e.Message}");
            }
        }
    }

    /// <summary>
    /// Patch for when ZNet finishes loading world data
    /// </summary>
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.LoadWorld))]
    public class SaveUtils_ZNet_LoadWorld_Patch
    {
        public static void Postfix(ZNet __instance)
        {
            Debug.Log("SaveUtils: ZNet_LoadWorld_Postfix");
            try
            {
                if (__instance != null && ZNet.m_world != null && __instance.IsServer() && !ZNet.m_loadError)
                {
                    Debug.Log("SaveUtils: ZNet world load completed, ensuring port data is loaded...");
                    SaveUtils.LoadPortData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveUtils: Error in ZNet_LoadWorld_Postfix - {e.Message}");
            }
        }
    }
}
