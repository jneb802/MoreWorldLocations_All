using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The verdict cache in the sweep: the registration sweep audits and stores,
/// the next start with the same inputs reuses without opening a single
/// template, and anything that changed — a key part, a stock prefab — audits
/// again. Diagnostic sweeps and resweeps never read, and the switch turns all
/// of it off.
///
/// <para>Driven synchronously (no coroutine host), with the installation's
/// parts and the live stock signer injected through the seams the engine half
/// fills in a real start.</para>
/// </summary>
public class AuditCacheSweepTests
{
    private sealed class CacheWorld : IDisposable
    {
        public readonly TemplateWorld World;
        public readonly GameObject Floor = Templates.StockPrefab("wood_floor");
        public readonly string Directory = Path.Combine(Path.GetTempPath(), "mwl-audit-cache-" + Guid.NewGuid().ToString("N"));
        public readonly string CachePath;
        public readonly List<string> Log = new List<string>();

        /// <summary>The installation's parts; a test changes one to change the key.</summary>
        public readonly Dictionary<string, string> Installation = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mwl-files"] = "mwl\n",
            ["mwl-config"] = "config\n",
            ["game"] = "game\n",
            ["loader"] = "loader\n",
        };

        /// <summary>Live signatures that differ from the world's own; a null value is a prefab that no longer resolves.</summary>
        public readonly Dictionary<string, string?> StockOverride = new Dictionary<string, string?>(StringComparer.Ordinal);

        private readonly string? _oldPath = Environment.GetEnvironmentVariable(ValidationSwitches.AuditCachePathVariable);
        private readonly string? _oldSwitch = Environment.GetEnvironmentVariable(ValidationSwitches.AuditCacheVariable);

        public CacheWorld(TemplateWorld? world = null)
        {
            World = (world ?? new TemplateWorld().WithPlainAssetsForEveryDefinition()).WithStock("wood_floor", Floor);
            CachePath = Path.Combine(Directory, "nested", "server-only-audit.txt");
            Environment.SetEnvironmentVariable(ValidationSwitches.AuditCachePathVariable, CachePath);
            Environment.SetEnvironmentVariable(ValidationSwitches.AuditCacheVariable, null);
            AuditCache.Installation = () => new Dictionary<string, string>(Installation, StringComparer.Ordinal);
            AuditCache.LiveStock = name => StockOverride.TryGetValue(name, out string? signature)
                ? signature
                : AuditCache.SignStock(name, TemplateAssets.StockPrefabs);
            AuditCache.DefaultPath = null;
            BepInEx.Logging.ManualLogSource.Captured = Log;
        }

        /// <summary>A registration sweep, as a start runs it, to completion.</summary>
        public CatalogueReport Sweep(bool diagnostic = false)
        {
            CatalogueReport? report = null;
            bool completed = false;
            Assert.True(CatalogueSweep.BeginAudit(r =>
            {
                report = r;
                completed = true;
            }, diagnostic));
            Assert.True(completed, "the sweep did not conclude synchronously");
            Assert.NotNull(report);
            return report!;
        }

        /// <summary>What a new start does before registration: a new world, nothing judged yet.</summary>
        public void Restart()
        {
            CatalogueSweep.Forget();
            Log.Clear();
        }

        public bool Logged(string fragment) => Log.Any(line => line.Contains(fragment));

        public string StoredKey() => File.ReadAllLines(CachePath)[1];

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.AuditCachePathVariable, _oldPath);
            Environment.SetEnvironmentVariable(ValidationSwitches.AuditCacheVariable, _oldSwitch);
            AuditCache.Installation = null;
            AuditCache.LiveStock = null;
            AuditCache.DefaultPath = null;
            AuditCache.Recording = null;
            BepInEx.Logging.ManualLogSource.Captured = null;
            World.Dispose();
            try { System.IO.Directory.Delete(Directory, recursive: true); } catch { }
        }
    }

    private static void AssertSameReport(CatalogueReport expected, CatalogueReport actual)
    {
        Assert.Equal(expected.StockBuildId, actual.StockBuildId);
        Assert.Equal(expected.PolicyFingerprint, actual.PolicyFingerprint);
        Assert.Equal(expected.Entries.Select(e => e.Name), actual.Entries.Select(e => e.Name));
        Assert.Equal(expected.Entries.Select(e => e.Decision.Outcome), actual.Entries.Select(e => e.Decision.Outcome));
        Assert.Equal(expected.Entries.Select(e => e.Decision.Reason), actual.Entries.Select(e => e.Decision.Reason));
        Assert.Equal(expected.Entries.Select(e => e.ContentFingerprint), actual.Entries.Select(e => e.ContentFingerprint));
        Assert.Equal(expected.Export(), actual.Export());
        Assert.Equal(expected.Summary(), actual.Summary());
    }

    [Fact]
    public void TheSecondStartReusesTheVerdictsWithoutOpeningATemplate()
    {
        using CacheWorld cache = new CacheWorld();
        int released = 0;
        CatalogueSweep.ReleaseGeneration = () => released++;

        CatalogueReport audited = cache.Sweep();

        Assert.NotEmpty(cache.World.Opened);
        Assert.True(File.Exists(cache.CachePath));
        Assert.True(cache.Logged("not reusing stored verdicts (there is no cache file at"), string.Join("\n", cache.Log));
        Assert.True(cache.Logged("catalogue audit: stored "), string.Join("\n", cache.Log));
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.Equal(1, released);

        cache.Restart();
        int opened = cache.World.Opened.Count;
        CatalogueReport reused = cache.Sweep();

        // The facts provider was never asked: not one template was opened.
        Assert.Equal(opened, cache.World.Opened.Count);
        Assert.True(cache.Logged($"catalogue audit: reused {audited.Entries.Count} verdicts from {cache.CachePath}"),
            string.Join("\n", cache.Log));
        AssertSameReport(audited, reused);
        Assert.Same(reused, CatalogueAudit.Report);
        Assert.Equal(CatalogueSweep.SweepState.Done, CatalogueSweep.State);
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.False(CatalogueSweep.HoldsGeneration);
        Assert.Equal(2, released);
        // Enforcement runs off the reused report exactly as off an audited one.
        Assert.True(CatalogueSweep.Enforce());
        // Nothing is left recording once the sweep is over.
        Assert.Null(AuditCache.Recording);
    }

    [Fact]
    public void ReuseRegistersTheSameLocationsAsTheAudit()
    {
        using CacheWorld cache = new CacheWorld();
        LocationDB.RegisterAll();
        string[] audited = cache.World.Opened.Distinct().Where(cache.World.IsInWorld).OrderBy(n => n).ToArray();
        Assert.NotEmpty(audited);

        cache.Restart();
        ZoneSystem.instance = new ZoneSystem();
        int opened = cache.World.Opened.Count;
        LocationDB.RegisterAll();

        Assert.Equal(opened, cache.World.Opened.Count);
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.Equal(audited, audited.Where(cache.World.IsInWorld).ToArray());
    }

    [Fact]
    public void AChangedKeyPartAuditsAgainAndRewrites()
    {
        using CacheWorld cache = new CacheWorld();
        cache.Sweep();
        string firstKey = cache.StoredKey();

        cache.Restart();
        cache.Installation["mwl-files"] = "mwl, a bundle changed\n";
        int opened = cache.World.Opened.Count;
        cache.Sweep();

        Assert.True(cache.World.Opened.Count > opened);
        Assert.True(cache.Logged("not reusing stored verdicts (inputs changed: mwl-files); auditing every template"),
            string.Join("\n", cache.Log));
        Assert.NotEqual(firstKey, cache.StoredKey());

        // And the rewritten file is the one the next start reuses.
        cache.Restart();
        opened = cache.World.Opened.Count;
        cache.Sweep();
        Assert.Equal(opened, cache.World.Opened.Count);
    }

    [Fact]
    public void AChangedSwitchIsAChangedKey()
    {
        using CacheWorld cache = new CacheWorld();
        cache.Sweep();
        string variable = ValidationSwitches.Prefix + "CACHE_TEST_SWITCH";
        try
        {
            cache.Restart();
            Environment.SetEnvironmentVariable(variable, "1");
            int opened = cache.World.Opened.Count;
            cache.Sweep();
            Assert.True(cache.World.Opened.Count > opened);
            Assert.True(cache.Logged("inputs changed: env"), string.Join("\n", cache.Log));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [Fact]
    public void AStockPrefabEditedInPlaceAuditsAgainAndIsNamed()
    {
        using CacheWorld cache = new CacheWorld();
        cache.Sweep();
        Assert.Contains("stock\twood_floor\t", File.ReadAllText(cache.CachePath));

        // What another mod editing a vanilla prefab looks like from here.
        cache.Floor.Child("added_by_another_mod");
        cache.Restart();
        int opened = cache.World.Opened.Count;
        cache.Sweep();

        Assert.True(cache.World.Opened.Count > opened);
        Assert.True(cache.Logged("not reusing stored verdicts (stock prefabs changed: wood_floor); auditing every template"),
            string.Join("\n", cache.Log));
    }

    [Fact]
    public void AStockPrefabThatNoLongerResolvesAuditsAgain()
    {
        using CacheWorld cache = new CacheWorld();
        cache.Sweep();

        cache.StockOverride["wood_floor"] = null;
        cache.Restart();
        int opened = cache.World.Opened.Count;
        cache.Sweep();

        Assert.True(cache.World.Opened.Count > opened);
        Assert.True(cache.Logged("(no longer resolve: wood_floor)"), string.Join("\n", cache.Log));
    }

    /// <summary>
    /// What the first real start hit: names that are not stock prefabs — snap
    /// points on pieces, objects inside templates — answered by whatever object
    /// of that name was loaded at the moment, so the store refused every time.
    /// </summary>
    [Fact]
    public void ObjectsThatComeAndGoWithLoadingNeitherBlockTheStoreNorAMatch()
    {
        TemplateWorld world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        GameObject withSnapPoints = Templates.Stock(LocationDB.All[0].Name);
        withSnapPoints.Child("$hud_snappoint_bottom 1").AddComponent<ZNetView>();
        world.WithAsset(LocationDB.All[0].Name, withSnapPoints);
        // A stock name whose lookup finds an object inside some template.
        GameObject elsewhere = Templates.Stock("MWL_OpenRightNow", child: "wood_floor");
        using CacheWorld cache = new CacheWorld(world);
        cache.World.WithStock("wood_floor", Templates.ChildOf(elsewhere, 0));
        Func<string, GameObject?> inner = TemplateAssets.StockPrefabs!;
        int loads = 0;
        TemplateAssets.StockPrefabs = name => name.StartsWith("$hud_snappoint", StringComparison.Ordinal)
            ? Templates.StockPrefab(name, "loaded " + loads++)
            : inner(name);

        cache.Sweep();

        Assert.True(cache.Logged("catalogue audit: stored "), string.Join("\n", cache.Log));
        string stored = File.ReadAllText(cache.CachePath);
        Assert.Contains("stock\t$hud_snappoint_bottom 1\tnot-stock\n", stored);
        Assert.Contains("stock\twood_floor\tnot-stock\n", stored);

        cache.Restart();
        int opened = cache.World.Opened.Count;
        cache.Sweep();
        Assert.Equal(opened, cache.World.Opened.Count);
        Assert.True(cache.Logged("reused"), string.Join("\n", cache.Log));
    }

    [Fact]
    public void ADiagnosticSweepNeitherReadsNorWrites()
    {
        using CacheWorld cache = new CacheWorld();
        cache.Sweep();
        string stored = File.ReadAllText(cache.CachePath);
        DateTime written = File.GetLastWriteTimeUtc(cache.CachePath);

        int opened = cache.World.Opened.Count;
        CatalogueSweep.Audit();
        Assert.True(cache.World.Opened.Count > opened);

        // Change a key part: a diagnostic sweep that wrote would store it.
        cache.Installation["game"] = "another game\n";
        opened = cache.World.Opened.Count;
        Assert.True(CatalogueSweep.Resweep());
        Assert.True(cache.World.Opened.Count > opened);

        Assert.Equal(stored, File.ReadAllText(cache.CachePath));
        Assert.Equal(written, File.GetLastWriteTimeUtc(cache.CachePath));
        Assert.False(cache.Logged("reused"));
    }

    [Fact]
    public void AResweepRetryingAFailedRegistrationAuditsAfresh()
    {
        using CacheWorld cache = new CacheWorld();
        LocationDB.RegisterAll();
        Assert.True(CatalogueSweep.RegistrationReady);

        // The next start reuses, and its registration fails.
        cache.Restart();
        ZoneSystem.instance = new ZoneSystem();
        LocationDB.LateRegistration = name => throw new InvalidOperationException("late registration failed");
        int opened = cache.World.Opened.Count;
        LocationDB.RegisterAll();
        Assert.Equal(opened, cache.World.Opened.Count);
        Assert.True(cache.Logged("reused"));
        Assert.Equal(CatalogueSweep.SweepState.Failed, CatalogueSweep.State);

        // The retry looks at the templates again rather than at the file.
        LocationDB.LateRegistration = null;
        Assert.True(CatalogueSweep.Resweep());
        Assert.True(cache.World.Opened.Count > opened);
        Assert.True(CatalogueSweep.RegistrationReady);
    }

    [Fact]
    public void OffMeansNeitherReadNorWritten()
    {
        using CacheWorld cache = new CacheWorld();
        Environment.SetEnvironmentVariable(ValidationSwitches.AuditCacheVariable, "OFF");

        cache.Sweep();
        Assert.False(File.Exists(cache.CachePath));
        Assert.True(cache.Logged("the verdict cache is off"));

        // A file from an earlier start is not read either.
        Environment.SetEnvironmentVariable(ValidationSwitches.AuditCacheVariable, null);
        cache.Restart();
        cache.Sweep();
        Assert.True(File.Exists(cache.CachePath));
        string stored = File.ReadAllText(cache.CachePath);

        Environment.SetEnvironmentVariable(ValidationSwitches.AuditCacheVariable, "off");
        cache.Restart();
        int opened = cache.World.Opened.Count;
        cache.Sweep();
        Assert.True(cache.World.Opened.Count > opened);
        Assert.Equal(stored, File.ReadAllText(cache.CachePath));
    }

    [Fact]
    public void AKeyThatCannotBeComputedAuditsAndStoresNothing()
    {
        using CacheWorld cache = new CacheWorld();
        AuditCache.Installation = () => throw new IOException("a bundle could not be read");

        CatalogueReport report = cache.Sweep();

        Assert.NotEmpty(cache.World.Opened);
        Assert.NotEmpty(report.Entries);
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.False(File.Exists(cache.CachePath));
        Assert.True(cache.Logged("the verdict cache key could not be computed (IOException: a bundle could not be read)"),
            string.Join("\n", cache.Log));
    }

    [Fact]
    public void AnUnresolvedVerdictIsNotStored()
    {
        TemplateWorld world = new TemplateWorld();
        foreach (MWLLocation location in LocationDB.All.Skip(1))
            world.WithAsset(location.Name, Templates.Stock(location.Name));
        using CacheWorld cache = new CacheWorld(world);

        CatalogueReport report = cache.Sweep();

        Assert.True(report.CountOf(TemplateVerdict.Unresolved) > 0);
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.False(File.Exists(cache.CachePath));
        Assert.True(cache.Logged("verdicts not stored:"), string.Join("\n", cache.Log));
    }

    [Fact]
    public void AnUnwritableCacheLogsAndTheServerCarriesOn()
    {
        using CacheWorld cache = new CacheWorld();
        string blocker = Path.Combine(cache.Directory, "a-file");
        Directory.CreateDirectory(cache.Directory);
        File.WriteAllText(blocker, "a file where a folder should be");
        Environment.SetEnvironmentVariable(ValidationSwitches.AuditCachePathVariable, Path.Combine(blocker, "cache.txt"));

        cache.Sweep();

        Assert.Equal(CatalogueSweep.SweepState.Done, CatalogueSweep.State);
        Assert.True(CatalogueSweep.RegistrationReady);
        Assert.True(cache.Logged("the verdicts could not be stored in"), string.Join("\n", cache.Log));
        Assert.Equal(new[] { blocker }, Directory.GetFiles(cache.Directory));
    }

    [Fact]
    public void WithoutAnInstallationThereIsNoCacheAndNoNoise()
    {
        using CacheWorld cache = new CacheWorld();
        AuditCache.Installation = null;

        cache.Sweep();

        Assert.False(File.Exists(cache.CachePath));
        Assert.False(cache.Logged("cache"), string.Join("\n", cache.Log));
    }
}
