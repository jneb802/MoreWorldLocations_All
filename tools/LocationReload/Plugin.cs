using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using SoftReferenceableAssets;
using UnityEngine;

namespace MWL.LocationReload;

// This is a separate development plugin. Do not ship it in the public mod package.
[BepInPlugin("warpalicious.MWL.LocationReload", "MWL Location Reload", "0.1.0")]
[BepInDependency("com.jotunn.jotunn", "2.30.2")]
public sealed class Plugin : BaseUnityPlugin
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private bool registered;
    private bool busy;

    private void Update()
    {
        if (registered || UnityEngine.Object.FindAnyObjectByType<Terminal>() == null) return;
        registered = true;
        new Terminal.ConsoleCommand("mwl_reload", "Reload the bundle and replace the nearest loaded MWL test location. Usage: mwl_reload <name>", args =>
        {
            if (busy) { args.Context.AddString("A location reload is already running."); return; }
            try
            {
                if (args.Length != 2) throw new InvalidOperationException("Usage: mwl_reload MWL_Ruins1");
                if (Player.m_localPlayer == null || ZoneSystem.instance == null) throw new InvalidOperationException("Enter a development world first.");
                if (!ZNet.instance.IsServer() || ZNet.instance.GetPeers().Count != 0)
                    throw new InvalidOperationException("This prototype requires a local world with no connected peers.");
                StartCoroutine(RunGuarded(Replace(args[1], args.Context.AddString), args.Context.AddString));
            }
            catch (Exception ex) { args.Context.AddString("MWL reload refused: " + ex.Message); }
        }, isCheat: true);
    }

    private IEnumerator RunGuarded(IEnumerator routine, Action<string> write)
    {
        busy = true;
        try
        {
            while (true)
            {
                object step;
                try { if (!routine.MoveNext()) break; step = routine.Current; }
                catch (Exception ex)
                {
                    write("MWL reload failed: " + ex.Message);
                    if (ex is InvalidOperationException) Logger.LogInfo("Reload stopped: " + ex.Message);
                    else Logger.LogError(ex);
                    break;
                }
                yield return step;
            }
        }
        finally { (routine as IDisposable)?.Dispose(); busy = false; }
    }

    private IEnumerator Replace(string name, Action<string> write)
    {
        if (!name.StartsWith("MWL_", StringComparison.Ordinal)) throw new InvalidOperationException("Only MWL locations are supported.");
        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.jotunn.jotunn", out PluginInfo jotunn) || jotunn.Metadata.Version != new System.Version(2, 30, 2))
            throw new InvalidOperationException("This development command currently requires Jotunn 2.30.2.");
        ZoneSystem.ZoneLocation location = (ZoneSystem.ZoneLocation)typeof(ZoneSystem).GetMethod("GetLocation", Flags, null, new[] { typeof(string) }, null).Invoke(ZoneSystem.instance, new object[] { name });
        if (location == null || !location.m_prefab.IsLoaded) throw new InvalidOperationException("That location is not loaded. Spawn it first.");
        LocationProxy[] proxies = UnityEngine.Object.FindObjectsByType<LocationProxy>(FindObjectsSortMode.None)
            .Where(p => p.GetComponent<ZNetView>()?.GetZDO()?.GetInt("location".GetStableHashCode()) == name.GetStableHashCode()).ToArray();
        if (proxies.Length != 1) throw new InvalidOperationException($"Expected one loaded {name} instance; found {proxies.Length}. Remove other test instances first.");
        LocationProxy proxy = proxies[0];
        if (Vector3.Distance(proxy.transform.position, Player.m_localPlayer.transform.position) > 100f)
            throw new InvalidOperationException("Move within 100 metres of the test location.");
        GameObject prefab = location.m_prefab.Asset;
        if (prefab.GetComponentsInChildren<DungeonGenerator>(true).Length != 0)
            throw new InvalidOperationException("Generated dungeon interiors are not supported.");
        AssetState asset = new AssetState(location.m_prefab.m_assetID);
        asset.ValidateBundle();
        Vector3 position = proxy.transform.position;
        Quaternion rotation = proxy.transform.rotation;
        int seed = proxy.GetComponent<ZNetView>().GetZDO().GetInt("seed".GetStableHashCode());
        ZNetView[] templatePieces = prefab.GetComponentsInChildren<ZNetView>(true);
        ZNetView[] livePieces = UnityEngine.Object.FindObjectsByType<ZNetView>(FindObjectsSortMode.None)
            .Where(view => view.IsValid() && HeldReferences(view, location.m_prefab.m_assetID) > 0)
            .ToArray();
        if (livePieces.Length == 0 && templatePieces.Length > 0)
            throw new InvalidOperationException("Spawn a preview with 'location " + name + "' during this session first. Saved-world pieces have no reliable location ownership record.");
        foreach (ZNetView piece in livePieces)
        {
            if (Vector2.Distance(new Vector2(piece.transform.position.x, piece.transform.position.z), new Vector2(position.x, position.z)) > Mathf.Max(location.m_exteriorRadius, 10f) + 5f)
                throw new InvalidOperationException("A piece using this prefab is outside the preview area: " + piece.name);
            Piece buildPiece = piece.GetComponent<Piece>();
            if (buildPiece != null && buildPiece.GetCreator() != 0) throw new InvalidOperationException("A player-built piece overlaps the test location.");
        }
        // Reference ownership identifies the original spawned objects even after
        // vegetation snaps to terrain. Never infer ownership from proximity alone.
        HashSet<GameObject> removal = new HashSet<GameObject>(livePieces.Select(p => p.gameObject)) { proxy.gameObject };
        uint knownReferences = 1;
        IEnumerable<Component> holders = UnityEngine.Object.FindObjectsByType<ReferenceHolder>(FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<Component>()
            .Concat(UnityEngine.Object.FindObjectsByType<ZNetView>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        foreach (Component holder in holders)
        {
            int count = HeldReferences(holder, location.m_prefab.m_assetID);
            if (count == 0) continue;
            knownReferences += (uint)count;
            if (holder.GetComponentInParent<LocationProxy>() == proxy || removal.Contains(holder.gameObject)) continue;
            if (holder.gameObject == ZoneSystem.instance.gameObject) throw new InvalidOperationException("Disable Keep Loaded asset memory policy before using reload.");
            throw new InvalidOperationException("Another object still uses this location prefab: " + holder.name);
        }
        IList cache = (IList)Field(ZoneSystem.instance, "m_locationPrefabs");
        knownReferences += (uint)cache.Cast<object>().Count(entry => Equals(Property(entry, "PrefabAssetID"), location.m_prefab.m_assetID));
        if (asset.References != knownReferences) throw new InvalidOperationException($"Unknown asset references: actual={asset.References}, accountedFor={knownReferences}. Nothing was removed.");
        write($"MWL reload: replacing {name} at {position}, pieces={livePieces.Length}, oldPrefab={prefab.GetInstanceID()}.");
        foreach (ZNetView view in livePieces.Concat(new[] { proxy.GetComponent<ZNetView>() }))
        {
            view.ClaimOwnership();
            ZNetScene.instance.Destroy(view.gameObject);
        }
        // Destroy is deferred until the end of the frame. Let holders release first.
        yield return null;
        yield return null;
        for (int i = cache.Count - 1; i >= 0; --i)
        {
            object entry = cache[i];
            if (!Equals(Property(entry, "PrefabAssetID"), location.m_prefab.m_assetID)) continue;
            entry.GetType().GetMethod("Release", Flags).Invoke(entry, null);
            cache.RemoveAt(i);
        }
        // Jotunn 2.30.2 retains one PrepareLocation reference. Never force counters
        // to zero: unknown holders must stop the operation instead of losing assets.
        if (asset.References != 1) throw new InvalidOperationException($"Expected one Jotunn reference after removal; found {asset.References}. No bundle was force-unloaded.");
        location.m_prefab.Release();
        float deadline = Time.realtimeSinceStartup + 15f;
        while (!asset.Unloaded)
        {
            if (Time.realtimeSinceStartup > deadline) throw new InvalidOperationException("Bundle did not unload within 15 seconds.");
            yield return null;
        }
        location.m_prefab = new SoftReference<GameObject>(location.m_prefab.m_assetID);
        location.m_prefab.Load();
        try
        {
            if (location.m_prefab.Asset == null) throw new InvalidOperationException("Replacement prefab could not be loaded. Restore the bundle before retrying.");
            int newId = location.m_prefab.Asset.GetInstanceID();
            typeof(ZoneSystem).GetMethod("SpawnLocation", Flags).Invoke(ZoneSystem.instance,
                new object[] { location, seed, position, rotation, ZoneSystem.SpawnMode.Full, new List<GameObject>(), true });
            write($"MWL reload complete: {name}, newPrefab={newId}, position={position}, pieces={location.m_prefab.Asset.GetComponentsInChildren<ZNetView>(true).Length}.");
            Logger.LogInfo($"Reload complete: {name}; old prefab destroyed={!prefab}; newPrefab={newId}; position={position}; seed={seed}");
        }
        finally { location.m_prefab.Release(); }
    }

    private static object Field(object value, string name) => value.GetType().GetField(name, Flags).GetValue(value);
    private static object Property(object value, string name) => value.GetType().GetProperty(name, Flags).GetValue(value);
    private static int HeldReferences(Component holder, AssetID id)
    {
        IList references = Field(holder, "m_heldReferences") as IList;
        return references?.Cast<object>().Count(r => r is SoftReference<GameObject> reference && reference.m_assetID == id) ?? 0;
    }

    private sealed class AssetState
    {
        private readonly object loader;
        private readonly int assetIndex;
        private readonly int bundleIndex;
        private object Asset => ((Array)Field(loader, "m_assetLoaders")).GetValue(assetIndex);
        private object Bundle => ((Array)Field(loader, "m_bundleLoaders")).GetValue(bundleIndex);
        public uint References => (uint)Property(Asset, "ReferenceCount");
        public bool Unloaded => References == 0 && Property(Bundle, "LoadStatus").ToString() == "NotLoaded";
        public AssetState(AssetID id)
        {
            loader = typeof(Runtime).GetField("s_assetLoader", Flags).GetValue(null);
            assetIndex = (int)((IDictionary)Field(loader, "m_assetIDToLoaderIndex"))[id];
            bundleIndex = (int)Field(Asset, "m_bundleLoaderIndex");
        }
        public void ValidateBundle()
        {
            string path = (string)Field(Bundle, "m_bundlePath");
            if (!File.Exists(path) || new FileInfo(path).Length < 32) throw new InvalidOperationException("Replacement bundle is missing or empty.");
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] signature = new byte[8];
                if (stream.Read(signature, 0, signature.Length) != signature.Length || System.Text.Encoding.ASCII.GetString(signature) != "UnityFS\0")
                    throw new InvalidOperationException("Replacement file is not a UnityFS asset bundle. Nothing was removed.");
            }
            if ((uint)Property(Bundle, "ReferenceCount") != 1) throw new InvalidOperationException("The bundle is shared with other loaded assets.");
        }
    }
}
