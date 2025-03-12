using Jotunn.Configs;

namespace More_World_Traders;

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

    public static LocationConfig MWL_PlainsTavern1 = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = More_World_TradersPlugin.MWL_PlainsTavern1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 32,
        ClearArea = true,
        RandomRotation = false,
        Group = "MWL_Trader",
        MinDistanceFromSimilar = 1024,
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
        BiomeArea = Heightmap.BiomeArea.Median,
        Unique = true,
        IconPlaced = true
    };
    
    public static LocationConfig MWL_OceanTavern1 = new LocationConfig
    {
        Biome = Heightmap.Biome.Ocean,
        Quantity = More_World_TradersPlugin.MWL_OceanTavern1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 50,
        ClearArea = false,
        RandomRotation = false,
        Group = "MWL_Trader",
        MinDistanceFromSimilar = 1024,
        // SlopeRotation = true,
        // MinTerrainDelta = 0f,
        // MaxTerrainDelta = 5f,
        // MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        // MaxDistance = LocationRings.Ring7.MaxDistance,
        // InteriorRadius = 64,
        // InForest = false,
        // ForestTresholdMin = 0f,
        // ForestTrasholdMax = 2,
        BiomeArea = Heightmap.BiomeArea.Median,
        Unique = true,
        IconPlaced = true
    };
    
    public static LocationConfig MWL_PlainsCamp1 = new LocationConfig
    {
        Biome = Heightmap.Biome.Plains,
        Quantity = More_World_TradersPlugin.MWL_PlainsCamp1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "MWL_Trader",
        MinDistanceFromSimilar = 1024,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 5f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        BiomeArea = Heightmap.BiomeArea.Median,
        Unique = true,
        IconPlaced = true
    };
    
    public static LocationConfig MWL_BlackForestBlacksmith1 = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = More_World_TradersPlugin.MWL_BlackForestBlacksmith1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "MWL_Trader",
        MinDistanceFromSimilar = 1024,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 6f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        BiomeArea = Heightmap.BiomeArea.Median,
        Unique = true,
        IconPlaced = true
    };
    
    public static LocationConfig MWL_BlackForestBlacksmith2 = new LocationConfig
    {
        Biome = Heightmap.Biome.BlackForest,
        Quantity = More_World_TradersPlugin.MWL_BlackForestBlacksmith2_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "MWL_Trader",
        MinDistanceFromSimilar = 1024,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 6f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        BiomeArea = Heightmap.BiomeArea.Median,
        Unique = true,
        IconPlaced = true
    };
    
    public static LocationConfig MWL_MountainsBlacksmith1 = new LocationConfig
    {
        Biome = Heightmap.Biome.Mountain,
        Quantity = More_World_TradersPlugin.MWL_MountainsBlacksmith1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "MWL_Trader",
        MinDistanceFromSimilar = 1024,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 6f,
        MinAltitude = 0f,
        MinDistance = LocationRings.Ring2.MinDistance,
        BiomeArea = Heightmap.BiomeArea.Median,
        Unique = true,
        IconPlaced = true
    };
    
    public static LocationConfig MWL_MistlandsBlacksmith1 = new LocationConfig
    {
        Biome = Heightmap.Biome.Mistlands,
        Quantity = More_World_TradersPlugin.MWL_MistlandsBlacksmith1_Configuration.Quantity.Value,
        Priotized = true,
        ExteriorRadius = 20,
        ClearArea = true,
        RandomRotation = false,
        Group = "MWL_Trader",
        MinDistanceFromSimilar = 1024,
        MinTerrainDelta = 0f,
        MaxTerrainDelta = 12f,
        MinAltitude = 1f,
        InForest = false,
        BiomeArea = Heightmap.BiomeArea.Median,
        Unique = true,
        IconPlaced = true
    };
}