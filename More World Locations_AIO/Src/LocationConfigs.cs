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
            { "MWL_Ruins1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Ruins1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

            { "MWL_Ruins2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Ruins2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_Ruins3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Ruins3_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

            { "MWL_Ruins6_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Ruins6_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 14, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_Ruins7_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Ruins7_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 7, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

            { "MWL_Ruins8_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Ruins8_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 11, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_RuinsArena1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsArena1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = true, ForestTresholdMin = 1.2f, ForestTrasholdMax = 2 } },

            { "MWL_RuinsArena3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsArena3_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = true, ForestTresholdMin = 0f, ForestTrasholdMax = 1 } },

            { "MWL_RuinsChurch1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsChurch1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = true, ForestTresholdMin = 1.2f, ForestTrasholdMax = 2 } },

            { "MWL_RuinsWell1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsWell1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 5, ClearArea = true, RandomRotation = false, Group = "Ruins_well", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },
        };

    public static Dictionary<string, LocationConfig> MeadowsPack2LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_DeerShrine1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_DeerShrine1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_shrine", Priotized = true, RandomRotation = false, 
            MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },
        
            { "MWL_DeerShrine2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_DeerShrine2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_shrine", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },
            
            { "MWL_MeadowsBarn1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsBarn1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Median, Group = "Wood_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },
            
            { "MWL_MeadowsHouse2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsHouse2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Wood_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring5.MinDistance, InForest = true } },
            
            { "MWL_MeadowsRuin1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsRuin1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },
            
            { "MWL_MeadowsTomb4_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsTomb4_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Environment_medium", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = -1, MaxAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },
            
            { "MWL_MeadowsTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance, InForest = false } },
            
            { "MWL_OakHut1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_OakHut1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Wood_small", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_SmallHouse1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SmallHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_medium", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },
            
            { "MWL_MeadowsFarm1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsFarm1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_MeadowsLighthouse1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsLighthouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 1.5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_MeadowsSawmill1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsSawmill1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Median, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

            { "MWL_MeadowsWall1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsWall1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 1.5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

            { "MWL_MeadowsTavern1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MeadowsTavern1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance,  InForest = true } },
        };
    
    public static Dictionary<string, LocationConfig> BlackforestPack1Configs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_RuinsArena2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsArena2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_large", 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },

            { "MWL_RuinsCastle1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsCastle1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 12.5f, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_RuinsCastle3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsCastle3_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_large", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 0, MaxAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_RuinsTower3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsTower3_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_RuinsTower8_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsTower8_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = true, Group = "Ruins_medium", 
                MinDistanceFromSimilar = 512, MinAltitude = -2, MaxAltitude = 0.5f, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_Tavern1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinsTavern1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 12, ClearArea = true, RandomRotation = false, Group = "Wood_small", 
                MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_WoodTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_WoodTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Wood_small", 
                MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

            { "MWL_WoodTower2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_WoodTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Wood_small", 
                MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

            { "MWL_WoodTower3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_WoodTower3_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 24, ClearArea = true, RandomRotation = false, Group = "Wood_medium", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },
        };

    
    public static Dictionary<string, LocationConfig> BlackforestPack2LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
              { "MWL_ForestForge1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestForge1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Forge_small", Priotized = true, RandomRotation = false, ExteriorRadius = 12, ClearArea = true, 
            MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },
        
            { "MWL_ForestForge2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestForge2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Forge_small", Priotized = true, RandomRotation = false, ExteriorRadius = 16, ClearArea = true, 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },
            
            { "MWL_ForestGreatHouse2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestGreatHouse2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "House_large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance } },
            
            { "MWL_ForestHouse2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestHouse2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "House_small", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },
            
            { "MWL_ForestRuin1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestRuin1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_large", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 8, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },
            
            { "MWL_ForestTower2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Tower_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance } },
            
            { "MWL_ForestTower3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestTower3_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Tower_large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 10, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },
            
            { "MWL_MassGrave1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MassGrave1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Grave_large", Priotized = true, RandomRotation = false, ExteriorRadius = 3, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance } },

            { "MWL_StoneFormation1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_StoneFormation1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Stone_small", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },
            
            { "MWL_GuardTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_GuardTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_large", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 6, MinDistance = LocationRings.Ring3.MinDistance } },
            
            { "MWL_RootRuins1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RootRuins1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance } },
            
            { "MWL_RootsTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RootsTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },
            
            { "MWL_RootsTower2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RootsTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },
            
            { "MWL_StoneOutlook1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_StoneOutlook1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Coastal", Priotized = true, RandomRotation = false, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinAltitude = -2, MaxAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, SlopeRotation = true } },

            { "MWL_ForestRuin2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestRuin2_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance } },
            
            { "MWL_ForestRuin3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestRuin3_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance } },
            
            { "MWL_ForestSkull1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestSkull1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance } },
            
            { "MWL_ForestTower4_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestTower5_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance } },
            
            { "MWL_ForestTower5_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestTower5_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },          
        };
    
    public static Dictionary<string, LocationConfig> SwampPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_GuckPit1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_GuckPit1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
            { "MWL_SwampAltar1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampAltar1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampAltar2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampAltar2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampAltar3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampAltar3_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampAltar4_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampAltar4_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampCastle2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampCastle2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampGrave1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampGrave1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampHouse1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampRuin1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampRuin1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampTower2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_tower", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampTower3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampTower3_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_large", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_SwampWell1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SwampWell1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_AbandonedHouse1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_AbandonedHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },
            
            { "MWL_Treehouse1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Treehouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Treehouse", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_Shipyard1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Shipyard1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_ship", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_FortBakkarhalt1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FortBakkarhalt1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_Huge", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },
            
            { "MWL_Belmont1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Belmont1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Group = "Swamp_Huge", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },
        };
    
    public static Dictionary<string, LocationConfig> MountainPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_StoneCastle1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_StoneCastle1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, 
            MinDistanceFromSimilar = 1024, SlopeRotation = true, MinTerrainDelta = 7f, MaxTerrainDelta = 15f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        
            { "MWL_StoneFort1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_StoneFort1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_StoneHall1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_StoneHall1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_StoneTavern1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_StoneTavern1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 18f, MinAltitude = 75, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_StoneTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_StoneTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_StoneTower2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_StoneTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_WoodBarn1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_WoodBarn1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_1", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 3f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_WoodFarm1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_WoodFarm1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_1", Priotized = true, RandomRotation = false, ExteriorRadius = 18, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 7f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
            
            { "MWL_WoodHouse1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_WoodHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_2", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 90f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },
        };
    
    public static Dictionary<string, LocationConfig> PlainsPack1LocationConfigs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_GoblinFort1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_GoblinFort1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsFort", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
        
            { "MWL_FulingRock1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FulingRock1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
            
            { "MWL_FulingVillage1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FulingVillage1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsVillage", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_FulingVillage2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FulingVillage2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsVillage", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
            
            { "MWL_PlainsPillar1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_PlainsPillar1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
            
            { "MWL_FulingTemple1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FulingTemple1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
            
            { "MWL_FulingTemple2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FulingTemple2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
            
            { "MWL_FulingTemple3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FulingTemple3_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
            
            { "MWL_FulingWall1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FulingWall1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsCamp", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
            
            // { "MWL_GoblinCave1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_GoblinCave1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsCave", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
            //    MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
            
            { "MWL_FulingTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_FulingTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Plains, Group = "PlainsCamp", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },
        };
    
    public static Dictionary<string, LocationConfig> MistlandsPack1Configs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_MistFort2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MistFort2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Mist3", 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_SecretRoom1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_SecretRoom1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist1", 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistWorkshop1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MistWorkshop1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist2", 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 1f, MaxAltitude = 6f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MistTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistWall1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MistWall1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist2", 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistTower2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MistTower2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = -2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_MistHut1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MistHut1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", 
                MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_DvergrEitrSingularity1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_DvergrEitrSingularity1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist5", 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

            { "MWL_DvergrHouse1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_DvergrHouse1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Edge } },

            { "MWL_DvergrKnowledgeExtractor1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_DvergrKnowledgeExtractor1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist5", 
                MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } }
        };

    
    public static Dictionary<string, LocationConfig> AshlandsPack1Configs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_AshlandsFort1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_AshlandsFort1_Configuration.Quantity.Value, Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } },

            { "MWL_AshlandsFort2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_AshlandsFort2_Configuration.Quantity.Value, Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } },

            { "MWL_AshlandsFort3_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_AshlandsFort3_Configuration.Quantity.Value, Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } }
        };

    
    public static Dictionary<string, LocationConfig> AdventureMapPack1Configs =
        new Dictionary<string, LocationConfig>
        {
            { "MWL_CastleCorner1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_CastleCorner1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Swamp_Ruins", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance, InForest = false } },

            { "MWL_ForestCamp1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_ForestCamp1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Camp", 
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_MistHut2_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_Misthut2_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Camp", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 5, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_MountainDvergrShrine1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MountainDvergrShrine1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Shrine", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring5.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance, BiomeArea = Heightmap.BiomeArea.Median } },

            { "MWL_MountainShrine1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_MountainShrine1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Shrine", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_RuinedTower1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_RuinedTower1_Configuration.Quantity.Value, Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "RuinedTower1", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

            { "MWL_TreeTowers1_Config", new LocationConfig { Quantity = BepinexConfigs.MWL_TreeTowers1_Configuration.Quantity.Value, Biome = Heightmap.Biome.Swamp, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "TreeTowers1", 
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } }
        };

}