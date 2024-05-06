using Jotunn.Configs;

namespace Swamp_Pack_1;

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
    
    public static LocationConfig MWL_GuckPit1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_GuckPit1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_small",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 5f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampAltar1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampAltar1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_altar",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = -1f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampAltar2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampAltar2_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_altar",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = -1f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampAltar3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampAltar3_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_altar",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = -1f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampAltar4_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampAltar4_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_altar",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = -1f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    /*public static LocationConfig MWL_SwampCastle1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampCastle1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_large",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        //MinAltitude = 80,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };*/
    
    public static LocationConfig MWL_SwampCastle2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampCastle2_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_medium",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    /*public static LocationConfig MWL_SwampChurch1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampChurch1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_large",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };*/
    
    public static LocationConfig MWL_SwampGrave1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampGrave1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_medium",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampHouse1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampHouse1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_small",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        //MinAltitude = 80,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampRuin1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampRuin1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_medium",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = -1f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampTower1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampTower1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_small",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampTower2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampTower2_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_tower",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampTower3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampTower3_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_large",
        MinDistanceFromSimilar = 1024,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
    
    public static LocationConfig MWL_SwampWell1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Swamp_Pack_1Plugin.MWL_SwampWell1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_small",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
}
