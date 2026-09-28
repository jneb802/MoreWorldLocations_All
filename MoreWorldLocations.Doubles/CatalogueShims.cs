// Stand-ins for the mod's own registration surface, so the real LocationDB and
// the real sweep can be compiled and exercised.
//
// The one thing they model faithfully is the timing that R1 turned on:
// Register() makes a location available immediately, which is the BEST case. If
// the audit still cannot inspect an unregistered template under ideal timing,
// no amount of real Jotunn latency would save it.

using System.Collections.Generic;

namespace Jotunn.Configs
{
    public class LocationConfig
    {
        public Heightmap.Biome Biome;
        public int Quantity;
    }
}

namespace Jotunn.Managers
{
    public static class ZoneManager
    {
        public static event System.Action? OnVanillaLocationsAvailable;

        internal static void Unused() => OnVanillaLocationsAvailable?.Invoke();
    }
}

namespace More_World_Locations_AIO
{
    /// <summary>A toggle that reads like the mod's config entries.</summary>
    public class Setting<T>
    {
        public T Value = default!;
    }

    public static class PortInit
    {
        public enum Toggle
        {
            Off,
            On,
        }

        public static Setting<Toggle> EnablePortLocations = new();
    }

    public static class BepinexConfigs
    {
        public static Setting<PortInit.Toggle> EnableTraders = new();
        public static Setting<PortInit.Toggle> EnableTrainers = new();
    }

    /// <summary>
    /// The mod's location record, and the catalogue the tests hand it.
    ///
    /// <c>Register</c> puts the location straight into the world's list, which is
    /// the earliest it could possibly become available. Anything the audit cannot
    /// do under that timing it could not do under the real one either.
    /// </summary>
    public class MWLLocation
    {
        public string Name { get; set; } = "";
        public string AssetPath { get; set; } = "";
        public string DungeonTheme { get; set; } = "";
        public string InteriorPrefabName { get; set; } = "";
        public Jotunn.Configs.LocationConfig Config { get; set; } = new();

        public void Register()
        {
            var location = new ZoneSystem.ZoneLocation();
            location.m_prefab.Name = Name;
            ZoneSystem.instance!.m_locations.Add(location);
            ZoneSystem.instance.m_locationsByHash[Name.GetStableHashCode()] = location;
        }
    }

    /// <summary>
    /// The packs, settable by a test so a fixture can describe its own
    /// catalogue. Defaults to the four shipped names plus one unapproved build,
    /// which is the shape R1 turns on.
    /// </summary>
    public static class LocationDefinitions
    {
        public static MWLLocation[] Meadows = Pack(
            "MWL_MeadowsTomb4", "MWL_WoodTower2", "MWL_RuinsWell1", "MWL_Ruins1", "Review_NewBuild");

        public static MWLLocation[] BlackForest = Pack();
        public static MWLLocation[] Swamp = Pack();
        public static MWLLocation[] Mountains = Pack();
        public static MWLLocation[] Plains = Pack();
        public static MWLLocation[] Mistlands = Pack();
        public static MWLLocation[] Ashlands = Pack();
        public static MWLLocation[] Ports = Pack();
        public static MWLLocation[] Traders = Pack();
        public static MWLLocation[] Trainers = Pack();
        public static MWLLocation[] Dungeons = Pack();

        public static MWLLocation[] Pack(params string[] names)
        {
            var pack = new List<MWLLocation>();
            foreach (string name in names)
                pack.Add(new MWLLocation { Name = name, AssetPath = name });
            return pack.ToArray();
        }
    }
}

namespace HarmonyLib
{
    /// <summary>
    /// The attributes the sweep's hooks carry. Declarations only: nothing here
    /// patches anything, and a test that calls a Postfix calls it directly. What
    /// a hook is attached to, and when Harmony runs it, is exactly the thing
    /// these doubles cannot establish -- it needs a running game.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true)]
    public class HarmonyPatch : System.Attribute
    {
        public HarmonyPatch(System.Type declaringType, string methodName) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method)]
    public class HarmonyAfter : System.Attribute
    {
        public HarmonyAfter(params string[] before) { }
    }
}
