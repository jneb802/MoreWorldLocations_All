using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using SoftReferenceableAssets;
using UnityEngine;

namespace More_World_Locations_AIO.Managers;

[PublicAPI]
public static class SoftAssetLoader
{
    private static readonly Dictionary<AssetID, AssetRef> m_assets = new();
    private static readonly Dictionary<AssetID, AssetRef> m_blueprintAssets = new();
    private static readonly Dictionary<string, AssetID> m_ids = new();

    internal static AssetBundleLoader? _assetBundleLoader;
	
    public static void Patch_AssetBundleLoader_OnInitCompleted(AssetBundleLoader __instance)
    {
	    if (!isReady())
	    {
		    Debug.LogWarning("[Location Manager] AssetBundle Loader is not ready");
            // should be ready since we are patching postfix to OnInitCompleted, but who knows ???
		    return;
	    }
        _assetBundleLoader = __instance;
	    foreach (KeyValuePair<AssetID, AssetRef> kvp in m_assets)
        {
            AddAssetToBundleLoader(__instance, kvp.Key, kvp.Value);
        }
    }

    public static void AddBlueprintSoftReferences()
    {
        // since blueprints are created during FejdStartup, we need to add their soft reference after they are created
        if (!isReady())
        {
            Debug.LogWarning("[Location Manager] AssetBundle Loader is not ready");
            return;
        }
        
        if (_assetBundleLoader == null)
        {
            Debug.LogWarning("[Location Manager] AssetBundle Loader is null");
            return;
        }
        
        foreach (KeyValuePair<AssetID, AssetRef> kvp in m_blueprintAssets)
        {
            AddAssetToBundleLoader(_assetBundleLoader, kvp.Key, kvp.Value);
        }
    }

    public static SoftReference<GameObject> GetSoftReference(AssetID assetID)
    {
        return assetID.IsValid ? new SoftReference<GameObject>(assetID) : default;
    }

    public static SoftReference<GameObject>? GetSoftReference(string name)
    {
        AssetID? assetID = GetAssetID(name);
        if (assetID == null) return null;
        return assetID.Value.IsValid ? new SoftReference<GameObject>(assetID.Value) : default;
    }
    
    private static void AddAssetToBundleLoader(AssetBundleLoader __instance, AssetID assetID, AssetRef assetRef)
    {
        string bundleName = $"RustyMods_{assetRef.asset.name}";
        string bundlePath = $"{assetRef.sourceMod.GUID}/Bundles/{bundleName}";
        string assetPath = $"{assetRef.sourceMod.GUID}/Prefabs/{assetRef.asset.name}";

        if (__instance.m_bundleNameToLoaderIndex.ContainsKey(bundleName))
        {
            Debug.LogWarning(bundleName + " already loaded");
	        return;
        }
        
        AssetLocation location = new AssetLocation(bundleName, assetPath);
        BundleLoader bundleLoader = new BundleLoader(bundleName, bundlePath);
        bundleLoader.HoldReference();
        __instance.m_bundleNameToLoaderIndex[bundleName] = __instance.m_bundleLoaders.Length;
        __instance.m_bundleLoaders = __instance.m_bundleLoaders.AddItem(bundleLoader).ToArray();

        int originalBundleLoaderIndex = __instance.m_assetLoaders
            .FirstOrDefault(l => l.m_assetID == assetID).m_bundleLoaderIndex;

        if (assetID.IsValid && originalBundleLoaderIndex > 0)
        {
            BundleLoader originalBundleLoader = __instance.m_bundleLoaders[originalBundleLoaderIndex];

            bundleLoader.m_bundleLoaderIndicesOfThisAndDependencies = originalBundleLoader.m_bundleLoaderIndicesOfThisAndDependencies
                .Where(i => i != originalBundleLoaderIndex)
                .AddItem(__instance.m_bundleNameToLoaderIndex[bundleName])
                .OrderBy(i => i)
                .ToArray();
        }
        else
        {
            bundleLoader.SetDependencies(Array.Empty<string>());
        }

        __instance.m_bundleLoaders[__instance.m_bundleNameToLoaderIndex[bundleName]] = bundleLoader;

        AssetLoader loader = new AssetLoader(assetID, location)
        {
            m_asset = assetRef.asset
        };
        loader.HoldReference();
        __instance.m_assetIDToLoaderIndex[assetID] = __instance.m_assetLoaders.Length;
        __instance.m_assetLoaders = __instance.m_assetLoaders.AddItem(loader).ToArray();

        m_ids[assetRef.asset.name] = assetID;
    }

    private static AssetID? GetAssetID(string name) => m_ids.TryGetValue(name, out AssetID id) ? id : null;
	
    public static AssetID AddAsset(UnityEngine.Object asset)
    {
        AssetID assetID = GenerateID(asset);
        AssetRef assetRef = new(PortInit.instance.Info.Metadata, asset, assetID);
        m_assets[assetID] = assetRef;
        return assetID;
    }

    public static AssetID AddBlueprintAsset(UnityEngine.Object asset)
    {
        AssetID assetID = GenerateID(asset);
        AssetRef assetRef = new(PortInit.instance.Info.Metadata, asset, assetID);
        m_blueprintAssets[assetID] = assetRef;
        return assetID;
    }

    private static AssetID GenerateID(UnityEngine.Object asset)
    {
        uint u = (uint)asset.name.GetStableHashCode();
        return new AssetID(u, u, u, u);
    }

    private static bool isReady()
    {
        return Runtime.s_assetLoader != null && ((AssetBundleLoader)Runtime.s_assetLoader).Initialized;
    }
}

public struct AssetRef
{
    public readonly BepInPlugin sourceMod;
    public readonly UnityEngine.Object asset;
    public AssetID originalID;

    public AssetRef(BepInPlugin sourceMod, UnityEngine.Object asset, AssetID assetID)
    {
        this.sourceMod = sourceMod;
        this.asset = asset;
        this.originalID = assetID;
    }
}