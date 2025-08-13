using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Extensions;
using Jotunn.GUI;
using Jotunn.Managers;
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
    public Button tabShipment;
    public GameObject shipmentsView;
    public RectTransform tabShipmentlistContainer;
    public Text tabShipmentdescription;
    public Button tabShipmentActionButton1;
    public Button tabShipmentActionButton2;

    // the port the player is currently at in the world
    public Port currentPort;
    
    // The port currently selected in the UI
    public Port selectedPort;
    
    public Shipment selectedShipment;
    
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
    
    public struct PortData
    {
        public Port port;
        public string PortName;
        public Vector3 Position;
        
        public PortData(string portName, Vector3 position, Port name)
        {
            PortName = portName;
            Position = position;
            port = name;
        }
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
        // 1) prevent re-entrancy / frame loops
        requirementsChanged = false;

        // 2) start from scratch each time
        requirements.Clear();
        
        foreach (Dropdown dropdown in tabPortDropsdowns)
        {
            Dictionary<string, int> requirementsDropdown = GetRequirements(dropdown.options[dropdown.value].text);
            foreach (string item in requirementsDropdown.Keys)
            {
                // in this method I need to somehow sum up the requirements from each dropdwon
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

    public void Awake()
    {
        instance = this;
        
        this.gameObject.AddComponent<DragWindowCntrl>();

        tabPortDropsdowns = new List<Dropdown>();
        
        portTitle = transform.Find("root/background/verticalLayout1/PortTitle")?.GetComponent<Text>();
        if (!portTitle) Debug.LogError("Null ref: portTitle path is incorrect");

        tabPort = transform.Find("root/background/verticalLayout1/TabPort")?.GetComponent<Button>();
        if (!tabPort) Debug.LogError("Null ref: tabPort path is incorrect");

        portsView = transform.Find("root/background/verticalLayout1/PortsView")?.gameObject;
        if (!portsView) Debug.LogError("Null ref: portsView path is incorrect");

        tabPortlistContainer = transform.Find("root/background/verticalLayout1/PortsView/listContainer")?.GetComponent<RectTransform>();
        if (!tabPortlistContainer) Debug.LogError("Null ref: tabPortlistContainer path is incorrect");

        tabPortdescription = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/description")?.GetComponent<Text>();
        if (!tabPortdescription) Debug.LogError("Null ref: tabPortdescription path is incorrect");

        tabPortActionButton = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/actionButton")?.GetComponent<Button>();
        if (!tabPortActionButton) Debug.LogError("Null ref: tabPortActionButton path is incorrect");
        
        var req1GO = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup1")?.gameObject;
        var req2GO = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup2")?.gameObject;
        var req3GO = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup3")?.gameObject;
        var req4GO = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup4")?.gameObject;
        var req5GO = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup5")?.gameObject;
        var req6GO = transform.Find("root/background/verticalLayout1/PortsView/descriptionContainer/requirements/requirementGroup6")?.gameObject;

        // Create Requirement objects (ctor wires up Image/Text/Value)
        tabPortRequirement1 = new Requirement(req1GO);
        tabPortRequirement2 = new Requirement(req2GO);
        tabPortRequirement3 = new Requirement(req3GO);
        tabPortRequirement4 = new Requirement(req4GO);
        tabPortRequirement5 = new Requirement(req5GO);
        tabPortRequirement6 = new Requirement(req6GO);

        // Store them
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

        // Optional: validate none are null
        for (int i = 0; i < tabPortRequirements.Count; i++)
        {
            if (tabPortRequirements[i] == null || tabPortRequirements[i].requirementParentObject == null)
                Debug.LogError($"Requirement {i+1} failed to initialize");
        }
        
        // --- Shipments tab ---
        tabShipment = transform.Find("root/background/verticalLayout1/TabShipments")?.GetComponent<Button>();
        if (!tabShipment) Debug.LogError("Null ref: tabShipment path is incorrect");

        shipmentsView = transform.Find("root/background/verticalLayout1/ShipmentsView")?.gameObject;
        if (!shipmentsView) Debug.LogError("Null ref: shipmentsView path is incorrect");

        tabShipmentlistContainer = transform.Find("root/background/verticalLayout1/ShipmentsView/listContainer")?.GetComponent<RectTransform>();
        if (!tabShipmentlistContainer) Debug.LogError("Null ref: tabShipmentlistContainer path is incorrect");

        tabShipmentdescription = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/description")?.GetComponent<Text>();
        if (!tabShipmentdescription) Debug.LogError("Null ref: tabShipmentdescription path is incorrect");

        tabShipmentActionButton1 = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/actionButton1")?.GetComponent<Button>();
        if (!tabShipmentActionButton1) Debug.LogError("Null ref: tabShipmentActionButton1 path is incorrect");
        
        tabShipmentActionButton2 = transform.Find("root/background/verticalLayout1/ShipmentsView/descriptionContainer/actionButton2")?.GetComponent<Button>();
        if (!tabShipmentActionButton2) Debug.LogError("Null ref: tabShipmentActionButton2 path is incorrect");
        
        // listeners
        if (tabPort) tabPort.onClick.AddListener(ShowPorts);
        if (tabShipment) tabShipment.onClick.AddListener(ShowShipment);
        if (tabPortActionButton) tabPortActionButton.onClick.AddListener(PurchaseShipment);
        
        if (tabShipmentActionButton1) tabShipmentActionButton1.onClick.AddListener(() => OpenShipment(selectedShipment));
        if (tabShipmentActionButton2) tabShipmentActionButton2.onClick.AddListener(() => SendShipment(selectedShipment));

        coinsSprite = GUIManager.Instance.GetSprite("coins");
        woodSprite = GUIManager.Instance.GetSprite("wood");
        fineWoodSprite = GUIManager.Instance.GetSprite("finewood");
        ironSprite = GUIManager.Instance.GetSprite("iron");
        blackMetalSprite = GUIManager.Instance.GetSprite("blackmetal");
        tarSprite = GUIManager.Instance.GetSprite("tar");
    }

    public void OpenShipment(Shipment shipment)
    {
        foreach (GameObject container in shipment.m_containers)
        {
            container.gameObject.transform.position = new Vector3(0, 0, 0);
        }
    }

    public void SendShipment(Shipment shipment)
    {
        
    }

    public void PurchaseShipment()
    {
        PortData port1 = new PortData(
            "Shipment A",
            new Vector3(100f, 0f, 250f),
            new Port()
        );
        
        if (!CheckPlayerInventory(Player.m_localPlayer, requirements, out var missing))
        {
            var msg = BuildMissingMessage(missing); // see helper below
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg);
            return;
        }
        
        ConsumePlayerInventory(Player.m_localPlayer, requirements);
        ShowShipment();
        List<GameObject> conatiners = CreateContainers();
        Shipment shipment = new Shipment(currentPort, selectedPort, 1, conatiners);
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

    /// <summary>
    /// Check if the player's inventory satisfies all requirements.
    /// </summary>
    /// <param name="player">Player to check.</param>
    /// <param name="requirements">Requirement amounts by display or shared name.</param>
    /// <param name="missing">Outputs only items still needed (name -> remaining needed).</param>
    /// <returns>true if everything is present, false otherwise.</returns>
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

            // Count all stacks regardless of world level / quality
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
                prefab = PrefabManager.Instance.GetPrefab("piece_chest_wood");
                break;

            case "Finewood Chest":
                prefab = PrefabManager.Instance.GetPrefab("piece_chest");
                break;
            
            case "Blackmetal Chest":
                prefab = PrefabManager.Instance.GetPrefab("piece_chest_blackmetal");
                break;

            default:
                prefab = null;
                Debug.LogWarning("Unknown container selected: " + selected);
                break;
        }
        
        return prefab;
    }

    public List<GameObject> CreateContainers()
    {
        Vector3 spawnPosition = new Vector3(0f, 0f, 0f);
        
        List<GameObject> containers = new List<GameObject>();

        foreach (Dropdown dropdown in tabPortDropsdowns)
        {
            spawnPosition = Player.m_localPlayer.transform.position;
            spawnPosition.x += 2f;
            spawnPosition.y += 2f;
            
            GameObject prefab = GetChestType(dropdown);

            if (prefab != null)
            {
                GameObject chest = Object.Instantiate(prefab, spawnPosition, Player.m_localPlayer.transform.rotation);
                containers.Add(chest);
            }
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

    public void SetupListElements()
    {
        tabPortActionButton.transform.Find("Label").GetComponent<Text>().text = "Purchase Shipment";
        tabShipmentActionButton1.transform.Find("Label").GetComponent<Text>().text = "Open Shipment";
        tabShipmentActionButton2.transform.Find("Label").GetComponent<Text>().text = "Send Shipment";


        BuildDropdowns();
        
        GUIManager.Instance.ApplyButtonStyle(tabPortActionButton);
        GUIManager.Instance.ApplyButtonStyle(tabShipmentActionButton1);
        GUIManager.Instance.ApplyButtonStyle(tabShipmentActionButton2);

        
        GUIManager.Instance.ApplyTextStyle(portTitle, 36);
        
        foreach (Text text in new List<Text> {tabPortdescription, tabShipmentdescription })
        {
            GUIManager.Instance.ApplyTextStyle(text, 18);
        }
        
        foreach (Text text in new List<Text> {tabPort.transform.Find("Label").GetComponent<Text>(), tabShipment.transform.Find("Label").GetComponent<Text>() })
        {
            GUIManager.Instance.ApplyTextStyle(text,18);
        }

        Debug.Log("testing a thing");
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
        
        PortData port1 = new PortData(
            "Seaside Dock",
            new Vector3(100f, 0f, 250f),
            new Port()
        );
        
        PortData port2 = new PortData(
            "Dangerous Cove",
            new Vector3(100f, 0f, 250f),
            new Port()
        );
        
        PortData port3 = new PortData(
            "Halbur Harbor",
            new Vector3(100f, 0f, 250f),
            new Port()
        );

        AddPortListElement(tabPortlistContainer, GUIManager.Instance.GetSprite("longship"), port1.PortName, port1, PortUITab.Ports);
        AddPortListElement(tabPortlistContainer, GUIManager.Instance.GetSprite("longship"), port2.PortName, port2, PortUITab.Ports);
        AddPortListElement(tabPortlistContainer, GUIManager.Instance.GetSprite("longship"), port3.PortName, port3, PortUITab.Ports);
    }

    public void AddPortListElement(RectTransform parent, Sprite icon, string portName, PortData port, PortUITab tab)
    {
        var element = Object.Instantiate(PortPrefabs.portUIListItem, parent);
        
        var image = element.transform.Find("Icon").GetComponent<Image>();
        image.sprite = icon;
        
        var label = element.transform.Find("Label").GetComponent<Text>();
        GUIManager.Instance.ApplyTextStyle(label);
        label.text = portName;

        string description = BuildDescriptionText(port);
        
        var button = element.GetComponent<Button>();
        button.onClick.AddListener(() => UpdateDescription(description, tab));
        button.onClick.AddListener(() => selectedPort = port.port);
    }
    
    public void AddShipmentListElement(RectTransform parent, Sprite icon, Shipment shipment, PortUITab tab)
    {
        var element = Object.Instantiate(PortPrefabs.portUIListItem, parent);
        
        var image = element.transform.Find("Icon").GetComponent<Image>();
        image.sprite = icon;
        
        var label = element.transform.Find("Label").GetComponent<Text>();
        GUIManager.Instance.ApplyTextStyle(label);
        label.text = shipment.m_destinationPort.name;

        string description = BuildDescriptionText(shipment);
        
        var button = element.GetComponent<Button>();
        button.onClick.AddListener(() => UpdateDescription(description, tab));
        button.onClick.AddListener(() => selectedShipment = shipment);
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
    
    public string BuildDescriptionText(Shipment shipment)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Destination:");
        sb.AppendLine(shipment.m_destinationPort.name);
        sb.AppendLine();

        sb.AppendLine("Distance:");
        sb.AppendLine(shipment.m_destinationPort.name); // change to the correct field
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
        GUIManager.BlockInput(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        GUIManager.BlockInput(false);
    }

    public enum PortUITab
    {
        Ports,
        Shipments,
    }
}