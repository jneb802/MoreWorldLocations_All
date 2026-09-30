// MWL's own plugin surface, for the server-only sources compiled into the tests. BepInEx's logger itself comes from
// Valheim.Testing.Doubles (ManualLogSource, with its Captured lines and ThrowOnNextInfo).
#nullable enable

namespace More_World_Locations_AIO
{
    /// <summary>Shim for the plugin class: the logger and the constants the
    /// server-only sources read. Nothing here touches BepInEx itself.</summary>
    public static class More_World_Locations_AIOPlugin
    {
        internal const string ModName = "More_World_Locations_AIO";
        internal const string ModVersion = "5.0.9";

        public static BepInEx.Logging.ManualLogSource More_World_Locations_AIOLogger { get; } = new();
    }
}
