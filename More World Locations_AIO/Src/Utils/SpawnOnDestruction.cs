using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Utils;

/// <summary>
/// Spawns creatures and effects when the attached Destructible is destroyed.
/// Add this component to any cloned prefab that has a Destructible component.
/// The original m_spawnWhenDestroyed behavior (e.g., key fragment drop) still works.
/// </summary>
public class SpawnOnDestruction : MonoBehaviour
{
    // Creature spawning configuration.
    public List<string> creaturePrefabNames = new List<string>();
    public Vector3 spawnOffset = new Vector3(0f, 5f, -3f);
    public int spawnCount = 1;
    public int creatureLevel = 1;
    
    // VFX and SFX to play once at the altar/object position when destroyed.
    public string spawnVfxPrefabName = "";
    public string spawnSfxPrefabName = "";
    
    // SFX to play at each creature spawn position.
    public string creatureSfxPrefabName = "";
    
    private Destructible? destructible;
    
    private void Awake()
    {
        destructible = GetComponent<Destructible>();
        if (destructible != null)
        {
            destructible.m_onDestroyed += OnDestroyed;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up delegate to avoid memory leaks.
        if (destructible != null)
        {
            destructible.m_onDestroyed -= OnDestroyed;
        }
    }
    
    private void OnDestroyed()
    {
        Vector3 basePos = transform.position;
        
        // Spawn VFX and SFX once at the altar/object position.
        if (!string.IsNullOrEmpty(spawnVfxPrefabName))
        {
            GameObject vfxPrefab = PrefabManager.Instance.GetPrefab(spawnVfxPrefabName);
            if (vfxPrefab != null) Object.Instantiate(vfxPrefab, basePos, Quaternion.identity);
        }
        if (!string.IsNullOrEmpty(spawnSfxPrefabName))
        {
            GameObject sfxPrefab = PrefabManager.Instance.GetPrefab(spawnSfxPrefabName);
            if (sfxPrefab != null) Object.Instantiate(sfxPrefab, basePos, Quaternion.identity);
        }
        
        foreach (string creatureName in creaturePrefabNames)
        {
            GameObject creaturePrefab = PrefabManager.Instance.GetPrefab(creatureName);
            if (creaturePrefab == null)
            {
                Debug.LogWarning($"SpawnOnDestruction: Could not find creature prefab {creatureName}");
                continue;
            }
            
            for (int i = 0; i < spawnCount; i++)
            {
                // Add randomness to prevent stacking.
                Vector3 randomOffset = new Vector3(
                    Random.Range(-2f, 2f),
                    0f,
                    Random.Range(-2f, 2f)
                );
                Vector3 spawnPos = basePos + spawnOffset + randomOffset;
                
                // Find ground height so creature doesn't spawn inside terrain.
                if (ZoneSystem.instance != null && ZoneSystem.instance.FindFloor(spawnPos, out float height))
                {
                    spawnPos.y = Mathf.Max(spawnPos.y, height);
                }
                
                Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                
                // Spawn creature SFX at each creature spawn position.
                if (!string.IsNullOrEmpty(creatureSfxPrefabName))
                {
                    GameObject sfxPrefab = PrefabManager.Instance.GetPrefab(creatureSfxPrefabName);
                    if (sfxPrefab != null) Object.Instantiate(sfxPrefab, spawnPos, Quaternion.identity);
                }
                
                // Spawn the creature.
                GameObject spawnedCreature = Object.Instantiate(creaturePrefab, spawnPos, rotation);
                
                // Set creature level if above 1.
                if (creatureLevel > 1)
                {
                    Character character = spawnedCreature.GetComponent<Character>();
                    if (character != null)
                    {
                        character.SetLevel(creatureLevel);
                    }
                }
            }
        }
    }
}

