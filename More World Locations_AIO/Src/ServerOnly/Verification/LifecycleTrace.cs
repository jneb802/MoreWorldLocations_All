using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Jotunn.Managers;
using SoftReferenceableAssets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// What happens to a template, and to the prefabs resolved into it, across
/// load, release, deferred cleanup and reload: identities, the loader's own
/// reference counts, bundle states and component-by-component liveness, at
/// named moments.
///
/// <para>Diagnostic only. It is installed by a validation switch and reached by
/// one console command; a shipped run has neither. Nothing here decides
/// anything, and every read is guarded so that a trace can fail without the
/// sweep noticing.</para>
///
/// <para><b>Why identities and not counts.</b> The regression it exists to
/// explain is five templates whose second read finds a component the first
/// read did not, and a count of anything cannot say which object changed. So
/// every object of interest is named by instance ID, and CLR null, Unity
/// destroyed and alive are three different words.</para>
/// </summary>
public static class LifecycleTrace
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    private const string Tag = "[LIFECYCLE]";

    private static HashSet<string> s_templates = new HashSet<string>(StringComparer.Ordinal);
    private static List<string> s_probes = new List<string>();

    /// <summary>Install the sweep observers if the switch asks for them. Called once, at startup.</summary>
    public static void Install()
    {
        if (!ValidationSwitches.TraceRequested(out IReadOnlyCollection<string> templates, out IReadOnlyCollection<string> probes))
            return;

        s_templates = new HashSet<string>(templates, StringComparer.Ordinal);
        s_probes = new List<string>(probes);

        CatalogueSweep.TemplateRead = (name, asset) =>
        {
            if (s_templates.Contains(name))
                Snapshot($"sweep holds {name}", name, asset);
        };
        CatalogueSweep.TemplateReleased = name =>
        {
            if (s_templates.Contains(name))
                Snapshot($"sweep released {name}, same frame", name, null);
        };
        CatalogueSweep.SweepFinished = () => Schedule(AfterSweep());
        CatalogueSweep.SweepStarted = () => Snapshot("sweep starts: nothing opened yet", "", null);
        InstallMemberWatch();

        Log.LogWarning(
            $"{ValidationSwitches.TraceVariable} is set: tracing {s_templates.Count} template(s) " +
            $"[{string.Join(", ", new List<string>(s_templates).ToArray())}] and probing {s_probes.Count} prefab(s) " +
            $"[{string.Join(", ", s_probes.ToArray())}]. Diagnostic output only.");
    }

    /// <summary>
    /// Watch Jötunn's reference fixing set members, and say so whenever it sets
    /// a Transform's parent or hands an object a persistent prefab.
    ///
    /// <para>Jötunn's walk is a reflective pass over every field and property
    /// of every component it visits, base types included, five levels deep. A
    /// <c>Transform.parent</c> is a settable Unity-object property, so a mock
    /// ancestor resolved through it becomes a reparenting — into whatever the
    /// name resolved to. Whether that is what happened here is exactly what this
    /// watch is for; it changes nothing.</para>
    /// </summary>
    private static void InstallMemberWatch()
    {
        try
        {
            Type? mockManager = AccessTools.TypeByName("Jotunn.Managers.MockManager");
            System.Reflection.MethodInfo? target = mockManager == null ? null : AccessTools.Method(mockManager, "FixMemberReferences");
            if (target == null)
            {
                Log.LogWarning($"{Tag} Jötunn's FixMemberReferences was not found; member sets will not be watched.");
                return;
            }
            var harmony = new Harmony("warpalicious.More_World_Locations_AIO.lifecycle");
            harmony.Patch(target,
                prefix: new HarmonyMethod(typeof(LifecycleTrace), nameof(MemberWatchPrefix)),
                postfix: new HarmonyMethod(typeof(LifecycleTrace), nameof(MemberWatchPostfix)));
            Log.LogInfo($"{Tag} watching Jötunn member sets");
        }
        catch (Exception ex)
        {
            Log.LogWarning($"{Tag} could not watch Jötunn member sets: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>Member sets seen since the last snapshot, by member name.</summary>
    private static readonly Dictionary<string, int> s_memberSets = new Dictionary<string, int>(StringComparer.Ordinal);

    private static string MemberName(object member)
    {
        try
        {
            Traverse t = Traverse.Create(member);
            object? info = t.Field("propertyInfo").GetValue() ?? t.Field("fieldInfo").GetValue();
            return (info as System.Reflection.MemberInfo)?.Name ?? member.GetType().Name;
        }
        catch
        {
            return "?";
        }
    }

    private static object? MemberValue(object member, object target)
    {
        try
        {
            System.Reflection.MethodInfo? get = member.GetType().GetMethod("GetValue");
            return get?.Invoke(member, new[] { target });
        }
        catch
        {
            return null;
        }
    }

    public static void MemberWatchPrefix(object __0, object __1, out object? __state)
    {
        __state = null;
        try
        {
            Traverse t = Traverse.Create(__0);
            if (!t.Property("HasSetMethod").GetValue<bool>() || !t.Property("IsUnityObject").GetValue<bool>())
                return;
            __state = MemberValue(__0, __1) ?? (object)"(null)";
        }
        catch
        {
            __state = null;
        }
    }

    public static void MemberWatchPostfix(object __0, object __1, object? __state)
    {
        if (__state == null)
            return;
        try
        {
            object? after = MemberValue(__0, __1);
            object? before = __state is string ? null : __state;
            if (ReferenceEquals(before, after))
                return;
            string name = MemberName(__0);
            s_memberSets[name] = s_memberSets.TryGetValue(name, out int n) ? n + 1 : 1;

            var afterObject = after as Object;
            bool persistentTarget = afterObject != null && Persistent(afterObject) == "True";
            if (name != "parent" && !persistentTarget)
                return;

            string owner = __1 switch
            {
                Component c when c != null => PathOf(c.transform) + " (" + c.GetType().Name + ")",
                GameObject g when g != null => PathOf(g.transform),
                _ => __1?.GetType().Name ?? "?",
            };
            string where = after switch
            {
                Transform tr when tr != null => PathOf(tr),
                GameObject go when go != null => PathOf(go.transform),
                _ => Identity(afterObject),
            };
            Log.LogWarning(
                $"{Tag} member set: {owner}.{name} = {Identity(afterObject)} at {where} persistent {Persistent(afterObject)} " +
                $"(was {Identity(before as Object)})");
        }
        catch (Exception ex)
        {
            Log.LogInfo($"{Tag} member watch threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static string MemberSetSummary()
    {
        if (s_memberSets.Count == 0)
            return "member sets since last snapshot: none";
        var parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in s_memberSets)
            parts.Add($"{pair.Key} {pair.Value}");
        s_memberSets.Clear();
        return "member sets since last snapshot: " + string.Join(", ", parts.ToArray());
    }

    /// <summary>
    /// The console entry: run the load/release/reload protocol over named
    /// templates on the main thread, a few frames at a time.
    ///
    /// Tokens: a template name; <c>hold:NAME</c> keeps one lease on NAME across
    /// the whole run and reads it at every boundary; <c>probe:NAME</c> adds a
    /// prefab to probe in every snapshot.
    /// </summary>
    public static string Run(IReadOnlyList<string> tokens)
    {
        var names = new List<string>();
        string? hold = null;
        foreach (string token in tokens)
        {
            if (token.StartsWith("hold:", StringComparison.OrdinalIgnoreCase))
                hold = token.Substring(5);
            else if (token.StartsWith("wait:", StringComparison.OrdinalIgnoreCase))
            {
                if (float.TryParse(token.Substring(5), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float seconds) && seconds >= 0f)
                    s_longWait = seconds;
            }
            else if (token.StartsWith("probe:", StringComparison.OrdinalIgnoreCase))
            {
                if (!s_probes.Contains(token.Substring(6)))
                    s_probes.Add(token.Substring(6));
            }
            else
                names.Add(token);
        }
        if (names.Count == 0 && hold == null)
            return "mwl_lifecycle <template>... [hold:<template>] [probe:<prefab>] [wait:<seconds>]";

        if (!Schedule(Protocol(names, hold)))
            return "no MonoBehaviour to host the trace on; is a world loaded?";
        return $"{Tag} protocol started over {names.Count} template(s)" + (hold == null ? "" : $", holding {hold}") +
               $"; probing [{string.Join(", ", s_probes.ToArray())}]. Output goes to the BepInEx log.";
    }

    private static bool Schedule(IEnumerator routine)
    {
        MonoBehaviour? host = ZoneSystem.instance != null ? ZoneSystem.instance : (MonoBehaviour?)ZNet.instance;
        if (host == null)
        {
            Log.LogWarning($"{Tag} nothing to host a frame-stepped trace on; skipped.");
            return false;
        }
        host.StartCoroutine(routine);
        return true;
    }

    private static IEnumerator AfterSweep()
    {
        // Frames AND wall time: a dedicated server at startup ran 54,748 frames
        // in 24 s, so a frame count alone says nothing about whether an async
        // bundle unload has had time to finish.
        foreach ((int frames, float seconds) in new[] { (1, 0f), (10, 0.2f), (120, 2f), (600, 10f) })
        {
            yield return Wait(frames, seconds);
            foreach (string name in s_templates)
                Snapshot($"after sweep +{frames} frame(s)/{seconds:0.0}s, {name}", name, null);
        }
    }

    /// <summary>How long the long waits in the protocol are, in seconds. 'wait:N' overrides.</summary>
    private static float s_longWait = 3f;

    private static IEnumerator Wait(int frames, float seconds)
    {
        int untilFrame = Time.frameCount + frames;
        float untilTime = Time.realtimeSinceStartup + seconds;
        while (Time.frameCount < untilFrame || Time.realtimeSinceStartup < untilTime)
            yield return null;
    }

    private static IEnumerator Protocol(List<string> names, string? hold)
    {
        ITemplateHandle? held = null;
        if (hold != null)
        {
            held = TemplateAssets.Open(hold);
            Snapshot($"hold control opened {hold}", hold, held?.Asset);
        }

        foreach (string name in names)
        {
            Snapshot($"P0 {name} before anything", name, null);
            ResolverProbe($"P0 {name}");

            ITemplateHandle? first = TemplateAssets.Open(name);
            Snapshot($"P1 {name} opened", name, first?.Asset);
            first?.Dispose();
            Snapshot($"P2 {name} released, same frame", name, null);

            yield return Wait(1, 0f);
            Snapshot($"P3 {name} +1 frame after release", name, null);
            yield return Wait(9, 0.2f);
            Snapshot($"P4 {name} +10 frames/0.2s after release", name, null);
            yield return Wait(110, s_longWait);
            Snapshot($"P5 {name} +120 frames/{s_longWait:0.0}s after release", name, null);

            ITemplateHandle? second = TemplateAssets.Open(name);
            Snapshot($"P6 {name} reopened", name, second?.Asset);
            second?.Dispose();
            yield return Wait(120, s_longWait);
            Snapshot($"P7 {name} +120 frames/{s_longWait:0.0}s after second release", name, null);

            ITemplateHandle? third = TemplateAssets.Open(name);
            Snapshot($"P8 {name} third open", name, third?.Asset);
            third?.Dispose();
            yield return Wait(120, s_longWait);
            Snapshot($"P9 {name} end (+120 frames/{s_longWait:0.0}s)", name, null);

            if (held != null)
                Snapshot($"hold control {hold} after {name}", hold!, held.Asset);
        }

        if (held != null)
        {
            held.Dispose();
            Snapshot($"hold control {hold} released, same frame", hold!, null);
            yield return Wait(120, s_longWait);
            Snapshot($"hold control {hold} +120 frames/{s_longWait:0.0}s after release", hold!, null);
            ITemplateHandle? again = TemplateAssets.Open(hold!);
            Snapshot($"hold control {hold} reopened", hold!, again?.Asset);
            again?.Dispose();
            yield return Wait(120, s_longWait);
            Snapshot($"hold control {hold} end", hold!, null);
        }

        Log.LogInfo($"{Tag} protocol complete");
    }

    /// <summary>
    /// What Jötunn's resolver would hand a mock right now, and what asking
    /// costs: the loader count before and after, because the resolver loads and
    /// does not release.
    /// </summary>
    private static void ResolverProbe(string tag)
    {
        foreach (string probe in s_probes)
        {
            try
            {
                uint before = ReferenceCountOf(probe, out bool readable);
                GameObject? resolved = PrefabManager.Cache.GetPrefab<GameObject>(probe);
                uint after = ReferenceCountOf(probe, out bool readableAfter);
                Log.LogInfo(
                    $"{Tag} {tag} | resolver {probe}: {Identity(resolved)} refs {(readable ? before.ToString() : "?")} -> " +
                    $"{(readableAfter ? after.ToString() : "?")} | {Summary(resolved)}");
            }
            catch (Exception ex)
            {
                Log.LogInfo($"{Tag} {tag} | resolver {probe}: threw {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static uint ReferenceCountOf(string name, out bool readable)
    {
        readable = false;
        try
        {
            SoftReference<GameObject> reference = AssetManager.Instance.GetSoftReference<GameObject>(name);
            if (!reference.IsValid)
                return 0;
            AssetBundleLoader loader = AssetBundleLoader.Instance;
            if (loader == null || !loader.m_assetIDToLoaderIndex.TryGetValue(reference.m_assetID, out int index))
                return 0;
            readable = true;
            return loader.m_assetLoaders[index].ReferenceCount;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>One moment, written out in full.</summary>
    public static void Snapshot(string tag, string templateName, GameObject? held)
    {
        var lines = new List<string>
        {
            $"=== {tag} | frame {Time.frameCount} | t {Time.realtimeSinceStartup:0.00}s | leases {TemplateAssets.OutstandingLeases} (peak {TemplateAssets.PeakLeases})"
        };
        try
        {
            lines.Add(BundleHistogram());
            lines.Add(MemberSetSummary());
            if (templateName.Length > 0)
            {
                lines.AddRange(ProbeAsset("template", templateName, fullCensus: false));
                lines.Add(ResolutionContext(templateName));
            }
            if (held != null)
                lines.AddRange(Census("held clone", held, full: true));
            foreach (string probe in s_probes)
            {
                lines.AddRange(ProbeAsset("probe", probe, fullCensus: true));
                lines.Add(ZNetScenePrefab(probe));
                lines.Add(CacheEntry(probe));
                lines.AddRange(CopyTest(probe));
            }
        }
        catch (Exception ex)
        {
            lines.Add($"snapshot threw {ex.GetType().Name}: {ex.Message}");
        }
        foreach (string line in lines)
            Log.LogInfo($"{Tag} {line}");
    }

    /// <summary>How many of the mod's own bundles are in each loader state right now.</summary>
    private static string BundleHistogram()
    {
        try
        {
            AssetBundleLoader loader = AssetBundleLoader.Instance;
            if (loader == null || loader.m_bundleLoaders == null)
                return "bundles: loader unavailable";
            var counts = new Dictionary<string, int>();
            int mwl = 0;
            for (int i = 0; i < loader.m_bundleLoaders.Length; i++)
            {
                BundleLoader bundle = loader.m_bundleLoaders[i];
                string bundleName = Traverse.Create(bundle).Field("m_bundleName").GetValue<string>() ?? "";
                if (!bundleName.StartsWith("mwl_", StringComparison.OrdinalIgnoreCase))
                    continue;
                mwl++;
                string key = bundle.LoadStatus.ToString();
                counts[key] = counts.TryGetValue(key, out int n) ? n + 1 : 1;
            }
            var parts = new List<string>();
            foreach (KeyValuePair<string, int> pair in counts)
                parts.Add($"{pair.Key} {pair.Value}");
            return $"bundles: {mwl} mwl_* bundle(s): {string.Join(", ", parts.ToArray())}";
        }
        catch (Exception ex)
        {
            return $"bundles: threw {ex.GetType().Name}: {ex.Message}";
        }
    }

    private static GameObject? s_copyHolder;

    /// <summary>
    /// Instantiate the probe's source right now, under an inactive holder, and
    /// read the copy: the question is not whether the source LOOKS intact but
    /// whether a copy made from it now is, which is what every later mock
    /// resolution and every placement actually does.
    /// </summary>
    private static IEnumerable<string> CopyTest(string name)
    {
        var lines = new List<string>();
        try
        {
            SoftReference<GameObject> reference = AssetManager.Instance.GetSoftReference<GameObject>(name);
            GameObject? source = null;
            if (reference.IsValid)
            {
                AssetBundleLoader loader = AssetBundleLoader.Instance;
                if (loader != null && loader.m_assetIDToLoaderIndex.TryGetValue(reference.m_assetID, out int index))
                    source = loader.m_assetLoaders[index].Asset as GameObject;
            }
            if (source == null && ZNetScene.instance != null)
                source = ZNetScene.instance.GetPrefab(name);
            if (source == null)
            {
                lines.Add($"copy test {name}: no source to copy");
                return lines;
            }
            if (s_copyHolder == null)
            {
                s_copyHolder = new GameObject("MWL lifecycle copy holder");
                s_copyHolder.SetActive(false);
                Object.DontDestroyOnLoad(s_copyHolder);
            }
            GameObject copy = Object.Instantiate(source, s_copyHolder.transform);
            try
            {
                lines.AddRange(Census($"copy test {name} (fresh copy of {Identity(source)})", copy, full: true));
                lines.AddRange(Tree("    copy tree", copy, 24));
            }
            finally
            {
                Object.DestroyImmediate(copy);
            }
        }
        catch (Exception ex)
        {
            lines.Add($"copy test {name}: threw {ex.GetType().Name}: {ex.Message}");
        }
        return lines;
    }

    /// <summary>Every object under a root by path and instance ID, for a root small enough to print.</summary>
    private static IEnumerable<string> Tree(string label, GameObject root, int cap)
    {
        var lines = new List<string>();
        int printed = 0, total = 0;
        Walk(root.transform, root.name);
        if (total > printed)
            lines.Add($"{label}: ... {total - printed} more object(s)");
        return lines;

        void Walk(Transform node, string path)
        {
            total++;
            if (printed < cap)
            {
                printed++;
                lines.Add($"{label}: {path} [{node.gameObject.GetInstanceID()}] persistent {Persistent(node.gameObject)}");
            }
            for (int i = 0; i < node.childCount; i++)
            {
                Transform child = node.GetChild(i);
                Walk(child, path + "/" + child.name);
            }
        }
    }

    private static IEnumerable<string> ProbeAsset(string kind, string name, bool fullCensus)
    {
        var lines = new List<string>();
        SoftReference<GameObject> reference = AssetManager.Instance.GetSoftReference<GameObject>(name);
        if (!reference.IsValid)
        {
            lines.Add($"{kind} {name}: no soft reference");
            return lines;
        }
        AssetBundleLoader loader = AssetBundleLoader.Instance;
        if (loader == null || !loader.m_assetIDToLoaderIndex.TryGetValue(reference.m_assetID, out int index))
        {
            lines.Add($"{kind} {name}: {reference.m_assetID} is not in the loader's table");
            return lines;
        }

        AssetLoader asset = loader.m_assetLoaders[index];
        Object? loaded = asset.Asset;
        lines.Add(
            $"{kind} {name}: id {reference.m_assetID} loader#{index} refs {asset.ReferenceCount} status {asset.LoadStatus} " +
            $"m_asset {Identity(loaded)} persistent {Persistent(loaded)} path {asset.m_assetPathInBundle}");

        int own = Traverse.Create(asset).Field("m_bundleLoaderIndex").GetValue<int>();
        int[] indices = loader.m_bundleLoaders[own].BundleLoaderIndices ?? new[] { own };
        foreach (int i in indices)
        {
            BundleLoader bundle = loader.m_bundleLoaders[i];
            Traverse t = Traverse.Create(bundle);
            string bundleName = t.Field("m_bundleName").GetValue<string>() ?? "?";
            AssetBundle? handle = t.Field("m_bundle").GetValue<AssetBundle>();
            lines.Add($"  bundle#{i} {bundleName}{(i == own ? " (own)" : "")} refs {bundle.ReferenceCount} status {bundle.LoadStatus} object {Identity(handle)}");
            if (i == own && !ReferenceEquals(handle, null) && handle != null && !asset.m_assetPathInBundle.EndsWith(".unity"))
            {
                try
                {
                    Object raw = handle.LoadAsset(asset.m_assetPathInBundle);
                    lines.Add($"  raw {Identity(raw)} persistent {Persistent(raw)} same-as-m_asset {(!ReferenceEquals(raw, null) && !ReferenceEquals(loaded, null) && raw.GetInstanceID() == loaded.GetInstanceID())}");
                    if (raw is GameObject rawGo && rawGo != null)
                        lines.AddRange(Census("  raw", rawGo, full: fullCensus));
                }
                catch (Exception ex)
                {
                    lines.Add($"  raw: LoadAsset threw {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        if (loaded is GameObject loadedGo && loadedGo != null)
        {
            lines.AddRange(Census("  m_asset", loadedGo, full: fullCensus));
            if (fullCensus)
                lines.AddRange(Tree("    m_asset tree", loadedGo, 24));
        }
        return lines;
    }

    private static string ResolutionContext(string name)
    {
        try
        {
            SoftReference<GameObject> reference = AssetManager.Instance.GetSoftReference<GameObject>(name);
            if (!reference.IsValid)
                return "resolution: (no soft reference)";
            var table = AccessTools.Field(typeof(AssetManager), "assetsToResolve")?.GetValue(AssetManager.Instance) as IDictionary;
            if (table == null)
                return "resolution: table unreadable";
            if (!table.Contains(reference.m_assetID))
                return "resolution: no context — nothing has asked for mocks to be resolved";
            object context = table[reference.m_assetID];
            Traverse t = Traverse.Create(context);
            Object? clone = t.Property("Asset").GetValue<Object>();
            Transform? parent = t.Property("Parent").GetValue<Transform>();
            bool resolved = t.Property("IsResolved").GetValue<bool>();
            return $"resolution: clone {Identity(clone)} resolved {resolved} parent {(parent != null ? PathOf(parent) : "-")}";
        }
        catch (Exception ex)
        {
            return $"resolution: threw {ex.GetType().Name}: {ex.Message}";
        }
    }

    private static string ZNetScenePrefab(string name)
    {
        try
        {
            if (ZNetScene.instance == null)
                return $"znetscene {name}: no ZNetScene";
            GameObject prefab = ZNetScene.instance.GetPrefab(name);
            return $"znetscene {name}: {Identity(prefab)} | {Summary(prefab)}";
        }
        catch (Exception ex)
        {
            return $"znetscene {name}: threw {ex.GetType().Name}: {ex.Message}";
        }
    }

    private static string CacheEntry(string name)
    {
        try
        {
            var cache = AccessTools.Field(typeof(PrefabManager.Cache), "dictionaryCache")?.GetValue(null)
                as Dictionary<Type, Dictionary<string, Object>>;
            if (cache == null)
                return $"cache {name}: unreadable";
            if (!cache.TryGetValue(typeof(GameObject), out Dictionary<string, Object> map))
                return $"cache {name}: no GameObject map has been built";
            if (!map.TryGetValue(name, out Object entry))
                return $"cache {name}: no entry ({map.Count} names in the map)";
            var go = entry as GameObject;
            string parent = go != null && go.transform.parent != null ? PathOf(go.transform.parent) : "-";
            return $"cache {name}: {Identity(entry)} parent {parent} | {Summary(go)} ({map.Count} names in the map)";
        }
        catch (Exception ex)
        {
            return $"cache {name}: threw {ex.GetType().Name}: {ex.Message}";
        }
    }

    /// <summary>
    /// Every object and component under a root, with each component named by
    /// type and instance ID, and CLR null, destroyed and alive told apart.
    /// </summary>
    private static IEnumerable<string> Census(string label, GameObject root, bool full)
    {
        var lines = new List<string>();
        int objects = 0, components = 0, nulls = 0, destroyed = 0;
        var interesting = new List<string>();
        var nullPaths = new List<string>();
        try
        {
            Walk(root.transform, root.transform, root.name);
        }
        catch (Exception ex)
        {
            lines.Add($"{label} census threw {ex.GetType().Name}: {ex.Message}");
        }
        lines.Add($"{label} {Identity(root)} active {root.activeSelf}: {objects} object(s), {components} component(s), {nulls} CLR-null, {destroyed} destroyed");
        int cap = full ? 24 : 6;
        for (int i = 0; i < nullPaths.Count && i < cap; i++)
            lines.Add($"    null at {nullPaths[i]}");
        for (int i = 0; i < interesting.Count && i < cap; i++)
            lines.Add($"    {interesting[i]}");
        if (interesting.Count > cap)
            lines.Add($"    ... {interesting.Count - cap} more");
        return lines;

        void Walk(Transform node, Transform top, string path)
        {
            objects++;
            Component[] found = node.gameObject.GetComponents<Component>();
            var parts = new List<string>();
            bool worth = IsInteresting(node.name);
            for (int i = 0; i < found.Length; i++)
            {
                components++;
                Component c = found[i];
                if (ReferenceEquals(c, null))
                {
                    nulls++;
                    nullPaths.Add($"{path}#{i}");
                    parts.Add($"NULL@{i}");
                    worth = true;
                }
                else if (c == null)
                {
                    destroyed++;
                    parts.Add($"DESTROYED@{i}#{c.GetInstanceID()}");
                    worth = true;
                }
                else
                    parts.Add($"{c.GetType().Name}#{c.GetInstanceID()}");
            }
            if (worth)
                interesting.Add($"{path} [{node.gameObject.GetInstanceID()}] active {node.gameObject.activeSelf}: {string.Join(", ", parts.ToArray())}");
            for (int i = 0; i < node.childCount; i++)
            {
                Transform child = node.GetChild(i);
                Walk(child, top, path + "/" + child.name);
            }
        }
    }

    private static bool IsInteresting(string name)
    {
        if (name.IndexOf("Point light", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        foreach (string probe in s_probes)
        {
            if (name.IndexOf(probe, StringComparison.Ordinal) >= 0)
                return true;
        }
        return false;
    }

    private static string Summary(GameObject? go)
    {
        if (ReferenceEquals(go, null) || go == null)
            return "-";
        var lines = new List<string>(Census("", go, full: false));
        return string.Join(" || ", lines.ToArray()).Replace("\n", " ");
    }

    private static string Identity(Object? o)
    {
        if (ReferenceEquals(o, null))
            return "clr-null";
        if (o == null)
            return $"DESTROYED#{o.GetInstanceID()}";
        return $"{o.name}#{o.GetInstanceID()}";
    }

    private static string Persistent(Object? o)
    {
        try
        {
            if (ReferenceEquals(o, null) || o == null)
                return "-";
            var method = AccessTools.Method(typeof(Object), "IsPersistent");
            return method == null ? "?" : ((bool)method.Invoke(null, new object[] { o })).ToString();
        }
        catch
        {
            return "?";
        }
    }

    private static string PathOf(Transform node)
    {
        var parts = new List<string>();
        Transform? current = node;
        while (current != null)
        {
            parts.Add(current.name);
            current = current.parent;
        }
        parts.Reverse();
        return string.Join("/", parts.ToArray());
    }
}
