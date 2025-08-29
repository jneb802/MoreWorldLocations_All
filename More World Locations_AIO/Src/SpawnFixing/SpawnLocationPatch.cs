using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.SpawnFixing;

public class SpawnLocationPatch
{
    
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
    public class SpawnLocation_Patch
    {
        public static void Prefix(ZoneSystem.ZoneLocation location,
            int seed,
            Vector3 pos,
            Quaternion rot,
            ZoneSystem.SpawnMode mode,
            List<GameObject> spawnedGhostObjects)
        {
            if (location == null) Debug.Log($"SpawnLocation_Patch.Prefix: location is null");  
            if (seed == null) Debug.Log($"SpawnLocation_Patch.Prefix: seed is null");  
            if (pos == null) Debug.Log($"SpawnLocation_Patch.Prefix: pos is null");
            if (rot == null) Debug.Log($"SpawnLocation_Patch.Prefix: rot is null");
            if (mode == null) Debug.Log($"SpawnLocation_Patch.Prefix: mode is null");
            
            foreach (var go in spawnedGhostObjects)
            {
                if (go == null) Debug.Log($"SpawnLocation_Patch.Prefix: {go} in spawnedGhostObjects is null");
            }
            
            Debug.Log($"SpawnLocation_Patch.Prefix: Location is name {location.m_name}");
            Debug.Log($"SpawnLocation_Patch.Prefix: Location is prefab {location.m_prefab}");
            Debug.Log($"SpawnLocation_Patch.Prefix: seed is {seed}");
            Debug.Log($"SpawnLocation_Patch.Prefix: pos is {pos}");
            Debug.Log($"SpawnLocation_Patch.Prefix: rot is {rot}");
            Debug.Log($"SpawnLocation_Patch.Prefix: mode is {mode}");
            Debug.Log($"SpawnLocation_Patch.Prefix: spawnedGhostObjects has count {spawnedGhostObjects.Count}");
        }
    }
    
}