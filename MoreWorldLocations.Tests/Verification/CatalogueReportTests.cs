using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// What the audit says afterwards.
///
/// The question an operator actually has is "why is MWL_SwampTemple1 not in my
/// world", and a count of templates by verdict cannot answer it. These guard
/// the shapes that can: one name's reasons with the object paths intact, and a
/// summary that never adds a scope exclusion to a technical failure.
/// </summary>
public class CatalogueReportTests
{
    private static CatalogueEntry Entry(string name, string pack, TemplateFacts? facts = null,
        SelectionOutcome outcome = SelectionOutcome.Registered)
    {
        TemplateFacts f = facts ?? Fixtures.Compatible(name, pack);
        TemplateEvaluation evaluation = Fixtures.Evaluate(f);
        return new CatalogueEntry(evaluation,
            new SelectionDecision(name, outcome, "because"),
            TemplateFingerprint.Of(f));
    }

    private static CatalogueReport Report(params CatalogueEntry[] entries) =>
        new CatalogueReport(entries, stockBuildId: "25253764", policyFingerprint: "abcdef1234567890");

    [Fact]
    public void ScopeExclusionsAreCountedApartFromTechnicalFailures()
    {
        // Adding them together produces a number that means nothing: a trader
        // post missing because this mode ships no traders and a ruin missing
        // because a wall is a custom prefab are not the same news.
        CatalogueReport report = Report(
            Entry("MWL_Good1", "Meadows"),
            Entry("MWL_Bad1", "Meadows", Fixtures.Compatible("MWL_Bad1", "Meadows",
                children: new[] { Fixtures.Networked("MWL_Shrine", "Root/Shrine") }),
                SelectionOutcome.NotSelected),
            Entry("MWL_TraderPost1", "Traders", Fixtures.Compatible("MWL_TraderPost1", "Traders"),
                SelectionOutcome.NotSelected));

        Assert.Equal(1, report.CountOf(TemplateVerdict.Compatible));
        Assert.Equal(1, report.CountOf(TemplateVerdict.Blocked));
        Assert.Equal(1, report.CountOf(TemplateVerdict.ScopeExcluded));
        Assert.Equal(1, report.RegisteredCount);

        string summary = report.Summary();
        Assert.Contains("compatible 1, blocked 1", summary);
        Assert.Contains("scope-excluded 1", summary);
    }

    [Fact]
    public void AReasonIsCountedOncePerTemplateNotOncePerFinding()
    {
        // One unsupported kit piece used 135 times in one build is ONE template
        // to exclude. Counting findings would put it at the top of a list of
        // what to fix next when it is a single decision.
        TemplateFacts many = Fixtures.Compatible("MWL_Many1", "Meadows", children: new[]
        {
            Fixtures.Networked("MD_Kit_widestone", "Root/a"),
            Fixtures.Networked("MD_Kit_widestone", "Root/b"),
            Fixtures.Networked("MD_Kit_widestone", "Root/c"),
        });
        TemplateFacts one = Fixtures.Compatible("MWL_One1", "Meadows", children: new[]
        {
            Fixtures.Networked("MWL_Shrine", "Root/s"),
        });

        CatalogueReport report = Report(
            Entry("MWL_Many1", "Meadows", many, SelectionOutcome.NotSelected),
            Entry("MWL_One1", "Meadows", one, SelectionOutcome.NotSelected));

        KeyValuePair<string, int> unknown = report.BlockingReasons()
            .Single(r => r.Key == FindingCodes.UnknownPrefab);
        Assert.Equal(2, unknown.Value);
    }

    [Fact]
    public void ExplainNamesTheObjectAndNotJustTheCode()
    {
        CatalogueReport report = Report(
            Entry("MWL_Bad1", "Swamp", Fixtures.Compatible("MWL_Bad1", "Swamp",
                children: new[] { Fixtures.Networked("MWL_Shrine", "Root/Inner/Shrine") }),
                SelectionOutcome.NotSelected));

        string explanation = report.Explain("MWL_Bad1");

        Assert.Contains("verdict: blocked", explanation);
        Assert.Contains("Root/Inner/Shrine", explanation);
        Assert.Contains(FindingCodes.UnknownPrefab, explanation);
        Assert.Contains("registered: no", explanation);
    }

    [Fact]
    public void AskingAboutANameTheCatalogueDoesNotHaveSaysSoRatherThanNothing()
    {
        string answer = Report(Entry("MWL_Good1", "Meadows")).Explain("MWL_Maypolehut1");

        Assert.Contains("not in the catalogue", answer);
        Assert.Contains("case-sensitive", answer);
    }

    [Fact]
    public void ApprovedButNotRegisteredIsCalledOutBecauseItIsOtherwiseInvisible()
    {
        // The file says a location is in the world and the world does not have
        // it. Nothing else in the run would mention that.
        CatalogueReport report = Report(
            Entry("MWL_Good1", "Meadows"),
            Entry("MWL_Drifted1", "Meadows", null, SelectionOutcome.ContentDrift),
            Entry("MWL_NeverApproved1", "Meadows", null, SelectionOutcome.NotSelected));

        Assert.Equal(new[] { "MWL_Drifted1" }, report.ApprovedButNotRegistered());
        Assert.Contains("approved and NOT registered", report.Summary());
    }

    [Fact]
    public void TheExportKeepsOneRowPerFindingSoTheObjectPathsSurvive()
    {
        CatalogueReport report = Report(
            Entry("MWL_Bad1", "Meadows", Fixtures.Compatible("MWL_Bad1", "Meadows", children: new[]
            {
                Fixtures.Networked("MWL_Shrine", "Root/a"),
                Fixtures.Networked("MWL_Waystone", "Root/b"),
            }), SelectionOutcome.NotSelected));

        string[] rows = report.Export().Split('\n')
            .Where(line => line.StartsWith("MWL_Bad1", StringComparison.Ordinal)).ToArray();

        Assert.Equal(2, rows.Length);
        Assert.Contains(rows, r => r.Contains("Root/a"));
        Assert.Contains(rows, r => r.Contains("Root/b"));
    }

    [Fact]
    public void ATemplateWithNoFindingsStillGetsARowInTheExport()
    {
        // Otherwise the export answers "what is wrong" and not "what is in the
        // catalogue", and a name missing from it would be indistinguishable
        // from a name nobody evaluated.
        string export = Report(Entry("MWL_Good1", "Meadows")).Export();

        Assert.Contains("MWL_Good1\tMeadows\tcompatible\tyes", export);
    }

    [Fact]
    public void AFindingWithATabInItDoesNotBecomeTwoColumns()
    {
        var evaluation = new TemplateEvaluation("MWL_X", "Meadows", TemplateVerdict.Blocked, new[]
        {
            new TemplateFinding("code", FindingSeverity.Blocking, "Root/a\tb", "detail\nwith a break", "v"),
        });
        var report = new CatalogueReport(new[]
        {
            new CatalogueEntry(evaluation, new SelectionDecision("MWL_X", SelectionOutcome.NotSelected, "r"), "f"),
        });

        string[] lines = report.Export().TrimEnd('\n').Split('\n');
        string row = lines.Single(l => l.StartsWith("MWL_X", StringComparison.Ordinal));

        Assert.Equal(10, row.Split('\t').Length);
    }

    [Fact]
    public void TheGeneratedSelectionCarriesOnlyTheCompatibleNamesWithTheirOwnFingerprints()
    {
        // This is the generator: the approved set is produced from the
        // evaluation rather than transcribed from it by hand.
        TemplateFacts good = Fixtures.Compatible("MWL_Good1", "Meadows");
        CatalogueReport report = Report(
            Entry("MWL_Good1", "Meadows", good),
            Entry("MWL_Bad1", "Meadows", Fixtures.Compatible("MWL_Bad1", "Meadows",
                children: new[] { Fixtures.Networked("MWL_Shrine", "Root/s") }), SelectionOutcome.NotSelected),
            Entry("MWL_TraderPost1", "Traders", null, SelectionOutcome.NotSelected));

        ApprovedSelection generated = ApprovedSelection.Parse(report.RenderSelection("a test run"));

        Assert.Equal(new[] { "MWL_Good1" }, generated.Names);
        Assert.Equal(TemplateFingerprint.Of(good), generated.FingerprintOf("MWL_Good1"));
        Assert.Empty(generated.Unbound);
        Assert.Equal("abcdef1234567890", generated.PolicyFingerprint);
    }

    [Fact]
    public void ANameAppearingTwiceIsRefusedBecauseTheSecondWouldDecideTheReport()
    {
        Assert.Throws<ArgumentException>(() => Report(
            Entry("MWL_Good1", "Meadows"),
            Entry("MWL_Good1", "Meadows")));
    }

    [Fact]
    public void TheSummaryStampsWhatTheVerdictsWereReachedAgainst()
    {
        // A verdict without the game build and the rules behind it is a verdict
        // nobody can date.
        string summary = Report(Entry("MWL_Good1", "Meadows")).Summary();

        Assert.Contains("25253764", summary);
        Assert.Contains("abcdef12", summary);
    }
}
