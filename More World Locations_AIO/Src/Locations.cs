using BepInEx;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO;

public class Locations
{
    public static void AddAllLocations()
    {
        AddMeadowPack2Locations();
        AddBlackForestPack2Locations();
        AddSwampPack1Locations();
        AddMountainsPack1Locations();
        AddPlainsPack1Locations();
        
        
        ZoneManager.OnVanillaLocationsAvailable -= AddAllLocations;
    }
    
    public static void AddMeadowPack2Locations(AssetBundle assetBundle)
    { 
       Common.LocationManager.AddLocation(assetBundle, "MWL_DeerShrine1", Meadows_Pack_2Plugin.MWL_DeerShrine1_Configuration, LocationConfigs.AllLocationConfigs["MWL_DeerShrine1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_DeerShrine2", Meadows_Pack_2Plugin.MWL_DeerShrine2_Configuration, LocationConfigs.AllLocationConfigs["MWL_DeerShrine2_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsBarn1", Meadows_Pack_2Plugin.MWL_MeadowsBarn1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsBarn1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsHouse2", Meadows_Pack_2Plugin.MWL_MeadowsHouse2_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsHouse2_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsRuin1", Meadows_Pack_2Plugin.MWL_MeadowsRuin1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsRuin1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsTomb4", Meadows_Pack_2Plugin.MWL_MeadowsTomb4_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsTomb4_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsTower1", Meadows_Pack_2Plugin.MWL_MeadowsTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsTower1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_OakHut1", Meadows_Pack_2Plugin.MWL_OakHut1_Configuration, LocationConfigs.AllLocationConfigs["MWL_OakHut1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_SmallHouse1", Meadows_Pack_2Plugin.MWL_SmallHouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SmallHouse1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsFarm1", Meadows_Pack_2Plugin.MWL_MeadowsFarm1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsFarm1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsLighthouse1", Meadows_Pack_2Plugin.MWL_MeadowsLighthouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsLighthouse1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsSawmill1", Meadows_Pack_2Plugin.MWL_MeadowsSawmill1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsSawmill1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsWall1", Meadows_Pack_2Plugin.MWL_MeadowsWall1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsWall1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
       Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsTavern1", Meadows_Pack_2Plugin.MWL_MeadowsTavern1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsTavern1_Config"], Meadows_Pack_2Plugin.meadows2YAMLManager);
    }
    
    public static void AddBlackForestPack2Locations(AssetBundle assetBundle)
    {
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestForge1", BlackForest_Pack_2Plugin.MWL_ForestForge1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestForge1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestForge2", BlackForest_Pack_2Plugin.MWL_ForestForge2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestForge2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestGreatHouse2", BlackForest_Pack_2Plugin.MWL_ForestGreatHouse2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestGreatHouse2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestHouse2", BlackForest_Pack_2Plugin.MWL_ForestHouse2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestHouse2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestRuin1", BlackForest_Pack_2Plugin.MWL_ForestRuin1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestRuin1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestTower2", BlackForest_Pack_2Plugin.MWL_ForestTower2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestTower2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestTower3", BlackForest_Pack_2Plugin.MWL_ForestTower3_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestTower3_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MassGrave1", BlackForest_Pack_2Plugin.MWL_MassGrave1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MassGrave1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneFormation1", BlackForest_Pack_2Plugin.MWL_StoneFormation1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneFormation1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_GuardTower1", BlackForest_Pack_2Plugin.MWL_GuardTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_GuardTower1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_RootRuins1", BlackForest_Pack_2Plugin.MWL_RootRuins1_Configuration, LocationConfigs.AllLocationConfigs["MWL_RootRuins1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_RootsTower1", BlackForest_Pack_2Plugin.MWL_RootsTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_RootsTower1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_RootsTower2", BlackForest_Pack_2Plugin.MWL_RootsTower2_Configuration, LocationConfigs.AllLocationConfigs["MWL_RootsTower2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestRuin2", BlackForest_Pack_2Plugin.MWL_ForestRuin2_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestRuin2_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestRuin3", BlackForest_Pack_2Plugin.MWL_ForestRuin3_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestRuin3_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestSkull1", BlackForest_Pack_2Plugin.MWL_ForestSkull1_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestSkull1_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestTower4", BlackForest_Pack_2Plugin.MWL_ForestTower4_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestTower4_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_ForestTower5", BlackForest_Pack_2Plugin.MWL_ForestTower5_Configuration, LocationConfigs.AllLocationConfigs["MWL_ForestTower5_Config"], BlackForest_Pack_2Plugin.blackforest2YAMLmanager);
    }
    
    public static void AddSwampPack1Locations(AssetBundle assetBundle)
    {
        Common.LocationManager.AddLocation(assetBundle, "MWL_GuckPit1", Swamp_Pack_1Plugin.MWL_GuckPit1_Configuration, LocationConfigs.AllLocationConfigs["MWL_GuckPit1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampAltar1", Swamp_Pack_1Plugin.MWL_SwampAltar1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampAltar1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampAltar2", Swamp_Pack_1Plugin.MWL_SwampAltar2_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampAltar2_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampAltar3", Swamp_Pack_1Plugin.MWL_SwampAltar3_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampAltar3_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampAltar4", Swamp_Pack_1Plugin.MWL_SwampAltar4_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampAltar4_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampCastle2", Swamp_Pack_1Plugin.MWL_SwampCastle2_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampCastle2_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampGrave1", Swamp_Pack_1Plugin.MWL_SwampGrave1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampGrave1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampHouse1", Swamp_Pack_1Plugin.MWL_SwampHouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampHouse1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampRuin1", Swamp_Pack_1Plugin.MWL_SwampRuin1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampRuin1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampTower1", Swamp_Pack_1Plugin.MWL_SwampTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampTower1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampTower2", Swamp_Pack_1Plugin.MWL_SwampTower2_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampTower2_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampTower3", Swamp_Pack_1Plugin.MWL_SwampTower3_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampTower3_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SwampWell1", Swamp_Pack_1Plugin.MWL_SwampWell1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SwampWell1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_AbandonedHouse1", Swamp_Pack_1Plugin.MWL_AbandonedHouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_AbandonedHouse1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_Treehouse1", Swamp_Pack_1Plugin.MWL_Treehouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_Treehouse1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_Shipyard1", Swamp_Pack_1Plugin.MWL_Shipyard1_Configuration, LocationConfigs.AllLocationConfigs["MWL_Shipyard1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FortBakkarhalt1", Swamp_Pack_1Plugin.MWL_FortBakkarhalt1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FortBakkarhalt1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_Belmont1", Swamp_Pack_1Plugin.MWL_Belmont1_Configuration, LocationConfigs.AllLocationConfigs["MWL_Belmont1_Config"], Swamp_Pack_1Plugin.swampYAMLmanager);
    }
    
    public static void AddMountainsPack1Locations(AssetBundle assetBundle)
    {
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneCastle1", Mountains_Pack_1Plugin.MWL_StoneCastle1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneCastle1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneFort1", Mountains_Pack_1Plugin.MWL_StoneFort1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneFort1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneHall1", Mountains_Pack_1Plugin.MWL_StoneHall1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneHall1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneTavern1", Mountains_Pack_1Plugin.MWL_StoneTavern1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneTavern1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneTower1", Mountains_Pack_1Plugin.MWL_StoneTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneTower1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_StoneTower2", Mountains_Pack_1Plugin.MWL_StoneTower2_Configuration, LocationConfigs.AllLocationConfigs["MWL_StoneTower2_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_WoodBarn1", Mountains_Pack_1Plugin.MWL_WoodBarn1_Configuration, LocationConfigs.AllLocationConfigs["MWL_WoodBarn1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_WoodFarm1", Mountains_Pack_1Plugin.MWL_WoodFarm1_Configuration, LocationConfigs.AllLocationConfigs["MWL_WoodFarm1_Config"], Mountains_Pack_1Plugin.MountainYAML);
        Common.LocationManager.AddLocation(assetBundle, "MWL_WoodHouse1", Mountains_Pack_1Plugin.MWL_WoodHouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_WoodHouse1_Config"], Mountains_Pack_1Plugin.MountainYAML);
    }
    
    public static void AddPlainsPack1Locations(AssetBundle assetBundle)
    {
        Common.LocationManager.AddLocation(assetBundle, "MWL_GoblinFort1", Plains_Pack_1Plugin.MWL_GoblinFort1_Configuration, LocationConfigs.AllLocationConfigs["MWL_GoblinFort1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingRock1", Plains_Pack_1Plugin.MWL_FulingRock1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingRock1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingVillage1", Plains_Pack_1Plugin.MWL_FulingVillage1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingVillage1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingVillage2", Plains_Pack_1Plugin.MWL_FulingVillage2_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingVillage2_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_PlainsPillar1", Plains_Pack_1Plugin.MWL_PlainsPillar1_Configuration, LocationConfigs.AllLocationConfigs["MWL_PlainsPillar1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingTemple1", Plains_Pack_1Plugin.MWL_FulingTemple1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingTemple1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingTemple2", Plains_Pack_1Plugin.MWL_FulingTemple2_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingTemple2_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingTemple3", Plains_Pack_1Plugin.MWL_FulingTemple3_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingTemple3_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingWall1", Plains_Pack_1Plugin.MWL_FulingWall1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingWall1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_FulingTower1", Plains_Pack_1Plugin.MWL_FulingTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_FulingTower1_Config"], Plains_Pack_1Plugin.plainsYAMLmanager);
    }
}