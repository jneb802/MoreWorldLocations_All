using System;
using System.Collections.Generic;
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
        TemplateAssets.BaselineProvenance = Provenance();
    }

    /// <summary>
    /// What is known against the baseline: which other plugins are loaded in
    /// this process.
    ///
    /// <para>The comparison reads the running server's registry, and this
    /// process has MWL in it. That much is accounted for — MWL clones prefabs
    /// rather than editing vanilla ones in place, so the entries it adds are
    /// under names of their own. Another plugin editing a vanilla prefab in
    /// place is a different matter: the baseline would move with it and a
    /// comparison against a moved baseline proves nothing.</para>
    ///
    /// <para>There is no pristine copy available in-process to compare against
    /// instead, so this is stated rather than solved: the verdict carries the
    /// names of everything else that is loaded, and a run on a server with other
    /// mods can be read for what it is.</para>
    /// </summary>
    /// <summary>Jötunn's plugin id. It adds prefabs of its own and edits no vanilla one.</summary>
    private const string JotunnGuid = "com.jotunn.jotunn";

    private static string Provenance()
    {
        var others = new List<string>();
        try
        {
            foreach (KeyValuePair<string, BepInEx.PluginInfo> plugin in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                // By GUID, exactly. A substring match on "moreworldlocations"
                // never fired against "warpalicious.More_World_Locations_AIO",
                // so a station run reported the mod itself as a foreign plugin
                // that might have moved the baseline.
                string id = plugin.Key ?? "";
                if (id.Length == 0
                    || string.Equals(id, More_World_Locations_AIOPlugin.ModGUID, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(id, JotunnGuid, StringComparison.OrdinalIgnoreCase))
                    continue;
                others.Add(plugin.Value?.Metadata?.Name ?? id);
            }
        }
        catch (Exception ex)
        {
            return $"the loaded plugin list could not be read ({ex.GetType().Name}), so nothing is known about " +
                   "whether another mod has edited a vanilla prefab in place.";
        }

        if (others.Count == 0)
            return "";

        others.Sort(StringComparer.Ordinal);
        return $"{others.Count} other plugin(s) are loaded — {string.Join(", ", others.ToArray())} — and any of " +
               "them editing a vanilla prefab in place would move the baseline with it.";
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
        return TemplateLease.Take(new SoftReferenceOwnership(reference));
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

    /// <summary>
    /// The game's own ownership of one soft-referenced asset.
    ///
    /// The reference count comes from the loader's tables, which the publicized
    /// assemblies make readable. It is the only thing in this process that can
    /// say whether an acquisition actually happened, which is what the lease
    /// needs when a load throws.
    /// </summary>
    private sealed class SoftReferenceOwnership : ITemplateOwnership
    {
        private SoftReference<GameObject> _reference;

        public SoftReferenceOwnership(SoftReference<GameObject> reference) => _reference = reference;

        public GameObject? Asset => _reference.Asset;

        public bool TryReferenceCount(out uint count)
        {
            count = 0;
            try
            {
                AssetBundleLoader loader = AssetBundleLoader.Instance;
                if (loader == null || loader.m_assetIDToLoaderIndex == null)
                    return false;
                if (!loader.m_assetIDToLoaderIndex.TryGetValue(_reference.m_assetID, out int index))
                    return false;
                count = loader.m_assetLoaders[index].ReferenceCount;
                return true;
            }
            catch
            {
                // The count is a diagnostic, not a decision: failing to read it
                // must not fail the load. The lease treats it as "cannot see".
                return false;
            }
        }

        public void Load() => _reference.Load();

        public void Release() => _reference.Release();
    }

}
