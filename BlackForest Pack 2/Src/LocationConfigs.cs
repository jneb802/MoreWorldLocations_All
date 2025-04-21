using System.Collections.Generic;
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
    
    public static Dictionary<string, LocationConfig> AllLocationConfigs = new Dictionary<string, LocationConfig>
    {
        { "MWL_ForestForge1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestForge1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Forge_small", Priotized = true, RandomRotation = false, ExteriorRadius = 12, ClearArea = true, 
            MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },
        
        { "MWL_ForestForge2_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestForge2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Forge_small", Priotized = true, RandomRotation = false, ExteriorRadius = 16, ClearArea = true, 
            MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },
        
        { "MWL_ForestGreatHouse2_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "House_large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance } },
        
        { "MWL_ForestHouse2_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestHouse2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "House_small", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },
        
        { "MWL_ForestRuin1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestRuin1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_large", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 8, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },
        
        { "MWL_ForestTower2_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Tower_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance } },
        
        { "MWL_ForestTower3_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestTower3_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Tower_large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 10, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },
        
        { "MWL_MassGrave1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_MassGrave1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Grave_large", Priotized = true, RandomRotation = false, ExteriorRadius = 3, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance } },

        { "MWL_StoneFormation1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_StoneFormation1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Stone_small", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },
        
        { "MWL_GuardTower1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_GuardTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_large", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 6, MinDistance = LocationRings.Ring3.MinDistance } },
        
        { "MWL_RootRuins1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_RootRuins1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance } },
        
        { "MWL_RootsTower1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_RootsTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },
        
        { "MWL_RootsTower2_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_RootsTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },
        
        { "MWL_StoneOutlook1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_StoneOutlook1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Coastal", Priotized = true, RandomRotation = false, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinAltitude = -2, MaxAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, SlopeRotation = true } },

        { "MWL_ForestRuin2_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestRuin2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance } },
        
        { "MWL_ForestRuin3_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestRuin3_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance } },
        
        { "MWL_ForestSkull1_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestSkull1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance } },
        
        { "MWL_ForestTower4_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestTower5_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance } },
        
        { "MWL_ForestTower5_Config", new LocationConfig { Quantity = BlackForest_Pack_2Plugin.MWL_ForestTower5_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },
    };

}