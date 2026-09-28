using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// Reuse the catalogue audit's verdicts across starts whose inputs have not
/// changed, and prove that they have not.
///
/// <para><b>Why.</b> The registration sweep opens every template in the
/// catalogue, one a frame with preload and settle waits, before the world may
/// load: three to five minutes on a dedicated server, every start, to reach the
/// verdicts the previous start reached over the same files. A restart to change
/// an unrelated setting paid for all of it.</para>
///
/// <para><b>What makes that acceptable.</b> The objection to a shipped approval
/// list was that it approved templates nobody inspected. A stored verdict is
/// not approved by anybody: it is the audit's own output, and it is used only
/// when every check says the audit would reach it again —
/// <see cref="AuditCacheKey"/> over every input all verdicts share, the
/// template's own input (<see cref="AuditCacheCanonical.TemplateInput"/>: its
/// definition, its manifest entry, its bundle), and <see cref="StockRecord"/>
/// over every live stock prefab that template's verdict was reached against,
/// re-signed from the running game. Any doubt, including not being able to
/// ask, is a miss, and a miss is exactly the audit that ran before this
/// existed.</para>
///
/// <para><b>One template at a time.</b> A change to the shared key re-audits
/// everything. A change to one template's input, or to a stock prefab one
/// template consulted, re-audits that template alone: an MWL update that
/// changes four bundles opens four templates, and the rest keep their verdicts.
/// The report is rebuilt in the run's order from the reused and the fresh
/// verdicts, and registration cannot tell the difference.</para>
///
/// <para><b>What never uses it.</b> A diagnostic resweep, or a resweep retrying
/// a failed registration: somebody asked for the templates to be looked at
/// again, and answering from a file would not be looking. The first start after
/// any change is also the audit of what changed, by construction.</para>
///
/// <para>The engine-facing half — finding MWL's folder, the game, the loader,
/// reading a live prefab — is <c>AuditCacheInstallation</c>, installed through
/// the seams here. With none installed (a test that does not set them, or a
/// start where installing failed) the cache is simply off.</para>
/// </summary>
public static class AuditCache
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    /// <summary>
    /// What only the engine can compute, given the names the run judges: the
    /// key parts <c>mwl-code</c>, <c>mwl-shared</c>, <c>mwl-config</c>,
    /// <c>game</c> and <c>loader</c> as canonical text, and each template's own
    /// files (<see cref="AuditCacheCanonical.MwlFiles"/>). The rest are computed
    /// here, from the run itself, so the key binds exactly what the run was
    /// constructed with.
    /// </summary>
    public static Func<IReadOnlyCollection<string>, InstalledInputs>? Installation { get; set; }

    /// <summary>
    /// A stock prefab's live signature by name — <see cref="SignStock"/> over
    /// the audit's own baseline lookup in a real start. Null when nothing of
    /// that name exists; <see cref="StockRecord.NotStockSignature"/> when what
    /// exists is not a stock prefab. Only ever asked about names in the stock
    /// snapshot. The same text at audit time and at the next start means the
    /// prefab is the one the verdicts were reached against.
    /// </summary>
    public static Func<string, string?>? LiveStock { get; set; }

    /// <summary>
    /// The live signature of the stock prefab the comparison would use for
    /// <paramref name="name"/>, found through <paramref name="baseline"/> — the
    /// same lookup the audit compares against.
    ///
    /// <para>An object with a parent is never signed. A stock prefab is a root:
    /// what the game registers and a client instantiates. Something parented is
    /// a piece of something else — a snap point on a piece, an object inside a
    /// template the audit has open — and a name lookup finds it only while
    /// whatever holds it happens to be loaded. Signing it made the baseline move
    /// with the audit's own loading and unloading, so no store could ever
    /// succeed. It is recorded as not a stock prefab instead, which does not
    /// change as things load.</para>
    /// </summary>
    public static string? SignStock(string name, Func<string, GameObject?>? baseline)
    {
        GameObject? prefab = baseline?.Invoke(name);
        if (prefab == null)
            return null;
        if (prefab.transform.parent != null)
            return StockRecord.NotStockSignature;
        return TemplateFactsExtractor.LiveSignature(prefab);
    }

    /// <summary>Where the file lives when the environment does not say.</summary>
    public static Func<string>? DefaultPath { get; set; }

    /// <summary>
    /// The record the running audit is adding to, for the engine's mock
    /// resolution hook. Null whenever no storing audit is in progress.
    /// </summary>
    public static StockRecord? Recording { get; internal set; }

    /// <summary>One start's use of the cache: where, under which key, what it may reuse, and the record to store with.</summary>
    public sealed class Attempt
    {
        internal Attempt(string path, AuditCacheKey? key, CatalogueReport? reused, StockRecord? record,
            IReadOnlyDictionary<string, StoredVerdict>? reusable = null, IReadOnlyDictionary<string, string>? inputs = null)
        {
            Path = path;
            Key = key;
            Reused = reused;
            Record = record;
            Reusable = reusable ?? new Dictionary<string, StoredVerdict>(StringComparer.Ordinal);
            Inputs = inputs ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public string Path { get; }

        /// <summary>Null when the key could not be computed; nothing is stored then.</summary>
        public AuditCacheKey? Key { get; }

        /// <summary>The whole report, when every name the run judges had a verdict to reuse; nothing is opened then.</summary>
        public CatalogueReport? Reused { get; }

        /// <summary>The stored verdicts this run may reuse, by name; the rest are audited.</summary>
        public IReadOnlyDictionary<string, StoredVerdict> Reusable { get; }

        /// <summary>Each name's template input digest in this run, stored beside its verdict.</summary>
        public IReadOnlyDictionary<string, string> Inputs { get; }

        /// <summary>What a fresh audit records, to be stored with its verdicts. Null when nothing may be stored.</summary>
        public StockRecord? Record { get; }
    }

    /// <summary>
    /// Whether this process could use the cache at all: the engine half is
    /// installed and there is somewhere to keep the file. When it could not,
    /// the sweep is exactly the sweep that ran before the cache existed, frame
    /// for frame.
    /// </summary>
    public static bool Installed =>
        Installation != null && LiveStock != null && CachePath() != null;

    /// <summary>
    /// Look for reusable verdicts for <paramref name="run"/>. Null when the cache
    /// is off or unavailable in this process. Logs how long the key took, and
    /// one line saying which way it went and why.
    /// </summary>
    public static Attempt? Try(CatalogueAudit.CatalogueAuditRun run)
    {
        if (run == null) throw new ArgumentNullException(nameof(run));

        if (ValidationSwitches.AuditCacheOff(out bool recognised))
        {
            Say(recognised
                ? $"catalogue audit: the verdict cache is off ({ValidationSwitches.AuditCacheVariable}); auditing every template"
                : $"catalogue audit: {ValidationSwitches.AuditCacheVariable} has a value this build does not know, so the verdict cache is off; auditing every template");
            return null;
        }

        string? path = CachePath();
        Func<IReadOnlyCollection<string>, InstalledInputs>? installation = Installation;
        Func<string, string?>? liveStock = LiveStock;
        if (path == null || installation == null || liveStock == null)
            return null;

        AuditCacheKey key;
        IReadOnlyDictionary<string, string> inputs;
        Stopwatch clock = Stopwatch.StartNew();
        try
        {
            key = ComputeKey(run, installation, out inputs);
        }
        catch (Exception ex)
        {
            Warn($"catalogue audit: the verdict cache key could not be computed ({ex.GetType().Name}: {ex.Message}); " +
                 "auditing every template, and nothing will be stored");
            return new Attempt(path, null, null, null);
        }
        clock.Stop();
        Say($"catalogue audit: verdict cache key {key.Short} computed in {clock.Elapsed.TotalMilliseconds:0} ms");

        clock.Restart();
        AuditCacheReuse reuse = Select(AuditCacheFile.Load(path, key), run, inputs, liveStock);
        clock.Stop();

        if (reuse.Reusable.Count == 0)
        {
            Say($"catalogue audit: not reusing stored verdicts ({reuse.Reason}); auditing every template");
            return new Attempt(path, key, null, new StockRecord(liveStock, run.Registry), null, inputs);
        }

        if (reuse.Stale.Count == 0)
        {
            List<CatalogueEntry> entries = new List<CatalogueEntry>(run.Subjects.Count);
            foreach (CatalogueSubject subject in run.Subjects)
                entries.Add(reuse.Reusable[subject.Name].Entry);
            CatalogueReport report = new CatalogueReport(entries, run.StockBuildId, run.PolicyFingerprint);
            Say($"catalogue audit: reused {report.Entries.Count} verdicts from {path} (key {key.Short}); " +
                $"the audit was skipped because every input matched, {reuse.StockChecked} stock prefab(s) included " +
                $"(checked in {clock.Elapsed.TotalMilliseconds:0} ms)");
            return new Attempt(path, key, report, null, reuse.Reusable, inputs);
        }

        // Some verdicts stand and some do not: the audit judges the rest, and
        // the record starts with what the reused ones were reached against, so
        // the file written afterwards still checks every one of them.
        StockRecord record = new StockRecord(liveStock, run.Registry);
        foreach (StoredVerdict verdict in reuse.Reusable.Values)
            record.Adopt(verdict.Name, verdict.Uses);
        Say($"catalogue audit: reusing {reuse.Reusable.Count} stored verdict(s) from {path} (key {key.Short}); " +
            $"auditing {reuse.Stale.Count} template(s): {Names(reuse.StaleDescriptions())} " +
            $"(checked in {clock.Elapsed.TotalMilliseconds:0} ms)");
        return new Attempt(path, key, null, record, reuse.Reusable, inputs);
    }

    /// <summary>
    /// Which stored verdicts this run may reuse. Everything the key cannot see:
    /// that the stored verdicts were reached under this run's rules and stock
    /// snapshot, that each name's own input is what it was, and that every stock
    /// prefab a verdict was reached against is still the same prefab. A name
    /// fails alone; only a file-level miss or other rules fail every name.
    /// </summary>
    public static AuditCacheReuse Select(AuditCacheLookup lookup, CatalogueAudit.CatalogueAuditRun run,
        IReadOnlyDictionary<string, string> inputs, Func<string, string?> liveStock)
    {
        if (lookup == null) throw new ArgumentNullException(nameof(lookup));
        if (run == null) throw new ArgumentNullException(nameof(run));
        if (inputs == null) throw new ArgumentNullException(nameof(inputs));
        if (liveStock == null) throw new ArgumentNullException(nameof(liveStock));
        if (!lookup.Hit)
            return AuditCacheReuse.None(lookup.Miss, lookup.Reason, lookup.Changed);

        AuditCacheContents contents = lookup.Contents!;
        if (!string.Equals(contents.PolicyFingerprint, run.PolicyFingerprint, StringComparison.Ordinal)
            || !string.Equals(contents.StockBuildId, run.StockBuildId, StringComparison.Ordinal))
        {
            return AuditCacheReuse.None(AuditCacheMiss.Subjects,
                "the stored verdicts were reached under other rules or another stock snapshot", Array.Empty<string>());
        }

        // Every stored stock prefab, signed once now.
        Dictionary<string, string> moved = new Dictionary<string, string>(StringComparer.Ordinal);
        List<string> changed = new List<string>();
        List<string> vanished = new List<string>();
        List<string> appeared = new List<string>();
        foreach (KeyValuePair<string, string> prefab in contents.Stock)
        {
            string now = StockRecord.DigestOf(liveStock, run.Registry, prefab.Key, out string? fault);
            if (fault != null)
            {
                moved[prefab.Key] = $"stock prefab '{prefab.Key}' could not be read: {fault}";
                changed.Add(prefab.Key);
                continue;
            }
            if (string.Equals(now, prefab.Value, StringComparison.Ordinal))
                continue;
            if (now == StockRecord.Absent)
            {
                vanished.Add(prefab.Key);
                moved[prefab.Key] = $"'{prefab.Key}' no longer resolves";
            }
            else if (prefab.Value == StockRecord.Absent)
            {
                appeared.Add(prefab.Key);
                moved[prefab.Key] = $"'{prefab.Key}' now resolves";
            }
            else
            {
                changed.Add(prefab.Key);
                moved[prefab.Key] = $"stock prefab '{prefab.Key}' changed";
            }
        }

        Dictionary<string, StoredVerdict> stored = new Dictionary<string, StoredVerdict>(StringComparer.Ordinal);
        foreach (StoredVerdict verdict in contents.Verdicts)
            stored[verdict.Name] = verdict;

        Dictionary<string, StoredVerdict> reusable = new Dictionary<string, StoredVerdict>(StringComparer.Ordinal);
        List<KeyValuePair<string, string>> stale = new List<KeyValuePair<string, string>>();
        foreach (CatalogueSubject subject in run.Subjects)
        {
            if (!stored.TryGetValue(subject.Name, out StoredVerdict? verdict))
            {
                stale.Add(new KeyValuePair<string, string>(subject.Name, "no stored verdict"));
                continue;
            }
            if (!inputs.TryGetValue(subject.Name, out string? input) || !string.Equals(input, verdict.Input, StringComparison.Ordinal))
            {
                stale.Add(new KeyValuePair<string, string>(subject.Name, "its definition, manifest entry or bundle changed"));
                continue;
            }
            List<string> why = new List<string>();
            foreach (string use in verdict.Uses)
            {
                if (moved.TryGetValue(use, out string? reason))
                    why.Add(reason);
            }
            if (why.Count > 0)
            {
                stale.Add(new KeyValuePair<string, string>(subject.Name, string.Join("; ", why.ToArray())));
                continue;
            }
            reusable[subject.Name] = verdict;
        }

        List<string> all = new List<string>();
        all.AddRange(changed);
        all.AddRange(vanished);
        all.AddRange(appeared);
        List<string> parts = new List<string>();
        if (changed.Count > 0) parts.Add("stock prefabs changed: " + Names(changed));
        if (vanished.Count > 0) parts.Add("no longer resolve: " + Names(vanished));
        if (appeared.Count > 0) parts.Add("now resolve: " + Names(appeared));
        string summary = parts.Count > 0 ? string.Join("; ", parts.ToArray())
            : stale.Count > 0 ? $"{stale.Count} template(s) have no reusable verdict" : "";
        return new AuditCacheReuse(reusable, stale, all, summary, contents.Stock.Count);
    }

    /// <summary>
    /// The shared key for a run, from the parts computed from the run and the
    /// process and the installation's, and each name's template input.
    /// </summary>
    /// <exception cref="ArgumentException">The installation left a part out or supplied one twice.</exception>
    public static AuditCacheKey ComputeKey(CatalogueAudit.CatalogueAuditRun run,
        Func<IReadOnlyCollection<string>, InstalledInputs> installation, out IReadOnlyDictionary<string, string> inputs)
    {
        if (run == null) throw new ArgumentNullException(nameof(run));
        if (installation == null) throw new ArgumentNullException(nameof(installation));

        List<string> names = new List<string>(run.Subjects.Count);
        foreach (CatalogueSubject subject in run.Subjects)
            names.Add(subject.Name);
        InstalledInputs installed = installation(names) ?? throw new ArgumentException("the installation supplied nothing");

        Dictionary<string, string> parts = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["format"] = AuditCacheCanonical.Format(AuditCacheFile.FormatVersion),
            ["policy"] = AuditCacheCanonical.Policy(run.PolicyFingerprint, run.Only),
            ["provenance"] = AuditCacheCanonical.Provenance(TemplateAssets.BaselineProvenance),
            ["env"] = AuditCacheCanonical.Environment(Environment.GetEnvironmentVariables()),
        };
        foreach (KeyValuePair<string, string> part in installed.Parts)
        {
            // Supplying a part computed here would let an installation replace
            // the run's own policy or names with something else.
            if (parts.ContainsKey(part.Key))
                throw new ArgumentException($"the installation supplied '{part.Key}', which is computed from the run");
            parts[part.Key] = part.Value;
        }

        Dictionary<string, string> byName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (CatalogueSubject subject in run.Subjects)
        {
            installed.Templates.TryGetValue(subject.Name, out string? files);
            byName[subject.Name] = AuditCacheCanonical.TemplateInput(subject, files);
        }
        inputs = byName;
        return AuditCacheKey.FromCanonical(parts);
    }

    /// <summary>
    /// Decide which of a finished audit's verdicts may be stored, and render
    /// them. Called at the end of the sweep, before registration, so the
    /// baseline is re-signed in the state the verdicts were reached in. Null,
    /// with the reason logged, when none may. Never throws: the worst a failure
    /// here does is make the next start audit again.
    /// </summary>
    public static string? Prepare(Attempt attempt, CatalogueReport report)
    {
        try
        {
            return PrepareCore(attempt, report);
        }
        catch (Exception ex)
        {
            Warn($"catalogue audit: the verdicts were not stored ({ex.GetType().Name}: {ex.Message}); the next start audits again");
            return null;
        }
    }

    private static string? PrepareCore(Attempt attempt, CatalogueReport report)
    {
        if (attempt?.Key == null || attempt.Record == null || report == null)
            return null;
        StockRecord record = attempt.Record;

        if (record.Fault != null)
        {
            Warn($"catalogue audit: verdicts not stored: a stock prefab could not be signed ({record.Fault}), " +
                 "so the next start could not check the baseline they were reached against");
            return null;
        }

        // A catalogue whose templates consulted no stock prefab at all is not
        // one this mod ships, and a file with an empty baseline would pass the
        // stock check by having nothing to check.
        if (record.TemplatesRead > 0 && record.Entries.Count == 0)
        {
            Warn("catalogue audit: verdicts not stored: templates were read and no stock prefab was recorded, " +
                 "which means the recording did not run");
            return null;
        }

        // The baseline has to be the same at the end as when each verdict was
        // reached. If the sweep itself moved a stock prefab, no single state of
        // the game is the one the verdicts describe.
        IReadOnlyList<string> moved = record.Recheck();
        if (record.Fault != null || moved.Count > 0)
        {
            Warn("catalogue audit: verdicts not stored: " + (record.Fault != null
                ? $"a stock prefab could not be re-signed ({record.Fault})"
                : "stock prefabs changed while the audit ran: " + Names(moved)));
            return null;
        }

        // An unresolved verdict is a gap in what THIS run could see — a
        // template that would not load, a walk that stopped — and the next
        // start may see further. It is left out, so that template alone is
        // judged again; storing it would make a passing failure permanent.
        List<StoredVerdict> verdicts = new List<StoredVerdict>();
        List<string> unresolved = new List<string>();
        HashSet<string> used = new HashSet<string>(StringComparer.Ordinal);
        foreach (CatalogueEntry entry in report.Entries)
        {
            if (entry.Evaluation.Verdict == TemplateVerdict.Unresolved)
            {
                unresolved.Add(entry.Name);
                continue;
            }
            if (!attempt.Inputs.TryGetValue(entry.Name, out string? input))
            {
                // A name the key was not computed for has no input to store.
                unresolved.Add(entry.Name);
                continue;
            }
            IReadOnlyList<string> uses = record.UsesOf(entry.Name);
            foreach (string use in uses)
                used.Add(use);
            verdicts.Add(new StoredVerdict(entry, input, uses));
        }
        if (unresolved.Count > 0)
        {
            Say($"catalogue audit: {unresolved.Count} verdict(s) not stored because they are unresolved, a gap in what " +
                $"this run could see; the next start judges them again: {Names(unresolved)}");
        }
        if (verdicts.Count == 0)
            return null;

        List<KeyValuePair<string, string>> stock = new List<KeyValuePair<string, string>>();
        foreach (KeyValuePair<string, string> entry in record.Entries)
        {
            if (used.Contains(entry.Key))
                stock.Add(entry);
        }
        return AuditCacheFile.Render(attempt.Key, report.StockBuildId, report.PolicyFingerprint, verdicts, stock);
    }

    /// <summary>
    /// Write what <see cref="Prepare"/> rendered, once the report has registered.
    /// Never throws and never changes the running server.
    /// </summary>
    public static void Write(Attempt attempt, CatalogueReport report, string text)
    {
        try
        {
            string? error = AuditCacheFile.Save(attempt.Path, text);
            if (error != null)
            {
                Warn($"catalogue audit: the verdicts could not be stored in {attempt.Path}: {error}. " +
                     "This start is unaffected; the next one audits again.");
                return;
            }
            Say($"catalogue audit: stored the verdicts of {report.Entries.Count} name(s) " +
                $"({attempt.Reusable.Count} carried over, {report.Entries.Count - attempt.Reusable.Count} audited now) and {attempt.Record?.Entries.Count ?? 0} " +
                $"stock prefab signature(s) in {attempt.Path} (key {attempt.Key?.Short}) for the next start");
        }
        catch (Exception ex)
        {
            Warn($"catalogue audit: the verdicts were not stored ({ex.GetType().Name}: {ex.Message}); this start is unaffected");
        }
    }

    /// <summary>The file this start reads and writes, or null when there is nowhere to keep one.</summary>
    public static string? CachePath()
    {
        string? path = ValidationSwitches.AuditCachePath();
        if (path != null)
            return path;
        try
        {
            return DefaultPath?.Invoke();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// The asset a mock name asks for, the way Jötunn reads it: one
    /// <c>JVLmock_</c> prefix off, then everything before a <c>__</c> child
    /// path; or one <c>VLmock_</c> prefix off. A doubly-prefixed name asks for a
    /// name that still carries the prefix, and that is recorded as asked.
    /// </summary>
    public static bool TryMockAsset(string name, out string assetName, out bool childPath)
    {
        assetName = "";
        childPath = false;
        if (name == null)
            return false;
        string trimmed = name.Trim();
        if (trimmed.StartsWith(TemplatePolicy.MockPrefix, StringComparison.Ordinal))
        {
            string rest = trimmed.Substring(TemplatePolicy.MockPrefix.Length);
            string[] parts = rest.Split(new[] { "__" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return false;
            assetName = parts[0];
            childPath = parts.Length > 1;
            return true;
        }
        if (trimmed.StartsWith("VLmock_", StringComparison.Ordinal))
        {
            assetName = trimmed.Substring("VLmock_".Length);
            return assetName.Length > 0;
        }
        return false;
    }

    /// <summary>Every name, never a count of the rest: the list is what somebody diagnosing a miss needs.</summary>
    internal static string Names(IReadOnlyList<string> names)
    {
        string[] all = new string[names.Count];
        for (int i = 0; i < names.Count; i++)
            all[i] = names[i];
        return string.Join(", ", all);
    }

    private static void Say(string line)
    {
        try
        {
            Log.LogInfo(line);
        }
        catch
        {
            // A log sink is not allowed to decide whether the audit runs.
        }
    }

    private static void Warn(string line)
    {
        try
        {
            Log.LogWarning(line);
        }
        catch
        {
            // As above.
        }
    }
}

/// <summary>Which stored verdicts a run may reuse, and why each of the others may not.</summary>
public sealed class AuditCacheReuse
{
    internal AuditCacheReuse(IReadOnlyDictionary<string, StoredVerdict> reusable, IReadOnlyList<KeyValuePair<string, string>> stale,
        IReadOnlyList<string> changedStock, string reason, int stockChecked, AuditCacheMiss miss = AuditCacheMiss.None)
    {
        Reusable = reusable;
        Stale = stale;
        ChangedStock = changedStock;
        Reason = reason;
        StockChecked = stockChecked;
        Miss = miss;
    }

    internal static AuditCacheReuse None(AuditCacheMiss miss, string reason, IReadOnlyList<string> changed) =>
        new AuditCacheReuse(new Dictionary<string, StoredVerdict>(StringComparer.Ordinal),
            Array.Empty<KeyValuePair<string, string>>(), changed, reason, 0, miss);

    /// <summary>The verdicts that stand, by name.</summary>
    public IReadOnlyDictionary<string, StoredVerdict> Reusable { get; }

    /// <summary>Each name that is audited again, in the run's order, with why.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Stale { get; }

    /// <summary>Stored stock prefabs whose live signature is no longer what was stored.</summary>
    public IReadOnlyList<string> ChangedStock { get; }

    /// <summary>Why nothing is reused (a file-level miss), or what changed.</summary>
    public string Reason { get; }

    /// <summary>For a file-level miss, which kind; <see cref="AuditCacheMiss.None"/> otherwise.</summary>
    public AuditCacheMiss Miss { get; }

    public int StockChecked { get; }

    /// <summary>"name (why)" for every stale name.</summary>
    public IReadOnlyList<string> StaleDescriptions()
    {
        List<string> all = new List<string>(Stale.Count);
        foreach (KeyValuePair<string, string> name in Stale)
            all.Add($"{name.Key} ({name.Value})");
        return all;
    }
}

/// <summary>
/// The live stock prefabs an audit's verdicts were reached against, each with a
/// digest of its signature at the time.
///
/// <para><b>Why names and not "the plugins".</b> A verdict reads the running
/// game through the stock prefab each emitted object is compared with (and
/// whose scale behaviour decides the scale rule), and through the real prefabs
/// Jötunn copies into a template when it resolves the template's mocks. Both
/// are found by name. Any plugin can change either — edit a vanilla prefab in
/// place, or register one under a name a template asks for — and recording the
/// names and their content catches that whichever plugin did it, while leaving
/// every plugin that touches nothing a template reads free to update without a
/// re-audit.</para>
///
/// <para><b>Only stock prefabs are signed.</b> A name is signed only when it is
/// in the embedded stock snapshot — the names a client without the mod can
/// build — and only when what the baseline lookup finds for it is a root
/// object (<see cref="AuditCache.SignStock"/>). Every other name the audit
/// asked about is recorded as <see cref="NotStock"/>, whatever it resolved to
/// at the moment: the lookup falls back to any loaded object of that name, so
/// what it finds for a name the game does not register depends on which
/// templates and bundles happen to be loaded, and a digest of that would change
/// with the audit's own loading. The snapshot is part of MWL's DLL, so which
/// names are signed cannot change without the key changing too.</para>
///
/// <para><b>What is recorded.</b> Every name the audit's stock lookup is asked
/// for (the comparison and the scale conversion), every asset Jötunn's mock
/// resolution asks for, every mock name a resolved template still carries,
/// every spawn and drop reference, and the interior prefab. Not the names that
/// objects in a template merely bear: a template's own children, snap points
/// and all, are the template's content, which the key covers.</para>
/// </summary>
public sealed class StockRecord
{
    /// <summary>The digest recorded for a stock name that resolved to nothing.</summary>
    public const string Absent = "-";

    /// <summary>The digest recorded for a name that is not a stock prefab, whether or not anything by that name is loaded.</summary>
    public const string NotStock = "not-stock";

    /// <summary>What a signer returns for a stock name that resolved to something that is not a stock prefab.</summary>
    public const string NotStockSignature = "\u0000not a stock prefab";

    private readonly Func<string, string?> _sign;
    private readonly StockPrefabRegistry _registry;
    private readonly Dictionary<string, string> _recorded = new Dictionary<string, string>(StringComparer.Ordinal);

    // Which template asked for which names. A name asked for while no template
    // is current (none in a sweep today) counts for every template: nobody can
    // say whose verdict it reached.
    private readonly Dictionary<string, SortedSet<string>> _uses = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
    private readonly SortedSet<string> _shared = new SortedSet<string>(StringComparer.Ordinal);
    private string? _current;

    public StockRecord(Func<string, string?> sign, StockPrefabRegistry registry)
    {
        _sign = sign ?? throw new ArgumentNullException(nameof(sign));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>The first signing failure. Once set nothing more is recorded and nothing is stored.</summary>
    public string? Fault { get; private set; }

    /// <summary>How many templates were actually read, so an empty record can be told from a broken one.</summary>
    public int TemplatesRead { get; private set; }

    /// <summary>
    /// The template whose reading the names asked for from now on belong to.
    /// The sweep sets it before it preloads a template, so a mock Jötunn
    /// resolves while the bundle loads is that template's too.
    /// </summary>
    public void Begin(string template) => _current = string.IsNullOrEmpty(template) ? null : template;

    /// <summary>A name the audit asked the live game for. Recorded resolved or not, and noted for the current template.</summary>
    public void Consulted(string name)
    {
        if (Fault != null || string.IsNullOrEmpty(name))
            return;
        if (!_recorded.ContainsKey(name))
        {
            string digest = DigestOf(_sign, _registry, name, out string? fault);
            if (fault != null)
            {
                Fault = fault;
                return;
            }
            _recorded[name] = digest;
        }
        if (_current == null)
        {
            _shared.Add(name);
            return;
        }
        if (!_uses.TryGetValue(_current, out SortedSet<string>? uses))
            _uses[_current] = uses = new SortedSet<string>(StringComparer.Ordinal);
        uses.Add(name);
    }

    /// <summary>
    /// A reused verdict's names, signed now, so the file written after a
    /// partial audit checks them again on the next start.
    /// </summary>
    public void Adopt(string template, IEnumerable<string> uses)
    {
        string? was = _current;
        _current = template;
        try
        {
            foreach (string use in uses)
                Consulted(use);
        }
        finally
        {
            _current = was;
        }
    }

    /// <summary>The names <paramref name="template"/>'s verdict was reached against, and every name nobody's in particular, sorted.</summary>
    public IReadOnlyList<string> UsesOf(string template)
    {
        SortedSet<string> all = new SortedSet<string>(_shared, StringComparer.Ordinal);
        if (_uses.TryGetValue(template, out SortedSet<string>? uses))
            all.UnionWith(uses);
        return new List<string>(all);
    }

    /// <summary>
    /// What a read template's facts ask for by name: a mock that is still a
    /// mock (by the asset it asks for), what its objects can spawn or drop, its
    /// interior. The names its objects bear are not recorded; the emitted ones
    /// reach the record through the stock lookup the comparison makes.
    /// </summary>
    public void Read(TemplateFacts facts)
    {
        if (facts == null || !facts.AssetLoaded)
            return;
        TemplatesRead++;
        foreach (ChildFact child in facts.Children)
        {
            if (!child.IsRoot && AuditCache.TryMockAsset(child.PrefabName, out string asset, out _))
                Consulted(asset);
            foreach (string referenced in child.ReferencedPrefabs)
                Consulted(AuditCache.TryMockAsset(referenced, out string mocked, out _) ? mocked : referenced);
        }
        if (facts.InteriorPrefabName.Length > 0)
            Consulted(facts.InteriorPrefabName);
    }

    /// <summary>Every recorded name and its digest, sorted by ordinal name.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Entries
    {
        get
        {
            List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>(_recorded);
            entries.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return entries;
        }
    }

    /// <summary>The recorded names whose live signature is no longer what was recorded.</summary>
    public IReadOnlyList<string> Recheck()
    {
        List<string> moved = new List<string>();
        foreach (KeyValuePair<string, string> entry in Entries)
        {
            string now = DigestOf(_sign, _registry, entry.Key, out string? fault);
            if (fault != null)
            {
                Fault ??= fault;
                moved.Add(entry.Key);
                continue;
            }
            if (!string.Equals(now, entry.Value, StringComparison.Ordinal))
                moved.Add(entry.Key);
        }
        return moved;
    }

    /// <summary>
    /// A name's digest: <see cref="NotStock"/> for a name outside the stock
    /// snapshot (the signer is not asked), otherwise SHA-256 of its live
    /// signature, <see cref="Absent"/>, or <see cref="NotStock"/> when what the
    /// lookup found is not a stock prefab.
    /// </summary>
    public static string DigestOf(Func<string, string?> sign, StockPrefabRegistry registry, string name, out string? fault)
    {
        fault = null;
        if (!registry.Has(name))
            return NotStock;
        try
        {
            string? signature = sign(name);
            if (signature == null)
                return Absent;
            if (signature == NotStockSignature)
                return NotStock;
            return AuditCacheCanonical.Sha256Hex(signature);
        }
        catch (Exception ex)
        {
            fault = $"'{name}': {ex.GetType().Name}: {ex.Message}";
            return Absent;
        }
    }
}
