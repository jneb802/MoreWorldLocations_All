using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Traders;

public class TraderPrefabs
{
    public static void AddTraderPrefabs()
    {
        var assetBundle = Prefabs.vendorNpcBundle;

        AddVendorPrefab(assetBundle, "MWL_PlainsTavern1_Vendor", "$mwl_plainstavern1_trader", GetPlainsTavern1Items());
        AddVendorPrefab(assetBundle, "MWL_PlainsCamp1_Vendor", "$mwl_plainscamp1_trader", GetPlainsCamp1Items());
        AddVendorPrefab(assetBundle, "MWL_BlackForestBlacksmith1_Vendor", "$mwl_blackforestblacksmith1_trader", GetBlackForestBlacksmith1Items());
        AddVendorPrefab(assetBundle, "MWL_BlackForestBlacksmith2_Vendor", "$mwl_blackforestblacksmith2_trader", GetBlackForestBlacksmith2Items());
        AddVendorPrefab(assetBundle, "MWL_MountainsBlacksmith1_Vendor", "$mwl_mountainsblacksmith1_trader", GetMountainsBlacksmith1Items());
        AddVendorPrefab(assetBundle, "MWL_MistlandsBlacksmith1_Vendor", "$mwl_mistlandsblacksmith1_trader", GetMistlandsBlacksmith1Items());
        AddVendorPrefab(assetBundle, "MWL_OceanTavern1_Vendor", "$mwl_oceantavern1_trader", GetOceanTavern1Items());
    }

    private static void AddVendorPrefab(AssetBundle assetBundle, string prefabName, string traderName, List<Trader.TradeItem> items)
    {
        GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"TraderPrefabs: Could not load {prefabName} from bundle");
            return;
        }

        CustomPrefab customPrefab = new CustomPrefab(prefab, true);
        Trader trader = customPrefab.Prefab.GetComponent<Trader>();
        trader.m_name = traderName;
        trader.m_items = items;

        PrefabManager.Instance.AddPrefab(customPrefab);
    }

    private static Trader.TradeItem CreateTradeItem(string prefabName, int stack, int price, string requiredGlobalKey = "")
    {
        GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(prefabName);
        if (itemPrefab == null)
        {
            Debug.LogWarning($"TraderPrefabs: Could not find item prefab '{prefabName}'");
            return null;
        }

        ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();
        if (itemDrop == null)
        {
            Debug.LogWarning($"TraderPrefabs: Prefab '{prefabName}' has no ItemDrop component");
            return null;
        }

        return new Trader.TradeItem
        {
            m_prefab = itemDrop,
            m_stack = stack,
            m_price = price,
            m_requiredGlobalKey = requiredGlobalKey
        };
    }

    private static List<Trader.TradeItem> GetPlainsTavern1Items()
    {
        var items = new List<Trader.TradeItem>();

        // Tier 1 - Always available
        AddItem(items, "QueensJam", 1, 20);
        AddItem(items, "CarrotSoup", 1, 20);
        AddItem(items, "DeerStew", 1, 20);
        AddItem(items, "MincedMeatSauce", 1, 20);

        // Tier 2 - After Bonemass
        AddItem(items, "Sausages", 1, 30, "defeated_bonemass");
        AddItem(items, "TurnipStew", 1, 30, "defeated_bonemass");
        AddItem(items, "BlackSoup", 1, 30, "defeated_bonemass");
        AddItem(items, "ShocklateSmoothie", 1, 30, "defeated_bonemass");

        // Tier 3 - After Moder
        AddItem(items, "OnionSoup", 1, 40, "defeated_dragon");
        AddItem(items, "WolfMeatSkewer", 1, 40, "defeated_dragon");
        AddItem(items, "Eyescream", 1, 40, "defeated_dragon");

        // Tier 4 - After Yagluth
        AddItem(items, "BloodPudding", 1, 50, "defeated_goblinking");
        AddItem(items, "FishWraps", 1, 50, "defeated_goblinking");
        AddItem(items, "LoxPie", 1, 50, "defeated_goblinking");

        // Tier 5 - After The Queen
        AddItem(items, "MeatPlatter", 1, 60, "defeated_queen");
        AddItem(items, "Salad", 1, 60, "defeated_queen");
        AddItem(items, "HoneyGlazedChicken", 1, 60, "defeated_queen");
        AddItem(items, "MisthareSupreme", 1, 60, "defeated_queen");
        AddItem(items, "MushroomOmelette", 1, 60, "defeated_queen");
        AddItem(items, "StuffedMushroom", 1, 60, "defeated_queen");
        AddItem(items, "YggdrasilPorridge", 1, 60, "defeated_queen");
        AddItem(items, "SeekerAspic", 1, 60, "defeated_queen");

        // Tier 6 - After Fader
        AddItem(items, "FierySvinstew", 1, 70, "defeated_fader");
        AddItem(items, "PiquantPie", 1, 70, "defeated_fader");
        AddItem(items, "SpicyMarmalade", 1, 70, "defeated_fader");
        AddItem(items, "ScorchingMedley", 1, 70, "defeated_fader");
        AddItem(items, "RoastedCrustPie", 1, 70, "defeated_fader");
        AddItem(items, "SizzlingBerryBroth", 1, 70, "defeated_fader");
        AddItem(items, "SparklingShroomshake", 1, 70, "defeated_fader");
        AddItem(items, "MarinatedGreens", 1, 70, "defeated_fader");

        return items;
    }

    private static List<Trader.TradeItem> GetPlainsCamp1Items()
    {
        var items = new List<Trader.TradeItem>();

        // No key required
        AddItem(items, "Tar", 1, 5);
        AddItem(items, "BlackMetalScrap", 1, 15);
        AddItem(items, "Needle", 1, 5);
        AddItem(items, "Cloudberry", 1, 2);
        AddItem(items, "Feathers", 1, 5);
        AddItem(items, "LoxPelt", 1, 30);
        AddItem(items, "LinenThread", 1, 20);
        AddItem(items, "CookedLoxMeat", 1, 20);
        AddItem(items, "LoxSaddle", 1, 500);
        AddItem(items, "BoltBlackmetal", 1, 20);

        // After Yagluth - bulk quantities
        AddItem(items, "Tar", 10, 40, "defeated_goblinking");
        AddItem(items, "Tar", 50, 200, "defeated_goblinking");
        AddItem(items, "Cloudberry", 10, 16, "defeated_goblinking");
        AddItem(items, "Cloudberry", 50, 75, "defeated_goblinking");

        return items;
    }

    private static List<Trader.TradeItem> GetBlackForestBlacksmith1Items()
    {
        var items = new List<Trader.TradeItem>();
        AddItem(items, "CopperOre", 1, 10);
        AddItem(items, "MWL_blacksmithStone_tier1", 1, 500, "defeated_gdking");
        return items;
    }

    private static List<Trader.TradeItem> GetBlackForestBlacksmith2Items()
    {
        var items = new List<Trader.TradeItem>();
        AddItem(items, "CopperOre", 1, 10);
        AddItem(items, "MWL_blacksmithStone_tier1", 1, 500, "defeated_gdking");
        return items;
    }

    private static List<Trader.TradeItem> GetMountainsBlacksmith1Items()
    {
        var items = new List<Trader.TradeItem>();
        AddItem(items, "Obsidian", 1, 15);
        AddItem(items, "MWL_blacksmithStone_tier2", 1, 750, "defeated_gdking");
        return items;
    }

    private static List<Trader.TradeItem> GetMistlandsBlacksmith1Items()
    {
        var items = new List<Trader.TradeItem>();
        AddItem(items, "BlackMarble", 1, 20);
        AddItem(items, "MWL_blacksmithStone_tier3", 1, 1000, "defeated_gdking");
        return items;
    }

    private static List<Trader.TradeItem> GetOceanTavern1Items()
    {
        var items = new List<Trader.TradeItem>();

        // Always available
        AddItem(items, "FishCooked", 1, 30);

        // After Serpent
        AddItem(items, "SerpentMeatCooked", 1, 40, "defeated_serpent");
        AddItem(items, "SerpentStew", 1, 50, "defeated_serpent");
        AddItem(items, "SpiceOceans", 5, 100, "defeated_serpent");

        // After Yagluth
        AddItem(items, "FishWraps", 1, 50, "defeated_goblinking");
        AddItem(items, "SerpentScale", 1, 20, "defeated_goblinking");

        // After Fader
        AddItem(items, "FishAndBread", 1, 70, "defeated_fader");
        AddItem(items, "CookedBoneMawSerpentMeat", 1, 70, "defeated_fader");

        return items;
    }

    private static void AddItem(List<Trader.TradeItem> items, string prefabName, int stack, int price, string requiredGlobalKey = "")
    {
        var item = CreateTradeItem(prefabName, stack, price, requiredGlobalKey);
        if (item != null)
            items.Add(item);
    }
}
