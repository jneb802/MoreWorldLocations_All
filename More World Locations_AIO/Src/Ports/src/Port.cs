using System;
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
    public Shipment? m_selectedDelivery;
    public Humanoid? m_currentHumanoid;
    private readonly TempItems m_tempItems = new();
    public void Awake()
    {
        m_view = GetComponent<ZNetView>();
        if (!m_view.IsValid()) return;
        
        m_name = m_view.GetZDO().GetString(PortVars.Name, NameGenerator.GenerateName());
        m_portID.GUID = m_view.GetZDO().GetString(PortVars.GUID, Guid.NewGuid().ToString());
        m_portID.Name = m_name;
        m_view.GetZDO().Set(PortVars.GUID, m_portID.GUID);
        m_view.GetZDO().Set(PortVars.Name, m_name);
        
        m_traderName = m_view.GetZDO().GetString(PortVars.TraderName, TraderNames.GetRandomName());
        m_view.GetZDO().Set(PortVars.TraderName, m_traderName);
    }

    public void Start()
    {
        if (!m_view.IsValid()) return;
        LoadSavedItems();
        
        var locationProxy = WorldUtils.GetLocationInRange(this.transform.position, 10);
        if (locationProxy == null)
        {
            Debug.LogWarning($"Port '{m_name}' could not find a LocationProxy within range. Container positions unavailable.");
            return;
        }
        
        Transform locationRoot = locationProxy.transform;
        if (m_containers.Placements.Count <= 0)
        {
            foreach (Transform child in locationRoot.FindAllRecursive("containerPosition"))
            {
                TempContainer temp = new TempContainer(child);
                m_containers.Placements.Add(temp);
            }

            if (m_containers.Placements.Count == 0)
            {
                Debug.LogWarning("No containers found");
            }
        }
    }

    public void OnDestroy()
    {
        DestroyContainers();
        // TODO: check multiplayer, if a player leaves, does it affect another player still interacting with port ??
        Manifest.ResetPurchasedManifests();
    }

    public void SaveItems()
    {
        m_tempItems.Clear();
        if (!m_view.IsValid())
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("ZNETVIEW not valid when trying to save items");
            return;
        }
        if (m_containers.HasItems())
        {
            m_tempItems.Add(m_containers.GetSpawnedContainers());
            ZPackage pkg = new ZPackage();
            pkg.Write(m_tempItems.Items.Count);
            foreach (ShipmentItem? item in m_tempItems.Items)
            {
                item.Write(pkg);
            }
            m_view.GetZDO().Set(PortVars.Items, pkg.GetBase64());
        }
        else
        {
            m_view.GetZDO().Set(PortVars.Items, "");
        }
    }

    private bool LoadSavedItems()
    {
        string? data = m_view.GetZDO().GetString(PortVars.Items);
        if (string.IsNullOrWhiteSpace(data)) return false;
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
        foreach (TempContainer? temp in m_containers.Placements)
        {
            if (temp.IsSpawned) continue;
            temp.manifest = manifest;
            Container? container = temp.Spawn();
            if (container == null) return null;
            container.GetInventory().m_onChanged = OnContainersChanged;
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
    public void OnContainersChanged()
    {
        if (ShipmentManager.instance == null) return;
        SaveItems();
        if (m_containers.HasItems() || m_selectedDelivery == null) return;
        m_selectedDelivery.OnCollected();
        m_selectedDelivery = null;
        if (m_currentHumanoid != null) m_currentHumanoid.Message(MessageHud.MessageType.Center, LocalKeys.DeliveryCollected);
        DestroyContainers();
    }
    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (PortUI.instance == null) return false;
        PortUI.instance.Show(this);
        if (user is Player player) player.AddKnownPort(m_portID);
        m_currentHumanoid = user;
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
        // remove callback
        foreach (Container container in m_containers.GetSpawnedContainers())
        {
            container.GetInventory().m_onChanged = null;
        }
        // load items
        foreach (ShipmentItem item in items)
        {
            Container? container = m_containers.GetOrCreate(item.ChestID);
            if (container == null)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"Failed to create container: {item.ChestID}, max container spawned ??");
                continue;
            }
            if (!item.AddItem(container))
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("Failed to add item: " + item.ItemName);
            }
        }
        // reset callback
        foreach (Container container in m_containers.GetSpawnedContainers())
        {
            container.GetInventory().m_onChanged = OnContainersChanged;
        }
        // save to ZDO and temp items
        OnContainersChanged();
        return true;
    }

    public bool LoadDelivery(Shipment delivery)
    {
        if (m_containers.HasItems())
        {
            if (m_currentHumanoid != null) m_currentHumanoid.Message(MessageHud.MessageType.Center, LocalKeys.FailedToLoadDelivery);
            return false;
        }
        LoadItems(delivery.Items);
        m_selectedDelivery = delivery;
        OnContainersChanged(); // checks if containers are loaded and saves to ZDO
        return m_containers.HasItems(); // if true, can use this statement to make containers visible ??
    }

    public bool SendShipment(PortInfo selectedPort)
    {
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
        if (m_currentHumanoid != null) m_currentHumanoid.Message(MessageHud.MessageType.Center, LocalKeys.SuccessfullySent);
        return true;
    }

    public string GetHoverName() => Localization.instance.Localize(m_traderName);

    public string GetTooltip()
    {
        if (!m_containers.HasItems()) return "";
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
        public Manifest? manifest;
        public bool IsSpawned => SpawnedContainer != null;
        public Container? SpawnedContainer;

        public TempContainer(Transform transform)
        {
            this.transform = transform;
        }

        public Container? Spawn()
        {
            if (manifest == null) return null;
            int hash = manifest.ChestStableHashCode;
            // had to create chest by setup ZDO first
            ZDO? zdo = ZDOMan.instance.CreateNewZDO(transform.position, hash);
            // set all the data in the zdo
            zdo.Persistent = false;
            zdo.Type = ZDO.ObjectType.Default;
            zdo.Distant = false;
            zdo.SetPrefab(hash);
            zdo.SetRotation(transform.rotation);
            zdo.SetOwner(ZDOMan.GetSessionID());
            // then tell znetscene to create object
            GameObject? chest = ZNetScene.instance.CreateObject(zdo);
            // and use that return
            Container? container = chest.GetComponent<Container>();
            // set temp container as spawned and hold reference
            SpawnedContainer = container;
            // set manifest to purchased to remove from UI list
            manifest.IsPurchased = true;
            manifest.PlaceEffect?.Create(chest.transform.position, chest.transform.rotation);
            return container;
        }

        public void Destroy()
        {
            if (SpawnedContainer == null || !SpawnedContainer.m_nview.IsValid()) return;
            if (manifest != null)
            {
                // reset manifest to make it available to purchase again
                manifest.IsPurchased = false;
                // remove reference
                manifest = null;
            }
            SpawnedContainer.m_nview.ClaimOwnership();
            SpawnedContainer.m_nview.Destroy();
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
            deliveries = ShipmentManager.GetDeliveries(portID.GUID);
            shipments = ShipmentManager.GetShipments(portID.GUID);
        }
        public void Reload()
        {
            deliveries.Clear();
            shipments.Clear();
            deliveries.AddRange(ShipmentManager.GetDeliveries(portID.GUID));
            shipments.AddRange(ShipmentManager.GetShipments(portID.GUID));
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
        
    }
}