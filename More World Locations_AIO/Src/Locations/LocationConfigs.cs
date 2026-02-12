using System.Collections.Generic;
using Jotunn.Configs;

namespace More_World_Locations_AIO;

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
    
    public static int GetQuantity(string key)
    {
        return LocationQuantityManager.GetQuantity(key);
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
    
    public static Dictionary<string, LocationConfig> MeadowsPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_Ruins1_Config", new LocationConfig { Quantity = GetQuantity("MWL_Ruins1"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

            { "MWL_Ruins2_Config", new LocationConfig { Quantity = GetQuantity("MWL_Ruins2"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_Ruins3_Config", new LocationConfig { Quantity = GetQuantity("MWL_Ruins3"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

            { "MWL_Ruins6_Config", new LocationConfig { Quantity = GetQuantity("MWL_Ruins6"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 14, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_Ruins7_Config", new LocationConfig { Quantity = GetQuantity("MWL_Ruins7"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 7, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

            { "MWL_Ruins8_Config", new LocationConfig { Quantity = GetQuantity("MWL_Ruins8"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 11, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_RuinsArena1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsArena1"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = true, ForestTresholdMin = 1.2f, ForestTrasholdMax = 2 } },

            { "MWL_RuinsArena3_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsArena3"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = true, ForestTresholdMin = 0f, ForestTrasholdMax = 1 } },

            { "MWL_RuinsChurch1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsChurch1"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = true, ForestTresholdMin = 1.2f, ForestTrasholdMax = 2 } },

            { "MWL_RuinsWell1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsWell1"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 5, ClearArea = true, RandomRotation = false, Group = "Ruins_well", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

            { "MWL_MaypoleHut1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MaypoleHut1"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 10, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },
        };


    public static Dictionary<string, LocationConfig> MeadowsPack2LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_DeerShrine1_Config", new LocationConfig { Quantity = GetQuantity("MWL_DeerShrine1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_shrine", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_DeerShrine2_Config", new LocationConfig { Quantity = GetQuantity("MWL_DeerShrine2"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_shrine", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

            { "MWL_MeadowsBarn1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsBarn1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Median, Group = "Wood_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_MeadowsHouse2_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsHouse2"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Wood_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring5.MinDistance, InForest = true } },

            { "MWL_MeadowsRuin1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsRuin1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },

            { "MWL_MeadowsTomb4_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsTomb4"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Environment_medium", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = -1, MaxAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },

            { "MWL_MeadowsTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsTower1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance, InForest = false } },

            { "MWL_OakHut1_Config", new LocationConfig { Quantity = GetQuantity("MWL_OakHut1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Wood_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_SmallHouse1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SmallHouse1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_medium", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_MeadowsFarm1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsFarm1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_MeadowsLighthouse1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsLighthouse1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 1.5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_MeadowsSawmill1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsSawmill1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Median, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

            { "MWL_MeadowsWall1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsWall1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 1.5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_MeadowsTavern1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsTavern1"), Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = true } },
        };

    
    public static Dictionary<string, LocationConfig> BlackforestPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_RuinsArena2_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsArena2"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_large", MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },

            { "MWL_RuinsCastle1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsCastle1"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 12.5f, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_RuinsCastle3_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsCastle3"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_large", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 0, MaxAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_RuinsTower3_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsTower3"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_RuinsTower6_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsTower6"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_large", MinDistanceFromSimilar = 512, MinDistance = LocationRings.Ring2.MinDistance, MaxAltitude = 1 } },

            { "MWL_RuinsTower8_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsTower8"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = true, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MinAltitude = -2, MaxAltitude = 0.5f, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_Tavern1_Config", new LocationConfig { Quantity = GetQuantity("MWL_Tavern1"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 12, ClearArea = true, RandomRotation = false, Group = "Wood_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_WoodTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_WoodTower1"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Wood_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_WoodTower2_Config", new LocationConfig { Quantity = GetQuantity("MWL_WoodTower2"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Wood_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_WoodTower3_Config", new LocationConfig { Quantity = GetQuantity("MWL_WoodTower3"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 24, ClearArea = true, RandomRotation = false, Group = "Wood_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },
        };


    
    public static Dictionary<string, LocationConfig> BlackforestPack2LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_ForestForge1_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestForge1"), Biome = Heightmap.Biome.BlackForest, Group = "Forge_small", Priotized = true, RandomRotation = false, ExteriorRadius = 12, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },

            { "MWL_ForestForge2_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestForge2"), Biome = Heightmap.Biome.BlackForest, Group = "Forge_small", Priotized = true, RandomRotation = false, ExteriorRadius = 16, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },

            { "MWL_ForestGreatHouse2_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestGreatHouse2"), Biome = Heightmap.Biome.BlackForest, Group = "House_large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance } },

            { "MWL_ForestHouse2_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestHouse2"), Biome = Heightmap.Biome.BlackForest, Group = "House_small", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },

            { "MWL_ForestRuin1_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestRuin1"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_large", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 8, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },

            { "MWL_ForestTower2_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestTower2"), Biome = Heightmap.Biome.BlackForest, Group = "Tower_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance } },

            { "MWL_ForestTower3_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestTower3"), Biome = Heightmap.Biome.BlackForest, Group = "Tower_large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 10, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },

            { "MWL_MassGrave1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MassGrave1"), Biome = Heightmap.Biome.BlackForest, Group = "Grave_large", Priotized = true, RandomRotation = false, ExteriorRadius = 3, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance } },

            { "MWL_StoneFormation1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneFormation1"), Biome = Heightmap.Biome.BlackForest, Group = "Stone_small", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },

            { "MWL_GuardTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_GuardTower1"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_large", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 6, MinDistance = LocationRings.Ring3.MinDistance } },

            { "MWL_RootRuins1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RootRuins1"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance } },

            { "MWL_RootsTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RootsTower1"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },

            { "MWL_RootsTower2_Config", new LocationConfig { Quantity = GetQuantity("MWL_RootsTower2"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },

            { "MWL_RuinedRootTower5_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinedRootTower5"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },

            { "MWL_StoneOutlook1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneOutlook1"), Biome = Heightmap.Biome.BlackForest, Group = "Coastal", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MinAltitude = -2, MaxAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, SlopeRotation = true } },

            { "MWL_ForestRuin2_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestRuin2"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance } },

            { "MWL_ForestRuin3_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestRuin3"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance } },

            { "MWL_ForestSkull1_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestSkull1"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance } },

            { "MWL_ForestTower4_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestTower4"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance } },

            { "MWL_ForestTower5_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestTower5"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },

            { "MWL_ForestPillar1_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestPillar1"), Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MaxDistance = LocationRings.Ring3.MaxDistance } },

            { "MWL_CoastTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_CoastTower1"), Biome = Heightmap.Biome.BlackForest, Group = "Coastal", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 512, MinAltitude = -1, MaxAltitude = 0 } },

            { "MWL_ForestGrove1_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestGrove1"), Biome = Heightmap.Biome.BlackForest, Group = "Grove", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1.5f, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_RockShrine1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RockShrine1"), Biome = Heightmap.Biome.BlackForest, Group = "Shrine", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1.5f, BiomeArea = Heightmap.BiomeArea.Median } }
        };

    
    public static Dictionary<string, LocationConfig> SwampPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_GuckPit1_Config", new LocationConfig { Quantity = GetQuantity("MWL_GuckPit1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampAltar1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampAltar1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampAltar2_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampAltar2"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampAltar3_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampAltar3"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampAltar4_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampAltar4"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampCastle2_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampCastle2"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampGrave1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampGrave1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampHouse1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampHouse1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampRuin1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampRuin1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampTower1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampTower2_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampTower2"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_tower", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampTower3_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampTower3"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_large", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampWell1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampWell1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_AbandonedHouse1_Config", new LocationConfig { Quantity = GetQuantity("MWL_AbandonedHouse1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_Treehouse1_Config", new LocationConfig { Quantity = GetQuantity("MWL_Treehouse1"), Biome = Heightmap.Biome.Swamp, Group = "Treehouse", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_Shipyard1_Config", new LocationConfig { Quantity = GetQuantity("MWL_Shipyard1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_ship", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_FortBakkarhalt1_Config", new LocationConfig { Quantity = GetQuantity("MWL_FortBakkarhalt1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_Huge", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_Belmont1_Config", new LocationConfig { Quantity = GetQuantity("MWL_Belmont1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_Huge", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

            { "MWL_SwampCourtyard1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampCourtyard1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, InForest = false } },

            { "MWL_SwampBrokenTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampBrokenTower1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

            { "MWL_SwampBrokenTower3_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampBrokenTower3"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, InForest = false } },

            { "MWL_StoneCircle1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneCircle1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_SwampTemple1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampTemple1"), Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } }
        };

    
    public static Dictionary<string, LocationConfig> MountainPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_StoneCastle1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneCastle1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, MinDistanceFromSimilar = 1024, SlopeRotation = true, MinTerrainDelta = 10f, MaxTerrainDelta = 15f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_StoneFort1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneFort1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_StoneHall1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneHall1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_StoneTavern1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneTavern1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 18f, MinAltitude = 75, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_StoneTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneTower1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_StoneTower2_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneTower2"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_WoodBarn1_Config", new LocationConfig { Quantity = GetQuantity("MWL_WoodBarn1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_1", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 3f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_WoodFarm1_Config", new LocationConfig { Quantity = GetQuantity("MWL_WoodFarm1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_1", Priotized = true, RandomRotation = false, ExteriorRadius = 18, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 7f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_WoodHouse1_Config", new LocationConfig { Quantity = GetQuantity("MWL_WoodHouse1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_2", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 10f, MinAltitude = 90f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

            { "MWL_TempleShrine1_Config", new LocationConfig { Quantity = GetQuantity("MWL_TempleShrine1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 10f, MinAltitude = 90f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_StoneAltar1_Config", new LocationConfig { Quantity = GetQuantity("MWL_StoneAltar1"), Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 10f, MinAltitude = 75f, InForest = false } },
        };

    
    public static Dictionary<string, LocationConfig> PlainsPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_GoblinFort1_Config", new LocationConfig { Quantity = GetQuantity("MWL_GoblinFort1"), Biome = Heightmap.Biome.Plains, Group = "PlainsFort", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_FulingRock1_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingRock1"), Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_FulingVillage1_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingVillage1"), Biome = Heightmap.Biome.Plains, Group = "PlainsVillage", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_FulingVillage2_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingVillage2"), Biome = Heightmap.Biome.Plains, Group = "PlainsVillage", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_PlainsPillar1_Config", new LocationConfig { Quantity = GetQuantity("MWL_PlainsPillar1"), Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_FulingTemple1_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingTemple1"), Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_FulingTemple2_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingTemple2"), Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_FulingTemple3_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingTemple3"), Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_FulingTempleBroken1_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingTempleBroken1"), Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_FulingTemple4_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingTemple4"), Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_FulingWall1_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingWall1"), Biome = Heightmap.Biome.Plains, Group = "PlainsCamp", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_FulingTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_FulingTower1"), Biome = Heightmap.Biome.Plains, Group = "PlainsCamp", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_RockGarden1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RockGarden1"), Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        };

    
    public static Dictionary<string, LocationConfig> MistlandsPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_MistFort2_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistFort2"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Mist3", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_SecretRoom1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SecretRoom1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist1", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistWorkshop1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistWorkshop1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist2", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 1f, MaxAltitude = 6f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistTower1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistWall1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistWall1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist2", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistTower2_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistTower2"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = -2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistHut1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistHut1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_DvergrEitrSingularity1_Config", new LocationConfig { Quantity = GetQuantity("MWL_DvergrEitrSingularity1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist5", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_DvergrHouse1_Config", new LocationConfig { Quantity = GetQuantity("MWL_DvergrHouse1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Edge } },

            { "MWL_DvergrHouseWood1_Config", new LocationConfig { Quantity = GetQuantity("MWL_DvergrHouseWood1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_DvergrHouseWood2_Config", new LocationConfig { Quantity = GetQuantity("MWL_DvergrHouseWood2"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MarbleJail1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MarbleJail1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MarbleHome1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MarbleHome1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MarbleHome2_Config", new LocationConfig { Quantity = GetQuantity("MWL_MarbleHome2"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MarbleCliffAltar1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MarbleCliffAltar1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistPond1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistPond1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MaxAltitude = 4f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_MarbleCage1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MarbleCage1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_DvergrKnowledgeExtractor1_Config", new LocationConfig { Quantity = GetQuantity("MWL_DvergrKnowledgeExtractor1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist5", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_AncientShrine1_Config", new LocationConfig { Quantity = GetQuantity("MWL_AncientShrine1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist6", MinDistanceFromSimilar = 512, MaxTerrainDelta = 15f, MinAltitude = -2f, MaxAltitude = 2f, InForest = false, BiomeArea = Heightmap.BiomeArea.Edge } },

            { "MWL_MistShrine1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistShrine1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist6", MinDistanceFromSimilar = 512, MaxTerrainDelta = 15f, MaxAltitude = 20f, MinAltitude = 3f, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        };


    
    public static Dictionary<string, LocationConfig> AshlandsPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_AshlandsFort1_Config", new LocationConfig { Quantity = GetQuantity("MWL_AshlandsFort1"), Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } },

            { "MWL_AshlandsFort2_Config", new LocationConfig { Quantity = GetQuantity("MWL_AshlandsFort2"), Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } },

            { "MWL_AshlandsFort3_Config", new LocationConfig { Quantity = GetQuantity("MWL_AshlandsFort3"), Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } }
        };


    
    public static Dictionary<string, LocationConfig> AdventureMapPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_CastleCorner1_Config", new LocationConfig { Quantity = GetQuantity("MWL_CastleCorner1"), Biome = Heightmap.Biome.Swamp, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Swamp_Ruins", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance, InForest = false } },

            { "MWL_ForestCamp1_Config", new LocationConfig { Quantity = GetQuantity("MWL_ForestCamp1"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Camp", MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_MistHut2_Config", new LocationConfig { Quantity = GetQuantity("MWL_Misthut2"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Camp", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 5, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_MountainDvergrShrine1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MountainDvergrShrine1"), Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Shrine", MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring5.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_MountainDvergrShrine2_Config", new LocationConfig { Quantity = GetQuantity("MWL_MountainDvergrShrine2"), Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Shrine", MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring5.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_MountainOverlook1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MountainOverlook1"), Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mountain_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_MountainCultShrine1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MountainCultShrine1"), Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Shrine", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_RuinsChurch2_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinsChurch2"), Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mountain_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_MountainShrine1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MountainShrine1"), Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Shrine", MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_RuinedTower1_Config", new LocationConfig { Quantity = GetQuantity("MWL_RuinedTower1"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "RuinedTower1", MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_TreeTowers1_Config", new LocationConfig { Quantity = GetQuantity("MWL_TreeTowers1"), Biome = Heightmap.Biome.Swamp, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "TreeTowers1", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },
        };
    
    public static Dictionary<string, LocationConfig> PortLocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_Port1_Config", new LocationConfig { Quantity = GetQuantity("MWL_Port1"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = -2f, MaxAltitude = 1, SlopeRotation = true, Group = "MWL_Ports"} },
            { "MWL_Port2_Config", new LocationConfig { Quantity = GetQuantity("MWL_Port2"), Biome = Heightmap.Biome.Plains, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = -2f, MaxAltitude = 1, SlopeRotation = true, Group = "MWL_Ports", BiomeArea = Heightmap.BiomeArea.Edge } },
            { "MWL_Port3_Config", new LocationConfig { Quantity = GetQuantity("MWL_Port3"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 10f, MinAltitude = -1f, MaxAltitude = 2, SlopeRotation = true, Group = "MWL_Ports" } },
            { "MWL_Port4_Config", new LocationConfig { Quantity = GetQuantity("MWL_Port4"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = -1f, MaxAltitude = 1, SlopeRotation = true, Group = "MWL_Ports" } },
        };
    
    public static Dictionary<string, LocationConfig> TraderLocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_PlainsTavern1_Config", new LocationConfig { Quantity = GetQuantity("MWL_PlainsTavern1"), Biome = Heightmap.Biome.Plains, Priotized = true, ExteriorRadius = 32, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_OceanTavern1_Config", new LocationConfig { Quantity = GetQuantity("MWL_OceanTavern1"), Biome = Heightmap.Biome.Ocean, Priotized = true, ExteriorRadius = 50, ClearArea = false, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_PlainsCamp1_Config", new LocationConfig { Quantity = GetQuantity("MWL_PlainsCamp1"), Biome = Heightmap.Biome.Plains, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_BlackForestBlacksmith1_Config", new LocationConfig { Quantity = GetQuantity("MWL_BlackForestBlacksmith1"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_BlackForestBlacksmith2_Config", new LocationConfig { Quantity = GetQuantity("MWL_BlackForestBlacksmith2"), Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_MountainsBlacksmith1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MountainsBlacksmith1"), Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_MistlandsBlacksmith1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistlandsBlacksmith1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 12f, MinAltitude = 1f, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_MeadowsTrainer1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MeadowsTrainer1"), Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 1f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_SwampTrainer1_Config", new LocationConfig { Quantity = GetQuantity("MWL_SwampTrainer1"), Biome = Heightmap.Biome.Swamp, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_PlainsTrainer1_Config", new LocationConfig { Quantity = GetQuantity("MWL_PlainsTrainer1"), Biome = Heightmap.Biome.Plains, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
            { "MWL_MistTrainer1_Config", new LocationConfig { Quantity = GetQuantity("MWL_MistTrainer1"), Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 12f, MinAltitude = -3f, MaxAltitude = 0f, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything, Unique = true, IconPlaced = true } },
        };
    
    /// <summary>
    /// Gets the LocationConfig for a given location name by searching all location config dictionaries
    /// </summary>
    public static LocationConfig GetLocationConfig(string locationName)
    {
        string configKey = locationName + "_Config";
        
        // Search through all location config dictionaries
        if (LocationConfigs.MeadowsPack1LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.MeadowsPack1LocationConfigs[configKey];
            
        if (LocationConfigs.MeadowsPack2LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.MeadowsPack2LocationConfigs[configKey];
            
        if (LocationConfigs.BlackforestPack1LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.BlackforestPack1LocationConfigs[configKey];
            
        if (LocationConfigs.BlackforestPack2LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.BlackforestPack2LocationConfigs[configKey];
            
        if (LocationConfigs.SwampPack1LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.SwampPack1LocationConfigs[configKey];
            
        if (LocationConfigs.MountainPack1LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.MountainPack1LocationConfigs[configKey];
            
        if (LocationConfigs.PlainsPack1LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.PlainsPack1LocationConfigs[configKey];
            
        if (LocationConfigs.MistlandsPack1LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.MistlandsPack1LocationConfigs[configKey];
            
        if (LocationConfigs.AshlandsPack1LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.AshlandsPack1LocationConfigs[configKey];
            
        if (LocationConfigs.AdventureMapPack1LocationConfigs.ContainsKey(configKey))
            return LocationConfigs.AdventureMapPack1LocationConfigs[configKey];
            
        if (LocationConfigs.TraderLocationConfigs.ContainsKey(configKey))
            return LocationConfigs.TraderLocationConfigs[configKey];
        return null;
    }
}