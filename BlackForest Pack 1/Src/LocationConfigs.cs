using Jotunn.Configs;

namespace BlackForest_Pack_1;

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
    
    public static LocationConfig MWL_RuinsArena2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_RuinsArena2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_large",
        MinDistanceFromSimilar = 1024,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring4.MinDistance,
        MaxDistance = LocationRings.Ring4.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_RuinsCastle1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_RuinsCastle1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 12.5f,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_medium",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_RuinsCastle3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_RuinsCastle3_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_large",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 2f,
        MinAltitude = 0,
        MaxAltitude = 2,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        //InteriorRadius = 32,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_RuinsTower3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_RuinsTower3_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_medium",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_RuinsTower8_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_RuinsTower8_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = true,
        Group = "Ruins_medium",
        MinDistanceFromSimilar = 512,
        //MaxTerrainDelta = 2f,
        MinAltitude = -2,
        MaxAltitude = 0.5f,
        //SnapToWater = true,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_Tavern1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_RuinsTavern1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 12,
        ClearArea = true,
        RandomRotation = false,
        Group = "Wood_small",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_WoodTower1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_WoodTower1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Wood_small",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_WoodTower2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_WoodTower2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Wood_small",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_WoodTower3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_1Plugin.MWL_WoodTower3_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 24,
        ClearArea = true,
        RandomRotation = false,
        Group = "Wood_medium",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
}