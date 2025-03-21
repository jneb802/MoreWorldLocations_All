using Jotunn.Configs;

namespace Mistlands_Pack_1;

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
    
    public static LocationConfig MWL_MistFort2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_MistFort2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist3",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 15f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_SecretRoom1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_SecretRoom1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist1",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 15f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_MistWorkshop1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_MistWorkshop1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist2",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 1f,
        MaxAltitude = 6f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_MistTower1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_MistTower1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist3",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 15f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_MistWall1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_MistWall1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist2",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 8f,
        MinAltitude = 1f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_MistTower2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_MistTower2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist3",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 15f,
        MinAltitude = -2f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_MistHut1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_MistHut1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist3",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 15f,
        MinAltitude = 5f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_DvergrEitrSingularity1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_DvergrEitrSingularity1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist5",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 15f,
        MinAltitude = 5f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Everything
    };
    
    public static LocationConfig MWL_DvergrHouse1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_DvergrHouse1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist4",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 15f,
        MinAltitude = 5f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Edge
    };
    
    public static LocationConfig MWL_DvergrKnowledgeExtractor1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Mistlands_Pack_1Plugin.MWL_DvergrKnowledgeExtractor1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mist5",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 15f,
        MinAltitude = 5f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median
    };
}