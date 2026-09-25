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
/// when two independent checks say the audit would reach it again —
/// <see cref="AuditCacheKey"/> over every input that can be named before
/// anything is opened, and <see cref="StockRecord"/> over every live stock
/// prefab the verdicts were actually reached against, re-signed from the
/// running game. Any doubt either way, including not being able to ask, is a
/// miss, and a miss is exactly the audit that ran before this existed.</para>
///
/// <para><b>What never uses it.</b> A diagnostic resweep, or a resweep retrying
/// a failed registration: somebody asked for the templates to be looked at
/// again, and answering from a file would not be looking. The first start after
/// any change is also the full audit, by construction.</para>
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
    /// The key parts only the engine can compute — <c>mwl-files</c>,
    /// <c>mwl-config</c>, <c>game</c>, <c>loader</c> — as canonical text by part
    /// name. The rest are computed here, from the run itself, so the key binds
    /// exactly what the run was constructed with.
    /// </summary>
    public static Func<IReadOnlyDictionary<string, string>>? Installation { get; set; }

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

    /// <summary>One start's use of the cache: where, under which key, and the record to store with a fresh audit.</summary>
    public sealed class Attempt
    {
        internal Attempt(string path, AuditCacheKey? key, CatalogueReport? reused, StockRecord? record)
        {
            Path = path;
            Key = key;
            Reused = reused;
            Record = record;
        }

        public string Path { get; }

        /// <summary>Null when the key could not be computed; nothing is stored then.</summary>
        public AuditCacheKey? Key { get; }

        /// <summary>The stored report, when every check passed.</summary>
        public CatalogueReport? Reused { get; }

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
        Func<IReadOnlyDictionary<string, string>>? installation = Installation;
        Func<string, string?>? liveStock = LiveStock;
        if (path == null || installation == null || liveStock == null)
            return null;

        AuditCacheKey key;
        Stopwatch clock = Stopwatch.StartNew();
        try
        {
            key = ComputeKey(run, installation);
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
        AuditCacheLookup lookup = Check(AuditCacheFile.Load(path, key), run, liveStock);
        clock.Stop();
        if (!lookup.Hit)
        {
            Say($"catalogue audit: not reusing stored verdicts ({lookup.Reason}); auditing every template");
            return new Attempt(path, key, null, new StockRecord(liveStock, run.Registry));
        }

        CatalogueReport report = lookup.Contents!.Report;
        Say($"catalogue audit: reused {report.Entries.Count} verdicts from {path} (key {key.Short}); " +
            $"the audit was skipped because every input matched, {lookup.Contents.Stock.Count} stock prefab(s) included " +
            $"(checked in {clock.Elapsed.TotalMilliseconds:0} ms)");
        return new Attempt(path, key, report, null);
    }

    /// <summary>
    /// Everything the key-only check cannot see: that the stored report is this
    /// run's names in this run's order under this run's rules, and that every
    /// stock prefab it was reached against is still the same prefab.
    /// </summary>
    public static AuditCacheLookup Check(AuditCacheLookup lookup, CatalogueAudit.CatalogueAuditRun run, Func<string, string?> liveStock)
    {
        if (lookup == null) throw new ArgumentNullException(nameof(lookup));
        if (run == null) throw new ArgumentNullException(nameof(run));
        if (liveStock == null) throw new ArgumentNullException(nameof(liveStock));
        if (!lookup.Hit)
            return lookup;

        CatalogueReport report = lookup.Contents!.Report;
        if (!string.Equals(report.PolicyFingerprint, run.PolicyFingerprint, StringComparison.Ordinal)
            || !string.Equals(report.StockBuildId, run.StockBuildId, StringComparison.Ordinal))
        {
            return AuditCacheLookup.Missed(AuditCacheMiss.Subjects,
                "the stored report was reached under other rules or another stock snapshot");
        }
        if (report.Entries.Count != run.Subjects.Count)
        {
            return AuditCacheLookup.Missed(AuditCacheMiss.Subjects,
                $"the stored report has {report.Entries.Count} names and this run judges {run.Subjects.Count}");
        }
        for (int i = 0; i < report.Entries.Count; i++)
        {
            // By name only: a name with no definition is judged without its
            // catalogue pack, so an entry's pack need not be its subject's.
            // The packs themselves are in the key's subjects part.
            CatalogueEntry entry = report.Entries[i];
            CatalogueSubject subject = run.Subjects[i];
            if (!string.Equals(entry.Name, subject.Name, StringComparison.Ordinal))
            {
                return AuditCacheLookup.Missed(AuditCacheMiss.Subjects,
                    $"the stored report's name {i + 1} is '{entry.Name}' and this run's is '{subject.Name}'");
            }
        }

        List<string> changed = new List<string>();
        List<string> vanished = new List<string>();
        List<string> appeared = new List<string>();
        foreach (KeyValuePair<string, string> prefab in lookup.Contents.Stock)
        {
            string now = StockRecord.DigestOf(liveStock, run.Registry, prefab.Key, out string? fault);
            if (fault != null)
                return AuditCacheLookup.Missed(AuditCacheMiss.Stock, $"stock prefab '{prefab.Key}' could not be read: {fault}", new[] { prefab.Key });
            if (string.Equals(now, prefab.Value, StringComparison.Ordinal))
                continue;
            if (now == StockRecord.Absent)
                vanished.Add(prefab.Key);
            else if (prefab.Value == StockRecord.Absent)
                appeared.Add(prefab.Key);
            else
                changed.Add(prefab.Key);
        }
        if (changed.Count + vanished.Count + appeared.Count > 0)
        {
            List<string> all = new List<string>();
            all.AddRange(changed);
            all.AddRange(vanished);
            all.AddRange(appeared);
            List<string> parts = new List<string>();
            if (changed.Count > 0) parts.Add("stock prefabs changed: " + Names(changed));
            if (vanished.Count > 0) parts.Add("no longer resolve: " + Names(vanished));
            if (appeared.Count > 0) parts.Add("now resolve: " + Names(appeared));
            return AuditCacheLookup.Missed(AuditCacheMiss.Stock, string.Join("; ", parts.ToArray()), all);
        }
        return lookup;
    }

    /// <summary>
    /// The key for a run: the parts computed from the run and the process, and
    /// the installation's.
    /// </summary>
    /// <exception cref="ArgumentException">The installation left a part out or supplied one twice.</exception>
    public static AuditCacheKey ComputeKey(CatalogueAudit.CatalogueAuditRun run, Func<IReadOnlyDictionary<string, string>> installation)
    {
        if (run == null) throw new ArgumentNullException(nameof(run));
        if (installation == null) throw new ArgumentNullException(nameof(installation));

        Dictionary<string, string> parts = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["format"] = AuditCacheCanonical.Format(AuditCacheFile.FormatVersion),
            ["policy"] = AuditCacheCanonical.Policy(run.PolicyFingerprint, run.Only),
            ["subjects"] = AuditCacheCanonical.Subjects(run.Subjects),
            ["provenance"] = AuditCacheCanonical.Provenance(TemplateAssets.BaselineProvenance),
            ["env"] = AuditCacheCanonical.Environment(Environment.GetEnvironmentVariables()),
        };
        foreach (KeyValuePair<string, string> part in installation())
        {
            // Supplying a part computed here would let an installation replace
            // the run's own policy or names with something else.
            if (parts.ContainsKey(part.Key))
                throw new ArgumentException($"the installation supplied '{part.Key}', which is computed from the run");
            parts[part.Key] = part.Value;
        }
        return AuditCacheKey.FromCanonical(parts);
    }

    /// <summary>
    /// Decide whether a fresh audit's verdicts may be stored, and render them if
    /// so. Called at the end of the sweep, before registration, so the baseline
    /// is re-signed in the state the verdicts were reached in. Null, with the
    /// reason logged, when they may not. Never throws: the worst a failure here
    /// does is make the next start audit again.
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

        // An unresolved verdict is a gap in what THIS run could see — a
        // template that would not load, a walk that stopped — and the next
        // start may see further. Storing it would make a passing failure
        // permanent until something unrelated changed.
        int unresolved = report.CountOf(TemplateVerdict.Unresolved);
        if (unresolved > 0)
        {
            Say($"catalogue audit: verdicts not stored: {unresolved} template(s) are unresolved, " +
                "and an unresolved verdict is a gap in what this run could see; the next start audits again");
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

        return AuditCacheFile.Render(attempt.Key, report, record.Entries);
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
            Say($"catalogue audit: stored {report.Entries.Count} verdicts and {attempt.Record?.Entries.Count ?? 0} " +
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
    private static string Names(IReadOnlyList<string> names)
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

    public StockRecord(Func<string, string?> sign, StockPrefabRegistry registry)
    {
        _sign = sign ?? throw new ArgumentNullException(nameof(sign));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>The first signing failure. Once set nothing more is recorded and nothing is stored.</summary>
    public string? Fault { get; private set; }

    /// <summary>How many templates were actually read, so an empty record can be told from a broken one.</summary>
    public int TemplatesRead { get; private set; }

    /// <summary>A name the audit asked the live game for. Recorded resolved or not.</summary>
    public void Consulted(string name)
    {
        if (Fault != null || string.IsNullOrEmpty(name) || _recorded.ContainsKey(name))
            return;
        string digest = DigestOf(_sign, _registry, name, out string? fault);
        if (fault != null)
        {
            Fault = fault;
            return;
        }
        _recorded[name] = digest;
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
