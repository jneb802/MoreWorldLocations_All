using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using More_World_Locations_AIO.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace More_World_Locations_AIO.Shipments;

public class PortManager : MonoBehaviour
{
    private const string SaveKey = "mwllocations.knownports.v1";
    public readonly HashSet<string> KnownPortIds = new();
    private bool _hasLoaded = false;
    
    [Serializable]
    private class SaveBlob { public List<string> ids = new(); }
    
    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Player), nameof(Player.Load))]
        private static void OnPlayerLoad(Player __instance)
        {
            if (__instance == null) return;
            if (!__instance.gameObject.TryGetComponent<PortManager>(out var portManager))
                portManager = __instance.gameObject.AddComponent<PortManager>();
            portManager.LoadFrom(__instance);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        private static void OnPlayerAwake(Player __instance)
        {
            if (__instance == null) return;
            if (!__instance.gameObject.TryGetComponent<PortManager>(out var portManager))
                portManager = __instance.gameObject.AddComponent<PortManager>();
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.Save))]
        private static void OnPlayerSave(Player __instance)
        {
            if (__instance == null) return;
            if (__instance.TryGetComponent<PortManager>(out var portManager))
            {
                portManager.SaveTo(__instance);
            }
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.AddKnownLocationName))]
        private static bool OnPlayerDiscoverLocation(Player __instance, string label)
        {
            if (__instance == null || __instance != Player.m_localPlayer) return true;
            if (!string.Equals(label, "MWL_PortLocation")) return true;
            PortManager pm = __instance.GetComponent<PortManager>();
            if (pm == null) return true;
            Location location = Location.GetLocation(__instance.transform.position, false);
            if (location == null) return true;
            Port port = WorldUtils.GetPortInRange(location.transform.position, location.m_exteriorRadius);
            if (port == null) return true;
            if (!pm.KnownPortIds.Contains(port.m_portID))
            {
                pm.KnownPortIds.Add(port.m_portID);
                MessageHud.instance.ShowBiomeFoundMsg(port.name, true);
            }

            return false;
        }
    }
    
    public void LoadFrom(Player player)
    {
        if (player == null) return;
        if (_hasLoaded) return;
        if (player.m_customData == null) return;
        if (!player.m_customData.TryGetValue(SaveKey, out var json))
        {
            _hasLoaded = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.Log($"PortManager.LoadFrom: SaveKey '{SaveKey}' exists but JSON value is null/empty/whitespace");
            _hasLoaded = true;
            return;
        }

        try
        {
            var blob = JsonUtility.FromJson<SaveBlob>(json);
            KnownPortIds.Clear();
            if (blob?.ids != null)
            {
                foreach (var id in blob.ids)
                {
                    Debug.Log($"PortManager.LoadFrom: Loading port: {id} from player save file");
                    KnownPortIds.Add(id);
                }
            }
            _hasLoaded = true;
            Debug.Log($"PortManager.LoadFrom: Successfully loaded {KnownPortIds.Count} ports");
        }
        catch (Exception e)
        {
            KnownPortIds.Clear();
            _hasLoaded = true;
        }
    }

    public void SaveTo(Player player)
    {
        if (player == null) return;
        if (player.m_customData == null) return;
        foreach (var port in KnownPortIds)
        {
            Debug.Log($"PortManager.SaveTo: Player knows port ID: {port}");
        }
        
        try
        {
            SaveBlob blob = new SaveBlob { ids = KnownPortIds.ToList() };
            string json = JsonUtility.ToJson(blob);
            player.m_customData[SaveKey] = json;
            Debug.Log($"PortManager.SaveTo: Successfully saved {KnownPortIds.Count} ports to player data");
        }
        catch (Exception e)
        {
            Debug.LogError($"PortManager.SaveTo: Failed to save port data: {e}");
        }
    }

    public List<Port> GetPlayerPorts()
    {
        Debug.Log($"PortManager.GetPlayerPorts: Getting ports for player");
        var ports = new List<Port>();
        foreach (var id in KnownPortIds)
        {
            Debug.Log($"PortManager.GetPlayerPorts: Trying to get port: {id} from PortDB");
            if (PortDB.Instance.allPorts.TryGetValue(id, out var port))
            {
                Debug.Log($"PortManager.GetPlayerPorts: Found port: {id} in PortDB");
                ports.Add(port);
            }
        }
        return ports;
    }
}
