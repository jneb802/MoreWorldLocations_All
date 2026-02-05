using BepInEx;
using Jotunn.Managers;
using UnityEngine;
using System;
using System.Linq;

namespace More_World_Locations_AIO;

public class LocationsNEW
{
    public static void AddAllLocations()
    {
        AddMeadowsPack1Locations();
        AddMeadowsPack2Locations();
        AddBlackForestPack1Locations();
        AddBlackForestPack2Locations();
        AddSwampPack1Locations();
        AddMountainsPack1Locations();
        AddPlainsPack1Locations();
        AddMistlandsPack1Locations();
        AddAshlandsPack1Locations();
        AddAdventureMapPack1Locations();
        AddPortLocations();
        AddTraderLocations();
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }

    public static void AddMeadowsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_Ruins1", LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins1_Config"]);
        Common.LocationManager.AddLocation("MWL_Ruins2", LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins2_Config"]);
        Common.LocationManager.AddLocation("MWL_Ruins3", LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins3_Config"]);
        Common.LocationManager.AddLocation("MWL_Ruins6", LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins6_Config"]);
        Common.LocationManager.AddLocation("MWL_Ruins7", LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins7_Config"]);
        Common.LocationManager.AddLocation("MWL_Ruins8", LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins8_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsArena1", LocationConfigs.MeadowsPack1LocationConfigs["MWL_RuinsArena1_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsArena3", LocationConfigs.MeadowsPack1LocationConfigs["MWL_RuinsArena3_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsChurch1", LocationConfigs.MeadowsPack1LocationConfigs["MWL_RuinsChurch1_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsWell1", LocationConfigs.MeadowsPack1LocationConfigs["MWL_RuinsWell1_Config"]);
        Common.LocationManager.AddLocation("MWL_MaypoleHut1", LocationConfigs.MeadowsPack1LocationConfigs["MWL_MaypoleHut1_Config"]);
    }
    
    public static void AddMeadowsPack2Locations()
    { 
        Common.LocationManager.AddLocation("MWL_DeerShrine1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_DeerShrine1_Config"]);
        Common.LocationManager.AddLocation("MWL_DeerShrine2", LocationConfigs.MeadowsPack2LocationConfigs["MWL_DeerShrine2_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsBarn1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsBarn1_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsHouse2", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsHouse2_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsRuin1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsRuin1_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsTomb4", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsTomb4_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsTower1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_OakHut1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_OakHut1_Config"]);
        Common.LocationManager.AddLocation("MWL_SmallHouse1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_SmallHouse1_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsFarm1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsFarm1_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsLighthouse1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsLighthouse1_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsSawmill1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsSawmill1_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsWall1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsWall1_Config"]);
        Common.LocationManager.AddLocation("MWL_MeadowsTavern1", LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsTavern1_Config"]);
    }

    
    public static void AddBlackForestPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_RuinsArena2", LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsArena2_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsCastle1", LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsCastle1_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsCastle3", LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsCastle3_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsTower3", LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsTower3_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsTower6", LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsTower6_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsTower8", LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsTower8_Config"]);
        Common.LocationManager.AddLocation("MWL_Tavern1", LocationConfigs.BlackforestPack1LocationConfigs["MWL_Tavern1_Config"]);
        Common.LocationManager.AddLocation("MWL_WoodTower1", LocationConfigs.BlackforestPack1LocationConfigs["MWL_WoodTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_WoodTower2", LocationConfigs.BlackforestPack1LocationConfigs["MWL_WoodTower2_Config"]);
        Common.LocationManager.AddLocation("MWL_WoodTower3", LocationConfigs.BlackforestPack1LocationConfigs["MWL_WoodTower3_Config"]);
    }

    
    public static void AddBlackForestPack2Locations()
    {
        Common.LocationManager.AddLocation("MWL_ForestForge1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestForge1_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestForge2", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestForge2_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestGreatHouse2", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestGreatHouse2_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestHouse2", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestHouse2_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestRuin1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestRuin1_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestTower2", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestTower2_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestTower3", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestTower3_Config"]);
        Common.LocationManager.AddLocation("MWL_MassGrave1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_MassGrave1_Config"]);
        Common.LocationManager.AddLocation("MWL_StoneFormation1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_StoneFormation1_Config"]);
        Common.LocationManager.AddLocation("MWL_GuardTower1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_GuardTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_RootRuins1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_RootRuins1_Config"]);
        Common.LocationManager.AddLocation("MWL_RootsTower1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_RootsTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_RootsTower2", LocationConfigs.BlackforestPack2LocationConfigs["MWL_RootsTower2_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestRuin2", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestRuin2_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestRuin3", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestRuin3_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestSkull1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestSkull1_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestTower4", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestTower4_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestTower5", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestTower5_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestPillar1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestPillar1_Config"]);
        Common.LocationManager.AddLocation("MWL_CoastTower1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_CoastTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestGrove1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestGrove1_Config"]);
        Common.LocationManager.AddLocation("MWL_RockShrine1", LocationConfigs.BlackforestPack2LocationConfigs["MWL_RockShrine1_Config"]);
    }

    
    public static void AddSwampPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_GuckPit1", LocationConfigs.SwampPack1LocationConfigs["MWL_GuckPit1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampAltar1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampAltar1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampAltar2", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampAltar2_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampAltar3", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampAltar3_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampAltar4", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampAltar4_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampCastle2", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampCastle2_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampGrave1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampGrave1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampHouse1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampHouse1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampRuin1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampRuin1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampTower1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampTower2", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampTower2_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampTower3", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampTower3_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampWell1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampWell1_Config"]);
        Common.LocationManager.AddLocation("MWL_AbandonedHouse1", LocationConfigs.SwampPack1LocationConfigs["MWL_AbandonedHouse1_Config"]);
        Common.LocationManager.AddLocation("MWL_Treehouse1", LocationConfigs.SwampPack1LocationConfigs["MWL_Treehouse1_Config"]);
        Common.LocationManager.AddLocation("MWL_Shipyard1", LocationConfigs.SwampPack1LocationConfigs["MWL_Shipyard1_Config"]);
        Common.LocationManager.AddLocation("MWL_FortBakkarhalt1", LocationConfigs.SwampPack1LocationConfigs["MWL_FortBakkarhalt1_Config"]);
        Common.LocationManager.AddLocation("MWL_Belmont1", LocationConfigs.SwampPack1LocationConfigs["MWL_Belmont1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampCourtyard1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampCourtyard1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampBrokenTower1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampBrokenTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampBrokenTower3", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampBrokenTower3_Config"]);
        Common.LocationManager.AddLocation("MWL_StoneCircle1", LocationConfigs.SwampPack1LocationConfigs["MWL_StoneCircle1_Config"]);
        Common.LocationManager.AddLocation("MWL_SwampTemple1", LocationConfigs.SwampPack1LocationConfigs["MWL_SwampTemple1_Config"]);
    }
    
    public static void AddMountainsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_StoneCastle1", LocationConfigs.MountainPack1LocationConfigs["MWL_StoneCastle1_Config"]);
        Common.LocationManager.AddLocation("MWL_StoneFort1", LocationConfigs.MountainPack1LocationConfigs["MWL_StoneFort1_Config"]);
        Common.LocationManager.AddLocation("MWL_StoneHall1", LocationConfigs.MountainPack1LocationConfigs["MWL_StoneHall1_Config"]);
        Common.LocationManager.AddLocation("MWL_StoneTavern1", LocationConfigs.MountainPack1LocationConfigs["MWL_StoneTavern1_Config"]);
        Common.LocationManager.AddLocation("MWL_StoneTower1", LocationConfigs.MountainPack1LocationConfigs["MWL_StoneTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_StoneTower2", LocationConfigs.MountainPack1LocationConfigs["MWL_StoneTower2_Config"]);
        Common.LocationManager.AddLocation("MWL_WoodBarn1", LocationConfigs.MountainPack1LocationConfigs["MWL_WoodBarn1_Config"]);
        Common.LocationManager.AddLocation("MWL_WoodFarm1", LocationConfigs.MountainPack1LocationConfigs["MWL_WoodFarm1_Config"]);
        Common.LocationManager.AddLocation("MWL_WoodHouse1", LocationConfigs.MountainPack1LocationConfigs["MWL_WoodHouse1_Config"]);
        Common.LocationManager.AddLocation("MWL_TempleShrine1", LocationConfigs.MountainPack1LocationConfigs["MWL_TempleShrine1_Config"]);
        Common.LocationManager.AddLocation("MWL_StoneAltar1", LocationConfigs.MountainPack1LocationConfigs["MWL_StoneAltar1_Config"]);
    }

    
    public static void AddPlainsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_GoblinFort1", LocationConfigs.PlainsPack1LocationConfigs["MWL_GoblinFort1_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingRock1", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingRock1_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingVillage1", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingVillage1_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingVillage2", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingVillage2_Config"]);
        Common.LocationManager.AddLocation("MWL_PlainsPillar1", LocationConfigs.PlainsPack1LocationConfigs["MWL_PlainsPillar1_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingTemple1", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTemple1_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingTemple2", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTemple2_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingTemple3", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTemple3_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingTempleBroken1", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTempleBroken1_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingTemple4", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTemple4_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingWall1", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingWall1_Config"]);
        Common.LocationManager.AddLocation("MWL_FulingTower1", LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_RockGarden1", LocationConfigs.PlainsPack1LocationConfigs["MWL_RockGarden1_Config"]);
    }
    
    public static void AddMistlandsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_MistFort2", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistFort2_Config"]);
        Common.LocationManager.AddLocation("MWL_SecretRoom1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_SecretRoom1_Config"]);
        Common.LocationManager.AddLocation("MWL_MistWorkshop1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistWorkshop1_Config"]);
        Common.LocationManager.AddLocation("MWL_MistTower1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_MistWall1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistWall1_Config"]);
        Common.LocationManager.AddLocation("MWL_MistTower2", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistTower2_Config"]);
        Common.LocationManager.AddLocation("MWL_MistHut1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistHut1_Config"]);
        Common.LocationManager.AddLocation("MWL_DvergrEitrSingularity1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_DvergrEitrSingularity1_Config"]);
        Common.LocationManager.AddLocation("MWL_DvergrHouse1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_DvergrHouse1_Config"]);
        Common.LocationManager.AddLocation("MWL_DvergrHouseWood1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_DvergrHouseWood1_Config"]);
        Common.LocationManager.AddLocation("MWL_DvergrHouseWood2", LocationConfigs.MistlandsPack1LocationConfigs["MWL_DvergrHouseWood2_Config"]);
        Common.LocationManager.AddLocation("MWL_MarbleJail1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MarbleJail1_Config"]);
        Common.LocationManager.AddLocation("MWL_MarbleHome1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MarbleHome1_Config"]);
        Common.LocationManager.AddLocation("MWL_MarbleHome2", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MarbleHome2_Config"]);
        Common.LocationManager.AddLocation("MWL_MarbleCliffAltar1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MarbleCliffAltar1_Config"]);
        Common.LocationManager.AddLocation("MWL_MistPond1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistPond1_Config"]);
        Common.LocationManager.AddLocation("MWL_MarbleCage1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MarbleCage1_Config"]);
        Common.LocationManager.AddLocation("MWL_DvergrKnowledgeExtractor1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_DvergrKnowledgeExtractor1_Config"]);
        Common.LocationManager.AddLocation("MWL_AncientShrine1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_AncientShrine1_Config"]);
        Common.LocationManager.AddLocation("MWL_MistShrine1", LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistShrine1_Config"]);
    }

    
    public static void AddAshlandsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_AshlandsFort1", LocationConfigs.AshlandsPack1LocationConfigs["MWL_AshlandsFort1_Config"]);
        Common.LocationManager.AddLocation("MWL_AshlandsFort2", LocationConfigs.AshlandsPack1LocationConfigs["MWL_AshlandsFort2_Config"]);
        Common.LocationManager.AddLocation("MWL_AshlandsFort3", LocationConfigs.AshlandsPack1LocationConfigs["MWL_AshlandsFort3_Config"]);
    }

    
    public static void AddAdventureMapPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_CastleCorner1", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_CastleCorner1_Config"]);
        Common.LocationManager.AddLocation("MWL_ForestCamp1", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_ForestCamp1_Config"]);
        Common.LocationManager.AddLocation("MWL_MistHut2", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MistHut2_Config"]);
        Common.LocationManager.AddLocation("MWL_MountainDvergrShrine1", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MountainDvergrShrine1_Config"]);
        Common.LocationManager.AddLocation("MWL_MountainDvergrShrine2", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MountainDvergrShrine2_Config"]);
        Common.LocationManager.AddLocation("MWL_MountainOverlook1", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MountainOverlook1_Config"]);
        Common.LocationManager.AddLocation("MWL_MountainCultShrine1", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MountainCultShrine1_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinsChurch2", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_RuinsChurch2_Config"]);
        Common.LocationManager.AddLocation("MWL_MountainShrine1", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MountainShrine1_Config"]);
        Common.LocationManager.AddLocation("MWL_RuinedTower1", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_RuinedTower1_Config"]);
        Common.LocationManager.AddLocation("MWL_TreeTowers1", LocationConfigs.AdventureMapPack1LocationConfigs["MWL_TreeTowers1_Config"]);
    
        //Common.LocationManager.AddLocation("Location1", LocationConfigs.AdventureMapPack1LocationConfigs["Location1_Config"]);
    }
    
    public static void AddPortLocations()
    {
        // Skip if port locations are disabled via config
        if (PortInit.EnablePortLocations.Value == PortInit.Toggle.Off) return;
        
        Common.LocationManager.AddLocation("MWL_Port1", LocationConfigs.PortLocationConfigs["MWL_Port1_Config"]);
        Common.LocationManager.AddLocation("MWL_Port2", LocationConfigs.PortLocationConfigs["MWL_Port2_Config"]);
        Common.LocationManager.AddLocation("MWL_Port3", LocationConfigs.PortLocationConfigs["MWL_Port3_Config"]);
        Common.LocationManager.AddLocation("MWL_Port4", LocationConfigs.PortLocationConfigs["MWL_Port4_Config"]);
    }
    
    public static void AddTraderLocations()
    {
        Common.LocationManager.AddLocation("MWL_PlainsTavern1", LocationConfigs.TraderLocationConfigs["MWL_PlainsTavern1_Config"]);
        //Common.LocationManager.AddLocation("MWL_OceanTavern1", LocationConfigs.TraderLocationConfigs["MWL_OceanTavern1_Config"]);
        Common.LocationManager.AddLocation("MWL_PlainsCamp1", LocationConfigs.TraderLocationConfigs["MWL_PlainsCamp1_Config"]);
        Common.LocationManager.AddLocation("MWL_BlackForestBlacksmith1", LocationConfigs.TraderLocationConfigs["MWL_BlackForestBlacksmith1_Config"]);
        Common.LocationManager.AddLocation("MWL_BlackForestBlacksmith2", LocationConfigs.TraderLocationConfigs["MWL_BlackForestBlacksmith2_Config"]);
        Common.LocationManager.AddLocation("MWL_MountainsBlacksmith1", LocationConfigs.TraderLocationConfigs["MWL_MountainsBlacksmith1_Config"]);
        Common.LocationManager.AddLocation("MWL_MistlandsBlacksmith1", LocationConfigs.TraderLocationConfigs["MWL_MistlandsBlacksmith1_Config"]);
    }
}