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
/// <para><b>What it covers, and why more than the rules read.</b> An earlier
/// version covered exactly the facts the policy consumes, on the theory that a
/// change no rule can see cannot matter. That was wrong, and the way it was
/// wrong is the point of the whole mechanism: an approval is not "the rules
/// passed", it is "this template was watched in game and behaved". Moving a
/// floor twenty metres, cutting the ground ten metres deeper or swapping the
/// order two modifiers are applied in leaves every rule's answer identical and
/// destroys the evidence. So the fingerprint binds the resolved CONTENT —
/// geometry, every terrain value, the objects and what they can emit — and the
/// separate policy digest binds the rules.</para>
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
    /// Bumped whenever the CONTENT digest's inputs change, so that a build with
    /// a wider fingerprint re-audits instead of comparing two digests that were
    /// never computed the same way.
    /// </summary>
    public const int ContentVersion = 2;

    /// <summary>
    /// Bumped whenever a rule changes what a verdict would be.
    ///
    /// It is a number in source rather than a hash of the source because a
    /// comment or a reworded reason is not a new rule, and a build that
    /// re-audited the whole catalogue over a typo would teach everyone to
    /// ignore the drift report.
    /// </summary>
    // 2: the scale rule asks whether the STOCK prefab reads a sent scale, not
    // only whether the template sends one. Approvals granted under version 1
    // were granted by a rule that looked at the sender alone.
    public const int PolicyVersion = 2;

    /// <summary>
    /// The facts as one canonical string. Exposed because a fingerprint that
    /// differs is useless on its own: this is what a run diffs to find out
    /// WHICH fact moved.
    /// </summary>
    public static string Canonical(TemplateFacts facts)
    {
        if (facts == null) throw new System.ArgumentNullException(nameof(facts));

        var text = new StringBuilder();
        text.Append("content=").Append(ContentVersion).Append('\n');
        text.Append("name=").Append(facts.Name).Append('\n');
        text.Append("pack=").Append(facts.Pack).Append('\n');
        text.Append("root=").Append(facts.RootActive ? '1' : '0').Append('\n');
        text.Append("interior=").Append(facts.InteriorPrefabName).Append('|').Append(facts.DungeonTheme).Append('\n');

        // Sorted by path, because the walk's order is the engine's. The path
        // already carries the hierarchy, so an object that moved WITHIN the
        // hierarchy changes its path and is a different line.
        var children = new List<ChildFact>(facts.Children);
        children.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));
        foreach (ChildFact child in children)
        {
            text.Append("child=").Append(child.Path).Append('|').Append(child.PrefabName)
                .Append('|').Append(Flags(child))
                .Append("|s:").Append(Point(child.Scale.X, child.Scale.Y, child.Scale.Z))
                .Append("|p:").Append(Point(child.RelativePosition.X, child.RelativePosition.Y, child.RelativePosition.Z))
                .Append("|r:").Append(Point(child.EulerAngles.X, child.EulerAngles.Y, child.EulerAngles.Z));
            foreach (string component in Sorted(child.ForeignComponents))
                text.Append("|c:").Append(component);
            foreach (string referenced in Sorted(child.ReferencedPrefabs))
                text.Append("|r:").Append(referenced);
            // The subtree the client is supposed to build, whatever the rules
            // made of it.
            if (child.AuthoredSignature.Length > 0)
                text.Append("|t:").Append(Digest(child.AuthoredSignature));
            text.Append('\n');
        }

        // In VANILLA's order, not the walk's. Each modifier reads what the one
        // before it left, so the same modifiers applied in a different order
        // draw different ground -- and the order is m_sortOrder, which a walk
        // over the hierarchy cannot see.
        foreach (TerrainFact modifier in TerrainModifierOrder.Apply(
                     facts.Terrain, m => m.PlayerModification, m => m.SortOrder))
        {
            text.Append("terrain=").Append(modifier.Path)
                .Append('|').Append(modifier.Enabled ? '1' : '0')
                .Append(modifier.UseTerrainCompiler ? '1' : '0')
                .Append(modifier.PlayerModification ? '1' : '0')
                .Append('|').Append(modifier.SortOrder.ToString(CultureInfo.InvariantCulture))
                .Append("|lvl:").Append(modifier.Level ? '1' : '0')
                .Append(',').Append(Number(modifier.LevelRadius))
                .Append(',').Append(Number(modifier.LevelOffset))
                .Append(',').Append(modifier.Square ? '1' : '0')
                .Append("|smo:").Append(modifier.Smooth ? '1' : '0')
                .Append(',').Append(Number(modifier.SmoothRadius))
                .Append(',').Append(Number(modifier.SmoothPower))
                .Append("|pnt:").Append(modifier.Paint ? '1' : '0')
                .Append(',').Append(modifier.PaintType)
                .Append(',').Append(Number(modifier.PaintRadius))
                .Append(',').Append(Number(modifier.PaintStrength))
                .Append(',').Append(modifier.PaintHeightCheck ? '1' : '0')
                .Append("|at:").Append(Point(
                    modifier.RelativePosition.X, modifier.RelativePosition.Y, modifier.RelativePosition.Z))
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
        // -0 and 0 are the same number and must not be two fingerprints.
        (value == 0f ? 0f : value).ToString("0.###", CultureInfo.InvariantCulture);

    private static string Point(float x, float y, float z) =>
        Number(x) + "," + Number(y) + "," + Number(z);

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
