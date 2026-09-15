using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The two files the verification ships with: what a stock client has, and
/// what the audit approved.
///
/// <para>Both are embedded resources rather than files beside the DLL, because
/// a server operator who deletes or edits one would change which locations
/// their world contains without changing the mod's version — and the first
/// symptom would be a player finding a hole where a building was. Embedded,
/// they travel with the build that was tested.</para>
///
/// <para><b>What happens when one is missing.</b> Nothing loud fails. The
/// registry falls back to empty, which makes every template unresolved rather
/// than blocked; the selection falls back to empty, which registers nothing.
/// Both defaults produce a smaller world and a clear reason, which is the
/// direction an unreadable audit should fail in.</para>
/// </summary>
public static class VerificationData
{
    /// <summary>The resource name of the stock prefab snapshot. Set explicitly so the mod and the test build agree.</summary>
    public const string StockPrefabsResource = "MoreWorldLocations.ServerOnly.StockPrefabs.tsv";

    /// <summary>The resource name of the generated selection.</summary>
    public const string ApprovedSelectionResource = "MoreWorldLocations.ServerOnly.ApprovedSelection.tsv";

    private static StockPrefabRegistry? _stockPrefabs;
    private static ApprovedSelection? _approvedSelection;
    private static readonly List<string> _loadProblems = new List<string>();

    /// <summary>Every networked prefab a client without the mod can build.</summary>
    public static StockPrefabRegistry StockPrefabs =>
        _stockPrefabs ??= Load(StockPrefabsResource,
            text => StockPrefabRegistry.Parse(text, StockPrefabsResource),
            StockPrefabRegistry.Empty);

    /// <summary>The templates the audit approved, and what it approved them as.</summary>
    public static ApprovedSelection ApprovedSelection =>
        _approvedSelection ??= Load(ApprovedSelectionResource,
            ApprovedSelection.Parse,
            ApprovedSelection.Empty);

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
            _ = ApprovedSelection;
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
