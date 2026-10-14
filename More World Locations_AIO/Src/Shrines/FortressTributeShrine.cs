using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ValheimFortress;

namespace More_World_Locations_AIO.Shrines;

/// <summary>Offering interaction for ShrineEvent1. Fortress owns the actual challenge.</summary>
public sealed class FortressTributeShrine : MonoBehaviour, Interactable, Hoverable
{
    public const string ShrineName = "VF_wild_shrine_yellow1";
    public const string TributePrefab = "Resin";
    // Fortress 0.39 returns the event definition, but does not expose its tribute amount.
    public const int TributeAmount = 50;
    public Transform SpawnRoot = null!;

    private const string RequestControl = "MWL_ShrineEvent_RequestControl";
    private const string RunnerKey = "MWL_ShrineEvent_Runner";
    private const string StartedKey = "MWL_ShrineEvent_Started";
    private const string ReservationKey = "MWL_ShrineEvent_Reservation";
    private const string ReservationUntilKey = "MWL_ShrineEvent_ReservationUntil";
    private const string RunnerPrefab = "VF_api_challenge_runner";
    private ZNetView view = null!;
    private bool pending;

    private void Update() => Initialize();

    private bool Initialize()
    {
        if (view != null) return view.IsValid();
        ZNetView candidate = GetComponentInParent<ZNetView>();
        // LocationProxy supplies this view after the location prefab is instantiated.
        if (candidate == null || !candidate.IsValid()) return false;
        view = candidate;
        view.Register(RequestControl, OnRequestControl);
        return true;
    }

    private static long Now => ZNet.instance.GetTime().Ticks;
    public bool EventActive
    {
        get
        {
            if (!Initialize()) return false;
            ZDO data = view.GetZDO();
            ZDOID runner = data.GetZDOID(RunnerKey);
            if (runner == ZDOID.None) return false;
            // Check the network record, not just a loaded GameObject: leaving the
            // location must not make an existing event appear to have finished.
            return ZDOMan.instance.GetZDO(runner) != null || Now - data.GetLong(StartedKey) < TimeSpan.FromSeconds(30).Ticks;
        }
    }

    private bool ReservedByOther(long peer)
    {
        ZDO data = view.GetZDO();
        long holder = data.GetLong(ReservationKey);
        return holder != 0 && holder != peer && data.GetLong(ReservationUntilKey) > Now;
    }

    private void OnRequestControl(long sender)
    {
        if (!view.IsOwner() || EventActive || ReservedByOther(sender)) return;
        ZDO data = view.GetZDO();
        data.Set(ReservationKey, sender);
        data.Set(ReservationUntilKey, Now + TimeSpan.FromSeconds(10).Ticks);
        data.SetOwner(sender);
    }

    public string GetHoverName() => "Plains challenge shrine";
    public float GetHoverOffset() => 0f;
    public string GetHoverText() => Localization.instance.Localize(GetHoverName() + "\n" +
        (EventActive ? "Challenge in progress" : "[<color=yellow><b>$KEY_Use</b></color>] Offer 50 $item_resin"));

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (hold) return false;
        return Offer(user);
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        if (item?.m_dropPrefab == null || item.m_dropPrefab.name != TributePrefab)
        {
            user.Message(MessageHud.MessageType.Center, "This shrine accepts 50 resin.");
            return false;
        }
        return Offer(user);
    }

    private static int CountTribute(Humanoid user) => user.GetInventory().GetAllItems()
        .Where(item => item.m_dropPrefab != null && item.m_dropPrefab.name == TributePrefab).Sum(item => item.m_stack);

    private bool Offer(Humanoid user)
    {
        if (user != Player.m_localPlayer || !Initialize()) return false;
        if (pending || EventActive) { user.Message(MessageHud.MessageType.Center, "A challenge is already in progress."); return true; }
        if (CountTribute(user) < TributeAmount) { user.Message(MessageHud.MessageType.Center, "You need 50 resin."); return true; }
        pending = true;
        StartCoroutine(StartOffering(user));
        return true;
    }

    private IEnumerator StartOffering(Humanoid user)
    {
        long peer = ZNet.GetUID();
        try
        {
            if (!view.IsOwner()) view.InvokeRPC(RequestControl);
            for (int attempt = 0; attempt < 30 && !view.IsOwner(); attempt++) yield return new WaitForSeconds(0.1f);
            if (!view.IsValid() || !view.IsOwner() || ReservedByOther(peer) || EventActive)
            { user.Message(MessageHud.MessageType.Center, "The shrine is busy. Try again."); yield break; }
            if (user == null || Vector3.Distance(user.transform.position, transform.position) > 5f || CountTribute(user) < TributeAmount) yield break;

            VFChallengeDefinition definition = API.GetShrineChallengeDefinition(ShrineName, TributePrefab);
            if (definition == null)
            { user.Message(MessageHud.MessageType.Center, "Resin challenge unavailable. Requires Fortress 0.39 and its shrine configuration."); yield break; }
            Vector3[] points = GetSpawnPoints();
            if (points.Length == 0)
            { user.Message(MessageHud.MessageType.Center, "The shrine has no clear spawn points."); yield break; }

            HashSet<ZDOID> existing = new HashSet<ZDOID>(FindRunners().Select(runner => runner.GetZDO().m_uid));
            Transform rewardMarker = transform.Find("spawnpoint");
            Vector3 rewardPosition = rewardMarker != null ? rewardMarker.position : transform.position + Vector3.up;
            if (!API.RunChallenge(definition, points, rewardPosition))
            { user.Message(MessageHud.MessageType.Center, "The challenge could not start. Your resin was not consumed."); yield break; }

            // The API returns only bool. Its synchronous call creates one registered
            // network runner; retain that runner's ID for the offering lock.
            ZNetView runnerView = FindRunners().FirstOrDefault(runner => !existing.Contains(runner.GetZDO().m_uid) && Vector3.Distance(runner.transform.position, rewardPosition) < 0.1f);
            if (runnerView == null) throw new InvalidOperationException("Fortress started a challenge but its network runner was not found.");
            view.GetZDO().Set(RunnerKey, runnerView.GetZDO().m_uid);
            view.GetZDO().Set(StartedKey, Now);
            int remaining = TributeAmount;
            foreach (ItemDrop.ItemData stack in user.GetInventory().GetAllItems().Where(item => item.m_dropPrefab != null && item.m_dropPrefab.name == TributePrefab).ToArray())
            {
                int amount = Math.Min(remaining, stack.m_stack);
                user.GetInventory().RemoveItem(stack, amount);
                remaining -= amount;
                if (remaining == 0) break;
            }
            Debug.Log($"[MWL ShrineEvent] Started {ShrineName}/{TributePrefab}; consumed={TributeAmount}; runner={runnerView.GetZDO().m_uid}; points={string.Join(", ", points.Select(point => point.ToString("F2")))}");
        }
        finally
        {
            if (view != null && view.IsValid() && view.IsOwner() && view.GetZDO().GetLong(ReservationKey) == peer)
            { view.GetZDO().Set(ReservationKey, 0L); view.GetZDO().Set(ReservationUntilKey, 0L); }
            pending = false;
        }
    }

    private static IEnumerable<ZNetView> FindRunners() => UnityEngine.Object.FindObjectsByType<ZNetView>(FindObjectsSortMode.None)
        .Where(candidate => candidate.IsValid() && global::Utils.GetPrefabName(candidate.gameObject) == RunnerPrefab);

    private Vector3[] GetSpawnPoints()
    {
        if (SpawnRoot == null) return Array.Empty<Vector3>();
        int mask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain");
        List<Vector3> points = new List<Vector3>();
        foreach (Transform marker in SpawnRoot)
        {
            if (!Physics.Raycast(marker.position + Vector3.up * 3f, Vector3.down, out RaycastHit floor, 6f, mask, QueryTriggerInteraction.Ignore)) continue;
            if (floor.normal.y < 0.7f || Math.Abs(floor.point.y - transform.position.y) > 4f) continue;
            if (Physics.CheckCapsule(floor.point + Vector3.up * 1.1f, floor.point + Vector3.up * 3f, 1f, mask, QueryTriggerInteraction.Ignore)) continue;
            points.Add(floor.point + Vector3.up * 0.2f);
        }
        return points.ToArray();
    }
}
