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
    public static Dictionary<string, Port> allPorts = new Dictionary<string, Port>();
    
    private class SaveBlob { public List<string> ids = new(); } // JsonUtility needs a wrapper
    
    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        private static void OnPlayerAwake(Player __instance)
        {
            if (!__instance.gameObject.TryGetComponent<PortManager>(out var portManager))
                portManager = __instance.gameObject.AddComponent<PortManager>();
            portManager.LoadFrom(__instance);
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.Save))]
        private static void OnPlayerSave(Player __instance)
        {
            if (__instance && __instance.TryGetComponent<PortManager>(out var portManager))
                portManager.SaveTo(__instance);
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.AddKnownLocationName))]
        private static bool OnPlayerDiscoverLocation(Player __instance, string label)
        {
            // Debug.Log("Running patch for addknonwnlocationName");
            if (!__instance || __instance != Player.m_localPlayer)
            {
                Debug.Log("Player is null or not local player");
                return true;
            }

            if (!string.Equals(label, "MWL_PortLocation"))
            {
                Debug.Log("String does not equal MWL_PortLocation");
                return true;
            }

            PortManager pm = __instance.GetComponent<PortManager>();
            if (pm == null)
            {
                Debug.Log("String does not equal MWL_PortLocation");
                return true;
            }
            Location location = Location.GetLocation(__instance.transform.position, false);
            if (location == null)
            {
                Debug.Log("Location is null");
                return true;
            }

            Port port = WorldUtils.GetPortInRange(location.transform.position, location.m_exteriorRadius);
            if (port == null)
            {
                Debug.Log("Port is null");
                return true;
            }
            
            // Debug.Log($"checking for port with id: {port.m_portID}");
            if (!pm.KnownPortIds.Contains(port.m_portID))
            {
                pm.KnownPortIds.Add(port.m_portID);
                MessageHud.instance.ShowBiomeFoundMsg(port.name, true);
            }
                

            return false; // don't run the original method for this placeholder
        }
    }
    
    public void LoadFrom(Player player)
    {
        if (player == null) return;
        if (!player.m_customData.TryGetValue(SaveKey, out var json) || string.IsNullOrWhiteSpace(json)) return;

        try
        {
            var blob = JsonUtility.FromJson<SaveBlob>(json);
            KnownPortIds.Clear();
            if (blob?.ids != null)
                foreach (var id in blob.ids)
                {
                    Debug.Log($"Loading port: {id} from player save file");
                    KnownPortIds.Add(id);
                }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PortManager] Failed to parse customData: {e}");
            // On parse failure, start fresh rather than crashing the player load.
            KnownPortIds.Clear();
        }
    }

    public void SaveTo(Player player)
    {
        if (player == null) return;
        SaveBlob blob = new SaveBlob { ids = KnownPortIds.ToList() };
        player.m_customData[SaveKey] = JsonUtility.ToJson(blob);
    }

    public List<Port> GetPlayerPorts()
    {
        return KnownPortIds
            .Where(id => allPorts.ContainsKey(id))
            .Select(id => allPorts[id])
            .ToList();
    }
}