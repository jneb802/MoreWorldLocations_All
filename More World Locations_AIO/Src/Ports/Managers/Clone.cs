using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO.Managers;

public class Clone
{
    internal static readonly Dictionary<string, GameObject> registeredPrefabs = new();
    private GameObject? Prefab;
    private readonly string PrefabName;
    private readonly string NewName;
    private bool Loaded;
    public event Action<GameObject>? OnCreated;

    public Clone(string prefabName, string newName)
    {
        PrefabName = prefabName;
        NewName = newName;
        PrefabManager.Clones.Add(this);
    }

    internal void Create()
    {
        if (Loaded) return;
        // find prefab, instantiate it into our root object
        // change the name, and register to scene
        // so ZNetScene has reference to something tangible
        // ZNetScene is cloning a clone ----> we made a monster!
        if (Helpers.GetPrefab(PrefabName) is not { } prefab) return;
        Prefab = Object.Instantiate(prefab, PortInit.root.transform, false);
        Prefab.name = NewName;
        PrefabManager.RegisterPrefab(Prefab);
        OnCreated?.Invoke(Prefab);
        registeredPrefabs[Prefab.name] = Prefab;
        Loaded = true;
    }
}