namespace More_World_Locations_AIO.Utils;

public static class UpgradeWorldCommands
{
    public static void AddUpgradeWorldCommands()
    {
        // All locations command
        UpgradeWorld.Upgrade.Register("mwl_allbiomes", "All MWL locations to the world.", 
            "locations_add " +
            // Meadows Pack 1 & 2 (24 locations)
            "MWL_Ruins1,MWL_Ruins2,MWL_Ruins3,MWL_Ruins6,MWL_Ruins7,MWL_Ruins8,MWL_RuinsArena1,MWL_RuinsArena3,MWL_RuinsChurch1,MWL_RuinsWell1," +
            "MWL_DeerShrine1,MWL_DeerShrine2,MWL_MeadowsBarn1,MWL_MeadowsHouse2,MWL_MeadowsRuin1,MWL_MeadowsTomb4,MWL_MeadowsTower1,MWL_OakHut1,MWL_SmallHouse1,MWL_MeadowsFarm1,MWL_MeadowsLighthouse1,MWL_MeadowsSawmill1,MWL_MeadowsWall1,MWL_MeadowsTavern1," +
            
            // Black Forest Pack 1 & 2 + Adventure (31 locations)
            "MWL_RuinsArena2,MWL_RuinsCastle1,MWL_RuinsCastle3,MWL_RuinsTower3,MWL_RuinsTower6,MWL_RuinsTower8,MWL_Tavern1,MWL_WoodTower1,MWL_WoodTower2,MWL_WoodTower3," +
            "MWL_ForestForge1,MWL_ForestForge2,MWL_ForestGreatHouse2,MWL_ForestHouse2,MWL_ForestRuin1,MWL_ForestTower2,MWL_ForestTower3,MWL_MassGrave1,MWL_StoneFormation1,MWL_GuardTower1,MWL_RootRuins1,MWL_RootsTower1,MWL_RootsTower2,MWL_ForestRuin2,MWL_ForestRuin3,MWL_ForestSkull1,MWL_ForestTower4,MWL_ForestTower5,MWL_ForestPillar1,MWL_CoastTower1," +
            "MWL_ForestCamp1,MWL_RuinedTower1," +
            
            // Swamp Pack 1 + Adventure (23 locations)
            "MWL_GuckPit1,MWL_SwampAltar1,MWL_SwampAltar2,MWL_SwampAltar3,MWL_SwampAltar4,MWL_SwampCastle2,MWL_SwampGrave1,MWL_SwampHouse1,MWL_SwampRuin1,MWL_SwampTower1,MWL_SwampTower2,MWL_SwampTower3,MWL_SwampWell1,MWL_AbandonedHouse1,MWL_Treehouse1,MWL_Shipyard1,MWL_FortBakkarhalt1,MWL_Belmont1,MWL_SwampCourtyard1,MWL_SwampBrokenTower1,MWL_SwampBrokenTower3," +
            "MWL_CastleCorner1,MWL_TreeTowers1," +
            
            // Mountains Pack 1 + Adventure (13 locations)
            "MWL_StoneCastle1,MWL_StoneFort1,MWL_StoneHall1,MWL_StoneTavern1,MWL_StoneTower1,MWL_StoneTower2,MWL_WoodBarn1,MWL_WoodFarm1,MWL_WoodHouse1,MWL_TempleShrine1,MWL_StoneAltar1," +
            "MWL_MountainDvergrShrine1,MWL_MountainShrine1," +
            
            // Plains Pack 1 (11 locations)
            "MWL_GoblinFort1,MWL_FulingRock1,MWL_FulingVillage1,MWL_FulingVillage2,MWL_PlainsPillar1,MWL_FulingTemple1,MWL_FulingTemple2,MWL_FulingTemple3,MWL_FulingWall1,MWL_FulingTower1,MWL_RockGarden1," +
            
            // Mistlands Pack 1 + Adventure (13 locations)
            "MWL_MistFort2,MWL_SecretRoom1,MWL_MistWorkshop1,MWL_MistTower1,MWL_MistWall1,MWL_MistTower2,MWL_MistHut1,MWL_DvergrEitrSingularity1,MWL_DvergrHouse1,MWL_DvergrKnowledgeExtractor1,MWL_AncientShrine1,MWL_MistShrine1," +
            "MWL_MistHut2," +
            
            // Ashlands Pack 1 (3 locations)
            "MWL_AshlandsFort1,MWL_AshlandsFort2,MWL_AshlandsFort3" +
            " start");

        // Meadows biome command (24 locations)
        UpgradeWorld.Upgrade.Register("mwl_meadows", "Adds MWL locations to the Meadows biome.", 
            "locations_add " +
            "MWL_Ruins1,MWL_Ruins2,MWL_Ruins3,MWL_Ruins6,MWL_Ruins7,MWL_Ruins8,MWL_RuinsArena1,MWL_RuinsArena3,MWL_RuinsChurch1,MWL_RuinsWell1," +
            "MWL_DeerShrine1,MWL_DeerShrine2,MWL_MeadowsBarn1,MWL_MeadowsHouse2,MWL_MeadowsRuin1,MWL_MeadowsTomb4,MWL_MeadowsTower1,MWL_OakHut1,MWL_SmallHouse1,MWL_MeadowsFarm1,MWL_MeadowsLighthouse1,MWL_MeadowsSawmill1,MWL_MeadowsWall1,MWL_MeadowsTavern1" +
            " start");

        // Black Forest biome command (31 locations)
        UpgradeWorld.Upgrade.Register("mwl_blackforest", "Adds MWL locations to the Black Forest biome.", 
            "locations_add " +
            "MWL_RuinsArena2,MWL_RuinsCastle1,MWL_RuinsCastle3,MWL_RuinsTower3,MWL_RuinsTower6,MWL_RuinsTower8,MWL_Tavern1,MWL_WoodTower1,MWL_WoodTower2,MWL_WoodTower3," +
            "MWL_ForestForge1,MWL_ForestForge2,MWL_ForestGreatHouse2,MWL_ForestHouse2,MWL_ForestRuin1,MWL_ForestTower2,MWL_ForestTower3,MWL_MassGrave1,MWL_StoneFormation1,MWL_GuardTower1,MWL_RootRuins1,MWL_RootsTower1,MWL_RootsTower2,MWL_ForestRuin2,MWL_ForestRuin3,MWL_ForestSkull1,MWL_ForestTower4,MWL_ForestTower5,MWL_ForestPillar1,MWL_CoastTower1," +
            "MWL_ForestCamp1,MWL_RuinedTower1" +
            " start");

        // Swamp biome command (23 locations)
        UpgradeWorld.Upgrade.Register("mwl_swamp", "Adds MWL locations to the Swamp biome.", 
            "locations_add " +
            "MWL_GuckPit1,MWL_SwampAltar1,MWL_SwampAltar2,MWL_SwampAltar3,MWL_SwampAltar4,MWL_SwampCastle2,MWL_SwampGrave1,MWL_SwampHouse1,MWL_SwampRuin1,MWL_SwampTower1,MWL_SwampTower2,MWL_SwampTower3,MWL_SwampWell1,MWL_AbandonedHouse1,MWL_Treehouse1,MWL_Shipyard1,MWL_FortBakkarhalt1,MWL_Belmont1,MWL_SwampCourtyard1,MWL_SwampBrokenTower1,MWL_SwampBrokenTower3," +
            "MWL_CastleCorner1,MWL_TreeTowers1" +
            " start");

        // Mountains biome command (13 locations)
        UpgradeWorld.Upgrade.Register("mwl_mountains", "Adds MWL locations to the Mountains biome.", 
            "locations_add " +
            "MWL_StoneCastle1,MWL_StoneFort1,MWL_StoneHall1,MWL_StoneTavern1,MWL_StoneTower1,MWL_StoneTower2,MWL_WoodBarn1,MWL_WoodFarm1,MWL_WoodHouse1,MWL_TempleShrine1,MWL_StoneAltar1," +
            "MWL_MountainDvergrShrine1,MWL_MountainShrine1" +
            " start");

        // Plains biome command (11 locations)
        UpgradeWorld.Upgrade.Register("mwl_plains", "Adds MWL locations to the Plains biome.", 
            "locations_add " +
            "MWL_GoblinFort1,MWL_FulingRock1,MWL_FulingVillage1,MWL_FulingVillage2,MWL_PlainsPillar1,MWL_FulingTemple1,MWL_FulingTemple2,MWL_FulingTemple3,MWL_FulingWall1,MWL_FulingTower1,MWL_RockGarden1" +
            " start");

        // Mistlands biome command (13 locations)
        UpgradeWorld.Upgrade.Register("mwl_mistlands", "Adds MWL locations to the Mistlands biome.", 
            "locations_add " +
            "MWL_MistFort2,MWL_SecretRoom1,MWL_MistWorkshop1,MWL_MistTower1,MWL_MistWall1,MWL_MistTower2,MWL_MistHut1,MWL_DvergrEitrSingularity1,MWL_DvergrHouse1,MWL_DvergrKnowledgeExtractor1,MWL_AncientShrine1,MWL_MistShrine1," +
            "MWL_MistHut2" +
            " start");

        // Ashlands biome command (3 locations)
        UpgradeWorld.Upgrade.Register("mwl_ashlands", "Adds MWL locations to the Ashlands biome.", 
            "locations_add " +
            "MWL_AshlandsFort1,MWL_AshlandsFort2,MWL_AshlandsFort3" +
            " start");
    }
}