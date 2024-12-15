using Common;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Traders;

public class Traders
{
    public static void AddAllTraderLists()
    {
        Debug.Log("Log 2");
        YAMLManager yamlManager = More_World_TradersPlugin.moreWorldTradersYAMLManager;
        Debug.Log("Log 3");
        TraderManager.BuildBuyItemList(yamlManager.defaultTraderYamlContent, "PlainsTavernTrader1");
        Debug.Log("Log 4");
        TraderManager.BuildSellItemList(yamlManager.defaultTraderYamlContent, "PlainsTavernTrader1");

        PrefabManager.OnVanillaPrefabsAvailable -= AddAllTraderLists;
    }
    
    
}