using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;

[BepInPlugin("local.mwl.portproof", "MWL Port Proof", "1.0.0")]
public class PortProof : BaseUnityPlugin
{
    private bool reportedLocations;
    private void Update()
    {
        if (reportedLocations || !ZNet.instance || !ZNet.instance.IsServer() || !ZoneSystem.instance) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        if (!(bool)typeof(ZoneSystem).GetField("m_locationsGenerated", flags).GetValue(ZoneSystem.instance)) return;
        IDictionary instances = (IDictionary)typeof(ZoneSystem).GetField("m_locationInstances", flags).GetValue(ZoneSystem.instance);
        if (instances.Count == 0) return;
        reportedLocations = true;
        Logger.LogInfo("PORTPROOF_WORLD locations=" + instances.Count);
        foreach (ZoneSystem.LocationInstance location in instances.Values.Cast<ZoneSystem.LocationInstance>().Where(l => l.m_location.m_prefabName.StartsWith("MWL_") && l.m_location.m_prefabName.Contains("Port")).Take(8))
            Logger.LogInfo("PORTPROOF_LOCATION " + location.m_location.m_prefabName + " " + location.m_position + " placed=" + location.m_placed);
    }
    private Type PortType => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("More_World_Locations_AIO.Port")).First(t => t != null);
    private Component Port => UnityEngine.Object.FindObjectsByType(PortType, FindObjectsSortMode.None).Cast<Component>().OrderBy(p => Vector3.Distance(p.transform.position, Player.m_localPlayer.transform.position)).First();
    private Container[] Chests => !Player.m_localPlayer ? Array.Empty<Container>() : UnityEngine.Object.FindObjectsByType<Container>(FindObjectsSortMode.None).Where(c => c.name.StartsWith("MWL_port_chest_") && c.GetComponent<ZNetView>() is ZNetView view && view.IsValid() && Vector3.Distance(c.transform.position, Player.m_localPlayer.transform.position) < 100).OrderBy(c => c.GetComponent<ZNetView>().GetZDO().m_uid.ToString()).ToArray();
    private object UI => PortType.Assembly.GetType("More_World_Locations_AIO.PortUI").GetField("instance").GetValue(null);
    private Type ManagerType => PortType.Assembly.GetType("More_World_Locations_AIO.ShipmentManager");
    private void Awake()
    {
        new Terminal.ConsoleCommand("portproof_go", "Move the development character to x y z", a => Player.m_localPlayer.TeleportTo(new Vector3(float.Parse(a[1]), float.Parse(a[2]), float.Parse(a[3])), Quaternion.identity, true));
        new Terminal.ConsoleCommand("portproof_open", "Open the nearest shipping port", a =>
        {
            ((Interactable)Port).Interact(Player.m_localPlayer, false, false);
            a.Context.AddString("OPEN requested; wait for port ownership and panel visibility");
        });
        new Terminal.ConsoleCommand("portproof_hide", "Close the shipping port panel", a => UI.GetType().GetMethod("Hide").Invoke(UI, null));
        new Terminal.ConsoleCommand("portproof_chest_open", "Use the nearest shipment chest's normal interaction", a =>
        {
            Container chest = Chests.FirstOrDefault();
            if (chest == null) { a.Context.AddString("CHEST OPEN waiting for a loaded chest and local player"); return; }
            chest.Interact(Player.m_localPlayer, false, false);
            a.Context.AddString("CHEST OPEN requested");
        });
        new Terminal.ConsoleCommand("portproof_chest_close", "Close the normal inventory window", a => InventoryGui.instance.Hide());
        new Terminal.ConsoleCommand("portproof_buy", "Supply recipe materials and buy through the normal manifest button", a =>
        {
            Type type = PortType.Assembly.GetType("More_World_Locations_AIO.Manifest");
            IDictionary manifests = (IDictionary)type.GetField("Manifests").GetValue(null);
            object manifest = manifests.Values.Cast<object>().First(m => ((Container)type.GetField("Chest").GetValue(m)).name == "MWL_port_chest_wood");
            Player.m_localPlayer.GetInventory().AddItem("Wood", 10, 1, 0, 0L, "PortProof", false);
            Player.m_localPlayer.GetInventory().AddItem("Resin", 5, 1, 0, 0L, "PortProof", false);
            UI.GetType().GetMethod("OnManifestTab").Invoke(UI, null);
            UI.GetType().GetField("m_selectedManifest", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(UI, manifest);
            UI.GetType().GetMethod("OnMainButton").Invoke(UI, null);
            Status(a.Context.AddString);
        });
        new Terminal.ConsoleCommand("portproof_add", "Add 17 Wood to an owned chest through its normal inventory", a =>
        {
            Container chest = Chests.First(c => c.GetComponent<ZNetView>().IsOwner());
            chest.GetInventory().AddItem("Wood", 17, 1, 0, 0L, "PortProof", false);
            Status(a.Context.AddString);
        });
        new Terminal.ConsoleCommand("portproof_roundtrip_send", "Test fixture: send to this same port through the normal send button and server RPC", a =>
        {
            object destination = Activator.CreateInstance(PortType.GetNestedType("PortInfo"), Port.GetComponent<ZNetView>().GetZDO());
            Player.m_localPlayer.GetInventory().AddItem("Coins", 500, 1, 0, 0L, "PortProof", false);
            UI.GetType().GetMethod("OnPortTab").Invoke(UI, null);
            UI.GetType().GetField("m_selectedDestination", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(UI, destination);
            UI.GetType().GetMethod("OnMainButton").Invoke(UI, null);
            Status(a.Context.AddString);
        });
        new Terminal.ConsoleCommand("portproof_deliver", "Open an arrived shipment through the normal delivery button", a =>
        {
            string id = Port.GetComponent<ZNetView>().GetZDO().GetString("PortGUID");
            IEnumerable deliveries = (IEnumerable)ManagerType.GetMethod("GetDeliveries").Invoke(null, new object[] { id });
            object delivery = deliveries.Cast<object>().FirstOrDefault(d => d.GetType().GetField("State").GetValue(d).ToString() == "Delivered");
            if (delivery == null) { a.Context.AddString("DELIVERY not arrived"); return; }
            UI.GetType().GetMethod("OnDeliveryTab").Invoke(UI, null);
            UI.GetType().GetField("m_selectedDelivery").SetValue(UI, delivery);
            UI.GetType().GetMethod("OnMainButton").Invoke(UI, null);
            Status(a.Context.AddString);
        });
        new Terminal.ConsoleCommand("portproof_status", "Report live shipment chests and saved snapshot", a => Status(a.Context.AddString));
        new Terminal.ConsoleCommand("portproof_seed", "Create one manifest with 17 Wood via MWL's normal spawn method", a =>
        {
            Component port = Port;
            Type type = PortType.Assembly.GetType("More_World_Locations_AIO.Manifest");
            IDictionary manifests = (IDictionary)type.GetField("Manifests").GetValue(null);
            object manifest = manifests.Values.Cast<object>().First(m => ((Container)type.GetField("Chest").GetValue(m)).name == "MWL_port_chest_wood");
            Container chest = (Container)PortType.GetMethod("SpawnContainer").Invoke(port, new[] { manifest });
            if (chest == null) { a.Context.AddString("SEED failed: no chest"); return; }
            chest.GetInventory().AddItem("Wood", 17, 1, 0, 0L, "PortProof", false);
            Status(a.Context.AddString);
        });
        new Terminal.ConsoleCommand("portproof_take", "Use Valheim TakeAll on shipment chest index", a =>
        {
            int index = a.Length > 1 ? int.Parse(a[1]) : 0;
            Container[] chests = Chests;
            if (index < 0 || index >= chests.Length) { a.Context.AddString("TAKE failed: no loaded chest at that index"); return; }
            Container chest = chests[index];
            a.Context.AddString("TAKE requested chest=" + chest.GetComponent<ZNetView>().GetZDO().m_uid + " accepted=" + chest.TakeAll(Player.m_localPlayer));
        });
    }
    private void Status(Action<string> output)
    {
        if (!Player.m_localPlayer) { output("PORTPROOF waiting for local player"); return; }
        Container[] chests = Chests;
        output("PORTPROOF chests=" + chests.Length + " totalWood=" + chests.Sum(c => c.GetInventory().CountItems("$item_wood")) + " playerWood=" + Player.m_localPlayer.GetInventory().CountItems("$item_wood"));
        foreach (Container c in chests)
            output("CHEST id=" + c.GetComponent<ZNetView>().GetZDO().m_uid + " prefab=" + c.name + " wood=" + c.GetInventory().CountItems("$item_wood") + " ownerLocal=" + c.GetComponent<ZNetView>().IsOwner() + " persistent=" + c.GetComponent<ZNetView>().GetZDO().Persistent + " inUse=" + c.GetComponent<ZNetView>().GetZDO().GetInt(ZDOVars.s_inUse) + " pos=" + c.transform.position);
        ZDO zdo = Port.GetComponent<ZNetView>().GetZDO();
        string saved = zdo.GetString("PortItems".GetStableHashCode());
        output("PORT id=" + zdo.m_uid + " snapshotBytes=" + saved.Length + " persistentStorage=" + zdo.GetBool("MWL_PortPersistentContainers") + " ownerLocal=" + Port.GetComponent<ZNetView>().IsOwner() + " pos=" + Port.transform.position);
        object placements = PortType.GetField("m_containers").GetValue(Port);
        Container[] linked = (Container[])placements.GetType().GetMethod("GetSpawnedContainers").Invoke(placements, null);
        output("LINKED chests=" + linked.Length + " uiVisible=" + (UI is Component ui && ui.gameObject.activeInHierarchy) + " playerCoins=" + Player.m_localPlayer.GetInventory().CountItems("$item_coins"));
    }
}
