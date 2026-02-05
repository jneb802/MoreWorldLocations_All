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

        AddVendorPrefab(assetBundle, "MWL_PlainsTavern1_Vendor", "$mwl_plainstavern1_trader");
        AddVendorPrefab(assetBundle, "MWL_PlainsCamp1_Vendor", "$mwl_plainscamp1_trader");
        AddVendorPrefab(assetBundle, "MWL_BlackForestBlacksmith1_Vendor", "$mwl_blackforestblacksmith1_trader");
        AddVendorPrefab(assetBundle, "MWL_BlackForestBlacksmith2_Vendor", "$mwl_blackforestblacksmith2_trader");
        AddVendorPrefab(assetBundle, "MWL_MountainsBlacksmith1_Vendor", "$mwl_mountainsblacksmith1_trader");
        AddVendorPrefab(assetBundle, "MWL_MistlandsBlacksmith1_Vendor", "$mwl_mistlandsblacksmith1_trader");
        AddVendorPrefab(assetBundle, "MWL_OceanTavern1_Vendor", "$mwl_oceantavern1_trader");
    }

    private static void AddVendorPrefab(AssetBundle assetBundle, string prefabName, string traderName)
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

        PrefabManager.Instance.AddPrefab(customPrefab);
    }
}
