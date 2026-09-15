using HarmonyLib;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Hold the world's location generation while the catalogue sweep is still
/// judging, and let it go when the sweep concludes.
///
/// <para>The game generates locations once, from <c>ZNet.ServerLoadWorld</c>,
/// in the same startup as the registration hook, and a location registered
/// after that pass is never placed. With the sweep spread over frames the
/// approved list is not known when that pass would run, so the pass waits.
/// Vanilla's own <c>ZoneSystem.Update</c> generates no zone until locations
/// are generated, so nothing else needs holding: the world stands still, ticks
/// its frames, and takes the sweep one name at a time.</para>
///
/// <para>This is a scheduling wait. No site exists yet, so no readiness or
/// attempt budget is touched by it.</para>
/// </summary>
public static class GenerationHold
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    private static bool s_held;

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

    /// <summary>Whether the game's generation pass was turned away and is owed a call.</summary>
    public static bool Held => s_held;

    internal static void Forget() => s_held = false;

    private static void Release()
    {
        if (!s_held)
            return;
        s_held = false;
        if (ZoneSystem.instance == null)
            return;
        Log.LogInfo("The catalogue sweep has concluded; generating the world's locations now.");
        ZoneSystem.instance.GenerateLocationsIfNeeded();
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocationsIfNeeded))]
    private static class Patch
    {
        private static bool Prefix()
        {
            if (!CatalogueSweep.HoldsGeneration)
                return true;
            if (!s_held)
            {
                s_held = true;
                Log.LogInfo(
                    "Location generation waits for the catalogue sweep to conclude " +
                    $"({CatalogueSweep.Progress} judged); nothing unverified can be placed.");
            }
            return false;
        }
    }
}
