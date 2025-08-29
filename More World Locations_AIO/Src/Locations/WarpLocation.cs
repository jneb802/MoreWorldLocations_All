using Jotunn.Configs;
using Jotunn.Extensions;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Entities;
using Jotunn.Managers;
using More_World_Locations_AIO.Utils;
using SoftReferenceableAssets;
using UnityEngine;

namespace More_World_Locations_AIO;

public class WarpLocation
{
    public string m_name;
    public int m_spawnQuantity;
    public LocationConfig m_locationConfig;

    public void CreateBepinexConfiguration(ConfigFile configFile)
    {
        ConfigFileExtensions.BindConfigInOrder(configFile, m_name, "Spawn Quantity", m_spawnQuantity, 
            "Amount of this location the game will attempt to place during world generation");
    }

    public void SetQuantity(int quantity)
    {
        this.m_spawnQuantity = quantity;
    }
    
    public WarpLocation() { }

    public WarpLocation(string name, int spawnQuantity)
    {
        m_name = name;
        m_spawnQuantity = spawnQuantity;
    }
    
    public void AddLocation()
    {
        SoftReference<GameObject> softReferencePrefab = Jotunn.Managers.AssetManager.Instance.GetSoftReference<GameObject>(m_name);
        
        Jotunn.Managers.AssetManager.Instance.ResolveMocksOnLoad(
            softReferencePrefab.m_assetID,
            null,
            resolvedObj =>
            {
                CreatureDB.SetupCreatures(
                    LocationCreatureMapping.GetCreatureListForLocation(m_name),
                    resolvedObj as GameObject);
            });
        
        Jotunn.Managers.AssetManager.Instance.ResolveMocksOnLoad(
            softReferencePrefab.m_assetID,
            null,
            resolvedObj => 
            {
                LootDB.SetupLoot(
                    m_locationConfig.Biome,
                    resolvedObj as GameObject);
            });
    
        CustomLocation customLocation = new 
            CustomLocation(
                softReferencePrefab,
                true,
                m_locationConfig);
        
        ZoneManager.Instance.AddCustomLocation(customLocation);
    }
}