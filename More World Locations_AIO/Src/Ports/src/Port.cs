using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using More_World_Locations_AIO.Managers;
using More_World_Locations_AIO.Utils;
using UnityEngine;

namespace More_World_Locations_AIO;

public class Port : MonoBehaviour, Interactable, Hoverable
{
    public ZNetView m_view = null!;
    public ShipmentManager.PortID m_portID;
    public string m_name = "Port";
    public string m_traderName = "Haldor";
    public readonly ContainerPlacement m_containers = new();
    public Humanoid? m_currentHumanoid;
    private readonly TempItems m_tempItems = new();
    private bool m_initialized;
    private bool m_openPending;
    private bool m_migrationFailed;
    public void Awake()
    {
        m_view = GetComponent<ZNetView>();
        if (!m_view.IsValid()) return;
        m_view.Register("MWL_RequestPortControl", RPC_RequestPortControl);
        
        m_name = m_view.GetZDO().GetString(PortVars.Name, NameGenerator.GenerateName());
        m_portID.GUID = m_view.GetZDO().GetString(PortVars.GUID, Guid.NewGuid().ToString());
        m_portID.Name = m_name;
        
        m_traderName = m_view.GetZDO().GetString(PortVars.TraderName, TraderNames.GetRandomName());
        if (m_view.IsOwner())
        {
            m_view.GetZDO().Set(PortVars.GUID, m_portID.GUID);
            m_view.GetZDO().Set(PortVars.Name, m_name);
            m_view.GetZDO().Set(PortVars.TraderName, m_traderName);
        }
    }

    public void Start()
    {
        if (!m_view.IsValid()) return;
        StartCoroutine(InitCoroutine());
        InvokeRepeating(nameof(RefreshContainers), 1f, 1f);
    }

    private IEnumerator InitCoroutine()
    {
        yield return null;
        for (int i = 0; i < 20 && !m_initialized; i++)
        {
            EnsureInitialized();
            if (!m_initialized) yield return new WaitForSeconds(0.5f);
        }
    }

    private void EnsureInitialized()
    {
        if (m_initialized) return;

        LocationProxy? locationProxy = WorldUtils.GetLocationInRange(this.transform.position, 10);
        if (locationProxy == null) return;

        Transform locationRoot = locationProxy.transform;
        foreach (Transform child in locationRoot.FindAllRecursive("containerPosition"))
        {
            TempContainer temp = new TempContainer(this, child, m_containers.Placements.Count);
            m_containers.Placements.Add(temp);
        }

        if (m_containers.Placements.Count == 0) return;

        m_initialized = true;
        RefreshContainers();
    }

    private void RefreshContainers()
    {
        if (!m_view.IsValid()) return;
        if (!m_initialized) { EnsureInitialized(); return; }
        foreach (TempContainer temp in m_containers.Placements) temp.Refresh();
        // Only the network owner converts the old snapshot. Normal chests then own
        // their inventory data; loading a port must never replay that snapshot.
        if (m_view.IsOwner() && !m_migrationFailed && !m_view.GetZDO().GetBool(PortVars.PersistentContainers))
        {
            try
            {
                if (!LoadSavedItems()) throw new InvalidOperationException("Could not restore every saved shipping item.");
            }
            catch (Exception exception)
            {
                DestroyContainers();
                m_migrationFailed = true;
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError($"Port {m_name}: saved items were retained because migration failed: {exception}");
                return;
            }
            m_view.GetZDO().Set(PortVars.PersistentContainers, true);
            m_view.GetZDO().Set(PortVars.Items, "");
        }
        if (m_view.IsOwner() && m_view.GetZDO().GetBool(PortVars.HasOpenDelivery) && CanModifyContainers() && !m_containers.HasItems())
        {
            // A remote inventory may not have loaded yet. Check the saved native
            // inventory as well before deleting an emptied delivery's chests.
            foreach (Container container in m_containers.GetSpawnedContainers())
            {
                byte[]? data = container.m_nview.GetZDO().GetByteArray(ZDOVars.s_items);
                if (data == null) return;
                Inventory inventory = new Inventory("Port delivery check", null, container.m_width, container.m_height);
                inventory.Load(new ZPackage(data));
                if (inventory.HasItems()) return;
            }
            SetHasOpenDelivery(false);
            DestroyContainers();
        }
    }

    private void RefreshItems()
    {
        m_tempItems.Clear();
        m_tempItems.Add(m_containers.GetSpawnedContainers());
    }

    private bool LoadSavedItems()
    {
        string? data = m_view.GetZDO().GetString(PortVars.Items);
        if (string.IsNullOrWhiteSpace(data)) return true;
        m_tempItems.Clear();
        ZPackage pkg = new ZPackage(data);
        int itemCount = pkg.ReadInt();
        for (int i = 0; i < itemCount; i++)
        {
            ShipmentItem temp = new ShipmentItem(pkg);
            m_tempItems.Add(temp);
        }
        return LoadItems(m_tempItems.Items);
    }
    
    public Container? SpawnContainer(Manifest manifest)
    {
        EnsureInitialized();
        if (!CanModifyContainers()) return null;
        if (m_containers.GetManifests().Contains(manifest)) return null;
        foreach (TempContainer? temp in m_containers.Placements)
        {
            if (temp.IsSpawned) continue;
            temp.manifest = manifest;
            Container? container = temp.Spawn();
            if (container == null) return null;
            return container;
        }
        return null;
    }
    
    public void DestroyContainers()
    {
        foreach (TempContainer? temp in m_containers.Placements)
        {
            temp.Destroy();
        }
    }
    private void SetHasOpenDelivery(bool value)
    {
        if (m_view.IsValid()) m_view.GetZDO().Set(PortVars.HasOpenDelivery, value);
    }

    private bool CanModifyContainers()
    {
        if (!m_initialized || !m_view.IsOwner() || !m_view.GetZDO().GetBool(PortVars.PersistentContainers)) return false;
        foreach (TempContainer temp in m_containers.Placements)
        {
            temp.Refresh();
            if (temp.IsSpawned && (temp.SpawnedContainer == null || temp.SpawnedContainer.IsInUse() ||
                temp.SpawnedContainer.m_nview.GetZDO().GetInt(ZDOVars.s_inUse) == 1)) return false;
        }
        return true;
    }

    private void RPC_RequestPortControl(long sender)
    {
        if (!m_view.IsOwner() || (PortUI.IsVisible() && PortUI.instance?.m_currentPort == this)) return;
        RefreshContainers();
        if (!CanModifyContainers()) return;
        m_view.GetZDO().SetOwner(sender);
    }

    private IEnumerator OpenWhenReady(Humanoid user)
    {
        m_openPending = true;
        m_view.InvokeRPC("MWL_RequestPortControl");
        for (int i = 0; i < 30; i++)
        {
            if (!user || !m_view.IsValid()) break;
            RefreshContainers();
            if (CanModifyContainers())
            {
                m_name = m_view.GetZDO().GetString(PortVars.Name, m_name);
                m_portID = new ShipmentManager.PortID(m_view.GetZDO().GetString(PortVars.GUID), m_name);
                ShipmentManager.RequestShipments();
                PortUI.instance?.Show(this);
                if (user is Player player) player.AddKnownPort(m_portID);
                m_currentHumanoid = user;
                m_openPending = false;
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        if (user) user.Message(MessageHud.MessageType.Center, "$msg_inuse");
        m_openPending = false;
    }
    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (hold || PortUI.instance == null || m_openPending) return false;
        StartCoroutine(OpenWhenReady(user));
        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

    public string GetHoverText()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(m_traderName);
        stringBuilder.Append($"\n[<color=yellow><b>$KEY_Use</b></color>] {LocalKeys.Open}");
        return Localization.instance.Localize(stringBuilder.ToString());
    }

    private bool LoadItems(List<ShipmentItem> items)
    {
        // load items
        foreach (ShipmentItem item in items)
        {
            Container? container = m_containers.GetOrCreate(item.ChestID);
            if (container == null)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"Failed to create container: {item.ChestID}, max container spawned ??");
                DestroyContainers();
                return false;
            }
            container.m_nview.ClaimOwnership();
            if (!item.AddItem(container))
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("Failed to add item: " + item.ItemName);
                DestroyContainers();
                return false;
            }
        }
        // Keep Container.OnContainerChanged attached: it saves and synchronizes
        // every inventory change through the normal Valheim chest record.
        return true;
    }

    public bool LoadDelivery(Shipment delivery)
    {
        EnsureInitialized();
        if (!CanModifyContainers()) return false;
        if (!delivery.CanAccess(Player.m_localPlayer))
        {
            if (m_currentHumanoid != null) m_currentHumanoid.Message(MessageHud.MessageType.Center, LocalKeys.ShipmentNotOwned);
            return false;
        }

        if (m_containers.HasItems())
        {
            if (m_currentHumanoid != null) m_currentHumanoid.Message(MessageHud.MessageType.Center, LocalKeys.FailedToLoadDelivery);
            return false;
        }
        if (!LoadItems(delivery.Items)) return false;
        if (!m_containers.HasItems()) return false;

        // Once a delivery is opened, its contents live in the destination port chests.
        // Remove the server shipment now so the original delivery cannot be opened again.
        SetHasOpenDelivery(true);
        delivery.OnCollected();
        if (m_currentHumanoid != null) m_currentHumanoid.Message(MessageHud.MessageType.Center, LocalKeys.DeliveryCollected);
        return true;
    }

    public bool SendShipment(PortInfo selectedPort)
    {
        if (!CanModifyContainers()) return false;
        if (!m_containers.HasItems())
        {
            if (m_currentHumanoid != null) m_currentHumanoid.Message(MessageHud.MessageType.Center, LocalKeys.FailedToSend);
            return false;
        }
        // construct a new shipment
        float distance = global::Utils.DistanceXZ(transform.position, selectedPort.position);
        Shipment shipment = new Shipment(m_portID, selectedPort.portID, distance);
        Container[] containers = m_containers.GetSpawnedContainers();
        // add items from containers
        shipment.Items.Add(containers);
        // send shipment to server to manage
        shipment.SendToServer();
        DestroyContainers();
        m_tempItems.Clear();
        m_view.GetZDO().Set(PortVars.Items, ""); // make sure to tell ZDO that there are no items
        SetHasOpenDelivery(false);
        if (m_currentHumanoid != null) m_currentHumanoid.Message(MessageHud.MessageType.Center, LocalKeys.SuccessfullySent);
        return true;
    }

    public string GetHoverName() => Localization.instance.Localize(m_traderName);

    public float GetHoverOffset() => 0f;

    public string GetTooltip()
    {
        if (!m_containers.HasItems()) return "";
        RefreshItems();
        StringBuilder sb = new StringBuilder();
        sb.Append($"{LocalKeys.CurrentShipments}:");
        sb.Append($"\n{LocalKeys.Cost}: <color=orange>{ShipmentManager.CurrencyItem?.m_shared.m_name ?? "$item_coins"}</color> <color=yellow>x{m_containers.GetCost()}</color>");
        sb.Append($"\n{m_tempItems.GetTooltip()}");
        return sb.ToString();
    }

    private class TempItems
    {
        public readonly List<ShipmentItem> Items = new();
        private float GetTotalWeight() => Items.Sum(i => i.Weight);
        private int GetTotalStack() => Items.Sum(i => i.Stack);
        
        public string GetTooltip()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"\n{LocalKeys.NrOfItems}: <color=yellow>{GetTotalStack()}</color>");
            sb.Append($"\n{LocalKeys.TotalWeight}: <color=yellow>{GetTotalWeight():0.0}</color>");
            sb.Append($"\n{LocalKeys.Contents}:");
            foreach (var item in Items)
            {
                sb.Append($"\n<color=orange>{item.SharedName}</color> x{item.Stack}");
            }
            return sb.ToString();
        }
        public void Clear() => Items.Clear();
        public void Add(ShipmentItem shipmentItem) => Items.Add(shipmentItem);
        public void Add(params Container[] containers) => Items.Add(containers);
        
    }

    public class ContainerPlacement
    {
        public readonly List<TempContainer> Placements = new List<TempContainer>();
        public Container? GetOrCreate(int chestID)
        {
            foreach (TempContainer? temp in Placements)
            {
                if (temp.IsSpawned && temp.manifest?.ChestStableHashCode == chestID)
                    return temp.SpawnedContainer;
            }
            if (!Manifest.Manifests.TryGetValue(chestID, out Manifest manifest))
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("Failed to find manifest: " + chestID);
                return null;
            }
            foreach (TempContainer? temp in Placements)
            {
                if (temp.IsSpawned) continue;
                temp.manifest = manifest;
                return temp.Spawn();
            }
            // if null, then all temp containers are spawned
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("All container placements are used!");
            return null;
        }

        public Container[] GetSpawnedContainers()
        {
            List<Container> containers = new List<Container>();
            foreach (var temp in Placements)
            {
                if (temp.SpawnedContainer != null) containers.Add(temp.SpawnedContainer);
            }

            return containers.ToArray();
        }

        public bool HasItems()
        {
            foreach (var container in GetSpawnedContainers())
            {
                if (container.GetInventory().HasItems()) return true;
            }

            return false;
        }

        public int GetCost()
        {
            List<Manifest> manifests = new();
            foreach (var temp in Placements)
            {
                if (temp.manifest == null) continue;
                manifests.Add(temp.manifest);
            }
            return manifests.Sum(i => i.CostToShip);
        }

        public List<Manifest> GetManifests()
        {
            List<Manifest> manifests = new();
            foreach (TempContainer? temp in Placements)
            {
                if (temp.manifest == null) continue;
                manifests.Add(temp.manifest);
            }
            return manifests;
        }
    }

    public class TempContainer
    {
        // class to manage spawning new containers
        // to keep relevant information organized
        // and keep the Spawn function within its own scope
        private readonly Transform transform;
        private readonly Port port;
        private readonly string slotKey;
        public Manifest? manifest;
        public bool IsSpawned => !string.IsNullOrEmpty(port.m_view.GetZDO().GetString(slotKey));
        public Container? SpawnedContainer;

        public TempContainer(Port port, Transform transform, int slot)
        {
            this.port = port;
            this.transform = transform;
            slotKey = "MWL_PortChest_" + slot;
        }

        public void Refresh()
        {
            string token = port.m_view.GetZDO().GetString(slotKey);
            if (string.IsNullOrEmpty(token))
            {
                SpawnedContainer = null;
                manifest = null;
                return;
            }
            SpawnedContainer = PortChest.Find(token);
            if (Manifest.Manifests.TryGetValue(port.m_view.GetZDO().GetInt(slotKey + "_prefab"), out Manifest found)) manifest = found;
        }

        public Container? Spawn()
        {
            if (manifest == null || !port.m_view.IsOwner() || IsSpawned) return null;
            int hash = manifest.ChestStableHashCode;
            // had to create chest by setup ZDO first
            ZDO? zdo = ZDOMan.instance.CreateNewZDO(transform.position, hash);
            // set all the data in the zdo
            zdo.Persistent = true;
            zdo.Type = ZDO.ObjectType.Default;
            zdo.Distant = false;
            zdo.SetPrefab(hash);
            zdo.SetRotation(transform.rotation);
            zdo.SetOwner(ZDOMan.GetSessionID());
            string token = Guid.NewGuid().ToString();
            zdo.Set(PortChest.TokenKey, token);
            // then tell znetscene to create object
            GameObject? chest = ZNetScene.instance.CreateObject(zdo);
            // and use that return
            Container? container = chest.GetComponent<Container>();
            // set temp container as spawned and hold reference
            SpawnedContainer = container;
            port.m_view.GetZDO().Set(slotKey, token);
            port.m_view.GetZDO().Set(slotKey + "_prefab", hash);
            manifest.PlaceEffect?.Create(chest.transform.position, chest.transform.rotation);
            return container;
        }

        public void Destroy()
        {
            if (!port.m_view.IsOwner() || SpawnedContainer == null || !SpawnedContainer.m_nview.IsValid()) return;
            SpawnedContainer.m_nview.ClaimOwnership();
            SpawnedContainer.m_nview.Destroy();
            port.m_view.GetZDO().Set(slotKey, "");
            port.m_view.GetZDO().Set(slotKey + "_prefab", 0);
            manifest = null;
            // remove reference
            SpawnedContainer = null;
        }
    }

    public class PortInfo
    {
        // class to parse ZDO into relevant information
        // and keep relevant functions within their own scope
        public readonly ShipmentManager.PortID portID;
        public readonly Vector3 position;
        public readonly List<Shipment> deliveries;
        public readonly List<Shipment> shipments;
        
        private double _estimatedDuration;
        private double EstimatedDuration
        {
            get
            {
                if (_estimatedDuration != 0 || PortUI.instance is null || PortUI.instance.m_currentPort is null) return _estimatedDuration;
                float distance = global::Utils.DistanceXZ(PortUI.instance.m_currentPort.transform.position, position);
                // calculate time by distance
                // cache result
                _estimatedDuration = Shipment.CalculateDistanceTime(distance);
                // since our ports are static (not moving)
                // no need to recalculate this
                return _estimatedDuration;
            }
        }

        private static readonly StringBuilder sb = new ();

        public PortInfo(ZDO zdo)
        {
            // cache information
            // to use to manage selected destination
            // and details about port
            portID = new ShipmentManager.PortID(zdo.GetString(PortVars.GUID), zdo.GetString(PortVars.Name));
            position = zdo.GetPosition();
            deliveries = ShipmentManager.GetDeliveries(portID.GUID)
                .Where(delivery => delivery.CanAccess(Player.m_localPlayer))
                .ToList();
            shipments = ShipmentManager.GetShipments(portID.GUID)
                .Where(shipment => shipment.CanAccess(Player.m_localPlayer))
                .ToList();
        }
        public void Reload()
        {
            deliveries.Clear();
            shipments.Clear();
            deliveries.AddRange(ShipmentManager.GetDeliveries(portID.GUID).Where(delivery => delivery.CanAccess(Player.m_localPlayer)));
            shipments.AddRange(ShipmentManager.GetShipments(portID.GUID).Where(shipment => shipment.CanAccess(Player.m_localPlayer)));
            ShipmentManager.OnShipmentsUpdated -= Reload;
        }
        
        public float GetDistance(Player player) => Vector3.Distance(player.transform.position, position);

        public string GetTooltip()
        {
            sb.Clear();

            sb.Append($"{LocalKeys.EstimatedShipTime}: <color=yellow>{Shipment.FormatTime(EstimatedDuration)}</color>\n");
            sb.Append($"\n{LocalKeys.Deliveries}(<color=yellow>{deliveries.Count}</color>): ");
            foreach (Shipment? delivery in deliveries)
            {
                double remainingTime = delivery.State == ShipmentState.InTransit 
                    ? delivery.GetTimeToArrivalSeconds() 
                    : delivery.GetTimeToExpirationSeconds();
                string time = Shipment.FormatTime(remainingTime);
                sb.AppendFormat("\n{3}: <color=orange>{0}</color> (<color=yellow>{1}</color>{2})", delivery.OriginPortName, delivery.State.ToKey(), string.IsNullOrEmpty(time) ? "" : $", {time}", LocalKeys.Origin);
            }
            sb.Append($"\n\nShipments (<color=yellow>{shipments.Count}</color>): ");
            foreach (Shipment? shipment in shipments)
            {
                double remainingTime = shipment.State == ShipmentState.InTransit 
                    ? shipment.GetTimeToArrivalSeconds() 
                    : shipment.GetTimeToExpirationSeconds();
                string time = Shipment.FormatTime(remainingTime);
                sb.AppendFormat("\n{3}: <color=orange>{0}</color> (<color=yellow>{1}</color>{2})", shipment.DestinationPortName, shipment.State.ToKey(), string.IsNullOrEmpty(time) ? "" : $", {time}", LocalKeys.Destination);
            }
            return sb.ToString();
        }
    }
    private static class PortVars
    {
        // organization sake to manage Port ZDO variables
        public static readonly int Name = "PortName".GetStableHashCode();
        public static readonly int GUID = "PortGUID".GetStableHashCode();
        public static readonly int Items = "PortItems".GetStableHashCode();
        public static readonly int TraderName = "PortTraderName".GetStableHashCode();
        public static readonly int HasOpenDelivery = "PortHasOpenDelivery".GetStableHashCode();
        public static readonly int PersistentContainers = "MWL_PortPersistentContainers".GetStableHashCode();
        
    }
}
