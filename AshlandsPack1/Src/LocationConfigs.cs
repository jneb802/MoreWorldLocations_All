using Jotunn.Configs;

namespace AshlandsPack1;

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
    
    public static LocationConfig MWL_AshlandsFort1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.AshLands,
        Quantity = AshlandsPack1Plugin.MWL_AshlandsFort1_Configuration.Quantity.Value,
        Priotized = true,
        // ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ashlands_Fort",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 5f,
        MinAltitude = 1,
        //MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_AshlandsFort2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.AshLands,
        Quantity = AshlandsPack1Plugin.MWL_AshlandsFort2_Configuration.Quantity.Value,
        Priotized = true,
        // ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ashlands_Fort",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 5f,
        MinAltitude = 1,
        //MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_AshlandsFort3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.AshLands,
        Quantity = AshlandsPack1Plugin.MWL_AshlandsFort3_Configuration.Quantity.Value,
        Priotized = true,
        // ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ashlands_Fort",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 5f,
        MinAltitude = 1,
        //MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
}