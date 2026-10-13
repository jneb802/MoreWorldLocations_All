using System;
using System.Collections.Generic;

// Only game/framework boundaries are replaced. Location definitions, filtering,
// registration flow and upgrade-command generation are linked production source.
namespace BepInEx.Bootstrap
{
    public static class Chainloader
    {
        public static readonly Dictionary<string, object> PluginInfos = new Dictionary<string, object>();
    }
}
public class Heightmap
{
    public enum Biome { Meadows, BlackForest, Swamp, Mountain, Plains, Mistlands, AshLands, Ocean }
    public enum BiomeArea { Everything, Edge, Median }
}
namespace Jotunn.Configs
{
    public class LocationConfig
    {
        public Heightmap.Biome Biome { get; set; }
        public Heightmap.BiomeArea BiomeArea { get; set; }
        public string Group { get; set; }
        public int Quantity { get; set; }
        public bool ClearArea { get; set; }
        public bool IconPlaced { get; set; }
        public bool InForest { get; set; }
        public bool Priotized { get; set; }
        public bool RandomRotation { get; set; }
        public bool SlopeRotation { get; set; }
        public bool Unique { get; set; }
        public float ExteriorRadius { get; set; }
        public float ForestTrasholdMax { get; set; }
        public float ForestTresholdMin { get; set; }
        public float MaxAltitude { get; set; }
        public float MaxDistance { get; set; }
        public float MaxTerrainDelta { get; set; }
        public float MinAltitude { get; set; }
        public float MinDistance { get; set; }
        public float MinDistanceFromSimilar { get; set; }
        public float MinTerrainDelta { get; set; }
    }
}
namespace Jotunn.Managers
{
    public static class ZoneManager { public static Action OnVanillaLocationsAvailable; }
}
namespace Common
{
    public static class LocationManager
    {
        public static readonly List<string> Registered = new List<string>();
        public static void AddLocation(string name, Jotunn.Configs.LocationConfig config, string theme = null, string interior = null) => Registered.Add(name);
    }
}
namespace More_World_Locations_AIO
{
    public static class More_World_Locations_AIOPlugin { public const string ValheimFortressGUID = "MidnightsFX.ValheimFortress"; }
    public class ToggleConfig { public PortInit.Toggle Value = PortInit.Toggle.On; }
    public static class PortInit
    {
        public enum Toggle { Off, On }
        public static readonly ToggleConfig EnablePortLocations = new ToggleConfig();
    }
    public static class BepinexConfigs
    {
        public static readonly ToggleConfig EnableTraders = new ToggleConfig();
        public static readonly ToggleConfig EnableTrainers = new ToggleConfig();
    }
    public static class LocationQuantityManager { public static int GetQuantity(string name) => 15; }
    public static class ProtectedLocationPeacefulArea
    {
        public static readonly HashSet<string> Patched = new HashSet<string>();
        public static void RegisterPrefabPatch(string name, Jotunn.Configs.LocationConfig config) => Patched.Add(name);
    }
}
namespace UpgradeWorld
{
    public static class Upgrade
    {
        public static readonly Dictionary<string, string> Commands = new Dictionary<string, string>();
        public static void Register(string name, string description, string command) => Commands.Add(name, command);
    }
}
