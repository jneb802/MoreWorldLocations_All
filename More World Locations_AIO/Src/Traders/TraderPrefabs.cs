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
        
        AddTraderPrefab(assetBundle, "MWL_VendorNPC", "MWL_PlainsTavern1_Trader", "$mwl_plainstavern1_trader");
        AddTraderPrefab(assetBundle, "MWL_VendorNPC", "MWL_PlainsCamp1_Trader", "$mwl_plainscamp1_trader");
        AddTraderPrefab(assetBundle, "MWL_VendorNPC", "MWL_BlackForestBlacksmith1_Trader", "$mwl_blackforestblacksmith1_trader");
        AddTraderPrefab(assetBundle, "MWL_VendorNPC", "MWL_BlackForestBlacksmith2_Trader", "$mwl_blackforestblacksmith2_trader");
        AddTraderPrefab(assetBundle, "MWL_VendorNPC", "MWL_MountainsBlacksmith1_Trader", "$mwl_mountainsblacksmith1_trader");
        AddTraderPrefab(assetBundle, "MWL_VendorNPC", "MWL_MistlandsBlacksmith1_Trader", "$mwl_mistlandsblacksmith1_trader");
    }
    
    private static void AddTraderPrefab(AssetBundle assetBundle, string basePrefabName, string cloneName, string traderName)
    {
        GameObject basePrefab = assetBundle.LoadAsset<GameObject>(basePrefabName);
        if (basePrefab == null)
        {
            Debug.LogWarning($"TraderPrefabs: Could not load {basePrefabName} from bundle");
            return;
        }
        
        GameObject clonedPrefab = PrefabManager.Instance.CreateClonedPrefab(cloneName, basePrefab);
        if (clonedPrefab == null)
        {
            Debug.LogWarning($"TraderPrefabs: Could not clone {basePrefabName} as {cloneName}");
            return;
        }
        
        CustomPrefab customPrefab = new CustomPrefab(clonedPrefab, true);
        Trader trader = customPrefab.Prefab.GetComponent<Trader>();
        trader.m_name = traderName;
        
        PrefabManager.Instance.AddPrefab(customPrefab);
    }
}
