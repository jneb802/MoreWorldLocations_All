using System;
using System.Collections.Generic;
using System.Text;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>Why a template was or was not registered, once the selection had its say.</summary>
public enum SelectionOutcome
{
    /// <summary>In the selection, unchanged since it was approved, and the runtime agrees.</summary>
    Registered,

    /// <summary>Not selected by the current validator/subset, or absent from an offline snapshot.</summary>
    NotSelected,

    /// <summary>Approved, but the template is not the one that was approved.</summary>
    ContentDrift,

    /// <summary>Approved under different rules from the ones this build applies.</summary>
    PolicyDrift,

    /// <summary>Approved, unchanged, and this run's own evaluation of it fails.</summary>
    RuntimeDisagrees,
}

/// <summary>One name's fate, with the reason attached rather than inferred.</summary>
public sealed class SelectionDecision
{
    public SelectionDecision(string name, SelectionOutcome outcome, string reason)
    {
        Name = name ?? "";
        Outcome = outcome;
        Reason = reason ?? "";
    }

    public string Name { get; }
    public SelectionOutcome Outcome { get; }
    public string Reason { get; }
    public bool Registered => Outcome == SelectionOutcome.Registered;
}

/// <summary>
/// An exported audit snapshot for evidence and offline comparison. Runtime
/// registration does not read it; every startup validates resolved templates,
/// or reuses the verdicts of a previous startup whose inputs were provably
/// identical (<see cref="AuditCache"/>).
/// </summary>
public sealed class ApprovedSelection
{
    private readonly Dictionary<string, string> _fingerprintByName;

    private ApprovedSelection(string policyFingerprint, string generatedFrom, Dictionary<string, string> fingerprintByName)
    {
        PolicyFingerprint = policyFingerprint;
        GeneratedFrom = generatedFrom;
        _fingerprintByName = fingerprintByName;
    }

    /// <summary>The fingerprint of the rules that granted every approval in this file.</summary>
    public string PolicyFingerprint { get; }

    /// <summary>What produced this file — the audit run's identity, for the report.</summary>
    public string GeneratedFrom { get; }

    /// <summary>The names captured in this audit snapshot.</summary>
    public IReadOnlyCollection<string> Names => _fingerprintByName.Keys;

    public int Count => _fingerprintByName.Count;

    /// <summary>An empty audit snapshot.</summary>
    public static ApprovedSelection Empty { get; } =
        new ApprovedSelection("", "none", new Dictionary<string, string>(StringComparer.Ordinal));

    /// <summary>The fingerprint this name was approved with, or null when it was never approved.</summary>
    public string? FingerprintOf(string name) =>
        name != null && _fingerprintByName.TryGetValue(name, out string fingerprint) ? fingerprint : null;

    /// <summary>
    /// Compare a recorded approval against newer content and rules, offline.
    /// Runtime registration does not consult this snapshot.
    ///
    /// Three different things have to agree: the file says the name was
    /// approved, the template is still the one that was approved, and this run's
    /// own evaluation of it passes. The third is not redundant — it is what
    /// makes the shipped file a record of an audit rather than a licence to skip
    /// one.
    /// </summary>
    public SelectionDecision Decide(string name, TemplateEvaluation evaluation, string contentFingerprint, string policyFingerprint)
    {
        if (evaluation == null) throw new ArgumentNullException(nameof(evaluation));

        string? approvedWith = FingerprintOf(name);
        if (approvedWith == null)
        {
            return new SelectionDecision(name, SelectionOutcome.NotSelected,
                "not in the approved selection. The catalogue report carries the reason it was never approved.");
        }

        if (!string.Equals(PolicyFingerprint, policyFingerprint, StringComparison.Ordinal))
        {
            return new SelectionDecision(name, SelectionOutcome.PolicyDrift,
                $"approved under rules {Short(PolicyFingerprint)} and this build applies {Short(policyFingerprint)}. " +
                "The approval was granted by rules that are no longer the ones being applied, so it is not carried over.");
        }

        if (!string.Equals(approvedWith, contentFingerprint, StringComparison.Ordinal))
        {
            return new SelectionDecision(name, SelectionOutcome.ContentDrift,
                $"approved as {Short(approvedWith)} and the resolved template is now {Short(contentFingerprint)}. " +
                "The content changed since it was audited, so the approval does not describe what is in this build.");
        }

        if (!evaluation.Approved)
        {
            return new SelectionDecision(name, SelectionOutcome.RuntimeDisagrees,
                $"approved in the selection and this run evaluates it as {evaluation.Verdict.ToString().ToLowerInvariant()}: " +
                string.Join(", ", new List<string>(evaluation.Codes).ToArray()) +
                ". The shipped file records an audit; it does not replace one.");
        }

        return new SelectionDecision(name, SelectionOutcome.Registered, "approved, unchanged, and re-checked this run.");
    }

    /// <summary>
    /// Parse an exported audit snapshot.
    ///
    /// <c>#policy</c> and <c>#generated</c> headers, then <c>name TAB
    /// fingerprint</c>. Flat text for the same reason the prefab snapshot is:
    /// a diff between two releases has to be readable by a person deciding
    /// whether a location appeared or disappeared on purpose.
    /// </summary>
    /// <exception cref="FormatException">A malformed line. A selection that quietly dropped one would register a different world from the one the file describes.</exception>
    public static ApprovedSelection Parse(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));

        var byName = new Dictionary<string, string>(StringComparer.Ordinal);
        string policy = "";
        string generated = "";
        int lineNumber = 0;

        foreach (string raw in text.Split('\n'))
        {
            lineNumber++;
            string line = raw.Trim('\r', ' ', '\t');
            if (line.Length == 0)
                continue;
            if (line[0] == '#')
            {
                if (line.StartsWith("#policy", StringComparison.Ordinal))
                    policy = line.Substring("#policy".Length).Trim();
                else if (line.StartsWith("#generated", StringComparison.Ordinal))
                    generated = line.Substring("#generated".Length).Trim();
                continue;
            }

            int tab = line.IndexOf('\t');
            if (tab <= 0)
                throw new FormatException($"approved selection line {lineNumber}: expected 'name<TAB>fingerprint', got '{line}'");

            string name = line.Substring(0, tab);
            string fingerprint = line.Substring(tab + 1).Trim();
            if (fingerprint == "*")
                throw new FormatException($"approved selection line {lineNumber}: wildcard approvals are not supported");
            if (fingerprint.Length == 0)
                throw new FormatException($"approved selection line {lineNumber}: '{name}' has no fingerprint");
            if (byName.ContainsKey(name))
                throw new FormatException($"approved selection line {lineNumber}: '{name}' appears twice");

            byName[name] = fingerprint;
        }

        return new ApprovedSelection(policy, generated, byName);
    }

    /// <summary>
    /// Export an audit snapshot for evidence. This is not a registration input.
    ///
    /// Names are sorted, so that two runs over the same catalogue produce byte
    /// identical files and a diff means the selection actually changed.
    /// </summary>
    public static string Render(IEnumerable<KeyValuePair<string, string>> approved, string policyFingerprint, string generatedFrom)
    {
        if (approved == null) throw new ArgumentNullException(nameof(approved));

        var names = new List<KeyValuePair<string, string>>(approved);
        names.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));

        var text = new StringBuilder();
        text.Append("# Snapshot exported by the server-only catalogue audit.\n");
        text.Append("# Evidence only; registration uses the current resolved-template validator.\n");
        text.Append("#policy ").Append(policyFingerprint ?? "").Append('\n');
        text.Append("#generated ").Append(generatedFrom ?? "").Append('\n');
        foreach (KeyValuePair<string, string> entry in names)
            text.Append(entry.Key).Append('\t').Append(entry.Value).Append('\n');
        return text.ToString();
    }

    private static string Short(string fingerprint) =>
        string.IsNullOrEmpty(fingerprint) ? "(none)"
        : fingerprint.Length <= 8 ? fingerprint
        : fingerprint.Substring(0, 8);
}
