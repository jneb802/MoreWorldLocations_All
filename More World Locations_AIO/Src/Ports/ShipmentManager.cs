using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments
{
    public static class ShipmentManager
    {
        // === Public state (one static cache per process) =========================================
        public static readonly Dictionary<Guid, Shipment> InTransitShipments = new(); // server: truth, client: mirror
        public static event Action ShipmentsChanged; // hook your UI refresh to this

        // === RPC names ===========================================================================
        const string RPC_Add       = "MWL_Shipment_Add";
        const string RPC_Remove    = "MWL_Shipment_Remove";
        const string RPC_Full      = "MWL_Shipment_Full";
        const string RPC_ReqFull   = "MWL_Shipment_RequestFull";
        const string RPC_CreateReq = "MWL_Shipment_Create";       // client -> server (create)
        const string RPC_PortShipments = "MWL_Shipment_Port";
        
        static bool _registered;

        // =========================================================================================
        // Init
        // Call this once in your plugin init (e.g., Awake or after ZRoutedRpc is ready)
        public static void RegisterRpcs()
        {
            if (_registered) return;
            _registered = true;

            System.Collections.IEnumerator Co(System.Action a) { a(); yield break; }

            // Server -> Clients
            Jotunn.Managers.NetworkManager.Instance.AddRPC(RPC_Add,    (s, p) => Co(() => Client_Add(p)),    null);
            Jotunn.Managers.NetworkManager.Instance.AddRPC(RPC_Remove, (s, p) => Co(() => Client_Remove(p)), null);
            Jotunn.Managers.NetworkManager.Instance.AddRPC(RPC_Full,   (s, p) => Co(() => Client_Full(p)),   null);
            
            // Clients -> Server
            Jotunn.Managers.NetworkManager.Instance.AddRPC(RPC_ReqFull,   null, (s, p) => Co(() => Server_HandleFullRequest(s)));
            Jotunn.Managers.NetworkManager.Instance.AddRPC(RPC_CreateReq, null, (s, p) => Co(() => Server_HandleCreate(s, p)));
        }


        static bool IsServer() => ZNet.instance && ZNet.instance.IsServer();
        
        static bool IsHostOrSinglePlayer()
        {
            // We're the server, and we are not connected to any "server peer" (i.e., we're not a remote client).
            // Covers both single-player and listen-host cases.
            return IsServer() && (ZNet.instance?.GetServerPeer() == null);
        }

        // =========================================================================================
        // Client entry points

        public static void ClientRequestFullSync()
        {
            Debug.Log("ShipmentManager.ClientRequestFullSync");
            Debug.Log($"ShipmentManager.ClientRequestFullSync: IsHostOrSinglePlayer = {IsHostOrSinglePlayer().ToString()}");
            // In SP / host: our local dict is authoritative already; no network round trip needed.
            if (IsHostOrSinglePlayer())
            {
                // Optionally trigger UI to refresh if it expects a "sync complete" event
                ShipmentsChanged?.Invoke();
                return;
            }
            
            ZNetPeer server = ZNet.instance?.GetServerPeer();
            if (server == null) return;
            ZRoutedRpc.instance.InvokeRoutedRPC(server.m_uid, RPC_ReqFull, new ZPackage());
        }

        // ask the server to create a shipment (clients should NEVER create locally)
        public static void ClientRequestCreateShipment(Shipment shipment)
        {
            Debug.Log($"ShipmentManager: ClientRequestCreateShipment for shipment with ID {shipment.m_shipmentID}");
            // In SP / host: execute server logic directly on the client (authoritative in this process)
            if (IsHostOrSinglePlayer())
            {
                ServerCreateShipment(shipment);
                return;
            }

            ZNetPeer server = ZNet.instance?.GetServerPeer();
            if (server == null) return;

            ZPackage package = ShipmentSerializer.SerializeShipmentPackage(shipment);
            ZRoutedRpc.instance.InvokeRoutedRPC(server.m_uid, RPC_CreateReq, package);
        }

        

        // =========================================================================================
        // Server entry points (only call these on server/host)

        public static Shipment ServerCreateShipment(Shipment shipment)
        {
            if (!IsServer()) { Debug.LogWarning("ServerCreateShipment called on client"); return null; }

            Debug.Log($"ShipmentManager: ServerCreateShipment for shipment with ID {shipment.m_shipmentID}");
            InTransitShipments[shipment.m_shipmentID] = shipment;
            BroadcastAdd(shipment);
            // ShipmentsChanged?.Invoke();
            return null;
        }

        public static void ServerRemoveShipment(Guid id)
        {
            if (!IsServer()) { Debug.LogWarning("ServerRemoveShipment called on client"); return; }
            if (!InTransitShipments.Remove(id)) return;

            BroadcastRemove(id);
            //ShipmentsChanged?.Invoke();
        }

        // =========================================================================================
        // RPC handlers

        // client <- server: add/replace a shipment
        static void Client_Add(ZPackage pkg)
        {
            Shipment shipment = ShipmentSerializer.DeserializeShipmentPackage(pkg);
            if (shipment == null) return;
            InTransitShipments[shipment.m_shipmentID] = shipment;
            ShipmentsChanged?.Invoke();
        }

        // client <- server: remove a shipment
        static void Client_Remove(ZPackage pkg)
        {
            if (Guid.TryParse(pkg.ReadString(), out var id))
            {
                InTransitShipments.Remove(id);
                ShipmentsChanged?.Invoke();
            }
        }

        // client <- server: sever sends client full snapshot of all shipments
        static void Client_Full(ZPackage pkg)
        {
            InTransitShipments.Clear();
            int n = pkg.ReadInt();
            for (int i = 0; i < n; i++)
            {
                var sp = pkg.ReadPackage();
                Shipment shipment  = ShipmentSerializer.DeserializeShipmentPackage(sp);
                if (shipment != null) InTransitShipments[shipment.m_shipmentID] = shipment;
            }
            ShipmentsChanged?.Invoke();
        }

        // server: handle client’s “give me the full list” request
        static void Server_HandleFullRequest(long sender)
        {
            if (!IsServer()) return;

            var outPkg = new ZPackage();
            outPkg.Write(InTransitShipments.Count);
            foreach (Shipment shipment in InTransitShipments.Values)
                outPkg.Write(ShipmentSerializer.SerializeShipmentPackage(shipment));

            ZRoutedRpc.instance.InvokeRoutedRPC(sender, RPC_Full, outPkg);
        }

        // server: handle client’s “create shipment” request
        static void Server_HandleCreate(long sender, ZPackage pkg)
        {
            if (!IsServer()) return;
            
            Debug.Log($"ShipmentManager.Server_HandleCreate: Creating shipment {sender} ");
            Shipment shipment = ShipmentSerializer.DeserializeShipmentPackage(pkg);
            ServerCreateShipment(shipment);
        }
        
        // =========================================================================================
        // Broadcast helpers (server only)

        static void BroadcastAdd(Shipment shipment)
        {
            Debug.Log($"ShipmentManager: BroadcastAdd for shipment with ID {shipment.m_shipmentID}");
            var pkg = ShipmentSerializer.SerializeShipmentPackage(shipment);
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RPC_Add, pkg);
        }

        static void BroadcastRemove(Guid id)
        {
            var pkg = new ZPackage();
            pkg.Write(id.ToString("N"));
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RPC_Remove, pkg);
        }
        
        // =========================================================================================
        // Optional: persistence on the server via a persistent ZDO (tiny blob)

        // public static void SaveToZDO(ZNetView znv)
        // {
        //     if (!IsServer() || znv == null || znv.GetZDO() == null) return;
        //
        //     var pack = new ZPackage();
        //     pack.Write(InTransitShipments.Count);
        //     foreach (var s in InTransitShipments.Values) pack.Write(ShipmentSerializer.BuildPackage(s));
        //
        //     znv.GetZDO().Set("MWL_ShipmentsBlob", pack.GetArray()); // byte[]
        // }
        //
        // public static void LoadFromZDO(ZNetView znv)
        // {
        //     if (znv == null || znv.GetZDO() == null) return;
        //
        //     var bytes = znv.GetZDO().GetByteArray("MWL_ShipmentsBlob");
        //     if (bytes == null) return;
        //
        //     InTransitShipments.Clear();
        //     var pack = new ZPackage(bytes);
        //     int n = pack.ReadInt();
        //     for (int i = 0; i < n; i++)
        //     {
        //         var sp = pack.ReadPackage();
        //         var s  = ShipmentSerializer.ParsePackage(sp);
        //         if (s != null) InTransitShipments[s.m_shipmentID] = s;
        //     }
        //     ShipmentsChanged?.Invoke();
        // }
        
        public static List<Shipment> GetShipmentsForDestination(string portId)
        {
            var result = new List<Shipment>();
            if (string.IsNullOrEmpty(portId)) return result;

            foreach (var s in InTransitShipments.Values)
            {
                if (s != null && string.Equals(s.m_destinationPortID, portId, StringComparison.Ordinal))
                {
                    Debug.Log($"ShipmentManager.GetShipmentsForDestination: Found port with matching destintionID:{portId}");
                    result.Add(s);
                }
                
            }
            return result;
        }
        
        public static List<Shipment> GetShipmentsForOrigin(string portId)
        {
            var result = new List<Shipment>();
            if (string.IsNullOrEmpty(portId)) return result;

            foreach (var s in InTransitShipments.Values)
            {
                if (s != null && string.Equals(s.m_originPortID, portId, StringComparison.Ordinal))
                {
                    Debug.Log($"ShipmentManager.GetShipmentsForOrigin: Found port with matching originID:{portId}");
                    result.Add(s);
                }
                
            }
            return result;
        }

        // public static List<GameObject> OpenShipment(string shipmentID)
        // {
        //     List<GameObject> result = new List<GameObject>();
        //     Shipment shipment = InTransitShipments[Guid.Parse(shipmentID)];
        //     //GameObject gameObject = PrefabManager.
        //
        // }
        
    }
}
