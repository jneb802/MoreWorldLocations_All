using System;
using System.Collections.Generic;
using System.Globalization;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// What a client without the mod can build: every networked prefab the stock
/// game ships, at one game build.
///
/// <para><b>Why not ask the running game.</b> The question is what the STOCK
/// client has, and the process asking is a server that has just added its own
/// prefabs to <c>ZNetScene</c>. Looking there would answer "can I build this",
/// which is true of every custom prefab MWL registers and proves nothing at all
/// about the player. So the answer comes from a snapshot taken from the game's
/// own asset bundles, shipped beside the mod and stamped with the build it was
/// taken from.</para>
///
/// <para><b>Why the hash matters as well as the name.</b> A ZDO carries the
/// prefab's stable hash, not its name; the client looks up the hash. Two
/// different names can hash to the same value, and a template child whose name
/// collides with a stock prefab would spawn that other prefab on the client
/// while looking correct on the server. So a name is only known when the
/// registry holds it AND the hash it holds is the hash of that name.</para>
/// </summary>
public sealed class StockPrefabRegistry
{
    private readonly Dictionary<string, int> _hashByName;
    private readonly Dictionary<int, string> _nameByHash;

    private StockPrefabRegistry(string gameBuildId, string source, Dictionary<string, int> hashByName, Dictionary<int, string> nameByHash)
    {
        GameBuildId = gameBuildId;
        Source = source;
        _hashByName = hashByName;
        _nameByHash = nameByHash;
    }

    /// <summary>The Steam build the snapshot was taken from, so drift can be reported rather than assumed away.</summary>
    public string GameBuildId { get; }

    /// <summary>Where the snapshot came from, for the report.</summary>
    public string Source { get; }

    public int Count => _hashByName.Count;

    /// <summary>An empty registry. Every lookup misses, which is what "we have no snapshot" should do to a verdict.</summary>
    public static StockPrefabRegistry Empty { get; } =
        new StockPrefabRegistry("", "none", new Dictionary<string, int>(), new Dictionary<int, string>());

    /// <summary>
    /// Whether the stock client has a prefab under exactly this name.
    /// Case-sensitive, because <c>GetStableHashCode</c> is.
    /// </summary>
    public bool Has(string prefabName) =>
        !string.IsNullOrEmpty(prefabName) && _hashByName.ContainsKey(prefabName);

    /// <summary>
    /// The stock prefab this name's hash would actually reach on the client, or
    /// null when the hash reaches nothing.
    ///
    /// Equal to <paramref name="prefabName"/> in the ordinary case. Anything
    /// else is a collision: the client would build that other prefab.
    /// </summary>
    public string? ResolvesTo(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
            return null;
        return _nameByHash.TryGetValue(prefabName.GetStableHashCode(), out string found) ? found : null;
    }

    /// <summary>
    /// Parse the shipped snapshot.
    ///
    /// The format is deliberately flat text — <c>name TAB hash</c>, one per
    /// line, after a <c>#buildid</c> header — so that a diff of two snapshots is
    /// readable and a malformed line is a line rather than a file.
    /// </summary>
    /// <exception cref="FormatException">
    /// A line that is not a name and a hash. A registry that silently dropped
    /// unparsable lines would shrink without saying so, and a smaller registry
    /// blocks templates that are fine.
    /// </exception>
    public static StockPrefabRegistry Parse(string text, string source = "embedded")
    {
        if (text == null) throw new ArgumentNullException(nameof(text));

        var hashByName = new Dictionary<string, int>(StringComparer.Ordinal);
        var nameByHash = new Dictionary<int, string>();
        string buildId = "";
        int lineNumber = 0;

        foreach (string raw in text.Split('\n'))
        {
            lineNumber++;
            string line = raw.Trim('\r', ' ', '\t');
            if (line.Length == 0)
                continue;
            if (line[0] == '#')
            {
                const string marker = "#buildid";
                if (line.StartsWith(marker, StringComparison.Ordinal))
                    buildId = line.Substring(marker.Length).Trim();
                continue;
            }

            int tab = line.IndexOf('\t');
            if (tab <= 0)
                throw new FormatException($"stock registry line {lineNumber}: expected 'name<TAB>hash', got '{line}'");

            string name = line.Substring(0, tab);
            string hashText = line.Substring(tab + 1).Trim();
            if (!int.TryParse(hashText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int hash))
                throw new FormatException($"stock registry line {lineNumber}: '{hashText}' is not a hash");

            // A duplicate name is the snapshot contradicting itself, and which
            // of the two entries won would decide verdicts silently.
            if (hashByName.ContainsKey(name))
                throw new FormatException($"stock registry line {lineNumber}: '{name}' appears twice");

            hashByName[name] = hash;
            // A hash two stock names share is a fact about the game, not an
            // error in the snapshot; the first name keeps it and the collision
            // surfaces through ResolvesTo.
            if (!nameByHash.ContainsKey(hash))
                nameByHash[hash] = name;
        }

        return new StockPrefabRegistry(buildId, source, hashByName, nameByHash);
    }

    /// <summary>
    /// Build a registry from names alone, hashing each. For tests and for a
    /// caller that has a name list rather than a snapshot.
    /// </summary>
    public static StockPrefabRegistry FromNames(IEnumerable<string> names, string buildId = "test", string source = "names")
    {
        if (names == null) throw new ArgumentNullException(nameof(names));

        var hashByName = new Dictionary<string, int>(StringComparer.Ordinal);
        var nameByHash = new Dictionary<int, string>();
        foreach (string name in names)
        {
            if (string.IsNullOrEmpty(name) || hashByName.ContainsKey(name))
                continue;
            int hash = name.GetStableHashCode();
            hashByName[name] = hash;
            if (!nameByHash.ContainsKey(hash))
                nameByHash[hash] = name;
        }
        return new StockPrefabRegistry(buildId, source, hashByName, nameByHash);
    }
}
