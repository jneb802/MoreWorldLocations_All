using HarmonyLib;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Hold the server's world load — and with it location generation and the
/// opening of the server — while the catalogue sweep is still judging, and
/// let it go when the sweep concludes.
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
    private static bool s_loadDeferred;

    /// <summary>Wire the sweep to the hold. Called once, at startup, in server-only mode.</summary>
    public static void Install()
    {
        CatalogueSweep.ReleaseGeneration = Release;
        CatalogueSweep.NewWorld = Forget;
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
    public static bool LoadDeferred => s_loadDeferred;

    /// <summary>Whether the game's generation pass was turned away and is owed a call.</summary>
    public static bool Held => s_generationHeld;

    internal static void Forget()
    {
        s_generationHeld = false;
        s_loadDeferred = false;
    }

    private static void Release()
    {
        if (s_loadDeferred)
        {
            s_loadDeferred = false;
            s_generationHeld = false;
            if (ZNet.instance == null)
                return;
            Log.LogInfo("The catalogue sweep has concluded; loading the world now.");
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
        Log.LogInfo("The catalogue sweep has concluded; generating the world's locations now.");
        ZoneSystem.instance.GenerateLocationsIfNeeded();
    }

    [HarmonyPatch(typeof(ZNet), "ServerLoadWorld")]
    private static class LoadPatch
    {
        private static bool Prefix()
        {
            if (!CatalogueSweep.HoldsGeneration)
                return true;
            if (!s_loadDeferred)
            {
                s_loadDeferred = true;
                Log.LogInfo(
                    "The world load waits for the catalogue sweep to conclude " +
                    $"({CatalogueSweep.Progress} judged): saved locations are resolved by name at load, and ours are " +
                    "registered when the sweep concludes. No player can join and nothing is placed until then.");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Save))]
    private static class SavePatch
    {
        private static bool Prefix()
        {
            if (!s_loadDeferred)
                return true;
            Log.LogWarning("A save was refused: the world has not been loaded yet (the catalogue sweep is still judging).");
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
            if (!CatalogueSweep.HoldsGeneration || s_loadDeferred)
                return true;
            if (!s_generationHeld)
            {
                s_generationHeld = true;
                Log.LogInfo(
                    "Location generation waits for the catalogue sweep to conclude " +
                    $"({CatalogueSweep.Progress} judged); nothing unverified can be placed.");
            }
            return false;
        }
    }
}
