using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace More_World_Locations_AIO.Logging;

[HarmonyPatch]
internal static class ShaderWarningLogFilter
{
    private const string ParticlesStandardUnlitWarning =
        "Failed to find expected binary shader data in 'Particles/Standard Unlit2'.";

    private const string CustomCreatureWarning =
        "Failed to find expected binary shader data in 'Custom/Creature'.";

    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(BepInEx.Logging.Logger), "InternalLogEvent");
    }

    private static bool Prefix(LogEventArgs eventArgs)
    {
        if ((eventArgs.Level & LogLevel.Warning) == 0)
        {
            return true;
        }

        string message = eventArgs.Data?.ToString() ?? string.Empty;
        return message != ParticlesStandardUnlitWarning && message != CustomCreatureWarning;
    }
}
