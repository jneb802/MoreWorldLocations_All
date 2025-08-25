using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Entities;
using Jotunn.Managers;
using More_World_Locations_AIO.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO.Shipments;

public class Port : MonoBehaviour, Interactable, Hoverable
{
    // Persistent fields
    public string m_portID;
    public ZNetView m_view;
    public string localizationKey;
    public string name;
    public Vector3 worldPosition;
    
    // Non-persistent fields
    public Location m_location;
    private bool m_locationInitialized = false;
    public List<GameObject> m_containerPositions;
    public List<GameObject> m_currentChests;
    public List<Shipment> shipmentsToThisPort = new List<Shipment>();
    

    public void Awake()
    {
        this.m_view = this.GetComponent<ZNetView>();
        m_portID = m_view.GetZDO().GetString("MWL_portID");
        if (string.IsNullOrEmpty(m_portID))
        {
            Debug.Log($"Port.Awake: MWL_PortID is null or empty in ZDO");
            if (m_view.IsOwner())
            {
                m_portID = Guid.NewGuid().ToString();
                m_view.GetZDO().Set("MWL_portID", m_portID);
            }
        }
        
        if (PortDB.Instance.allPorts.ContainsKey(m_portID))
        {
            worldPosition = PortDB.Instance.allPorts[m_portID].worldPosition;
            localizationKey = PortDB.Instance.allPorts[m_portID].localizationKey;
            name = LocalizationManager.Instance.TryTranslate(localizationKey);
            Debug.Log($"Port with name: {name} has id {m_portID}");
        }
        else
        {
            worldPosition = this.transform.position;
            localizationKey = PortNames.GetRandomPortName();
            name = LocalizationManager.Instance.TryTranslate(localizationKey);
            PortDB.Instance.allPorts.Add(m_portID, this);
            Debug.Log($"Port.Awake: Adding port with name: {name} and id {m_portID} to PortDB.allPorts");
        }
        
        Debug.Log($"Port with name: {name} called awake");
        Debug.Log($"Port with name: {name} has id {m_portID}");
        Debug.Log($"Port with name: {name} has worldPosition {worldPosition}");
    }

    // public void GetLocationAndContainers()
    // {
    //     LocationProxy locationProxy = WorldUtils.GetLocationInRange(this.transform.position, 50);
    //     if (locationProxy == null) Debug.Log($"Port.Awake: LocationProxy is null");
    //     
    //     m_location = locationProxy.GetComponentInChildren<Location>();
    //     if (m_location == null) Debug.Log($"Port.Awake: Location is null");
    //     
    //     GameObject containerPosition1 = locationProxy.gameObject.transform.Find("MWL_PortLocation(Clone)/Blueprint/containerPosition1")?.gameObject;
    //     GameObject containerPosition2 = locationProxy.gameObject.transform.Find("MWL_PortLocation(Clone)/Blueprint/containerPosition2")?.gameObject;
    //     GameObject containerPosition3 = locationProxy.gameObject.transform.Find("MWL_PortLocation(Clone)/Blueprint/containerPosition3")?.gameObject;
    //     
    //     m_containerPositions.Add(containerPosition1);
    //     m_containerPositions.Add(containerPosition2);
    //     m_containerPositions.Add(containerPosition3);
    //
    //     foreach (GameObject gameObject in m_containerPositions)
    //     {
    //         if (gameObject == null) Debug.Log($"Port.Awake: container position is null");
    //     } 
    // }

    private void Start()
    {
        StartCoroutine(InitializeWhenLocationReady());
    }
    
    private IEnumerator InitializeWhenLocationReady()
    {
        LocationProxy locationProxy = null;
        while (locationProxy == null)
        {
            locationProxy = WorldUtils.GetLocationInRange(this.transform.position, 50);
            if (locationProxy == null)
            {
                Debug.Log($"Port {name}: Waiting for LocationProxy...");
                yield return new WaitForSeconds(0.1f);
            }
        }
        while (locationProxy.m_instance == null)
        {
            Debug.Log($"Port {name}: LocationProxy found but location not spawned yet, waiting...");
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log($"Port {name}: Location spawned: {locationProxy.m_instance.name}");
        InitializeLocationStuff(locationProxy);
    }
    
    private void InitializeLocationStuff(LocationProxy locationProxy)
    {
        m_location = locationProxy.GetComponentInChildren<Location>();
        if (m_location == null) 
        {
            Debug.LogError($"Port {name}: Location component is null");
            return;
        }
        
        Transform locationContent = locationProxy.m_instance.transform;
        
        GameObject containerPosition1 = locationContent.Find("Blueprint/containerPosition1")?.gameObject;
        GameObject containerPosition2 = locationContent.Find("Blueprint/containerPosition2")?.gameObject;
        GameObject containerPosition3 = locationContent.Find("Blueprint/containerPosition3")?.gameObject;
        
        m_containerPositions.Add(containerPosition1);
        m_containerPositions.Add(containerPosition2);
        m_containerPositions.Add(containerPosition3);

        foreach (GameObject gameObject in m_containerPositions)
        {
            if (gameObject == null) 
                Debug.LogError($"Port {name}: container position is null");
            else
                Debug.Log($"Port {name}: Found container at {gameObject.name}");
        }
        
        m_locationInitialized = true;
        Debug.Log($"Port {name}: Location initialization complete");
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        Debug.Log($"Port with name: {name} called ineract");
        if (PortUI.portUIRoot == null) {Debug.Log("PortUI_new.portUIRoot is null");}
        
        PortUI.instance.SetTitle(this.name);
        PortUI.instance.SetupListElements();
        if (user is not Player player)
            return true;
        
        PortManager portManager = player.GetComponent<PortManager>();

        ShipmentManager.ClientRequestFullSync();
        
        List<Shipment> shipmentsToThisPortTemp = ShipmentManager.GetShipmentsForDestination(this.m_portID);
        foreach (Shipment shipment in shipmentsToThisPortTemp)
        {
            if (!shipmentsToThisPort.Contains(shipment))
            {
                shipmentsToThisPort.Add(shipment);
            }
        }
        
        List<Shipment> shipmentsAtThisPortTemp = ShipmentManager.GetShipmentsForOrigin(this.m_portID);
        foreach (Shipment shipment in shipmentsAtThisPortTemp)
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
            $"Port Manager\n" +
            $"[<color=yellow><b>$KEY_Use</b></color>] Open Port\n");
            
        return hoverText;
    }

    public string GetHoverName()
    {
        return name;
    }
    
    public void CreateContainers(List<GameObject> chests)
    {
        for (int i = 0; i < chests.Count; i++)
        {
            if (chests[i] == null) continue;
            Debug.Log($"Port.CreateContainers: Creating chest with name {chests[i].name} at position {i}");
            GameObject chest = Object.Instantiate(chests[i], m_containerPositions[i].transform.position, m_containerPositions[i].transform.rotation);
            m_currentChests.Add(chest);
        }
    }

    public void ClearShipment()
    {
        if (m_currentChests.Count == 0) return;
        foreach (GameObject chest in m_currentChests)
        {
            ZNetScene.instance.Destroy(chest);
        }
        
        m_currentChests.Clear();
    }
}