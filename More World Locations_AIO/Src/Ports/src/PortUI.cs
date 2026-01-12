using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using More_World_Locations_AIO.Managers;
using More_World_Locations_AIO.tutorials;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO;

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
public static class LoadPortUI
{
    [UsedImplicitly]
    private static void Postfix(InventoryGui __instance)
    {
        GameObject? panel = AssetBundleManager.LoadAsset<GameObject>("portbundle", "PortUI");
        if (panel == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("PortUI is null");
            return;
        }
        
        GameObject craftingPanel = __instance.m_crafting.gameObject;
        if (craftingPanel == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("CraftingPanel is null");
            return;
        }
        PortUI._tooltipPrefab = craftingPanel.GetComponentInChildren<UITooltip>().m_tooltipPrefab;
        GameObject? go = Object.Instantiate(panel, __instance.transform.parent.Find("HUD"));
        go.name = "PortUI";
        go.AddComponent<PortUI>();
        Text[]? panelTexts = go.GetComponentsInChildren<Text>(true);
        Text[]? listItemTexts = PortUI.ListItem.GetComponentsInChildren<Text>(true);
        
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Selected", "selected_frame/selected (1)");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/bkg", "Bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/TitlePanel/BraidLineHorisontalMedium (1)", "TitlePanel/BraidLineHorisontalMedium (1)");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/TitlePanel/BraidLineHorisontalMedium (2)", "TitlePanel/BraidLineHorisontalMedium (2)");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Port", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Port/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Panel/Tabs/Port", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Shipment", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Shipment/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Panel/Tabs/Shipment", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Delivery", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Delivery/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Panel/Tabs/Delivery", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Manifests", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Manifests/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Panel/Tabs/Manifests", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Teleport", "TabsButtons/Craft");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Tabs/Teleport/Selected", "TabsButtons/Craft/Selected");
        go.CopyButtonState(craftingPanel, "Panel/Tabs/Teleport", "TabsButtons/Craft");
        
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Separator", "TabsButtons/TabBorder");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/LeftPanel/Viewport", "RecipeList/Recipes");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description", "Decription");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Icon", "Decription/Icon");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/SendButton", "Decription/craft_button_panel/CraftButton");
        go.CopyButtonState(craftingPanel, "Panel/Description/SendButton", "Decription/craft_button_panel/CraftButton");
        
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/1", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/2", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/3", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/4", "Decription/requirements/res_bkg");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/1/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/2/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/3/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/4/Icon", "Decription/requirements/res_bkg/res_icon");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/level", "Decription/requirements/level");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/Description/Requirements/level/MinLevel", "Decription/requirements/level/MinLevel");
        
        go.CopySpriteAndMaterial(craftingPanel, "Panel/HowToButton", "RepairButton");
        go.CopyButtonState(craftingPanel, "Panel/HowToButton", "RepairButton");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/HowToButton/Glow", "RepairButton/Glow");
        go.CopySpriteAndMaterial(craftingPanel, "Panel/RepairSimple", "RepairSimple");
        go.transform.position = new Vector3(1760f, 850f, 0f);
        PortUI.ListItem.CopySpriteAndMaterial(craftingPanel, "Icon", "RecipeList/Recipes/RecipeElement/icon");
        
        GameObject? sfx = craftingPanel.GetComponentInChildren<ButtonSfx>().m_sfxPrefab;
        foreach (var component in go.GetComponentsInChildren<ButtonSfx>(true)) component.m_sfxPrefab = sfx;
        FontManager.SetFont(panelTexts);
        FontManager.SetFont(listItemTexts);
    }
}


[HarmonyPatch(typeof(Player), nameof(Player.TakeInput))]
public static class PlayerTakeInput_Patch
{
    [UsedImplicitly]
    private static void Postfix(ref bool __result)
    {
        __result &= !PortUI.IsVisible();
    } 
}

[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.InInventoryEtc))]
public static class PlayerController_InInventoryEtc_Patch
{
    [UsedImplicitly]
    private static void Postfix(ref bool __result)
    {
        __result |= PortUI.IsVisible();
    }
}

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.IsVisible))]
public static class InventoryGui_IsVisible_Patch
{
    [UsedImplicitly]
    private static void Postfix(ref bool __result)
    {
        __result |= PortUI.IsVisible();
    }
}

public class PortUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public enum BackgroundOption { Opaque, Transparent }

    internal static readonly GameObject ListItem = AssetBundleManager.LoadAsset<GameObject>("portbundle", "ListItem")!;
    internal static GameObject? _tooltipPrefab;
    internal static ConfigEntry<BackgroundOption>? BkgOption;
    public static ConfigEntry<Vector3>? PanelPositionConfig;
    public static ConfigEntry<PortInit.Toggle>? UseTeleportTab;
    public static ConfigEntry<float>? TeleportCostPerMeter;
    
    public static PortUI? instance;
    private static Minimap.PinData? m_tempPin; // let's keep this static so we only ever have one port pin on the map
    
    // fields set on awake, shouldn't change after
    private float m_leftListMinHeight;
    private static Sprite? m_defaultIcon;
    private float m_listItemHeight;
    
    private RectTransform m_rect = null!;
    private Image Selected = null!;
    private Image Background = null!;
    private Image HelpBackground = null!;
    private GameObject Darken = null!;
    private Text Topic = null!;
    private Tab PortTab = null!;
    private Tab ShipmentTab = null!;
    private Tab DeliveryTab = null!;
    private Tab ManifestTab = null!;
    private Tab TeleportTab = null!;
    private VerticalLayoutGroup LeftPanelLayout = null!;
    private RectTransform LeftPanelRoot = null!;
    private Image Icon = null!;
    private Button MainButton = null!;
    private HowTo Help = null!;
    private Text MainButtonText = null!;
    private RightPanel Description = null!;
    private Requirement Requirements = null!;
    private readonly List<TempListItem> m_tempListItems = new();
    private readonly List<Tab> Tabs = new();

    public Port? m_currentPort;
    private Port.PortInfo? m_selectedDestination;
    public Shipment? m_selectedDelivery;
    private Manifest? m_selectedManifest;
    
    private TabOption m_currentTab = TabOption.Ports;
    private float m_portPinTimer;

    private Action<float>? OnUpdate;
    private Action? OnSentShipment;
    private enum TabOption
    {
        Ports, Shipments, Delivery, Manifest, Teleport
    }
    
    public void Awake()
    {
        instance = this;
        m_listItemHeight = ListItem.GetComponent<RectTransform>().sizeDelta.y;
        m_rect = GetComponent<RectTransform>();
        Selected = transform.Find("Panel/Selected").GetComponent<Image>();
        Background = transform.Find("Panel/bkg").GetComponent<Image>();
        HelpBackground = transform.Find("Panel/RepairSimple").GetComponent<Image>();
        Darken = transform.Find("Panel/darken").gameObject;
        Topic = transform.Find("Panel/TitlePanel/topic").GetComponent<Text>();
        Icon  = transform.Find("Panel/Description/Icon").GetComponent<Image>();
        MainButton = transform.Find("Panel/Description/SendButton").GetComponent<Button>();
        MainButtonText =  transform.Find("Panel/Description/SendButton/Text").GetComponent<Text>();
        Help = new HowTo(transform.Find("Panel/HowToButton"));
        PortTab = new Tab(transform.Find("Panel/Tabs/Port"));
        ShipmentTab = new Tab(transform.Find("Panel/Tabs/Shipment"));
        DeliveryTab = new Tab(transform.Find("Panel/Tabs/Delivery"));
        ManifestTab = new Tab(transform.Find("Panel/Tabs/Manifests"));
        TeleportTab = new Tab(transform.Find("Panel/Tabs/Teleport"));
        LeftPanelRoot =  transform.Find("Panel/LeftPanel/Viewport/ListRoot").GetComponent<RectTransform>();
        m_leftListMinHeight = LeftPanelRoot.sizeDelta.y;
        LeftPanelLayout = LeftPanelRoot.GetComponent<VerticalLayoutGroup>();
        Description = new RightPanel(
            transform.Find("Panel/Description/Name").GetComponent<Text>(), 
            transform.Find("Panel/Description/Body/Viewport/Text").GetComponent<Text>(), 
            transform.Find("Panel/Description/MapButton").GetComponent<Button>());
        Requirements = new Requirement(transform.Find("Panel/Description/Requirements").gameObject);
        Requirements.Add(transform.Find("Panel/Description/Requirements/1"));
        Requirements.Add(transform.Find("Panel/Description/Requirements/2"));
        Requirements.Add(transform.Find("Panel/Description/Requirements/3"));
        Requirements.Add(transform.Find("Panel/Description/Requirements/4"));
        Requirements.level.Icon = transform.Find("Panel/Description/Requirements/level/MinLevel").GetComponent<Image>();
        Requirements.level.Label = transform.Find("Panel/Description/Requirements/level/MinLevel/Text").GetComponent<Text>();

        m_defaultIcon = Icon.sprite;
        PortTab.SetButton(OnPortTab);
        ShipmentTab.SetButton(OnShipmentTab);
        DeliveryTab.SetButton(OnDeliveryTab);
        ManifestTab.SetButton(OnManifestTab);
        TeleportTab.SetButton(OnTeleportTab);
        MainButton.onClick.AddListener(OnMainButton);
        Description.SetMapButton(OnMapButton);
        
        PortTab.SetLabel(LocalKeys.Port);
        PortTab.SetTooltip(LocalKeys.PortTooltip);
        ShipmentTab.SetLabel(LocalKeys.Shipments);
        ShipmentTab.SetTooltip(LocalKeys.ShipmentTooltip);
        DeliveryTab.SetLabel(LocalKeys.Deliveries);
        DeliveryTab.SetTooltip(LocalKeys.DeliveryTooltip);
        ManifestTab.SetLabel(LocalKeys.Manifest);
        ManifestTab.SetTooltip(LocalKeys.ManifestTooltip);
        TeleportTab.SetLabel(LocalKeys.Teleport);
        TeleportTab.SetTooltip(LocalKeys.TeleportTooltip);
        transform.Find("Panel/Description/MapButton/Text").GetComponent<Text>().text = Localization.instance.Localize(LocalKeys.OpenMap);
        Help.SetButton(OnHelp);
        Help.SetGlow(false);
        Requirements.SetActive(false);
        
        SetBackground(BkgOption?.Value ?? BackgroundOption.Opaque);
        SetPanelPosition(PanelPositionConfig?.Value ?? new Vector3(1760f, 850f, 0f));
        TeleportTab.Enable(UseTeleportTab?.Value is PortInit.Toggle.On);
        Hide();
    }

    public void Update()
    {
        float dt = Time.deltaTime;
        OnUpdate?.Invoke(dt);
        m_portPinTimer -= dt;
        if (m_portPinTimer <= 0.0f && m_tempPin != null)
        {
            Minimap.instance.RemovePin(m_tempPin);
        }
        if (!ZInput.GetKeyDown(KeyCode.Escape) && !ZInput.GetKeyDown(KeyCode.Tab)) return;
        Hide();
    }

    public void OnDestroy()
    {
        instance = null;
    }

    public static bool IsVisible() => instance != null && instance.gameObject.activeInHierarchy;

    public void Hide()
    {
        gameObject.SetActive(false);
        OnUpdate = null;
        OnSentShipment = null;
    }

    public static void OnBackgroundOptionChange(object sender, EventArgs args)
    {
        if (instance == null) return;
        if (sender is not ConfigEntry<BackgroundOption> option) return;
        instance.SetBackground(option.Value);
    }

    public void SetBackground(BackgroundOption option)
    {
        switch (option)
        {
            case BackgroundOption.Opaque:
                Background.gameObject.SetActive(true);
                HelpBackground.gameObject.SetActive(true);
                Darken.SetActive(false);
                break;
            case BackgroundOption.Transparent:
                Background.gameObject.SetActive(false);
                HelpBackground.gameObject.SetActive(false);
                Darken.SetActive(true);
                break;
        }
    }

    public static void OnPanelPositionConfigChange(object sender, EventArgs args)
    {
        if (PortUI.instance == null) return;
        if (sender is not ConfigEntry<Vector3> config) return;
        instance.SetPanelPosition(config.Value);
    }

    public void SetPanelPosition(Vector3 pos) => transform.position = pos;

    public static void OnUseTeleportTabChange(object sender, EventArgs args)
    {
        if (instance == null) return;
        if (sender is not ConfigEntry<PortInit.Toggle> config) return;
        instance.TeleportTab.Enable(config.Value is PortInit.Toggle.On);
    }

    public void OnMapButton()
    {
        if (Description.MapInfo == null || !Minimap.instance) return;
        Vector3 pos = Description.MapInfo.position;
        Hide();
        if (m_tempPin != null)
        {
            Minimap.instance.RemovePin(m_tempPin);
        }
        Minimap.PinData? pin = Minimap.instance.AddPin(pos, Minimap.PinType.Icon2, Description.MapInfo.portID.Name, false, false);
        Minimap.instance.ShowPointOnMap(pos);
        m_tempPin = pin;
        m_portPinTimer = 300f;
    }
    public void SetTopic(string topic) => Topic.text = Localization.instance.Localize(topic);
    
    public void SetMainButtonText(string text) => MainButtonText.text = Localization.instance.Localize(text);
    
    public void Show(Port port)
    {
        if (ShipmentManager.instance == null) return;
        m_currentPort = port;
        gameObject.SetActive(true);
        PortTab.SetSelected(true);
        SetTopic(m_currentPort.m_name);
        Description.ResetDescription();
        m_currentTab = TabOption.Ports;
        PortTab.SetSelected(true);
        LoadPorts();
        ResizeLeftList();
        SetMainButtonText(LocalKeys.Exit);
        Requirements.SetActive(false);
        m_selectedDestination = null;
        Description.ResetDescription();
        Icon.sprite = m_defaultIcon;
        MainButton.interactable = true;
        OnUpdate = null;
        Help.SetGlow(false);
        Help.SetIcon(Minimap.instance.GetSprite(Minimap.PinType.Hildir1));
    }

    public void OnHelp()
    {
        Tab.SetAllSelected(false);
        Help.SetGlow(true);
        LoadTutorials();
        SetMainButtonText(LocalKeys.Exit);
        m_selectedDestination = null;
        m_selectedManifest = null;
        Description.ResetDescription();
        MainButton.interactable = true;
        Requirements.SetActive(false);
        Icon.sprite = m_defaultIcon;
        OnUpdate = null;
    }
    public void OnManifestTab()
    {
        m_currentTab = TabOption.Manifest;
        if (ManifestTab.IsSelected) return;
        ManifestTab.SetSelected(true);
        LoadManifests();
        SetMainButtonText(LocalKeys.Purchase);
        m_selectedDestination = null;
        Description.ResetDescription();
        MainButton.interactable = false;
        Requirements.SetActive(false);
        Icon.sprite = m_defaultIcon;
        OnUpdate = null;
        Help.SetGlow(false);
    }

    public void OnPortTab()
    {
        m_currentTab = TabOption.Ports;
        if (PortTab.IsSelected) return;
        PortTab.SetSelected(true);
        LoadPorts();
        SetMainButtonText(LocalKeys.Exit);
        m_selectedDestination = null;
        m_selectedManifest = null;
        Description.ResetDescription();
        MainButton.interactable = true;
        Requirements.SetActive(false);
        Icon.sprite = m_defaultIcon;
        OnUpdate = null;
        Help.SetGlow(false);
    }

    public void OnShipmentTab()
    {
        m_currentTab = TabOption.Shipments;
        if (ShipmentTab.IsSelected || m_currentPort == null) return;
        ShipmentTab.SetSelected(true);
        LoadShipments();
        SetMainButtonText(LocalKeys.Exit);
        Description.ResetDescription();
        m_selectedDestination = null;
        m_selectedManifest = null;
        MainButton.interactable = true;
        Requirements.SetActive(false);
        Icon.sprite = m_defaultIcon;
        OnUpdate = null;
        Help.SetGlow(false);
    }

    public void OnDeliveryTab()
    {
        m_currentTab = TabOption.Delivery;
        if (DeliveryTab.IsSelected) return;
        DeliveryTab.SetSelected(true);
        LoadDeliveries();
        SetMainButtonText(LocalKeys.OpenDelivery);
        MainButton.interactable = false;
        Description.ResetDescription();
        m_selectedDestination = null;
        m_selectedManifest = null;
        Requirements.SetActive(false);
        Icon.sprite = m_defaultIcon;
        OnUpdate = null;
        Help.SetGlow(false);
    }

    public void OnTeleportTab()
    {
        m_currentTab = TabOption.Teleport;
        if (TeleportTab.IsSelected) return;
        TeleportTab.SetSelected(true);
        LoadPortals();
        SetMainButtonText(LocalKeys.Teleport);
        m_selectedDestination = null;
        m_selectedManifest = null;
        Description.ResetDescription();
        MainButton.interactable = false;
        Requirements.SetActive(false);
        Icon.sprite = m_defaultIcon;
        OnUpdate = null;
        Help.SetGlow(false);
    }

    private bool CanShip()
    {
        if (m_currentPort == null || !Player.m_localPlayer) return false;
        if (Player.m_localPlayer.NoCostCheat()) return true;
        int cost = m_currentPort.m_containers.GetCost();
        string costItem = ShipmentManager.CurrencyItem?.m_shared.m_name ?? "$item_coins";
        int count = Player.m_localPlayer.GetInventory().CountItems(costItem);
        return count >= cost;
    }

    public void OnMainButton()
    {
        if (m_currentPort == null) return;
        switch (m_currentTab)
        {
            case TabOption.Ports:
                // port tab has 2 options, to exit UI, or send shipment
                // depending on state of selected destination
                if (m_selectedDestination == null) Hide();
                else
                {
                    if (m_currentPort.SendShipment(m_selectedDestination))
                    {
                        if (ZNet.instance && ZNet.instance.IsServer())
                        {
                            m_selectedDestination.Reload();
                        }
                        else ShipmentManager.OnShipmentsUpdated += m_selectedDestination.Reload;
                        Player.m_localPlayer.GetInventory().RemoveItem(
                            ShipmentManager.CurrencyItem?.m_shared.m_name ?? "$item_coins",
                            m_currentPort.m_containers.GetCost());
                        m_selectedDestination = null;
                        OnSentShipment?.Invoke();
                    }
                }
                break;
            case TabOption.Shipments:
                Hide();
                break;
            case TabOption.Delivery:
                if (m_selectedDelivery == null) return;
                if (m_currentPort.LoadDelivery(m_selectedDelivery))
                {
                    MainButton.interactable = false;
                    Description.ResetDescription();
                    m_selectedDelivery = null;
                    OnUpdate = null;
                    // should we Hide panel when delivery loaded ??
                }
                break;
            case TabOption.Manifest:
                if (m_selectedManifest == null || !Player.m_localPlayer.HasRequirements(m_selectedManifest)) return;
                if (m_currentPort.SpawnContainer(m_selectedManifest) is {} container)
                {
                    Player.m_localPlayer.Purchase(m_selectedManifest);
                    // set manifest to purchased
                    m_selectedManifest.IsPurchased = true;
                    // reload manifests to visually update list of manifests
                    // with all manifests that are NOT purchased
                    // we do this so there are no duplicate manifests being sent in a single shipment
                    // to not overload delivery
                    Description.ResetDescription();
                    MainButton.interactable = false;
                    Requirements.SetActive(false);
                    Icon.sprite = m_defaultIcon;
                    OnUpdate = null;
                    
                    Player.m_localPlayer.SetLookDir(container.transform.position - Player.m_localPlayer.transform.position, 3.5f);
                    Hide();
                }
                else
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, LocalKeys.MaximumShipments);
                }
                m_selectedManifest = null;
                break;
            case TabOption.Teleport:
                if (m_selectedDestination == null || !Player.m_localPlayer) return;
                if (!Player.m_localPlayer.IsTeleportable())
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noteleport");
                    return;
                }
                // Check and consume teleport requirements before teleporting.
                if (!Player.m_localPlayer.NoCostCheat())
                {
                    var teleportCost = GetTeleportRequirements(m_selectedDestination);
                    if (!HasTeleportRequirements(Player.m_localPlayer, teleportCost))
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_missingrequirement");
                        return;
                    }
                    ConsumeTeleportRequirements(Player.m_localPlayer, teleportCost);
                }
                Player.m_localPlayer.TeleportTo(m_selectedDestination.position, Quaternion.identity, true);
                Hide();
                break;
        }
    }

    public void ResizeLeftList()
    {
        int count = m_tempListItems.Count;
        float padding = LeftPanelLayout.spacing;
        float totalItemHeight = count * m_listItemHeight;
        float totalSpacingHeight = Mathf.Max(0, count - 1) * padding;
        float newHeight = totalItemHeight + totalSpacingHeight;
        LeftPanelRoot.sizeDelta = new Vector2(LeftPanelRoot.sizeDelta.x, Mathf.Max(newHeight, m_leftListMinHeight));
    }

    public void LoadTutorials()
    {
        ClearLeftPanel();
        foreach (PortTutorial? tutorial in PortTutorial.tutorials) AddTutorial(tutorial);
        ResizeLeftList();
    }

    public void LoadPorts()
    {
        ClearLeftPanel();
        foreach (ZDO port in ShipmentManager.GetPorts()) AddPort(port);
        ResizeLeftList();
    }

    public void LoadPortals()
    {
        ClearLeftPanel();
        foreach (ZDO port in ShipmentManager.GetPorts()) AddPortal(port);
        ResizeLeftList();
    }

    public void LoadManifests()
    {
        ClearLeftPanel();
        foreach (Manifest manifest in Manifest.Manifests.Values) AddManifest(manifest);
        ResizeLeftList();
    }

    public void LoadShipments()
    {
        ClearLeftPanel();
        if (m_currentPort == null) return;
        List<Shipment> shipments = ShipmentManager.GetShipments(m_currentPort.m_portID.GUID)
            .OrderBy(shipment => shipment.ArrivalTime)
            .ToList();
        foreach (Shipment shipment in shipments) AddShipment(shipment);
        ResizeLeftList();
    }

    public void LoadDeliveries()
    {
        ClearLeftPanel();
        if (m_currentPort == null) return;
        List<Shipment> deliveries = ShipmentManager.GetDeliveries(m_currentPort.m_portID.GUID);
        foreach(Shipment? delivery in deliveries) AddDelivery(delivery);
        ResizeLeftList();
    }

    public void ClearLeftPanel()
    {
        foreach(TempListItem? item in m_tempListItems) item.Destroy();
        m_tempListItems.Clear();
    }

    public void AddManifest(Manifest manifest)
    {
        if (!Player.m_localPlayer.IsKnownManifest(manifest)) return;
        if (manifest.IsPurchased) return;
        TempListItem item = new TempListItem(Instantiate(ListItem, LeftPanelRoot));
        item.SetIcon(manifest.Icon);
        item.SetLabel(manifest.Name);
        item.SetButton(() =>
        {
            m_selectedManifest = manifest;
            Description.SetName(manifest.Name);
            Description.SetBodyText(manifest.GetTooltip());
            MainButton.interactable = Player.m_localPlayer.HasRequirements(manifest);
            Requirements.SetActive(true);
            Requirements.LoadRequirements(manifest);
            Requirements.SetLevel(manifest.CostToShip.ToString());
            Icon.sprite = manifest.Icon;
            OnUpdate = dt => Requirements.Update(dt, Player.m_localPlayer);
        });
        m_tempListItems.Add(item);
    }

    public void AddDelivery(Shipment shipment)
    {
        TempListItem item = new TempListItem(Instantiate(ListItem, LeftPanelRoot));
        item.SetIcon("Cart");
        item.SetLabel(shipment.OriginPortName);
        item.SetButton(() =>
        {
            item.SetSelected(true);
            m_selectedDelivery = shipment;
            Description.SetName(shipment.OriginPortName);
            Description.SetBodyText(shipment.GetTooltip());
            MainButton.interactable = shipment.State == ShipmentState.Delivered;
            float timer = 0f;
            OnUpdate = dt =>
            {
                bool shouldUpdate = shipment.State is ShipmentState.InTransit or ShipmentState.Delivered;
                if (!shouldUpdate) return;
                timer += dt;
                if (timer <= 1f) return; // update every 1 second
                timer = 0.0f;
                Description.SetBodyText(shipment.GetTooltip());
            };
        });
        m_tempListItems.Add(item);
    }

    public void AddTutorial(PortTutorial tutorial)
    {
        TempListItem item = new TempListItem(Instantiate(ListItem, LeftPanelRoot));
        item.SetIcon(Minimap.PinType.Hildir1);
        item.SetLabel(tutorial.label);
        item.SetButton(() =>
        {
            item.SetSelected(true);
            Description.SetName(tutorial.label);
            Description.SetBodyText(tutorial.text);
            MainButton.interactable = false;
            OnUpdate = null;
        });
        m_tempListItems.Add(item);
    }

    public void AddShipment(Shipment shipment)
    {
        TempListItem item = new TempListItem(Instantiate(ListItem, LeftPanelRoot));
        item.SetIcon("VikingShip");
        item.SetLabel(shipment.DestinationPortName);
        item.SetButton(() =>
        {
            item.SetSelected(true);
            Description.SetName(shipment.DestinationPortName);
            Description.SetBodyText(shipment.GetTooltip());
            float timer = 0f;
            OnUpdate = dt =>
            {
                bool shouldUpdate = shipment.State is ShipmentState.InTransit or ShipmentState.Delivered;
                if (!shouldUpdate) return;
                timer += dt;
                if (timer <= 1f) return; // update every 1 second
                timer = 0.0f;
                Description.SetBodyText(shipment.GetTooltip());
            };
        });
        m_tempListItems.Add(item);
    }

    public void AddPort(ZDO port)
    {
        if (m_currentPort == null || !Player.m_localPlayer) return;
        if (port == m_currentPort.m_view.GetZDO()) return;
        Port.PortInfo info = new Port.PortInfo(port);
        if (!Player.m_localPlayer.IsKnownPort(info.portID)) return;
        if (string.IsNullOrEmpty(info.portID.Name)) return;
        TempListItem item = new TempListItem(Instantiate(ListItem, LeftPanelRoot));
        item.SetIcon(Minimap.instance.GetSprite(Minimap.PinType.Icon2));
        item.SetLabel($"{info.portID.Name}");
        item.SetButton(() =>
        {
            item.SetSelected(true);
            m_selectedDestination = info;
            Description.SetName($"<color=orange>{info.portID.Name}</color> ({(int)info.GetDistance(Player.m_localPlayer)}m)");
            string containerTooltip = m_currentPort.GetTooltip(); // store it here, to keep this part static
            OnSentShipment = () =>
            {
                // for visual feedback that current shipment is sent
                // thus should not be displayed as the active shipment
                // being worked on
                containerTooltip = string.Empty;
                OnSentShipment = null;
            };
            Description.SetBodyText(info.GetTooltip() + "\n\n" + containerTooltip);
            Description.ShowMapButton(info);
            SetMainButtonText(LocalKeys.SendShipment);
            bool hasItems = m_currentPort.m_containers.HasItems();
            // cache hasItems since we do not need to re-calculate the amount of items
            if (hasItems)
            {
                Requirements.SetActive(true);
                Requirements.LoadCost(m_currentPort.m_containers);
                Requirements.SetLevel(1.ToString());
            }
            
            MainButton.interactable = CanShip();
            float timer = 0f;
            OnUpdate = dt =>
            {
                if (hasItems) Requirements.Update(dt, Player.m_localPlayer);
                bool shouldUpdate =
                    info.shipments.Any(s => s.State is ShipmentState.InTransit or ShipmentState.Delivered) ||
                    info.deliveries.Any(d => d.State is ShipmentState.InTransit or ShipmentState.Delivered);
                if (!shouldUpdate) return;
                timer += dt;
                if (timer <= 1f) return; // update every 1 second
                timer = 0.0f;
                Description.SetBodyText(info.GetTooltip() + "\n\n" + containerTooltip);
            };
        });
        m_tempListItems.Add(item);
    }
    
    public void AddPortal(ZDO port)
    {
        if (m_currentPort == null || !Player.m_localPlayer) return;
        if (port == m_currentPort.m_view.GetZDO()) return; // do not show current port
        Port.PortInfo info = new Port.PortInfo(port);
        if (!Player.m_localPlayer.IsKnownPort(info.portID)) return;
        if (string.IsNullOrEmpty(info.portID.Name)) return; // make sure zdo has a port name
        TempListItem item = new TempListItem(Instantiate(ListItem, LeftPanelRoot));
        item.SetIcon(Minimap.instance.GetSprite(Minimap.PinType.Icon4));
        item.SetLabel($"{info.portID.Name}");
        item.SetButton(() =>
        {
            item.SetSelected(true);
            m_selectedDestination = info;
            Description.SetName($"<color=orange>{info.portID.Name}</color> ({(int)info.GetDistance(Player.m_localPlayer)}m)");
            Description.SetBodyText(info.GetTooltip());
            Description.ShowMapButton(info);
            SetMainButtonText(LocalKeys.Teleport);
            
            Requirements.SetActive(true);
            Requirements.LoadTeleportCost(GetTeleportRequirements(info));
            Requirements.SetLevel(1.ToString());
            MainButton.interactable = true;
            float timer = 0f;
            OnUpdate = dt =>
            {
                Requirements.Update(dt, Player.m_localPlayer);
                bool shouldUpdate =
                    info.shipments.Any(s => s.State == ShipmentState.InTransit) ||
                    info.deliveries.Any(d => d.State == ShipmentState.InTransit);
                if (!shouldUpdate) return;
                timer += dt;
                if (timer <= 1f) return;
                timer = 0.0f;
                Description.SetBodyText(info.GetTooltip());
            };
        });
        m_tempListItems.Add(item);
    }
    private class TempListItem
    {
        private readonly GameObject? Prefab;
        private readonly Button? Button;
        private readonly Image? Icon;
        private readonly Text? Label;
        private readonly GameObject? Selected;
        private bool IsSelected => Selected != null && Selected.activeInHierarchy;

        public TempListItem(GameObject prefab)
        {
            Prefab = prefab;
            Button = Prefab.GetComponent<Button>();
            Icon = Prefab.transform.Find("Icon").GetComponent<Image>();
            Label = Prefab.transform.Find("Label").GetComponent<Text>();
            Selected = Prefab.transform.Find("selected").gameObject;
        }

        public void SetLabel(string label)
        {
            if (Label == null) return;
            Label.text = Localization.instance.Localize(label);
        }

        public void SetIcon(Sprite? sprite)
        {
            if (Icon == null) return;
            Icon.sprite = sprite;
        }
        
        public void SetIcon(Minimap.PinType pinType) => SetIcon(Minimap.instance.GetSprite(pinType));

        public void SetIcon(string prefabName)
        {
            if (ZNetScene.instance.GetPrefab(prefabName) is not {} prefab) return;
            if (prefab.TryGetComponent(out ItemDrop itemDrop))
            {
                SetIcon(itemDrop.m_itemData);
            }
            else if (prefab.TryGetComponent(out Piece piece))
            {
                SetIcon(piece.m_icon);
            }
        }

        private void SetIcon(ItemDrop.ItemData item)
        {
            if (!item.IsValid()) return; // make sure item has an icon
            SetIcon(item.GetIcon());
        }

        public void ShowIcon(bool enable)
        {
            if (Icon == null) return;
            Icon.gameObject.SetActive(enable);
        }
        public void SetSelected(bool selected)
        {
            if (IsSelected) return;
            if (Selected == null || instance == null) return;
            foreach (TempListItem? temp in instance.m_tempListItems)
            {
                if (temp.Selected == null) continue;
                temp.Selected.SetActive(false);
            }
            Selected.SetActive(selected);
        }

        public void SetButton(UnityAction action)
        {
            if (Button == null) return;
            Button.onClick.AddListener(action);
        }

        public void Destroy()
        {
            Object.Destroy(Prefab);
        }
    }
    
    private class Tab
    {
        private readonly GameObject Prefab;
        private readonly Button Button;
        private readonly Text Label;
        private readonly GameObject Selected;
        private readonly Text SelectedLabel;
        private readonly UITooltip? Tooltip;
        public bool IsSelected => Selected.activeInHierarchy;

        public Tab(Transform transform)
        {
            Prefab = transform.gameObject;
            Button = transform.GetComponent<Button>();
            Label = transform.Find("Text").GetComponent<Text>();
            Selected = transform.Find("Selected").gameObject;
            SelectedLabel = transform.Find("Selected/SelectedText").GetComponent<Text>();
            instance?.Tabs.Add(this);
            Tooltip = Button.gameObject.AddComponent<UITooltip>();
            // anchor and fixed position only works for gamepad :(
            // so perhaps we will just make our own UI Tooltip component
            Tooltip.m_anchor = transform.Find("TooltipAnchor").GetComponent<RectTransform>();
            Tooltip.m_fixedPosition = new Vector2(0f, 10f);
            Tooltip.m_tooltipPrefab = _tooltipPrefab;
            Tooltip.m_topic = "MWL_Ports_Tooltip";
        }

        public void Enable(bool enable) => Prefab.SetActive(enable);
        public void SetButton(UnityAction action) => Button.onClick.AddListener(action);
        public static void SetAllSelected(bool enable)
        {
            if (instance == null) return;
            foreach (Tab? tab in instance.Tabs)
            {
                if (tab.Selected == null) continue;
                tab.SetSelected(enable);
            }
        }
        public void SetLabel(string label)
        {
            Label.text = Localization.instance.Localize(label);
            SelectedLabel.text = Localization.instance.Localize(label);
        }

        public void SetTooltip(string tooltip)
        {
            if (Tooltip == null) return;
            Tooltip.m_text = Localization.instance.Localize(tooltip);
        }

        public void SetSelected(bool selected)
        {
            if (instance == null) return;
            foreach(Tab tab in instance.Tabs) tab.Selected.SetActive(false);
            Selected.SetActive(selected);
        }
    }

    private class RightPanel
    {
        private readonly Text Name;
        private readonly Text BodyText;
        private readonly RectTransform BodyRect;
        private readonly Button MapButton;
        private readonly Image MapIcon;
        public Port.PortInfo? MapInfo;
        
        private readonly float BodyMinHeight;
        
        public RightPanel(Text name, Text body, Button mapButton)
        {
            Name = name;
            BodyText = body;
            BodyRect = BodyText.rectTransform;
            MapButton = mapButton;
            MapIcon = MapButton.GetComponent<Image>();
            BodyMinHeight = body.rectTransform.sizeDelta.y;
        }
        public void SetMapButton(UnityAction action) => MapButton.onClick.AddListener(action);
        public void SetName(string name)
        {
            Name.text = Localization.instance.Localize(name);
        }
        public void SetBodyText(string body)
        {
            BodyText.text = Localization.instance.Localize(body);
            ResizePanel();
        }
        public void ResetDescription()
        {
            SetName("");
            SetBodyText("");
            ShowMapButton(null);
        }

        public void ShowMapButton(Port.PortInfo? info)
        {
            MapButton.gameObject.SetActive(info != null);
            MapInfo = info;
            if (Minimap.instance && MapIcon.sprite == null)
            {
                MapIcon.sprite = Minimap.instance.GetSprite(Minimap.PinType.Icon2);
            }
        }

        private void ResizePanel()
        {
            float height = GetTextPreferredHeight(BodyText, BodyRect);
            float finalHeight = Mathf.Max(height, BodyMinHeight);
            BodyRect.sizeDelta = new Vector2(BodyRect.sizeDelta.x, finalHeight);
        }
    
        private static float GetTextPreferredHeight(Text text, RectTransform rect)
        {
            if (string.IsNullOrEmpty(text.text)) return 0f;
        
            TextGenerator textGen = text.cachedTextGenerator;
        
            TextGenerationSettings settings = text.GetGenerationSettings(rect.rect.size);
            float preferredHeight = textGen.GetPreferredHeight(text.text, settings);
        
            return preferredHeight;
        }
    }

    public class Requirement
    {
        private readonly GameObject Prefab;
        private readonly List<RequirementItem> items = new List<RequirementItem>();
        public readonly Level level = new();
        public Requirement(GameObject prefab)
        {
            Prefab = prefab;
        }

        public void SetLevel(string text)
        {
            if (level.Label == null) return;
            level.Label.text = Localization.instance.Localize(text);
        }
        public void Add(Transform parent, string icon = "Icon", string name = "Name", string amount = "Amount")
        {
            Image Icon = parent.Find(icon).GetComponent<Image>();
            Text Label = parent.Find(name).GetComponent<Text>();
            Text Amount = parent.Find(amount).GetComponent<Text>();
            items.Add(new RequirementItem(Icon, Label, Amount));
        }

        public void LoadRequirements(Manifest manifest)
        {
            for (int i = 0; i < items.Count; i++)
            {
                RequirementItem item = items[i];
                if (manifest.GetRequirement(i) is not { } requirement)
                {
                    // if requirements are less than 4 (max amount of requirements currently available)
                    // then make the requirement item invisible
                    item.Hide();
                }
                else
                {
                    item.Set(requirement.Item.GetIcon(), requirement.Item.m_shared.m_name, requirement.Amount);
                }
            }
        }

        public void LoadCost(Port.ContainerPlacement tempContainers)
        {
            var total = tempContainers.GetCost();
            var currency = ShipmentManager.CurrencyItem ?? ObjectDB.instance.GetItemPrefab("Coins").GetComponent<ItemDrop>().m_itemData;
            var maxStack = currency.m_shared.m_maxStackSize;

            foreach (RequirementItem item in items)
            {
                if (total <= 0)
                {
                    item.Hide();
                    continue;
                }

                int amount = Mathf.Min(total, maxStack);
                total -= amount;

                item.Set(currency.GetIcon(), currency.m_shared.m_name, amount);
            }
        }

        public void LoadTeleportCost(List<Manifest.Requirement> requirements)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                RequirementItem item = items[i];
                if (i >= requirements.Count)
                {
                    item.Hide();
                    continue;
                }

                Manifest.Requirement data = requirements[i];
                item.Set(data.Item.GetIcon(), data.Item.m_shared.m_name, data.Amount);
            }
        }
        
        public void Update(float dt, Player? player)
        {
            if (player is null) return;
            foreach (RequirementItem? item in items) item.Update(dt, player);
        }
        
        public void SetActive(bool active) => Prefab.SetActive(active);

        private class RequirementItem
        {
            private readonly Image Icon;
            private readonly Text Name;
            private readonly Text Amount;

            private string? SharedName;
            private int Count;

            public RequirementItem(Image icon, Text name, Text amount)
            {
                Icon = icon;
                Name = name;
                Amount = amount;
            }

            public void Set(Sprite icon, string sharedName, int amount)
            {
                SharedName = sharedName;
                Count = amount;
                Icon.sprite = icon;
                Icon.color = Color.white;
                Name.text = Localization.instance.Localize(sharedName);
                Amount.text = amount.ToString();
            }

            public void Update(float dt, Player player)
            {
                if (SharedName == null) return;
                Inventory inventory = player.GetInventory();
                int count = inventory.CountItems(SharedName);
                bool hasRequirement = Count <= count;

                if (!hasRequirement)
                {
                    Amount.color = Mathf.Sin(Time.time * 10f) > 0.0 ? Color.red : Color.white;
                }
                else
                {
                    Amount.color = Color.white;
                }
            }

            public void Hide()
            {
                Icon.color = Color.clear;
                Name.text = "";
                Amount.text = "";
                Count = 0;
                SharedName = null;
            }
        }

        public class Level
        {
            public Image? Icon;
            public Text? Label;
        }
    }

    
    private Vector3 mouseDifference = Vector3.zero;

    // Helper methods for teleport cost calculation and consumption.
    private static List<Manifest.Requirement> GetTeleportRequirements(Port.PortInfo destination)
    {
        float costPerMeter = TeleportCostPerMeter?.Value ?? 0.5f;
        int coinCost = Mathf.FloorToInt(destination.GetDistance(Player.m_localPlayer) * costPerMeter);
        
        // Free teleport if cost is zero
        if (coinCost <= 0) return new List<Manifest.Requirement>();
        
        return new List<Manifest.Requirement>
        {
            new Manifest.Requirement
            {
                Item = ShipmentManager.CurrencyItem ?? ObjectDB.instance.GetItemPrefab("Coins").GetComponent<ItemDrop>().m_itemData,
                Amount = coinCost
            }
        };
    }

    private static bool HasTeleportRequirements(Player player, List<Manifest.Requirement> requirements)
    {
        Inventory inventory = player.GetInventory();
        foreach (Manifest.Requirement req in requirements)
        {
            if (inventory.CountItems(req.Item.m_shared.m_name) < req.Amount)
                return false;
        }
        return true;
    }

    private static void ConsumeTeleportRequirements(Player player, List<Manifest.Requirement> requirements)
    {
        Inventory inventory = player.GetInventory();
        foreach (Manifest.Requirement req in requirements)
        {
            inventory.RemoveItem(req.Item.m_shared.m_name, req.Amount);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // only allow drag if L.Alt is held down
        if (!Input.GetKey(KeyCode.LeftAlt)) return;
        // move panel
        m_rect.position = Input.mousePosition + mouseDifference;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // get the mouse position to make the drag point linked to the mouse position
        Vector2 pos = eventData.position;
        mouseDifference = m_rect.position - new Vector3(pos.x, pos.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // save new position to config file
        if (PanelPositionConfig != null) PanelPositionConfig.Value = m_rect.position;
    }

    private class HowTo
    {
        private readonly Button button;
        private readonly Text text;
        private readonly Image glow;
        private readonly Image icon;

        public HowTo(Transform transform)
        {
            button = transform.GetComponent<Button>();
            text = transform.Find("Text").GetComponent<Text>();
            glow = transform.Find("Glow").GetComponent<Image>();
            icon = transform.Find("Icon").GetComponent<Image>();
        }
        
        public void SetButton(UnityAction action) => button.onClick.AddListener(action);
        public void SetText(string label) => text.text = Localization.instance.Localize(label);
        public void SetGlow(bool enable) => glow.enabled = enable;

        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
            icon.preserveAspect = true;
        }
    }
}