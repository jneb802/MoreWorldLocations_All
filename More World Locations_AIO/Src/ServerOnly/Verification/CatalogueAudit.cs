using System;
using System.Collections.Generic;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The sweep: resolve every template this world could place, judge it, and say
/// so.
///
/// <para><b>Why it runs in the game and not beside it.</b> An offline audit of
/// the bundles answers a question about the bundles. What a player meets is the
/// template after Jötunn has resolved it, and resolution adds objects, brings
/// components with them, and changes the terrain — one template gains a second
/// modifier. Anything that decides what ships has to look at the thing that
/// ships, which means it has to run where that thing exists. That is also what
/// makes this repeatable: a build added next month is judged by the same code,
/// in the same place, without anybody checking it by hand first.</para>
///
/// <para><b>What it does with a disagreement.</b> A registered template this
/// run judges incompatible is removed before locations are generated, and named
/// in the log. The shipped selection records that an audit passed; it is not a
/// licence to skip one, and the case it is guarding against — content that
/// changed under an approval — is exactly the case where the file is wrong and
/// the run is right.</para>
/// </summary>
public static class CatalogueAudit
{
    /// <summary>What the last sweep found, for the console commands. Null until one has run.</summary>
    public static CatalogueReport? Report { get; private set; }

    /// <summary>A new world sweeps again: the templates are reloaded and may resolve differently.</summary>
    internal static void Forget() => Report = null;

    /// <summary>
    /// Judge every location in <paramref name="definitions"/> and return the
    /// names to keep.
    /// </summary>
    /// <param name="definitions">
    /// Every name the catalogue declares, with its pack — including the ones
    /// this mode does not serve. A report that only covered what was registered
    /// could not answer "why is this location missing", which is the question
    /// anyone actually has.
    /// </param>
    /// <param name="factsOf">
    /// How to read one template's facts. Injected rather than called directly
    /// so that everything deciding anything here stays free of the engine: the
    /// walk needs Unity, the order the names are judged in and what happens
    /// when one of them fails do not.
    /// </param>
    public static CatalogueReport Run(
        IEnumerable<CatalogueSubject> definitions,
        Func<CatalogueSubject, TemplateFacts> factsOf,
        StockPrefabRegistry registry,
        ApprovedSelection selection,
        IReadOnlyCollection<string> excludedPacks,
        ComponentPolicy? components = null)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));
        if (factsOf == null) throw new ArgumentNullException(nameof(factsOf));
        if (registry == null) throw new ArgumentNullException(nameof(registry));
        if (selection == null) throw new ArgumentNullException(nameof(selection));
        if (excludedPacks == null) throw new ArgumentNullException(nameof(excludedPacks));

        components ??= ComponentPolicy.Default;
        string policyFingerprint = TemplateFingerprint.OfPolicy(registry, components, excludedPacks);
        var entries = new List<CatalogueEntry>();

        foreach (CatalogueSubject subject in definitions)
        {
            TemplateFacts facts;
            if (!subject.SourceDeclared)
            {
                // An asset with no definition places nothing anywhere. Resolving
                // it would answer a question nobody asked and load 200 MB to do
                // it.
                facts = TemplateFacts.MissingDefinition(subject.Name);
            }
            else if (IsExcluded(excludedPacks, subject.Pack))
            {
                // Same reasoning: a pack this mode does not serve is settled
                // before any template is opened.
                facts = new TemplateFacts(subject.Name, subject.Pack);
            }
            else
            {
                facts = Read(subject, factsOf);
            }

            TemplateEvaluation evaluation = TemplatePolicy.Evaluate(facts, registry, excludedPacks, components);
            string fingerprint = TemplateFingerprint.Of(facts);
            SelectionDecision decision = selection.Decide(subject.Name, evaluation, fingerprint, policyFingerprint);
            entries.Add(new CatalogueEntry(evaluation, decision, fingerprint));
        }

        Report = new CatalogueReport(entries, registry.GameBuildId, policyFingerprint);
        return Report;
    }

    /// <summary>
    /// One template's facts, with a resolution failure turned into a reason
    /// rather than an exception.
    ///
    /// One template's failure is one template's. Letting it out of here would
    /// stop the sweep and leave every name after it unjudged, which reads in the
    /// log exactly like a catalogue that got smaller.
    /// </summary>
    private static TemplateFacts Read(CatalogueSubject subject, Func<CatalogueSubject, TemplateFacts> factsOf)
    {
        try
        {
            return factsOf(subject) ?? TemplateFacts.Unreadable(
                subject.Name, subject.Pack, "the extractor returned nothing at all");
        }
        catch (Exception ex)
        {
            return TemplateFacts.Unreadable(subject.Name, subject.Pack,
                $"reading the template threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool IsExcluded(IReadOnlyCollection<string> excludedPacks, string pack)
    {
        foreach (string excluded in excludedPacks)
        {
            if (string.Equals(excluded, pack, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}

/// <summary>
/// One name to judge: what the catalogue declares about it, before anything is
/// loaded.
///
/// It carries <see cref="SourceDeclared"/> because an asset that exists with no
/// definition is a real entry in the catalogue with a real answer — "nothing
/// places it" — and folding it in with the names that failed a prefab check
/// would report a gap in the definitions as an incompatibility.
/// </summary>
public readonly struct CatalogueSubject
{
    public CatalogueSubject(string name, string pack, bool sourceDeclared = true,
        string interiorPrefabName = "", string dungeonTheme = "")
    {
        Name = name ?? "";
        Pack = pack ?? "";
        SourceDeclared = sourceDeclared;
        InteriorPrefabName = interiorPrefabName ?? "";
        DungeonTheme = dungeonTheme ?? "";
    }

    public string Name { get; }
    public string Pack { get; }
    public bool SourceDeclared { get; }
    public string InteriorPrefabName { get; }
    public string DungeonTheme { get; }
}
