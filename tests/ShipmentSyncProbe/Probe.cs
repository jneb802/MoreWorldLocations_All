using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;

// Test-client helper only. Never include this assembly in a release package.
[BepInPlugin("praetoris.tests.ShipmentSyncProbe", "Shipment Sync Probe", "1.0.0")]
[BepInDependency("warpalicious.More_World_Locations_AIO")]
public sealed class Probe : BaseUnityPlugin
{
    private void Start()
    {
        new Terminal.ConsoleCommand("mwl_probe_open", "Open the nearest port through its normal interaction handler", args => Run(args, Open));
        new Terminal.ConsoleCommand("mwl_probe_seed", "Create one test shipment at the nearest port", args => Run(args, Seed));
        new Terminal.ConsoleCommand("mwl_probe_status", "Read shipment state without opening a port", args => Run(args, () => string.Join(",", Shipments.Keys.Cast<object>())));
        new Terminal.ConsoleCommand("mwl_probe_collect", "Collect one shipment created by this probe", args => Run(args, Collect));
    }

    private static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(assembly => assembly.GetType("More_World_Locations_AIO." + name)).First(type => type != null);

    private static object Open()
    {
        if (!Player.m_localPlayer) throw new InvalidOperationException("A local player is required");
        Component port = UnityEngine.Object.FindObjectsByType(Find("Port"), FindObjectsSortMode.None).Cast<Component>()
            .OrderBy(value => Vector3.Distance(value.transform.position, Player.m_localPlayer.transform.position)).First();
        if (Vector3.Distance(port.transform.position, Player.m_localPlayer.transform.position) > 10)
            throw new InvalidOperationException("Move within 10 metres of a port");
        port.GetType().GetMethod("Interact").Invoke(port, new object[] { Player.m_localPlayer, false, false });
        return port;
    }

    private static object Seed()
    {
        object port = Open();
        object portId = port.GetType().GetField("m_portID").GetValue(port);
        string guid = (string)portId.GetType().GetField("GUID").GetValue(portId);
        string name = (string)portId.GetType().GetField("Name").GetValue(portId);
        string json = JsonConvert.SerializeObject(new
        {
            OriginPortName = name, DestinationPortName = name,
            OriginPortID = guid, DestinationPortID = guid,
            ShipmentID = "sync-probe-" + Guid.NewGuid().ToString("N"),
            ArrivalTime = ZNet.instance.GetTimeSeconds() - 1,
            ExpirationTime = ZNet.instance.GetTimeSeconds() + 3600,
            SenderPlayerID = Player.m_localPlayer.GetPlayerID(),
            SenderName = Player.m_localPlayer.GetPlayerName(),
            Items = new object[0]
        });
        object shipment = Activator.CreateInstance(Find("Shipment"), json);
        shipment.GetType().GetMethod("SendToServer").Invoke(shipment, null);
        return shipment.GetType().GetField("ShipmentID").GetValue(shipment);
    }

    private static void Run(Terminal.ConsoleEventArgs args, Func<object> action)
    {
        try
        {
            object result = action();
            args.Context.AddString($"OK: {result} shipments={Shipments.Count}");
        }
        catch (Exception exception)
        {
            args.Context.AddString("ERROR: " + (exception.InnerException ?? exception));
        }
    }

    private static IDictionary Shipments => (IDictionary)Find("ShipmentManager")
        .GetField("Shipments", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null);

    private static object Collect()
    {
        string id = Shipments.Keys.Cast<string>().First(key => key.StartsWith("sync-probe-", StringComparison.Ordinal));
        object shipment = Shipments[id];
        shipment.GetType().GetMethod("OnCollected").Invoke(shipment, null);
        return id;
    }
}
