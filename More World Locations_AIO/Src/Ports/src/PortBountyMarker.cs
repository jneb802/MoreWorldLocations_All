using System;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO;

// Optional integration contract. MWL never creates the NPC or references its mod.
public sealed class PortBountyMarker : MonoBehaviour
{
    public const string MarkerName = "MWL_BodyRetrievalMarker";
    public const int ContractVersion = 1;
    public static readonly Vector3 LocalPosition = new Vector3(2.5f, 0f, 0f);
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

    public static void AddToPrefab(GameObject port)
    {
        Transform existing = port.transform.Find(MarkerName);
        if (existing != null) return;
        GameObject marker = new GameObject(MarkerName);
        marker.transform.SetParent(port.transform, false);
        marker.transform.localPosition = LocalPosition;
        marker.AddComponent<PortBountyMarker>();
    }

    private void Start()
    {
        _port = GetComponentInParent<ZNetView>();
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
