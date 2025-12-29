using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Jotunn;
using More_World_Locations_AIO.Managers;
using More_World_Locations_AIO.tutorials;

namespace More_World_Locations_AIO;

public static class PortInit
{
    private static BaseUnityPlugin? _plugin;
    public static BaseUnityPlugin plugin
    {
        get
        {
            if (_plugin is null)
            {
                IEnumerable<TypeInfo> types;
                try
                {
                    types = Assembly.GetExecutingAssembly().DefinedTypes.ToList();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).Select(t => t.GetTypeInfo());
                }
                _plugin = (BaseUnityPlugin)BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(types.First(t => t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));
            }
            return _plugin;
        }
    }
    private static bool hasConfigSync = true;
    private static object? _configSync;
    public static object? configSync
    {
        get
        {
            if (_configSync == null && hasConfigSync)
            {
                if (Assembly.GetExecutingAssembly().GetType("ServerSync.ConfigSync") is { } configSyncType)
                {
                    _configSync = Activator.CreateInstance(configSyncType, plugin.Info.Metadata.GUID + " RS_PortInitManager");
                    configSyncType.GetField("CurrentVersion").SetValue(_configSync, plugin.Info.Metadata.Version.ToString());
                    configSyncType.GetProperty("IsLocked")!.SetValue(_configSync, true);
                }
                else
                {
                    hasConfigSync = false;
                }
            }

            return _configSync;
        }
    }
    
    public static GameObject root = null!;

    public enum Toggle { On = 1, Off = 0 }

    private static void SetupManagers()
    {
        plugin.gameObject.AddComponent<ShipmentManager>();
        plugin.gameObject.AddComponent<PortManager>();
    }

    private static void SetupConfigs()
    {
        PortUI.PanelPositionConfig = config("Shipment Ports", "Panel Position", new Vector3(1760f, 850f, 0f), "Set position of UI", false);
        PortUI.PanelPositionConfig.SettingChanged += PortUI.OnPanelPositionConfigChange;
        ShipmentManager.TransitByDistance = config("Shipment Ports", "Time Per Meter", 2f, "Set seconds per meter for shipment transit");
        ShipmentManager.CurrencyConfig = config("Shipment Ports", "Shipment Currency", "Coins", "Set item prefab to use as currency to ship items");
        ShipmentManager.CurrencyConfig.SettingChanged += (_, _) => ShipmentManager._currencyItem = null;
        ShipmentManager.OverrideTransitTime = config("Shipment Ports", "Override Transit Duration", Toggle.Off, "If on, transit time will be based off override instead of calculated based off distance");
        ShipmentManager.TransitTime = config("Shipment Ports", "Transit Duration", 1800f, "Set override transit duration in seconds, 1800 = 30min");
        ShipmentManager.ExpirationEnabled = config("Shipment Ports", "Expires", Toggle.On, "If on, shipments can expire");
        ShipmentManager.ExpirationTime = config("Shipment Ports", "Expiration Time", 3600f, "Set time until expiration, 3600 = 1 hour");
        PortUI.BkgOption = config("Shipment Ports", "Background", PortUI.BackgroundOption.Opaque, "Set background type", false);
        PortUI.BkgOption.SettingChanged += PortUI.OnBackgroundOptionChange;
        PortUI.UseTeleportTab = config("Shipment Ports", "Teleport To Ports", Toggle.Off, "If on, players can teleport to ports");
        PortUI.UseTeleportTab.SettingChanged += PortUI.OnUseTeleportTabChange;
    }

    private static void SetupLocations()
    {
        BlueprintLocation large = new BlueprintLocation("portbundle", "MWL_Port_Location_Large");
        large.OnCreated += blueprint =>
        {
            if (blueprint.Location == null) return;
            blueprint.Location.Setup();
            blueprint.Location.Biome = Heightmap.Biome.All;
            blueprint.Location.Placement.Altitude.Min = -10f;
            blueprint.Location.Placement.Altitude.Max = 10f;
            blueprint.Location.Placement.SlopeRotation = true;
            blueprint.Location.Placement.ClearArea = true;
            blueprint.Location.Placement.Quantity = 100;
            blueprint.Location.Placement.Prioritized = true;
            blueprint.Location.Group.Name = "MWL_Ports";
            blueprint.Location.Placement.DistanceFromSimilar.Min = 300f;
        };
    }

    private static void SetupPort()
    {
        Blueprint portTrader = new Blueprint("portbundle", "MWL_PortTrader");
        portTrader.Prefab.AddComponent<Port>();
        portTrader.OnCreated += blueprint =>
        {
            foreach (Transform child in blueprint.Prefab.transform)
            {
                if (child.TryGetComponent(out Trader component))
                {
                    child.gameObject.name = "PortTrader";
                    PortTrader? trader = child.gameObject.AddComponent<PortTrader>();
                    trader.m_standRange = component.m_standRange;
                    trader.m_greetRange = component.m_greetRange;
                    trader.m_byeRange = component.m_byeRange;
                    trader.m_hideDialogDelay = component.m_hideDialogDelay;
                    trader.m_randomTalkInterval = component.m_randomTalkInterval;
                    trader.m_dialogHeight = component.m_dialogHeight;
                    trader.m_randomTalkFX = component.m_randomTalkFX;
                    trader.m_randomGreetFX =  component.m_randomGreetFX;
                    trader.m_randomGoodbyeFX =  component.m_randomGoodbyeFX;
                }
                child.gameObject.RemoveAllComponents<MonoBehaviour>(false, typeof(PortTrader));
            }
            PrefabManager.RegisterPrefab(blueprint.Prefab);
        };
    }

    private static void SetupManifests()
    {
        // simple class to clone in-game assets
        Clone piece_chest_wood = new Clone("piece_chest_wood", "MWL_port_chest_wood");
        piece_chest_wood.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
            var placeEffect = prefab.GetComponent<Piece>().m_placeEffect;

            prefab.RemoveComponent<Piece>();
            prefab.RemoveComponent<WearNTear>();
            prefab.GetComponent<ZNetView>().m_persistent = false;

            // current size 10
            Manifest manifest = new Manifest("Wooden Shipment", prefab.GetComponent<Container>());
            manifest.CostToShip = 50;
            manifest.Recipe.Add("Wood", 10);
            manifest.Recipe.Add("Resin", 5);
            manifest.Icon = icon;
            manifest.PlaceEffect = placeEffect;
        };
        
        Clone piece_chest_barrel = new Clone("piece_chest_barrel", "MWL_port_chest_barrel");
        piece_chest_barrel.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
            var placeEffect = prefab.GetComponent<Piece>().m_placeEffect;
            prefab.RemoveComponent<Piece>();
            prefab.RemoveComponent<WearNTear>();
            prefab.GetComponent<ZNetView>().m_persistent = false;

            // current size 12
            Manifest manifest = new Manifest("Barrel Shipment", prefab.GetComponent<Container>());
            manifest.CostToShip = 55;
            manifest.Recipe.Add("Wood", 10);
            manifest.Recipe.Add("BarrelRings", 1);
            manifest.Icon = icon;
            manifest.PlaceEffect = placeEffect;
        };
        
        Clone piece_chest = new Clone("piece_chest", "MWL_port_chest_finewood");
        piece_chest.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
            var placeEffect = prefab.GetComponent<Piece>().m_placeEffect;
            prefab.RemoveComponent<Piece>();
            prefab.RemoveComponent<WearNTear>();
            prefab.GetComponent<ZNetView>().m_persistent = false;

            // current size 24
            Manifest manifest = new Manifest("Fine Shipment", prefab.GetComponent<Container>());
            manifest.CostToShip = 100;
            manifest.Recipe.Add("FineWood", 10);
            manifest.Recipe.Add("Iron", 2);
            manifest.Recipe.Add("BlackMetal", 6);
            manifest.Icon = icon;
            manifest.PlaceEffect = placeEffect;
        };
        
        Clone piece_chest_blackmetal = new Clone("piece_chest_blackmetal", "MWL_port_chest_blackmetal");
        piece_chest_blackmetal.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
            var placeEffect = prefab.GetComponent<Piece>().m_placeEffect;
            prefab.RemoveComponent<Piece>();
            prefab.RemoveComponent<WearNTear>();
            prefab.GetComponent<ZNetView>().m_persistent = false;

            // current size 32
            Manifest manifest = new Manifest("Fuling Shipment", prefab.GetComponent<Container>());
            manifest.CostToShip = 280;
            manifest.Recipe.Add("FineWood", 10);
            manifest.Recipe.Add("Tar", 2);
            manifest.Recipe.Add("BlackMetal", 6);
            manifest.Icon = icon;
            manifest.PlaceEffect = placeEffect;
        };
        
        Clone TreasureChest_dvergrtower = new Clone("TreasureChest_dvergrtower", "MWL_port_chest_dvergr");
        TreasureChest_dvergrtower.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
            
            // use the same place effect as the blackmetal chest
            var placeEffect = Jotunn.Managers.PrefabManager.Cache.GetPrefab<Piece>("piece_chest_blackmetal").m_placeEffect;
            prefab.RemoveComponent<Piece>();
            prefab.RemoveComponent<WearNTear>();
            prefab.GetComponent<ZNetView>().m_persistent = false;
            
            // current size 8, let's change that
            var container = prefab.GetComponent<Container>();
            container.m_width = 8;
            container.m_height = 5;
            // new size 40
            // this container has default items, remove
            container.m_defaultItems.m_drops.Clear();
            Manifest manifest = new Manifest("Dvergr Shipment", prefab.GetComponent<Container>());
            manifest.CostToShip = 410;
            manifest.Recipe.Add("YggdrasilWood", 10);
            manifest.Recipe.Add("Copper", 2);
            manifest.PlaceEffect = placeEffect;
        };
    }
    
    public static void Init(GameObject m_root)
    {
        root = m_root;
        Commands.Setup();
        SetupManagers();
        SetupConfigs();
        ShipmentManager.PrefabsToSearch.Add("MWL_PortTrader");
        SetupLocations();
        SetupPort();
        SetupManifests();
        PortTutorial.Setup();
    }
    private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
    {
        ConfigEntry<T> configEntry = plugin.Config.Bind(group, name, value, description);
        if (synchronizedSetting) configSync?.GetType().GetMethod("AddConfigEntry")!.MakeGenericMethod(typeof(T)).Invoke(_configSync, new object[] { configEntry });
        return configEntry;
    }
    private static ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    private class ConfigurationManagerAttributes
    {
        [UsedImplicitly] public int? Order = null!;
        [UsedImplicitly] public bool? Browsable = null!;
        [UsedImplicitly] public string? Category = null!;
        [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
    }
    
}