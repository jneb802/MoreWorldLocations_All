using Jotunn.Configs;

namespace Dungeon_Mountain;

public class LocationConfigs
{
    public struct LocationRing
    {
        public int MinDistance { get; set; }
        public int MaxDistance { get; set; }

        public LocationRing(int minDistance, int maxDistance)
        {
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }
    }
    
    public static class LocationRings
    {
        public static LocationRing Ring1 { get; set; } = new LocationRing(0, 500);
        public static LocationRing Ring2 { get; set; } = new LocationRing(500, 2000);
        public static LocationRing Ring3 { get; set; } = new LocationRing(1500, 3000);
        public static LocationRing Ring4 { get; set; } = new LocationRing(2500, 4000);
        public static LocationRing Ring5 { get; set; } = new LocationRing(3500, 6000);
        public static LocationRing Ring6 { get; set; } = new LocationRing(4500, 8500);
        public static LocationRing Ring7 { get; set; } = new LocationRing(5000, 10500);
    }
    
    public static LocationConfig MWD_MountainDungeon_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        BiomeArea = Heightmap.BiomeArea.Median,
        Quantity = Dungeon_MountainPlugin.MWD_MountainDungeon_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Dungeon_MountainDvergrDungeon",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 4f,
        MinAltitude = 10,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring6.MaxDistance,
    };
}