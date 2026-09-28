using System.Collections.Generic;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>Whether a finding decides the verdict or only warns.</summary>
public enum FindingSeverity
{
    /// <summary>Worth knowing and not a reason to exclude the template.</summary>
    Advisory,

    /// <summary>This template cannot be served to a client without the mod.</summary>
    Blocking,

    /// <summary>
    /// Not enough was established to say either way.
    ///
    /// Separate from <see cref="Blocking"/> because the two need different work:
    /// a block is a fact about the template and an unresolved is a gap in what
    /// this run could see. Folding them together would let a missing baseline be
    /// reported as the author's defect — and, far worse, would invite somebody to
    /// "fix" it by removing the check.
    /// </summary>
    Unresolving,
}

/// <summary>
/// The reason codes, as constants rather than an enum.
///
/// They are exported, read by an operator and compared between runs, so they
/// have to be stable text. An enum's names are stable too, but its numbering is
/// not, and a report keyed by number is a report nobody can read a year later.
/// </summary>
public static class FindingCodes
{
    /// <summary>The pack is out of scope for server-only mode, whatever the template contains.</summary>
    public const string ScopeExcludedPack = "scope_excluded_pack";

    /// <summary>An asset exists under this name and no location definition places it.</summary>
    public const string NoActiveDefinition = "no_active_definition";

    /// <summary>The soft-referenced template would not load, so nothing about it is known.</summary>
    public const string TemplateNotLoaded = "template_not_loaded";

    /// <summary>The walk over the template did not finish. Not a pass and not a block.</summary>
    public const string ExtractionIncomplete = "extraction_incomplete";

    /// <summary>The stock snapshot is missing or was taken from a different game build.</summary>
    public const string RegistryUnavailable = "registry_unavailable";

    /// <summary>The template root is inactive, so the game spawns none of its children.</summary>
    public const string InactiveRoot = "inactive_root";

    /// <summary>The template emits nothing at all: a site that is a name on a map and no objects.</summary>
    public const string NothingEmitted = "nothing_emitted";

    /// <summary>A networked child's prefab is not in the stock registry.</summary>
    public const string UnknownPrefab = "unknown_prefab";

    /// <summary>A networked child's name hashes to a DIFFERENT stock prefab, which is what the client would build.</summary>
    public const string PrefabHashCollision = "prefab_hash_collision";

    /// <summary>A mock Jötunn did not resolve, so the server itself holds a placeholder.</summary>
    public const string UnresolvedMock = "unresolved_mock";

    /// <summary>The mock prefix appears twice, so Jötunn strips one and looks for a name that does not exist.</summary>
    public const string DoubleMockPrefix = "double_mock_prefix";

    /// <summary>A rendered or collidable object that only a client with the template would build.</summary>
    public const string EssentialProxyOnly = "essential_proxy_only";

    /// <summary>A networked child carries a component from an assembly the stock client does not have.</summary>
    public const string CustomComponent = "custom_component";

    /// <summary>A networked child is scaled and its prefab does not send the scale, so the client builds it at 1.</summary>
    public const string ScaleNotSynced = "scale_not_synced";

    /// <summary>A networked child is scaled and the STOCK prefab the client builds ignores a sent scale.</summary>
    public const string ScaleNotReceived = "scale_not_received";

    /// <summary>A networked child's ZDO is not persistent, so the object is gone after a save.</summary>
    public const string NotPersistent = "not_persistent";

    /// <summary>A prefab this template can emit later — a spawn, a drop, a container's contents — is not stock.</summary>
    public const string UnknownSpawnReference = "unknown_spawn_reference";

    /// <summary>The location generates an interior from dungeon rooms, which are not vanilla prefabs.</summary>
    public const string InteriorDungeon = "interior_dungeon";

    /// <summary>The template's terrain can reach further than the conversion can see from one zone. A placement risk, not a template defect.</summary>
    public const string TerrainReachBeyondRing = "terrain_reach_beyond_ring";

    /// <summary>An object the client receives is not the stock prefab of that name: something was added, removed or changed beneath it.</summary>
    public const string ModifiedStockSubtree = "modified_stock_subtree";

    /// <summary>No stock prefab was available to compare an object against, so nothing can be said about what the client would build.</summary>
    public const string StockBaselineUnavailable = "stock_baseline_unavailable";

    /// <summary>Component types were present whose settings this build cannot read, so the comparison is not a claim of full equivalence.</summary>
    public const string SubtreeNotFullyCompared = "subtree_not_fully_compared";

    /// <summary>The baseline came from a registry other mods could already have changed, so it is not independent stock evidence.</summary>
    public const string StockBaselineProvenance = "stock_baseline_provenance";
}

/// <summary>
/// One reason, with enough in it to act on without opening a station log:
/// which template, which object, what the value was, and why it matters.
/// </summary>
public sealed class TemplateFinding
{
    public TemplateFinding(string code, FindingSeverity severity, string path, string detail, string value = "")
    {
        Code = code ?? "";
        Severity = severity;
        Path = path ?? "";
        Detail = detail ?? "";
        Value = value ?? "";
    }

    public string Code { get; }
    public FindingSeverity Severity { get; }

    /// <summary>The object inside the template, from the root down. Empty when the finding is about the template itself.</summary>
    public string Path { get; }

    /// <summary>The measured value the finding turns on — a prefab name, a scale, a distance.</summary>
    public string Value { get; }

    /// <summary>Why this stops a client without the mod from seeing what the author built.</summary>
    public string Detail { get; }

    public override string ToString() =>
        Path.Length == 0 ? $"{Code}: {Detail}" : $"{Code} at {Path}: {Detail}";
}

/// <summary>What the policy decided about one template.</summary>
public enum TemplateVerdict
{
    /// <summary>Every part a player needs arrives on a client without the mod.</summary>
    Compatible,

    /// <summary>Something essential cannot arrive. The reasons say what.</summary>
    Blocked,

    /// <summary>Not enough was read to say. Never treated as either of the above.</summary>
    Unresolved,

    /// <summary>Out of scope for this mode by pack, before any technical question is asked.</summary>
    ScopeExcluded,

    /// <summary>An asset with no location definition: nothing places it, so there is nothing to judge.</summary>
    MissingDefinition,
}

/// <summary>One template's verdict and every reason behind it.</summary>
public sealed class TemplateEvaluation
{
    public TemplateEvaluation(string name, string pack, TemplateVerdict verdict, IReadOnlyList<TemplateFinding> findings)
    {
        Name = name ?? "";
        Pack = pack ?? "";
        Verdict = verdict;
        Findings = findings ?? System.Array.Empty<TemplateFinding>();
    }

    public string Name { get; }
    public string Pack { get; }
    public TemplateVerdict Verdict { get; }
    public IReadOnlyList<TemplateFinding> Findings { get; }

    /// <summary>Whether server-only mode may register this template.</summary>
    public bool Approved => Verdict == TemplateVerdict.Compatible;

    /// <summary>The distinct reason codes, in the order they were found, for a one-line summary.</summary>
    public IReadOnlyList<string> Codes
    {
        get
        {
            var seen = new HashSet<string>();
            var codes = new List<string>();
            foreach (TemplateFinding finding in Findings)
            {
                if (seen.Add(finding.Code))
                    codes.Add(finding.Code);
            }
            return codes;
        }
    }
}
