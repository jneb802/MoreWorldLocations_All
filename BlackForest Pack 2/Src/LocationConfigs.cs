using Jotunn.Configs;

namespace BlackForest_Pack_2;

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
    
    public static LocationConfig MWL_ForestForge1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_ForestForge1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 12,
        ClearArea = true,
        RandomRotation = false,
        Group = "Forge_small",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 3f,
        MinAltitude = 2,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring6.MaxDistance,
        // InForest = false,
    };

    public static LocationConfig MWL_ForestForge2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_ForestForge2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 16,
        ClearArea = true,
        RandomRotation = false,
        Group = "Forge_small",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 3f,
        MinAltitude = 2,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring6.MaxDistance,
        // InForest = false,
    };

    public static LocationConfig MWL_ForestGreatHouse2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 10,
        ClearArea = true,
        RandomRotation = false,
        Group = "House_large",
        MinDistanceFromSimilar = 512,
        MaxTerrainDelta = 4f,
        MinAltitude = 2,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        // InForest = false,
    };

    public static LocationConfig MWL_ForestHouse2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_ForestHouse2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 10,
        ClearArea = true,
        RandomRotation = false,
        Group = "House_small",
        MinDistanceFromSimilar = 1024,
        MaxTerrainDelta = 2f,
        MinAltitude = 10,
        MinDistance = LocationRings.Ring4.MinDistance,
        MaxDistance = LocationRings.Ring5.MaxDistance,
        // InForest = false,
    };

    public static LocationConfig MWL_ForestRuin1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_ForestRuin1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Ruins_large",
        MinDistanceFromSimilar = 1024,
        MaxTerrainDelta = 2f,
        MinAltitude = 8,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring5.MaxDistance,
        // InForest = false,
    };

    public static LocationConfig MWL_ForestTower2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_ForestTower2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 10,
        ClearArea = true,
        RandomRotation = false,
        Group = "Tower_medium",
        MinDistanceFromSimilar = 1024,
        MaxTerrainDelta = 2f,
        MinAltitude = 2,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        // InForest = false,
    };

    public static LocationConfig MWL_ForestTower3_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_ForestTower3_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 10,
        ClearArea = true,
        RandomRotation = false,
        Group = "Tower_large",
        MinDistanceFromSimilar = 1024,
        MaxTerrainDelta = 3f,
        MinAltitude = 10,
        MinDistance = LocationRings.Ring3.MinDistance,
        MaxDistance = LocationRings.Ring5.MaxDistance,
        // InForest = false,
    };

    public static LocationConfig MWL_MassGrave1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_MassGrave1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 3,
        ClearArea = true,
        RandomRotation = false,
        Group = "Grave_large",
        MinDistanceFromSimilar = 1024,
        MaxTerrainDelta = 2f,
        MinAltitude = 10,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring2.MaxDistance,
        // InForest = false,
    };

    public static LocationConfig MWL_StoneFormation1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = BlackForest_Pack_2Plugin.MWL_StoneFormation1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 5,
        ClearArea = true,
        RandomRotation = false,
        Group = "Stone_small",
        MinDistanceFromSimilar = 1024,
        MaxTerrainDelta = 2f,
        MinAltitude = 10,
        MinDistance = LocationRings.Ring2.MinDistance,
        MaxDistance = LocationRings.Ring3.MaxDistance,
        // InForest = false,
    };

}