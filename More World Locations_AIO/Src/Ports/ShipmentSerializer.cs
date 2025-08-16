using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class ShipmentSerializer
{
    public static ZPackage SerializeShipmentPackage(Shipment shipment)
    {
        ZPackage pkg = new ZPackage();
        pkg.Write(shipment.m_shipmentID.ToString());
        pkg.Write(shipment.m_originPortID);
        pkg.Write(shipment.m_destinationPortID);
        
        int shipmentContainerCount = shipment.shipmentContainers.Count;
        if (shipmentContainerCount == 0)
        {
            Debug.Log($"The list of shipmentContainers is empty for shipment with id: {shipment.m_shipmentID}");
            return null;
        }
        
        pkg.Write(shipmentContainerCount);
        for (int i = 0; i < shipmentContainerCount; i++)
        {
            pkg.Write(shipment.shipmentContainers[i].Key);
            SerializeContainerItemDataList(pkg, shipment.shipmentContainers[i].Value);
        }
        
        return pkg;
    }
    
    public static Shipment DeserializeShipmentPackage(ZPackage package)
    {
        Shipment shipment = new Shipment();

        shipment.m_shipmentID = Guid.Parse(package.ReadString());
        shipment.m_originPortID = package.ReadString();
        shipment.m_destinationPortID = package.ReadString();
        
        int shipmentContainerCount = package.ReadInt();
        shipment.shipmentContainers = new List<KeyValuePair<string, List<Shipment.ShipmentItemData>>>();
        for (int i = 0; i < shipmentContainerCount; i++)
        {
            shipment.shipmentContainers.Add(new KeyValuePair<string, List<Shipment.ShipmentItemData>>(package.ReadString(), DeserializeContainerItemDataList(package)));
        }

        return shipment;
    }

    public static void SerializeContainerItemDataList(ZPackage pkg, List<Shipment.ShipmentItemData> containerItems)
    {
        int count = containerItems.Count;
        pkg.Write(count);
        if (containerItems.Count == 0) return;
        for (int i = 0; i < count; i++)
        {
            pkg.Write(containerItems[i].m_itemName);
            pkg.Write(containerItems[i].m_stack);
            pkg.Write((double)containerItems[i].m_durability);
            pkg.Write(containerItems[i].m_quality);
            pkg.Write(containerItems[i].m_variant);
            pkg.Write(containerItems[i].m_crafterID);
            pkg.Write(containerItems[i].m_crafterName);

            List<KeyValuePair<string, string>> dictionaryList = containerItems[i].m_customData.ToList();
            int customDataCount = dictionaryList.Count;
            pkg.Write(customDataCount);
            if (customDataCount == 0) continue;
            for (int k = 0; k < customDataCount; k++)
            {
                pkg.Write(dictionaryList[k].Key);
                pkg.Write(dictionaryList[k].Value);
            }
        }
    }
        
    public static List<Shipment.ShipmentItemData> DeserializeContainerItemDataList(ZPackage pkg)
    {
        List<Shipment.ShipmentItemData> shipmentItems = new List<Shipment.ShipmentItemData>();
        
        var count = pkg.ReadInt();
        if (count == 0) return shipmentItems;
        for (int i = 0; i < count; i++)
        {
            string itemName = pkg.ReadString();
            int stack = pkg.ReadInt();
            double durability = pkg.ReadDouble();
            int quality = pkg.ReadInt();
            int variant = pkg.ReadInt();
            long crafterID = pkg.ReadLong();
            string crafterName = pkg.ReadString();
            int dictionarySize = pkg.ReadInt();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            
            if (dictionarySize == 0) continue;
            for (int k = 0; k < dictionarySize; k++)
            {
                string key = pkg.ReadString();
                string value = pkg.ReadString();
                dictionary.Add(key, value);
            }
            
            Shipment.ShipmentItemData shipmentItemData = new Shipment.ShipmentItemData(itemName, stack, (float)durability, quality, variant, crafterID, crafterName, dictionary);
            shipmentItems.Add(shipmentItemData);
            
        }
        
        return shipmentItems;
    }
    
    // public static ZPackage BuildPackage(Shipment s)
    // {
    //     ZPackage package = new ZPackage();
    //     package.Write(s.m_shipmentID.ToString("N"));
    //     package.Write(s.m_originPortID ?? "");
    //     package.Write(s.m_destinationPortID ?? "");
    //
    //     // If you need to sync containers, prefer stable IDs (e.g., ZDOIDs), not GameObject refs.
    //     // p.Write(s.ContainerIds?.Count ?? 0);
    //     // for each: p.Write(zdoid);
    //
    //     return package;
    // }
    //
    // public static Shipment ParsePackage(ZPackage p)
    // {
    //     var idStr = p.ReadString();
    //     if (!Guid.TryParse(idStr, out var id))
    //     {
    //         Debug.LogWarning($"ShipmentManager.ParsePackage: invalid GUID '{idStr}'");
    //         return null;
    //     }
    //
    //     // Shipment shipment = new Shipment
    //     // {
    //     //     m_shipmentID = id,
    //     //     m_originPortID = p.ReadString(),
    //     //     m_destinationPortID = p.ReadString(),
    //     // };
    //
    //     // If syncing containers:
    //     // int c = p.ReadInt();
    //     // s.ContainerIds = new List<ZDOID>(c);
    //     // for (int i = 0; i < c; i++) s.ContainerIds.Add(p.ReadZDOID());
    //
    //     return null;
    // }
}