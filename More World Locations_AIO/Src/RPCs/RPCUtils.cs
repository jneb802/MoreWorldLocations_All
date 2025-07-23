using System.Collections;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.RPCs;

public static class RPCUtils
{
    public static CustomRPC TriggerRaidEventRPC;
    
    public static void InitializeRPCs()
    {
        TriggerRaidEventRPC = NetworkManager.Instance.AddRPC(
            "TriggerRaidEvent", 
            TriggerRaidEventServerReceive, 
            TriggerRaidEventClientReceive);
    }
    
    // Server receives the RPC and triggers the raid
    private static IEnumerator TriggerRaidEventServerReceive(long sender, ZPackage package)
    {
        if (!ZNet.instance.IsServer())
        {
            Debug.Log("RPCUtils: Not server, ignoring raid trigger request");
            yield break;
        }
            
        // Read data from package
        string eventName = package.ReadString();
        Vector3 position = package.ReadVector3();
        
        Debug.Log($"RPCUtils Server: Received raid trigger request for '{eventName}' at {position}");
        
        // Trigger the raid event
        if (RandEventSystem.instance != null)
        {
            RandEventSystem.instance.SetRandomEventByName(eventName, position);
            Debug.Log($"RPCUtils Server: Successfully triggered raid event '{eventName}'");
        }
        else
        {
            Debug.LogError("RPCUtils Server: RandEventSystem.instance is null, cannot trigger raid");
        }
        
        yield return null; // Required for coroutine
    }
    
    // Client receives confirmation (optional - currently unused)
    private static IEnumerator TriggerRaidEventClientReceive(long sender, ZPackage package)
    {
        // This could be used to show effects or messages to all clients
        // For now, we'll just yield
        Debug.Log("RPCUtils Client: Received raid event RPC (currently unused)");
        yield return null;
    }
    
    // Helper method to send raid trigger RPC
    public static void SendTriggerRaidEvent(string eventName, Vector3 position)
    {
        if (TriggerRaidEventRPC == null)
        {
            Debug.LogError("RPCUtils: TriggerRaidEventRPC is null, make sure InitializeRPCs() was called");
            return;
        }
        
        // Create package with event data
        ZPackage package = new ZPackage();
        package.Write(eventName);
        package.Write(position);
        
        // Send RPC to server
        TriggerRaidEventRPC.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), package);
        
        Debug.Log($"RPCUtils: Sent raid trigger RPC for '{eventName}' at {position}");
    }
}