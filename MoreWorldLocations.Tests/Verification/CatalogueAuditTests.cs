using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The sweep: every name judged once, in a stable order, with one template's
/// trouble kept to itself.
///
/// The failures here are the ones that make a gate look like it is working
/// while it is not — a name quietly skipped, an exception swallowing the rest
/// of the catalogue, an out-of-scope pack having its templates opened and
/// reported for faults nobody should act on.
/// </summary>
public class CatalogueAuditTests
{
    private static readonly IReadOnlyCollection<string> Excluded = Fixtures.ExcludedPacks;

    private static CatalogueReport Run(
        IEnumerable<CatalogueSubject> subjects,
        Func<CatalogueSubject, TemplateFacts> factsOf) =>
        CatalogueAudit.Run(subjects, factsOf, Fixtures.Stock, Excluded);

    [Fact]
    public void EveryNameGetsExactlyOneRowInTheOrderItWasDeclared()
    {
        // Aggregate-only success is the thing to avoid: "170 evaluated" is true
        // of a run that judged one name 170 times.
        var subjects = new[]
        {
            new CatalogueSubject("MWL_C", "Meadows"),
            new CatalogueSubject("MWL_A", "Swamp"),
            new CatalogueSubject("MWL_B", "Plains"),
        };

        CatalogueReport report = Run(subjects, s => Fixtures.Compatible(s.Name, s.Pack));

        Assert.Equal(new[] { "MWL_C", "MWL_A", "MWL_B" }, report.Entries.Select(e => e.Name));
    }

    [Fact]
    public void OneTemplateThrowingDoesNotCostTheRestTheirVerdicts()
    {
        // Letting it out would leave every name after it unjudged, which reads
        // in a log exactly like a catalogue that got smaller.
        var subjects = new[]
        {
            new CatalogueSubject("MWL_Good1", "Meadows"),
            new CatalogueSubject("MWL_Explodes", "Meadows"),
            new CatalogueSubject("MWL_Good2", "Meadows"),
        };

        CatalogueReport report = Run(subjects, s =>
            s.Name == "MWL_Explodes"
                ? throw new InvalidOperationException("the bundle is busy")
                : Fixtures.Compatible(s.Name, s.Pack));

        Assert.Equal(3, report.Entries.Count);
        Assert.Equal(TemplateVerdict.Compatible, report.Find("MWL_Good1")!.Evaluation.Verdict);
        Assert.Equal(TemplateVerdict.Compatible, report.Find("MWL_Good2")!.Evaluation.Verdict);

        // And the one that threw is unresolved, not blocked: the template said
        // nothing about itself, the run did.
        CatalogueEntry exploded = report.Find("MWL_Explodes")!;
        Assert.Equal(TemplateVerdict.Unresolved, exploded.Evaluation.Verdict);
        // The thrown message survives into a reason. A row saying only
        // "unresolved" would send whoever reads it back to a station log.
        Assert.Contains(exploded.Evaluation.Findings, f => f.Detail.Contains("the bundle is busy"));
    }

    [Fact]
    public void AnExtractorThatReturnsNothingIsUnresolvedRatherThanCompatible()
    {
        CatalogueReport report = Run(new[] { new CatalogueSubject("MWL_Null1", "Meadows") }, s => null!);

        Assert.Equal(TemplateVerdict.Unresolved, report.Find("MWL_Null1")!.Evaluation.Verdict);
    }

    [Fact]
    public void AnExcludedPacksTemplateIsNeverEvenOpened()
    {
        // Not an optimisation. Opening it would produce technical reasons for a
        // location that is out of scope whatever they say, and an operator
        // reading that a trader post was excluded for a custom component goes
        // and fixes the wrong thing.
        var opened = new List<string>();

        CatalogueReport report = Run(
            new[]
            {
                new CatalogueSubject("MWL_TraderPost1", "Traders"),
                new CatalogueSubject("MWL_Meadow1", "Meadows"),
            },
            s =>
            {
                opened.Add(s.Name);
                return Fixtures.Compatible(s.Name, s.Pack);
            });

        Assert.Equal(new[] { "MWL_Meadow1" }, opened);
        Assert.Equal(TemplateVerdict.ScopeExcluded, report.Find("MWL_TraderPost1")!.Evaluation.Verdict);
    }

    [Fact]
    public void AnAssetWithNoDefinitionIsNotOpenedEither()
    {
        // Nothing places it, so there is nothing to judge -- and resolving it
        // would load a template to answer a question nobody asked.
        var opened = new List<string>();

        CatalogueReport report = Run(
            new[] { new CatalogueSubject("MWL_SwampCastle1", "", sourceDeclared: false) },
            s =>
            {
                opened.Add(s.Name);
                return Fixtures.Compatible(s.Name, s.Pack);
            });

        Assert.Empty(opened);
        Assert.Equal(TemplateVerdict.MissingDefinition, report.Find("MWL_SwampCastle1")!.Evaluation.Verdict);
    }

    [Fact]
    public void ADefinitionsInteriorReachesThePolicy()
    {
        // The interior is a fact about the DEFINITION, not about the template,
        // so it has to be carried in rather than looked for in the hierarchy.
        CatalogueReport report = Run(
            new[] { new CatalogueSubject("BFD_Exterior", "BlackForest", dungeonTheme: "MWL_BlackForestDungeon") },
            s => new TemplateFacts(s.Name, s.Pack,
                children: new[] { Fixtures.Networked() },
                interiorPrefabName: s.InteriorPrefabName,
                dungeonTheme: s.DungeonTheme));

        Assert.True(report.Find("BFD_Exterior")!.Evaluation.Findings
            .Any(f => f.Code == FindingCodes.InteriorDungeon));
    }

    [Fact]
    public void EachVerdictIsBoundToTheFingerprintOfWhatWasActuallyJudged()
    {
        TemplateFacts facts = Fixtures.Compatible("MWL_Good1", "Meadows");

        CatalogueReport report = Run(new[] { new CatalogueSubject("MWL_Good1", "Meadows") }, s => facts);

        Assert.Equal(TemplateFingerprint.Of(facts), report.Find("MWL_Good1")!.ContentFingerprint);
    }

    [Fact]
    public void TheCurrentValidatorRejectsUnsupportedContent()
    {
        // Unsupported content is rejected on current facts, not on a stored name list.
        CatalogueReport report = Run(
            new[] { new CatalogueSubject("MWL_Changed1", "Meadows") },
            s => Fixtures.Compatible(s.Name, s.Pack,
                children: new[] { Fixtures.Networked("MWL_Shrine", "Root/Shrine") }));

        CatalogueEntry entry = report.Find("MWL_Changed1")!;
        Assert.Equal(SelectionOutcome.NotSelected, entry.Decision.Outcome);
        Assert.False(entry.Registered);
        Assert.Empty(report.ApprovedButNotRegistered());
        Assert.DoesNotContain("approved and NOT registered", report.Summary());
    }

    [Fact]
    public void ACompatibleApprovedTemplateIsRegistered()
    {
        CatalogueReport report = Run(
            new[] { new CatalogueSubject("MWL_Good1", "Meadows") },
            s => Fixtures.Compatible(s.Name, s.Pack));

        Assert.True(report.Find("MWL_Good1")!.Registered);
        Assert.Equal(1, report.RegisteredCount);
    }

    [Fact]
    public void ACompatibleTemplateNeedsNoPriorApprovalFile()
    {
        // The current verdict is sufficient; no historical name list is consulted.
        CatalogueReport report = Run(
            new[] { new CatalogueSubject("MWL_Good1", "Meadows") },
            s => Fixtures.Compatible(s.Name, s.Pack));

        Assert.True(report.Find("MWL_Good1")!.Registered);
        Assert.Equal(TemplateVerdict.Compatible, report.Find("MWL_Good1")!.Evaluation.Verdict);
    }

    [Fact]
    public void TheReportIsStampedWithTheBuildAndRulesItWasReachedUnder()
    {
        CatalogueReport report = Run(new[] { new CatalogueSubject("MWL_Good1", "Meadows") },
            s => Fixtures.Compatible(s.Name, s.Pack));

        Assert.Equal(Fixtures.Stock.GameBuildId, report.StockBuildId);
        Assert.Equal(
            TemplateFingerprint.OfPolicy(Fixtures.Stock, ComponentPolicy.Default, Excluded),
            report.PolicyFingerprint);
    }

    [Fact]
    public void AWithdrawnTemplateStopsBeingOneOfOurs()
    {
        // The terrain conversion asks this to tell an MWL location from a
        // vanilla one. A name the sweep took back must stop answering yes, or
        // the conversion would go on shaping ground for a site nothing places.
        ServerOnlySelection.SetRegistered(new[] { "MWL_Good1", "MWL_Withdrawn1" });
        Assert.True(ServerOnlySelection.IsOurs("MWL_Withdrawn1"));

        ServerOnlySelection.Forget("MWL_Withdrawn1");

        Assert.False(ServerOnlySelection.IsOurs("MWL_Withdrawn1"));
        Assert.True(ServerOnlySelection.IsOurs("MWL_Good1"));
    }
}
