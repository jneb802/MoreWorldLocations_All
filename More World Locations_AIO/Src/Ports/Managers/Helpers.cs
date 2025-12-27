using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO.Managers;

[PublicAPI]
public static class Helpers
{
    internal static ZNetScene? _ZNetScene;
    internal static ObjectDB? _ObjectDB;

    internal static GameObject? GetPrefab(string prefabName)
    {
        // first we check if znetscene is active, if so, then all prefabs should be found there
        if (ZNetScene.instance != null) return ZNetScene.instance.GetPrefab(prefabName);
        // then we check fejd startup znetscene, if null, then this is being called too early
        if (_ZNetScene == null) return null;
        // we can't use znetscene GetPrefab since that looks through the hash dictionary
        // so we need to search using Find()
        GameObject? result = _ZNetScene.m_prefabs.Find(prefab => prefab.name == prefabName);
        // if we find it, return it
        if (result != null) return result;
        // check created blueprints that have yet to be registered to znetscene
        if (Blueprint.registeredPrefabs.TryGetValue(prefabName, out GameObject blueprint)) return blueprint;
        // check clones that have yet to be registered to znetscene
        return Clone.registeredPrefabs.TryGetValue(prefabName, out GameObject clone) ? clone : result;
    }
    
    public static string GetNormalizedName(string name) => Regex.Replace(name, @"\s*\(.*?\)", "").Trim();

    public static bool HasComponent<T>(this GameObject go) where T : Component => go.GetComponent<T>();

    public static void RemoveComponent<T>(this GameObject go) where T : Component
    {
        if (!go.TryGetComponent(out T component)) return;
        // use destroy immediate since Destroy() gets queued for the next frame, which might be too late
        Object.DestroyImmediate(component);
    }

    public static void RemoveAllComponents<T>(this GameObject go, bool includeChildren = false, params Type[] ignoreComponents) where T : MonoBehaviour
    {
        List<T> components = go.GetComponents<T>().ToList();
        if (includeChildren)
        {
            components.AddRange(go.GetComponentsInChildren<T>(true));
        }
        foreach (T component in components)
        {
            if (ignoreComponents.Contains(component.GetType())) continue;
            Object.DestroyImmediate(component);
        }
    }

    public static List<Transform> FindAll(this Transform parent, string name)
    {
        List<Transform> result = new List<Transform>();
        foreach (Transform transform in parent)
        {
            if (transform.name == name) result.Add(transform);
        }

        return result;
    }
    
    public static List<Transform> FindAllRecursive(this Transform parent, string name)
    {
        List<Transform> result = new List<Transform>();
        if (parent == null) return result;
    
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                result.Add(child);
        }
        return result;
    }
    
    internal static string GetInternalName(this LocationManager.IconSettings.LocationIcon table)
    {
        Type type = typeof(LocationManager.IconSettings.LocationIcon);
        MemberInfo[] memInfo = type.GetMember(table.ToString());
        if (memInfo.Length <= 0) return table.ToString();
        LocationManager.IconSettings.InternalName? attr = (LocationManager.IconSettings.InternalName)Attribute.GetCustomAttribute(memInfo[0], typeof(LocationManager.IconSettings.InternalName));
        return attr != null ? attr.internalName : table.ToString();
    }
}