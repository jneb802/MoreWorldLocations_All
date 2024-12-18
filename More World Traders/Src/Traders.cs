using Common;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Traders;

public class Traders
{
    public static void AddAllTraderLists()
    {
        YAMLManager yamlManager = More_World_TradersPlugin.moreWorldTradersYAMLManager;
        TraderManager.BuildBuyItemList(yamlManager.defaultTraderYamlContent, "PlainsTavernTrader1");
        // TraderManager.BuildSellItemList(yamlManager.defaultTraderYamlContent, "PlainsTavernTrader1");

        PrefabManager.OnVanillaPrefabsAvailable -= AddAllTraderLists;
    }
}