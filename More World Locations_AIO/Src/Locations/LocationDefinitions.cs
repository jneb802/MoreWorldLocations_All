using Jotunn.Configs;

namespace More_World_Locations_AIO;

public static class LocationDefinitions
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

    // ── Meadows ───────────────────────────────────────────────────────

    public static readonly MWLLocation[] Meadows =
    {
        new() { Name = "MWL_Ruins1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_Ruins1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

        new() { Name = "MWL_Ruins2", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_Ruins2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

        new() { Name = "MWL_Ruins3", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_Ruins3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

        new() { Name = "MWL_Ruins6", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_Ruins6.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 14, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

        new() { Name = "MWL_Ruins7", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_Ruins7.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 7, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

        new() { Name = "MWL_Ruins8", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_Ruins8.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 11, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

        new() { Name = "MWL_RuinsArena1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_RuinsArena1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = true, ForestTresholdMin = 1.2f, ForestTrasholdMax = 2 } },

        new() { Name = "MWL_RuinsArena3", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_RuinsArena3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = true, ForestTresholdMin = 0f, ForestTrasholdMax = 1 } },

        new() { Name = "MWL_RuinsChurch1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_RuinsChurch1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 256, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = true, ForestTresholdMin = 1.2f, ForestTrasholdMax = 2 } },

        new() { Name = "MWL_RuinsWell1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_RuinsWell1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 5, ClearArea = true, RandomRotation = false, Group = "Ruins_well", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring1.MaxDistance, InForest = false } },

        new() { Name = "MWL_MaypoleHut1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MaypoleHut1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 10, ClearArea = true, RandomRotation = false, Group = "Ruins_small", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

        new() { Name = "MWL_DeerShrine1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_DeerShrine1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_shrine", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

        new() { Name = "MWL_DeerShrine2", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_DeerShrine2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_shrine", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

        new() { Name = "MWL_MeadowsBarn1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsBarn1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Median, Group = "Wood_small", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

        new() { Name = "MWL_MeadowsHouse2", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsHouse2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Wood_small", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring5.MinDistance, InForest = true } },

        new() { Name = "MWL_MeadowsRuin1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsRuin1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_small", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },

        new() { Name = "MWL_MeadowsTomb4", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsTomb4.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Environment_medium", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = -1, MaxAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },

        new() { Name = "MWL_MeadowsTower1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_small", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance, InForest = false } },

        new() { Name = "MWL_OakHut1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_OakHut1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Wood_small", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_SmallHouse1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_SmallHouse1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Ruins_medium", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

        new() { Name = "MWL_MeadowsFarm1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsFarm1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

        new() { Name = "MWL_MeadowsLighthouse1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsLighthouse1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 1.5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

        new() { Name = "MWL_MeadowsSawmill1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsSawmill1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Median, Group = "Structure_large", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

        new() { Name = "MWL_MeadowsWall1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsWall1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 1.5f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

        new() { Name = "MWL_MeadowsTavern1", AssetPath = "Assets/WarpProjects/More World Locations/Meadows/MWL_MeadowsTavern1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, BiomeArea = Heightmap.BiomeArea.Everything, Group = "Structure_large", Priotized = true, RandomRotation = false,
                MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, InForest = true } },
    };

    // ── Black Forest ──────────────────────────────────────────────────

    public static readonly MWLLocation[] BlackForest =
    {
        new() { Name = "MWL_RuinsArena2", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RuinsArena2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_large", MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance, InForest = false } },

        new() { Name = "MWL_RuinsCastle1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RuinsCastle1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 12.5f, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

        new() { Name = "MWL_RuinsCastle3", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RuinsCastle3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Ruins_large", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 0, MaxAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

        new() { Name = "MWL_RuinsTower3", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RuinsTower3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

        new() { Name = "MWL_RuinsTower6", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RuinsTower6.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Ruins_large", MinDistanceFromSimilar = 512, MinDistance = LocationRings.Ring2.MinDistance, MaxAltitude = 1 } },

        new() { Name = "MWL_RuinsTower8", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RuinsTower8.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = true, Group = "Ruins_medium", MinDistanceFromSimilar = 512, MinAltitude = -2, MaxAltitude = 0.5f, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

        new() { Name = "MWL_Tavern1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_Tavern1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 12, ClearArea = true, RandomRotation = false, Group = "Wood_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

        new() { Name = "MWL_WoodTower1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_WoodTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Wood_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance, InForest = false } },

        new() { Name = "MWL_WoodTower2", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_WoodTower2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Wood_small", MinDistanceFromSimilar = 256, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

        new() { Name = "MWL_WoodTower3", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_WoodTower3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 24, ClearArea = true, RandomRotation = false, Group = "Wood_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 1, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance, InForest = false } },

        new() { Name = "MWL_ForestForge1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestForge1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Forge_small", Priotized = true, RandomRotation = false, ExteriorRadius = 12, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },

        new() { Name = "MWL_ForestForge2", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestForge2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Forge_small", Priotized = true, RandomRotation = false, ExteriorRadius = 16, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },

        new() { Name = "MWL_ForestGreatHouse2", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestGreatHouse2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "House_large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring3.MaxDistance } },

        new() { Name = "MWL_ForestHouse2", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestHouse2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "House_small", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },

        new() { Name = "MWL_ForestRuin1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestRuin1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_large", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 8, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },

        new() { Name = "MWL_ForestTower2", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestTower2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Tower_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance } },

        new() { Name = "MWL_ForestTower3", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestTower3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Tower_large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 10, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },

        new() { Name = "MWL_MassGrave1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_MassGrave1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Grave_large", Priotized = true, RandomRotation = false, ExteriorRadius = 3, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring2.MaxDistance } },

        new() { Name = "MWL_StoneFormation1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_StoneFormation1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Stone_small", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },

        new() { Name = "MWL_GuardTower1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_GuardTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_large", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 6, MinDistance = LocationRings.Ring3.MinDistance } },

        new() { Name = "MWL_RootRuins1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RootRuins1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring4.MaxDistance } },

        new() { Name = "MWL_RootsTower1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RootsTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring3.MinDistance, MaxDistance = LocationRings.Ring6.MaxDistance } },

        new() { Name = "MWL_RootsTower2", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RootsTower2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },

        new() { Name = "MWL_RuinedRootTower5", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RuinedRootTower5.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 5, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 10, MinDistance = LocationRings.Ring2.MinDistance } },

        new() { Name = "MWL_ForestRuin2", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestRuin2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance } },

        new() { Name = "MWL_ForestRuin3", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestRuin3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance } },

        new() { Name = "MWL_ForestSkull1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestSkull1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance } },

        new() { Name = "MWL_ForestTower4", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestTower4.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring3.MinDistance } },

        new() { Name = "MWL_ForestTower5", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestTower5.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_medium", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring5.MaxDistance } },

        new() { Name = "MWL_ForestPillar1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestPillar1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Ruins_small", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 2, MaxDistance = LocationRings.Ring3.MaxDistance } },

        new() { Name = "MWL_CoastTower1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_CoastTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Coastal", Priotized = true, RandomRotation = false, ClearArea = true, MinDistanceFromSimilar = 512, MinAltitude = -1, MaxAltitude = 0 } },

        new() { Name = "MWL_ForestGrove1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestGrove1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Grove", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 1.5f, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_RockShrine1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RockShrine1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Group = "Shrine", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1.5f, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_ForestCamp1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_ForestCamp1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Camp", MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 2, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

        new() { Name = "MWL_RuinedTower1", AssetPath = "Assets/WarpProjects/More World Locations/Blackforest/MWL_RuinedTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "RuinedTower1", MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },
    };

    // ── Swamp ─────────────────────────────────────────────────────────

    public static readonly MWLLocation[] Swamp =
    {
        new() { Name = "MWL_GuckPit1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_GuckPit1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampAltar1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampAltar1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampAltar2", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampAltar2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampAltar3", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampAltar3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampAltar4", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampAltar4.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_altar", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampCastle2", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampCastle2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampGrave1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampGrave1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampHouse1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampHouse1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampRuin1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampRuin1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = -1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampTower1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampTower2", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampTower2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_tower", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampTower3", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampTower3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_large", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampWell1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampWell1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_AbandonedHouse1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_AbandonedHouse1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

        new() { Name = "MWL_Treehouse1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_Treehouse1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Treehouse", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_Shipyard1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_Shipyard1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_ship", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_FortBakkarhalt1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_FortBakkarhalt1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_Huge", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

        new() { Name = "MWL_Belmont1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_Belmont1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_Huge", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

        new() { Name = "MWL_SwampCourtyard1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampCourtyard1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, InForest = false } },

        new() { Name = "MWL_SwampBrokenTower1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampBrokenTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, MinDistance = LocationRings.Ring4.MinDistance, InForest = false } },

        new() { Name = "MWL_SwampBrokenTower3", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampBrokenTower3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 3f, InForest = false } },

        new() { Name = "MWL_StoneCircle1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_StoneCircle1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_SwampTemple1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_SwampTemple1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Group = "Swamp_small", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_CastleCorner1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_CastleCorner1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Swamp_Ruins", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance, InForest = false } },

        new() { Name = "MWL_TreeTowers1", AssetPath = "Assets/WarpProjects/More World Locations/Swamp/MWL_TreeTowers1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "TreeTowers1", MinDistanceFromSimilar = 512, MaxTerrainDelta = 3f, MinAltitude = 1, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },
    };

    // ── Mountains ─────────────────────────────────────────────────────

    public static readonly MWLLocation[] Mountains =
    {
        new() { Name = "MWL_StoneCastle1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_StoneCastle1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, MinDistanceFromSimilar = 1024, SlopeRotation = true, MinTerrainDelta = 10f, MaxTerrainDelta = 15f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_StoneFort1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_StoneFort1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_StoneHall1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_StoneHall1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_StoneTavern1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_StoneTavern1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 15, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 18f, MinAltitude = 75, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_StoneTower1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_StoneTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_StoneTower2", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_StoneTower2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Small", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_WoodBarn1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_WoodBarn1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_1", Priotized = true, RandomRotation = false, ExteriorRadius = 20, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 3f, MinAltitude = 80, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_WoodFarm1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_WoodFarm1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_1", Priotized = true, RandomRotation = false, ExteriorRadius = 18, ClearArea = true, MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 7f, MinAltitude = 70, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_WoodHouse1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_WoodHouse1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Wood_2", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 10f, MinAltitude = 90f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false } },

        new() { Name = "MWL_TempleShrine1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_TempleShrine1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Large", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 10f, MinAltitude = 90f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false } },

        new() { Name = "MWL_StoneAltar1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_StoneAltar1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Group = "Mountain_Stone_Medium", Priotized = true, RandomRotation = false, ExteriorRadius = 10, ClearArea = true, MinDistanceFromSimilar = 512, MaxTerrainDelta = 10f, MinAltitude = 75f, InForest = false } },

        new() { Name = "MWL_MountainDvergrShrine1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_MountainDvergrShrine1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Shrine", MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring5.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_MountainDvergrShrine2", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_MountainDvergrShrine2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Shrine", MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring5.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_MountainOverlook1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_MountainOverlook1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mountain_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

        new() { Name = "MWL_MountainCultShrine1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_MountainCultShrine1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Shrine", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

        new() { Name = "MWL_RuinsChurch2", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_RuinsChurch2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mountain_medium", MinDistanceFromSimilar = 512, MaxTerrainDelta = 2f, MinAltitude = 2, MinDistance = LocationRings.Ring2.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },

        new() { Name = "MWL_MountainShrine1", AssetPath = "Assets/WarpProjects/More World Locations/Mountain/MWL_MountainShrine1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Shrine", MinDistanceFromSimilar = 512, MaxTerrainDelta = 4f, MinAltitude = 2, MinDistance = LocationRings.Ring4.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },
    };

    // ── Plains ────────────────────────────────────────────────────────

    public static readonly MWLLocation[] Plains =
    {
        new() { Name = "MWL_GoblinFort1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_GoblinFort1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsFort", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_FulingRock1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingRock1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_FulingVillage1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingVillage1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsVillage", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_FulingVillage2", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingVillage2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsVillage", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_PlainsPillar1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_PlainsPillar1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_FulingTemple1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingTemple1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_FulingTemple2", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingTemple2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_FulingTemple3", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingTemple3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_FulingTempleBroken1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingTempleBroken1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_FulingTemple4", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingTemple4.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsTemple", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_FulingWall1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingWall1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsCamp", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_FulingTower1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_FulingTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsCamp", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_RockGarden1", AssetPath = "Assets/WarpProjects/More World Locations/Plains/MWL_RockGarden1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Group = "PlainsRock", Priotized = true, RandomRotation = false, ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },
    };

    // ── Mistlands ─────────────────────────────────────────────────────

    public static readonly MWLLocation[] Mistlands =
    {
        new() { Name = "MWL_MistFort2", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistFort2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 8, ClearArea = true, RandomRotation = false, Group = "Mist3", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_SecretRoom1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_SecretRoom1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist1", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MistWorkshop1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistWorkshop1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist2", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 10f, MinAltitude = 1f, MaxAltitude = 6f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MistTower1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistTower1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MistWall1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistWall1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist2", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 1f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MistTower2", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistTower2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = -2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MistHut1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistHut1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist3", MinDistanceFromSimilar = 256, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_DvergrEitrSingularity1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_DvergrEitrSingularity1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist5", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_DvergrHouse1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_DvergrHouse1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Edge } },

        new() { Name = "MWL_DvergrHouseWood1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_DvergrHouseWood1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_DvergrHouseWood2", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_DvergrHouseWood2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MarbleJail1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MarbleJail1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 2f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MarbleHome1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MarbleHome1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MarbleHome2", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MarbleHome2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MarbleCliffAltar1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MarbleCliffAltar1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_MistPond1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistPond1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MaxAltitude = 4f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_MarbleCage1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MarbleCage1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist4", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything } },

        new() { Name = "MWL_DvergrKnowledgeExtractor1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_DvergrKnowledgeExtractor1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist5", MinDistanceFromSimilar = 512, MinTerrainDelta = 0f, MaxTerrainDelta = 15f, MinAltitude = 5f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_AncientShrine1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_AncientShrine1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist6", MinDistanceFromSimilar = 512, MaxTerrainDelta = 15f, MinAltitude = -2f, MaxAltitude = 2f, InForest = false, BiomeArea = Heightmap.BiomeArea.Edge } },

        new() { Name = "MWL_MistShrine1", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistShrine1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 15, ClearArea = true, RandomRotation = false, Group = "Mist6", MinDistanceFromSimilar = 512, MaxTerrainDelta = 15f, MaxAltitude = 20f, MinAltitude = 3f, InForest = false, BiomeArea = Heightmap.BiomeArea.Median } },

        new() { Name = "MWL_MistHut2", AssetPath = "Assets/WarpProjects/More World Locations/Mistlands/MWL_MistHut2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "Camp", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 5, MinDistance = LocationRings.Ring1.MinDistance, MaxDistance = LocationRings.Ring7.MaxDistance } },
    };

    // ── Ashlands ──────────────────────────────────────────────────────

    public static readonly MWLLocation[] Ashlands =
    {
        new() { Name = "MWL_AshlandsFort1", AssetPath = "Assets/WarpProjects/More World Locations/Ashlands/MWL_AshlandsFort1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } },

        new() { Name = "MWL_AshlandsFort2", AssetPath = "Assets/WarpProjects/More World Locations/Ashlands/MWL_AshlandsFort2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } },

        new() { Name = "MWL_AshlandsFort3", AssetPath = "Assets/WarpProjects/More World Locations/Ashlands/MWL_AshlandsFort3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.AshLands, Priotized = true, ClearArea = true, RandomRotation = false, Group = "Ashlands_Fort", MinDistanceFromSimilar = 512, MaxTerrainDelta = 5f, MinAltitude = 1, InForest = false } },
    };

    // ── Ports ────────────────────────────────────────────────────────────

    public static readonly MWLLocation[] Ports =
    {
        new() { Name = "MWL_Port1", AssetPath = "Assets/WarpProjects/More World Locations/Ports/MWL_Port1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = -2f, MaxAltitude = 1, SlopeRotation = true, Group = "MWL_Ports" } },

        new() { Name = "MWL_Port2", AssetPath = "Assets/WarpProjects/More World Locations/Ports/MWL_Port2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 3f, MinAltitude = -2f, MaxAltitude = 1, SlopeRotation = true, Group = "MWL_Ports", BiomeArea = Heightmap.BiomeArea.Edge } },

        new() { Name = "MWL_Port3", AssetPath = "Assets/WarpProjects/More World Locations/Ports/MWL_Port3.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 10f, MinAltitude = -1f, MaxAltitude = 2, SlopeRotation = true, Group = "MWL_Ports" } },

        new() { Name = "MWL_Port4", AssetPath = "Assets/WarpProjects/More World Locations/Ports/MWL_Port4.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, MinDistanceFromSimilar = 1024, MaxTerrainDelta = 4f, MinAltitude = -1f, MaxAltitude = 1, SlopeRotation = true, Group = "MWL_Ports" } },
    };

    // ── Traders ──────────────────────────────────────────────────────────

    public static readonly MWLLocation[] Traders =
    {
        new() { Name = "MWL_PlainsTavern1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_PlainsTavern1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Priotized = true, ExteriorRadius = 32, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_OceanTavern1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_OceanTavern1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Ocean, Priotized = true, ExteriorRadius = 50, ClearArea = false, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_PlainsCamp1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_PlainsCamp1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_BlackForestBlacksmith1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_BlackForestBlacksmith1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_BlackForestBlacksmith2", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_BlackForestBlacksmith2.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.BlackForest, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_MountainsBlacksmith1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_MountainsBlacksmith1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mountain, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 6f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_MistlandsBlacksmith1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_MistlandsBlacksmith1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 12f, MinAltitude = 1f, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },
    };

    // ── Trainers ─────────────────────────────────────────────────────────

    public static readonly MWLLocation[] Trainers =
    {
        new() { Name = "MWL_MeadowsTrainer1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_MeadowsTrainer1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Meadows, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 1f, MinDistance = LocationRings.Ring3.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_SwampTrainer1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_SwampTrainer1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Swamp, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 8f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_PlainsTrainer1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_PlainsTrainer1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Plains, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 5f, MinAltitude = 0f, MinDistance = LocationRings.Ring2.MinDistance, InForest = false, BiomeArea = Heightmap.BiomeArea.Median, Unique = true, IconPlaced = true } },

        new() { Name = "MWL_MistTrainer1", AssetPath = "Assets/WarpProjects/MoreWorldTraders/MWL_MistTrainer1.prefab",
            Config = new LocationConfig { Biome = Heightmap.Biome.Mistlands, Priotized = true, ExteriorRadius = 20, ClearArea = true, RandomRotation = false, Group = "MWL_Trader", MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, MaxTerrainDelta = 12f, MinAltitude = -3f, MaxAltitude = 0f, InForest = false, BiomeArea = Heightmap.BiomeArea.Everything, Unique = true, IconPlaced = true } },
    };
}
