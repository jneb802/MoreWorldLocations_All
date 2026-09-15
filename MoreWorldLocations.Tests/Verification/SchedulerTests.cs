using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The sweep spread over frames: nothing registered and nothing generated until
/// it concludes, one template open at a time, a released template settled
/// before the next is opened, and a world that goes away taking its sweep with
/// it.
///
/// <para>The frames are driven by hand here: the routine the engine would hand
/// to a coroutine host is captured and stepped, so each assertion is about a
/// state between two frames rather than about a timing.</para>
/// </summary>
public class SchedulerTests
{
    private static IEnumerator? s_routine;

    private static bool Capture(IEnumerator routine)
    {
        s_routine = routine;
        return true;
    }

    /// <summary>Advance the sweep by <paramref name="frames"/> frames, or to its end.</summary>
    private static void Drive(int frames = int.MaxValue)
    {
        for (int i = 0; i < frames && s_routine != null; i++)
        {
            UnityEngine.Time.frameCount++;
            UnityEngine.Time.realtimeSinceStartup += 0.02f;
            if (!s_routine.MoveNext())
                s_routine = null;
        }
    }

    private static TemplateWorld World()
    {
        s_routine = null;
        var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        CatalogueSweep.ScheduleRoutine = Capture;
        return world;
    }

    [Fact]
    public void TheStepperAndTheLoopAgree()
    {
        var subjects = new[]
        {
            new CatalogueSubject("MWL_C", "Meadows"),
            new CatalogueSubject("MWL_A", "Swamp"),
            new CatalogueSubject("MWL_Nowhere", "Meadows", sourceDeclared: false),
            new CatalogueSubject("MWL_Port", "Ports"),
        };
        Func<CatalogueSubject, TemplateFacts> factsOf = s => Fixtures.Compatible(s.Name, s.Pack);
        ApprovedSelection selection = ApprovedSelection.Parse(ApprovedSelection.Render(
            Array.Empty<KeyValuePair<string, string>>(), policyFingerprint: "", generatedFrom: "a test"));

        CatalogueReport looped = CatalogueAudit.Run(subjects, factsOf, Fixtures.Stock, selection, Fixtures.ExcludedPacks);
        CatalogueAudit.CatalogueAuditRun run = CatalogueAudit.Begin(subjects, factsOf, Fixtures.Stock, selection, Fixtures.ExcludedPacks);
        var opened = new List<bool>();
        while (!run.Done)
        {
            opened.Add(run.NextOpensATemplate);
            run.JudgeNext();
        }
        CatalogueReport stepped = run.Finish();

        Assert.Equal(looped.Entries.Select(e => e.Name), stepped.Entries.Select(e => e.Name));
        Assert.Equal(looped.Entries.Select(e => e.Evaluation.Verdict), stepped.Entries.Select(e => e.Evaluation.Verdict));
        Assert.Equal(looped.Entries.Select(e => e.ContentFingerprint), stepped.Entries.Select(e => e.ContentFingerprint));
        // Only names with a definition in a served pack cost a template.
        Assert.Equal(new[] { true, true, false, false }, opened);
    }

    [Fact]
    public void NothingIsRegisteredAndGenerationIsHeldUntilTheSweepConcludes()
    {
        using TemplateWorld world = World();
        bool released = false;
        CatalogueSweep.ReleaseGeneration = () => released = true;

        LocationDB.RegisterAll();

        // Started, not run: a frame has not passed yet.
        Assert.Equal(CatalogueSweep.SweepState.Auditing, CatalogueSweep.State);
        Assert.True(CatalogueSweep.HoldsGeneration);
        Assert.Empty(world.Opened);
        Assert.False(world.IsInWorld("MWL_Ruins1"));
        Assert.Contains("sweep Auditing 0/", CatalogueSweep.MemoryStatus());

        Drive(4);

        // Part way: templates have been opened and given back, and still
        // nothing is in the world and generation still waits.
        Assert.NotEmpty(world.Opened);
        Assert.Equal(world.Opened.Count, world.Released.Count);
        Assert.False(world.IsInWorld("MWL_Ruins1"));
        Assert.True(CatalogueSweep.HoldsGeneration);
        Assert.False(released);
        Assert.Null(CatalogueAudit.Report);

        Drive();

        Assert.Equal(CatalogueSweep.SweepState.Done, CatalogueSweep.State);
        Assert.False(CatalogueSweep.HoldsGeneration);
        Assert.True(released);
        Assert.NotNull(CatalogueAudit.Report);
        Assert.True(world.IsInWorld("MWL_Ruins1"));
        Assert.False(world.IsInWorld("Review_NewBuild"));
        // Ownership is the lease tests' business; here only that nothing is
        // left open. (The doubles' handle does not count itself, and the
        // global peak is whatever an earlier test left it at.)
        Assert.Equal(world.Opened.Count, world.Released.Count);
        Assert.Contains("sweep Done", CatalogueSweep.MemoryStatus());
    }

    [Fact]
    public void ANewWorldCancelsTheSweepAndRegistersNothing()
    {
        using TemplateWorld world = World();
        bool released = false;
        CatalogueSweep.ReleaseGeneration = () => released = true;
        LocationDB.RegisterAll();
        Drive(3);
        int openedSoFar = world.Opened.Count;

        // What ZoneSystem.Awake does for the next world.
        CatalogueSweep.Forget();
        Drive();

        Assert.Equal(openedSoFar, world.Opened.Count);
        Assert.Equal(world.Opened.Count, world.Released.Count);
        Assert.False(world.IsInWorld("MWL_Ruins1"));
        Assert.Null(CatalogueAudit.Report);
        // The abandoned sweep leaves the new world's state alone.
        Assert.Equal(CatalogueSweep.SweepState.Idle, CatalogueSweep.State);
        Assert.False(released);
    }

    [Fact]
    public void TheNextTemplateIsNotOpenedUntilThePreviousHasSettled()
    {
        using TemplateWorld world = World();
        int unsettledAnswersLeft = 0;
        TemplateAssets.IsSettled = name =>
        {
            if (unsettledAnswersLeft <= 0)
                return true;
            unsettledAnswersLeft--;
            return false;
        };
        LocationDB.RegisterAll();

        Drive(1);
        Assert.Single(world.Opened);

        unsettledAnswersLeft = 3;
        Drive(3);
        // Three frames of "not yet": nothing else opened.
        Assert.Single(world.Opened);
        Assert.Equal(0, unsettledAnswersLeft);

        Drive(1);
        Assert.Equal(2, world.Opened.Count);
    }

    [Fact]
    public void ASweepStillJudgingIsNotStartedAgain()
    {
        using TemplateWorld world = World();
        LocationDB.RegisterAll();
        Drive(2);

        Assert.False(CatalogueSweep.Resweep());
        Assert.Equal(CatalogueSweep.SweepState.Auditing, CatalogueSweep.State);

        Drive();
        Assert.Equal(CatalogueSweep.SweepState.Done, CatalogueSweep.State);
        Assert.True(CatalogueSweep.Resweep());
        Drive();
        Assert.Equal(CatalogueSweep.SweepState.Done, CatalogueSweep.State);
        Assert.True(world.IsInWorld("MWL_Ruins1"));
    }

    [Fact]
    public void ASettleWaitIsBoundedByFramesAndTime()
    {
        using TemplateWorld world = World();
        TemplateAssets.IsSettled = name => false;   // never settles
        LocationDB.RegisterAll();

        Drive(1);
        Assert.Single(world.Opened);
        Drive(CatalogueSweep.SettleTimeoutFrames + 2);

        // The bound, not the answer, moved the sweep on.
        Assert.True(world.Opened.Count >= 2);
    }
}


/// <summary>The validation switch registers a name for one process; enforcement must not take it back.</summary>
public class ValidationApprovalTests
{
    [Fact]
    public void ANameRequestedForValidationStaysInTheWorldThroughEnforcement()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, "Review_NewBuild");
        try
        {
            LocationDB.RegisterAll();

            Assert.True(world.IsInWorld("Review_NewBuild"));
            Assert.True(CatalogueSweep.Enforce());
            Assert.True(world.IsInWorld("Review_NewBuild"));
            // And it was judged like everything else, not waved through.
            Assert.NotNull(CatalogueAudit.Report!.Find("Review_NewBuild"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, null);
        }
    }
}
