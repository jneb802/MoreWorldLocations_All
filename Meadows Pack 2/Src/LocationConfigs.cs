using System.Collections.Generic;
using Jotunn.Configs;

namespace Meadows_Pack_2;

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
        { "MWL_DeerShrine1_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_DeerShrine1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_shrine", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },
        
        { "MWL_DeerShrine2_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_DeerShrine2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_shrine", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },
        
        { "MWL_MeadowsBarn1_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_MeadowsBarn1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Median, Group = "Wood_small", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },
        
        { "MWL_MeadowsHouse2_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_MeadowsHouse2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Wood_small", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring5.MinDistance, InForest = true } },
        
        { "MWL_MeadowsRuin1_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_MeadowsRuin1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_small", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },
        
        { "MWL_MeadowsTomb1_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_MeadowsTomb1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Environment_small", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance, InForest = true, ClearArea = false} },
        
        { "MWL_MeadowsTomb4_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_MeadowsTomb4_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Environment_medium", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 0, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },
        
        { "MWL_MeadowsTower1_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_MeadowsTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_small", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance, InForest = false } },
        
        { "MWL_OakHut1_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_OakHut1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Wood_small", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_SmallHouse1_Config", new LocationConfig { Quantity = Meadows_Pack_2Plugin.MWL_SmallHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_medium", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } }
    };
}