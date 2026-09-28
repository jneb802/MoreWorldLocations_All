using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>One name's complete disposition: what was measured, judged and done.</summary>
public sealed class CatalogueEntry
{
    public CatalogueEntry(TemplateEvaluation evaluation, SelectionDecision decision, string contentFingerprint)
    {
        Evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
        Decision = decision ?? throw new ArgumentNullException(nameof(decision));
        ContentFingerprint = contentFingerprint ?? "";
    }

    public TemplateEvaluation Evaluation { get; }
    public SelectionDecision Decision { get; }
    public string ContentFingerprint { get; }

    public string Name => Evaluation.Name;
    public string Pack => Evaluation.Pack;
    public bool Registered => Decision.Registered;
}

/// <summary>
/// What the audit found, in the shapes a person asks for it.
///
/// <para>The reason this is a type and not a log line: the question an operator
/// actually has is "why is <c>MWL_SwampTemple1</c> not in my world", and a
/// number of templates by verdict cannot answer it. So the report keeps every
/// name's disposition and can be asked about one, and the summary is a
/// convenience over that rather than the only thing kept.</para>
///
/// <para>Scope exclusions are counted apart from technical failures throughout.
/// A trader post that is missing because this mode does not ship traders and a
/// ruin that is missing because one of its walls is a custom prefab are not the
/// same news, and adding them together produces a number that means
/// nothing.</para>
/// </summary>
public sealed class CatalogueReport
{
    private readonly List<CatalogueEntry> _entries;
    private readonly Dictionary<string, CatalogueEntry> _byName;

    public CatalogueReport(IEnumerable<CatalogueEntry> entries, string stockBuildId = "", string policyFingerprint = "")
    {
        if (entries == null) throw new ArgumentNullException(nameof(entries));

        _entries = new List<CatalogueEntry>(entries);
        // Stable source order is the catalogue's order, which is the order the
        // packs are declared in. Sorting here would make the report disagree
        // with the order registration walks, and the two being comparable is
        // worth more than alphabetical.
        _byName = new Dictionary<string, CatalogueEntry>(StringComparer.Ordinal);
        foreach (CatalogueEntry entry in _entries)
        {
            // A duplicate name is a bug in the caller, not something to average
            // over: the second one would silently decide the report.
            if (_byName.ContainsKey(entry.Name))
                throw new ArgumentException($"'{entry.Name}' appears twice in the catalogue");
            _byName[entry.Name] = entry;
        }
        StockBuildId = stockBuildId ?? "";
        PolicyFingerprint = policyFingerprint ?? "";
    }

    public IReadOnlyList<CatalogueEntry> Entries => _entries;

    /// <summary>The game build the names were checked against.</summary>
    public string StockBuildId { get; }

    /// <summary>The rules the verdicts were reached under.</summary>
    public string PolicyFingerprint { get; }

    public CatalogueEntry? Find(string name) =>
        name != null && _byName.TryGetValue(name, out CatalogueEntry entry) ? entry : null;

    public int CountOf(TemplateVerdict verdict)
    {
        int count = 0;
        foreach (CatalogueEntry entry in _entries)
        {
            if (entry.Evaluation.Verdict == verdict)
                count++;
        }
        return count;
    }

    public int RegisteredCount
    {
        get
        {
            int count = 0;
            foreach (CatalogueEntry entry in _entries)
            {
                if (entry.Registered)
                    count++;
            }
            return count;
        }
    }

    /// <summary>
    /// How many templates each blocking reason accounts for.
    ///
    /// By template rather than by finding: one unsupported kit piece used 135
    /// times in one build is one template to exclude, and counting the findings
    /// would put it at the top of a list of what to fix next when it is a single
    /// decision.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, int>> BlockingReasons()
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (CatalogueEntry entry in _entries)
        {
            if (entry.Evaluation.Verdict == TemplateVerdict.Compatible)
                continue;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (TemplateFinding finding in entry.Evaluation.Findings)
            {
                if (finding.Severity != FindingSeverity.Blocking || !seen.Add(finding.Code))
                    continue;
                counts.TryGetValue(finding.Code, out int count);
                counts[finding.Code] = count + 1;
            }
        }

        var ordered = new List<KeyValuePair<string, int>>(counts);
        ordered.Sort((a, b) => a.Value != b.Value ? b.Value.CompareTo(a.Value) : string.CompareOrdinal(a.Key, b.Key));
        return ordered;
    }

    /// <summary>
    /// The short answer: how many of each, what is registered, and what the
    /// commonest obstacles are. Meant to fit on a console screen.
    /// </summary>
    public string Summary()
    {
        var text = new StringBuilder();
        text.Append("Server-only catalogue: ").Append(_entries.Count).Append(" name(s), stock build ")
            .Append(StockBuildId.Length == 0 ? "(no snapshot)" : StockBuildId)
            .Append(", rules ").Append(Short(PolicyFingerprint)).Append('\n');
        text.Append("  compatible ").Append(CountOf(TemplateVerdict.Compatible))
            .Append(", blocked ").Append(CountOf(TemplateVerdict.Blocked))
            .Append(", unresolved ").Append(CountOf(TemplateVerdict.Unresolved))
            .Append(", scope-excluded ").Append(CountOf(TemplateVerdict.ScopeExcluded))
            .Append(", no definition ").Append(CountOf(TemplateVerdict.MissingDefinition)).Append('\n');
        text.Append("  registered this run: ").Append(RegisteredCount).Append('\n');

        IReadOnlyList<KeyValuePair<string, int>> reasons = BlockingReasons();
        if (reasons.Count > 0)
        {
            text.Append("  by reason (templates, not findings):\n");
            foreach (KeyValuePair<string, int> reason in reasons)
                text.Append("    ").Append(reason.Value.ToString(CultureInfo.InvariantCulture).PadLeft(4))
                    .Append("  ").Append(reason.Key).Append('\n');
        }

        IReadOnlyList<string> notRegistered = ApprovedButNotRegistered();
        if (notRegistered.Count > 0)
        {
            text.Append("  approved and NOT registered (drift, or this run disagrees):\n");
            foreach (string name in notRegistered)
                text.Append("    ").Append(name).Append(" — ").Append(Find(name)!.Decision.Reason).Append('\n');
        }

        text.Append("  mwl_location <name> for one template's reasons; mwl_catalogue export for all of them.");
        return text.ToString();
    }

    /// <summary>
    /// Names the selection approves that this run did not register.
    ///
    /// Always printed in the summary even when it is empty-by-being-absent,
    /// because this is the case that would otherwise be invisible: the file says
    /// a location is in the world and the world does not have it.
    /// </summary>
    public IReadOnlyList<string> ApprovedButNotRegistered()
    {
        var names = new List<string>();
        foreach (CatalogueEntry entry in _entries)
        {
            if (entry.Decision.Outcome == SelectionOutcome.ContentDrift
                || entry.Decision.Outcome == SelectionOutcome.PolicyDrift
                || entry.Decision.Outcome == SelectionOutcome.RuntimeDisagrees)
                names.Add(entry.Name);
        }
        return names;
    }

    /// <summary>
    /// Everything known about one name: its verdict, whether it was registered,
    /// and every reason with the object path it was found at.
    /// </summary>
    public string Explain(string name)
    {
        CatalogueEntry? entry = Find(name);
        if (entry == null)
        {
            return $"'{name}' is not in the catalogue. Names are exact and case-sensitive; " +
                   "mwl_catalogue export lists every one.";
        }

        var text = new StringBuilder();
        text.Append(entry.Name).Append("  [").Append(entry.Pack.Length == 0 ? "no pack" : entry.Pack).Append("]\n");
        text.Append("  verdict: ").Append(entry.Evaluation.Verdict.ToString().ToLowerInvariant()).Append('\n');
        text.Append("  registered: ").Append(entry.Registered ? "yes" : "no").Append(" — ").Append(entry.Decision.Reason).Append('\n');
        text.Append("  content: ").Append(entry.ContentFingerprint.Length == 0 ? "(not fingerprinted)" : entry.ContentFingerprint).Append('\n');
        if (entry.Evaluation.Findings.Count == 0)
        {
            text.Append("  no findings.");
            return text.ToString();
        }

        text.Append("  findings:\n");
        foreach (TemplateFinding finding in entry.Evaluation.Findings)
        {
            text.Append("    [").Append(finding.Severity == FindingSeverity.Blocking ? "block" : "note").Append("] ")
                .Append(finding.Code);
            if (finding.Path.Length > 0)
                text.Append(" at ").Append(finding.Path);
            if (finding.Value.Length > 0)
                text.Append(" (").Append(finding.Value).Append(')');
            text.Append('\n');
            text.Append("      ").Append(finding.Detail).Append('\n');
        }
        return text.ToString().TrimEnd('\n');
    }

    /// <summary>
    /// The whole catalogue as tab-separated rows, one per finding, plus a row
    /// for every name that produced none.
    ///
    /// One row per finding rather than per template so that the object paths
    /// survive: a row saying a template is blocked by an unknown prefab, without
    /// saying which object, sends whoever reads it back to a station log — and
    /// not needing that is the whole point of the command.
    /// </summary>
    public string Export()
    {
        var text = new StringBuilder();
        text.Append("# server-only catalogue, stock build ").Append(StockBuildId)
            .Append(", rules ").Append(PolicyFingerprint).Append('\n');
        text.Append("name\tpack\tverdict\tregistered\tfingerprint\tseverity\tcode\tpath\tvalue\tdetail\n");
        foreach (CatalogueEntry entry in _entries)
        {
            string prefix = entry.Name + "\t" + entry.Pack + "\t"
                + entry.Evaluation.Verdict.ToString().ToLowerInvariant() + "\t"
                + (entry.Registered ? "yes" : "no") + "\t" + entry.ContentFingerprint + "\t";
            if (entry.Evaluation.Findings.Count == 0)
            {
                text.Append(prefix).Append("\t\t\t\t").Append(Clean(entry.Decision.Reason)).Append('\n');
                continue;
            }
            foreach (TemplateFinding finding in entry.Evaluation.Findings)
            {
                text.Append(prefix)
                    .Append(finding.Severity == FindingSeverity.Blocking ? "block" : "note").Append('\t')
                    .Append(finding.Code).Append('\t')
                    .Append(Clean(finding.Path)).Append('\t')
                    .Append(Clean(finding.Value)).Append('\t')
                    .Append(Clean(finding.Detail)).Append('\n');
            }
        }
        return text.ToString();
    }

    /// <summary>
    /// The selection an audit run would ship, from what it just evaluated.
    ///
    /// Only <see cref="TemplateVerdict.Compatible"/> names, each bound to the
    /// fingerprint of the template that was judged. This is the generator: the
    /// approved set is produced from the evaluation rather than transcribed from
    /// it by hand.
    /// </summary>
    public string RenderSelection(string generatedFrom)
    {
        var approved = new List<KeyValuePair<string, string>>();
        foreach (CatalogueEntry entry in _entries)
        {
            if (entry.Evaluation.Verdict == TemplateVerdict.Compatible)
                approved.Add(new KeyValuePair<string, string>(entry.Name, entry.ContentFingerprint));
        }
        return ApprovedSelection.Render(approved, PolicyFingerprint, generatedFrom);
    }

    /// <summary>Tabs and newlines out of a field, so one row stays one row.</summary>
    private static string Clean(string value) =>
        value == null ? "" : value.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');

    private static string Short(string fingerprint) =>
        string.IsNullOrEmpty(fingerprint) ? "(none)"
        : fingerprint.Length <= 8 ? fingerprint : fingerprint.Substring(0, 8);
}
