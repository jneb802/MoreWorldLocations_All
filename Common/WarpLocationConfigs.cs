using Jotunn.Configs;

namespace Meadows_Pack_1;



public class WarpLocationConfigs
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
    
    public static LocationConfig MWL_Ruins1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_Ruins1_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_small",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring1.MinDistance,
        MaxDistance = LocationRings.Ring1.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_Ruins2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_Ruins2_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_medium",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 3f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };

    public static LocationConfig MWL_Ruins3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_Ruins3_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_small",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 3f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring1.MinDistance,
        MaxDistance = LocationRings.Ring1.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_Ruins6_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_Ruins6_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 14,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_small",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 3f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_Ruins7_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_Ruins7_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 7,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_medium",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 5f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring1.MinDistance,
        MaxDistance = LocationRings.Ring1.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_Ruins8_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_Ruins8_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 11,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_small",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 5f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_RuinsArena1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_RuinsArena1_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_medium",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 3f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        //InteriorRadius = 64,
        InForest = true,
        ForestTresholdMin = 1.2f,
        ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_RuinsArena3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_RuinsArena3_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_small",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 3f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = true,
        ForestTresholdMin = 0f,
        ForestTrasholdMax = 1,
    };
    
    public static LocationConfig MWL_RuinsChurch1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_RuinsChurch1_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_medium",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 3f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        //InteriorRadius = 64,
        InForest = true,
        ForestTresholdMin = 1.2f,
        ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_RuinsWell1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Meadows,
        Quantity = Meadows_Pack_1Plugin.MWL_RuinsWell1_Quantity_Config.Value,
        Priotized = true,
        ExteriorRadius = 5,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_small",
        MinDistanceFromSimilar = 256,
        MaxTerrainDelta = 2f,
        MinAltitude = 1,
        MinDistance = LocationRings.Ring1.MinDistance,
        MaxDistance = LocationRings.Ring1.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
}
