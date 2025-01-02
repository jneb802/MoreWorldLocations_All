using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Common;

public static class LootManager_v2
{
   public static void SetupContainers(GameObject gameObject, string yamlContent)
   {
      List<Container> containers = GetContainers(gameObject);
      List<DropTable.DropData> dropDataList = ParseContainerYAML(yamlContent);
      foreach (Container container in containers)
      {
         container.m_defaultItems.m_drops = dropDataList;
      }
   }
   
   public static void SetupPickableItems(GameObject gameObject, string yamlContent, string listName)
   {
      List<PickableItem> pickableItems = GetPickableItems(gameObject);
      List<PickableItem.RandomItem> randomItemsList = ParsePickableYaml(listName, yamlContent);
      PickableItem.RandomItem[] randomItemsArray = randomItemsList.ToArray();
      foreach (PickableItem pickableItem in pickableItems)
      {
         pickableItem.m_randomItemPrefabs = randomItemsArray;
      }
   }
   
   public static List<DropTable.DropData> ParseContainerYAML(string yamlContent)
   {
      List<DropTable.DropData> dropDataList = new List<DropTable.DropData>();
      
      var deserializer = new DeserializerBuilder().Build();
      var lootData = deserializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(yamlContent);

      foreach (var lootCategory in lootData.Values)
      {
         foreach (var itemData in lootCategory)
         {
            string itemName = itemData["item"].ToString();
            GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(itemName);

            if (itemPrefab != null)
            {
               var dropData = new DropTable.DropData
               {
                  m_item = itemPrefab,
                  m_stackMin = int.Parse(itemData["stackMin"].ToString()),
                  m_stackMax = int.Parse(itemData["stackMax"].ToString()),
                  m_weight = float.Parse(itemData["weight"].ToString()),
                  m_dontScale = false
               };
               dropDataList.Add(dropData);
            }
         }
      }
      
      return dropDataList;
   }

   public static List<PickableItem.RandomItem> ParsePickableYaml(string pickablelootListName, string yamlContent)
    {
        List<PickableItem.RandomItem> randomItemsList = new List<PickableItem.RandomItem>();
        
        var deserializer = new DeserializerBuilder().Build();
        var lootData = deserializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(yamlContent);
        
        if (lootData.ContainsKey(pickablelootListName))
        {
            WarpLogger.Logger.LogDebug("Found loot list with name " + pickablelootListName + " in pickable list Yaml file");
            
            foreach (var itemData in lootData[pickablelootListName])
            {
                string itemName = itemData["item"].ToString();
                GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(itemName);

                if (itemPrefab != null)
                {
                    
                    ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();

                        if (itemDrop != null)
                        {
                            var randomItem = new PickableItem.RandomItem
                            {
                                m_itemPrefab = itemDrop,
                                m_stackMin = int.Parse(itemData["stackMin"].ToString()),
                                m_stackMax = int.Parse(itemData["stackMax"].ToString())
                            };
                            randomItemsList.Add(randomItem);
                            WarpLogger.Logger.LogDebug("Added item with name: " + itemName + " to pickable list " + pickablelootListName + " with stackMin: " + randomItem.m_stackMin + ", stackMax: " + randomItem.m_stackMax);
                        }   
                }
                else
                {
                    WarpLogger.Logger.LogWarning("Prefab for item " + itemName + " not found.");
                }
            }
        }
        else
        {
            WarpLogger.Logger.LogError("Failed to find loot list with name: " + pickablelootListName + " in loot list Yaml file");
        }

        return randomItemsList;
    }

   public static List<Container> GetContainers(GameObject room)
   {
      return room.GetComponentsInChildren<Container>().ToList();
   }
   
   public static List<PickableItem> GetPickableItems(GameObject room)
   {
      return room.GetComponentsInChildren<PickableItem>().ToList();
   }
}
