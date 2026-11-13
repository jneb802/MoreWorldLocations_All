using System;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO;

// Optional integration contract. MWL never creates the NPC or references its mod.
public sealed class PortBountyMarker : MonoBehaviour
{
    public const string PrefabName = "PraetorisBountyNPC";
    public const int ContractVersion = 2;
    private static readonly HashSet<PortBountyMarker> Live = new();
    private static event Action<ZNetView, Transform>? Loaded;
    private ZNetView? _port;

    public static void Subscribe(Action<ZNetView, Transform> listener)
    {
        Loaded -= listener;
        Loaded += listener;
        foreach (PortBountyMarker marker in new List<PortBountyMarker>(Live))
            if (marker != null && marker._port != null && marker._port.IsValid())
                Invoke(listener, marker._port, marker.transform);
    }

    public static void Unsubscribe(Action<ZNetView, Transform> listener) => Loaded -= listener;

    public static void RegisterPrefab()
    {
        if (PrefabManager.Instance.GetPrefab(PrefabName) != null) return;
        // JVLmock_PraetorisBountyNPC in a location resolves to this persistent
        // anchor. No NPC is created unless the author placed a mock.
        GameObject marker = PrefabManager.Instance.CreateEmptyPrefab(PrefabName);
        UnityEngine.Object.DestroyImmediate(marker.GetComponent<MeshRenderer>());
        UnityEngine.Object.DestroyImmediate(marker.GetComponent<MeshFilter>());
        UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
        marker.GetComponent<ZNetView>().m_persistent = true;
        marker.AddComponent<PortBountyMarker>();
        PrefabManager.Instance.AddPrefab(new CustomPrefab(marker, false));
    }

    private void Start()
    {
        _port = GetComponent<ZNetView>();
        if (_port == null || !_port.IsValid()) return;
        Live.Add(this);
        if (Loaded == null) return;
        foreach (Delegate listener in Loaded.GetInvocationList())
            Invoke((Action<ZNetView, Transform>)listener, _port, transform);
    }

    private static void Invoke(Action<ZNetView, Transform> listener, ZNetView port, Transform marker)
    {
        try { listener(port, marker); }
        catch (Exception error) { Debug.LogWarning("MWL optional bounty marker failed: " + error.Message); }
    }

    private void OnDestroy() => Live.Remove(this);
}
