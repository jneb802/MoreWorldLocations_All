using Jotunn.Configs;

namespace Plains_Pack_1;

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
    
    public static LocationConfig MWL_GoblinFort1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_GoblinFort1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsFort",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_FulingRock1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_FulingRock1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsRock",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_FulingVillage1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_FulingVillage1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsVillage",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_FulingVillage2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_FulingVillage2_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsVillage",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_PlainsPillar1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_PlainsPillar1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsRock",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };

    public static LocationConfig MWL_FulingTemple1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_FulingTemple1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsTemple",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };

    public static LocationConfig MWL_FulingTemple2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_FulingTemple2_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsTemple",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
        
    public static LocationConfig MWL_FulingTemple3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_FulingTemple3_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsTemple",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_FulingWall1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_FulingWall1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsCamp",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_FulingTower1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = Plains_Pack_1Plugin.MWL_FulingTower1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "PlainsCamp",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
}