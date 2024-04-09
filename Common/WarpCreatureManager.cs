using System.Collections.Generic;
using System.IO;
using Jotunn.Managers;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using Random = UnityEngine.Random;

namespace Meadows_Pack_1;

public static class WarpCreatureManager
{
    public static void SetupCreatures(WarpLocationManager.LocationPosition position, string creatureListName, GameObject jotunnLocationContainer, int creatureCount, string creatureYAMLContent)
    {
        if (creatureCount == 0)
        {
            return;
        }
        
        if (position == WarpLocationManager.LocationPosition.Exterior)
        {
            var exteriorCreatureList = CreateCreatureList(creatureListName,creatureCount,creatureYAMLContent);
            var exteriorCreatureSpawnerList = GetExteriorCreatureSpawners(jotunnLocationContainer);
            AddCreaturestoSpawnerList(exteriorCreatureSpawnerList,exteriorCreatureList);
        }
        else
        {
            var interiorCreatureList = CreateCreatureList(creatureListName,creatureCount,creatureYAMLContent);
            var interiorCreatureSpawnerList = GetInteriorCreatureSpawners(jotunnLocationContainer);
            AddCreaturestoSpawnerList(interiorCreatureSpawnerList,interiorCreatureList);
        }
    }
    
    public static GameObject GetCreaturePrefab(string prefabName)
    {   
        GameObject creaturePrefab = PrefabManager.Cache.GetPrefab<GameObject>(prefabName);
        if (creaturePrefab != null)
        {
            return creaturePrefab;
        }
        else
        {
            WarpLogger.Logger.LogError("Prefab not found for name:" + prefabName);
            return null;
        }
    }
    
    public static void AddCreaturetoSpawner(CreatureSpawner creatureSpawner, string creaturePrefab)
    {
        var creature = GetCreaturePrefab(creaturePrefab);
        if (creature != null)
        {
            creatureSpawner.m_creaturePrefab = creature;
            WarpLogger.Logger.LogDebug("Creature with name " + creaturePrefab + " was added to " + creatureSpawner.transform.parent.name);
 
        }
        else
        {
            WarpLogger.Logger.LogError("Creature not found for name: " + creaturePrefab);
 
        }
    }

    public static void AddCreaturestoSpawnerList(List<CreatureSpawner> CreatureSpawnerList, List<string> CreatureList)
    {
        int creatureIndex = 0;

        foreach (var spawner in CreatureSpawnerList)
        {
            // Use modulo to loop back to the start of the creature list if necessary
            string creatureName = CreatureList[creatureIndex % CreatureList.Count];
            
            // Add the creature to the spawner
            AddCreaturetoSpawner(spawner, creatureName);
            
            // Increment the creatureIndex to get the next creature for the next spawner
            creatureIndex++;
        }
    }

    public static List<CreatureSpawner> GetExteriorCreatureSpawners(GameObject location)
    {
        List<CreatureSpawner> locationExteriorSpawners = new List<CreatureSpawner>();
        
        CreatureSpawner[] allSpawners = location.GetComponentsInChildren<CreatureSpawner>();
        
        foreach (var spawner in allSpawners)
        {
            if (spawner.transform.parent != null && spawner.transform.position.y <= 5000)
            {
                locationExteriorSpawners.Add(spawner);
                WarpLogger.Logger.LogDebug("Exterior creature spawner found in " + location + "with name: " + spawner.name);
            }
        }

        return locationExteriorSpawners;
    }
    
    public static List<CreatureSpawner> GetInteriorCreatureSpawners(GameObject location)
    {
        List<CreatureSpawner> locationInteriorSpawners = new List<CreatureSpawner>();
        
        CreatureSpawner[] allSpawners = location.GetComponentsInChildren<CreatureSpawner>();

        foreach (var spawner in allSpawners)
        {
            if (spawner.transform.parent != null && spawner.transform.position.y >= 5000)
            {
                locationInteriorSpawners.Add(spawner);
                WarpLogger.Logger.LogDebug("Interior creature spawner found in " + location + " with name: " + spawner.transform.parent.name);
            }
        }

        return locationInteriorSpawners;
    }
    
    /*public static List<string> CreateCreatureList(string locationName, int creatureCount, string yamlContent)
    {
        List<string> locationCreatureList = new List<string>();
        
        
        var yaml = new YamlStream();
        yaml.Load(new StringReader(yamlContent));
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
        if (mapping.Children.ContainsKey(new YamlScalarNode(locationName)))
        {
            var creaturesNode = (YamlSequenceNode)mapping.Children[new YamlScalarNode(locationName)]["creatures"];
            int creaturesInNode = creaturesNode.Children.Count;
            
            int randomNumber = Random.Range(0, creaturesInNode-1);
            
            for (int i = 0; i < creatureCount; i++)
            {
                int index = (randomNumber + i) % creaturesInNode;
                var creature = (YamlScalarNode)creaturesNode.Children[index];
                locationCreatureList.Add(creature.Value);
            }
        }
        
        return locationCreatureList;
    }*/
    
    public static List<string> CreateCreatureList(string creatureListName, int creatureCount, string yamlContent)
    {
        List<string> locationCreatureList = new List<string>();
        
        
        var yaml = new YamlStream();
        yaml.Load(new StringReader(yamlContent));
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
        if (mapping.Children.ContainsKey(new YamlScalarNode(creatureListName)))
        {
            var creatureNames = mapping.Children[new YamlScalarNode(creatureListName)] as YamlSequenceNode;
            int creaturesInNode = creatureNames.Children.Count;
            
            int randomNumber = Random.Range(0, creaturesInNode-1);
            
            for (int i = 0; i < creatureCount; i++)
            {
                int index = (randomNumber + i) % creaturesInNode;
                var creature = (YamlScalarNode)creatureNames.Children[index];
                locationCreatureList.Add(creature.Value);
            }
        }
        
        return locationCreatureList;
    }
    
    
}