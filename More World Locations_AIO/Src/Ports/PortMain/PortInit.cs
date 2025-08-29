using Jotunn.Entities;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using More_World_Locations_AIO;
using More_World_Locations_AIO.Managers;
using More_World_Locations_AIO.tutorials;
using ServerSync;
using UnityEngine;

namespace More_World_Locations_AIO;

public class PortInit
{
    public static GameObject root = null!;
    public static BaseUnityPlugin instance = null!;

    public enum Toggle { On = 1, Off = 0 }
    
    public static void Init(BaseUnityPlugin plugin, GameObject m_root)
    {
        instance = plugin;
        root = m_root;
        
        // PortNames.Setup();
        Commands.Setup();
        
        // make shipment manager a monobehavior to keep functions within it's scope while taking advantages of monobehaviors
        plugin.gameObject.AddComponent<ShipmentManager>();
        plugin.gameObject.AddComponent<PortManager>();
        
        ShipmentManager.PrefabsToSearch.Add("MWL_Port", "MWL_PortTrader"); // add port variants to search for
        // this is used by server to iterate through ZDOs and send them to players
        // this is how portals work

        PortUI.PanelPositionConfig = config("3 - UI", "Panel Position", new Vector3(1760f, 850f, 0f), "Set position of UI", false);
        PortUI.PanelPositionConfig.SettingChanged += PortUI.OnPanelPositionConfigChange;
        ShipmentManager.TransitByDistance = config("2 - Settings", "Time Per Meter", 2f, "Set seconds per meter for shipment transit");
        ShipmentManager.CurrencyConfig = config("2 - Settings", "Shipment Currency", "Coins", "Set item prefab to use as currency to ship items");
        ShipmentManager.CurrencyConfig.SettingChanged += (_, _) => ShipmentManager._currencyItem = null;
        ShipmentManager.OverrideTransitTime = config("2 - Settings", "Override Transit Duration", Toggle.Off, "If on, transit time will be based off override instead of calculated based off distance");
        ShipmentManager.TransitTime = config("2 - Settings", "Transit Duration", 1800f, "Set override transit duration in seconds, 1800 = 30min");
        ShipmentManager.ExpirationEnabled = config("2 - Settings", "Expires", Toggle.On, "If on, shipments can expire");
        ShipmentManager.ExpirationTime = config("2 - Settings", "Expiration Time", 3600f, "Set time until expiration, 3600 = 1 hour");
        PortUI.BkgOption = config("3 - UI", "Background", PortUI.BackgroundOption.Opaque, "Set background type", false);
        PortUI.BkgOption.SettingChanged += PortUI.OnBackgroundOptionChange;
        PortUI.UseTeleportTab = config("2 - Settings", "Teleport To Ports", Toggle.Off, "If on, players can teleport to ports");
        PortUI.UseTeleportTab.SettingChanged += PortUI.OnUseTeleportTabChange;
        // this gets created after blueprints
        // it will iterate through children to find prefabs
        // and replace them
        BlueprintLocation location = new BlueprintLocation("portbundle", "MWL_Port_Location");
        location.OnCreated += blueprint =>
        {
            if (blueprint.Location == null) return;
            blueprint.Location.Setup();
            blueprint.Location.Biome = Heightmap.Biome.All;
            blueprint.Location.Placement.Altitude.Min = 10f;
            blueprint.Location.Placement.ClearArea = true;
            blueprint.Location.Placement.Quantity = 100;
            blueprint.Location.Placement.Prioritized = true;
            blueprint.Location.Group.Name = "MWL_Ports";
            blueprint.Location.Placement.DistanceFromSimilar.Min = 300f;
            blueprint.Location.Icon.Enabled = false;
            // blueprint.Location.Icon.Icon = MySprite ----> if you want to use a custom sprite
            // blueprint.Location.Icon.InGameIcon = LocationManager.IconSettings.LocationIcon.Hildir;
        };
        
        // this gets created before blueprint location
        // since this prefab has a ZNetView, we make sure to create it as its own prefab first
        // then the location uses it when creating itself
        Blueprint port = new Blueprint("portbundle", "MWL_Port");
        port.Prefab.AddComponent<Port>();
        port.OnCreated += blueprint =>
        {
            foreach (Transform child in blueprint.Prefab.transform)
            {
                if (child.gameObject.HasComponent<TerrainModifier>()) continue;
                child.gameObject.RemoveAllComponents<MonoBehaviour>(false, typeof(SnapToGround));
            }
            
            PrefabManager.RegisterPrefab(blueprint.Prefab);
        };
        
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
            blueprint.Location.Icon.Enabled = false;
            // blueprint.Location.Icon.Icon = MySprite ----> if you want to use a custom sprite
            // blueprint.Location.Icon.InGameIcon = LocationManager.IconSettings.LocationIcon.Boss;
        };
        
        Blueprint portTrader = new Blueprint("portbundle", "MWL_PortTrader");
        portTrader.Prefab.AddComponent<Port>();
        portTrader.OnCreated += blueprint =>
        {
            foreach (Transform child in blueprint.Prefab.transform)
            {
                if (child.TryGetComponent(out Trader component))
                {
                    Debug.LogWarning("Adding port trader component on: " + child.name);
                    child.gameObject.name = "PortTrader";
                    var trader = child.gameObject.AddComponent<PortTrader>();
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
        
        // simple class to clone in-game assets
        Clone piece_chest_wood = new Clone("piece_chest_wood", "MWL_port_chest_wood");
        piece_chest_wood.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
            prefab.RemoveComponent<Piece>();
            prefab.RemoveComponent<WearNTear>();
            prefab.GetComponent<ZNetView>().m_persistent = false;

            // current size 10
            Manifest manifest = new Manifest("Wooden Shipment", prefab.GetComponent<Container>());
            manifest.CostToShip = 50;
            manifest.Recipe.Add("Wood", 10);
            manifest.Recipe.Add("Resin", 5);
            manifest.Icon = icon;
        };
        
        Clone piece_chest_barrel = new Clone("piece_chest_barrel", "MWL_port_chest_barrel");
        piece_chest_barrel.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
            prefab.RemoveComponent<Piece>();
            prefab.RemoveComponent<WearNTear>();
            prefab.GetComponent<ZNetView>().m_persistent = false;

            // current size 12
            Manifest manifest = new Manifest("Barrel Shipment", prefab.GetComponent<Container>());
            manifest.CostToShip = 55;
            manifest.Recipe.Add("Wood", 10);
            manifest.Recipe.Add("BarrelRings", 1);
            manifest.Icon = icon;
        };
        
        Clone piece_chest = new Clone("piece_chest", "MWL_port_chest_finewood");
        piece_chest.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
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
        };
        
        Clone piece_chest_blackmetal = new Clone("piece_chest_blackmetal", "MWL_port_chest_blackmetal");
        piece_chest_blackmetal.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
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
        };
        
        Clone TreasureChest_dvergrtower = new Clone("TreasureChest_dvergrtower", "MWL_port_chest_dvergr");
        TreasureChest_dvergrtower.OnCreated += prefab =>
        {
            var icon = prefab.GetComponent<Piece>().m_icon;
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
        };

        PortTutorial.Setup();
    }
    
    private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        ConfigDescription extendedDescription =
            new(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);
        ConfigEntry<T> configEntry = BepinexConfigs.Config.Bind(group, name, value, extendedDescription);
        //var configEntry = Config.Bind(group, name, value, description);

        // SyncedConfigEntry<T> syncedConfigEntry = More_World_Locations_AIOPlugin.ConfigSync.AddConfigEntry(configEntry);
        // syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    private static ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }

    private class ConfigurationManagerAttributes
    {
        [UsedImplicitly] public int? Order = null!;
        [UsedImplicitly] public bool? Browsable = null!;
        [UsedImplicitly] public string? Category = null!;
        [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
    }
    
}