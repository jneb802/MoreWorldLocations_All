using System.Collections.Generic;
using Jotunn.Configs;

namespace Swamp_Pack_1;

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
        { "MWL_GuckPit1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_GuckPit1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampAltar1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampAltar1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampAltar2_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampAltar2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampAltar3_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampAltar3_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampAltar4_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampAltar4_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampCastle2_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampCastle2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampGrave1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampGrave1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampHouse1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampRuin1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampRuin1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampTower1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampTower2_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_tower", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampTower3_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampTower3_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_large", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
        { "MWL_SwampWell1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_SwampWell1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_AbandonedHouse1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_AbandonedHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },
        
        { "MWL_Treehouse1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_Treehouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Treehouse", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_Shipyard1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_Shipyard1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_ship", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
        { "MWL_FortBakkarhalt1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_FortBakkarhalt1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_Huge", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },
        
        { "MWL_Belmont1_Config", new LocationConfig { Quantity = Swamp_Pack_1Plugin.MWL_Belmont1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_Huge", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },
    };
    
}