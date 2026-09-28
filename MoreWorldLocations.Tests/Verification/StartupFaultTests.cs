using System;
using System.Linq;
using System.Reflection;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

public sealed class StartupFaultTests : IDisposable
{
    private readonly StartupFaults _prior = StartupFaults.Current;
    private readonly WorldLoadGate _priorGate = GenerationHold.LoadGate;
    private delegate bool LoadPrefix(out int ticket);
    private static MethodInfo Hook(string patch, string method) => typeof(GenerationHold)
        .GetNestedType(patch, BindingFlags.NonPublic)!
        .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly LoadPrefix Begin = (LoadPrefix)Hook("LoadPatch", "Prefix")
        .CreateDelegate(typeof(LoadPrefix));
    private static readonly Func<int, Exception?, Exception?> Finish =
        (Func<int, Exception?, Exception?>)Hook("LoadPatch", "Finalizer")
        .CreateDelegate(typeof(Func<int, Exception?, Exception?>));
    private static readonly Func<ZNet, bool> Save = (Func<ZNet, bool>)Hook("SavePatch", "Prefix")
        .CreateDelegate(typeof(Func<ZNet, bool>));

    public StartupFaultTests()
    {
        StartupFaults.Current = new StartupFaults(null);
        GenerationHold.LoadGate = new WorldLoadGate();
        GenerationHold.Forget();
        ZNet.m_loadError = false;
    }
    public void Dispose()
    {
        StartupFaults.Current = _prior;
        GenerationHold.LoadGate = _priorGate;
        GenerationHold.Forget();
        ZNet.m_loadError = false;
    }

    // Execute the actual prefix and finalizer, preserving the out ticket even
    // when the prefix throws. Harmony's invocation of them is station-tested.
    private static Exception? Load(Action body)
    {
        int ticket = -1;
        Exception? error = null;
        try { if (Begin(out ticket)) body(); }
        catch (Exception ex) { error = ex; }
        return Finish(ticket, error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void UnsetFaultNeverChangesRegistrationOrLoading(string? mode)
    {
        var faults = new StartupFaults(mode);
        for (int i = 0; i < 2; i++)
        {
            faults.AfterRegistration("A", _ => throw new Exception("should not announce"));
            Assert.True(faults.BeforeLoad(() => throw new Exception("should not set error"), _ => throw new Exception()));
        }
    }

    [Theory]
    [InlineData("load-throw")]
    [InlineData("load-error")]
    public void LoadFaultOnlyFiresOnceAndSurvivesRegistrationChecks(string mode)
    {
        var faults = new StartupFaults(mode);
        faults.AfterRegistration("A", _ => throw new Exception());
        int errors = 0;
        Action<string> badLogger = _ => throw new Exception("logger failed");
        if (mode == "load-throw")
            Assert.Throws<InvalidOperationException>(() => faults.BeforeLoad(() => errors++, badLogger));
        else
            Assert.False(faults.BeforeLoad(() => errors++, badLogger));
        Assert.Equal(mode == "load-error" ? 1 : 0, errors);
        Assert.True(faults.BeforeLoad(() => errors++, _ => throw new Exception()));
    }

    [Fact]
    public void BadSwitchCannotSilentlyPassAValidationRun()
    {
        Assert.Throws<InvalidOperationException>(() => new StartupFaults("typo").BeforeLoad(() => { }, _ => { }));
    }

    [Fact]
    public void RegistrationFaultHappensAfterAnInsertionAndRetryCleansUpWithoutDuplicates()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        StartupFaults.Current = new StartupFaults("registration");
        string? inserted = null;
        LocationDB.LateRegistration = name =>
        {
            Assert.True(world.IsInWorld(name));
            inserted = name;
        };
        int loads = 0;
        CatalogueSweep.ReleaseGeneration = () => Assert.Null(Load(() => loads++));
        Assert.Null(Load(() => loads++)); // initial load is held
        LocationDB.RegisterAll();
        Assert.NotNull(inserted);
        Assert.Contains("ON PURPOSE", CatalogueSweep.FailureReason);
        Assert.False(CatalogueSweep.RegistrationReady);
        Assert.False(Save(new ZNet()));
        Assert.Equal(0, loads);
        Assert.True(CatalogueSweep.Resweep()); // one-shot is consumed, no reset or env edit
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.True(Save(new ZNet()));
        Assert.Equal(1, loads);
        Assert.Equal(ZoneSystem.instance!.m_locations.Count,
            ZoneSystem.instance.m_locations.Select(x => x.m_prefab.Name).Distinct().Count());
        Assert.Null(Load(() => loads++));
        Assert.Equal(1, loads);
    }

    [Theory]
    [InlineData("load-throw")]
    [InlineData("load-error")]
    public void RealLoadFinalizerBlocksSavesAndResweepCannotRetryFailedLoad(string mode)
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        LocationDB.RegisterAll();
        StartupFaults.Current = new StartupFaults(mode);
        int loads = 0;
        Exception? error = Load(() => loads++);
        if (mode == "load-throw") Assert.IsType<InvalidOperationException>(error);
        else { Assert.Null(error); Assert.True(ZNet.m_loadError); }
        Assert.Equal(0, loads);
        Assert.False(Save(new ZNet()));
        Assert.False(CatalogueSweep.Resweep());
        Assert.Null(Load(() => loads++));
        Assert.False(Save(new ZNet()));
        Assert.Equal(0, loads);
        GenerationHold.LoadGate = new WorldLoadGate(); // a new process, not a scene reset
        GenerationHold.Forget();
        ZNet.m_loadError = false;
        StartupFaults.Current = new StartupFaults(null);
        Assert.Null(Load(() => { Assert.False(Save(new ZNet())); loads++; }));
        Assert.Equal(1, loads);
        Assert.True(Save(new ZNet()));
    }

    [Theory]
    [InlineData("load-throw")]
    [InlineData("load-error")]
    public void SceneBounceCannotResweepOrReloadAfterTerminalFailure(string mode)
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        Action? previous = CatalogueSweep.NewWorld;
        CatalogueSweep.NewWorld = GenerationHold.Forget;
        try
        {
            LocationDB.RegisterAll();
            StartupFaults.Current = new StartupFaults(mode);
            int loads = 0;
            Load(() => loads++);
            int opened = world.Opened.Count;
            ZoneSystem.instance = new ZoneSystem();
            CatalogueSweep.Forget(); // real scene-reset callback chain
            ZNet.m_loadError = false; // even if vanilla clears its own error
            string reason = GenerationHold.LoadGate.Failure;
            Assert.True(GenerationHold.RestartRequired);
            Assert.Contains("restart required", GenerationHold.LoadStatus);
            Assert.False(CatalogueSweep.Resweep());
            Assert.False(CatalogueSweep.BeginAudit(_ => throw new Exception("must not register")));
            LocationDB.RegisterAll(); // direct registration cannot bypass the stop either
            Assert.True(CatalogueSweep.HoldsGeneration);
            Assert.Equal(reason, GenerationHold.LoadGate.Failure);
            Assert.Equal(opened, world.Opened.Count);
            Assert.Null(Load(() => loads++));
            Assert.False(Save(new ZNet()));
            Assert.False(CatalogueSweep.RegistrationReady);
            Assert.Equal(0, loads);
        }
        finally { CatalogueSweep.NewWorld = previous; }
    }

    [Fact]
    public void FullModeIgnoresArmedFaultsAndSaveGate()
    {
        using var world = new TemplateWorld();
        ServerOnlyMode.Set(false);
        StartupFaults.Current = new StartupFaults("load-throw");
        int loads = 0;
        Assert.Null(Load(() => loads++));
        Assert.Equal(1, loads);
        Assert.True(Save(new ZNet()));
    }

    [Fact]
    public void RegistrationFaultStillThrowsWhenTheAnnouncementThrows()
    {
        var faults = new StartupFaults("registration");
        Assert.True(faults.BeforeLoad(() => throw new Exception(), _ => throw new Exception()));
        Assert.Contains("ON PURPOSE", Assert.Throws<InvalidOperationException>(() =>
            faults.AfterRegistration("A", _ => throw new Exception())).Message);
        faults.AfterRegistration("B", _ => throw new Exception());
    }
}
