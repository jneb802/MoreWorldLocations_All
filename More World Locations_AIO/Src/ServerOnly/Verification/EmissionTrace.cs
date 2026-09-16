using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// What vanilla selected and emitted for one placement of a traced template,
/// and what later destroyed any of it: recorded at the game's own boundaries,
/// never inferred from what a census finds afterwards.
///
/// <para><b>Why.</b> A site short of a chest can be short because a
/// <c>RandomSpawn</c> left the branch out, because the object was never
/// instantiated, or because something destroyed it after it stood. A census
/// cannot tell those apart, and a "selected outcome" read off the census
/// hides a selected branch whose chest is missing. So the record is taken
/// where the decisions are made: after each randomiser's <c>Randomize</c>,
/// at each emitted object's <c>ZNetView.Awake</c> (the moment its ZDO exists),
/// and at the ZDO manager's destroy paths.</para>
///
/// <para><b>Diagnostic only.</b> Installed by the
/// <see cref="ValidationSwitches.EmissionTraceVariable"/> switch; a shipped run
/// has no observers and pays nothing. Nothing here decides anything. Every
/// row carries the site's identity (definition, position, seed) so a station
/// script can cut one site's rows out of a whole log.</para>
///
/// <para><b>Rows</b>, tab separated after the tag:
/// <list type="bullet">
/// <item><c>S</c> definition, site, seed, mode, template fingerprint, netviews considered</item>
/// <item><c>R</c> definition, site, seed, component, randomiser path, spawned 0/1</item>
/// <item><c>C</c> definition, site, seed, randomiser path, member path, prefab, active 0/1</item>
/// <item><c>E</c> definition, site, seed, member path or ?, prefab, ZDO id, world position</item>
/// <item><c>Z</c> definition, site, seed, emitted count</item>
/// <item><c>X</c> definition, site, seed, member path, prefab, ZDO id, by</item>
/// </list>
/// A member's state in a <c>C</c> row is its state after THAT randomiser ran.
/// A member under two randomisers is inactive at emission if either turned it
/// off, so a reader takes the conjunction. <c>E</c> is the truth of emission
/// and is what a reader compares a selected member against.</para>
/// </summary>
public static class EmissionTrace
{
    public const string Tag = "[EMISSION]";

    /// <summary>Where rows go. The BepInEx log unless a test captures them.</summary>
    public static Action<string>? Sink;

    private static readonly HashSet<string> s_names = new(StringComparer.Ordinal);
    private static bool s_all;
    private static bool s_enabled;

    private sealed class Source
    {
        public ZNetView View = null!;
        public string Path = "";
        public string Prefab = "";
        public Vector3 WorldPosition;
    }

    private sealed class Site
    {
        public string Definition = "";
        public string Key = "";
        public GameObject Asset = null!;
        public List<Source> Sources = new();
        public int Emitted;
    }

    private sealed class Traced
    {
        public string Key = "";
        public string Path = "";
        public string Prefab = "";
    }

    private static Site? s_current;
    private static readonly Dictionary<ZDOID, Traced> s_zdos = new();

    /// <summary>Whether any template is traced at all. False is the shipped state.</summary>
    public static bool Enabled => s_enabled;

    /// <summary>Configure from the switch's parse: names to trace, or every template.</summary>
    public static void Configure(IReadOnlyCollection<string> names, bool all)
    {
        s_names.Clear();
        foreach (string name in names)
            s_names.Add(name);
        s_all = all;
        s_enabled = all || s_names.Count > 0;
    }

    public static bool Traces(string definition) =>
        s_enabled && (s_all || s_names.Contains(definition));

    /// <summary>A new world, or a test: nothing from before applies.</summary>
    public static void Forget()
    {
        s_current = null;
        s_zdos.Clear();
    }

    /// <summary>The site a trace is currently inside, or null.</summary>
    public static string? CurrentSite => s_current?.Key;

    /// <summary>How many emitted objects are being watched for destruction.</summary>
    public static int Watched => s_zdos.Count;

    // ---- the spawn ----------------------------------------------------------

    /// <summary>
    /// Vanilla is about to spawn <paramref name="definition"/> at a site. Called
    /// from the prefix, before the game resets the asset root, so member
    /// positions are taken relative to the root as it stands.
    /// </summary>
    public static void BeginSite(string definition, GameObject asset, int seed, Vector3 pos, Quaternion rot,
        string mode, string fingerprint, IEnumerable<ZNetView> enabledViews)
    {
        if (!Traces(definition) || asset == null)
            return;

        Site site = new Site
        {
            Definition = definition,
            Asset = asset,
            Key = Key(definition, pos, seed),
        };
        Transform root = asset.transform;
        Quaternion unroot = Conjugate(root.rotation);
        foreach (ZNetView view in enabledViews)
        {
            if (view == null)
                continue;
            Transform node = view.transform;
            Vector3 local = unroot * (node.position - root.position);
            site.Sources.Add(new Source
            {
                View = view,
                Path = PathOf(node, root, definition),
                Prefab = view.gameObject.name,
                WorldPosition = pos + rot * local,
            });
        }
        s_current = site;
        Emit("S", site.Key, mode, fingerprint, site.Sources.Count.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// A randomiser has decided. <paramref name="spawned"/> is the randomiser's
    /// own object's active state after the decision; <paramref name="members"/>
    /// are the networked objects under it (itself included when it is one),
    /// each with the state vanilla will test at emission.
    /// </summary>
    public static void Randomised(string component, GameObject randomiser, bool spawned, IEnumerable<GameObject> members)
    {
        Site? site = s_current;
        if (site == null || randomiser == null)
            return;
        string path = PathOf(randomiser.transform, site.Asset.transform, site.Definition);
        Emit("R", site.Key, component, path, spawned ? "1" : "0");
        foreach (GameObject member in members)
        {
            if (member == null)
                continue;
            Emit("C", site.Key, path, PathOf(member.transform, site.Asset.transform, site.Definition),
                member.name, member.activeSelf ? "1" : "0");
        }
    }

    /// <summary>
    /// An object has just received its ZDO during the spawn. Matched back to
    /// the source member by prefab name and predicted world position; an
    /// object nothing predicts (a dungeon room, for instance) is recorded with
    /// path <c>?</c> rather than guessed.
    /// </summary>
    public static void Emitted(ZNetView view, ZDO? zdo)
    {
        Site? site = s_current;
        if (site == null || view == null || zdo == null)
            return;
        string clone = view.gameObject.name;
        if (clone.EndsWith("(Clone)", StringComparison.Ordinal))
            clone = clone.Substring(0, clone.Length - "(Clone)".Length);
        Vector3 at = view.transform.position;

        Source? best = null;
        float bestDistance = 0.01f;
        foreach (Source source in site.Sources)
        {
            if (!string.Equals(source.Prefab, clone, StringComparison.Ordinal))
                continue;
            float d = Vector3.Distance(source.WorldPosition, at);
            if (d <= bestDistance)
            {
                best = source;
                bestDistance = d;
            }
        }
        string path = best?.Path ?? "?";
        site.Emitted++;
        s_zdos[zdo.m_uid] = new Traced { Key = site.Key, Path = path, Prefab = clone };
        Emit("E", site.Key, path, clone, zdo.m_uid.ToString(), V(at));
    }

    /// <summary>The spawn has returned. Closes the site; later rows are destructions.</summary>
    public static void EndSite()
    {
        Site? site = s_current;
        if (site == null)
            return;
        Emit("Z", site.Key, site.Emitted.ToString(CultureInfo.InvariantCulture));
        s_current = null;
    }

    // ---- afterwards ----------------------------------------------------------

    /// <summary>A ZDO is being destroyed. Only a traced emission is recorded.</summary>
    public static void Destroyed(ZDOID uid, string by)
    {
        if (!s_zdos.TryGetValue(uid, out Traced? traced))
            return;
        s_zdos.Remove(uid);
        Emit("X", traced.Key, traced.Path, traced.Prefab, uid.ToString(), by);
    }

    // ---- helpers -------------------------------------------------------------

    /// <summary>The networked objects under one object, itself included.</summary>
    public static List<GameObject> NetworkedUnder(GameObject go)
    {
        List<GameObject> found = new List<GameObject>();
        Collect(go, found);
        return found;
    }

    private static void Collect(GameObject go, List<GameObject> into)
    {
        if (go.GetComponent<ZNetView>() != null)
            into.Add(go);
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            Collect(t.GetChild(i).gameObject, into);
    }

    /// <summary>The same spelling the plan uses: root name, then each object's name.</summary>
    public static string PathOf(Transform node, Transform root, string rootName)
    {
        List<string> parts = new List<string>();
        Transform? current = node;
        while (current != null && current != root)
        {
            parts.Add(current.name);
            current = current.parent;
        }
        parts.Add(rootName);
        parts.Reverse();
        return string.Join("/", parts.ToArray());
    }

    public static string Key(string definition, Vector3 pos, int seed) =>
        definition + "\t" + V(pos) + "\t" + seed.ToString(CultureInfo.InvariantCulture);

    private static string V(Vector3 v) =>
        v.x.ToString("0.###", CultureInfo.InvariantCulture) + "," +
        v.y.ToString("0.###", CultureInfo.InvariantCulture) + "," +
        v.z.ToString("0.###", CultureInfo.InvariantCulture);

    private static Quaternion Conjugate(Quaternion q) => new Quaternion(-q.x, -q.y, -q.z, q.w);

    private static void Emit(string kind, params string[] fields)
    {
        StringBuilder text = new StringBuilder(Tag).Append(' ').Append(kind);
        foreach (string field in fields)
            text.Append('\t').Append(field);
        string line = text.ToString();
        try
        {
            if (Sink != null)
                Sink(line);
            else
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(line);
        }
        catch
        {
            // A trace that cannot be written must not become a spawn that fails.
        }
    }
}
