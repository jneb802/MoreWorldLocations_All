using System;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class Shipment
{
    public static Dictionary<Guid, Shipment> inTransitShipments = new Dictionary<Guid, Shipment>();

    public Guid m_shipmentID;
    public Port m_originPort;
    public Port m_destinationPort;
    public int m_startDay;
    public List<GameObject> m_containers = new List<GameObject>();

    public Shipment(Port originPort, Port destinationPort, int startDay, List<GameObject> containers)
    {
        m_originPort = originPort;
        m_destinationPort = destinationPort;
        m_startDay = startDay;
        m_containers = containers;
        
        // m_shipmentID = Guid.NewGuid().ToString("D");
    }

    public void SendShipment()
    {
        
    }
}