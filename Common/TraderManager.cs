using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Common
{
    public static class TraderManager
    {
        public static Dictionary<string, List<Trader.TradeItem>> buyItemLists = new Dictionary<string, List<Trader.TradeItem>>();
        
        public static void AddTraderBuyItems(GameObject locationContainer, List<Trader.TradeItem> traderItems)
        {
            Trader trader = locationContainer.GetComponentInChildren<Trader>();
            if (trader == null)
            {
                WarpLogger.Logger.LogWarning("Failed to find Trader script in location with name: " + locationContainer);
                return;
            }

            trader.m_items = traderItems;
        }

        public static void BuildBuyItemList(string traderItemYamlContent, string traderName)
        {
            List<Trader.TradeItem> buyItems = new List<Trader.TradeItem>();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var parsedData = deserializer.Deserialize<Dictionary<string, List<TradeItemYAML>>>(traderItemYamlContent);
            if (parsedData == null || !parsedData.ContainsKey(traderName))
            {
                WarpLogger.Logger.LogWarning("Parsed data is null or trader name: " + traderName + " does not exist in the YAML content.");
                return;
            }
            var traderList = parsedData[traderName];

            foreach (var item in traderList)
            {
                GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(item.PrefabName);
                if (itemPrefab == null)
                {
                    WarpLogger.Logger.LogWarning("Failed to find prefab with name: " + item.PrefabName);
                    continue;
                }

                ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                {
                    WarpLogger.Logger.LogWarning("Failed to find ItemDrop component on prefab: " + item.PrefabName);
                    continue;
                }

                Trader.TradeItem tradeItem = new Trader.TradeItem
                {
                    m_prefab = itemDrop,
                    m_stack = item.Stack,
                    m_price = item.Price,
                    m_requiredGlobalKey = item.RequiredGlobalKey
                };
                buyItems.Add(tradeItem);
                Debug.Log("Added item with name: " + item.PrefabName + " to trader buy list with name: " + traderName);
            }

            buyItemLists.Add(traderName,buyItems);
        }
        
        public class TradeItemYAML
        {
            public string PrefabName { get; set; }
            public int Stack { get; set; } = 1;
            public int Price { get; set; }
            public string RequiredGlobalKey { get; set; } = "";
        }

        public class TrainerTierConfig
        {
            public int Tier { get; set; }
            public int Price { get; set; }
            public string RequiredGlobalKey { get; set; } = "";
            public string NotRequiredGlobalKey { get; set; } = "";
        }

        public class TrainerYAML
        {
            public List<TrainerTierConfig> Tiers { get; set; }
        }
    }
}