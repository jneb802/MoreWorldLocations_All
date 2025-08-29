using System.Reflection;
using Jotunn.Configs;
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
    public static AssetBundle portsLocationBundle;
    public static GameObject portUI;
    public static GameObject portUIListItem;
    
    public static void LoadPrefabBundles()
    {
        portsBundle = AssetUtils.LoadAssetBundleFromResources(
            "ports",
            Assembly.GetExecutingAssembly());
        
        portsLocationBundle = AssetUtils.LoadAssetBundleFromResources(
            "portlocation",
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
        
        GameObject portContainerWoodChest = portsBundle.LoadAsset<GameObject>("MWL_portContainer_woodChest");
        CustomPrefab MWL_portContainer_woodChest = new CustomPrefab(portContainerWoodChest, true);
        PrefabManager.Instance.AddPrefab(MWL_portContainer_woodChest);
        
        GameObject portContainerFinewoodChest = portsBundle.LoadAsset<GameObject>("MWL_portContainer_finewoodChest");
        CustomPrefab MWL_portContainer_finewoodChest = new CustomPrefab(portContainerFinewoodChest, true);
        PrefabManager.Instance.AddPrefab(MWL_portContainer_finewoodChest);
        
        GameObject portContainerBlackmetalChest = portsBundle.LoadAsset<GameObject>("MWL_portContainer_blackmetalChest");
        CustomPrefab MWL_portContainer_blackmetalChest = new CustomPrefab(portContainerBlackmetalChest, true);
        PrefabManager.Instance.AddPrefab(MWL_portContainer_blackmetalChest);
        
    }

    public static void AddPortLocation()
    {
        LocationConfig locationConfig = new LocationConfig
        {
            Quantity = 20, Biome = Heightmap.Biome.Meadows, Group = "Port", Priotized = true, RandomRotation = false,
            ExteriorRadius = 8, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f,
            MaxTerrainDelta = 10f, MinAltitude = 0f, MinDistance = LocationConfigs.LocationRings.Ring2.MinDistance,
            InForest = false, BiomeArea = Heightmap.BiomeArea.Median
        };
        
        LocationConfig locationConfig2 = new LocationConfig
        {
            Quantity = 20, Biome = Heightmap.Biome.Meadows, Group = "Port", Priotized = true, RandomRotation = false,
            ExteriorRadius = 16, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, SlopeRotation = true,
            MaxTerrainDelta = 10f, MinAltitude = 0f, MaxAltitude = 1f, MinDistance = LocationConfigs.LocationRings.Ring2.MinDistance,
            InForest = false, BiomeArea = Heightmap.BiomeArea.Everything
        };
        
        LocationConfig locationConfig3 = new LocationConfig
        {
            Quantity = 20, Biome = Heightmap.Biome.BlackForest, Group = "Port", Priotized = true, RandomRotation = false,
            ExteriorRadius = 16, ClearArea = true, MinDistanceFromSimilar = 1024, MinTerrainDelta = 0f, SlopeRotation = true,
            MaxTerrainDelta = 10f, MinAltitude = 0f, MaxAltitude = 1f, MinDistance = LocationConfigs.LocationRings.Ring2.MinDistance,
            InForest = false, BiomeArea = Heightmap.BiomeArea.Everything
        };
        
        GameObject portLocation = portsBundle.LoadAsset<GameObject>("MWL_PortLocation");
        GameObject jotunnLocationContainer = ZoneManager.Instance.CreateLocationContainer(portLocation);
        CustomLocation customLocation = new CustomLocation(jotunnLocationContainer, fixReference: true, locationConfig);
        ZoneManager.Instance.AddCustomLocation(customLocation);
        
        GameObject portLocation2 = portsLocationBundle.LoadAsset<GameObject>("MWL_DragonPort1");
        GameObject jotunnLocationContainer2 = ZoneManager.Instance.CreateLocationContainer(portLocation2);
        CustomLocation customLocation2 = new CustomLocation(jotunnLocationContainer2, fixReference: true, locationConfig3);
        ZoneManager.Instance.AddCustomLocation(customLocation2);
        
        GameObject portLocation3 = portsBundle.LoadAsset<GameObject>("MWL_PortStone1");
        GameObject jotunnLocationContainer3 = ZoneManager.Instance.CreateLocationContainer(portLocation3);
        CustomLocation customLocation3 = new CustomLocation(jotunnLocationContainer3, fixReference: true, locationConfig2);
        ZoneManager.Instance.AddCustomLocation(customLocation3);
        
        ZoneManager.OnVanillaLocationsAvailable -= PortPrefabs.AddPortLocation;
    }
}