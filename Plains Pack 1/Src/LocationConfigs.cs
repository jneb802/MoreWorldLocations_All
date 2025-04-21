using System.Collections.Generic;
using Jotunn.Configs;

namespace Plains_Pack_1;

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
        { "MWL_GoblinFort1_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_GoblinFort1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsFort", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_FulingRock1_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_FulingRock1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        
        { "MWL_FulingVillage1_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_FulingVillage1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsVillage", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_FulingVillage2_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_FulingVillage2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsVillage", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_PlainsPillar1_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_PlainsPillar1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        
        { "MWL_FulingTemple1_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_FulingTemple1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        
        { "MWL_FulingTemple2_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_FulingTemple2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        
        { "MWL_FulingTemple3_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_FulingTemple3_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        
        { "MWL_FulingWall1_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_FulingWall1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsCamp", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        
        { "MWL_GoblinCave1_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_GoblinCave1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsCave", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        
        { "MWL_FulingTower1_Config", new LocationConfig { Quantity = Plains_Pack_1Plugin.MWL_FulingTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsCamp", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        
    };
    
}