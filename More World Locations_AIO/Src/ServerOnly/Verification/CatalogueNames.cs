using System;
using System.Collections.Generic;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>One name the catalogue is known to contain, as the input recorded it.</summary>
public readonly struct CatalogueName
{
    public CatalogueName(string name, string pack, bool declaredInSource)
    {
        Name = name ?? "";
        Pack = pack ?? "";
        DeclaredInSource = declaredInSource;
    }

    public string Name { get; }
    public string Pack { get; }

    /// <summary>
    /// What the INPUT recorded. Never treated as a runtime fact: the sweep
    /// decides for itself whether a definition exists, and a disagreement
    /// between the two is a finding rather than a correction.
    /// </summary>
    public bool DeclaredInSource { get; }
}

/// <summary>
/// The names a runtime report has to account for.
///
/// <para>Without this the report can only cover what the definitions happen to
/// declare, which means an asset that exists and is placed nowhere simply never
/// appears — and "nothing places it" is a real answer that somebody needs. It
/// is also what lets the report be reconciled by identity instead of by a
/// total: 194 rows and 194 names are different claims, and only one of them is
/// worth anything.</para>
///
/// <para>Names are compared exactly. <c>MWL_MaypoleHut1</c> and
/// <c>MWL_MayPoleHut1</c> are two lookups, and folding them would hide the fact
/// that one of them is a registration failure in the mod today.</para>
/// </summary>
public sealed class CatalogueNames
{
    private readonly List<CatalogueName> _names;
    private readonly Dictionary<string, CatalogueName> _byName;

    private CatalogueNames(List<CatalogueName> names, string source)
    {
        _names = names;
        _byName = new Dictionary<string, CatalogueName>(StringComparer.Ordinal);
        foreach (CatalogueName name in names)
            _byName[name.Name] = name;
        Source = source;
    }

    /// <summary>The input this list was taken from, for the report's provenance.</summary>
    public string Source { get; }

    public IReadOnlyList<CatalogueName> All => _names;
    public int Count => _names.Count;

    public static CatalogueNames Empty { get; } = new CatalogueNames(new List<CatalogueName>(), "none");

    public bool TryGet(string name, out CatalogueName found) =>
        _byName.TryGetValue(name ?? "", out found);

    /// <exception cref="FormatException">A malformed line. A list that quietly dropped one would under-report the catalogue and call it complete.</exception>
    public static CatalogueNames Parse(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));

        var names = new List<CatalogueName>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        string source = "";
        int lineNumber = 0;

        foreach (string raw in text.Split('\n'))
        {
            lineNumber++;
            string line = raw.Trim('\r', ' ', '\t');
            if (line.Length == 0)
                continue;
            if (line[0] == '#')
            {
                if (line.StartsWith("#source", StringComparison.Ordinal))
                    source = line.Substring("#source".Length).Trim();
                continue;
            }

            string[] parts = line.Split('\t');
            if (parts.Length != 3)
                throw new FormatException($"catalogue names line {lineNumber}: expected 'name<TAB>pack<TAB>declared', got '{line}'");
            if (!seen.Add(parts[0]))
                throw new FormatException($"catalogue names line {lineNumber}: '{parts[0]}' appears twice");

            names.Add(new CatalogueName(parts[0], parts[1], parts[2].Trim() == "1"));
        }

        return new CatalogueNames(names, source);
    }
}
