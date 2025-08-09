using System.Reflection;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace More_World_Locations_AIO.Shipments;

public class PortPrefabs
{
    public static AssetBundle portsBundle;
    public static GameObject portUI;
    public static GameObject portUIListItem;
    
    public static void LoadPrefabBundles()
    {
        portsBundle = AssetUtils.LoadAssetBundleFromResources(
            "ports",
            Assembly.GetExecutingAssembly());
    }
    
    public static void AddPortUIPrefabs()
    {
        portUI = portsBundle.LoadAsset<GameObject>("PortUI1");
        if (portUI == null)
        {
            Debug.LogError("Error: Could not load PortUI1 prefab from bundle");
        }
        
        portUIListItem = portsBundle.LoadAsset<GameObject>("ListItem");
        if (portUI == null)
        {
            Debug.LogError("Error: Could not load PortUI1 prefab from bundle");
        }
    }
    
    public static void AddPortPrefabs()
    {
        GameObject portTraderPrefab = portsBundle.LoadAsset<GameObject>("PortTrader");
        if (portTraderPrefab == null)
        {
            Debug.LogError("Error: Could not load Port Trader prefab from bundle");
        }
        CustomPrefab portTraderCustomPrefab = new CustomPrefab(portTraderPrefab, true);
        portTraderCustomPrefab.Prefab.AddComponent<Port>();
        PrefabManager.Instance.AddPrefab(portTraderCustomPrefab);
    }
}