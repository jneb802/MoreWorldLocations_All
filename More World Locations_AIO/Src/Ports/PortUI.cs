using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Extensions;
using Jotunn.GUI;
using Jotunn.Managers;
using More_World_Locations_AIO.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

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
    public List<Dropdown> tabPortDropsdowns;
    public static Sprite coinsSprite;
    public static Sprite woodSprite;
    public static Sprite fineWoodSprite;
    public static Sprite ironSprite;
    public static Sprite tarSprite;
    public static Sprite blackMetalSprite;
    public Requirement tabPortRequirement1;
    public Requirement tabPortRequirement2;
    public Requirement tabPortRequirement3;
    public Requirement tabPortRequirement4;
    public Requirement tabPortRequirement5;
    public Requirement tabPortRequirement6;
    public List<Requirement> tabPortRequirements = new List<Requirement>();
    Dictionary<string, int> requirements = new Dictionary<string, int>();
    public bool requirementsChanged;
    private bool _dropdownsBuilt = false;
    public Button tabPortActionButton;
    // Locate button
    public Button tabPortActionButton2;
    public Button tabShipment;
    public GameObject shipmentsView;
    public RectTransform tabShipmentlistContainer;
    public Text tabShipmentdescription;
    public Button tabShipmentActionButton1;
    public Button tabShipmentActionButton2;
    public Button tabShipmentActionButton3;
    public Port currentPort;
    public Port selectedPort;
    public Shipment selectedShipment;
    public List<GameObject> selectedPortListItem = new List<GameObject>();
    public List<GameObject> selectedShipmentListItem = new List<GameObject>();
    
    public void Awake()
    {
        instance = this;
        
        this.gameObject.AddComponent<DragWindowCntrl>();

        tabPortDropsdowns = new List<Dropdown>();
        tabPort = transform.Find("root/background/verticalLayout1/TabPort")?.GetComponent<Button>();
        Button tabPorttUpgrade = transform.Find("root/background/verticalLayout1/TabPort/UPGRADE")?.GetComponent<Button>();
        portTitle = transform.Find("root/background/verticalLayout1/PortTitle")?.GetComponent<Text>();
        portsView = transform.Find("root/background/verticalLayout1/PortsView")?.gameObject;
        tabPortlistContainer = transform.Find("root/background/verticalLayout1/PortsView/listContainer")?.GetComponent<RectTransform>();
        tabPortdescription = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/description")?.GetComponent<Text>();
        tabPortActionButton = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/actionButton")?.GetComponent<Button>();
        tabPortActionButton2 = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/actionButton2")?.GetComponent<Button>();
        
        var requirement1 = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup1")?.gameObject;
        var requirement2 = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup2")?.gameObject;
        var requirement3 = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup3")?.gameObject;
        var requirement4 = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup4")?.gameObject;
        var requirement5 = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup5")?.gameObject;
        var requirement6 = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup6")?.gameObject;

        tabPortRequirement1 = new Requirement(requirement1);
        tabPortRequirement2 = new Requirement(requirement2);
        tabPortRequirement3 = new Requirement(requirement3);
        tabPortRequirement4 = new Requirement(requirement4);
        tabPortRequirement5 = new Requirement(requirement5);
        tabPortRequirement6 = new Requirement(requirement6);

        tabPortRequirements.Clear();
        tabPortRequirements.Add(tabPortRequirement1);
        tabPortRequirements.Add(tabPortRequirement2);
        tabPortRequirements.Add(tabPortRequirement3);
        tabPortRequirements.Add(tabPortRequirement4);
        tabPortRequirements.Add(tabPortRequirement5);
        tabPortRequirements.Add(tabPortRequirement6);

        foreach (var requirment in tabPortRequirements)
        {
            requirment.requirementParentObject.SetActive(false);
        }
        
        tabShipment = transform.Find("root/background/verticalLayout1/TabPort")?.GetComponent<Button>();
        Button tabShipmentUpgrade = transform.Find("root/background/verticalLayout1/TabShipments/UPGRADE")?.GetComponent<Button>();
        shipmentsView = transform.Find("root/background/verticalLayout1/ShipmentsView")?.gameObject;
        tabShipmentlistContainer = transform.Find("root/background/verticalLayout1/ShipmentsView/listContainer")?.GetComponent<RectTransform>();
        tabShipmentdescription = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/description")?.GetComponent<Text>();
        tabShipmentActionButton1 = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/actionButton1")?.GetComponent<Button>();
        tabShipmentActionButton2 = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/actionButton2")?.GetComponent<Button>();
        tabShipmentActionButton3 = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/actionButton3")?.GetComponent<Button>();
        
        if (tabPorttUpgrade) tabPorttUpgrade.onClick.AddListener(ShowPorts);
        if (tabShipmentUpgrade) tabShipmentUpgrade.onClick.AddListener(ShowShipment);
        
        if (tabPortActionButton) tabPortActionButton.onClick.AddListener(PurchaseShipment);
        if (tabPortActionButton2) tabPortActionButton2.onClick.AddListener(LocatePort);
        if (tabShipmentActionButton1) tabShipmentActionButton1.onClick.AddListener(() => OpenShipment(selectedShipment));
        if (tabShipmentActionButton2) tabShipmentActionButton2.onClick.AddListener(() => SendShipment(selectedShipment));
        if (tabShipmentActionButton3) tabShipmentActionButton3.onClick.AddListener(() => CloseShipment(selectedShipment));

        coinsSprite = GUIManager.Instance.GetSprite("coins");
        woodSprite = GUIManager.Instance.GetSprite("wood");
        fineWoodSprite = GUIManager.Instance.GetSprite("finewood");
        ironSprite = GUIManager.Instance.GetSprite("iron");
        blackMetalSprite = GUIManager.Instance.GetSprite("blackmetal");
        tarSprite = GUIManager.Instance.GetSprite("tar");
    }
    
    public void SetupTabs()
    {
        var tabPortTransform = transform.Find("root/background/verticalLayout1/TabPort");
        GameObject portTabButtonGameObject = Object.Instantiate(InventoryGui.instance.m_tabUpgrade, tabPortTransform.transform).gameObject;
        Button portTabButton = portTabButtonGameObject.GetComponent<Button>();
        GameObject portTabGameObject = portTabButton.gameObject;
        RectTransform rectTransformPort = portTabGameObject.GetComponent<RectTransform>();
        rectTransformPort.pivot = new Vector2(0, 1);
        rectTransformPort.localPosition = new Vector3(0, 0, 0);
        Transform portTabLabelTransform = portTabGameObject.transform.Find("Text");
        TextMeshProUGUI portTabText = portTabLabelTransform.GetComponent<TextMeshProUGUI>();
        portTabText.text = "Ports";
        if (portTabButton) portTabButton.onClick.AddListener(ShowPorts);
        
        var tabShipmentTransform = transform.Find("root/background/verticalLayout1/TabShipments");
        GameObject shipmentTabButtonGameObject = Object.Instantiate(InventoryGui.instance.m_tabUpgrade, tabShipmentTransform.transform).gameObject;
        Button shipmentTabButton = shipmentTabButtonGameObject.GetComponent<Button>();
        GameObject shipmentTabGameObject = shipmentTabButton.gameObject;
        RectTransform rectTransformShipment = shipmentTabGameObject.GetComponent<RectTransform>();
        rectTransformShipment.pivot = new Vector2(0, 1);
        rectTransformShipment.localPosition = new Vector3(0, 0, 0);
        Transform shipmentTabLabelTransform = shipmentTabGameObject.transform.Find("Text");
        TextMeshProUGUI shipmentTabText = shipmentTabLabelTransform.GetComponent<TextMeshProUGUI>();
        shipmentTabText.text = "Shipments";
        if (shipmentTabButton) shipmentTabButton.onClick.AddListener(ShowShipment);
    }

    public void LocatePort()
    {
        if (selectedPort == null) return;
        this.Hide();
        Minimap.instance.ShowPointOnMap(selectedPort.worldPosition);
    }
    
    public void SetupListElements()
    {
        tabPortActionButton.transform.Find("Label").GetComponent<Text>().text = "Purchase Shipment";
        tabShipmentActionButton1.transform.Find("Label").GetComponent<Text>().text = "Open Shipment";
        tabShipmentActionButton2.transform.Find("Label").GetComponent<Text>().text = "Send Shipment";
        tabShipmentActionButton3.transform.Find("Label").GetComponent<Text>().text = "Close Shipment";
        
        BuildDropdowns();
        
        GUIManager.Instance.ApplyButtonStyle(tabPortActionButton);
        GUIManager.Instance.ApplyButtonStyle(tabPortActionButton2);
        GUIManager.Instance.ApplyButtonStyle(tabShipmentActionButton1);
        GUIManager.Instance.ApplyButtonStyle(tabShipmentActionButton2);
        GUIManager.Instance.ApplyButtonStyle(tabShipmentActionButton3);
        
        GUIManager.Instance.ApplyTextStyle(portTitle, 36);
        
        foreach (Text text in new List<Text> {tabPortdescription, tabShipmentdescription })
        {
            GUIManager.Instance.ApplyTextStyle(text, 18);
        }
        
        foreach (Text text in new List<Text> {tabPort.transform.Find("UPGRADE/Text").GetComponent<Text>(), tabShipment.transform.Find("UPGRADE/Text").GetComponent<Text>() })
        {
            GUIManager.Instance.ApplyTextStyle(text,18);
        }
        
        foreach (Text[] textArray in new List<Text[]>
                 
                 {
                     tabPortRequirement1.requirementParentObject.GetComponentsInChildren<Text>(),
                     tabPortRequirement2.requirementParentObject.GetComponentsInChildren<Text>(),
                     tabPortRequirement3.requirementParentObject.GetComponentsInChildren<Text>(),
                     tabPortRequirement4.requirementParentObject.GetComponentsInChildren<Text>(),
                     tabPortRequirement5.requirementParentObject.GetComponentsInChildren<Text>(),
                     tabPortRequirement6.requirementParentObject.GetComponentsInChildren<Text>()
                 })
        {
            foreach (Text text in textArray)
            {
                GUIManager.Instance.ApplyTextStyle(text, 18);
            }
        }
    }
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Hide();
        }

        if (requirementsChanged)
        {
            UpdateRequirements();
        }
    }

    public void UpdateRequirements()
    {
        requirementsChanged = false;

        requirements.Clear();
        
        foreach (Dropdown dropdown in tabPortDropsdowns)
        {
            Dictionary<string, int> requirementsDropdown = GetRequirements(dropdown.options[dropdown.value].text);
            foreach (string item in requirementsDropdown.Keys)
            {
                if (requirements.TryGetValue(item, out int current))
                    requirements[item] = current + requirementsDropdown[item];
                else
                    requirements[item] = requirementsDropdown[item];
            }
        }
        
        SetRequirements(requirements);
    }

    public void SetRequirements(Dictionary<string, int> requirements)
    {
        var items = new List<KeyValuePair<string,int>>(requirements);
        for (int i = 0; i < tabPortRequirements.Count; i++)
        {
            var ui = tabPortRequirements[i];
            if (ui == null) continue;

            if (i < items.Count)
            {
                var (name, value) = (items[i].Key, items[i].Value);
                ui.requirementImage.sprite = GetRequirementSprite(name);
                ui.requirementText.text    = name;
                ui.requirementValue.text   = value.ToString();
                ui.requirementParentObject.SetActive(true);
            }
            else
            {
                ui.requirementParentObject.SetActive(false); // hide unused rows
            }
        }
     
    }
    
    public Sprite GetRequirementSprite(string name)
    {
        switch (name) // with OrdinalIgnoreCase dict above you can also do name = name.ToLowerInvariant()
        {
            case "Coins":      return coinsSprite;
            case "Wood":       return woodSprite;
            case "Finewood":   return fineWoodSprite;
            case "Iron":       return ironSprite;
            case "Blackmetal": return blackMetalSprite;
            case "Tar":        return tarSprite;
            default:           return coinsSprite;
        }
    }

    public Dictionary<string, int> GetRequirements(string itemName)
    {
        Dictionary<string, int> requirements = new Dictionary<string, int>();

        switch (itemName)
        {
            case "Empty":
                break;
            case "Wood Chest":
                requirements["Wood"] = 10;
                break;
            case "Finewood Chest":
                requirements["Finewood"] = 10;  // <— match sprite switch
                requirements["Iron"] = 2;
                break;
            case "Blackmetal Chest":
                requirements["Wood"] = 10;
                requirements["Tar"] = 2;
                requirements["Blackmetal"] = 6; // <— match sprite switch
                break;
        }

        
        return requirements;
    }

    

    public void SetSelectedPort(Port port, GameObject selectedMarker)
    {
        // deactivate previous if one exists
        if (selectedPortListItem.Count > 0)
        {
            selectedPortListItem[0].SetActive(false);
            selectedPortListItem.Clear();
        }

        // activate the new one and store it
        selectedMarker.SetActive(true);
        selectedPortListItem.Add(selectedMarker);

        Debug.Log($"Setting selected port to: {port.name}");
        this.selectedPort = port;
    }
    
    public void SetSelectedShipment(Shipment shipment, GameObject selectedMarker)
    {
        // deactivate previous if one exists
        if (selectedShipmentListItem.Count > 0)
        {
            selectedShipmentListItem[0].SetActive(false);
            selectedShipmentListItem.Clear();
        }

        // activate the new one and store it
        selectedMarker.SetActive(true);
        selectedShipmentListItem.Add(selectedMarker);
        
        Debug.Log($"Setting selected port to: {shipment.m_shipmentID}");
        this.selectedShipment = shipment;
    }

    public void OpenShipment(Shipment shipment)
    {
        Debug.Log($"PortUI.OpenShipment: Openning shipment for shipment with ID: {shipment.m_shipmentID}");
        ShipmentManager.InTransitShipments.TryGetValue(shipment.m_shipmentID, out Shipment shipmentTemp);
        if (shipmentTemp == null) {Debug.Log($"PortUI.OpenShipment: Failed to get shipment with ID from InTransitShipments: {shipment.m_shipmentID}");}
        List<GameObject> shipmentChests = shipmentTemp.ReadShipmentItemData(currentPort);

        foreach (GameObject shipmentChest in shipmentChests)
        {
            currentPort.m_currentChests.Add(shipmentChest);
        }
    }

    public void SendShipment(Shipment shipment)
    {
        Debug.Log($"PortUI.SendShipment: Starting send shipment for shipment with ID: {shipment.m_shipmentID}");
        Debug.Log($"PortUI.SendShipment: Shipment has chests: {currentPort.m_currentChests.Count}");
        foreach (GameObject chest in currentPort.m_currentChests)
        {
            shipment.CreateShipmentItemData(chest);
        }

        shipment.m_shipmentState = Shipment.ShipmentState.InTransit;
        
        ShipmentManager.ClientRequestCreateShipment(shipment);
        
        currentPort.ClearShipment();
    }
    
    public void CloseShipment(Shipment shipment)
    {
        Debug.Log($"PortUI.ClearShipment: Clearing shipment for shipment with ID: {shipment.m_shipmentID}");
        for (int i = 0; i < currentPort.m_currentChests.Count; i++)
        {
            shipment.UpdateShipmentItemData(currentPort.m_currentChests[i], i);
        }
        
        ShipmentManager.ClientRequestCreateShipment(shipment);
        
        currentPort.ClearShipment();
    }

    public void PurchaseShipment()
    {
        Debug.Log($"PortUI.PurchaseShipment: Calling purchase shipment");
        // if (selectedPort == null)
        // {
        //     Debug.Log($"PortUI.PurchaseShipment: No selected port");
        //     Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Please select a destination port");
        //     return;
        // }
        
        currentPort.ClearShipment();
        if (!CheckPlayerInventory(Player.m_localPlayer, requirements, out var missing))
        {
            var msg = BuildMissingMessage(missing); // see helper below
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg);
            return;
        }
        
        ConsumePlayerInventory(Player.m_localPlayer, requirements);
        ShowShipment();

        List<GameObject> chests = GetContainerTypes();
        currentPort.CreateContainers(chests);
        
        Shipment shipment = new Shipment(currentPort, selectedPort, Shipment.ShipmentState.InPortOfOrigin);
        ShipmentManager.ClientRequestCreateShipment(shipment);
        
        AddShipmentListElement(tabShipmentlistContainer, GUIManager.Instance.GetSprite("longship"), shipment, PortUITab.Shipments);
        selectedPort = null;
    }
    
    private string BuildMissingMessage(Dictionary<string,int> missing)
    {
        if (missing == null || missing.Count == 0) return "All requirements met.";
        var sb = new StringBuilder("Missing: ");
        bool first = true;  
        foreach (var kv in missing)
        {
            if (!first) sb.Append(", ");
            first = false;
            // If you prefer display names, reverse-map here; otherwise shared names are fine.
            sb.Append($"{kv.Key} x{kv.Value}");
        }
        return sb.ToString();
    }
    
    public bool CheckPlayerInventory(Player player, Dictionary<string, int> requirements, out Dictionary<string, int> missing)
    {
        missing = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (player == null) return false;

        Inventory inv = player.GetInventory();
        if (inv == null) return false;

        foreach (var kv in requirements)
        {
            string sharedName = ResolveSharedName(kv.Key);
            int needed = kv.Value;
            
            int have = inv.CountItems(sharedName, quality: -1, matchWorldLevel: false);

            if (have < needed)
            {
                missing[sharedName] = needed - have;
            }
        }

        return missing.Count == 0;
    }
    
    public void ConsumePlayerInventory(Player player, Dictionary<string, int> requirements)
    {
        if (player == null)
        {
            Debug.LogError($"Player {player.name} is not in inventory");
            return;
        }
        
        var inv = player.GetInventory();
        foreach (var kv in requirements)
        {
            string sharedName = ResolveSharedName(kv.Key);
            int amount = kv.Value;
            
            inv.RemoveItem(sharedName, amount, itemQuality: -1, worldLevelBased: false);
        }
    }

    public GameObject GetChestType(Dropdown dropdown)
    {
        string selected = dropdown.options[dropdown.value].text;

        GameObject prefab = new GameObject();
        
        switch (selected)
        {
            case "Empty":
                prefab = null;
                break;

            case "Wood Chest":
                prefab = PrefabManager.Instance.GetPrefab("MWL_portContainer_woodChest");
                if (prefab == null) {Debug.Log($"Failed to get chest with name: {selected} from PrefabManager"); }
                break;

            case "Finewood Chest":
                prefab = PrefabManager.Instance.GetPrefab("MWL_portContainer_finewoodChest");
                if (prefab == null) {Debug.Log($"Failed to get chest with name: {selected} from PrefabManager"); }
                break;
            
            case "Blackmetal Chest":
                prefab = PrefabManager.Instance.GetPrefab("MWL_portContainer_blackmetalChest");
                if (prefab == null) {Debug.Log($"Failed to get chest with name: {selected} from PrefabManager"); }
                break;

            default:
                prefab = null;
                Debug.LogWarning("Unknown container selected: " + selected);
                break;
        }
        
        return prefab;
    }
    
    public List<GameObject> GetContainerTypes()
    {
        List<GameObject> containers = new List<GameObject>();

        Debug.Log("CreateContainers: Starting dropdown assignment");
        foreach (Dropdown dropdown in tabPortDropsdowns)
        {
            if (dropdown == null) { Debug.Log($"CreateContainers: A drop down is null while looping through tabPortDropdowns"); continue; }
            
            GameObject prefab = GetChestType(dropdown);
            if (prefab == null) { Debug.Log($"PortUI.GetContainerTypes: Prefab is null"); continue; }
            Debug.Log($"PortUI.GetContainerTypes: Adding container: {prefab.name} to containers list");
            containers.Add(prefab);
        }
        
        return containers;
    }

    public void BuildDropdowns()
    {
        if (_dropdownsBuilt) return;   // <-- guard (important if SetupListElements can run more than once)
        _dropdownsBuilt = true;
        
        List<string> dropDownOptions = new List<string>
        {
            "Empty", "Wood Chest", "Finewood Chest", "Blackmetal Chest"
        };
        
        string dropDownPath = "root/background/verticalLayout1/PortsView/descriptionContainer/dropDownButtons/";
        
        List<GameObject> dropdownsGameObjects = new List<GameObject>();
        dropdownsGameObjects.Add(transform.Find(dropDownPath + "dropDown1")?.gameObject);
        dropdownsGameObjects.Add(transform.Find(dropDownPath + "dropDown2")?.gameObject);
        dropdownsGameObjects.Add(transform.Find(dropDownPath + "dropDown3")?.gameObject);

        foreach (GameObject gameObject in dropdownsGameObjects)
        {
            GameObject dropdownObject = GUIManager.Instance.CreateDropDown(
                parent: gameObject.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, 0f),
                fontSize: 10,
                width: 200f,
                height: 35f);

            Dropdown dropdown = dropdownObject.GetComponent<Dropdown>();
            dropdown.AddOptions(dropDownOptions);
            dropdown.onValueChanged.AddListener(selectedIndex => requirementsChanged = true);
            
            tabPortDropsdowns.Add(dropdown);
        }
    }

    

    public void SetListElements(List<Port> ports, List<Shipment> shipments)
    {
        foreach (Port port in ports)
        {
            if (port.m_portID == currentPort.m_portID) continue;
            AddPortListElement(tabPortlistContainer, GUIManager.Instance.GetSprite("longship"), port, PortUITab.Ports);
            Debug.Log($"PortUI.SetListElements: Adding port with name: {port.name}");
        }
        
        foreach (Shipment shipment in shipments)
        {
            AddShipmentListElement(tabShipmentlistContainer, GUIManager.Instance.GetSprite("longship"), shipment, PortUITab.Shipments);
            Debug.Log($"PortUI.SetListElements: Adding shipment with id: {shipment.m_shipmentID}");
        }
    }

    public void AddPortListElement(RectTransform parent, Sprite icon, Port port, PortUITab tab)
    {
        var element = Object.Instantiate(PortPrefabs.portUIListItem, parent);
        
        var image = element.transform.Find("Icon").GetComponent<Image>();
        image.sprite = icon;
        
        var label = element.transform.Find("Label").GetComponent<Text>();
        GUIManager.Instance.ApplyTextStyle(label, 12);
        label.text = port.name;

        string description = BuildDescriptionText(port);
        
        Transform selected = element.transform.Find("selected");
        
        var button = element.GetComponent<Button>();
        button.onClick.AddListener(() => UpdateDescription(description, tab));
        button.onClick.AddListener(() => SetSelectedPort(port, selected.gameObject));
        
        Debug.Log($"PortUI.AddPortListElement: Adding port list element for port: {port.name}");
    }
    
    public void AddShipmentListElement(RectTransform parent, Sprite icon, Shipment shipment, PortUITab tab)
    {
        var element = Object.Instantiate(PortPrefabs.portUIListItem, parent);
        
        var image = element.transform.Find("Icon").GetComponent<Image>();
        image.sprite = icon;
        
        var label = element.transform.Find("Label").GetComponent<Text>();
        GUIManager.Instance.ApplyTextStyle(label);
        label.text = shipment.m_shipmentID.ToString();

        string description = BuildDescriptionText(shipment);
        
        Transform selected = element.transform.Find("selected");
        
        var button = element.GetComponent<Button>();
        button.onClick.AddListener(() => UpdateDescription(description, tab));
        button.onClick.AddListener(() => SetSelectedShipment(shipment, selected.gameObject));
        
        Debug.Log($"PortUI.AddShipmentListElement: Adding shipment list element for shipment: {shipment.m_shipmentID}");
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

    public string BuildDescriptionText(Port port)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Destination:");
        sb.AppendLine(port.name);
        sb.AppendLine();

        sb.AppendLine("Travel Time:");
        sb.AppendLine("destinationPortETA");

        return sb.ToString();
    }
    
    public string BuildDescriptionText(Shipment shipment)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Destination:");
        sb.AppendLine(PortDB.Instance.GetPort(shipment.m_destinationPortID).name);
        sb.AppendLine();

        sb.AppendLine("Travel Time:");
        sb.AppendLine("destinationPortETA");

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
    
    public void Show(Port port, List<Port> ports, List<Shipment> shipments) 
    {
        Debug.Log($"$PortUI.Show: Show port {port.name} with {ports.Count} ports and {shipments.Count} shipments");
        this.currentPort = port;
        SetListElements(ports, shipments);
        
        this.gameObject.SetActive(true);
        
        GUIManager.BlockInput(true);
    }

    public void Hide()
    {
        foreach (Transform child in tabPortlistContainer)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in tabShipmentlistContainer)
        {
            Destroy(child.gameObject);
        }
        
        selectedPortListItem.Clear();
        selectedShipmentListItem.Clear();

        currentPort = null;
        selectedPort = null;
        selectedShipment = null;
        this.gameObject.SetActive(false);
        GUIManager.BlockInput(false);
    }

    public enum PortUITab
    {
        Ports,
        Shipments,
    }
    
    public class Requirement
    {
        public GameObject requirementParentObject;
        public Image requirementImage;
        public Text requirementText;
        public Text requirementValue;

        public Requirement(GameObject prefab)
        {
            requirementParentObject = prefab;
            if (prefab == null)
            {
                Debug.LogError("Requirement created with null prefab");
                return;
            }

            // No leading slashes in Transform.Find
            var iconT  = prefab.transform.Find("Icon");
            var labelT = prefab.transform.Find("Label");
            var valueT = prefab.transform.Find("Value");

            if (!iconT || !labelT || !valueT)
            {
                Debug.LogError("Requirement child missing: expected Icon/Label/Value under " + prefab.name);
                return;
            }

            requirementImage = iconT.GetComponent<Image>();
            requirementText  = labelT.GetComponent<Text>();
            requirementValue = valueT.GetComponent<Text>();

            if (!requirementImage || !requirementText || !requirementValue)
            {
                Debug.LogError("Requirement components missing on children of " + prefab.name);
            }
        }
    }
    
    private static readonly Dictionary<string, string> DisplayToShared = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Coins",       "$item_coins" },
        { "Wood",        "$item_wood" },
        { "Finewood",    "$item_finewood" },
        { "Iron",        "$item_iron" },
        { "Tar",         "$item_tar" },
        { "Blackmetal",  "$item_blackmetal" },
    };
    
    private static string ResolveSharedName(string name)
    {
        if (DisplayToShared.TryGetValue(name, out var shared)) return shared;
        
        return name;
    }
}