using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// A short, stable digest of everything the policy reads off a template.
///
/// <para><b>What it is for.</b> An approval is a claim about a particular
/// template. Content changes — the author ships a new version, a dependency
/// resolves differently, Jötunn's resolution brings something else in — and an
/// approval that survives that is an approval of something nobody checked. So
/// the generated selection stores a fingerprint beside each name and the
/// runtime recomputes it; a name whose template no longer matches is excluded
/// and said so, rather than registered on last month's evidence.</para>
///
/// <para><b>What it covers, and why exactly that.</b> The facts the policy
/// consumes, and nothing else. Covering less would let a policy-relevant change
/// through unnoticed; covering more — an object's position, say — would make
/// the fingerprint drift on changes that cannot alter any verdict, and a guard
/// that cries wolf gets turned off.</para>
/// </summary>
public static class TemplateFingerprint
{
    /// <summary>
    /// The digest of one template's facts.
    ///
    /// FNV-1a over a canonical rendering rather than a framework hash: this
    /// value is written into a shipped file and compared on another machine and
    /// another runtime, and <c>string.GetHashCode</c> is randomised per process.
    /// </summary>
    public static string Of(TemplateFacts facts)
    {
        if (facts == null) throw new System.ArgumentNullException(nameof(facts));
        return Digest(Canonical(facts));
    }

    /// <summary>
    /// The digest of the policy itself: the rules' version, the stock snapshot
    /// the names were checked against, and which components were forgiven.
    ///
    /// Stored once at the head of the selection. An approval is only as good as
    /// the rules that granted it, so a build with different rules re-audits
    /// rather than inheriting.
    /// </summary>
    public static string OfPolicy(StockPrefabRegistry registry, ComponentPolicy components, IReadOnlyCollection<string> excludedPacks)
    {
        if (registry == null) throw new System.ArgumentNullException(nameof(registry));
        if (components == null) throw new System.ArgumentNullException(nameof(components));
        if (excludedPacks == null) throw new System.ArgumentNullException(nameof(excludedPacks));

        var text = new StringBuilder();
        text.Append("policy=").Append(PolicyVersion).Append('\n');
        text.Append("registry=").Append(registry.GameBuildId).Append(':').Append(registry.Count).Append('\n');
        foreach (string name in Sorted(components.HarmlessForeignComponents))
            text.Append("harmless=").Append(name).Append('\n');
        foreach (string pack in Sorted(excludedPacks))
            text.Append("excluded=").Append(pack).Append('\n');
        return Digest(text.ToString());
    }

    /// <summary>
    /// Bumped whenever a rule changes what a verdict would be.
    ///
    /// It is a number in source rather than a hash of the source because a
    /// comment or a reworded reason is not a new rule, and a build that
    /// re-audited the whole catalogue over a typo would teach everyone to
    /// ignore the drift report.
    /// </summary>
    public const int PolicyVersion = 1;

    /// <summary>
    /// The facts as one canonical string. Exposed because a fingerprint that
    /// differs is useless on its own: this is what a run diffs to find out
    /// WHICH fact moved.
    /// </summary>
    public static string Canonical(TemplateFacts facts)
    {
        if (facts == null) throw new System.ArgumentNullException(nameof(facts));

        var text = new StringBuilder();
        text.Append("name=").Append(facts.Name).Append('\n');
        text.Append("pack=").Append(facts.Pack).Append('\n');
        text.Append("root=").Append(facts.RootActive ? '1' : '0').Append('\n');
        text.Append("interior=").Append(facts.InteriorPrefabName).Append('|').Append(facts.DungeonTheme).Append('\n');

        // Sorted by path, because the walk's order is the engine's and a child
        // that moved in the hierarchy without changing is not a content change.
        var children = new List<ChildFact>(facts.Children);
        children.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));
        foreach (ChildFact child in children)
        {
            text.Append("child=").Append(child.Path).Append('|').Append(child.PrefabName)
                .Append('|').Append(Flags(child))
                .Append('|').Append(Number(child.Scale.X)).Append(',').Append(Number(child.Scale.Y)).Append(',').Append(Number(child.Scale.Z));
            foreach (string component in Sorted(child.ForeignComponents))
                text.Append("|c:").Append(component);
            foreach (string referenced in Sorted(child.ReferencedPrefabs))
                text.Append("|r:").Append(referenced);
            text.Append('\n');
        }

        // Authored order, NOT sorted: the order terrain modifiers appear in is
        // the order vanilla applies them, so two templates with the same
        // modifiers in a different order are different ground.
        foreach (TerrainFact modifier in facts.Terrain)
        {
            text.Append("terrain=").Append(modifier.Path)
                .Append('|').Append(modifier.Enabled ? '1' : '0')
                .Append(modifier.UseTerrainCompiler ? '1' : '0')
                .Append(modifier.Level ? '1' : '0')
                .Append(modifier.Smooth ? '1' : '0')
                .Append(modifier.Paint ? '1' : '0')
                .Append('|').Append(modifier.PaintType)
                .Append('|').Append(Number(modifier.Reach))
                .Append('\n');
        }
        return text.ToString();
    }

    private static string Flags(ChildFact child)
    {
        var flags = new StringBuilder(6);
        flags.Append(child.Networked ? 'n' : '-');
        flags.Append(child.UnderNetworkedAncestor ? 'a' : '-');
        flags.Append(child.EnabledInHierarchy ? 'e' : '-');
        flags.Append(child.Persistent ? 'p' : '-');
        flags.Append(child.SyncInitialScale ? 's' : '-');
        flags.Append(child.HasRenderer ? 'R' : '-');
        flags.Append(child.HasCollider ? 'C' : '-');
        return flags.ToString();
    }

    /// <summary>
    /// A float as fixed text.
    ///
    /// Three decimals, invariant: a fingerprint is compared across machines, and
    /// round-trip formatting differs between runtimes at the last digit. Three
    /// decimals is finer than any difference a policy rule turns on and coarser
    /// than the noise.
    /// </summary>
    private static string Number(float value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);

    private static List<string> Sorted(IEnumerable<string> values)
    {
        var sorted = new List<string>(values);
        sorted.Sort(System.StringComparer.Ordinal);
        return sorted;
    }

    /// <summary>FNV-1a, 64 bit, over UTF-8, as 16 hex characters.</summary>
    public static string Digest(string text)
    {
        if (text == null) throw new System.ArgumentNullException(nameof(text));

        const ulong offsetBasis = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;

        ulong hash = offsetBasis;
        foreach (byte b in Encoding.UTF8.GetBytes(text))
        {
            hash ^= b;
            hash *= prime;
        }
        return hash.ToString("x16", CultureInfo.InvariantCulture);
    }
}
