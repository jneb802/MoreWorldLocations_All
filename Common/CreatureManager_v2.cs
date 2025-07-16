using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using Random = UnityEngine.Random;

namespace Common;

public static class CreatureManager_v2
{
    public static void AddCreatureToSpawner(GameObject spawnerPrefab, List<GameObject> creaturePrefabs)
    {
        CreatureSpawner creatureSpawner = spawnerPrefab.GetComponent<CreatureSpawner>();
        
        int randomIndex = Random.Range(0, creaturePrefabs.Count);
        
        creatureSpawner.m_creaturePrefab = creaturePrefabs[randomIndex];
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

    public static List<GameObject> CreateCreatureList(string creatureListName, string yamlContent)
    {
        List<GameObject> locationCreatureList = new List<GameObject>();
        
        var yaml = new YamlStream();
        yaml.Load(new StringReader(yamlContent));
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
        if (mapping.Children.ContainsKey(new YamlScalarNode(creatureListName)))
        {
            var creatureNames = mapping.Children[new YamlScalarNode(creatureListName)] as YamlSequenceNode;
            
            foreach (var creatureNode in creatureNames.Children)
            {
                if (creatureNode is YamlScalarNode creature)
                {
                    locationCreatureList.Add(GetCreaturePrefab(creature.Value));
                }
            }
        }
        
        return locationCreatureList;
    }
}