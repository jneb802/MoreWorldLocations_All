using System;
using System.Collections.Generic;
using System.IO;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;
using System.Globalization;

namespace Common;

public class LootManager: MonoBehaviour
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

    public static bool isLootListVersion2(string yamlContent)
    {
        var yaml = new YamlStream();
        yaml.Load(new StringReader(yamlContent));
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
        if (mapping.Children.ContainsKey(new YamlScalarNode("version")))
        {
            var versionNode = (YamlScalarNode)mapping.Children[new YamlScalarNode("version")];
            string version = versionNode.Value;
            
            if (version == "2.0")
            {
                WarpLogger.Logger.LogDebug("Version is 2");
                return true;
            }
        }
        else
        {
            WarpLogger.Logger.LogDebug("Version not found in YAML file");
            return false;
        }

        return false;
    }
    
    
    public static List<String> ParseContainerYaml_v1(string lootListName, string yamlContent)
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
    
    public static List<DropTable.DropData> ParseContainerYaml_v2(string lootListName, string yamlContent)
    {
        List<DropTable.DropData> dropDataList = new List<DropTable.DropData>();
        
        var modifiedYamlContent = RemoveVersionFromYaml(yamlContent);
        
        var deserializer = new DeserializerBuilder().Build();
        var lootData = deserializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(modifiedYamlContent);
        
        if (lootData.ContainsKey(lootListName))
        {
            WarpLogger.Logger.LogDebug("Found loot list with name " + lootListName + " in loot list Yaml file");
            
            foreach (var itemData in lootData[lootListName])
            {
                string itemName = itemData["item"].ToString();
                GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(itemName);

                if (itemPrefab != null)
                {
                    int stackMinValue;
                    if (!int.TryParse(itemData["stackMin"].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out stackMinValue))
                    {
                        stackMinValue = 2;
                        WarpLogger.Logger.LogWarning("Failed to parse stackMin for item " + itemName + ". Defaulting to " + stackMinValue);
                        
                    }
                    
                    int stackMaxValue;
                    if (!int.TryParse(itemData["stackMax"].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out stackMaxValue))
                    {
                        stackMaxValue = 3;
                        WarpLogger.Logger.LogWarning("Failed to parse stackMax for item " + itemName + ". Defaulting to " + stackMaxValue);
                        
                    }
                    
                    float weightValue;
                    if (!float.TryParse(itemData["weight"].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out weightValue))
                    {
                        weightValue = 1.0f;
                        WarpLogger.Logger.LogWarning("Failed to parse weight for item " + itemName + ". Defaulting to " + weightValue);
                        
                    }
                    
                    var dropData = new DropTable.DropData
                    {
                        m_item = itemPrefab,
                        m_stackMin = stackMinValue,
                        m_stackMax = stackMaxValue,
                        m_weight = weightValue,
                        m_dontScale = false
                    };
                    
                    dropDataList.Add(dropData);
                    WarpLogger.Logger.LogDebug("Added item with name: " + itemName + " to loot list " + lootListName + " with stackMin: " + dropData.m_stackMin + ", stackMax: " + dropData.m_stackMax + ", weight: " + dropData.m_weight);
                }
                else
                {
                    WarpLogger.Logger.LogWarning("Prefab for item " + itemName + " not found.");
                }
            }
        }
        else
        {
            WarpLogger.Logger.LogError("Failed to find loot list with name: " + lootListName + " in loot list Yaml file");
        }

        return dropDataList;
    }
    
    private static string RemoveVersionFromYaml(string yamlContent)
    {
        var lines = yamlContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var filteredLines = new List<string>();

        bool versionFound = false;

        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("version"))
            {
                versionFound = true;
                continue; // Skip this line
            }
            filteredLines.Add(line);
        }

        if (versionFound)
        {
            WarpLogger.Logger.LogDebug("Version key found and removed from YAML content.");
        }

        return string.Join("\n", filteredLines);
    }
    
    public static List<Container> GetLocationsContainers(GameObject location)
    {
        List<Container> locationContainers = new List<Container>();
        Container[] allContainers = location.GetComponentsInChildren<Container>();

        if (allContainers.Length > 0)
        {
            foreach (var container in allContainers)
            {
                locationContainers.Add(container);
                WarpLogger.Logger.LogDebug("Container found in " + location + "with name: " + container.name);
            } 
        }
        
        return locationContainers;
    }
    
    public static List<Container> GetLocationsContainers(GameObject location, LocationManager.LocationPosition locationPosition)
    {
        List<Container> locationContainers = new List<Container>();
        Container[] allContainers = location.GetComponentsInChildren<Container>();
    
        foreach (var container in allContainers)
        {
            if (container.transform.position.y <= 5000)
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
            
            WarpLogger.Logger.LogDebug("Container with name " + container.name + " has received new dropTable");
        }
    }
    
    public static void SetupChestLoot(List<Container> containerList, List<DropTable.DropData> lootList)
    {
        DropTable dropTable = new DropTable();
        dropTable.m_dropMin = 2;
        dropTable.m_dropMax = 3;
        dropTable.m_drops = lootList;

        if (containerList.Count == 0)
        {
            return;
        }
        
        foreach (var container in containerList)
        {
            container.m_defaultItems = dropTable;
            WarpLogger.Logger.LogDebug("Container with name " + container.name + " has received new dropTable");
        }
    }
    
    public static List<DropOnDestroyed> GetLocationsDropOnDestroyeds(GameObject location, LocationManager.LocationPosition locationPosition)
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
                WarpLogger.Logger.LogDebug("Attempting to add item with name " + itemName);
                
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
                    
                    WarpLogger.Logger.LogDebug("Added item with name " + itemName);
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
