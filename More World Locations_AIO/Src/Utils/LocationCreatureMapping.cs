using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.Utils
{
    public static class LocationCreatureMapping
    {
        public static Dictionary<string, string> locationToCreatureList = new Dictionary<string, string>
        {
            // Meadows Pack 1
            { "MWL_Ruins1", "MeadowsCreatures1" },
            { "MWL_Ruins2", "MeadowsCreatures1" },
            { "MWL_Ruins3", "MeadowsCreatures1" },
            { "MWL_Ruins6", "MeadowsCreatures2" },
            { "MWL_Ruins7", "MeadowsCreatures2" },
            { "MWL_Ruins8", "MeadowsCreatures2" },
            { "MWL_RuinsArena1", "MeadowsCreatures1" },
            { "MWL_RuinsArena3", "MeadowsCreatures1" },
            { "MWL_RuinsChurch1", "MeadowsCreatures1" },
            { "MWL_RuinsWell1", "MeadowsCreatures1" },

            // Meadows Pack 2
            { "MWL_DeerShrine1", "MeadowsCreatures1" },
            { "MWL_DeerShrine2", "MeadowsCreatures1" },
            { "MWL_MeadowsBarn1", "MeadowsCreatures5" },
            { "MWL_MeadowsHouse2", "MeadowsCreatures2" },
            { "MWL_MeadowsRuin1", "MeadowsCreatures1" },
            { "MWL_MeadowsTomb4", "MeadowsCreatures1" },
            { "MWL_MeadowsTower1", "MeadowsCreatures1" },
            { "MWL_OakHut1", "MeadowsCreatures1" },
            { "MWL_SmallHouse1", "MeadowsCreatures1" },
            { "MWL_MeadowsFarm1", "MeadowsCreatures1" },
            { "MWL_MeadowsLighthouse1", "MeadowsCreatures1" },
            { "MWL_MeadowsSawmill1", "MeadowsCreatures1" },
            { "MWL_MeadowsWall1", "MeadowsCreatures1" },
            { "MWL_MeadowsTavern1", "MeadowsCreatures1" },

            // Blackforest Pack 1
            { "MWL_RuinsArena2", "BlackforestCreatures3" },
            { "MWL_RuinsCastle1", "BlackforestCreatures2" },
            { "MWL_RuinsCastle3", "BlackforestCreatures2" },
            { "MWL_RuinsTower3", "BlackforestCreatures1" },
            { "MWL_RuinsTower8", "BlackforestCreatures1" },
            { "MWL_Tavern1", "BlackforestCreatures2" },
            { "MWL_WoodTower1", "BlackforestCreatures1" },
            { "MWL_WoodTower2", "BlackforestCreatures2" },
            { "MWL_WoodTower3", "BlackforestCreatures3" },
            { "MWL_RuinsTower6", "BlackforestCreatures1" },

            // Blackforest Pack 2
            { "MWL_ForestForge1", "BlackforestCreatures3" },
            { "MWL_ForestForge2", "BlackforestCreatures3" },
            { "MWL_ForestGreatHouse2", "BlackforestCreatures3" },
            { "MWL_ForestHouse2", "BlackforestCreatures3" },
            { "MWL_ForestRuin1", "BlackforestCreatures3" },
            { "MWL_ForestTower2", "BlackforestCreatures1" },
            { "MWL_ForestTower3", "BlackforestCreatures1" },
            { "MWL_MassGrave1", "BlackforestCreatures3" },
            { "MWL_StoneFormation1", "BlackforestCreatures3" },
            { "MWL_GuardTower1", "BlackforestCreatures1" },
            { "MWL_RootRuins1", "BlackforestCreatures3" },
            { "MWL_RootsTower1", "BlackforestCreatures3" },
            { "MWL_RootsTower2", "BlackforestCreatures3" },
            { "MWL_RuinedRootTower5", "BlackforestCreatures3" },
            { "MWL_StoneOutlook1", "BlackforestCreatures3" },
            { "MWL_ForestRuin2", "BlackforestCreatures2" },
            { "MWL_ForestRuin3", "BlackforestCreatures3" },
            { "MWL_ForestSkull1", "BlackforestCreatures1" },
            { "MWL_ForestTower4", "BlackforestCreatures3" },
            { "MWL_ForestTower5", "BlackforestCreatures3" },
            { "MWL_ForestPillar1", "BlackforestCreatures3" },
            { "MWL_CoastTower1", "BlackforestCreatures1" },

            // Swamp Pack 1
            { "MWL_GuckPit1", "SwampCreatures1" },
            { "MWL_SwampAltar1", "SwampCreatures3" },
            { "MWL_SwampAltar2", "SwampCreatures3" },
            { "MWL_SwampAltar3", "SwampCreatures3" },
            { "MWL_SwampAltar4", "SwampCreatures2" },
            { "MWL_SwampCastle2", "SwampCreatures2" },
            { "MWL_SwampGrave1", "SwampCreatures2" },
            { "MWL_SwampHouse1", "SwampCreatures3" },
            { "MWL_SwampRuin1", "SwampCreatures3" },
            { "MWL_SwampTower1", "SwampCreatures3" },
            { "MWL_SwampTower2", "SwampCreatures4" },
            { "MWL_SwampTower3", "SwampCreatures2" },
            { "MWL_SwampWell1", "SwampCreatures1" },
            { "MWL_AbandonedHouse1", "SwampCreatures3" },
            { "MWL_Treehouse1", "SwampCreatures1" },
            { "MWL_Shipyard1", "SwampCreatures2" },
            { "MWL_FortBakkarhalt1", "SwampCreatures2" },
            { "MWL_Belmont1", "SwampCreatures2" },
            { "MWL_SwampCourtyard1", "SwampCreatures2" },
            { "MWL_SwampBrokenTower1", "SwampCreatures2" },
            { "MWL_SwampBrokenTower3", "SwampCreatures4" },

            // Mountain Pack 1
            { "MWL_StoneCastle1", "MountainsCreatures1" },
            { "MWL_StoneFort1", "MountainsCreatures1" },
            { "MWL_StoneHall1", "MountainsCreatures4" },
            { "MWL_StoneTavern1", "MountainsCreatures1" },
            { "MWL_StoneTower1", "MountainsCreatures2" },
            { "MWL_StoneTower2", "MountainsCreatures2" },
            { "MWL_WoodBarn1", "MountainsCreatures1" },
            { "MWL_WoodFarm1", "MountainsCreatures3" },
            { "MWL_WoodHouse1", "MountainsCreatures1" },
            { "MWL_TempleShrine1", "MountainsCreatures1" },
            { "MWL_StoneAltar1", "MountainsCreatures1" },

            // Plains Pack 1
            { "MWL_GoblinFort1", "PlainsCreatures2" },
            { "MWL_FulingRock1", "PlainsCreatures3" },
            { "MWL_FulingVillage1", "PlainsCreatures3" },
            { "MWL_FulingVillage2", "PlainsCreatures2" },
            { "MWL_PlainsPillar1", "PlainsCreatures1" },
            { "MWL_FulingTemple1", "PlainsCreatures2" },
            { "MWL_FulingTemple2", "PlainsCreatures1" },
            { "MWL_FulingTemple3", "PlainsCreatures1" },
            { "MWL_FulingWall1", "PlainsCreatures3" },
            { "MWL_FulingTower1", "PlainsCreatures3" },
            { "MWL_RockGarden1", "PlainsCreatures3" },

            // Mistlands Pack 1
            { "MWL_MistFort2", "MistCreatures1" },
            { "MWL_SecretRoom1", "MistCreatures2" },
            { "MWL_MistWorkshop1", "MistCreatures2" },
            { "MWL_MistTower1", "MistCreatures1" },
            { "MWL_MistWall1", "MistCreatures2" },
            { "MWL_MistTower2", "MistCreatures1" },
            { "MWL_MistHut1", "MistCreatures1" },
            { "MWL_DvergrEitrSingularity1", "MistCreatures1" },
            { "MWL_DvergrHouse1", "PlainsCreatures3" }, // Note: This uses PlainsCreatures3 as per your config
            { "MWL_DvergrKnowledgeExtractor1", "MistCreatures1" },
            { "MWL_AncientShrine1", "MistCreatures1" },
            { "MWL_MistShrine1", "MistCreatures1" },

            // Ashlands Pack 1
            { "MWL_AshlandsFort1", "AshlandsCreatures1" },
            { "MWL_AshlandsFort2", "AshlandsCreatures1" },
            { "MWL_AshlandsFort3", "AshlandsCreatures1" },

            // Adventure Map Pack 1
            { "MWL_CastleCorner1", "SwampCreatures1" },
            { "MWL_ForestCamp1", "BlackforestCreatures1" },
            { "MWL_MistHut2", "MistCreatures1" },
            { "MWL_MountainDvergrShrine1", "MistCreatures1" },
            { "MWL_MountainShrine1", "MountainsCreatures1" }, // Fixed typo from "MountainCreatures1"
            { "MWL_RuinedTower1", "BlackforestCreatures2" },
            { "MWL_TreeTowers1", "SwampCreatures1" }
        };

        public static string GetCreatureListForLocation(string locationName)
        {
            if (locationToCreatureList.ContainsKey(locationName))
            {
                return locationToCreatureList[locationName];
            }
            
            Debug.LogWarning($"LocationCreatureMapping: No creature list mapping found for {locationName}");
            return null;
        }
    }
}