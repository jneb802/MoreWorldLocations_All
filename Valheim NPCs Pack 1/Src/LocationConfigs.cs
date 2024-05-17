using Jotunn.Configs;

namespace Valheim_NPCs_Pack_1;

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
        
        public static LocationConfig MWL_FenrisMaw_Config = new LocationConfig
        {
            Biome = Heightmap.Biome.Meadows,
            Quantity = Valheim_NPCs_Pack_1Plugin.MWL_FenrisMaw_QuantityConfig.Value,
            Priotized = true,
            ExteriorRadius = 8,
            ClearArea = true,
            RandomRotation = false,
            Group = "NPC_location",
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
            //BiomeArea = Heightmap.BiomeArea.Median
        };
        
        public static LocationConfig MWL_MarketplaceLarge1_Config = new LocationConfig
        {
            Biome = Heightmap.Biome.Meadows,
            Quantity = Valheim_NPCs_Pack_1Plugin.MWL_MarketplaceLarge1_QuantityConfig.Value,
            Priotized = true,
            ExteriorRadius = 8,
            ClearArea = true,
            RandomRotation = false,
            Group = "NPC_location",
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
            //BiomeArea = Heightmap.BiomeArea.Median
        };
        
        public static LocationConfig MWL_MarketSmall1_Config = new LocationConfig
        {
            Biome = Heightmap.Biome.Meadows,
            Quantity = Valheim_NPCs_Pack_1Plugin.MWL_MarketSmall1_QuantityConfig.Value,
            Priotized = true,
            ExteriorRadius = 8,
            ClearArea = true,
            RandomRotation = false,
            Group = "NPC_location",
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
            //BiomeArea = Heightmap.BiomeArea.Median
        };

        public static LocationConfig MWL_ShopSmall1_Config = new LocationConfig
        {
            Biome = Heightmap.Biome.Meadows,
            Quantity = Valheim_NPCs_Pack_1Plugin.MWL_ShopSmall1_QuantityConfig.Value,
            Priotized = true,
            ExteriorRadius = 8,
            ClearArea = true,
            RandomRotation = false,
            Group = "NPC_location",
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
            //BiomeArea = Heightmap.BiomeArea.Median
        };
            
        public static LocationConfig MWL_ThirstyRavenInn_Config = new LocationConfig
        {
            Biome = Heightmap.Biome.Meadows,
            Quantity = Valheim_NPCs_Pack_1Plugin.MWL_ThirstyRavenInn_QuantityConfig.Value,
            Priotized = true,
            ExteriorRadius = 8,
            ClearArea = true,
            RandomRotation = false,
            Group = "NPC_location",
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
            //BiomeArea = Heightmap.BiomeArea.Median
        };
    }
