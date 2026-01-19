using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Gambling;

/// <summary>
/// Creates the GoblinGambler prefab by cloning a Goblin and adding the gambling component.
/// </summary>
public static class GoblinGamblerPrefabs
{
    public static void AddGoblinGamblerPrefab()
    {
        // Clone the base Goblin creature
        GameObject goblinPrefab = PrefabManager.Instance.CreateClonedPrefab(
            "MWL_GoblinGambler", 
            "Goblin");
        
        if (goblinPrefab == null)
        {
            Debug.LogWarning("GoblinGamblerPrefabs: Could not clone Goblin");
            return;
        }
        
        // Configure humanoid to be friendly and hide from EnemyHud
        Humanoid humanoid = goblinPrefab.GetComponent<Humanoid>();
        if (humanoid != null)
        {
            humanoid.m_faction = Character.Faction.Players; // Friendly to players
            humanoid.m_name = "$mwl_goblin_gambler";
            humanoid.m_boss = false;
            humanoid.m_health = 99999f; // Effectively invincible
        }
        
        // Remove the Character component's collider hover by adding an interact collider child
        // The GoblinGambler component will handle the interaction
        GameObject interactPoint = new GameObject("InteractPoint");
        interactPoint.transform.SetParent(goblinPrefab.transform);
        interactPoint.transform.localPosition = Vector3.up * 1f; // Chest height
        
        // Add sphere collider for interaction detection
        SphereCollider interactCollider = interactPoint.AddComponent<SphereCollider>();
        interactCollider.radius = 1.5f;
        interactCollider.isTrigger = true;
        
        // Move the GoblinGambler component to the interact point so it's found first
        interactPoint.AddComponent<GoblinGambler>();
        
        // Configure MonsterAI to be passive - don't destroy it or EnemyHud breaks
        MonsterAI monsterAI = goblinPrefab.GetComponent<MonsterAI>();
        if (monsterAI != null)
        {
            monsterAI.m_viewRange = 0f;           // Can't see players
            monsterAI.m_viewAngle = 0f;           // No view angle
            monsterAI.m_hearRange = 0f;           // Can't hear
            monsterAI.m_alertRange = 0f;          // Won't alert
            monsterAI.m_fleeIfLowHealth = 0f;     // Won't flee
            monsterAI.m_circulateWhileCharging = false;
            monsterAI.m_interceptTimeMin = 0f;
            monsterAI.m_interceptTimeMax = 0f;
            monsterAI.m_maxChaseDistance = 0f;    // Won't chase
            monsterAI.m_minAttackInterval = 9999f; // Effectively never attacks
            monsterAI.m_randomMoveRange = 0f;     // Won't wander
            monsterAI.m_randomMoveInterval = 9999f;
        }
        
        // Remove Tameable if present
        Tameable tameable = goblinPrefab.GetComponent<Tameable>();
        if (tameable != null)
        {
            Object.Destroy(tameable);
        }
        
        // Remove CharacterDrop so it doesn't drop loot if somehow killed
        CharacterDrop characterDrop = goblinPrefab.GetComponent<CharacterDrop>();
        if (characterDrop != null)
        {
            Object.Destroy(characterDrop);
        }
        
        // Register the prefab
        CustomPrefab customPrefab = new CustomPrefab(goblinPrefab, false);
        PrefabManager.Instance.AddPrefab(customPrefab);
        
        // Add localizations
        AddGoblinGamblerLocalizations();
        
        Debug.Log("GoblinGamblerPrefabs: Successfully created MWL_GoblinGambler");
    }
    
    private static void AddGoblinGamblerLocalizations()
    {
        var localization = LocalizationManager.Instance.GetLocalization();
        
        localization.AddTranslation("English", new Dictionary<string, string>
        {
            { "$mwl_goblin_gambler", "Goblin Gambler" }
        });
    }
}
