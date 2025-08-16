using System;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class Shipment
{
    public Guid m_shipmentID;
    public string m_originPortID;
    public string m_destinationPortID;
    public List<KeyValuePair<string, List<ShipmentItemData>>> shipmentContainers = new List<KeyValuePair<string, List<ShipmentItemData>>>();

    public Shipment(Port originPort, Port destinationPort, List<GameObject> chests)
    {
        m_originPortID = originPort.m_portID;
        m_destinationPortID = destinationPort.m_portID;
        
        foreach (GameObject chest in chests)
        { 
            string chestName = chest.name.Split('(')[0];
            List<ShipmentItemData> shipmentItems = new List<ShipmentItemData>();
            
            Container container = chest.GetComponent<Container>();
            Inventory inventory = container.GetInventory();
            List<ItemDrop.ItemData> chestItems = inventory.GetAllItems();
            
           foreach (ItemDrop.ItemData itemData in chestItems)
           {
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
        
        m_shipmentID = Guid.NewGuid();
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

    public Shipment()
    {
        
    }

    public void SendShipment()
    {
        
    }
}
