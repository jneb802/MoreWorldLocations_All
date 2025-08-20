using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class Shipment
{
    public Guid m_shipmentID;
    public string m_originPortID;
    public string m_destinationPortID;
    public List<KeyValuePair<string, List<ShipmentItemData>>> shipmentContainers = new List<KeyValuePair<string, List<ShipmentItemData>>>();
    public ShipmentState m_shipmentState;

    public Shipment(Port originPort, Port destinationPort, List<GameObject> chests)
    {
        m_originPortID = originPort.m_portID;
        m_destinationPortID = destinationPort.m_portID;
        
        foreach (GameObject chest in chests)
        { 
            CreateShipmentItemData(chest);
        }
        
        m_shipmentID = Guid.NewGuid();
    }
    
    public Shipment(Port originPort, Port destinationPort, ShipmentState shipmentState)
    {
        m_originPortID = originPort.m_portID;
        m_destinationPortID = destinationPort.m_portID;
        m_shipmentID = Guid.NewGuid();
        m_shipmentState = shipmentState;
    }
    
    public Shipment()
    {
        
    }

    public void CreateShipmentItemData(GameObject chest)
    {
        string chestName = chest.name.Split('(')[0];
        List<ShipmentItemData> shipmentItems = new List<ShipmentItemData>();
            
        Container container = chest.GetComponent<Container>();
        Inventory inventory = container.GetInventory();
        List<ItemDrop.ItemData> chestItems = inventory.GetAllItems();
            
        foreach (ItemDrop.ItemData itemData in chestItems)
        {
            Debug.Log($"m_dropPrefab:   {itemData.m_dropPrefab?.name ?? "null"}");
            Debug.Log($"m_stack:        {itemData.m_stack}");
            Debug.Log($"m_durability:   {itemData.m_durability}");
            Debug.Log($"m_quality:      {itemData.m_quality}");
            Debug.Log($"m_variant:      {itemData.m_variant}");
            Debug.Log($"m_crafterID:    {itemData.m_crafterID}");
            Debug.Log($"m_crafterName:  {itemData.m_crafterName}");
            
            ShipmentItemData shipmentItemData = new ShipmentItemData(
                itemData.m_dropPrefab.name,
                itemData.m_stack,
                itemData.m_durability,
                itemData.m_quality,
                itemData.m_variant,
                itemData.m_crafterID,
                itemData.m_crafterName,
                itemData.m_customData
            );               
            shipmentItems.Add(shipmentItemData);
        }
           
        shipmentContainers.Add(new KeyValuePair<string, List<ShipmentItemData>>(chestName, shipmentItems));
    }
    
    public List<GameObject> ReadShipmentItemData(Port port)
    {
        Debug.Log($"Shipment.ReadShipmentItemData: {m_shipmentID} ");
        List<GameObject> shipmentChests = new List<GameObject>();
        Debug.Log($"Shipment.ReadShipmentItemData: Shipment with ID: {m_shipmentID} has {shipmentContainers.Count} chests");
        
        for (int i = 0; i < shipmentContainers.Count; i++)
        {
            string chestName = shipmentContainers[i].Key;
            Debug.Log($"Shipment.ReadShipmentItemData: Reading ShipmentItemData for container with name {chestName} ");
            GameObject chestPrefab = PrefabManager.Instance.GetPrefab(chestName);
            
            {
                GameObject chest = UnityEngine.Object.Instantiate(chestPrefab, port.m_containerPositions[i].transform.position, port.m_containerPositions[i].transform.rotation);;
                Container container = chest.GetComponent<Container>();
                Inventory inventory = container.GetInventory();
                foreach (ShipmentItemData shipmentItemData in shipmentContainers[i].Value)
                {
                    Debug.Log($"Shipment.ReadShipmentItemData: Reading ShipmentItemData item with name {shipmentItemData.m_itemName}");
                    ItemDrop.ItemData itemData = new ItemDrop.ItemData();
                    Debug.Log($"Shipment.ReadShipmentItemData: Initialized new ItemData instance");
                    GameObject prefab = PrefabManager.Instance.GetPrefab(shipmentItemData.m_itemName);
                    if (prefab == null) { Debug.Log($"Shipment.ReadShipmentItemData: Failed to get prefab with name {shipmentItemData.m_itemName} from Jotunn PrefabManager"); }
                    Debug.Log($"Shipment.ReadShipmentItemData: Jotunn Prefab Manager returned: {prefab}");
                    
                    ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
                    itemData = itemDrop.m_itemData;
                    itemData.m_dropPrefab = prefab;
                    itemData.m_stack = shipmentItemData.m_stack;
                    itemData.m_durability = shipmentItemData.m_durability;
                    itemData.m_quality = shipmentItemData.m_quality;
                    itemData.m_variant = shipmentItemData.m_variant;
                    itemData.m_crafterID = shipmentItemData.m_crafterID;
                    itemData.m_crafterName = shipmentItemData.m_crafterName;
                    itemData.m_customData = shipmentItemData.m_customData;

                    Debug.Log($"m_dropPrefab:   {itemData.m_dropPrefab?.name ?? "null"}");
                    Debug.Log($"m_stack:        {itemData.m_stack}");
                    Debug.Log($"m_durability:   {itemData.m_durability}");
                    Debug.Log($"m_quality:      {itemData.m_quality}");
                    Debug.Log($"m_variant:      {itemData.m_variant}");
                    Debug.Log($"m_crafterID:    {itemData.m_crafterID}");
                    Debug.Log($"m_crafterName:  {itemData.m_crafterName}");
                
                    inventory.AddItem(itemData);
                }
            
                shipmentChests.Add(chest);
                
            }
        }
        
        return shipmentChests;
    }
    
    public class ShipmentItemData
    {
        public string m_itemName;
        public int m_stack = 1;
        public float m_durability = 100f;
        public int m_quality = 1;
        public int m_variant;
        public long m_crafterID;
        public string m_crafterName = "";
        public Dictionary<string, string> m_customData = new Dictionary<string, string>();

        public ShipmentItemData(string itemName, int stack, float durability, int quality, int variant, long crafterID, string crafterName, Dictionary<string, string> customData)
        {
            m_itemName = itemName;
            m_stack = stack;
            m_durability = durability;
            m_quality = quality;
            m_variant = variant;
            m_crafterID = crafterID;
            m_crafterName = crafterName;
            m_customData = customData;
        }
    }

    public void SendShipment()
    {
        
    }

    public enum ShipmentState
    {
        InPortOfOrigin,
        InTransit,
        InPortOfDestination,
    }
}
