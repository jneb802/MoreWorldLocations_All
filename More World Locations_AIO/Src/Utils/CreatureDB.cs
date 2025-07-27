using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Utils
{
    public static class CreatureDB
    {
        public static Dictionary<string, List<WeightedCreature>> creatureLists;
        
        public static void InitializeCreatureLists()
        {
            creatureLists = new Dictionary<string, List<WeightedCreature>>
            {
                // Meadows Creature Lists
                { "MeadowsCreatures1", GetMeadowsCreatures1() },
                { "MeadowsCreatures2", GetMeadowsCreatures2() },
                { "MeadowsCreatures3", GetMeadowsCreatures3() },
                { "MeadowsCreatures4", GetMeadowsCreatures4() },
                { "MeadowsCreatures5", GetMeadowsCreatures5() },
                
                // Black Forest Creature Lists
                { "BlackforestCreatures1", GetBlackforestCreatures1() },
                { "BlackforestCreatures2", GetBlackforestCreatures2() },
                { "BlackforestCreatures3", GetBlackforestCreatures3() },
                
                // Swamp Creature Lists
                { "SwampCreatures1", GetSwampCreatures1() },
                { "SwampCreatures2", GetSwampCreatures2() },
                { "SwampCreatures3", GetSwampCreatures3() },
                { "SwampCreatures4", GetSwampCreatures4() },
                
                // Mountains Creature Lists
                { "MountainsCreatures1", GetMountainsCreatures1() },
                { "MountainsCreatures2", GetMountainsCreatures2() },
                { "MountainsCreatures3", GetMountainsCreatures3() },
                { "MountainsCreatures4", GetMountainsCreatures4() },
                
                // Plains Creature Lists
                { "PlainsCreatures1", GetPlainsCreatures1() },
                { "PlainsCreatures2", GetPlainsCreatures2() },
                { "PlainsCreatures3", GetPlainsCreatures3() },
                
                // Mistlands Creature Lists
                { "MistCreatures1", GetMistCreatures1() },
                { "MistCreatures2", GetMistCreatures2() },
                { "MistCreatures3", GetMistCreatures3() },
                
                // Ashlands Creature Lists
                { "AshlandsCreatures1", GetAshlandsCreatures1() }
            };
            
            PrefabManager.OnVanillaPrefabsAvailable -= InitializeCreatureLists;
        }
        
        #region Meadows Creature Lists
        
        private static List<WeightedCreature> GetMeadowsCreatures1()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Skeleton"), weight = 4.0f },
                new WeightedCreature { creature = GetCreature("Skeleton_Poison"), weight = 1.0f }
            };
        }
        
        private static List<WeightedCreature> GetMeadowsCreatures2()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Greydwarf"), weight = 6.0f },
                new WeightedCreature { creature = GetCreature("Greydwarf_Elite"), weight = 1.0f },
                new WeightedCreature { creature = GetCreature("Greydwarf_Shaman"), weight = 1.0f }
            };
        }
        
        private static List<WeightedCreature> GetMeadowsCreatures3()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Greyling"), weight = 3.0f }
            };
        }
        
        private static List<WeightedCreature> GetMeadowsCreatures4()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Skeleton"), weight = 3.0f }
            };
        }
        
        private static List<WeightedCreature> GetMeadowsCreatures5()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Boar"), weight = 3.0f }
            };
        }
        
        #endregion
        
        #region Black Forest Creature Lists
        
        private static List<WeightedCreature> GetBlackforestCreatures1()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Skeleton"), weight = 4.0f },
                new WeightedCreature { creature = GetCreature("Skeleton_Poison"), weight = 1.0f }
            };
        }
        
        private static List<WeightedCreature> GetBlackforestCreatures2()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Greydwarf"), weight = 5.0f },
                new WeightedCreature { creature = GetCreature("Greydwarf_Elite"), weight = 1.0f },
                new WeightedCreature { creature = GetCreature("Greydwarf_Shaman"), weight = 1.0f }
            };
        }
        
        private static List<WeightedCreature> GetBlackforestCreatures3()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Greydwarf"), weight = 8.0f },
                new WeightedCreature { creature = GetCreature("Greydwarf_Elite"), weight = 4.0f },
                new WeightedCreature { creature = GetCreature("Greydwarf_Shaman"), weight = 2.0f }
            };
        }
        
        #endregion
        
        #region Swamp Creature Lists
        
        private static List<WeightedCreature> GetSwampCreatures1()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Blob"), weight = 4.0f },
                new WeightedCreature { creature = GetCreature("BlobElite"), weight = 1.0f }
            };
        }
        
        private static List<WeightedCreature> GetSwampCreatures2()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Draugr"), weight = 9.0f },
                new WeightedCreature { creature = GetCreature("Draugr_Elite"), weight = 2.0f }
            };
        }
        
        private static List<WeightedCreature> GetSwampCreatures3()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Skeleton"), weight = 9.0f },
                new WeightedCreature { creature = GetCreature("Skeleton_Poison"), weight = 2.0f }
            };
        }
        
        private static List<WeightedCreature> GetSwampCreatures4()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Wraith"), weight = 2.0f }
            };
        }
        
        #endregion
        
        #region Mountains Creature Lists
        
        private static List<WeightedCreature> GetMountainsCreatures1()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Skeleton"), weight = 7.0f },
                new WeightedCreature { creature = GetCreature("Skeleton_Poison"), weight = 1.0f }
            };
        }
        
        private static List<WeightedCreature> GetMountainsCreatures2()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Draugr"), weight = 11.0f },
                new WeightedCreature { creature = GetCreature("Draugr_Elite"), weight = 2.0f }
            };
        }
        
        private static List<WeightedCreature> GetMountainsCreatures3()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("StoneGolem"), weight = 2.0f }
            };
        }
        
        private static List<WeightedCreature> GetMountainsCreatures4()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Ulv"), weight = 9.0f },
                new WeightedCreature { creature = GetCreature("Fenring"), weight = 4.0f },
                new WeightedCreature { creature = GetCreature("Fenring_Cultist"), weight = 1.0f }
            };
        }
        
        #endregion
        
        #region Plains Creature Lists
        
        private static List<WeightedCreature> GetPlainsCreatures1()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Goblin"), weight = 7.0f },
                new WeightedCreature { creature = GetCreature("GoblinArcher"), weight = 2.0f },
                new WeightedCreature { creature = GetCreature("GoblinShaman"), weight = 2.0f }
            };
        }
        
        private static List<WeightedCreature> GetPlainsCreatures2()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Goblin"), weight = 7.0f },
                new WeightedCreature { creature = GetCreature("GoblinArcher"), weight = 3.0f },
                new WeightedCreature { creature = GetCreature("GoblinShaman"), weight = 2.0f },
                new WeightedCreature { creature = GetCreature("GoblinBrute"), weight = 2.0f }
            };
        }
        
        private static List<WeightedCreature> GetPlainsCreatures3()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Goblin"), weight = 4.0f },
                new WeightedCreature { creature = GetCreature("GoblinArcher"), weight = 1.0f }
            };
        }
        
        #endregion
        
        #region Mistlands Creature Lists
        
        private static List<WeightedCreature> GetMistCreatures1()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Dverger"), weight = 6.0f },
                new WeightedCreature { creature = GetCreature("DvergerMage"), weight = 4.0f }
            };
        }
        
        private static List<WeightedCreature> GetMistCreatures2()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Seeker"), weight = 9.0f },
                new WeightedCreature { creature = GetCreature("SeekerBrute"), weight = 2.0f }
            };
        }
        
        private static List<WeightedCreature> GetMistCreatures3()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Gjall"), weight = 1.0f },
                new WeightedCreature { creature = GetCreature("Seeker"), weight = 6.0f }
            };
        }
        
        #endregion
        
        #region Ashlands Creature Lists
        
        private static List<WeightedCreature> GetAshlandsCreatures1()
        {
            return new List<WeightedCreature>
            {
                new WeightedCreature { creature = GetCreature("Charred_Twitcher"), weight = 6.0f },
                new WeightedCreature { creature = GetCreature("Charred_Archer"), weight = 2.0f },
                new WeightedCreature { creature = GetCreature("Charred_Melee"), weight = 2.0f }
            };
        }
        
        #endregion
        
        private static GameObject GetCreature(string creatureName)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(creatureName);
            if (prefab == null)
            {
                Debug.LogWarning($"CreatureDB: Could not find creature prefab: {creatureName}");
                return null;
            }
            return prefab;
        }
        
        public static List<WeightedCreature> GetCreatureList(string listName)
        {
            if (!creatureLists.ContainsKey(listName))
            {
                Debug.LogWarning($"CreatureDB: No creature list found for: {listName}");
                return new List<WeightedCreature>();
            }
            
            return creatureLists[listName];
        }
        
        public static void SetupCreatures(string creatureListName, GameObject locationObject)
        {
            foreach (CreatureSpawner creatureSpawner in locationObject.GetComponentsInChildren<CreatureSpawner>())
            {
                creatureSpawner.m_creaturePrefab = GetRandomCreatureFromList(creatureListName);
            }
        }
        
        public static GameObject GetRandomCreatureFromList(string listName)
        {
            List<WeightedCreature> creatures = GetCreatureList(listName);
            if (creatures.Count == 0)
            {
                Debug.LogWarning($"CreatureDB: Empty creature list for: {listName}");
                return null;
            }
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var creature in creatures)
            {
                totalWeight += creature.weight;
            }
            
            // Random selection based on weight
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var creature in creatures)
            {
                currentWeight += creature.weight;
                if (randomValue <= currentWeight)
                {
                    return creature.creature;
                }
            }
            
            // Fallback (shouldn't happen)
            return creatures[0].creature;
        }
        
        public static List<string> GetAllCreatureListNames()
        {
            return new List<string>(creatureLists.Keys);
        }
        
        [System.Serializable]
        public class WeightedCreature
        {
            public GameObject creature;
            public float weight;
        }
    }
}