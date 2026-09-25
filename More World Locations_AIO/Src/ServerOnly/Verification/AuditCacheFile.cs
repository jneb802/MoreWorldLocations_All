using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>Why stored verdicts were not reused. Every value but <see cref="None"/> means the full audit runs.</summary>
public enum AuditCacheMiss
{
    None,
    /// <summary>Switched off, or this process has no way to compute a key.</summary>
    Off,
    NoFile,
    /// <summary>The file exists and could not be read, or is not valid UTF-8.</summary>
    Unreadable,
    /// <summary>Not a verdict cache at all.</summary>
    Magic,
    /// <summary>A verdict cache written in a format this build does not read.</summary>
    FormatVersion,
    /// <summary>The trailer is missing: the write stopped part way, or the file was cut short.</summary>
    Truncated,
    /// <summary>The trailer's digest does not match what precedes it: something changed the file after it was written.</summary>
    BodyDigest,
    /// <summary>A line that is not what the format says belongs there.</summary>
    Malformed,
    /// <summary>A verdict, outcome or severity this build does not know.</summary>
    UnknownValue,
    /// <summary>The trailer's counts disagree with the lines that were read.</summary>
    CountMismatch,
    /// <summary>One or more key parts differ from this start's.</summary>
    KeyChanged,
    /// <summary>The stored report does not cover this run's names in this run's order.</summary>
    Subjects,
    /// <summary>A stock prefab the verdicts were reached against is not what it was.</summary>
    Stock,
    /// <summary>The key could not be computed.</summary>
    KeyFailed,
}

/// <summary>What a stored verdict file held, once every check on it passed.</summary>
public sealed class AuditCacheContents
{
    public AuditCacheContents(CatalogueReport report, IReadOnlyList<KeyValuePair<string, string>> stock)
    {
        Report = report;
        Stock = stock;
    }

    public CatalogueReport Report { get; }

    /// <summary>Each recorded stock prefab name and the digest of its live signature, or <see cref="StockRecord.Absent"/>.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Stock { get; }
}

/// <summary>The answer to "may this start reuse the stored verdicts", with the reason when it may not.</summary>
public sealed class AuditCacheLookup
{
    private AuditCacheLookup(AuditCacheMiss miss, string reason, AuditCacheContents? contents, IReadOnlyList<string> changed)
    {
        Miss = miss;
        Reason = reason;
        Contents = contents;
        Changed = changed;
    }

    public AuditCacheMiss Miss { get; }
    public string Reason { get; }
    public AuditCacheContents? Contents { get; }

    /// <summary>For <see cref="AuditCacheMiss.KeyChanged"/> the key parts, for <see cref="AuditCacheMiss.Stock"/> the prefabs, that differ.</summary>
    public IReadOnlyList<string> Changed { get; }

    public bool Hit => Miss == AuditCacheMiss.None;

    internal static AuditCacheLookup Found(AuditCacheContents contents) =>
        new AuditCacheLookup(AuditCacheMiss.None, "", contents, Array.Empty<string>());

    internal static AuditCacheLookup Missed(AuditCacheMiss miss, string reason, IReadOnlyList<string>? changed = null) =>
        new AuditCacheLookup(miss, reason, null, changed ?? Array.Empty<string>());
}

/// <summary>
/// The verdict cache on disk: write it, read it, and refuse it on any doubt.
///
/// <para><b>Plain text, one record per line.</b> For the same reason the
/// approved snapshot and the stock registry are text: when a start audits that
/// was expected to reuse, the person asking why can open the file, read the key
/// parts and the verdicts, and diff it against the next one. Tab separated, with
/// backslash escapes, so a finding's detail can say anything and still be one
/// field.</para>
///
/// <para><b>Order is kept.</b> The entries are written in the report's order,
/// which is the catalogue's and the one registration walks. A reused report in
/// any other order would register the same names and announce them
/// differently, and "the same report" means the same report.</para>
///
/// <para><b>Every check fails closed.</b> A file that is short, edited, from
/// another format, carries a value this build does not know, or counts
/// differently from what it holds is a miss with a reason — never a partial
/// reuse. A digest over everything before the trailer catches the edits the
/// parse itself would not: a verdict changed from blocked to compatible parses
/// perfectly well.</para>
/// </summary>
public static class AuditCacheFile
{
    /// <summary>The first field of the first line.</summary>
    public const string Magic = "MWL-SERVER-ONLY-AUDIT-CACHE";

    /// <summary>
    /// Bumped whenever the layout or the meaning of a stored field changes. Part
    /// of the key as well, so a new format is a new key even if a reader were
    /// lenient.
    /// </summary>
    // 2: every name outside the stock snapshot is stored as "not-stock", and a
    // stock name is signed from the comparison's own lookup alone.
    public const int FormatVersion = 2;

    /// <summary>Render the verdicts, the key they were reached under, and the stock prefabs they were reached against.</summary>
    public static string Render(AuditCacheKey key, CatalogueReport report, IReadOnlyList<KeyValuePair<string, string>> stock)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (report == null) throw new ArgumentNullException(nameof(report));
        if (stock == null) throw new ArgumentNullException(nameof(stock));

        StringBuilder text = new StringBuilder();
        text.Append(Magic).Append('\t').Append(FormatVersion.ToString(CultureInfo.InvariantCulture)).Append('\n');
        text.Append("key\t").Append(key.Digest).Append('\n');
        foreach (KeyValuePair<string, string> part in key.Components)
            text.Append("component\t").Append(part.Key).Append('\t').Append(part.Value).Append('\n');
        text.Append("report\t").Append(E(report.StockBuildId)).Append('\t').Append(E(report.PolicyFingerprint)).Append('\n');
        foreach (KeyValuePair<string, string> prefab in stock)
            text.Append("stock\t").Append(E(prefab.Key)).Append('\t').Append(E(prefab.Value)).Append('\n');
        foreach (CatalogueEntry entry in report.Entries)
        {
            text.Append("entry\t").Append(E(entry.Evaluation.Name))
                .Append('\t').Append(E(entry.Evaluation.Pack))
                .Append('\t').Append(entry.Evaluation.Verdict.ToString())
                .Append('\t').Append(E(entry.Decision.Name))
                .Append('\t').Append(entry.Decision.Outcome.ToString())
                .Append('\t').Append(E(entry.Decision.Reason))
                .Append('\t').Append(E(entry.ContentFingerprint))
                .Append('\t').Append(entry.Evaluation.Findings.Count.ToString(CultureInfo.InvariantCulture))
                .Append('\n');
            foreach (TemplateFinding finding in entry.Evaluation.Findings)
            {
                text.Append("finding\t").Append(E(finding.Code))
                    .Append('\t').Append(finding.Severity.ToString())
                    .Append('\t').Append(E(finding.Path))
                    .Append('\t').Append(E(finding.Detail))
                    .Append('\t').Append(E(finding.Value))
                    .Append('\n');
            }
        }

        string body = text.ToString();
        return body + "end\t" + report.Entries.Count.ToString(CultureInfo.InvariantCulture)
               + "\t" + stock.Count.ToString(CultureInfo.InvariantCulture)
               + "\t" + AuditCacheCanonical.Sha256Hex(body) + "\n";
    }

    /// <summary>
    /// Read a stored file against this start's key. Integrity first, then the
    /// key, then the body: a file whose digest does not add up is not asked
    /// which key parts it claims.
    /// </summary>
    public static AuditCacheLookup Parse(string text, AuditCacheKey key)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        if (key == null) throw new ArgumentNullException(nameof(key));

        int firstBreak = text.IndexOf('\n');
        string first = firstBreak < 0 ? text : text.Substring(0, firstBreak);
        string[] header = first.Split('\t');
        if (header[0] != Magic)
            return AuditCacheLookup.Missed(AuditCacheMiss.Magic, "the file is not a verdict cache");
        if (header.Length != 2 || header[1] != FormatVersion.ToString(CultureInfo.InvariantCulture))
        {
            return AuditCacheLookup.Missed(AuditCacheMiss.FormatVersion,
                $"the file is cache format {(header.Length > 1 ? header[1] : "(none)")} and this build reads {FormatVersion}");
        }

        // The trailer: the last line, complete, with its newline.
        if (!text.EndsWith("\n", StringComparison.Ordinal))
            return AuditCacheLookup.Missed(AuditCacheMiss.Truncated, "the file does not end with a complete line");
        int trailerStart = text.LastIndexOf('\n', text.Length - 2) + 1;
        string[] trailer = text.Substring(trailerStart, text.Length - 1 - trailerStart).Split('\t');
        if (trailer.Length != 4 || trailer[0] != "end"
            || !TryCount(trailer[1], out int entryCount) || !TryCount(trailer[2], out int stockCount))
            return AuditCacheLookup.Missed(AuditCacheMiss.Truncated, "the file has no trailer; it was cut short");
        string body = text.Substring(0, trailerStart);
        if (!string.Equals(AuditCacheCanonical.Sha256Hex(body), trailer[3], StringComparison.Ordinal))
            return AuditCacheLookup.Missed(AuditCacheMiss.BodyDigest, "the file's digest does not match its contents; it was changed after it was written");

        string[] lines = body.Split('\n');
        // body ends with '\n', so the split's last element is empty.
        int count = lines.Length - 1;
        int at = 1;

        if (at >= count || !TrySplit(lines[at], "key", 2, out string[] keyLine))
            return Malformed(at, "expected the key");
        at++;
        List<KeyValuePair<string, string>> stored = new List<KeyValuePair<string, string>>();
        while (at < count && lines[at].StartsWith("component\t", StringComparison.Ordinal))
        {
            if (!TrySplit(lines[at], "component", 3, out string[] part))
                return Malformed(at, "a key part");
            stored.Add(new KeyValuePair<string, string>(part[1], part[2]));
            at++;
        }
        // The stored whole-key digest must be what its own parts add up to;
        // otherwise the header is not describing the file it heads.
        if (!string.Equals(AuditCacheKey.FromDigests(stored).Digest, keyLine[1], StringComparison.Ordinal))
            return Malformed(1, "the key does not match its parts");

        IReadOnlyList<string> differing = key.Differences(stored);
        if (differing.Count > 0)
        {
            return AuditCacheLookup.Missed(AuditCacheMiss.KeyChanged,
                "inputs changed: " + string.Join(", ", ToArray(differing)), differing);
        }

        if (at >= count || !TrySplit(lines[at], "report", 3, out string[] reportLine)
            || !TryUnescape(reportLine[1], out string stockBuildId) || !TryUnescape(reportLine[2], out string policyFingerprint))
            return Malformed(at, "expected the report header");
        at++;

        List<KeyValuePair<string, string>> stock = new List<KeyValuePair<string, string>>();
        while (at < count && lines[at].StartsWith("stock\t", StringComparison.Ordinal))
        {
            if (!TrySplit(lines[at], "stock", 3, out string[] prefab)
                || !TryUnescape(prefab[1], out string name) || !TryUnescape(prefab[2], out string signature))
                return Malformed(at, "a stock prefab");
            stock.Add(new KeyValuePair<string, string>(name, signature));
            at++;
        }

        List<CatalogueEntry> entries = new List<CatalogueEntry>();
        while (at < count)
        {
            if (!TrySplit(lines[at], "entry", 9, out string[] e)
                || !TryUnescape(e[1], out string evaluationName) || !TryUnescape(e[2], out string pack)
                || !TryUnescape(e[4], out string decisionName) || !TryUnescape(e[6], out string reason)
                || !TryUnescape(e[7], out string fingerprint) || !TryCount(e[8], out int findingCount))
                return Malformed(at, "expected an entry");
            if (!TryEnum(e[3], out TemplateVerdict verdict))
                return Unknown(at, "verdict", e[3]);
            if (!TryEnum(e[5], out SelectionOutcome outcome))
                return Unknown(at, "selection outcome", e[5]);
            at++;

            List<TemplateFinding> findings = new List<TemplateFinding>(findingCount);
            for (int i = 0; i < findingCount; i++, at++)
            {
                if (at >= count || !TrySplit(lines[at], "finding", 6, out string[] f)
                    || !TryUnescape(f[1], out string code) || !TryUnescape(f[3], out string path)
                    || !TryUnescape(f[4], out string detail) || !TryUnescape(f[5], out string value))
                    return Malformed(at, $"expected finding {i + 1} of {findingCount} for '{evaluationName}'");
                if (!TryEnum(f[2], out FindingSeverity severity))
                    return Unknown(at, "finding severity", f[2]);
                findings.Add(new TemplateFinding(code, severity, path, detail, value));
            }

            entries.Add(new CatalogueEntry(
                new TemplateEvaluation(evaluationName, pack, verdict, findings),
                new SelectionDecision(decisionName, outcome, reason),
                fingerprint));
        }

        if (entries.Count != entryCount || stock.Count != stockCount)
        {
            return AuditCacheLookup.Missed(AuditCacheMiss.CountMismatch,
                $"the trailer counts {entryCount} entries and {stockCount} stock prefabs and the file holds {entries.Count} and {stock.Count}");
        }

        CatalogueReport report;
        try
        {
            report = new CatalogueReport(entries, stockBuildId, policyFingerprint);
        }
        catch (ArgumentException ex)
        {
            return AuditCacheLookup.Missed(AuditCacheMiss.Malformed, "the stored report is not a valid report: " + ex.Message);
        }
        return AuditCacheLookup.Found(new AuditCacheContents(report, stock));
    }

    /// <summary>Read and check the file at <paramref name="path"/>. Never throws for anything about the file.</summary>
    public static AuditCacheLookup Load(string path, AuditCacheKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        string text;
        try
        {
            if (!File.Exists(path))
                return AuditCacheLookup.Missed(AuditCacheMiss.NoFile, $"there is no cache file at {path}");
            // Strict decoding, and no guessing from a byte-order mark: a byte
            // sequence that is not UTF-8 is damage, and a lenient or guessing
            // decoder would turn it into text the digest then agrees with only
            // by accident. The writer never writes a mark.
            text = new UTF8Encoding(false, true).GetString(File.ReadAllBytes(path));
        }
        catch (Exception ex)
        {
            return AuditCacheLookup.Missed(AuditCacheMiss.Unreadable, $"{path} could not be read: {ex.GetType().Name}: {ex.Message}");
        }
        return Parse(text, key);
    }

    /// <summary>
    /// Write atomically: a temporary file beside the target, then a replace.
    /// A reader sees the old file or the new one and never half of either.
    /// </summary>
    /// <returns>Null on success, otherwise what went wrong. It never throws: a cache that cannot be written costs the next start an audit and nothing else.</returns>
    public static string? Save(string path, string text)
    {
        string? temporary = null;
        try
        {
            string full = Path.GetFullPath(path);
            string? directory = Path.GetDirectoryName(full);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            temporary = full + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporary, text, new UTF8Encoding(false));
            if (File.Exists(full))
            {
                try
                {
                    File.Replace(temporary, full, null);
                }
                catch (Exception ex) when (ex is PlatformNotSupportedException || ex is IOException || ex is UnauthorizedAccessException)
                {
                    // Not every filesystem Mono runs on supports the atomic
                    // replace. Delete-then-move leaves a moment with no file,
                    // which reads as a miss, never as half a file.
                    File.Delete(full);
                    File.Move(temporary, full);
                }
            }
            else
            {
                File.Move(temporary, full);
            }
            temporary = null;
            return null;
        }
        catch (Exception ex)
        {
            return $"{ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            if (temporary != null)
            {
                try
                {
                    if (File.Exists(temporary))
                        File.Delete(temporary);
                }
                catch
                {
                    // Nothing left to do with a temporary file we cannot remove;
                    // it is never read, only ever replaced by the next one.
                }
            }
        }
    }

    private static string E(string value) => AuditCacheCanonical.Escape(value);

    private static bool TryUnescape(string value, out string result) => AuditCacheCanonical.TryUnescape(value, out result);

    private static bool TrySplit(string line, string tag, int fields, out string[] parts)
    {
        parts = line.Split('\t');
        return parts.Length == fields && parts[0] == tag;
    }

    private static bool TryCount(string text, out int value) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value) && value >= 0;

    /// <summary>
    /// An enum by its exact name. Not <c>Enum.TryParse</c>: that accepts any
    /// number, defined or not, and a verdict read as "7" is a verdict this
    /// build never reached.
    /// </summary>
    private static bool TryEnum<T>(string text, out T value) where T : struct, Enum
    {
        foreach (T candidate in (T[])Enum.GetValues(typeof(T)))
        {
            if (string.Equals(candidate.ToString(), text, StringComparison.Ordinal))
            {
                value = candidate;
                return true;
            }
        }
        value = default;
        return false;
    }

    private static AuditCacheLookup Malformed(int line, string what) =>
        AuditCacheLookup.Missed(AuditCacheMiss.Malformed, $"line {line + 1} is malformed: {what}");

    private static AuditCacheLookup Unknown(int line, string what, string value) =>
        AuditCacheLookup.Missed(AuditCacheMiss.UnknownValue, $"line {line + 1} has a {what} this build does not know: '{value}'");

    private static string[] ToArray(IReadOnlyList<string> values)
    {
        string[] array = new string[values.Count];
        for (int i = 0; i < values.Count; i++)
            array[i] = values[i];
        return array;
    }
}
