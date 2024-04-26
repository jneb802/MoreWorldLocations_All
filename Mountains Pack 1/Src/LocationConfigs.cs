using Jotunn.Configs;

namespace Mountains_Pack_1;

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
    
    public static LocationConfig MWL_StoneCastle1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_StoneCastle1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Stone_Large",
        MinDistanceFromSimilar = 1024,
        SlopeRotation = true,
        MinTerrainDelta = 7f,
        MaxTerrainDelta = 15f,
        MinAltitude = 80,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_StoneFort1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_StoneFort1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Stone_Small",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 70,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_StoneHall1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_StoneHall1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Stone_Medium",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 80,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_StoneTavern1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_StoneTavern1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 15,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Stone_Medium",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 18f,
        MinAltitude = 75,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_StoneTower1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_StoneTower1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 8,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Stone_Small",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        //MaxTerrainDelta = 5f,
        MinAltitude = 80,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_StoneTower2_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_StoneTower2_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 10,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Stone_Small",
        MinDistanceFromSimilar = 256,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 70,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_WoodBarn1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_WoodBarn1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Wood_1",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 3f,
        MinAltitude = 80,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_WoodFarm1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_WoodFarm1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 18,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Wood_1",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 7f,
        MinAltitude = 70,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
    
    public static LocationConfig MWL_WoodHouse1_Config = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = Mountains_Pack_1Plugin.MWL_WoodHouse1_QuantityConfig.Value,
        Priotized = true,
        ExteriorRadius = 10,
        ClearArea = true,
        RandomRotation = false,
        Group = "Mountain_Wood_2",
        MinDistanceFromSimilar = 512,
        //SlopeRotation = true,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 10f,
        MinAltitude = 90f,
        MinDistance = LocationRings.Ring2.MinDistance,
        //MaxDistance = LocationRings.Ring7.MaxDistance,
        //InteriorRadius = 64,
        InForest = false,
        //ForestTresholdMin = 0f,
        //ForestTrasholdMax = 2,
    };
}
