using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Instrumentation, not gameplay: ask the production height read for N
/// distinct zones, through the builder, so a station run can watch the
/// budget reserve, evict, defer and recover without a person walking through
/// two thousand zones.
///
/// <para>Everything it calls is the path a site's conversion takes:
/// <see cref="LocationTerrainBridge.ReadGeneratedHeightAt"/> with no live
/// heightmap, which asks the builder once and keeps the answer in the budgeted
/// cache. The first <c>hold</c> zones' holds are kept until the end, so that
/// with a small budget the later zones find everything in use and are
/// deferred; releasing the holds lets the next read succeed. Results go to the
/// log, one line per zone, and a summary at the end.</para>
/// </summary>
public static class HeightBudgetDriver
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    private const string Tag = "[HEIGHTS]";

    public static string Run(int zones, int hold)
    {
        if (zones <= 0)
            return "mwl_heights <zones> [hold]";
        MonoBehaviour? host = ZoneSystem.instance != null ? ZoneSystem.instance : (MonoBehaviour?)ZNet.instance;
        if (host == null)
            return "no world to host the driver on";
        host.StartCoroutine(Drive(zones, Math.Max(0, Math.Min(hold, zones))));
        return $"{Tag} driver started over {zones} zone(s), holding the first {hold}; results go to the BepInEx log";
    }

    private static IEnumerator Drive(int zones, int hold)
    {
        var held = new List<IDisposable>();
        int read = 0, deferred = 0, notReady = 0;
        long peakBytes = 0;
        // Zones along one row far from the origin, so they are nobody's.
        for (int i = 0; i < zones; i++)
        {
            var zoneId = new Vector2s(60 + i, 60);
            Vector3 origin = ZoneSystem.GetZonePos(zoneId);
            var zone = new TerrainZoneDeltas(origin, TerrainZoneDeltas.ZoneWidth, TerrainZoneDeltas.ZoneScale);

            LocationTerrainBridge.HeightOutcome outcome = LocationTerrainBridge.HeightOutcome.NotReady;
            string reason = "";
            IDisposable? holdHandle = null;
            TerrainConversion.VertexHeight? height = null;
            int frames = 0;
            // The builder builds on its own thread; NotReady means ask again.
            while (frames < 600)
            {
                outcome = LocationTerrainBridge.ReadGeneratedHeightAt(zone, null, out height, out reason, out holdHandle);
                if (outcome != LocationTerrainBridge.HeightOutcome.NotReady)
                    break;
                frames++;
                yield return null;
            }

            float sample = height != null ? height(32, 32) : float.NaN;
            string line = $"{Tag} zone {zoneId.x},{zoneId.y}: {outcome} after {frames} frame(s)" +
                          (outcome == LocationTerrainBridge.HeightOutcome.Read ? $", height at centre {sample:0.00}" : "") +
                          (reason.Length > 0 ? $" — {reason}" : "") +
                          $" | cache {LocationTerrainBridge.GeneratedHeightBytes} B in {LocationTerrainBridge.GeneratedHeightEntries} zone(s), " +
                          $"{LocationTerrainBridge.GeneratedHeightPins} in use, {LocationTerrainBridge.GeneratedHeightAdopted} adopted, " +
                          $"budget {LocationTerrainBridge.GeneratedHeightBudgetBytes} B";
            Log.LogInfo(line);
            peakBytes = Math.Max(peakBytes, LocationTerrainBridge.GeneratedHeightBytes);

            switch (outcome)
            {
                case LocationTerrainBridge.HeightOutcome.Read:
                    read++;
                    if (held.Count < hold && holdHandle != null)
                        held.Add(holdHandle);
                    else
                        holdHandle?.Dispose();
                    break;
                case LocationTerrainBridge.HeightOutcome.Deferred:
                    deferred++;
                    break;
                default:
                    notReady++;
                    break;
            }
            yield return null;
        }

        // Release what was held and read once more: a deferral is a wait, not a loss.
        foreach (IDisposable h in held)
            h.Dispose();
        held.Clear();
        string recovery = "";
        if (deferred > 0)
        {
            var zoneId = new Vector2s(60 + zones, 60);
            var zone = new TerrainZoneDeltas(ZoneSystem.GetZonePos(zoneId), TerrainZoneDeltas.ZoneWidth, TerrainZoneDeltas.ZoneScale);
            LocationTerrainBridge.HeightOutcome outcome = LocationTerrainBridge.HeightOutcome.NotReady;
            IDisposable? holdHandle = null;
            int frames = 0;
            while (frames < 600)
            {
                outcome = LocationTerrainBridge.ReadGeneratedHeightAt(zone, null, out _, out _, out holdHandle);
                if (outcome != LocationTerrainBridge.HeightOutcome.NotReady)
                    break;
                frames++;
                yield return null;
            }
            holdHandle?.Dispose();
            recovery = $"; after releasing the holds, zone {zoneId.x},{zoneId.y}: {outcome}";
        }

        Log.LogInfo(
            $"{Tag} done: {read} read, {deferred} deferred, {notReady} never ready; peak cache {peakBytes} B of " +
            $"{LocationTerrainBridge.GeneratedHeightBudgetBytes} B; now {LocationTerrainBridge.GeneratedHeightBytes} B in " +
            $"{LocationTerrainBridge.GeneratedHeightEntries} zone(s), {LocationTerrainBridge.GeneratedHeightPins} in use, " +
            $"{LocationTerrainBridge.GeneratedHeightAdopted} adopted{recovery}");
    }
}
