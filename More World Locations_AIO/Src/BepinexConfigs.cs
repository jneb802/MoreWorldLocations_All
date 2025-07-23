using System.Collections.Generic;
using Common;

namespace More_World_Locations_AIO;

public class BepinexConfigs
{
    public static BepInEx.Configuration.ConfigFile Config;

    public static Dictionary<string, LocationConfiguration> bepinexConfigs =
        new Dictionary<string, LocationConfiguration>();

    public static void GenerateConfigs(BepInEx.Configuration.ConfigFile configFile)
    {
        Config = configFile;
        
        bepinexConfigs = new Dictionary<string, LocationConfiguration>
        {
            // Meadows Pack 1
            { "MWL_Ruins1_Configuration", new LocationConfiguration(Config, "MWL_Ruins1", 5, "MeadowsCreatures1", "MeadowsLoot1") },
            { "MWL_Ruins2_Configuration", new LocationConfiguration(Config, "MWL_Ruins2", 10, "MeadowsCreatures1", "MeadowsLoot2") },
            { "MWL_Ruins3_Configuration", new LocationConfiguration(Config, "MWL_Ruins3", 25, "MeadowsCreatures1", "MeadowsLoot1") },
            { "MWL_Ruins6_Configuration", new LocationConfiguration(Config, "MWL_Ruins6", 5, "MeadowsCreatures2", "MeadowsLoot3") },
            { "MWL_Ruins7_Configuration", new LocationConfiguration(Config, "MWL_Ruins7", 2, "MeadowsCreatures2", "MeadowsLoot3") },
            { "MWL_Ruins8_Configuration", new LocationConfiguration(Config, "MWL_Ruins8", 5, "MeadowsCreatures2", "MeadowsLoot1") },
            { "MWL_RuinsArena1_Configuration", new LocationConfiguration(Config, "MWL_RuinsArena1", 25, "MeadowsCreatures1", "MeadowsLoot3") },
            { "MWL_RuinsArena3_Configuration", new LocationConfiguration(Config, "MWL_RuinsArena3", 25, "MeadowsCreatures1", "MeadowsLoot2") },
            { "MWL_RuinsChurch1_Configuration", new LocationConfiguration(Config, "MWL_RuinsChurch1", 25, "MeadowsCreatures1", "MeadowsLoot2") },
            { "MWL_RuinsWell1_Configuration", new LocationConfiguration(Config, "MWL_RuinsWell1", 5, "MeadowsCreatures1", "MeadowsLoot1") },
    
            // Meadows Pack 2
            { "MWL_DeerShrine1_Configuration", new LocationConfiguration(Config, "MWL_DeerShrine1", 10, "MeadowsCreatures1", "MeadowsLoot1") },
            { "MWL_DeerShrine2_Configuration", new LocationConfiguration(Config, "MWL_DeerShrine2", 10, "MeadowsCreatures1", "MeadowsLoot3") },
            { "MWL_MeadowsBarn1_Configuration", new LocationConfiguration(Config, "MWL_MeadowsBarn1", 5, "MeadowsCreatures5", "MeadowsLoot1") },
            { "MWL_MeadowsHouse2_Configuration", new LocationConfiguration(Config, "MWL_MeadowsHouse2", 20, "MeadowsCreatures2", "MeadowsLoot3") },
            { "MWL_MeadowsRuin1_Configuration", new LocationConfiguration(Config, "MWL_MeadowsRuin1", 5, "MeadowsCreatures1", "MeadowsLoot1") },
            { "MWL_MeadowsTomb4_Configuration", new LocationConfiguration(Config, "MWL_MeadowsTomb4", 10, "MeadowsCreatures1", "MeadowsLoot1") },
            { "MWL_MeadowsTower1_Configuration", new LocationConfiguration(Config, "MWL_MeadowsTower1", 15, "MeadowsCreatures1", "MeadowsLoot1") },
            { "MWL_OakHut1_Configuration", new LocationConfiguration(Config, "MWL_OakHut1", 15, "MeadowsCreatures1", "MeadowsLoot1") },
            { "MWL_SmallHouse1_Configuration", new LocationConfiguration(Config, "MWL_SmallHouse1", 20, "MeadowsCreatures1", "MeadowsLoot1") },
            { "MWL_MeadowsFarm1_Configuration", new LocationConfiguration(Config, "MWL_MeadowsFarm1", 10, "MeadowsCreatures1", "MeadowsLoot3") },
            { "MWL_MeadowsLighthouse1_Configuration", new LocationConfiguration(Config, "MWL_MeadowsLighthouse1", 10, "MeadowsCreatures1", "MeadowsLoot3") },
            { "MWL_MeadowsSawmill1_Configuration", new LocationConfiguration(Config, "MWL_MeadowsSawmill1", 10, "MeadowsCreatures1", "MeadowsLoot3") },
            { "MWL_MeadowsWall1_Configuration", new LocationConfiguration(Config, "MWL_MeadowsWall1", 10, "MeadowsCreatures1", "MeadowsLoot3") },
            { "MWL_MeadowsTavern1_Configuration", new LocationConfiguration(Config, "MWL_MeadowsTavern1", 10, "MeadowsCreatures1", "MeadowsLoot3") },
            
            // Blackforest Pack 1
            { "MWL_RuinsArena2_Configuration", new LocationConfiguration(Config, "MWL_RuinsArena2", 5, "BlackforestCreatures3", "BlackforestLoot3") },
            { "MWL_RuinsCastle1_Configuration", new LocationConfiguration(Config, "MWL_RuinsCastle1", 15, "BlackforestCreatures2", "BlackforestLoot1") },
            { "MWL_RuinsCastle3_Configuration", new LocationConfiguration(Config, "MWL_RuinsCastle3", 5, "BlackforestCreatures2", "BlackforestLoot2") },
            { "MWL_RuinsTower3_Configuration", new LocationConfiguration(Config, "MWL_RuinsTower3", 15, "BlackforestCreatures1", "BlackforestLoot2") },
            { "MWL_RuinsTower8_Configuration", new LocationConfiguration(Config, "MWL_RuinsTower8", 10, "BlackforestCreatures1", "BlackforestLoot2") },
            { "MWL_Tavern1_Configuration", new LocationConfiguration(Config, "MWL_Tavern1", 15, "BlackforestCreatures2", "WoodTavernLoot1") },
            { "MWL_WoodTower1_Configuration", new LocationConfiguration(Config, "MWL_WoodTower1", 10, "BlackforestCreatures1", "BlackforestLoot1") },
            { "MWL_WoodTower2_Configuration", new LocationConfiguration(Config, "MWL_WoodTower2", 10, "BlackforestCreatures2", "BlackforestLoot1") },
            { "MWL_WoodTower3_Configuration", new LocationConfiguration(Config, "MWL_WoodTower3", 10, "BlackforestCreatures3", "BlackforestLoot3") },
            { "MWL_RuinsTower6_Configuration", new LocationConfiguration(Config, "MWL_RuinsTower6", 15, "BlackforestCreatures1", "BlackforestLoot3") },
            
            // Blackforest Pack 2
            { "MWL_ForestForge1_Configuration", new LocationConfiguration(Config, "ForestForge1", 20, "BlackforestCreatures3", "BlackforestLoot3") },
            { "MWL_ForestForge2_Configuration", new LocationConfiguration(Config, "ForestForge2", 20, "BlackforestCreatures3", "BlackforestLoot3") },
            { "MWL_ForestGreatHouse2_Configuration", new LocationConfiguration(Config, "ForestGreatHouse2", 20, "BlackforestCreatures3", "BlackforestLoot3") },
            { "MWL_ForestHouse2_Configuration", new LocationConfiguration(Config, "ForestHouse2", 20, "BlackforestCreatures3", "BlackforestLoot3") },
            { "MWL_ForestRuin1_Configuration", new LocationConfiguration(Config, "ForestRuin1", 20, "BlackforestCreatures3", "BlackforestLoot1") },
            { "MWL_ForestTower2_Configuration", new LocationConfiguration(Config, "ForestTower2", 20, "BlackforestCreatures1", "BlackforestLoot3") },
            { "MWL_ForestTower3_Configuration", new LocationConfiguration(Config, "ForestTower3", 20, "BlackforestCreatures1", "BlackforestLoot3") },
            { "MWL_MassGrave1_Configuration", new LocationConfiguration(Config, "MassGrave1", 15, "BlackforestCreatures3", "BlackforestLoot1") },
            { "MWL_StoneFormation1_Configuration", new LocationConfiguration(Config, "StoneFormation1", 15, "BlackforestCreatures3", "BlackforestLoot3") },
            { "MWL_GuardTower1_Configuration", new LocationConfiguration(Config, "GuardTower1", 5, "BlackforestCreatures1", "BlackforestLoot3") },
            { "MWL_RootRuins1_Configuration", new LocationConfiguration(Config, "RootRuins1", 15, "BlackforestCreatures3", "BlackforestLoot1") },
            { "MWL_RootsTower1_Configuration", new LocationConfiguration(Config, "RootsTower1", 20, "BlackforestCreatures3", "BlackforestLoot2") },
            { "MWL_RootsTower2_Configuration", new LocationConfiguration(Config, "RootsTower2", 10, "BlackforestCreatures3", "BlackforestLoot2") },
            { "MWL_StoneOutlook1_Configuration", new LocationConfiguration(Config, "StoneOutlook1", 10, "BlackforestCreatures3", "BlackforestLoot1") },
            { "MWL_ForestRuin2_Configuration", new LocationConfiguration(Config, "ForestRuin2", 15, "BlackforestCreatures2", "BlackforestLoot3") },
            { "MWL_ForestRuin3_Configuration", new LocationConfiguration(Config, "ForestRuin3", 15, "BlackforestCreatures3", "BlackforestLoot1") },
            { "MWL_ForestSkull1_Configuration", new LocationConfiguration(Config, "ForestSkull1", 15, "BlackforestCreatures1", "BlackforestLoot3") },
            { "MWL_ForestTower4_Configuration", new LocationConfiguration(Config, "ForestTower4", 15, "BlackforestCreatures3", "BlackforestLoot2") },
            { "MWL_ForestTower5_Configuration", new LocationConfiguration(Config, "ForestTower5", 15, "BlackforestCreatures3", "BlackforestLoot1") },
            { "MWL_ForestPillar1_Configuration", new LocationConfiguration(Config, "ForestPillar1", 15, "BlackforestCreatures3", "BlackforestLoot1") },
            { "MWL_CoastTower1_Configuration", new LocationConfiguration(Config, "CoastTower1", 15, "BlackforestCreatures1", "BlackforestLoot1") },
    
            // Swamp Pack 1
            { "MWL_GuckPit1_Configuration", new LocationConfiguration(Config, "GuckPit1", 15, "SwampCreatures1", "SwampLoot1") },
            { "MWL_SwampAltar1_Configuration", new LocationConfiguration(Config, "SwampAltar1", 15, "SwampCreatures3", "SwampLoot1") },
            { "MWL_SwampAltar2_Configuration", new LocationConfiguration(Config, "SwampAltar2", 10, "SwampCreatures3", "SwampLoot2") },
            { "MWL_SwampAltar3_Configuration", new LocationConfiguration(Config, "SwampAltar3", 10, "SwampCreatures3", "SwampLoot2") },
            { "MWL_SwampAltar4_Configuration", new LocationConfiguration(Config, "SwampAltar4", 10, "SwampCreatures2", "SwampLoot2") },
            { "MWL_SwampCastle2_Configuration", new LocationConfiguration(Config, "SwampCastle2", 10, "SwampCreatures2", "SwampLoot3") },
            { "MWL_SwampGrave1_Configuration", new LocationConfiguration(Config, "SwampGrave1", 25, "SwampCreatures2", "SwampLoot1") },
            { "MWL_SwampHouse1_Configuration", new LocationConfiguration(Config, "SwampHouse1", 20, "SwampCreatures3", "SwampLoot1") },
            { "MWL_SwampRuin1_Configuration", new LocationConfiguration(Config, "SwampRuin1", 25, "SwampCreatures3", "SwampLoot1") },
            { "MWL_SwampTower1_Configuration", new LocationConfiguration(Config, "SwampTower1", 20, "SwampCreatures3", "SwampLoot1") },
            { "MWL_SwampTower2_Configuration", new LocationConfiguration(Config, "SwampTower2", 25, "SwampCreatures4", "SwampLoot3") },
            { "MWL_SwampTower3_Configuration", new LocationConfiguration(Config, "SwampTower3", 25, "SwampCreatures2", "SwampLoot1") },
            { "MWL_SwampWell1_Configuration", new LocationConfiguration(Config, "SwampWell1", 20, "SwampCreatures1", "SwampLoot1") },
            { "MWL_AbandonedHouse1_Configuration", new LocationConfiguration(Config, "AbandonedHouse1", 15, "SwampCreatures3", "SwampLoot1") },
            { "MWL_Treehouse1_Configuration", new LocationConfiguration(Config, "Treehouse1", 20, "SwampCreatures1", "SwampLoot1") },
            { "MWL_Shipyard1_Configuration", new LocationConfiguration(Config, "Shipyard1", 20, "SwampCreatures2", "SwampLoot1") },
            { "MWL_FortBakkarhalt1_Configuration", new LocationConfiguration(Config, "FortBakkarhalt1", 5, "SwampCreatures2", "SwampLoot1") },
            { "MWL_Belmont1_Configuration", new LocationConfiguration(Config, "Belmont1", 5, "SwampCreatures2", "SwampLoot1") },
            { "MWL_SwampCourtyard1_Configuration", new LocationConfiguration(Config, "SwampCourtyard1", 5, "SwampCreatures2", "SwampLoot1") },
            { "MWL_SwampBrokenTower1_Configuration", new LocationConfiguration(Config, "SwampBrokenTower1", 15, "SwampCreatures2", "SwampLoot1") },
            { "MWL_SwampBrokenTower3_Configuration", new LocationConfiguration(Config, "SwampBrokenTower3", 10, "SwampCreatures4", "SwampLoot3") },
    
            // Mountain Pack 1
            { "MWL_StoneCastle1_Configuration", new LocationConfiguration(Config, "StoneCastle1", 5, "MountainsCreatures1", "MountainsLoot1") },
            { "MWL_StoneFort1_Configuration", new LocationConfiguration(Config, "StoneFort1", 10, "MountainsCreatures1", "MountainsLoot1") },
            { "MWL_StoneHall1_Configuration", new LocationConfiguration(Config, "StoneHall1", 10, "MountainsCreatures4", "MountainsLoot1") },
            { "MWL_StoneTavern1_Configuration", new LocationConfiguration(Config, "StoneTavern1", 10, "MountainsCreatures1", "MountainsLoot1") },
            { "MWL_StoneTower1_Configuration", new LocationConfiguration(Config, "StoneTower1", 10, "MountainsCreatures2", "MountainsLoot1") },
            { "MWL_StoneTower2_Configuration", new LocationConfiguration(Config, "StoneTower2", 10, "MountainsCreatures2", "MountainsLoot1") },
            { "MWL_WoodBarn1_Configuration", new LocationConfiguration(Config, "WoodBarn1", 10, "MountainsCreatures1", "MountainsLoot1") },
            { "MWL_WoodFarm1_Configuration", new LocationConfiguration(Config, "WoodFarm1", 10, "MountainsCreatures3", "MountainsLoot1") },
            { "MWL_WoodHouse1_Configuration", new LocationConfiguration(Config, "WoodHouse1", 5, "MountainsCreatures1", "MountainsLoot1") },
            { "MWL_TempleShrine1_Configuration", new LocationConfiguration(Config, "TempleShrine1", 5, "MountainsCreatures1", "MountainsLoot1") },
            { "MWL_StoneAltar1_Configuration", new LocationConfiguration(Config, "StoneAltar1", 15, "MountainsCreatures1", "MountainsLoot1") },
    
            // Plains Pack 1
            { "MWL_GoblinFort1_Configuration", new LocationConfiguration(Config, "GoblinFort1", 10, "PlainsCreatures2", "PlainsLoot1") },
            { "MWL_FulingRock1_Configuration", new LocationConfiguration(Config, "FulingRock1", 15, "PlainsCreatures3", "PlainsLoot2") },
            { "MWL_FulingVillage1_Configuration", new LocationConfiguration(Config, "FulingVillage1", 15, "PlainsCreatures3", "PlainsLoot1") },
            { "MWL_FulingVillage2_Configuration", new LocationConfiguration(Config, "FulingVillage2", 15, "PlainsCreatures2", "PlainsLoot1") },
            { "MWL_PlainsPillar1_Configuration", new LocationConfiguration(Config, "PlainsPillar1", 15, "PlainsCreatures1", "PlainsLoot1") },
            { "MWL_FulingTemple1_Configuration", new LocationConfiguration(Config, "FulingTemple1", 15, "PlainsCreatures2", "PlainsLoot1") },
            { "MWL_FulingTemple2_Configuration", new LocationConfiguration(Config, "FulingTemple2", 20, "PlainsCreatures1", "PlainsLoot1") },
            { "MWL_FulingTemple3_Configuration", new LocationConfiguration(Config, "FulingTemple3", 20, "PlainsCreatures1", "PlainsLoot1") },
            { "MWL_FulingWall1_Configuration", new LocationConfiguration(Config, "FulingWall1", 20, "PlainsCreatures3", "PlainsLoot1") },
            { "MWL_FulingTower1_Configuration", new LocationConfiguration(Config, "FulingTower1", 20, "PlainsCreatures3", "PlainsLoot1") },
            // { "MWL_GoblinCave1_Configuration", new LocationConfiguration(Config, "GoblinCave1", 20, "PlainsCreatures2", "PlainsLoot1") },
            { "MWL_RockGarden1_Configuration", new LocationConfiguration(Config, "RockGarden1", 15, "PlainsCreatures3", "PlainsLoot1") },
            
            // Mistlands Pack 1
            { "MWL_MistFort2_Configuration", new LocationConfiguration(Config, "MistFort2", 20, "MistCreatures1", "MistLoot1") },
            { "MWL_SecretRoom1_Configuration", new LocationConfiguration(Config, "SecretRoom1", 15, "MistCreatures2", "MistLoot2") },
            { "MWL_MistWorkshop1_Configuration", new LocationConfiguration(Config, "MistWorkshop1", 25, "MistCreatures2", "MistLoot2") },
            { "MWL_MistTower1_Configuration", new LocationConfiguration(Config, "MistTower1", 20, "MistCreatures1", "MistLoot1") },
            { "MWL_MistWall1_Configuration", new LocationConfiguration(Config, "MistWall1", 15, "MistCreatures2", "MistLoot2") },
            { "MWL_MistTower2_Configuration", new LocationConfiguration(Config, "MistTower2", 25, "MistCreatures1", "MistLoot1") },
            { "MWL_MistHut1_Configuration", new LocationConfiguration(Config, "MistHut1", 25, "MistCreatures1", "MistLoot1") },
            { "MWL_DvergrEitrSingularity1_Configuration", new LocationConfiguration(Config, "DvergrEitrSingularity1", 25, "MistCreatures1", "MistLoot1") },
            { "MWL_DvergrHouse1_Configuration", new LocationConfiguration(Config, "DvergrHouse1", 20, "PlainsCreatures3", "MistLoot1") },
            { "MWL_DvergrKnowledgeExtractor1_Configuration", new LocationConfiguration(Config, "DvergrKnowledgeExtractor1", 15, "MistCreatures1", "MistLoot1") },
            { "MWL_AncientShrine1_Configuration", new LocationConfiguration(Config, "AncientShrine1", 15, "MistCreatures1", "MistLoot1") },
            { "MWL_MistShrine1_Configuration", new LocationConfiguration(Config, "MistShrine1", 15, "MistCreatures1", "MistLoot1") },
    
            // Ashlands Pack 1
            { "MWL_AshlandsFort1_Configuration", new LocationConfiguration(Config, "MWL_AshlandsFort1", 5, "AshlandsCreatures1", "AshlandsLoot1") },
            { "MWL_AshlandsFort2_Configuration", new LocationConfiguration(Config, "MWL_AshlandsFort2", 5, "AshlandsCreatures1", "AshlandsLoot1") },
            { "MWL_AshlandsFort3_Configuration", new LocationConfiguration(Config, "MWL_AshlandsFort3", 5, "AshlandsCreatures1", "AshlandsLoot1") },
    
            // Adventure Map Pack 1
            { "MWL_CastleCorner1_Configuration", new LocationConfiguration(Config, "CastleCorner1", 15, "SwampCreatures1", "SwampLoot1") },
            { "MWL_ForestCamp1_Configuration", new LocationConfiguration(Config, "ForestCamp1", 20, "BlackforestCreatures1", "BlackforestLoot1") },
            { "MWL_Misthut2_Configuration", new LocationConfiguration(Config, "Misthut2", 15, "MistCreatures1", "MistLoot1") },
            { "MWL_MountainDvergrShrine1_Configuration", new LocationConfiguration(Config, "MountainDvergrShrine1", 15, "MistCreatures1", "MistLoot1") },
            { "MWL_MountainShrine1_Configuration", new LocationConfiguration(Config, "MountainShrine1", 20, "MountainCreatures1", "MountainsLoot1") },
            { "MWL_RuinedTower1_Configuration", new LocationConfiguration(Config, "RuinedTower1", 15, "BlackforestCreatures2", "BlackforestLoot1") },
            { "MWL_TreeTowers1_Configuration", new LocationConfiguration(Config, "TreeTowers1", 20, "SwampCreatures1", "SwampLoot1") },
    
        };
    }
}
