using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class Port : MonoBehaviour, Interactable, Hoverable
{
    public string m_portID;
    public ZNetView m_view;
    public string localizationKey;
    public string name;
    public float distance;     // in meters
    public string etaFormatted;         // formatted string, e.g., "3m 42s"
    public int price;
    
    public Vector3 worldPosition;
    public Heightmap.Biome biome;

    public List<Shipment> shipmentsToThisPort = new List<Shipment>();

    public void Awake()
    {
        this.localizationKey = PortNames.GetRandomPortName();
        this.name = LocalizationManager.Instance.TryTranslate(localizationKey);

        Debug.Log($"Port with name: {name} called awake");
        
        this.m_view = this.GetComponentInParent<ZNetView>();
        if (m_view == null) {Debug.LogError("Port with name " + name + ", ZNetview is null");}

        m_portID = m_view.GetZDO().GetString("MWL_portID", string.Empty);
        if (string.IsNullOrEmpty(m_portID))
        {
            if (m_view.GetZDO().IsOwner())
            {
                this.m_portID = Guid.NewGuid().ToString("N");
                m_view.GetZDO().Set("MWL_portID", m_portID);
            }
        }
        
        if(!PortManager.allPorts.ContainsKey(m_portID))
        {
            PortManager.allPorts.Add(m_portID, this);
            Debug.Log($"Adding port with name: {name} and id {m_portID} to allPorts");
        }
        
        Debug.Log($"Port with name: {name} has id {m_portID}");
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        Debug.Log($"Port with name: {name} called ineract");
        if (PortUI.portUIRoot == null) {Debug.Log("PortUI_new.portUIRoot is null");}
        PortUI.instance.SetTitle(this.name);
        PortUI.instance.SetupListElements();
        if (user is not Player player)
            return true;
        
        PortManager portManager = player.gameObject.GetComponent<PortManager>();

        List<Shipment> shipmentsToThisPortTemp = ShipmentManager.GetShipmentsForDestination(this.m_portID);
        foreach (Shipment shipment in shipmentsToThisPortTemp)
        {
            if (!shipmentsToThisPort.Contains(shipment))
            {
                shipmentsToThisPort.Add(shipment);
            }
        }
        
        PortUI.instance.Show(this, portManager.GetPlayerPorts(), shipmentsToThisPort);
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        Debug.Log("added");
        return true;
    }

    public string GetHoverText()
    {
        string hoverText;
            
        hoverText = Localization.instance.Localize(
            $"Port\n" +
            $"[<color=yellow><b>$KEY_Use</b></color>] Open Port\n");
            
        return hoverText;
    }

    public string GetHoverName()
    {
        return Localization.instance.Localize(this.name);
    }
}