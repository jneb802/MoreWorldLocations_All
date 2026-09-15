using System;
using Jotunn.Managers;
using SoftReferenceableAssets;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The game's own way of getting at a template and at a stock prefab.
///
/// <para>The same asset path an ordinary registration takes —
/// <c>AssetManager.GetSoftReference</c>, then <c>Load</c> — so what the audit
/// reads is what the game would place, mocks resolved and all. Notably it does
/// NOT go through <c>ZoneSystem.m_locationsByHash</c>: that table holds
/// registered locations, and needing to be registered before you can be
/// inspected is exactly the circularity this replaces.</para>
/// </summary>
public static class GameTemplateAssets
{
    /// <summary>Point the audit at the game. Called once, at startup.</summary>
    public static void Install()
    {
        TemplateAssets.Source = Open;
        TemplateAssets.StockPrefabs = StockPrefab;
    }

    private static ITemplateHandle? Open(string name)
    {
        SoftReference<GameObject> reference = AssetManager.Instance.GetSoftReference<GameObject>(name);
        if (!reference.IsValid)
            return null;

        // Ask for the mocks to be fixed the same way LocationManager.AddLocation
        // does. A template read before resolution is a template that never
        // exists: resolution swaps every mock for the real prefab and brings its
        // children, components and settings with it.
        AssetManager.Instance.ResolveMocksOnLoad(reference.m_assetID, null, null);
        return new SoftReferenceHandle(reference);
    }

    /// <summary>
    /// The stock prefab of a name, as the baseline an authored object is
    /// compared against.
    ///
    /// <para><c>ZNetScene</c> is what a ZDO's hash is resolved against, so it is
    /// the right list — and on this process it also holds the prefabs MWL
    /// registered. That does not matter for the comparison: a name that is not
    /// in the shipped stock snapshot has already been rejected on identity
    /// before any baseline is consulted, so the only names reaching here are
    /// ones the stock client has too.</para>
    ///
    /// <para><b>The limit, stated rather than hidden:</b> this is the SERVER's
    /// copy of the stock prefab. If some other mod mutates a vanilla prefab in
    /// place, the baseline moves with it and a difference from the real stock
    /// client goes unseen. Nothing available in-process is closer to the
    /// client's own copy.</para>
    /// </summary>
    private static GameObject? StockPrefab(string name)
    {
        if (ZNetScene.instance != null)
        {
            GameObject prefab = ZNetScene.instance.GetPrefab(name);
            if (prefab != null)
                return prefab;
        }
        return PrefabManager.Instance?.GetPrefab(name);
    }

    private sealed class SoftReferenceHandle : ITemplateHandle
    {
        private SoftReference<GameObject> _reference;
        private readonly bool _loadedHere;

        public SoftReferenceHandle(SoftReference<GameObject> reference)
        {
            _reference = reference;
            // Only release what this handle loaded. Releasing a template the
            // game had already loaded for its own reasons would pull it out from
            // under whatever asked for it.
            _loadedHere = _reference.Asset == null;
            if (_loadedHere)
                _reference.Load();
        }

        public GameObject? Asset => _reference.Asset;

        public void Dispose()
        {
            if (_loadedHere)
                _reference.Release();
        }
    }
}
