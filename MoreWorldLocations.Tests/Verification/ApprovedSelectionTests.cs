using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The one place that says which templates a server-only world registers.
///
/// The failure this exists to prevent is a set of names maintained in three
/// places — the auditor, the registration and the documentation — drifting
/// apart quietly, with the world following whichever the code happened to read.
/// The second failure is subtler: an approval outliving the template it was
/// granted for, so a build serves content nobody checked on evidence about
/// content that no longer exists.
/// </summary>
public class ApprovedSelectionTests
{
    private static TemplateEvaluation Passing(string name = "MWL_Example1") =>
        Fixtures.Evaluate(Fixtures.Compatible(name));

    private static TemplateEvaluation Failing(string name = "MWL_Example1") =>
        Fixtures.Evaluate(Fixtures.Compatible(name,
            children: new[] { Fixtures.Networked("MWL_Shrine", "Root/Shrine") }));

    private static ApprovedSelection Selection(params (string Name, string Fingerprint)[] entries) =>
        ApprovedSelection.Parse(ApprovedSelection.Render(
            entries.Select(e => new KeyValuePair<string, string>(e.Name, e.Fingerprint)),
            policyFingerprint: "policy-1", generatedFrom: "a test"));

    [Fact]
    public void AnApprovedUnchangedTemplateThatStillPassesIsRegistered()
    {
        ApprovedSelection selection = Selection(("MWL_Example1", "content-1"));

        SelectionDecision decision = selection.Decide("MWL_Example1", Passing(), "content-1", "policy-1");

        Assert.Equal(SelectionOutcome.Registered, decision.Outcome);
        Assert.True(decision.Registered);
    }

    [Fact]
    public void ANameTheSelectionDoesNotCarryIsNotRegistered()
    {
        ApprovedSelection selection = Selection(("MWL_Example1", "content-1"));

        SelectionDecision decision = selection.Decide("MWL_Other1", Passing("MWL_Other1"), "content-9", "policy-1");

        Assert.Equal(SelectionOutcome.NotSelected, decision.Outcome);
    }

    [Fact]
    public void ATemplateThatChangedSinceItWasApprovedIsNotCarriedOver()
    {
        ApprovedSelection selection = Selection(("MWL_Example1", "content-1"));

        SelectionDecision decision = selection.Decide("MWL_Example1", Passing(), "content-2", "policy-1");

        Assert.Equal(SelectionOutcome.ContentDrift, decision.Outcome);
        // And it says which, because "missing" and "changed and waiting to be
        // re-checked" are different problems with different next steps.
        Assert.Contains("content changed since it was audited", decision.Reason);
    }

    [Fact]
    public void AnApprovalGrantedByRulesThisBuildNoLongerAppliesIsNotCarriedOver()
    {
        ApprovedSelection selection = Selection(("MWL_Example1", "content-1"));

        SelectionDecision decision = selection.Decide("MWL_Example1", Passing(), "content-1", "policy-2");

        Assert.Equal(SelectionOutcome.PolicyDrift, decision.Outcome);
    }

    [Fact]
    public void TheShippedFileRecordsAnAuditAndDoesNotReplaceOne()
    {
        // The runtime guard. Without it the file would be a licence to skip the
        // check, and a template that changed in a way the fingerprint somehow
        // missed would sail through on its name alone.
        ApprovedSelection selection = Selection(("MWL_Example1", "content-1"));

        SelectionDecision decision = selection.Decide("MWL_Example1", Failing(), "content-1", "policy-1");

        Assert.Equal(SelectionOutcome.RuntimeDisagrees, decision.Outcome);
        Assert.Contains(FindingCodes.UnknownPrefab, decision.Reason);
    }

    // ---- the transitional entries ------------------------------------------

    [Fact]
    public void WildcardApprovalsAreRejected()
    {
        Assert.Throws<FormatException>(() => ApprovedSelection.Parse("#policy p\nMWL_A\t*\n"));
    }

    [Fact]
    public void RenderAndParseRoundTrip()
    {
        ApprovedSelection selection = Selection(("MWL_B", "f-b"), ("MWL_A", "f-a"));

        Assert.Equal(2, selection.Count);
        Assert.Equal("f-a", selection.FingerprintOf("MWL_A"));
        Assert.Equal("policy-1", selection.PolicyFingerprint);
        Assert.Equal("a test", selection.GeneratedFrom);
    }

    [Fact]
    public void TwoRunsOverTheSameCatalogueProduceTheSameBytes()
    {
        // So that a diff between releases means the selection actually changed,
        // rather than a dictionary iterating differently.
        var entries = new[]
        {
            new KeyValuePair<string, string>("MWL_C", "f-c"),
            new KeyValuePair<string, string>("MWL_A", "f-a"),
            new KeyValuePair<string, string>("MWL_B", "f-b"),
        };
        string first = ApprovedSelection.Render(entries, "p", "run");
        string second = ApprovedSelection.Render(entries.Reverse(), "p", "run");

        Assert.Equal(first, second);
        Assert.True(first.IndexOf("MWL_A", StringComparison.Ordinal) < first.IndexOf("MWL_B", StringComparison.Ordinal));
    }

    [Fact]
    public void AMalformedOrDuplicatedLineIsRefusedRatherThanDropped()
    {
        // A selection that quietly lost a line would register a different world
        // from the one the file describes.
        Assert.Throws<FormatException>(() => ApprovedSelection.Parse("#policy p\nMWL_A\n"));
        Assert.Throws<FormatException>(() => ApprovedSelection.Parse("#policy p\nMWL_A\t\n"));
        Assert.Throws<FormatException>(() => ApprovedSelection.Parse("#policy p\nMWL_A\tf1\nMWL_A\tf2\n"));
    }

    [Fact]
    public void AnEmptySelectionApprovesNothingRatherThanEverything()
    {
        Assert.Equal(0, ApprovedSelection.Empty.Count);
        Assert.Equal(SelectionOutcome.NotSelected,
            ApprovedSelection.Empty.Decide("MWL_Example1", Passing(), "c", "p").Outcome);
    }

    [Fact]
    public void NoEmbeddedApprovalListCanOverrideTheRuntimeValidator()
    {
        Assert.Null(VerificationData.ReadResource("MoreWorldLocations.ServerOnly.ApprovedSelection.tsv"));
        Assert.Empty(VerificationData.LoadProblems);
    }
}
