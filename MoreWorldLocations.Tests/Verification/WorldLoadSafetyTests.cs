using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

public class WorldLoadSafetyTests
{
    private static IEnumerator Start(Action<CatalogueReport?> complete)
    {
        IEnumerator? routine = null;
        CatalogueSweep.ScheduleRoutine = r => { routine = r; return true; };
        Assert.True(CatalogueSweep.BeginAudit(complete));
        return routine!;
    }

    private static void Drive(IEnumerator routine)
    {
        for (int i = 0; i < 20000; i++)
        {
            Time.frameCount++;
            Time.realtimeSinceStartup += 0.02f;
            if (!routine.MoveNext()) return;
        }
        throw new Exception("scheduler exceeded the test's frame limit");
    }

    private static void FailAudit() => typeof(CatalogueSweep)
        .GetMethod("Conclude", BindingFlags.NonPublic | BindingFlags.Static)!
        .Invoke(null, new object?[]
        {
            CatalogueSweep.SweepState.Failed, null,
            (Action<CatalogueReport?>)(_ => throw new Exception("must not register an incomplete report")),
            "injected audit failure"
        });

    [Fact]
    public void FailedAuditKeepsTheSavedWorldAndSavingClosed()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        var gate = new WorldLoadGate();
        Assert.False(gate.TryBegin(CatalogueSweep.RegistrationReady, out _));
        int loads = 0;
        CatalogueSweep.ReleaseGeneration = () => loads++;
        Start(_ => { });
        FailAudit();
        Assert.Equal(0, loads);
        Assert.True(CatalogueSweep.HoldsGeneration);
        Assert.False(CatalogueSweep.RegistrationReady);
        Assert.False(gate.TryBegin(CatalogueSweep.RegistrationReady, out _));
        Assert.False(gate.CanSave);
        Assert.Contains("injected audit failure", CatalogueSweep.FailureReason);
    }

    [Fact]
    public void ThrowingRegistrationDoesNotReportDoneOrReleaseTheWorld()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        int loads = 0;
        CatalogueSweep.ReleaseGeneration = () => loads++;
        Drive(Start(_ => throw new InvalidOperationException("registration failed")));
        Assert.Equal(0, loads);
        Assert.Equal(CatalogueSweep.SweepState.Failed, CatalogueSweep.State);
        Assert.True(CatalogueSweep.HoldsGeneration);
        Assert.Contains("registration failed", CatalogueSweep.FailureReason);
    }

    [Fact]
    public void LateRegistrationFailureCanRetryTheActualRegistrationOnce()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        int loads = 0;
        var gate = new WorldLoadGate();
        gate.TryBegin(false, out _);
        CatalogueSweep.ReleaseGeneration = () =>
        {
            Assert.True(world.IsInWorld("MWL_Ruins1"));
            Assert.True(gate.TryBegin(CatalogueSweep.RegistrationReady, out int ticket));
            Assert.False(gate.CanSave);
            loads++;
            gate.Complete(ticket, false, null);
        };
        LocationDB.LateRegistration = name => throw new InvalidOperationException("late registration failed");
        LocationDB.RegisterAll();
        Assert.Equal(CatalogueSweep.SweepState.Failed, CatalogueSweep.State);
        Assert.False(gate.CanSave);
        Assert.Equal(0, loads);

        LocationDB.LateRegistration = null;
        Assert.True(CatalogueSweep.Resweep());
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.True(gate.CanSave);
        Assert.Equal(1, loads);
        Assert.Equal(ZoneSystem.instance!.m_locations.Count,
            ZoneSystem.instance.m_locations.Select(l => l.m_prefab.Name).Distinct().Count());
        LocationDB.RegisterAll(); // Repeated event does not load/register twice.
        Assert.Equal(1, loads);
    }

    [Fact]
    public void MissingLateRegistrationIsDetectedByNameEvenWithoutAnException()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        int loads = 0;
        CatalogueSweep.ReleaseGeneration = () => loads++;
        LocationDB.LateRegistration = name => ZoneSystem.instance!.m_locationsByHash.Remove(name.GetStableHashCode());
        LocationDB.RegisterAll();
        Assert.Equal(0, loads);
        Assert.False(CatalogueSweep.RegistrationReady);
        Assert.Contains("missing from the world's location map", CatalogueSweep.FailureReason);
    }

    [Fact]
    public void AuditFailureRetryRegistersRatherThanJustRechecking()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        Start(_ => { });
        FailAudit();
        CatalogueSweep.ScheduleRoutine = null;
        Assert.True(CatalogueSweep.Resweep());
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.True(world.IsInWorld("MWL_Ruins1"));
    }

    [Fact]
    public void FailedDiagnosticResweepKeepsTheCommittedWorld()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        LocationDB.RegisterAll();
        var original = ZoneSystem.instance!.m_locationsByHash["MWL_Ruins1".GetStableHashCode()];
        int loads = 0;
        CatalogueSweep.ReleaseGeneration = () => loads++;
        CatalogueSweep.ScheduleRoutine = _ => true;
        Assert.True(CatalogueSweep.Resweep());
        FailAudit();
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.False(CatalogueSweep.HoldsGeneration);
        Assert.True(CatalogueSweep.Enforce());
        Assert.Same(original, ZoneSystem.instance.m_locationsByHash["MWL_Ruins1".GetStableHashCode()]);
        Assert.Equal(0, loads);
    }

    [Fact]
    public void DiagnosticResultDoesNotWithdrawOrReloadAnAlreadyRegisteredLocation()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        LocationDB.RegisterAll();
        var original = ZoneSystem.instance!.m_locationsByHash["MWL_Ruins1".GetStableHashCode()];
        var changed = new GameObject("MWL_Ruins1");
        changed.AddComponent<TestOnlyClientBehaviour>();
        world.WithAsset("MWL_Ruins1", changed);
        int loads = 0;
        CatalogueSweep.ReleaseGeneration = () => loads++;
        Assert.True(CatalogueSweep.Resweep());
        Assert.False(CatalogueAudit.Report!.Find("MWL_Ruins1")!.Registered);
        Assert.True(CatalogueSweep.Enforce());
        Assert.Same(original, ZoneSystem.instance.m_locationsByHash["MWL_Ruins1".GetStableHashCode()]);
        Assert.Equal(0, loads);
    }

    [Fact]
    public void ACallbackFromTheOldWorldCannotReleaseItsSuccessor()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        int loads = 0;
        CatalogueSweep.ReleaseGeneration = () => loads++;
        Drive(Start(_ => CatalogueSweep.Forget()));
        Assert.Equal(0, loads);
        Assert.False(CatalogueSweep.RegistrationReady);
        Assert.Equal(CatalogueSweep.SweepState.Idle, CatalogueSweep.State);
    }

    [Fact]
    public void LoadingKeepsSavingBlockedUntilSuccessfulCompletion()
    {
        var gate = new WorldLoadGate();
        Assert.False(gate.CanSave);
        Assert.False(gate.TryBegin(false, out _));
        Assert.True(gate.Deferred);
        Assert.True(gate.TryBegin(true, out int ticket));
        Assert.False(gate.CanSave);
        Assert.False(gate.TryBegin(true, out _)); // no recursive or duplicate load
        gate.Complete(ticket, false, null);
        Assert.True(gate.CanSave);
        Assert.False(gate.Deferred);
        Assert.False(gate.TryBegin(true, out _)); // no reload after success
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "load threw")]
    public void AGameLoadErrorOrExceptionBlocksSavingUntilRestart(bool gameError, string? exception)
    {
        var gate = new WorldLoadGate();
        Assert.True(gate.TryBegin(true, out int ticket));
        gate.Complete(ticket, gameError, exception);
        Assert.Equal(WorldLoadGate.LoadState.Failed, gate.State);
        Assert.False(gate.CanSave);
        Assert.False(gate.TryBegin(true, out _)); // partial world is not safe to reload
        Assert.NotEmpty(gate.Failure);
        gate = new WorldLoadGate(); // a process restart, not a scene reset
        Assert.True(gate.TryBegin(true, out int fresh));
        gate.Complete(fresh, false, null);
        Assert.True(gate.CanSave);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "load threw")]
    public void SceneResetCannotClearATerminalLoadFailure(bool gameError, string? exception)
    {
        var gate = new WorldLoadGate();
        Assert.True(gate.TryBegin(true, out int ticket));
        gate.Complete(ticket, gameError, exception);
        string reason = gate.Failure;
        gate.Reset();
        gate.Reset();
        Assert.Equal(WorldLoadGate.LoadState.Failed, gate.State);
        Assert.Equal(reason, gate.Failure);
        Assert.False(gate.CanSave);
        Assert.False(gate.TryBegin(true, out _));
        gate.Complete(ticket, false, null); // late success cannot erase the failure
        Assert.False(gate.CanSave);
        Assert.Equal(reason, gate.Failure);
    }

    [Fact]
    public void AnOldOrSkippedLoadFinalizerCannotOpenSavingInTheNewWorld()
    {
        var gate = new WorldLoadGate();
        Assert.True(gate.TryBegin(true, out int old));
        gate.Reset();
        Assert.True(gate.TryBegin(true, out int current));
        gate.Complete(old, false, null);
        gate.Complete(-1, false, null);
        Assert.False(gate.CanSave);
        gate.Complete(current, false, null);
        Assert.True(gate.CanSave);
    }
}
