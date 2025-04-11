using System.Collections.Generic;
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
    
    public static Dictionary<string, LocationConfig> AllLocationConfigs = new Dictionary<string, LocationConfig>
    {
        { "MWL_StoneCastle1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_StoneCastle1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, 
            MinDistanceFromSimilar = 1024, SlopeRotation = true, MinTerrainDelta = 7f, MaxTerrainDelta = 15f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_StoneFort1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_StoneFort1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, 
            MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_StoneHall1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_StoneHall1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_StoneTavern1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_StoneTavern1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 18f, MinAltitude = 75, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_StoneTower1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_StoneTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_StoneTower2_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_StoneTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_WoodBarn1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_WoodBarn1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_1", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 3f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_WoodFarm1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_WoodFarm1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_1", Priotized = true, RandomRotation = false, ExteriorRadius = 18, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 7f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_WoodHouse1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_WoodHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_2", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 90f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        
        { "MWL_MountainAedicule1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_MountainAedicule1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, BiomeArea = Heightmap.BiomeArea.Median, Group = "Mountain_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        { "MWL_MountainPagota1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_MountainPagota1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, BiomeArea = Heightmap.BiomeArea.Median, Group = "Mountain_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 12, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 75, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        { "MWL_MountainTower1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_MountainTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, BiomeArea = Heightmap.BiomeArea.Median, Group = "Mountain_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 90, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        { "MWL_MountainTreasury1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_MountainTreasury1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, BiomeArea = Heightmap.BiomeArea.Median, Group = "Mountain_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 85, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        { "MWL_MountainHogan1_Config", new LocationConfig { Quantity = Mountains_Pack_1Plugin.MWL_MountainHogan1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, BiomeArea = Heightmap.BiomeArea.Median, Group = "Mountain_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

    };
    
}
