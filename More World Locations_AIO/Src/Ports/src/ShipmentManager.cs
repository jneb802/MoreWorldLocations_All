using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace More_World_Locations_AIO;

[PublicAPI]
public class ShipmentManager : MonoBehaviour
{
    public static ConfigEntry<float> TransitByDistance = null!; 
    public static ConfigEntry<string> CurrencyConfig = null!;
    public static ConfigEntry<float> TransitTime = null!;
    public static ConfigEntry<PortInit.Toggle> OverrideTransitTime = null!;
    public static ConfigEntry<float> ExpirationTime = null!;
    public static ConfigEntry<PortInit.Toggle> ExpirationEnabled = null!;
    private static CustomRPC PrivateShipments = null!;
    private static readonly Dictionary<long, PeerView> PeerViews = new();

    private sealed class PeerView
    {
        public ZNetPeer Peer = null!;
        public long PlayerID;
        public string PlayerName = string.Empty;
        public HashSet<string> ShipmentIDs = new();
    }
    
    public static ShipmentManager? instance;
    
    private static string ShipmentFileName = "shipments.dat";
    private static string MWL_FolderName = "MWL_Ports";
    private static string MWL_FolderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + MWL_FolderName;
    private static string GetFilePath(string worldName) => MWL_FolderPath + Path.DirectorySeparatorChar + worldName + "_" + ShipmentFileName;
    private const bool COMPRESS_DATA = true;
    internal static Dictionary<string, Shipment> Shipments = new();
    private static readonly List<ZDO> TempZDO = new(); // server side
    private static HashSet<ZDO> TempZDOHashSet = new(); // client side
    public static readonly List<string> PrefabsToSearch = new();
    public static event Action? OnShipmentsUpdated;
    
    public static ItemDrop.ItemData? _currencyItem;
    public static ItemDrop.ItemData? CurrencyItem
    {
        get
        {
            if (_currencyItem != null) return _currencyItem;
            if (!ObjectDB.instance) return null;
            if (ObjectDB.instance.GetItemPrefab(CurrencyConfig.Value) is { } itemPrefab && itemPrefab.TryGetComponent(out ItemDrop component))
            {
                _currencyItem = component.m_itemData;
            }
            return _currencyItem;
        }
    }
    
    // no need to create a new wait for seconds every time, so let's construct a static one here
    private static WaitForSeconds _wait = new WaitForSeconds(10f);
    // same for the coroutine, even though we only create one
    // but might be some edge case where our plugin gets destroyed ??? who are these fools messing with our plugin ???
    private static Coroutine? _sendZDOCoroutine;
    
    private float m_checkTransitTimer;
    private float m_checkTransitInterval = 1f;

    public void Awake()
    {
        instance = this;
        PrivateShipments = NetworkManager.Instance.AddRPC("PrivateShipments", IgnoreClientSnapshot, ReceiveSnapshot);
    }

    public void Update()
    {
        float dt = Time.deltaTime;
        CheckTransit(dt);
    }

    public void OnDestroy()
    {
        // clean up if destroyed for some reason
        instance = null;
        if (_sendZDOCoroutine != null) StopCoroutine(_sendZDOCoroutine);
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static class ZNet_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ZNet __instance)
        {
            Shipments.Clear();
            PeerViews.Clear();
            if (!__instance.IsServer()) return;
            // read file once world is set, to get the world name
            ReadLocalFile();
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            if (instance == null) return;
            if (!ZNet.instance || !ZNet.instance.IsServer()) return;
            // only the server should run this
            instance.InitCoroutine();
        }
    }
    public void InitCoroutine()
    {
        _sendZDOCoroutine = StartCoroutine(SendPortsToClients());
    }

    public IEnumerator SendPortsToClients()
    {
        // only the server runs this operation
        // runs forever while game is active
        // if you have over 2000 ports in the world, we might want to batch
        while (true)
        {
            if (Game.instance && ZDOMan.instance != null && ZNet.instance && ZNet.instance.IsServer())
            {
                TempZDO.Clear();
                foreach (string prefab in PrefabsToSearch)
                {
                    int index = 0;
                    while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(prefab, TempZDO, ref index)) yield return null;
                }

                foreach (ZDO zdo in TempZDO)
                {
                    ZDOMan.instance.ForceSendZDO(zdo.m_uid);
                }
            }

            yield return _wait;
        }
    }

    public void CheckTransit(float dt)
    {
        // everyone runs this
        if (!ZNet.instance) return;
        m_checkTransitTimer += dt;
        if (m_checkTransitTimer < m_checkTransitInterval) return;
        m_checkTransitTimer = 0f;
        bool isServer = ZNet.instance.IsServer();
        List<Shipment> expiredShipments = new List<Shipment>();
        foreach (Shipment shipment in Shipments.Values)
        {
            shipment.CheckTransit();
            if (isServer && shipment.State is ShipmentState.Expired && ExpirationEnabled.Value is PortInit.Toggle.On) expiredShipments.Add(shipment);
        }

        bool removedExpiredShipment = false;
        foreach (Shipment? shipment in expiredShipments)
        {
            removedExpiredShipment |= Shipments.Remove(shipment.ShipmentID);
        }

        if (removedExpiredShipment) UpdateShipments();
        else if (isServer) RefreshPeerViews();
    }

    private static IEnumerator IgnoreClientSnapshot(long sender, ZPackage package)
    {
        yield break;
    }

    private static IEnumerator ReceiveSnapshot(long sender, ZPackage package)
    {
        if (!ZNet.instance || ZNet.instance.IsServer() || sender != ZRoutedRpc.instance.GetServerPeerID()) yield break;
        Dictionary<string, Shipment>? data = JsonConvert.DeserializeObject<Dictionary<string, Shipment>>(package.ReadString());
        if (data == null) yield break;
        Shipments.Clear();
        Shipments.AddRange(data);
        OnShipmentsUpdated?.Invoke();
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"Received {Shipments.Count} shipments from server");
    }

    private static void RefreshPeerViews()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        HashSet<long> connected = new();
        foreach (ZNetPeer peer in ZNet.instance.GetPeers())
        {
            if (peer.m_server || !peer.IsReady()) continue;
            long peerID = peer.m_uid;
            long playerID = GetSenderPlayerID(peerID);
            if (playerID == 0L) continue;
            connected.Add(peerID);
            Dictionary<string, Shipment> visible = new();
            foreach (KeyValuePair<string, Shipment> entry in Shipments)
            {
                if (entry.Value.CanServerAccess(peerID, playerID, peer.m_playerName)) visible.Add(entry.Key, entry.Value);
            }
            HashSet<string> ids = new(visible.Keys);
            // Transit state follows shared timestamps on each client. Only membership
            // changes require a new snapshot; another player's changes do not.
            if (PeerViews.TryGetValue(peerID, out PeerView view) && ReferenceEquals(view.Peer, peer)
                && view.PlayerID == playerID && view.PlayerName == peer.m_playerName && view.ShipmentIDs.SetEquals(ids)) continue;
            ZPackage package = new();
            package.Write(JsonConvert.SerializeObject(visible, Formatting.None));
            PrivateShipments.SendPackage(peerID, package);
            PeerViews[peerID] = new PeerView { Peer = peer, PlayerID = playerID, PlayerName = peer.m_playerName, ShipmentIDs = ids };
        }
        foreach (long peerID in new List<long>(PeerViews.Keys))
        {
            if (!connected.Contains(peerID)) PeerViews.Remove(peerID);
        }
    }

    public static HashSet<ZDO> GetPorts()
    {
        // client side
        // since we have the server (who is the principle manager of ZDOs) force send the ZDO of our ports
        // we can now search through our own ZDOMan for the relevant ZDOs
        // we use this system because we want access to the prefab ZDO
        // which contains the name, guid, etc... that we saved on it
        List<ZDO> ports = new List<ZDO>();
        foreach (string prefab in PrefabsToSearch)
        {
            int index = 0;
            while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(prefab, ports, ref index))
            {
            }
        }
        // cache the list, so we can use it elsewhere, without needing to iterate
        // data can be invalid, since prefab might be destroyed
        // so we only use it in special cases
        // for our case, to update minimap pins
        TempZDOHashSet = new HashSet<ZDO>(ports);
        return TempZDOHashSet;
    }
    
    public static HashSet<ZDO> GetTempPorts() => TempZDOHashSet;

    public static List<Shipment> GetShipments(string portID)
    {
        List<Shipment> shipments = new List<Shipment>();
        foreach (Shipment shipment in Shipments.Values)
        {
            if (shipment.OriginPortID != portID) continue;
            shipments.Add(shipment);
        }
        return shipments;
    }

    public static List<Shipment> GetDeliveries(string portID)
    {
        List<Shipment> shipments = new List<Shipment>();
        foreach (Shipment? shipment in Shipments.Values)
        {
            if (shipment.DestinationPortID != portID) continue;
            shipments.Add(shipment);
        }
        return shipments;
    }
    
    public static void ReadLocalFile()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        Shipments.Clear();
        PeerViews.Clear();
        if (!Directory.Exists(MWL_FolderPath)) Directory.CreateDirectory(MWL_FolderPath);

        string path = GetFilePath(ZNet.m_world.m_name);
        if (!File.Exists(path))
        {
            return;
        }
        string json;
        if (COMPRESS_DATA)
        {
            byte[] compressed = File.ReadAllBytes(path);
            using MemoryStream memory = new MemoryStream(compressed);
            using GZipStream zip = new GZipStream(memory, CompressionMode.Decompress);
            using StreamReader reader = new StreamReader(zip, Encoding.UTF8);
            json = reader.ReadToEnd();
        }
        else
        {
            json = File.ReadAllText(path);
        }
        Dictionary<string, Shipment>? data = JsonConvert.DeserializeObject<Dictionary<string, Shipment>>(json);
        if (data != null) Shipments = data;
    }

    public static void UpdateShipments()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string json = JsonConvert.SerializeObject(Shipments, Formatting.Indented);
        if (!Directory.Exists(MWL_FolderPath)) Directory.CreateDirectory(MWL_FolderPath);
        string path = GetFilePath(ZNet.m_world.m_name);
        if (COMPRESS_DATA)
        {
            byte[] rawBytes = Encoding.UTF8.GetBytes(json);
            using MemoryStream memory = new MemoryStream();
            using (GZipStream zip = new GZipStream(memory, CompressionLevel.Fastest))
            {
                zip.Write(rawBytes, 0, rawBytes.Length);
            }
            File.WriteAllBytes(path, memory.ToArray());
        }
        else
        {
            File.WriteAllText(path, json);
        }
        RefreshPeerViews();
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class RegisterCustomRPC
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            ZRoutedRpc.instance.Register<string, string>(nameof(RPC_ServerReceiveShipment), RPC_ServerReceiveShipment);
            ZRoutedRpc.instance.Register<string, string>(nameof(RPC_ServerShipmentCollected), RPC_ServerShipmentCollected);
        }
    }
    public static void RPC_ServerReceiveShipment(long sender, string senderName, string serializedShipment)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        long playerID = GetSenderPlayerID(sender);
        if (peer == null || !peer.IsReady() || playerID == 0L) return;
        senderName = peer.m_playerName;
        Shipment newShipment = new Shipment(serializedShipment);
        newShipment.SetServerSender(sender, senderName);
        newShipment.SenderPlayerID = playerID;
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug(newShipment.IsValid // make sure that the shipment is deserialized correctly
            ? $"Shipment from {senderName} registered!"
            : $"Shipment from {senderName} is invalid");
        if (!newShipment.IsValid || string.IsNullOrEmpty(newShipment.ShipmentID) || Shipments.ContainsKey(newShipment.ShipmentID)) return;

        Shipments[newShipment.ShipmentID] = newShipment;
        UpdateShipments();
    }

    public static void RPC_ServerShipmentCollected(long sender, string senderName, string shipmentID)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null || !peer.IsReady()) return;
        senderName = peer.m_playerName;
        if (!Shipments.TryGetValue(shipmentID, out Shipment shipment))
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"{senderName} said that they collected shipment {shipmentID}, but not found in dictionary");
            return;
        }

        long senderPlayerID = GetSenderPlayerID(sender);
        if (!shipment.CanServerAccess(sender, senderPlayerID, senderName))
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"{senderName} tried to collect shipment {shipmentID}, but does not own it");
            return;
        }

        Shipments.Remove(shipmentID);
        UpdateShipments();
    }

    private static long GetSenderPlayerID(long sender)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return 0L;
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return 0L;
        ZDO? character = ZDOMan.instance != null && !peer.m_characterID.IsNone() ? ZDOMan.instance.GetZDO(peer.m_characterID) : null;
        long playerID = character != null ? character.GetLong(ZDOVars.s_playerID) : 0L;
        return playerID != 0L ? playerID : peer.m_playerID;
    }

    public struct PortID
    {
        public string Name;
        public string GUID;

        public PortID(string guid, string name)
        {
            Name = name;
            GUID = guid;
        }
    }
}
