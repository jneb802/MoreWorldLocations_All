using System;
using System.Collections.Generic;
using System.IO;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;

namespace Meadows_Pack_1;

public class WarpLootManager: MonoBehaviour
{
    public static List<String> CreateLootList(string lootListName, string yamlContent)
    {
        var yaml = new YamlStream();
        yaml.Load(new StringReader(yamlContent));
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
        List<string> lootList = new List<string>();

        if (mapping.Children.ContainsKey(new YamlScalarNode(lootListName)))
        {
            WarpLogger.Logger.LogDebug("Found loot list with name " + lootListName + " in loot list Yaml file");
            var lootItems = mapping.Children[new YamlScalarNode(lootListName)] as YamlSequenceNode;
            
            foreach (var item in lootItems)
            {
                var scalar = item as YamlScalarNode;
                lootList.Add(scalar.Value);
                WarpLogger.Logger.LogDebug("Added item with name: " + item + " to loot list with name " + lootListName);
            }
        }
        else
        {
            WarpLogger.Logger.LogError("Failed to find loot list with name: " + lootListName + " in loot list Yaml file");
        }

        return lootList;
    }
    
    public static List<Container> GetLocationsContainers(GameObject location, WarpLocationManager.LocationPosition locationPosition)
    {
        List<Container> locationContainers = new List<Container>();
        Container[] allContainers = location.GetComponentsInChildren<Container>();
    
        foreach (var container in allContainers)
        {
            if (container.name.StartsWith("loot_chest") && container.transform.position.y <= 5000)
            {
                locationContainers.Add(container);
                WarpLogger.Logger.LogDebug("Container found in " + location + "with name: " + container.name);
            }
        }
        
        return locationContainers;
    }
    
    public static void SetupChestLoot(List<Container> containerList, List<String> lootList)
    {
        foreach (var container in containerList)
        {
            var dropTable = CreateDropTable(lootList, 2, 3);
            container.m_defaultItems = dropTable;
        }
    }
    
    public static List<DropOnDestroyed> GetLocationsDropOnDestroyeds(GameObject location, WarpLocationManager.LocationPosition locationPosition)
    {
        List<DropOnDestroyed> locationDropOnDestroyeds = new List<DropOnDestroyed>();
        DropOnDestroyed[] allDropOnDestroyeds = location.GetComponentsInChildren<DropOnDestroyed>();
    
        foreach (var dropOnDestroyed in allDropOnDestroyeds)
        {
            if (dropOnDestroyed.name.StartsWith("loot_drop") && dropOnDestroyed.transform.position.y <= 5000)
            {
                locationDropOnDestroyeds.Add(dropOnDestroyed);
                WarpLogger.Logger.LogDebug("DropOnDestroyed found in " + location + "with name: " + dropOnDestroyed.name);
            }
        }

        if (locationDropOnDestroyeds.Count > 0)
        {
            return locationDropOnDestroyeds;  
        }
        else
        {
            return null;
        }
    }
    
    public static void SetupDropOnDestroyedLoot(List<DropOnDestroyed> dropOnDestroyedList, List<String> lootList)
    {
        foreach (var dropOnDestroyed in dropOnDestroyedList)
        {
            var dropTable = CreateDropTable(lootList, 1, 2);
            dropOnDestroyed.m_dropWhenDestroyed = dropTable;
        }
    }

    public static DropTable CreateDropTable(List<string> itemNames, int dropMin, int dropMax)
        {
            DropTable newDropTable = new DropTable
            {
                m_dropMin = dropMin,
                m_dropMax = dropMax,
                m_dropChance = 1.0f,
                m_oneOfEach = true,
            };

            foreach (var itemName in itemNames)
            {
                GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(itemName);

                if (itemPrefab != null)
                {
                    DropTable.DropData dropData = new DropTable.DropData
                    {
                        m_item = itemPrefab,
                        m_stackMin = 2,
                        m_stackMax = 4,
                        m_weight = 1.0f,
                        m_dontScale = false
                    };

                    newDropTable.m_drops.Add(dropData);
                }
                else
                {
                    WarpLogger.Logger.LogError("Prefab for " + itemName + " not found");
                }
            }

            return newDropTable;
        }

        public static void AddContainerToChild(GameObject parentGameObject, string childName, DropTable dropTable)
        {
            // Find the child GameObject by name
            Transform childTransform = parentGameObject.transform.Find(childName);

            // Check if the child was found
            if (childTransform != null)
            {
                // Add the Container component to the child GameObject
                Container container = childTransform.gameObject.AddComponent<Container>();
                if (container != null)
                {
                    // Configure the Container properties
                    container.m_defaultItems = dropTable;
                    container.m_name = "Chest";
                    container.m_width = 4;
                    container.m_height = 2;
                }
            }
            else
            {
                WarpLogger.Logger.LogError("Child GameObject (" + childName + ") not found in parent GameObject (" + parentGameObject + ")");
            }
        }
        
        
}
