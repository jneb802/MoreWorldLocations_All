using System;
using HarmonyLib;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Hold the server's world load — and with it location generation and the
/// opening of the server — until catalogue registration succeeds. Saving stays
/// blocked until the initial load returns without an exception or load error.
///
/// <para><b>Why the whole load and not only generation.</b> The first version
/// held only <c>GenerateLocationsIfNeeded</c>, and a station reload showed
/// what that misses: <c>ZoneSystem.Load</c> resolves every saved location
/// instance by name and DROPS the ones it cannot find, and our locations are
/// registered only when the sweep concludes — minutes after
/// <c>ZNet.ServerLoadWorld</c> has run. An existing world came back with its
/// MWL instances gone and the reseed pass reporting none of our sites. So the
/// load itself waits: <c>ZNet.Start</c> calls <c>ServerLoadWorld</c>, which is
/// turned away and called back once registration has happened. Inside it the
/// game loads the world, generates locations if the world never had them, and
/// subscribes the server's opening to that — so no player can join, no zone
/// generates and nothing is placed before the approved list is complete. A
/// save is refused while the load is deferred: there is nothing to save yet,
/// and writing an empty world over a real one is the worst outcome
/// available.</para>
///
/// <para>This is a scheduling wait. No site exists yet, so no readiness or
/// attempt budget is touched by it.</para>
/// </summary>
public static class GenerationHold
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    private static bool s_generationHeld;
    // One gate for the process. Only tests replace it to model a new process;
    // scene callbacks must call Reset on the existing instance.
    internal static WorldLoadGate LoadGate { get; set; } = new WorldLoadGate();

    public static bool RestartRequired =>
        ServerOnlyMode.Enabled && LoadGate.State == WorldLoadGate.LoadState.Failed;

    public static string LoadStatus =>
        $"world load {LoadGate.State}" + (LoadGate.Failure.Length == 0 ? "" : $": {LoadGate.Failure}; restart required");

    internal static bool RefuseFailedProcess()
    {
        if (!RestartRequired) return false;
        SayError($"{LoadStatus}. This process cannot load, generate or save another world. Restart after fixing the cause.");
        return true;
    }

    /// <summary>Wire the sweep to the hold. Called once, at startup, in server-only mode.</summary>
    public static void Install()
    {
        CatalogueSweep.ReleaseGeneration = Release;
        CatalogueSweep.NewWorld = Forget;
        CatalogueSweep.WorldLoadStatus = () => LoadStatus;
        CatalogueSweep.ScheduleRoutine = routine =>
        {
            MonoBehaviour? host = ZoneSystem.instance != null ? ZoneSystem.instance : (MonoBehaviour?)ZNet.instance;
            if (host == null)
                return false;
            host.StartCoroutine(routine);
            return true;
        };
    }

    /// <summary>Whether the game's world load was turned away and is owed a call.</summary>
    public static bool LoadDeferred => LoadGate.Deferred;

    /// <summary>Whether the game's generation pass was turned away and is owed a call.</summary>
    public static bool Held => s_generationHeld;

    internal static void Forget()
    {
        s_generationHeld = false;
        LoadGate.Reset();
        RefuseFailedProcess(); // a menu bounce must remain a loud, terminal stop
    }

    private static void Release()
    {
        if (RefuseFailedProcess() || !CatalogueSweep.RegistrationReady)
            return;
        if (LoadGate.Deferred)
        {
            if (LoadGate.State != WorldLoadGate.LoadState.Waiting || ZNet.instance == null)
                return;
            s_generationHeld = false;
            Say("The catalogue is registered; loading the world now.");
            // Private, so through Harmony's accessor. It loads, generates if
            // needed, and subscribes the server's opening to generation.
            Traverse.Create(ZNet.instance).Method("ServerLoadWorld").GetValue();
            return;
        }
        if (!s_generationHeld)
            return;
        s_generationHeld = false;
        if (ZoneSystem.instance == null)
            return;
        Say("The catalogue is registered; generating the world's locations now.");
        ZoneSystem.instance.GenerateLocationsIfNeeded();
    }

    [HarmonyPatch(typeof(ZNet), "ServerLoadWorld")]
    private static class LoadPatch
    {
        private static bool Prefix(out int __state)
        {
            __state = -1;
            if (!ServerOnlyMode.Enabled)
                return true;
            if (RefuseFailedProcess())
                return false;
            bool requested = LoadGate.Requested;
            if (LoadGate.TryBegin(CatalogueSweep.RegistrationReady, out __state))
                return StartupFaults.Current.BeforeLoad(() => ZNet.m_loadError = true, SayWarning);
            if (!requested)
            {
                // An empty Progress is not a cosmetic gap: it means no sweep is
                // running, and at this point in startup that means none ever
                // started — MWL threw before it subscribed RegisterAll, and this
                // hold will never lift on its own. Say which of the two it is.
                string progress = CatalogueSweep.Progress.Length > 0
                    ? $"{CatalogueSweep.Progress} judged"
                    : "no catalogue sweep has started: look for an earlier startup error";
                Say(
                    "The world load waits for successful catalogue registration " +
                    $"({progress}): saved locations are resolved by name at load, and ours are " +
                    "registered before loading. mwl_memory reports progress and any failure.");
            }
            return false;
        }

        private static Exception? Finalizer(int __state, Exception? __exception)
        {
            if (__state >= 0)
            {
                // A normal return can still mean a failed load: vanilla catches
                // file/decoder errors and sets this flag instead of throwing.
                LoadGate.Complete(__state, ZNet.m_loadError, __exception?.Message);
                if (LoadGate.State == WorldLoadGate.LoadState.Failed)
                    SayError($"World loading failed: {LoadGate.Failure}. Saving remains blocked; restart after fixing the cause.");
            }
            return __exception;
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Save))]
    private static class SavePatch
    {
        private static bool Prefix(ZNet __instance)
        {
            if (!ServerOnlyMode.Enabled || !__instance.IsServer() || LoadGate.CanSave)
                return true;

            // No need to touch Game.m_saveTimer here: Game.UpdateSaving calls
            // SavePlayerProfile immediately before ZNet.Save, and that zeroes
            // the timer, so refusing the save still leaves the game asking at
            // its own interval. Measured on the station at -saveinterval 5:
            // 26 refusals in 144 s, one per interval, not one per frame.
            string why = RestartRequired ? LoadStatus
                : CatalogueSweep.FailureReason.Length > 0 ? CatalogueSweep.FailureReason : LoadStatus;
            SayWarning($"A save was refused: this world's initial load has not succeeded ({why}).");
            return false;
        }
    }

    // A second guard, for a resweep or any path that reaches generation while
    // a sweep is judging: the pass waits and is called back.
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocationsIfNeeded))]
    private static class GeneratePatch
    {
        private static bool Prefix()
        {
            if (RefuseFailedProcess())
                return false;
            if (!CatalogueSweep.HoldsGeneration)
                return true;
            if (!s_generationHeld)
            {
                s_generationHeld = true;
                Say(
                    "Location generation waits for the catalogue sweep to conclude " +
                    $"({CatalogueSweep.Progress} judged); nothing unverified can be placed.");
            }
            return false;
        }
    }

    // Diagnostics must not open or interrupt the load/save gate, so every one
    // of these swallows its own failure. The severity is not decoration: a
    // refused save and a failed load are what an operator greps for, and
    // ReleaseGeneration is invoked through CatalogueSweep.Observe, which turns
    // an exception out of the world load into one ignorable warning. Without
    // these the only loud line for a failed load is that warning.
    private static void Say(string message)
    {
        try { Log.LogInfo(message); }
        catch { }
    }

    private static void SayWarning(string message)
    {
        try { Log.LogWarning(message); }
        catch { }
    }

    private static void SayError(string message)
    {
        try { Log.LogError(message); }
        catch { }
    }
}
