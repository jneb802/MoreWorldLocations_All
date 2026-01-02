using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace More_World_Locations_AIO;

[Serializable][PublicAPI][JsonObject(MemberSerialization.Fields)]
public class Shipment
{
    public string OriginPortName = string.Empty;
    public string DestinationPortName = string.Empty;
    public string OriginPortID = string.Empty; // Guid.ToString()
    public string DestinationPortID = string.Empty;
    public string ShipmentID = string.Empty;
    public ShipmentState State = ShipmentState.Pending;
    public double ArrivalTime;
    public double ExpirationTime;
    public List<ShipmentItem> Items = new List<ShipmentItem>();

    [NonSerialized] public bool IsValid = true;
    
    // used by port to format data, then adds items, then calls SendToServer()
    public Shipment(ShipmentManager.PortID originPort, ShipmentManager.PortID destinationPort, float distance)
    {
        OriginPortName = originPort.Name;
        OriginPortID = originPort.GUID;
        DestinationPortID = destinationPort.GUID;
        DestinationPortName = destinationPort.Name;
        ShipmentID = Guid.NewGuid().ToString();
        ArrivalTime = ZNet.instance.GetTimeSeconds() + CalculateDistanceTime(distance);
        ExpirationTime = ArrivalTime + ShipmentManager.ExpirationTime.Value;
    }

    public static double CalculateDistanceTime(float distance)
    {
        if (ShipmentManager.OverrideTransitTime.Value is PortInit.Toggle.On)
        {
            return ShipmentManager.TransitTime.Value;
        }
        return ShipmentManager.TransitByDistance.Value * distance;
    }

    // used when receiving shipment from client
    public Shipment(string serializedShipment)
    {
        if (ShipmentManager.instance == null) return;
        Shipment? data = JsonConvert.DeserializeObject<Shipment>(serializedShipment);
        if (data == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("[SERVER] Failed to parse shipment JSON");
            IsValid = false;
            return;
        }
        OriginPortName = data.OriginPortName;
        OriginPortID = data.OriginPortID;
        DestinationPortName = data.DestinationPortName;
        DestinationPortID = data.DestinationPortID;
        ShipmentID = data.ShipmentID;
        State = data.State;
        ArrivalTime = data.ArrivalTime;
        ExpirationTime = data.ExpirationTime;
        Items = data.Items;
        ShipmentManager.Shipments[ShipmentID] = this;
        ShipmentManager.UpdateShipments();
    }
    
    public void SendToServer()
    {
        if (ShipmentManager.instance == null) return;
        if (ZNet.instance && ZNet.instance.IsServer())
        {
            // if local client is server, then simply register to shipments, and update
            ShipmentManager.Shipments[ShipmentID]  = this;
            ShipmentManager.UpdateShipments();
        }
        else
        {
            // else send data to server to manage
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(ShipmentManager.RPC_ServerReceiveShipment), Player.m_localPlayer.GetPlayerName(), ToJson());
        }
    }

    public void OnCollected()
    {
        if (ShipmentManager.instance == null) return;
        if (ZNet.instance && ZNet.instance.IsServer())
        {
            // if local client is server, simply remove from dictionary
            if (!ShipmentManager.Shipments.Remove(ShipmentID))
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"{Player.m_localPlayer.GetPlayerName()} said that they collected shipment {ShipmentID}, but not found in dictionary");
            }
            else
            {
                ShipmentManager.UpdateShipments();
            }
        }
        else
        {
            // else send shipment ID to server to manage
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(ShipmentManager.RPC_ServerShipmentCollected), Player.m_localPlayer.GetPlayerName(), ShipmentID);
        }
    }

    public void CheckTransit()
    {
        double currentTime = ZNet.instance.GetTimeSeconds();

        if (currentTime < ArrivalTime)
        {
            State = ShipmentState.InTransit;
        }
        else if (currentTime <= ExpirationTime)
        {
            State = ShipmentState.Delivered;
        }
        else
        {
            State = ShipmentState.Expired;
        }
    }


    public double GetTimeTo(double targetTime) => Math.Max(targetTime - ZNet.instance.GetTimeSeconds(), 0);

    public double GetTimeToArrivalSeconds() => GetTimeTo(ArrivalTime);
    public double GetTimeToExpirationSeconds() => GetTimeTo(ExpirationTime);
    
    // Formats duration as human-readable time, with Valheim days for longer durations.
    // Reads day length from EnvMan to support mods that change day duration.
    public static string FormatTime(double totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;

        int hours   = (int)(totalSeconds / 3600);
        int minutes = (int)((totalSeconds % 3600) / 60);
        int seconds = (int)(totalSeconds % 60);

        List<string> parts = new List<string>();
        if (hours > 0)   parts.Add($"{hours}h");
        if (minutes > 0) parts.Add($"{minutes}m");
        if (seconds > 0) parts.Add($"{seconds}s");
        
        string realTime = string.Join(" ", parts);
        
        // Use game's actual day length (supports mods like LongerDays).
        long dayLengthSec = EnvMan.instance != null ? EnvMan.instance.m_dayLengthSec : 1200L;
        double valheimDays = totalSeconds / dayLengthSec;
        if (valheimDays >= 1)
        {
            return $"~{valheimDays:0.#} days ({realTime})";
        }
        return realTime;
    }
    
    public string ToJson() => JsonConvert.SerializeObject(this);
    
    public string GetTooltip()
    {
        double remainingTime = State == ShipmentState.InTransit 
            ? GetTimeToArrivalSeconds() 
            : GetTimeToExpirationSeconds();

        string time = FormatTime(remainingTime);
        StringBuilder stringBuilder = new();
        stringBuilder.Append($"{LocalKeys.OriginPort}: <color=orange>{OriginPortName}</color>");
        stringBuilder.Append($"\n{LocalKeys.DestinationPort}:  <color=orange>{DestinationPortName}</color>");
        stringBuilder.AppendFormat("\n{2}: <color=yellow>{0}</color>{1}\n", State.ToKey(), string.IsNullOrEmpty(time) ? "" : $" ({time})", LocalKeys.State);
        stringBuilder.Append($"\n{LocalKeys.Items}: ");
        foreach (ShipmentItem? shipmentItem in Items)
        {
            if (ObjectDB.instance.GetItemPrefab(shipmentItem.ItemName) is not { } itemPrefab || !itemPrefab.TryGetComponent(out ItemDrop component)) continue;
            stringBuilder.Append($"\n<color=orange>{component.m_itemData.m_shared.m_name}</color>");
            if (shipmentItem.Stack > 1) stringBuilder.Append($" x{shipmentItem.Stack}");
        }
        return stringBuilder.ToString();
    }

    public List<string> LogPrint()
    {
        List<string> log = new List<string>
        {
            "Origin Port Name:      " + OriginPortName,
            "Destination Port Name: " + DestinationPortName,
            "Origin Port ID:        " + OriginPortID,
            "Destination Port ID:   " + DestinationPortID,
            "Shipment ID:           " + ShipmentID,
            "State:                 " + State,
            "Arrival Time:          " + ArrivalTime,
            "Items Count:           " + Items.Count,
            ""
        };
        return log;
    }
}

[PublicAPI]
public enum ShipmentState
{
    Pending,
    InTransit,
    Delivered,
    Expired
}

[Serializable][PublicAPI][JsonObject(MemberSerialization.Fields)]
public class ShipmentItem
{
    public int ChestID;
    public string ItemName;
    public int Stack;
    public float Durability;
    public int Quality;
    public int Variant;
    public long CrafterID;
    public string CrafterName;
    public float Weight;
    public string SharedName;
    public Dictionary<string, string> CustomData;
    
    /// <summary>
    /// Use this to construct a new shipment to send to server
    /// </summary>
    /// <param name="chestID"></param>
    /// <param name="item"></param>
    public ShipmentItem(int chestID, ItemDrop.ItemData item)
    {
        ChestID = chestID;
        ItemName = item.m_dropPrefab.name;
        Stack = item.m_stack;
        Durability = item.m_durability;
        Quality = item.m_quality;
        Variant = item.m_variant;
        CrafterID = item.m_crafterID;
        CrafterName = item.m_crafterName;
        Weight = item.m_shared.m_weight;
        SharedName = item.m_shared.m_name;
        CustomData = item.m_customData;
    }
    /// <summary>
    /// Fixed food items having a valid SharedData
    /// Use Valheim inventory <see cref="Inventory.AddItem(string, int, int, int, long, string, bool)"/>
    /// That way, it instantiates a new game object, grabs ItemDrop component, then destroys game object
    /// </summary>
    /// <param name="container"></param>
    /// <returns></returns>
    public bool AddItem(Container container)
    {
        ItemDrop.ItemData? item = container.GetInventory().AddItem(ItemName, Stack, Quality, Variant, CrafterID, CrafterName);
        if (item == null) return false;
        item.m_durability = Durability;
        item.m_customData = CustomData;
        return true;
    }
    /// <summary>
    /// Use this constructor to read from ZDO
    /// </summary>
    /// <param name="pkg"></param>
    public ShipmentItem(ZPackage pkg)
    {
        ChestID = pkg.ReadInt();
        ItemName = pkg.ReadString();
        Stack = pkg.ReadInt();
        Durability = (float)pkg.ReadDouble();
        Quality = pkg.ReadInt();
        Variant = pkg.ReadInt();
        CrafterID = pkg.ReadLong();
        CrafterName = pkg.ReadString();
        SharedName = pkg.ReadString();
        Weight = (float)pkg.ReadDouble();
        CustomData = new Dictionary<string, string>();
        int customDataCount = pkg.ReadInt();
        if (customDataCount <= 0) return;
        for (int i = 0; i < customDataCount; i++)
        {
            CustomData[pkg.ReadString()] = pkg.ReadString();
        }
    }

    /// <summary>
    /// Use this to format shipment into pkg
    /// </summary>
    /// <param name="pkg"></param>
    public void Write(ZPackage pkg)
    {
        pkg.Write(ChestID);
        pkg.Write(ItemName);
        pkg.Write(Stack);
        pkg.Write((double)Durability);
        pkg.Write(Quality);
        pkg.Write(Variant);
        pkg.Write(CrafterID);
        pkg.Write(CrafterName);
        pkg.Write(SharedName);
        pkg.Write((double)Weight);
        pkg.Write(CustomData.Count);
        foreach (KeyValuePair<string, string> kvp in CustomData)
        {
            pkg.Write(kvp.Key);
            pkg.Write(kvp.Value);
        }
    }
}