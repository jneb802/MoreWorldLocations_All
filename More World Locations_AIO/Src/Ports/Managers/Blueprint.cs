using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO.Managers;

[PublicAPI]
public class Blueprint
{
    internal static Dictionary<string, GameObject> registeredPrefabs = new();
    public GameObject Prefab;
    public event Action<Blueprint>? OnCreated;
    internal bool Loaded;
    
    public Blueprint(string assetBundleName, string prefabName):this(AssetBundleManager.GetAssetBundle(assetBundleName), prefabName){}
    
    public Blueprint(AssetBundle bundle, string prefabName):this(bundle.LoadAsset<GameObject>(prefabName)){}

    public Blueprint(GameObject prefab)
    {
        Prefab = Object.Instantiate(prefab, PortInit.root.transform, false);
        Prefab.name = prefab.name; // so our prefab isn't called MWL_Port_Location(Clone), even though it is! haha
        PrefabManager.Blueprints[Prefab.name] = this;
    }

    internal void Create()
    {
        if (Loaded) return;
        List<GameObject> objectsToDestroy = new();
        List<BlueprintObject> objectsToAdd = new List<BlueprintObject>();
        // first we get all the children we want to replace
        // this is a surface search, first layer, not digging into the children of children
        foreach (Transform child in Prefab.transform)
        {
            if (!child.name.StartsWith("MOCK_")) continue;
            if (Helpers.GetPrefab(child.name.Replace("MOCK_", string.Empty)) is not { } original)
            {
                Debug.LogWarning($"Prefab {child.name} not found, when creating blueprint: {Prefab.name}");
            }
            else
            {
                objectsToAdd.Add(new BlueprintObject(original, child));
                objectsToDestroy.Add(child.gameObject);
            }
        }
        // secondly we add the replacements
        foreach (BlueprintObject? obj in objectsToAdd)
        {
            GameObject clone = Object.Instantiate(obj.Original, Prefab.transform);
            clone.name = obj.Mock.name.Replace("MOCK_", string.Empty);
            clone.layer = obj.Mock.gameObject.layer;
            clone.transform.SetLocalPositionAndRotation(obj.Mock.localPosition, obj.Mock.localRotation);
        }
        // finally we remove the mocks
        foreach (GameObject? obj in objectsToDestroy)
        { 
            Object.DestroyImmediate(obj);
        }
        // invoke the on created event so we can uniquely modify prefab after its created
        OnCreated?.Invoke(this);
        // register to a dictionary to use as reference
        registeredPrefabs[Prefab.name] = Prefab;
        // make sure it does not run twice during same load
        // that is, if a player logs in, logs out, logs in
        Loaded = true;
    }
    
    public class BlueprintObject
    {
        public readonly GameObject Original;
        public readonly Transform Mock;
        public BlueprintObject(GameObject original, Transform mock)
        {
            Original = original;
            Mock = mock;
        }
    }
}