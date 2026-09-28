using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// Embedded stock-prefab and catalogue-name snapshots. Neither grants location
/// approval: the current validator does that. Missing data is reported and
/// falls back to an empty snapshot, never an implicit location list.
/// </summary>
public static class VerificationData
{
    /// <summary>The resource name of the stock prefab snapshot. Set explicitly so the mod and the test build agree.</summary>
    public const string StockPrefabsResource = "MoreWorldLocations.ServerOnly.StockPrefabs.tsv";

    /// <summary>The resource name of the catalogue the report is reconciled against.</summary>
    public const string CatalogueNamesResource = "MoreWorldLocations.ServerOnly.CatalogueNames.tsv";

    private static StockPrefabRegistry? _stockPrefabs;
    private static CatalogueNames? _catalogueNames;
    private static readonly List<string> _loadProblems = new List<string>();

    /// <summary>Every networked prefab a client without the mod can build.</summary>
    public static StockPrefabRegistry StockPrefabs =>
        _stockPrefabs ??= Load(StockPrefabsResource,
            text => StockPrefabRegistry.Parse(text, StockPrefabsResource),
            StockPrefabRegistry.Empty);

    /// <summary>Every name a runtime report has to account for.</summary>
    public static CatalogueNames CatalogueNames =>
        _catalogueNames ??= Load(CatalogueNamesResource, CatalogueNames.Parse, CatalogueNames.Empty);

    /// <summary>
    /// What went wrong reading either file, so a run can say "the world is
    /// empty because the snapshot did not load" instead of leaving an operator
    /// to guess.
    /// </summary>
    public static IReadOnlyList<string> LoadProblems
    {
        get
        {
            // Touching both properties first: a problem nobody has read yet is
            // still a problem, and a caller asking for the list before asking
            // for the data would otherwise be told there were none.
            _ = StockPrefabs;
            _ = CatalogueNames;
            return _loadProblems;
        }
    }

    /// <summary>Read a resource as text, or null when this assembly does not carry it.</summary>
    public static string? ReadResource(string name)
    {
        Assembly assembly = typeof(VerificationData).Assembly;
        using Stream? stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
            return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static T Load<T>(string resource, Func<string, T> parse, T fallback)
    {
        string? text;
        try
        {
            text = ReadResource(resource);
        }
        catch (Exception ex)
        {
            _loadProblems.Add($"{resource} could not be read: {ex.Message}");
            return fallback;
        }

        if (text == null)
        {
            _loadProblems.Add($"{resource} is not embedded in this build.");
            return fallback;
        }

        try
        {
            return parse(text);
        }
        catch (Exception ex)
        {
            _loadProblems.Add($"{resource} is malformed: {ex.Message}");
            return fallback;
        }
    }
}
