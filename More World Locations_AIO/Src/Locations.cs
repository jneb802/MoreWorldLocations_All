using BepInEx;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO;

public class Locations
{
    public static void AddAllLocations()
    {
        //AddMeadowsPack1Locations();
        //AddMeadowsPack2Locations();
        //AddBlackForestPack1Locations();
        AddBlackForestPack2Locations();
        //AddSwampPack1Locations();
        //AddMountainsPack1Locations();
        //AddPlainsPack1Locations();
        //AddMistlandsPack1Locations();
        //AddAshlandsPack1Locations();
        //AddAdventureMapPack1Locations();
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }

    public static void AddMeadowsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_Ruins1", BepinexConfigs.MWL_Ruins1_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Ruins2", BepinexConfigs.MWL_Ruins2_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Ruins3", BepinexConfigs.MWL_Ruins3_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Ruins6", BepinexConfigs.MWL_Ruins6_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins6_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Ruins7", BepinexConfigs.MWL_Ruins7_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins7_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Ruins8", BepinexConfigs.MWL_Ruins8_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_Ruins8_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsArena1", BepinexConfigs.MWL_RuinsArena1_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_RuinsArena1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsArena3", BepinexConfigs.MWL_RuinsArena3_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_RuinsArena3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsChurch1", BepinexConfigs.MWL_RuinsChurch1_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_RuinsChurch1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsWell1", BepinexConfigs.MWL_RuinsWell1_Configuration, LocationConfigs.MeadowsPack1LocationConfigs["MWL_RuinsWell1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }
    
    public static void AddMeadowsPack2Locations()
    { 
       Common.LocationManager.AddLocation("MWL_DeerShrine1", BepinexConfigs.MWL_DeerShrine1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_DeerShrine1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_DeerShrine2", BepinexConfigs.MWL_DeerShrine2_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_DeerShrine2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsBarn1", BepinexConfigs.MWL_MeadowsBarn1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsBarn1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsHouse2", BepinexConfigs.MWL_MeadowsHouse2_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsHouse2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsRuin1", BepinexConfigs.MWL_MeadowsRuin1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsRuin1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsTomb4", BepinexConfigs.MWL_MeadowsTomb4_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsTomb4_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsTower1", BepinexConfigs.MWL_MeadowsTower1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_OakHut1", BepinexConfigs.MWL_OakHut1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_OakHut1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_SmallHouse1", BepinexConfigs.MWL_SmallHouse1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_SmallHouse1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       
       Common.LocationManager.AddLocation("MWL_MeadowsFarm1", BepinexConfigs.MWL_MeadowsFarm1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsFarm1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsLighthouse1", BepinexConfigs.MWL_MeadowsLighthouse1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsLighthouse1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsSawmill1", BepinexConfigs.MWL_MeadowsSawmill1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsSawmill1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsWall1", BepinexConfigs.MWL_MeadowsWall1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsWall1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
       Common.LocationManager.AddLocation("MWL_MeadowsTavern1", BepinexConfigs.MWL_MeadowsTavern1_Configuration, LocationConfigs.MeadowsPack2LocationConfigs["MWL_MeadowsTavern1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }
    
    public static void AddBlackForestPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_RuinsArena2", BepinexConfigs.MWL_RuinsArena2_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsArena2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsCastle1", BepinexConfigs.MWL_RuinsCastle1_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsCastle1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsCastle3", BepinexConfigs.MWL_RuinsCastle3_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsCastle3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsTower3", BepinexConfigs.MWL_RuinsTower3_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsTower3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsTower6", BepinexConfigs.MWL_RuinsTower6_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsTower6_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinsTower8", BepinexConfigs.MWL_RuinsTower8_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_RuinsTower8_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Tavern1", BepinexConfigs.MWL_RuinsTavern1_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_Tavern1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_WoodTower1", BepinexConfigs.MWL_WoodTower1_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_WoodTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_WoodTower2", BepinexConfigs.MWL_WoodTower2_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_WoodTower2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_WoodTower3", BepinexConfigs.MWL_WoodTower3_Configuration, LocationConfigs.BlackforestPack1LocationConfigs["MWL_WoodTower3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }
    
    public static void AddBlackForestPack2Locations()
    {
        Common.LocationManager.AddLocation("MWL_ForestForge1", BepinexConfigs.MWL_ForestForge1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestForge1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestForge2", BepinexConfigs.MWL_ForestForge2_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestForge2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestGreatHouse2", BepinexConfigs.MWL_ForestGreatHouse2_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestGreatHouse2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestHouse2", BepinexConfigs.MWL_ForestHouse2_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestHouse2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestRuin1", BepinexConfigs.MWL_ForestRuin1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestRuin1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestTower2", BepinexConfigs.MWL_ForestTower2_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestTower2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestTower3", BepinexConfigs.MWL_ForestTower3_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestTower3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MassGrave1", BepinexConfigs.MWL_MassGrave1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_MassGrave1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_StoneFormation1", BepinexConfigs.MWL_StoneFormation1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_StoneFormation1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_GuardTower1", BepinexConfigs.MWL_GuardTower1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_GuardTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RootRuins1", BepinexConfigs.MWL_RootRuins1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_RootRuins1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RootsTower1", BepinexConfigs.MWL_RootsTower1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_RootsTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RootsTower2", BepinexConfigs.MWL_RootsTower2_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_RootsTower2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestRuin2", BepinexConfigs.MWL_ForestRuin2_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestRuin2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestRuin3", BepinexConfigs.MWL_ForestRuin3_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestRuin3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestSkull1", BepinexConfigs.MWL_ForestSkull1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestSkull1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestTower4", BepinexConfigs.MWL_ForestTower4_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestTower4_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestTower5", BepinexConfigs.MWL_ForestTower5_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestTower5_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestPillar1", BepinexConfigs.MWL_ForestPillar1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_ForestPillar1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_CoastTower1", BepinexConfigs.MWL_CoastTower1_Configuration, LocationConfigs.BlackforestPack2LocationConfigs["MWL_CoastTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }
    
    public static void AddSwampPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_GuckPit1", BepinexConfigs.MWL_GuckPit1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_GuckPit1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampAltar1", BepinexConfigs.MWL_SwampAltar1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampAltar1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampAltar2", BepinexConfigs.MWL_SwampAltar2_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampAltar2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampAltar3", BepinexConfigs.MWL_SwampAltar3_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampAltar3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampAltar4", BepinexConfigs.MWL_SwampAltar4_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampAltar4_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampCastle2", BepinexConfigs.MWL_SwampCastle2_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampCastle2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampGrave1", BepinexConfigs.MWL_SwampGrave1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampGrave1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampHouse1", BepinexConfigs.MWL_SwampHouse1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampHouse1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampRuin1", BepinexConfigs.MWL_SwampRuin1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampRuin1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampTower1", BepinexConfigs.MWL_SwampTower1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampTower2", BepinexConfigs.MWL_SwampTower2_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampTower2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampTower3", BepinexConfigs.MWL_SwampTower3_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampTower3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampWell1", BepinexConfigs.MWL_SwampWell1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampWell1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_AbandonedHouse1", BepinexConfigs.MWL_AbandonedHouse1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_AbandonedHouse1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Treehouse1", BepinexConfigs.MWL_Treehouse1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_Treehouse1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Shipyard1", BepinexConfigs.MWL_Shipyard1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_Shipyard1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FortBakkarhalt1", BepinexConfigs.MWL_FortBakkarhalt1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_FortBakkarhalt1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_Belmont1", BepinexConfigs.MWL_Belmont1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_Belmont1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampCourtyard1", BepinexConfigs.MWL_SwampCourtyard1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampCourtyard1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampBrokenTower1", BepinexConfigs.MWL_SwampBrokenTower1_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampBrokenTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SwampBrokenTower3", BepinexConfigs.MWL_SwampBrokenTower3_Configuration, LocationConfigs.SwampPack1LocationConfigs["MWL_SwampBrokenTower3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }
    
    public static void AddMountainsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_StoneCastle1", BepinexConfigs.MWL_StoneCastle1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_StoneCastle1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_StoneFort1", BepinexConfigs.MWL_StoneFort1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_StoneFort1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_StoneHall1", BepinexConfigs.MWL_StoneHall1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_StoneHall1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_StoneTavern1", BepinexConfigs.MWL_StoneTavern1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_StoneTavern1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_StoneTower1", BepinexConfigs.MWL_StoneTower1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_StoneTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_StoneTower2", BepinexConfigs.MWL_StoneTower2_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_StoneTower2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_WoodBarn1", BepinexConfigs.MWL_WoodBarn1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_WoodBarn1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_WoodFarm1", BepinexConfigs.MWL_WoodFarm1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_WoodFarm1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_WoodHouse1", BepinexConfigs.MWL_WoodHouse1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_WoodHouse1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_TempleShrine1", BepinexConfigs.MWL_TempleShrine1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_TempleShrine1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_StoneAltar1", BepinexConfigs.MWL_StoneAltar1_Configuration, LocationConfigs.MountainPack1LocationConfigs["MWL_StoneAltar1_Config"], More_World_Locations_AIOPlugin.YAMLManager);

    }
    
    public static void AddPlainsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_GoblinFort1", BepinexConfigs.MWL_GoblinFort1_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_GoblinFort1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FulingRock1", BepinexConfigs.MWL_FulingRock1_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingRock1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FulingVillage1", BepinexConfigs.MWL_FulingVillage1_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingVillage1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FulingVillage2", BepinexConfigs.MWL_FulingVillage2_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingVillage2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_PlainsPillar1", BepinexConfigs.MWL_PlainsPillar1_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_PlainsPillar1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FulingTemple1", BepinexConfigs.MWL_FulingTemple1_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTemple1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FulingTemple2", BepinexConfigs.MWL_FulingTemple2_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTemple2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FulingTemple3", BepinexConfigs.MWL_FulingTemple3_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTemple3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FulingWall1", BepinexConfigs.MWL_FulingWall1_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingWall1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_FulingTower1", BepinexConfigs.MWL_FulingTower1_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_FulingTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RockGarden1", BepinexConfigs.MWL_RockGarden1_Configuration, LocationConfigs.PlainsPack1LocationConfigs["MWL_RockGarden1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }
    
    public static void AddMistlandsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_MistFort2", BepinexConfigs.MWL_MistFort2_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistFort2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_SecretRoom1", BepinexConfigs.MWL_SecretRoom1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_SecretRoom1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MistWorkshop1", BepinexConfigs.MWL_MistWorkshop1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistWorkshop1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MistTower1", BepinexConfigs.MWL_MistTower1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MistWall1", BepinexConfigs.MWL_MistWall1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistWall1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MistTower2", BepinexConfigs.MWL_MistTower2_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistTower2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MistHut1", BepinexConfigs.MWL_MistHut1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistHut1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_DvergrEitrSingularity1", BepinexConfigs.MWL_DvergrEitrSingularity1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_DvergrEitrSingularity1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_DvergrHouse1", BepinexConfigs.MWL_DvergrHouse1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_DvergrHouse1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_DvergrKnowledgeExtractor1", BepinexConfigs.MWL_DvergrKnowledgeExtractor1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_DvergrKnowledgeExtractor1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_AncientShrine1", BepinexConfigs.MWL_AncientShrine1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_AncientShrine1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MistShrine1", BepinexConfigs.MWL_MistShrine1_Configuration, LocationConfigs.MistlandsPack1LocationConfigs["MWL_MistShrine1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }
    
    public static void AddAshlandsPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_AshlandsFort1", BepinexConfigs.MWL_AshlandsFort1_Configuration, LocationConfigs.AshlandsPack1LocationConfigs["MWL_AshlandsFort1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_AshlandsFort2", BepinexConfigs.MWL_AshlandsFort2_Configuration, LocationConfigs.AshlandsPack1LocationConfigs["MWL_AshlandsFort2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_AshlandsFort3", BepinexConfigs.MWL_AshlandsFort3_Configuration, LocationConfigs.AshlandsPack1LocationConfigs["MWL_AshlandsFort3_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }
    
    public static void AddAdventureMapPack1Locations()
    {
        Common.LocationManager.AddLocation("MWL_CastleCorner1", BepinexConfigs.MWL_CastleCorner1_Configuration, LocationConfigs.AdventureMapPack1LocationConfigs["MWL_CastleCorner1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_ForestCamp1", BepinexConfigs.MWL_ForestCamp1_Configuration, LocationConfigs.AdventureMapPack1LocationConfigs["MWL_ForestCamp1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MistHut2", BepinexConfigs.MWL_Misthut2_Configuration, LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MistHut2_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MountainDvergrShrine1", BepinexConfigs.MWL_MountainDvergrShrine1_Configuration, LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MountainDvergrShrine1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_MountainShrine1", BepinexConfigs.MWL_MountainShrine1_Configuration, LocationConfigs.AdventureMapPack1LocationConfigs["MWL_MountainShrine1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_RuinedTower1", BepinexConfigs.MWL_RuinedTower1_Configuration, LocationConfigs.AdventureMapPack1LocationConfigs["MWL_RuinedTower1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
        Common.LocationManager.AddLocation("MWL_TreeTowers1", BepinexConfigs.MWL_TreeTowers1_Configuration, LocationConfigs.AdventureMapPack1LocationConfigs["MWL_TreeTowers1_Config"], More_World_Locations_AIOPlugin.YAMLManager);
    }


}