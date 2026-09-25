using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// Everything a stored verdict depends on, as one digest and as the per-part
/// digests it is made of.
///
/// <para><b>Why a key and not an approval file.</b> A shipped approval was taken
/// out because an approval is a claim about a particular template, and a list
/// that outlives the template it was about approves something nobody looked at.
/// A stored verdict is the same kind of claim, so it is only worth anything
/// while every input that could have changed it is what it was when the audit
/// ran. The key is that "every input" as far as it can be named before anything
/// is opened: the rules, the names to judge, MWL's own files and settings, the
/// game, the loader that resolves the templates' mocks, which other plugins are
/// loaded, and this mode's switches. The rest — the live stock prefabs the
/// templates were compared against and resolved from — is only known once an
/// audit has run, so it is recorded beside the verdicts and checked separately
/// (<see cref="StockRecord"/>). Anything either check cannot vouch for is a
/// miss, and a miss is the full audit.</para>
///
/// <para><b>What is deliberately not in it.</b> Other plugins' files and
/// settings. A verdict reads them only through the stock prefabs they may edit,
/// and those are checked directly, prefab by prefab; hashing every plugin
/// instead would re-audit on every unrelated update and teach an operator to
/// switch the cache off.</para>
///
/// <para><b>Why each part keeps its own digest.</b> "The key changed" sends an
/// operator looking at everything. The stored per-part digests let a miss name
/// the part that moved — config, not the game — so a boot that audited when it
/// was expected to reuse can be explained from the log line alone.</para>
///
/// <para>SHA-256 rather than the fingerprints' FNV: this digest stands between
/// an operator's disk and a decision about what a stock client receives, and a
/// 64-bit non-cryptographic hash over three hundred megabytes of bundles is not
/// something to rest that on.</para>
/// </summary>
public sealed class AuditCacheKey
{
    /// <summary>
    /// The parts, in the order they are rendered and compared. Fixed, and all
    /// required: a key missing a part would compare equal to another missing the
    /// same part, which is an approval of whatever that part now holds.
    /// </summary>
    public static readonly IReadOnlyList<string> ComponentNames = new[]
    {
        "format", "policy", "subjects", "mwl-files", "mwl-config", "game", "loader", "provenance", "env",
    };

    private readonly List<KeyValuePair<string, string>> _components;

    private AuditCacheKey(List<KeyValuePair<string, string>> components, string digest)
    {
        _components = components;
        Digest = digest;
    }

    /// <summary>Each part's name and the SHA-256 of its canonical text, in <see cref="ComponentNames"/> order.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Components => _components;

    /// <summary>The whole key: SHA-256 over every part's name and digest.</summary>
    public string Digest { get; }

    /// <summary>The first twelve hex digits, for a log line.</summary>
    public string Short => Digest.Length <= 12 ? Digest : Digest.Substring(0, 12);

    /// <summary>
    /// Build a key from each part's canonical text.
    /// </summary>
    /// <exception cref="ArgumentException">A part is missing, unknown or null. Thrown rather than defaulted: a key that quietly covers less is the defect this type exists to prevent.</exception>
    public static AuditCacheKey FromCanonical(IReadOnlyDictionary<string, string> canonicalByName)
    {
        if (canonicalByName == null) throw new ArgumentNullException(nameof(canonicalByName));

        foreach (KeyValuePair<string, string> given in canonicalByName)
        {
            if (!Contains(ComponentNames, given.Key))
                throw new ArgumentException($"'{given.Key}' is not a cache key part");
        }

        List<KeyValuePair<string, string>> digests = new List<KeyValuePair<string, string>>();
        foreach (string name in ComponentNames)
        {
            if (!canonicalByName.TryGetValue(name, out string? canonical) || canonical == null)
                throw new ArgumentException($"the cache key part '{name}' was not computed");
            digests.Add(new KeyValuePair<string, string>(name, AuditCacheCanonical.Sha256Hex(canonical)));
        }
        return FromDigests(digests);
    }

    /// <summary>
    /// A key from part digests already computed — which is what a stored file
    /// carries. The whole-key digest is recomputed, never read, so a file cannot
    /// claim a key its parts do not add up to.
    /// </summary>
    internal static AuditCacheKey FromDigests(List<KeyValuePair<string, string>> digests)
    {
        StringBuilder text = new StringBuilder();
        text.Append("mwl-server-only-audit-key\n");
        foreach (KeyValuePair<string, string> part in digests)
            text.Append(part.Key).Append('=').Append(part.Value).Append('\n');
        return new AuditCacheKey(digests, AuditCacheCanonical.Sha256Hex(text.ToString()));
    }

    /// <summary>
    /// The parts whose digests differ from <paramref name="stored"/>, by name,
    /// in this key's order. A part present on one side only counts as differing.
    /// </summary>
    public IReadOnlyList<string> Differences(IReadOnlyList<KeyValuePair<string, string>> stored)
    {
        if (stored == null) throw new ArgumentNullException(nameof(stored));

        Dictionary<string, string> theirs = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> part in stored)
            theirs[part.Key] = part.Value;

        List<string> differing = new List<string>();
        foreach (KeyValuePair<string, string> part in _components)
        {
            if (!theirs.TryGetValue(part.Key, out string? digest) || !string.Equals(digest, part.Value, StringComparison.Ordinal))
                differing.Add(part.Key);
            theirs.Remove(part.Key);
        }
        foreach (string extra in theirs.Keys)
            differing.Add(extra);
        return differing;
    }

    private static bool Contains(IReadOnlyList<string> names, string name)
    {
        foreach (string candidate in names)
        {
            if (string.Equals(candidate, name, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}

/// <summary>
/// The canonical text of each key part, and the hashing under it.
///
/// <para>Separate from the code that finds the files because this is the half
/// that decides what "the same input" means, and it has to be testable without
/// a game: a rendering that lets two different installations produce the same
/// text is a cache that reuses a verdict about something else.</para>
///
/// <para>Every free-text field is escaped, so no value can forge a line
/// boundary, and every list is sorted by ordinal comparison, so the text does
/// not depend on a directory listing's or a dictionary's order.</para>
/// </summary>
public static class AuditCacheCanonical
{
    /// <summary>The cache format, the content fingerprint's inputs and the rules' version.</summary>
    public static string Format(int cacheFormatVersion) =>
        "cache=" + cacheFormatVersion.ToString(CultureInfo.InvariantCulture) + "\n" +
        "content=" + TemplateFingerprint.ContentVersion.ToString(CultureInfo.InvariantCulture) + "\n" +
        "policy=" + TemplateFingerprint.PolicyVersion.ToString(CultureInfo.InvariantCulture) + "\n";

    /// <summary>
    /// The rules the run applies and the subset it may register.
    ///
    /// The subset is part of it because a verdict's selection outcome is: the
    /// same template judged under a narrower subset is recorded as not
    /// selected, and reusing that under a wider one would register nothing.
    /// </summary>
    public static string Policy(string policyFingerprint, IEnumerable<string> only)
    {
        if (only == null) throw new ArgumentNullException(nameof(only));
        StringBuilder text = new StringBuilder();
        text.Append("fingerprint=").Append(Escape(policyFingerprint ?? "")).Append('\n');
        foreach (string name in Sorted(only))
            text.Append("only=").Append(Escape(name)).Append('\n');
        return text.ToString();
    }

    /// <summary>
    /// The names to judge, in the order the run judges them. Not sorted: the
    /// report is in this order and registration walks it, so two orders are two
    /// different runs.
    /// </summary>
    public static string Subjects(IEnumerable<CatalogueSubject> subjects)
    {
        if (subjects == null) throw new ArgumentNullException(nameof(subjects));
        StringBuilder text = new StringBuilder();
        foreach (CatalogueSubject subject in subjects)
        {
            text.Append("subject\t").Append(Escape(subject.Name))
                .Append('\t').Append(Escape(subject.Pack))
                .Append('\t').Append(subject.SourceDeclared ? '1' : '0')
                .Append('\t').Append(Escape(subject.InteriorPrefabName))
                .Append('\t').Append(Escape(subject.DungeonTheme))
                .Append('\n');
        }
        return text.ToString();
    }

    /// <summary>A directory's files as relative path and hash, one per line, under a label.</summary>
    public static string Tree(string label, FileTree tree)
    {
        if (tree == null) throw new ArgumentNullException(nameof(tree));
        StringBuilder text = new StringBuilder();
        // Absent and empty are different installations: a folder somebody
        // deleted is not the same as one somebody emptied, and a rendering
        // that agreed would be deciding that for them.
        text.Append(label).Append(tree.Exists ? " present " : " absent ")
            .Append(tree.Files.Count.ToString(CultureInfo.InvariantCulture)).Append('\n');
        foreach (KeyValuePair<string, string> file in tree.Files)
            text.Append(label).Append('\t').Append(Escape(file.Key)).Append('\t').Append(file.Value).Append('\n');
        return text.ToString();
    }

    /// <summary>
    /// The game: its code, its version text, and the network version it was
    /// built with. The version text carries the build's own hash, which moves
    /// with the data the stock prefabs come from.
    ///
    /// Headless is part of it because Jötunn resolves mocks differently without
    /// a graphics device (it skips textures), so a dedicated server and a
    /// hosting client can resolve the same bundle into two templates.
    /// </summary>
    public static string Game(string assemblySha256, string versionString, string networkVersion, bool headless) =>
        "assembly=" + Escape(assemblySha256 ?? "") + "\n" +
        "version=" + Escape(versionString ?? "") + "\n" +
        "network=" + Escape(networkVersion ?? "") + "\n" +
        "headless=" + (headless ? "1" : "0") + "\n";

    /// <summary>
    /// The loader: the Jötunn build that resolves every template's mock
    /// references, and BepInEx's own core. A different resolver can turn the
    /// same bundle into a different template.
    /// </summary>
    public static string Loader(string jotunnSha256, FileTree core)
    {
        if (core == null) throw new ArgumentNullException(nameof(core));
        return "jotunn=" + Escape(jotunnSha256 ?? "") + "\n" + Tree("core", core);
    }

    /// <summary>
    /// Whether a file under the config folder is one of MWL's own: its BepInEx
    /// settings (named after its plugin id) and its YAML files. Everything else
    /// there belongs to other plugins, and reaches a verdict only through the
    /// stock prefabs, which are checked directly.
    /// </summary>
    public static bool IsMwlConfigFile(string relativePath, string pluginGuid)
    {
        if (string.IsNullOrEmpty(relativePath) || string.IsNullOrEmpty(pluginGuid))
            return false;
        int slash = relativePath.LastIndexOf('/');
        string name = slash < 0 ? relativePath : relativePath.Substring(slash + 1);
        if (name.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase))
            return name.StartsWith(pluginGuid, StringComparison.OrdinalIgnoreCase);
        if (name.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            return name.StartsWith(MwlYamlPrefix, StringComparison.OrdinalIgnoreCase);
        return false;
    }

    /// <summary>What every one of MWL's YAML files is named with.</summary>
    public const string MwlYamlPrefix = "warpalicious.More_World_Locations_";

    /// <summary>
    /// Which other plugins the baseline was read beside — the text every
    /// verdict's provenance note carries. Part of the key so a reused report
    /// never names a different set of plugins from the one actually loaded.
    /// </summary>
    public static string Provenance(string baselineProvenance) =>
        "provenance=" + Escape(baselineProvenance ?? "") + "\n";

    /// <summary>
    /// The four parts only an installation can supply, from where its files
    /// are. Separate from finding those places so that what is read, and what
    /// is deliberately not, is exercised against real folders in a test.
    /// </summary>
    /// <param name="manifestDirectory">
    /// Where the bundle manifest the templates are loaded through was found.
    /// MWL looks for it beside its DLL and falls back to a fixed plugin folder,
    /// so the bundles actually read need not be under <paramref name="mwlDirectory"/>;
    /// when they are elsewhere, that folder is hashed as well.
    /// </param>
    /// <param name="exclude">Full paths never to read; the cache file itself.</param>
    /// <exception cref="InvalidOperationException">MWL's own folder is missing or empty: there is nothing to vouch for.</exception>
    public static Dictionary<string, string> Installation(
        string mwlDirectory, string? manifestDirectory, string configDirectory, string pluginGuid,
        string gameAssemblyPath, string versionString, string networkVersion, bool headless,
        string jotunnPath, string coreDirectory, Func<string, bool>? exclude)
    {
        FileTree mwl = HashTree(mwlDirectory, null, exclude);
        if (!mwl.Exists || mwl.Files.Count == 0)
            throw new InvalidOperationException($"MWL's own folder {mwlDirectory} holds nothing");
        string mwlText = Tree("mwl", mwl);
        if (!string.IsNullOrEmpty(manifestDirectory) && !IsWithin(manifestDirectory!, mwlDirectory))
            mwlText += Tree("manifest", HashTree(manifestDirectory!, null, exclude));
        // Only MWL's own settings: other plugins' files reach a verdict through
        // the stock prefabs alone, and those are checked prefab by prefab.
        // A .cfg is read by its settings, not its bytes: BepInEx rewrites the
        // comment block every start, and one setting's default is a fresh
        // random id each time, so the file's bytes change on every boot while
        // no value does.
        FileTree config = HashTree(configDirectory, relative => IsMwlConfigFile(relative, pluginGuid), exclude,
            (relative, absolute) => relative.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase)
                ? Sha256OfCfgSettings(absolute)
                : Sha256OfFile(absolute));

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mwl-files"] = mwlText,
            ["mwl-config"] = Tree("config", config),
            ["game"] = Game(Sha256OfFile(gameAssemblyPath), versionString, networkVersion, headless),
            ["loader"] = Loader(Sha256OfFile(jotunnPath), HashTree(coreDirectory, null, exclude)),
        };
    }

    private static bool IsWithin(string path, string directory)
    {
        string full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(full, root, StringComparison.OrdinalIgnoreCase)
               || full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// This mode's switches: every variable named with the switches' prefix,
    /// except the cache's own two, which decide where the verdicts are kept and
    /// not what they are.
    ///
    /// The prefix is matched without regard to case because Windows treats
    /// variable names that way; a wider match only ever costs a miss.
    /// </summary>
    public static string Environment(IDictionary variables)
    {
        if (variables == null) throw new ArgumentNullException(nameof(variables));
        List<KeyValuePair<string, string>> ours = new List<KeyValuePair<string, string>>();
        foreach (DictionaryEntry entry in variables)
        {
            string name = entry.Key as string ?? "";
            if (!name.StartsWith(ValidationSwitches.Prefix, StringComparison.OrdinalIgnoreCase))
                continue;
            if (string.Equals(name, ValidationSwitches.AuditCacheVariable, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, ValidationSwitches.AuditCachePathVariable, StringComparison.OrdinalIgnoreCase))
                continue;
            ours.Add(new KeyValuePair<string, string>(name, entry.Value as string ?? ""));
        }
        ours.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        StringBuilder text = new StringBuilder();
        foreach (KeyValuePair<string, string> variable in ours)
            text.Append(Escape(variable.Key)).Append('=').Append(Escape(variable.Value)).Append('\n');
        return text.ToString();
    }

    /// <summary>
    /// Hash every file under <paramref name="root"/>, recursively, as relative
    /// path (with '/' separators) and SHA-256, sorted by ordinal path.
    /// </summary>
    /// <param name="include">Which relative paths count; null for all of them.</param>
    /// <param name="exclude">Full paths never to read — the cache file itself, should it be kept inside a hashed folder.</param>
    /// <param name="hash">How one file is digested, from its relative and full path; null for its bytes.</param>
    /// <exception cref="IOException">A file that cannot be read. Not skipped: a file the key could not read is a file it cannot vouch for.</exception>
    public static FileTree HashTree(string root, Func<string, bool>? include = null, Func<string, bool>? exclude = null,
        Func<string, string, string>? hash = null)
    {
        if (root == null) throw new ArgumentNullException(nameof(root));
        if (!Directory.Exists(root))
            return new FileTree(false, new List<KeyValuePair<string, string>>());

        string full = Path.GetFullPath(root);
        string prefix = full.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? full
            : full + Path.DirectorySeparatorChar;
        List<KeyValuePair<string, string>> files = new List<KeyValuePair<string, string>>();
        foreach (string path in Directory.GetFiles(full, "*", SearchOption.AllDirectories))
        {
            string absolute = Path.GetFullPath(path);
            if (exclude != null && exclude(absolute))
                continue;
            string relative = absolute.StartsWith(prefix, StringComparison.Ordinal)
                ? absolute.Substring(prefix.Length)
                : absolute;
            relative = relative.Replace('\\', '/');
            if (include != null && !include(relative))
                continue;
            files.Add(new KeyValuePair<string, string>(relative, hash != null ? hash(relative, absolute) : Sha256OfFile(absolute)));
        }
        files.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        return new FileTree(true, files);
    }

    /// <summary>
    /// The digest of a BepInEx .cfg by what it sets: every line that is not
    /// blank and not a '#' comment, trimmed, in order. Section headers and
    /// "key = value" lines are all a .cfg holds besides comments, so a change
    /// to any value still changes the digest; a rewritten description or
    /// default does not.
    /// </summary>
    public static string Sha256OfCfgSettings(string path)
    {
        StringBuilder settings = new StringBuilder();
        foreach (string line in File.ReadAllLines(path, new UTF8Encoding(false)))
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed[0] == '#')
                continue;
            settings.Append(trimmed).Append('\n');
        }
        return Sha256Hex(settings.ToString());
    }

    public static string Sha256OfFile(string path)
    {
        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1 << 16);
        using SHA256 sha = SHA256.Create();
        return Hex(sha.ComputeHash(stream));
    }

    /// <summary>SHA-256 over UTF-8, as 64 lower-case hex characters.</summary>
    public static string Sha256Hex(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        using SHA256 sha = SHA256.Create();
        return Hex(sha.ComputeHash(new UTF8Encoding(false).GetBytes(text)));
    }

    private static string Hex(byte[] bytes)
    {
        StringBuilder text = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
            text.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        return text.ToString();
    }

    /// <summary>
    /// Backslash, tab, carriage return and newline as two-character escapes, so
    /// one value is always one field on one line.
    /// </summary>
    public static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        StringBuilder text = new StringBuilder(value.Length + 8);
        foreach (char c in value)
        {
            switch (c)
            {
                case '\\': text.Append("\\\\"); break;
                case '\t': text.Append("\\t"); break;
                case '\n': text.Append("\\n"); break;
                case '\r': text.Append("\\r"); break;
                default: text.Append(c); break;
            }
        }
        return text.ToString();
    }

    /// <summary>The inverse of <see cref="Escape"/>. False on a stray backslash or an escape it never writes.</summary>
    public static bool TryUnescape(string value, out string result)
    {
        result = "";
        if (value == null)
            return false;
        if (value.IndexOf('\\') < 0)
        {
            result = value;
            return true;
        }
        StringBuilder text = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c != '\\')
            {
                text.Append(c);
                continue;
            }
            if (++i >= value.Length)
                return false;
            switch (value[i])
            {
                case '\\': text.Append('\\'); break;
                case 't': text.Append('\t'); break;
                case 'n': text.Append('\n'); break;
                case 'r': text.Append('\r'); break;
                default: return false;
            }
        }
        result = text.ToString();
        return true;
    }

    private static List<string> Sorted(IEnumerable<string> values)
    {
        List<string> sorted = new List<string>(values);
        sorted.Sort(StringComparer.Ordinal);
        return sorted;
    }
}

/// <summary>A hashed directory: whether it exists, and its files as relative path and SHA-256, sorted.</summary>
public sealed class FileTree
{
    public FileTree(bool exists, IReadOnlyList<KeyValuePair<string, string>> files)
    {
        Exists = exists;
        Files = files ?? Array.Empty<KeyValuePair<string, string>>();
    }

    public bool Exists { get; }
    public IReadOnlyList<KeyValuePair<string, string>> Files { get; }
}
