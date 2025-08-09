using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using BepInEx.Configuration;
using Jotunn.Extensions;
using Jotunn.GUI;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace More_World_Locations_AIO.Shipments;

public class PortUI : MonoBehaviour
{
    public static GameObject portUIRoot;
    
    public static PortUI instance;

    public Text portTitle;
    
    public Button tabPort;
    public GameObject portsView;
    public RectTransform tabPortlistContainer;
    public Text tabPortdescription;
    public Text tabPortPrice;
    public Dropdown tabPortdropDown;
    public Button tabPortActionButton;
    
    public Button tabShipment;
    public GameObject shipmentsView;
    public RectTransform tabShipmentlistContainer;
    public Text tabShipmentdescription;
    public Button tabShipmentActionButton;

    public struct PortData
    {
        public string PortName;
        public Vector3 Position;
        
        public PortData(string portName, Vector3 position)
        {
            PortName = portName;
            Position = position;
        }
    }

    public void Awake()
    {
        instance = this;
        portTitle = transform.Find("root/background/verticalLayout1/PortTitle").GetComponent<Text>();
        
        tabPort = transform.Find("root/background/verticalLayout1/TabPort").GetComponent<Button>();
        portsView = transform.Find("root/background/verticalLayout1/PortsView").gameObject;
        tabPortlistContainer = transform.Find("root/background/verticalLayout1/PortsView/listContainer").GetComponent<RectTransform>();
        tabPortdescription = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/description").GetComponent<Text>();
        tabPortPrice = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/price").GetComponent<Text>();
        tabPortdropDown = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/dropDown").GetComponent<Dropdown>();
        tabPortActionButton = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/actionButton").GetComponent<Button>();
        
        tabShipment = transform.Find("root/background/verticalLayout1/TabShipments").GetComponent<Button>();
        shipmentsView = transform.Find("root/background/verticalLayout1/ShipmentsView").gameObject;
        tabShipmentlistContainer = transform.Find("root/background/verticalLayout1/ShipmentsView/listContainer").GetComponent<RectTransform>();
        tabShipmentdescription = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/description").GetComponent<Text>();
        tabShipmentActionButton = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/actionButton").GetComponent<Button>();
        
        tabPort.onClick.AddListener(ShowPorts);
        tabShipment.onClick.AddListener(ShowShipment);
        
        tabPortActionButton.onClick.AddListener(PurchaseShipment);
    }

    public void PurchaseShipment()
    {
        PortData port1 = new PortData(
            "Shipment A",
            new Vector3(100f, 0f, 250f)
        );
        
        ShowShipment();
        AddListElement(tabShipmentlistContainer, GUIManager.Instance.GetSprite("longship"), port1.PortName, BuildDescriptionText(port1), PortUITab.Shipments);
        CreateContainer(GetContainerSize());
    }

    public Vector2 GetContainerSize()
    {
        string selected = tabPortdropDown.options[tabPortdropDown.value].text;

        Vector2 size = new Vector2();
        
        switch (selected)
        {
            case "small container":
                size.x = 5;
                size.y = 2;
                break;

            case "medium container":
                size.x = 6;
                size.y = 4; 
                break;

            case "large container":
                size.x = 8;
                size.y = 4;
                break;

            default:
                Debug.LogWarning("Unknown container size selected: " + selected);
                break;
        }
        
        return size;
    }

    public void CreateContainer(Vector2 containerSize)
    {
        GameObject woodChest = PrefabManager.Instance.CreateClonedPrefab("shipmentContainer", "loot_chest_wood");
        Container container = woodChest.GetComponent<Container>();
        container.name = "shipmentContainer";
        container.m_width = (int)containerSize.x;
        container.m_height = (int)containerSize.y;
        
        Object.Instantiate(woodChest, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
        InventoryGui.instance.Show(container);
    }

    public void SetupListElements()
    {
        tabPortActionButton.transform.Find("Label").GetComponent<Text>().text = "Purchase Shipment";
        tabShipmentActionButton.transform.Find("Label").GetComponent<Text>().text = "Send Shipment";
        
        // GUIManager.Instance.ApplyDropdownStyle(tabPortdropDown);
        
        var dropdownObject = GUIManager.Instance.CreateDropDown(
            parent: tabPortdropDown.transform,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            position: new Vector2(0f, 0f),
            fontSize: 10,
            width: 200f,
            height: 35f);
        
        dropdownObject.GetComponent<Dropdown>().AddOptions(new List<string>
        {
            "Small Container", "Medium Container", "Large Container"
        });
        
        GUIManager.Instance.ApplyButtonStyle(tabPortActionButton);
        GUIManager.Instance.ApplyButtonStyle(tabShipmentActionButton);
        
        GUIManager.Instance.ApplyTextStyle(portTitle, 36);
        
        foreach (Text text in new List<Text> {tabPortdescription, tabShipmentdescription })
        {
            GUIManager.Instance.ApplyTextStyle(text, 18);
        }
        
        foreach (Text text in new List<Text> {tabPort.transform.Find("Label").GetComponent<Text>(), tabShipment.transform.Find("Label").GetComponent<Text>() })
        {
            GUIManager.Instance.ApplyTextStyle(text,18);
        }
        
        PortData port1 = new PortData(
            "Seaside Dock",
            new Vector3(100f, 0f, 250f)
        );
        
        PortData port2 = new PortData(
            "Dangerous Cove",
            new Vector3(100f, 0f, 250f)
        );
        
        PortData port3 = new PortData(
            "Halbur Harbor",
            new Vector3(100f, 0f, 250f)
        );

        AddListElement(tabPortlistContainer, GUIManager.Instance.GetSprite("longship"), port1.PortName, BuildDescriptionText(port1), PortUITab.Ports);
        AddListElement(tabPortlistContainer, GUIManager.Instance.GetSprite("longship"), port2.PortName, BuildDescriptionText(port2), PortUITab.Ports);
        AddListElement(tabPortlistContainer, GUIManager.Instance.GetSprite("longship"), port3.PortName, BuildDescriptionText(port3), PortUITab.Ports);
    }

    public void AddListElement(RectTransform parent, Sprite icon, string portName, string portDescription, PortUITab tab)
    {
        var element = Object.Instantiate(PortPrefabs.portUIListItem, parent);
        
        var image = element.transform.Find("Icon").GetComponent<Image>();
        image.sprite = icon;
        
        var label = element.transform.Find("Label").GetComponent<Text>();
        GUIManager.Instance.ApplyTextStyle(label);
        label.text = portName;
        
        var button = element.GetComponent<Button>();
        button.onClick.AddListener(() => UpdateDescription(portDescription, tab)); 
    }

    public void ClearRegisteredPlayerList()
    {
        // add logic to clear the items in list container
        // run this each time the UI is shown to make sure it doesn't show other players stuff
        // destroy each child is the list container
    }

    public void UpdateDescription(string description, PortUITab tab)
    {
        if (tab == PortUITab.Ports)
        {
            tabPortdescription.text = description;  
        }
        else
        {
            tabShipmentdescription.text = description;
        }
        
    }

    public int GetDistance()
    {
        
        return 1;
    }

    public string BuildDescriptionText(PortData portData)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Destination:");
        sb.AppendLine(portData.PortName);
        sb.AppendLine();

        sb.AppendLine("Distance:");
        sb.AppendLine(portData.PortName); // change to the correct field
        sb.AppendLine();

        sb.AppendLine("ETA:");
        sb.AppendLine("destinationPortETA"); // replace with correct value

        return sb.ToString();
    }

    public void ShowShipment()
    {
        portsView.SetActive(false);
        shipmentsView.SetActive(true);
    }
    
    public void ShowPorts()
    {
        shipmentsView.SetActive(false);
        portsView.SetActive(true);
    }

    public void SetTitle(string text)
    {
        portTitle.text = text;
    }
    
    public static void CreatePortUI()
    {
        Debug.Log("Creating PortUI");
        portUIRoot = Object.Instantiate(PortPrefabs.portUI, GUIManager.CustomGUIFront ? GUIManager.CustomGUIFront.transform : null);
        PortUI portUI = portUIRoot.AddComponent<PortUI>();

        portUIRoot.GetComponent<RectTransform>().transform.localPosition = new Vector3(1025f, 80f, 0f);
        
        portUIRoot.SetActive(false);
    }
    
    public void Show() 
    {
        Debug.Log("Calling Show");
        
        this.gameObject.SetActive(true);
    }

    public enum PortUITab
    {
        Ports,
        Shipments,
    }
}