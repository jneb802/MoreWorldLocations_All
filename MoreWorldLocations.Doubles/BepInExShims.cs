// Minimal logging stand-ins so mod sources compile without BepInEx.

namespace BepInEx.Logging
{
    public class ManualLogSource
    {
        /// <summary>
        /// When non-null, every log line is also appended here — tests use
        /// this to observe mod behavior (e.g. which roads were generated).
        /// </summary>
        public static System.Collections.Generic.List<string>? Captured;

        /// <summary>
        /// Make the next info line throw, once.
        ///
        /// It exists for one fixture: reporting must not be able to decide
        /// whether a rejected location stays registered. A sink that fails is
        /// the cheapest way to prove the two are not the same step.
        /// </summary>
        public static bool ThrowOnNextInfo;

        public void LogDebug(object data) => Write("DEBUG", data);
        public void LogInfo(object data)
        {
            if (ThrowOnNextInfo)
            {
                ThrowOnNextInfo = false;
                throw new System.InvalidOperationException("injected report sink failure");
            }
            Write("INFO ", data);
        }
        public void LogWarning(object data) => Write("WARN ", data);
        public void LogError(object data) => Write("ERROR", data);

        private static void Write(string level, object data)
        {
            string line = data?.ToString() ?? "";
            Captured?.Add(line);
            System.Console.WriteLine($"[{level}] {line}");
        }
    }
}

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
