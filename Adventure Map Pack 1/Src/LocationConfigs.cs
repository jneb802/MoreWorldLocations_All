using Adventure_Map_Pack_1;
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
    
    public static LocationConfig MWL_CastleCorner1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Adventure_Map_Pack_1Plugin.MWL_CastleCorner1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Swamp_Ruins",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 3f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_ForestCamp1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = Adventure_Map_Pack_1Plugin.MWL_ForestCamp1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Camp",
        MinDistanceFromSimilar = 1024,
        MaxTerrainDelta = 3f,
        MinAltitude = 2,
        MinDistance = LocationRings.Ring1.MinDistance,
        MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        // InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_MistHut2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = Adventure_Map_Pack_1Plugin.MWL_Misthut2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Camp",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 5f,
        MinAltitude = 5,
        MinDistance = LocationRings.Ring1.MinDistance,
        MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        // InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_MountainDvergrShrine1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Adventure_Map_Pack_1Plugin.MWL_MountainDvergrShrine1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Shrine",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 4f,
        MinAltitude = 2,
        MinDistance = LocationRings.Ring5.MinDistance,
        MaxDistance = LocationRings.Ring7.MaxDistance,
        BiomeArea = Heightmap.BiomeArea.Median
        //InteriorRadius = 64,
        // InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    // public static LocationConfig MWL_MountainDvergrShrine2_Config = new LocationConfig
    // {
    //     Biome = Heightmap.Biome.Mountain,
    //     Quantity = Adventure_Map_Pack_1Plugin.MWL_MountainDvergrShrine2_Quantity_Config.Value,
    //     Priotized = true,
    //     ExteriorRadius = 20,
    //     ClearArea = true,
    //     RandomRotation = false,
    //     Group = "Shrine",
    //     MinDistanceFromSimilar = 512,
    //     MaxTerrainDelta = 4f,
    //     MinAltitude = 2,
    //     MinDistance = LocationRings.Ring5.MinDistance,
    //     MaxDistance = LocationRings.Ring7.MaxDistance,
    //     BiomeArea = Heightmap.BiomeArea.Median
    //     //InteriorRadius = 64,
    //     // InForest = false,
    //     //ForestTresholdMin = 0f,
    //     //ForestTrasholdMax = 2,
    // };
    
    public static LocationConfig MWL_MountainShrine1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Adventure_Map_Pack_1Plugin.MWL_MountainShrine1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Shrine",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 4f,
        MinAltitude = 2,
        MinDistance = LocationRings.Ring4.MinDistance,
        MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        // InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_RuinedTower1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = Adventure_Map_Pack_1Plugin.MWL_RuinedTower1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "RuinedTower1",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 4f,
        MinAltitude = 2,
        MinDistance = LocationRings.Ring1.MinDistance,
        MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        // InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_TreeTowers1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Swamp,
        Quantity = Adventure_Map_Pack_1Plugin.MWL_TreeTowers1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "TreeTowers1",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 3f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring1.MinDistance,
        MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        // InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
}