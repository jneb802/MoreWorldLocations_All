using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO.Managers;

[PublicAPI]
public class BlueprintLocation
{
    public LocationManager.CustomLocation? Location;
    public GameObject Prefab;
    internal bool Loaded;
    public event Action<BlueprintLocation>? OnCreated;
    
    public BlueprintLocation(string assetBundleName, string prefabName) : this(AssetBundleManager.GetAssetBundle(assetBundleName), prefabName){}
    
    public BlueprintLocation(AssetBundle bundle, string prefabName) : this(bundle.LoadAsset<GameObject>(prefabName)){}

    public BlueprintLocation(GameObject prefab)
    {
        Prefab = Object.Instantiate(prefab, PortInit.root.transform, false);
        Prefab.name = prefab.name;
        PrefabManager.BlueprintLocations[prefab.name] = this;
    }

    public void Create()
    {
        if (Loaded) return;
        List<GameObject> objectsToDestroy = new();
        List<Blueprint.BlueprintObject> objectsToAdd = new List<Blueprint.BlueprintObject>();
        foreach (Transform child in Prefab.transform)
        {
            if (!child.name.StartsWith("MOCK_")) continue;
            if (Helpers.GetPrefab(child.name.Replace("MOCK_", string.Empty)) is not { } original)
            {
                Debug.LogError($"Prefab {child.name} not found");
            }
            else
            {
                objectsToAdd.Add(new Blueprint.BlueprintObject(original, child));
                objectsToDestroy.Add(child.gameObject);
            }
        }

        foreach (Blueprint.BlueprintObject? obj in objectsToAdd)
        {
            GameObject clone = Object.Instantiate(obj.Original, Prefab.transform);
            clone.name = obj.Mock.name.Replace("MOCK_", string.Empty);
            clone.layer = obj.Mock.gameObject.layer;
            clone.transform.SetLocalPositionAndRotation(obj.Mock.localPosition, obj.Mock.localRotation);
        }

        foreach (GameObject? obj in objectsToDestroy)
        { 
            Object.DestroyImmediate(obj);
        }
        
        Location = new LocationManager.CustomLocation(Prefab, true);
        OnCreated?.Invoke(this);
        Loaded = true;
    }
}