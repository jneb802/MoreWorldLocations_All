using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Utils;

public class LocationCustomPrefabs
{
    public static void AddMarbleJail1Prefabs()
    {
        // Create a custom key cloned from CryptKey.
        GameObject key = Prefabs.AddKeyPrefab("MWL_MarbleJail1_Key", "CryptKey");
        if (key == null) return;
        
        ItemDrop keyItemDrop = key.GetComponent<ItemDrop>();
        keyItemDrop.m_itemData.m_shared.m_name = "$mwl_marblejail1_key";
        keyItemDrop.m_itemData.m_shared.m_description = "$mwl_marblejail1_key_desc";
        
        // Create a locked door that requires the custom key.
        GameObject door = Prefabs.AddDoorPrefab("MWL_MarbleJail1_iron_grate", "iron_grate");
        if (door != null)
        {
            door.GetComponent<Door>().m_keyItem = keyItemDrop;
        }
        
        // Create a pickable that gives the key.
        GameObject pickableItem = Prefabs.AddPickableItemPrefab("MWL_MarbleJail1_Pickable1", "Pickable_Item");
        if (pickableItem != null)
        {
            PickableItem pickable = pickableItem.GetComponent<PickableItem>();
            pickable.m_itemPrefab = keyItemDrop;
            pickable.m_stack = 1;
        }
        
        // Create a runestone with custom text and no map pin.
        GameObject runestone = Prefabs.AddRuneStonePrefab("MWL_MarbleJail1_Runestone1", "RuneStone_Mistlands_bosshint");
        if (runestone != null)
        {
            RuneStone rs = runestone.GetComponent<RuneStone>();
            rs.m_name = "$mwl_marblejail1_runestone";
            rs.m_text = "$mwl_marblejail1_runestone_text";
            
            // Remove map pin functionality.
            rs.m_pinName = "";
            rs.m_pinType = Minimap.PinType.None;
            rs.m_locationName = "";
        }
        
    }

    public static void AddMarbleCliffAltar1Prefabs()
    {
        // Clone the altar crystal that drops a key fragment when destroyed.
        // We add SpawnOnDestruction to also spawn a Gjall when the player breaks it.
        GameObject altarPrefab = PrefabManager.Instance.CreateClonedPrefab(
            "MWL_MarbleCliffAtlar1_blackmarble_altar_crystal", 
            "blackmarble_altar_crystal");
        
        if (altarPrefab == null)
        {
            Debug.LogWarning("LocationCustomPrefabs: Could not clone black_marble_altar_crystal");
            return;
        }
        
        // Add the spawn-on-destruction component to spawn creatures when destroyed.
        SpawnOnDestruction spawner = altarPrefab.AddComponent<SpawnOnDestruction>();
        spawner.creaturePrefabNames = new List<string> { "Gjall" };
        spawner.spawnOffset = new Vector3(0f, 20f, -10f);
        spawner.spawnCount = 2;
        spawner.creatureLevel = 3;
        spawner.spawnVfxPrefabName = "vfx_offering";         // Spawns once at altar
        spawner.spawnSfxPrefabName = "sfx_offering"; // Spawns once at altar
        spawner.creatureSfxPrefabName = "sfx_gjall_idle_vocals";                   // Spawns per creature
        
        // Register the prefab so it can be used in locations.
        CustomPrefab customPrefab = new CustomPrefab(altarPrefab, false);
        PrefabManager.Instance.AddPrefab(customPrefab);

        // Create a runestone with custom text and no map pin.
        GameObject runestone = Prefabs.AddRuneStonePrefab("MWL_MarbleCliffAltar1_Runestone1", "RuneStone_Mistlands_bosshint");
        if (runestone != null)
        {
            RuneStone rs = runestone.GetComponent<RuneStone>();
            rs.m_name = "$mwl_marblecliffaltar1_runestone";
            rs.m_text = "$mwl_marblecliffaltar1_runestone_text";
            
            // Remove map pin functionality.
            rs.m_pinName = "";
            rs.m_pinType = Minimap.PinType.None;
            rs.m_locationName = "";
        }
        
    }
}
