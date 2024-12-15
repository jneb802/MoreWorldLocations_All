using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Common
{
    public static class TraderManager
    {
        public static Dictionary<string, List<Trader.TradeItem>> buyItemLists;
        public static Dictionary<string, List<SellItem>> sellItemLists;
        
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
                GameObject itemPrefab = PrefabManager.Instance.GetPrefab(item.PrefabName);
                if (itemPrefab == null)
                {
                    WarpLogger.Logger.LogWarning("Failed to find prefab with name: " + item.PrefabName);
                }

                if (item.Action != "buy")
                {
                    continue;
                }
                
                ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();

                if (item.Quality != 0)
                {
                    itemDrop.SetQuality(item.Quality);
                }
                
                Trader.TradeItem tradeItem = new Trader.TradeItem
                {
                    m_prefab = itemDrop,
                    m_stack = item.Stack,
                    m_price = item.Price,
                    m_requiredGlobalKey = item.RequiredGlobalKey
                };
                buyItems.Add(tradeItem);
            }

            buyItemLists.Add(traderName,buyItems);
        }
        
        public static void BuildSellItemList(string traderItemYamlContent, string traderName)
        {
            List<SellItem> sellItems = new List<SellItem>();
            
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var parsedData = deserializer.Deserialize<Dictionary<string, List<TradeItemYAML>>>(traderItemYamlContent);

            var traderList = parsedData[traderName];
            
            foreach (var item in traderList)
            {
                GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(item.PrefabName);
                if (itemPrefab == null)
                {
                    WarpLogger.Logger.LogWarning("Failed to find prefab with name: " + item.PrefabName);
                }

                if (item.Action != "sell")
                {
                    continue;
                }
                
                ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();

                if (item.Quality != 0)
                {
                    itemDrop.SetQuality(item.Quality);
                }
                
                SellItem tradeItem = new SellItem
                {
                    m_prefab = itemDrop,
                    m_stack = item.Stack,
                    m_price = item.Price,
                    m_requiredGlobalKey = item.RequiredGlobalKey
                };
                sellItems.Add(tradeItem);
            }
            
            sellItemLists.Add(traderName, sellItems);
        }
        
        public class TradeItemYAML
        {
            public string PrefabName { get; set; }
            public string Action { get; set; }
            public int Quality { get; set; }
            public int Stack { get; set; }
            public int Price { get; set; }
            public string RequiredGlobalKey { get; set; }
        }
        
        public class SellItem
        {
            public ItemDrop m_prefab { get; set; }
            public int m_stack { get; set; }
            public int m_price { get; set; }
            public string m_requiredGlobalKey { get; set; }
        }
    }
}