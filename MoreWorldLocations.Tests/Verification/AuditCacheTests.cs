using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The verdict cache: that a stored report comes back as the same report, that
/// every key part and every recorded stock prefab can cause a miss and is
/// named when it does, that anything wrong with the file is a miss rather than
/// a partial reuse, and that the registration sweep reuses without opening a
/// template while a diagnostic sweep never touches the file.
/// </summary>
public class AuditCacheTests
{
    // ---------------------------------------------------------------- fixtures

    private static Dictionary<string, string> Parts() =>
        AuditCacheKey.ComponentNames.ToDictionary(name => name, name => $"{name}=base\n", StringComparer.Ordinal);

    private static AuditCacheKey Key(Dictionary<string, string>? parts = null) =>
        AuditCacheKey.FromCanonical(parts ?? Parts());

    private static CatalogueEntry Entry(
        string name, string pack, TemplateVerdict verdict, SelectionOutcome outcome, string reason, string fingerprint,
        params TemplateFinding[] findings) =>
        new CatalogueEntry(
            new TemplateEvaluation(name, pack, verdict, findings),
            new SelectionDecision(name, outcome, reason),
            fingerprint);

    /// <summary>Every verdict and outcome, and findings whose text tries to break a line.</summary>
    private static CatalogueReport SampleReport() => new CatalogueReport(new[]
    {
        Entry("MWL_Ruins1", "Meadows", TemplateVerdict.Compatible, SelectionOutcome.Registered,
            "approved by the current resolved-template validator", "0123456789abcdef"),
        Entry("MWL_Swamp\tTemple", "Swamp", TemplateVerdict.Blocked, SelectionOutcome.NotSelected,
            "current validator: Blocked; unknown_prefab, stock_baseline_provenance", "fedcba9876543210",
            new TemplateFinding(FindingCodes.UnknownPrefab, FindingSeverity.Blocking, "MWL_Swamp/Root\tWall",
                "tab\there, newline\nthere, carriage\rreturn, back\\slash, trailing \\", "Vines"),
            new TemplateFinding(FindingCodes.StockBaselineProvenance, FindingSeverity.Advisory, "",
                "ünïcødé ✓ 城堡 — and an escape lookalike \\t that is not one", "")),
        Entry("MWL_Mountain1", "Mountains", TemplateVerdict.Unresolved, SelectionOutcome.NotSelected,
            "current validator: Unresolved; stock_baseline_unavailable", "1111222233334444",
            new TemplateFinding(FindingCodes.StockBaselineUnavailable, FindingSeverity.Unresolving, "MWL_Mountain1/x", "no baseline", "x")),
        Entry("MWL_Port1", "Ports", TemplateVerdict.ScopeExcluded, SelectionOutcome.NotSelected, "", "",
            new TemplateFinding(FindingCodes.ScopeExcludedPack, FindingSeverity.Blocking, "", "out of scope", "Ports")),
        Entry("MWL_Nowhere", "", TemplateVerdict.MissingDefinition, SelectionOutcome.NotSelected, "", ""),
        Entry("MWL_Drift", "Plains", TemplateVerdict.Compatible, SelectionOutcome.ContentDrift, "drifted", "aa"),
        Entry("MWL_Rules", "Plains", TemplateVerdict.Compatible, SelectionOutcome.PolicyDrift, "rules", "bb"),
        Entry("MWL_Disagree", "Plains", TemplateVerdict.Blocked, SelectionOutcome.RuntimeDisagrees, "disagrees", "cc"),
    }, "build\t25390630\\x", "policy\nfingerprint");

    private static readonly IReadOnlyList<KeyValuePair<string, string>> SampleStock = new[]
    {
        new KeyValuePair<string, string>("Vines", StockRecord.NotStock),
        new KeyValuePair<string, string>("wood\tfloor", "ab12"),
    };

    /// <summary>The report as stored verdicts: each with an input of its own, the first using every sample stock name.</summary>
    private static List<StoredVerdict> Verdicts(CatalogueReport report, IReadOnlyList<KeyValuePair<string, string>>? stock = null)
    {
        List<string> all = (stock ?? SampleStock).Select(p => p.Key).OrderBy(n => n, StringComparer.Ordinal).ToList();
        return report.Entries.Select((e, i) => new StoredVerdict(e, "input-" + e.Name, i == 0 ? all : new List<string>())).ToList();
    }

    private static string Render(AuditCacheKey key, CatalogueReport report, IReadOnlyList<KeyValuePair<string, string>> stock) =>
        AuditCacheFile.Render(key, report.StockBuildId, report.PolicyFingerprint, Verdicts(report, stock), stock);

    private static CatalogueReport ReportOf(AuditCacheContents contents) =>
        new CatalogueReport(contents.Verdicts.Select(v => v.Entry), contents.StockBuildId, contents.PolicyFingerprint);

    private static void AssertSameReport(CatalogueReport expected, CatalogueReport actual)
    {
        Assert.Equal(expected.StockBuildId, actual.StockBuildId);
        Assert.Equal(expected.PolicyFingerprint, actual.PolicyFingerprint);
        Assert.Equal(expected.Entries.Count, actual.Entries.Count);
        for (int i = 0; i < expected.Entries.Count; i++)
        {
            CatalogueEntry e = expected.Entries[i];
            CatalogueEntry a = actual.Entries[i];
            Assert.Equal(e.Evaluation.Name, a.Evaluation.Name);
            Assert.Equal(e.Evaluation.Pack, a.Evaluation.Pack);
            Assert.Equal(e.Evaluation.Verdict, a.Evaluation.Verdict);
            Assert.Equal(e.Decision.Name, a.Decision.Name);
            Assert.Equal(e.Decision.Outcome, a.Decision.Outcome);
            Assert.Equal(e.Decision.Reason, a.Decision.Reason);
            Assert.Equal(e.ContentFingerprint, a.ContentFingerprint);
            Assert.Equal(e.Evaluation.Findings.Count, a.Evaluation.Findings.Count);
            for (int j = 0; j < e.Evaluation.Findings.Count; j++)
            {
                TemplateFinding ef = e.Evaluation.Findings[j];
                TemplateFinding af = a.Evaluation.Findings[j];
                Assert.Equal(ef.Code, af.Code);
                Assert.Equal(ef.Severity, af.Severity);
                Assert.Equal(ef.Path, af.Path);
                Assert.Equal(ef.Detail, af.Detail);
                Assert.Equal(ef.Value, af.Value);
            }
        }
        // The shapes an operator reads, as a whole.
        Assert.Equal(expected.Export(), actual.Export());
        Assert.Equal(expected.Summary(), actual.Summary());
    }

    /// <summary>Change the body of a rendered file and seal it again, so the change survives the digest.</summary>
    private static string Reseal(string text, Func<string, string> edit, string? counts = null)
    {
        int trailer = text.LastIndexOf("end\t", StringComparison.Ordinal);
        string body = edit(text.Substring(0, trailer));
        string[] old = text.Substring(trailer).TrimEnd('\n').Split('\t');
        return body + "end\t" + (counts ?? old[1] + "\t" + old[2]) + "\t" + AuditCacheCanonical.Sha256Hex(body) + "\n";
    }

    // ---------------------------------------------------------------- round trip

    [Fact]
    public void AReportRoundTripsExactlyAndInOrder()
    {
        CatalogueReport report = SampleReport();
        AuditCacheKey key = Key();
        string text = Render(key, report, SampleStock);

        AuditCacheLookup lookup = AuditCacheFile.Parse(text, key);

        Assert.True(lookup.Hit, lookup.Reason);
        AssertSameReport(report, ReportOf(lookup.Contents!));
        Assert.Equal(SampleStock, lookup.Contents!.Stock);
        Assert.Equal(report.Entries.Select(e => "input-" + e.Name), lookup.Contents.Verdicts.Select(v => v.Input));
        Assert.Equal(SampleStock.Select(p => p.Key).OrderBy(n => n, StringComparer.Ordinal), lookup.Contents.Verdicts[0].Uses);
        Assert.All(lookup.Contents.Verdicts.Skip(1), v => Assert.Empty(v.Uses));
        // One record per line: nothing in a free-text field made a line of its own.
        Assert.Equal(1 + 1 + key.Components.Count + 1 + SampleStock.Count + report.Entries.Count + SampleStock.Count
                     + report.Entries.Sum(e => e.Evaluation.Findings.Count) + 1,
            text.Split('\n').Length - 1);
    }

    [Fact]
    public void RenderingIsDeterministic()
    {
        Assert.Equal(Render(Key(), SampleReport(), SampleStock),
            Render(Key(), SampleReport(), SampleStock));
    }

    [Theory]
    [InlineData("")]
    [InlineData("plain")]
    [InlineData("\\")]
    [InlineData("a\tb\nc\rd\\e")]
    [InlineData("\\t literally")]
    public void EscapingIsReversible(string value)
    {
        string escaped = AuditCacheCanonical.Escape(value);
        Assert.DoesNotContain('\t', escaped);
        Assert.DoesNotContain('\n', escaped);
        Assert.DoesNotContain('\r', escaped);
        Assert.True(AuditCacheCanonical.TryUnescape(escaped, out string back));
        Assert.Equal(value, back);
    }

    [Theory]
    [InlineData("trailing\\")]
    [InlineData("\\x")]
    public void AnEscapeTheWriterNeverProducesIsRefused(string value)
    {
        Assert.False(AuditCacheCanonical.TryUnescape(value, out _));
    }

    // ---------------------------------------------------------------- the key

    [Fact]
    public void EveryPartChangesTheKeyAndAMissNamesThatPartAlone()
    {
        AuditCacheKey baseline = Key();
        string stored = Render(baseline, SampleReport(), SampleStock);

        foreach (string part in AuditCacheKey.ComponentNames)
        {
            Dictionary<string, string> changed = Parts();
            changed[part] += "x";
            AuditCacheKey other = Key(changed);

            Assert.NotEqual(baseline.Digest, other.Digest);
            AuditCacheLookup lookup = AuditCacheFile.Parse(stored, other);
            Assert.Equal(AuditCacheMiss.KeyChanged, lookup.Miss);
            Assert.Equal(new[] { part }, lookup.Changed);
            Assert.Equal("inputs changed: " + part, lookup.Reason);
        }
    }

    [Fact]
    public void TwoPartsChangedAreBothNamed()
    {
        string stored = Render(Key(), SampleReport(), SampleStock);
        Dictionary<string, string> changed = Parts();
        changed["game"] += "x";
        changed["env"] += "x";

        AuditCacheLookup lookup = AuditCacheFile.Parse(stored, Key(changed));

        Assert.Equal(new[] { "game", "env" }, lookup.Changed);
    }

    [Fact]
    public void AKeyWithAPartMissingOrUnknownIsRefused()
    {
        Dictionary<string, string> missing = Parts();
        missing.Remove("mwl-shared");
        Assert.Throws<ArgumentException>(() => AuditCacheKey.FromCanonical(missing));

        Dictionary<string, string> extra = Parts();
        extra["plugins"] = "x";
        Assert.Throws<ArgumentException>(() => AuditCacheKey.FromCanonical(extra));
    }

    [Fact]
    public void TheEnvironmentPartIsThisModesSwitchesExceptTheCachesOwn()
    {
        Hashtable variables = new Hashtable
        {
            ["MOREWORLDLOCATIONS_APPROVE"] = "MWL_A",
            ["MOREWORLDLOCATIONS_TRACE"] = "x\ty",
            ["PATH"] = "/usr/bin",
            [ValidationSwitches.AuditCacheVariable] = "on",
            [ValidationSwitches.AuditCachePathVariable] = "/tmp/x",
        };
        string text = AuditCacheCanonical.Environment(variables);

        Assert.Equal("MOREWORLDLOCATIONS_APPROVE=MWL_A\nMOREWORLDLOCATIONS_TRACE=x\\ty\n", text);

        variables[ValidationSwitches.AuditCachePathVariable] = "/elsewhere";
        variables.Remove(ValidationSwitches.AuditCacheVariable);
        Assert.Equal(text, AuditCacheCanonical.Environment(variables));
    }

    [Fact]
    public void ThePolicyPartSortsTheSubset()
    {
        Assert.Equal(AuditCacheCanonical.Policy("p", new[] { "B", "A" }), AuditCacheCanonical.Policy("p", new[] { "A", "B" }));
        Assert.NotEqual(AuditCacheCanonical.Policy("p", new[] { "A" }), AuditCacheCanonical.Policy("p", new[] { "A", "B" }));
    }

    [Fact]
    public void ATemplatesInputIsItsDefinitionAndItsOwnFiles()
    {
        CatalogueSubject plain = new CatalogueSubject("MWL_B", "Swamp");
        string input = AuditCacheCanonical.TemplateInput(plain, "bundle\tBundles/mwl_b\taa\n");

        Assert.Equal(input, AuditCacheCanonical.TemplateInput(new CatalogueSubject("MWL_B", "Swamp"), "bundle\tBundles/mwl_b\taa\n"));
        // Each field the audit reads, and the files, change it.
        Assert.NotEqual(input, AuditCacheCanonical.TemplateInput(new CatalogueSubject("MWL_B", "Plains"), "bundle\tBundles/mwl_b\taa\n"));
        Assert.NotEqual(input, AuditCacheCanonical.TemplateInput(new CatalogueSubject("MWL_B", "Swamp", interiorPrefabName: "Crypt"), "bundle\tBundles/mwl_b\taa\n"));
        Assert.NotEqual(input, AuditCacheCanonical.TemplateInput(new CatalogueSubject("MWL_B", "Swamp", dungeonTheme: "Cave"), "bundle\tBundles/mwl_b\taa\n"));
        Assert.NotEqual(input, AuditCacheCanonical.TemplateInput(new CatalogueSubject("MWL_B", "Swamp", sourceDeclared: false), "bundle\tBundles/mwl_b\taa\n"));
        Assert.NotEqual(input, AuditCacheCanonical.TemplateInput(plain, "bundle\tBundles/mwl_b\tbb\n"));
        Assert.NotEqual(input, AuditCacheCanonical.TemplateInput(plain, null));
    }

    [Fact]
    public void MwlsCacheVersionIsPartOfTheFormatPart()
    {
        Assert.Contains("mwl-cache=" + AuditCacheCanonical.MwlCacheVersion + "\n", AuditCacheCanonical.Format(AuditCacheFile.FormatVersion));
        Assert.NotEqual(AuditCacheCanonical.Format(3, 1), AuditCacheCanonical.Format(3, 2));
    }

    [Theory]
    [InlineData("warpalicious.More_World_Locations_AIO.cfg", true)]
    [InlineData("warpalicious.More_World_Locations_LootLists.yml", true)]
    [InlineData("warpalicious.More_World_Locations_Localization.English.yml", false)]
    [InlineData("warpalicious.More_World_Locations_Localization.French.yaml", false)]
    [InlineData("warpalicious.More_World_Locations_LocalizationExtras.yml", true)]
    [InlineData("sub/warpalicious.More_World_Locations_Extra.yaml", true)]
    [InlineData("warpalicious.More_World_Locations_AIO.json", false)]
    [InlineData("MWL_Ports/shipments.json", false)]
    [InlineData("com.jotunn.jotunn.cfg", false)]
    [InlineData("BepInEx.cfg", false)]
    [InlineData("other.mod.yml", false)]
    public void OnlyMwlsOwnSettingsArePartOfTheKey(string relative, bool mine)
    {
        Assert.Equal(mine, AuditCacheCanonical.IsMwlConfigFile(relative, "warpalicious.More_World_Locations_AIO"));
    }

    private const string Manifest =
        "SoftRef manifest - Text\nversion: 2\nbundles directory: ./Bundles\nbundle dependencies:\nasset locations:\n" +
        "- asset ID: 01\n  bundle: mwl_ruins1\n  path in bundle: Assets/MWL/Meadows/MWL_Ruins1.prefab\n" +
        "- asset ID: 02\n  bundle: mwl_tower1\n  path in bundle: Assets/MWL/Swamp/MWL_Tower1.prefab\n" +
        "- asset ID: 03\n  bundle: cd_room1\n  path in bundle: Assets/MWL/Rooms/CD_Room1.prefab\n";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedAssetInsideATemplateBundleMustInvalidateOtherConsumers(bool sharedFirst)
    {
        using Installation install = new Installation();
        // A valid mixed bundle: the ruins and a room used by another template.
        // Its manifest lines do not change when the room's authored content changes.
        string manifest = Manifest.Replace("bundle: cd_room1", "bundle: mwl_ruins1");
        if (sharedFirst)
        {
            int start = manifest.IndexOf("- asset ID: 03", StringComparison.Ordinal);
            string room = manifest.Substring(start);
            manifest = manifest.Substring(0, start).Replace("asset locations:\n", "asset locations:\n" + room);
        }
        install.Write("plugins/MWL/assetBundleManifest_full", manifest);
        AuditCacheKey before = install.Key();
        string? towerBefore = install.Files("MWL_Tower1");

        install.Write("plugins/MWL/Bundles/mwl_ruins1", "ruins unchanged; shared room changed");

        Assert.True(before.Digest != install.Key().Digest || towerBefore != install.Files("MWL_Tower1"),
            "The shared room changed, but neither the shared key nor its other consumer's input changed.");
    }

    [Fact]
    public void ChangingADependencyBundleInvalidatesConsumersWithoutAManifestEdit()
    {
        using Installation install = new Installation();
        install.Write("plugins/MWL/assetBundleManifest_full",
            Manifest.Replace("bundle dependencies:\n", "bundle dependencies:\n- mwl_tower1: mwl_ruins1\n"));
        AuditCacheKey before = install.Key();
        string? towerBefore = install.Files("MWL_Tower1");

        install.Write("plugins/MWL/Bundles/mwl_ruins1", "a dependency of the unchanged tower bundle, rebuilt");

        Assert.True(before.Digest != install.Key().Digest || towerBefore != install.Files("MWL_Tower1"),
            "Dependency bytes changed without changing the manifest; the consumer must be re-audited.");
    }

    /// <summary>A BepInEx installation on disk, as the engine half reads it.</summary>
    internal sealed class Installation : IDisposable
    {
        public readonly string Root = Path.Combine(Path.GetTempPath(), "mwl-install-" + Guid.NewGuid().ToString("N"));

        /// <summary>Beside the DLL, as in an ordinary install, unless a test moves it.</summary>
        public string? ManifestPath;

        public string Code = "code\n";

        public readonly string[] Names = { "MWL_Ruins1", "MWL_Tower1", "MWL_Unbundled" };

        public Installation()
        {
            Write("plugins/MWL/More_World_Locations_AIO.dll", "mwl dll");
            Write("plugins/MWL/More_World_Locations_AIO.pdb", "mwl symbols");
            Write("plugins/MWL/Bundles/mwl_ruins1", "ruins bundle");
            Write("plugins/MWL/Bundles/mwl_tower1", "tower bundle");
            Write("plugins/MWL/Bundles/cd_room1", "a dungeon room every dungeon shares");
            Write("plugins/MWL/assetBundleManifest_full", Manifest);
            Write("plugins/Other/Other.dll", "other dll");
            Write("plugins/Jotunn/Jotunn.dll", "jotunn");
            Write("core/BepInEx.dll", "bepinex");
            Write("Managed/assembly_valheim.dll", "game");
            Write("config/warpalicious.More_World_Locations_AIO.cfg", "mwl settings");
            Write("config/warpalicious.More_World_Locations_LootLists.yml", "loot");
            Write("config/other.plugin.cfg", "other settings");
            Write("config/other.plugin.output.json", "per boot");
            Write("patchers/Patcher.dll", "patcher");
            ManifestPath = Path.Combine(Root, "plugins/MWL/assetBundleManifest_full");
        }

        public void Write(string relative, string content)
        {
            string path = Path.Combine(Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public InstalledInputs Inputs() => AuditCacheCanonical.Installation(
            Path.Combine(Root, "plugins/MWL"), ManifestPath, Path.Combine(Root, "plugins/MWL/More_World_Locations_AIO.dll"),
            Names, Path.Combine(Root, "config"), "warpalicious.More_World_Locations_AIO",
            Path.Combine(Root, "Managed/assembly_valheim.dll"), "1.0.15", "40", true,
            Path.Combine(Root, "plugins/Jotunn/Jotunn.dll"), Path.Combine(Root, "core"), Code, null);

        public AuditCacheKey Key()
        {
            Dictionary<string, string> parts = Parts();
            foreach (KeyValuePair<string, string> part in Inputs().Parts)
                parts[part.Key] = part.Value;
            return AuditCacheKey.FromCanonical(parts);
        }

        public string? Files(string name) => Inputs().Templates.TryGetValue(name, out string? files) ? files : null;

        public void Dispose()
        {
            try { Directory.Delete(Root, recursive: true); } catch { }
        }
    }

    [Fact]
    public void AnUnrelatedPluginOrItsSettingsDoNotChangeTheKey()
    {
        using Installation install = new Installation();
        AuditCacheKey before = install.Key();

        install.Write("plugins/Other/Other.dll", "other dll, updated");
        install.Write("plugins/Newcomer/Newcomer.dll", "a new plugin");
        install.Write("config/other.plugin.cfg", "other settings, tweaked");
        install.Write("config/other.plugin.output.json", "written every boot");
        install.Write("patchers/Patcher.dll", "patcher, updated");

        Assert.Equal(before.Digest, install.Key().Digest);
    }

    [Theory]
    [InlineData("plugins/MWL/Bundles/cd_room1", "mwl-shared")]
    [InlineData("plugins/MWL/Bundles/new_bundle", "mwl-shared")]
    [InlineData("plugins/MWL/readme.txt", "mwl-shared")]
    [InlineData("config/warpalicious.More_World_Locations_AIO.cfg", "mwl-config")]
    [InlineData("config/warpalicious.More_World_Locations_LootLists.yml", "mwl-config")]
    [InlineData("plugins/Jotunn/Jotunn.dll", "loader")]
    [InlineData("core/BepInEx.dll", "loader")]
    [InlineData("core/0Harmony.dll", "loader")]
    [InlineData("Managed/assembly_valheim.dll", "game")]
    public void SharedMwlFilesItsSettingsTheLoaderAndTheGameDoChangeIt(string file, string part)
    {
        using Installation install = new Installation();
        AuditCacheKey before = install.Key();
        string stored = Render(before, SampleReport(), SampleStock);

        install.Write(file, "changed");

        AuditCacheLookup lookup = AuditCacheFile.Parse(stored, install.Key());
        Assert.Equal(AuditCacheMiss.KeyChanged, lookup.Miss);
        Assert.Equal(new[] { part }, lookup.Changed);
    }

    [Fact]
    public void ATemplatesOwnBundleChangesOnlyThatTemplatesInput()
    {
        using Installation install = new Installation();
        AuditCacheKey before = install.Key();
        string? ruins = install.Files("MWL_Ruins1");
        string? tower = install.Files("MWL_Tower1");
        Assert.NotNull(ruins);
        Assert.Contains("bundle\tBundles/mwl_ruins1\t", ruins);
        // A name no manifest asset carries has no files of its own.
        Assert.Null(install.Files("MWL_Unbundled"));

        install.Write("plugins/MWL/Bundles/mwl_ruins1", "ruins bundle, rebuilt");

        Assert.Equal(before.Digest, install.Key().Digest);
        Assert.NotEqual(ruins, install.Files("MWL_Ruins1"));
        Assert.Equal(tower, install.Files("MWL_Tower1"));
    }

    [Fact]
    public void ATemplatesManifestEntryIsItsInputAndEveryOtherAssetIsShared()
    {
        using Installation install = new Installation();
        AuditCacheKey before = install.Key();
        string? ruins = install.Files("MWL_Ruins1");

        install.Write("plugins/MWL/assetBundleManifest_full", Manifest.Replace("Meadows/MWL_Ruins1", "Plains/MWL_Ruins1"));
        Assert.Equal(before.Digest, install.Key().Digest);
        Assert.NotEqual(ruins, install.Files("MWL_Ruins1"));

        install.Write("plugins/MWL/assetBundleManifest_full", Manifest.Replace("Rooms/CD_Room1", "Rooms/CD_Room1b"));
        Assert.Equal(new[] { "mwl-shared" }, install.Key().Differences(before.Components));
    }

    [Fact]
    public void MwlsDllIsNotHashedAsAFileButItsCodeIs()
    {
        using Installation install = new Installation();
        AuditCacheKey before = install.Key();

        // A release: new DLL bytes, new symbols, same server-only code.
        install.Write("plugins/MWL/More_World_Locations_AIO.dll", "mwl dll 5.1.3");
        install.Write("plugins/MWL/More_World_Locations_AIO.pdb", "mwl symbols 5.1.3");
        Assert.Equal(before.Digest, install.Key().Digest);

        install.Code = "code, changed\n";
        Assert.Equal(new[] { "mwl-code" }, install.Key().Differences(before.Components));
    }

    [Theory]
    [InlineData("not a manifest at all")]
    [InlineData("SoftRef manifest - Text\nversion: 3\nbundles directory: ./Bundles\nbundle dependencies:\nasset locations:\n")]
    [InlineData("SoftRef manifest - Text\nversion: 2\nbundles directory: ./Bundles\nbundle dependencies:\nasset locations:\n- asset ID: 01\n  bundle: mwl_ruins1\n")]
    [InlineData("SoftRef manifest - Text\nversion: 2\nbundles directory: ./Bundles\nasset locations:\n")]
    [InlineData("SoftRef manifest - Text\nversion: 2\nbundles directory: ../elsewhere\nbundle dependencies:\nasset locations:\n")]
    public void AManifestTheSplitCannotBeSureOfMakesEveryBundleShared(string manifest)
    {
        using Installation install = new Installation();
        install.Write("plugins/MWL/assetBundleManifest_full", manifest);
        AuditCacheKey before = install.Key();

        Assert.Empty(install.Inputs().Templates);
        Assert.Contains("split\tnone\t", install.Inputs().Parts["mwl-shared"]);
        install.Write("plugins/MWL/Bundles/mwl_ruins1", "ruins bundle, rebuilt");
        Assert.Equal(new[] { "mwl-shared" }, install.Key().Differences(before.Components));
    }

    [Fact]
    public void ManifestDependenciesAreShared()
    {
        using Installation install = new Installation();
        AuditCacheKey before = install.Key();
        install.Write("plugins/MWL/assetBundleManifest_full",
            Manifest.Replace("bundle dependencies:\n", "bundle dependencies:\n- mwl_ruins1: cd_room1\n"));
        Assert.Equal(new[] { "mwl-shared" }, install.Key().Differences(before.Components));
    }

    [Fact]
    public void AManifestReadsStrictly()
    {
        Assert.True(SoftRefManifest.TryParse(Manifest, out SoftRefManifest? manifest, out string error), error);
        Assert.Equal("2", manifest!.Version);
        Assert.Equal("./Bundles", manifest.BundlesDirectory);
        Assert.Equal(new[] { "mwl_ruins1", "mwl_tower1", "cd_room1" }, manifest.Assets.Select(a => a.Bundle));
        Assert.Equal("Assets/MWL/Meadows/MWL_Ruins1.prefab", manifest.Assets[0].PathInBundle);
        Assert.True(SoftRefManifest.TryParse(Manifest.Replace("\n", "\r\n"), out _, out _));
        Assert.False(SoftRefManifest.TryParse(Manifest + "stray line\n", out _, out string stray));
        Assert.Contains("is not an asset location", stray);
    }

    [Fact]
    public void BundlesFoundOutsideMwlsFolderAreSharedAndHashed()
    {
        using Installation install = new Installation();
        install.Write("plugins/warpalicious-More_World_Locations_AIO/assetBundleManifest_full", Manifest);
        install.Write("plugins/warpalicious-More_World_Locations_AIO/Bundles/mwl_ruins1", "bundle");
        install.ManifestPath = Path.Combine(install.Root, "plugins/warpalicious-More_World_Locations_AIO/assetBundleManifest_full");
        AuditCacheKey before = install.Key();
        Assert.Empty(install.Inputs().Templates);

        install.Write("plugins/warpalicious-More_World_Locations_AIO/Bundles/mwl_ruins1", "bundle, changed");

        Assert.Equal(new[] { "mwl-shared" }, install.Key().Differences(before.Components));
    }

    [Fact]
    public void ATreeIsSortedRelativeAndTellsAbsentFromEmpty()
    {
        using Installation install = new Installation();
        FileTree mwl = AuditCacheCanonical.HashTree(Path.Combine(install.Root, "plugins/MWL"));
        Assert.Equal(new[] { "Bundles/cd_room1", "Bundles/mwl_ruins1", "Bundles/mwl_tower1", "More_World_Locations_AIO.dll",
                "More_World_Locations_AIO.pdb", "assetBundleManifest_full" },
            mwl.Files.Select(f => f.Key));

        Directory.CreateDirectory(Path.Combine(install.Root, "empty"));
        FileTree empty = AuditCacheCanonical.HashTree(Path.Combine(install.Root, "empty"));
        FileTree absent = AuditCacheCanonical.HashTree(Path.Combine(install.Root, "absent"));
        Assert.NotEqual(AuditCacheCanonical.Tree("x", empty), AuditCacheCanonical.Tree("x", absent));
    }

    [Fact]
    public void AnEmptyMwlFolderIsNotSomethingTheKeyCanVouchFor()
    {
        using Installation install = new Installation();
        Directory.CreateDirectory(Path.Combine(install.Root, "empty"));
        Assert.Throws<InvalidOperationException>(() => AuditCacheCanonical.Installation(
            Path.Combine(install.Root, "empty"), null, Path.Combine(install.Root, "empty/x.dll"), install.Names,
            Path.Combine(install.Root, "config"), "g",
            Path.Combine(install.Root, "Managed/assembly_valheim.dll"), "v", "n", true,
            Path.Combine(install.Root, "plugins/Jotunn/Jotunn.dll"), Path.Combine(install.Root, "core"), "code", null));
    }

    // ---------------------------------------------------------------- MWL's code

    /// <summary>Something whose code the canonical text can be checked against.</summary>
    private static class CodeSample
    {
        public const int Answer = 42;
        public static int Pick(int a, int b) => Math.Max(a, b) + "sample".Length;
    }

    [Fact]
    public void MwlsCodeIsReadByWhatItsTokensNameAndIsDeterministic()
    {
        System.Reflection.Assembly assembly = typeof(CodeSample).Assembly;
        Func<Type, bool> sample = t => t == typeof(CodeSample);
        string text = AuditCacheCode.Canonical(assembly, sample);

        Assert.Equal(text, AuditCacheCode.Canonical(assembly, sample));
        Assert.Contains("System.Math::Int32 Max(Int32, Int32)", text);
        Assert.Contains("\"sample\"", text);
        Assert.Contains("\tAnswer\tSystem.Int32\t", text);
        Assert.DoesNotContain("token:", text);
        Assert.Contains("method\tInt32 Pick(Int32, Int32)", text);
    }

    [Fact]
    public void IlRepacksMergeListIsABuildRecordNotData()
    {
        Assert.True(AuditCacheCode.IsBuildRecord("ILRepack.List"));
        Assert.False(AuditCacheCode.IsBuildRecord("MoreWorldLocations.ServerOnly.StockPrefabs.tsv"));
        Assert.False(AuditCacheCode.IsBuildRecord("More_World_Locations_AIO.assets.mockplaceholders"));
    }

    [Fact]
    public void TheServerOnlyScopeIsTheFeaturesNamespaceAndNoOther()
    {
        Assert.True(AuditCacheCode.InServerOnly(typeof(AuditCache)));
        Assert.True(AuditCacheCode.InServerOnly(typeof(ServerOnlyMode)));
        Assert.True(AuditCacheCode.InServerOnly(typeof(AuditCache.Attempt)));
        Assert.False(AuditCacheCode.InServerOnly(typeof(AuditCacheTests)));
        Assert.False(AuditCacheCode.InServerOnly(typeof(string)));

        // The feature's own code, in this build: resolves, and carries the cache version.
        string code = AuditCacheCode.Canonical(typeof(AuditCache).Assembly, AuditCacheCode.InServerOnly);
        Assert.Contains("type\tMore_World_Locations_AIO.ServerOnly.Verification.AuditCache\t", code);
        Assert.Contains("\tMwlCacheVersion\tSystem.Int32\t", code);
        Assert.DoesNotContain("type\tMore_World_Locations_AIO.Tests.", code);
        // No assembly's version: a generic's FullName names its arguments' assemblies, MWL's own included.
        Assert.DoesNotContain("Version=", code);
        Assert.DoesNotContain("PublicKeyToken", code);
    }

    // ---------------------------------------------------------------- failing closed

    [Fact]
    public void EverythingWrongWithTheFileIsAMissWithItsOwnReason()
    {
        AuditCacheKey key = Key();
        string good = Render(key, SampleReport(), SampleStock);
        string dir = Path.Combine(Path.GetTempPath(), "mwl-cache-" + Guid.NewGuid().ToString("N"));
        try
        {
            Dictionary<AuditCacheMiss, AuditCacheLookup> seen = new Dictionary<AuditCacheMiss, AuditCacheLookup>
            {
                [AuditCacheMiss.NoFile] = AuditCacheFile.Load(Path.Combine(dir, "none.txt"), key),
                [AuditCacheMiss.Magic] = AuditCacheFile.Parse("SOMETHING-ELSE\t1\n" + good.Substring(good.IndexOf('\n') + 1), key),
                [AuditCacheMiss.FormatVersion] = AuditCacheFile.Parse(
                    good.Replace(AuditCacheFile.Magic + "\t" + AuditCacheFile.FormatVersion + "\n",
                        AuditCacheFile.Magic + "\t" + (AuditCacheFile.FormatVersion - 1) + "\n"), key),
                [AuditCacheMiss.Truncated] = AuditCacheFile.Parse(good.Substring(0, good.Length / 2), key),
                [AuditCacheMiss.BodyDigest] = AuditCacheFile.Parse(good.Replace("\tBlocked\t", "\tCompatible\t"), key),
                [AuditCacheMiss.CountMismatch] = AuditCacheFile.Parse(Reseal(good, body => body, counts: "7\t2"), key),
                [AuditCacheMiss.KeyChanged] = AuditCacheFile.Parse(good, Key(new Dictionary<string, string>(Parts()) { ["game"] = "other" })),
                [AuditCacheMiss.Malformed] = AuditCacheFile.Parse(Reseal(good, body => body.Replace("\nentry\t", "\nentree\t")), key),
            };
            Directory.CreateDirectory(dir);
            // A valid header, then a byte sequence that is not UTF-8.
            File.WriteAllBytes(Path.Combine(dir, "bad.txt"),
                System.Text.Encoding.ASCII.GetBytes(AuditCacheFile.Magic + "\t" + AuditCacheFile.FormatVersion + "\n").Concat(new byte[] { 0xc3, 0x28, 0x0a }).ToArray());
            seen[AuditCacheMiss.Unreadable] = AuditCacheFile.Load(Path.Combine(dir, "bad.txt"), key);

            foreach (KeyValuePair<AuditCacheMiss, AuditCacheLookup> pair in seen)
            {
                Assert.False(pair.Value.Hit);
                Assert.Null(pair.Value.Contents);
                Assert.True(pair.Key == pair.Value.Miss, $"expected {pair.Key}, got {pair.Value.Miss}: {pair.Value.Reason}");
            }
            Assert.Equal(seen.Count, seen.Values.Select(l => l.Reason).Distinct().Count());
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { }
        }
    }

    [Theory]
    [InlineData("\tBlocked\t", "\tMaybe\t", "verdict")]
    [InlineData("\tRegistered\t", "\tPerhaps\t", "selection outcome")]
    [InlineData("\tAdvisory\t", "\tMild\t", "finding severity")]
    [InlineData("\tBlocked\t", "\t1\t", "verdict")]
    [InlineData("\tRegistered\t", "\tregistered\t", "selection outcome")]
    public void AValueThisBuildDoesNotKnowIsAMissEvenWhenTheFileIsSealed(string from, string to, string what)
    {
        AuditCacheKey key = Key();
        string text = Reseal(Render(key, SampleReport(), SampleStock), body => ReplaceFirst(body, from, to));

        AuditCacheLookup lookup = AuditCacheFile.Parse(text, key);

        Assert.Equal(AuditCacheMiss.UnknownValue, lookup.Miss);
        Assert.Contains(what, lookup.Reason);
    }

    [Fact]
    public void AnEntryMissingOneOfItsFindingsIsAMiss()
    {
        AuditCacheKey key = Key();
        string text = Reseal(Render(key, SampleReport(), SampleStock), body =>
        {
            int finding = body.IndexOf("\nfinding\t", StringComparison.Ordinal);
            int end = body.IndexOf('\n', finding + 1);
            return body.Remove(finding, end - finding);
        });

        Assert.Equal(AuditCacheMiss.Malformed, AuditCacheFile.Parse(text, key).Miss);
    }

    [Fact]
    public void AHeaderWhoseKeyDoesNotAddUpIsAMiss()
    {
        AuditCacheKey key = Key();
        string text = Reseal(Render(key, SampleReport(), SampleStock),
            body => body.Replace("key\t" + key.Digest, "key\t" + new string('0', 64)));

        Assert.Equal(AuditCacheMiss.Malformed, AuditCacheFile.Parse(text, key).Miss);
    }

    private static string ReplaceFirst(string text, string from, string to)
    {
        int at = text.IndexOf(from, StringComparison.Ordinal);
        Assert.True(at >= 0, $"'{from}' is not in the rendered file");
        return text.Substring(0, at) + to + text.Substring(at + from.Length);
    }

    // ---------------------------------------------------------------- the switch

    [Theory]
    [InlineData(null, false, true)]
    [InlineData("", false, true)]
    [InlineData("off", true, true)]
    [InlineData("OFF", true, true)]
    [InlineData(" 0 ", true, true)]
    [InlineData("False", true, true)]
    [InlineData("on", false, true)]
    [InlineData("1", false, true)]
    [InlineData("true", false, true)]
    [InlineData("of", true, false)]
    public void TheSwitchParsesAndAnythingUnknownIsOff(string? value, bool off, bool recognised)
    {
        Assert.Equal(off, ValidationSwitches.ParseAuditCacheOff(value, out bool known));
        Assert.Equal(recognised, known);
    }

    // ---------------------------------------------------------------- the stock record

    [Theory]
    [InlineData("JVLmock_stone_wall_2x1", "stone_wall_2x1", false)]
    [InlineData("JVLmock_JVLmock_stone_wall_2x1", "JVLmock_stone_wall_2x1", false)]
    [InlineData("JVLmock_Prefab__Child__Leaf", "Prefab", true)]
    [InlineData("VLmock_Thing", "Thing", false)]
    public void AMockNameIsRecordedByTheAssetItAsksFor(string name, string asset, bool childPath)
    {
        Assert.True(AuditCache.TryMockAsset(name, out string found, out bool child));
        Assert.Equal(asset, found);
        Assert.Equal(childPath, child);
        Assert.False(AuditCache.TryMockAsset("stone_wall_2x1", out _, out _));
    }

    [Fact]
    public void TheRecordKeepsWhatWasAskedForAndSignsOnlyStockNames()
    {
        Dictionary<string, string> live = new Dictionary<string, string>
        {
            ["stone_wall_2x1"] = "wall",
            ["Greydwarf"] = "creature",
            ["$hud_snappoint_bottom 1"] = "a snap point on whatever is loaded",
            ["Missing_piece"] = "a template's own object",
        };
        List<string> signed = new List<string>();
        StockRecord record = new StockRecord(name =>
        {
            signed.Add(name);
            return live.TryGetValue(name, out string s) ? s : null;
        }, Fixtures.Stock);
        TemplateFacts facts = new TemplateFacts("MWL_A", "Meadows", children: new[]
        {
            new ChildFact("MWL_A", "MWL_A", false, false, true, isRoot: true),
            new ChildFact("MWL_A/wall", "stone_wall_2x1", true, false, true),
            new ChildFact("MWL_A/Cube", "Cube", false, false, true),
            new ChildFact("MWL_A/wall/$hud_snappoint_bottom 1", "$hud_snappoint_bottom 1", false, true, true),
            new ChildFact("MWL_A/mock", "JVLmock_Missing_piece", false, false, true),
            new ChildFact("MWL_A/mock2", "JVLmock_Skeleton", false, false, true),
            new ChildFact("MWL_A/chest", "piece_chest_wood", true, false, true,
                referencedPrefabs: new[] { "Greydwarf", "JVLmock_Coins", "Ruby" }),
        }, interiorPrefabName: "Crypt_Interior");

        // The comparison's lookup, as the sweep's wrapper records it.
        record.Consulted("stone_wall_2x1");
        record.Read(facts);

        Assert.Null(record.Fault);
        Assert.Equal(1, record.TemplatesRead);
        Dictionary<string, string> recorded = record.Entries.ToDictionary(e => e.Key, e => e.Value);
        Assert.Equal(new[] { "Coins", "Crypt_Interior", "Greydwarf", "Missing_piece", "Ruby", "Skeleton", "stone_wall_2x1" },
            recorded.Keys.OrderBy(k => k, StringComparer.Ordinal));
        Assert.Equal(AuditCacheCanonical.Sha256Hex("wall"), recorded["stone_wall_2x1"]);
        Assert.Equal(AuditCacheCanonical.Sha256Hex("creature"), recorded["Greydwarf"]);
        // Stock names that resolved to nothing.
        Assert.Equal(StockRecord.Absent, recorded["Skeleton"]);
        Assert.Equal(StockRecord.Absent, recorded["Coins"]);
        Assert.Equal(StockRecord.Absent, recorded["Ruby"]);
        // Asked for, and not stock prefabs: recorded without being looked at.
        Assert.Equal(StockRecord.NotStock, recorded["Missing_piece"]);
        Assert.Equal(StockRecord.NotStock, recorded["Crypt_Interior"]);
        Assert.DoesNotContain("Missing_piece", signed);
        Assert.DoesNotContain("Crypt_Interior", signed);
        // Names objects merely bear are the template's content, not the game's.
        Assert.DoesNotContain("MWL_A", recorded.Keys);
        Assert.DoesNotContain("Cube", recorded.Keys);
        Assert.DoesNotContain("$hud_snappoint_bottom 1", recorded.Keys);
        Assert.DoesNotContain("piece_chest_wood", recorded.Keys);

        Assert.Empty(record.Recheck());
        live["stone_wall_2x1"] = "wall, edited by some other mod";
        Assert.Equal(new[] { "stone_wall_2x1" }, record.Recheck());
    }

    [Fact]
    public void ANameOutsideTheSnapshotIsNeverSignedSoItsLoadStateCannotMoveTheBaseline()
    {
        int calls = 0;
        StockRecord record = new StockRecord(name => "a different object every time " + calls++, Fixtures.Stock);

        record.Consulted("$hud_snappoint_bottom 1");
        record.Consulted("$hud_snappoint_top 4");

        Assert.Equal(0, calls);
        Assert.All(record.Entries, e => Assert.Equal(StockRecord.NotStock, e.Value));
        Assert.Empty(record.Recheck());
        Assert.Equal(0, calls);
    }

    [Fact]
    public void AStockNameThatResolvesToATemplateOwnedObjectIsNotSigned()
    {
        GameObject template = Templates.Stock("MWL_Somewhere", child: "wood_floor");
        GameObject owned = Templates.ChildOf(template, 0);
        GameObject stock = Templates.StockPrefab("wood_floor", "collider");

        Assert.Equal(StockRecord.NotStockSignature, AuditCache.SignStock("wood_floor", _ => owned));
        Assert.Equal(TemplateFactsExtractor.LiveSignature(stock), AuditCache.SignStock("wood_floor", _ => stock));
        Assert.Null(AuditCache.SignStock("wood_floor", _ => null));
        Assert.Null(AuditCache.SignStock("wood_floor", null));

        StockRecord record = new StockRecord(name => AuditCache.SignStock(name, _ => owned), Fixtures.Stock);
        record.Consulted("wood_floor");
        Assert.Equal(StockRecord.NotStock, record.Entries.Single().Value);
    }

    [Fact]
    public void ASignerThatThrowsBreaksTheRecordRatherThanTheAudit()
    {
        StockRecord record = new StockRecord(name => throw new InvalidOperationException("no ZNetScene"), Fixtures.Stock);
        record.Consulted("wood_floor");
        Assert.NotNull(record.Fault);
        Assert.Empty(record.Entries);
    }

    [Fact]
    public void TheLiveSignatureSeesWhatTheComparisonsOwnSignatureLeavesOut()
    {
        GameObject prefab = Templates.StockPrefab("wood_floor", "collider");
        ZNetView view = prefab.AddComponent<ZNetView>();
        string before = TemplateFactsExtractor.LiveSignature(prefab);

        view.m_syncInitialScale = !view.m_syncInitialScale;
        string afterView = TemplateFactsExtractor.LiveSignature(prefab);
        Assert.NotEqual(before, afterView);

        prefab.transform.localScale = new Vector3(2f, 2f, 2f);
        Assert.NotEqual(afterView, TemplateFactsExtractor.LiveSignature(prefab));
    }

    private static CatalogueAudit.CatalogueAuditRun RunOf(params CatalogueSubject[] subjects) =>
        CatalogueAudit.Begin(subjects, s => Fixtures.Compatible(s.Name, s.Pack), Fixtures.Stock, Fixtures.ExcludedPacks);

    private static readonly Dictionary<string, string> SameInputs =
        new Dictionary<string, string>(StringComparer.Ordinal) { ["MWL_A"] = "in-A", ["MWL_B"] = "in-B" };

    /// <summary>A stored file for <paramref name="report"/> whose verdicts use what <paramref name="record"/> noted for each.</summary>
    private static string Stored(AuditCacheKey key, CatalogueReport report, StockRecord record, IReadOnlyDictionary<string, string>? inputs = null) =>
        AuditCacheFile.Render(key, report.StockBuildId, report.PolicyFingerprint,
            report.Entries.Select(e => new StoredVerdict(e, (inputs ?? SameInputs)[e.Name], record.UsesOf(e.Name))).ToList(),
            record.Entries);

    [Fact]
    public void AChangedStockPrefabMakesOnlyTheTemplatesThatUsedItStaleAndNamesIt()
    {
        CatalogueSubject[] subjects = { new CatalogueSubject("MWL_A", "Meadows"), new CatalogueSubject("MWL_B", "Swamp") };
        CatalogueReport report = CatalogueAudit.Run(subjects, s => Fixtures.Compatible(s.Name, s.Pack), Fixtures.Stock, Fixtures.ExcludedPacks);
        Dictionary<string, string?> live = new Dictionary<string, string?>
        {
            ["stone_wall_2x1"] = "wall",
            ["wood_floor"] = "floor",
            ["Skeleton"] = null,
            ["Vines"] = "whatever is loaded",
        };
        Func<string, string?> sign = name => live.TryGetValue(name, out string? s) ? s : null;
        StockRecord record = new StockRecord(sign, Fixtures.Stock);
        record.Begin("MWL_A");
        record.Consulted("stone_wall_2x1");
        record.Consulted("wood_floor");
        record.Begin("MWL_B");
        record.Consulted("Skeleton");
        record.Consulted("Vines");
        AuditCacheKey key = Key();
        string text = Stored(key, report, record);

        AuditCacheReuse Select() => AuditCache.Select(AuditCacheFile.Parse(text, key), RunOf(subjects), SameInputs, sign);
        string[] Stale() => Select().Stale.Select(p => p.Key + ": " + p.Value).ToArray();

        Assert.Equal(new[] { "MWL_A", "MWL_B" }, Select().Reusable.Keys.OrderBy(k => k));

        // Not a stock name: whatever it resolves to now is not a reason to audit.
        live["Vines"] = "something else is loaded now";
        Assert.Empty(Select().Stale);

        live["stone_wall_2x1"] = "wall with a new child";
        Assert.Equal(new[] { "MWL_A: stock prefab 'stone_wall_2x1' changed" }, Stale());
        Assert.Equal(new[] { "MWL_B" }, Select().Reusable.Keys);
        Assert.Equal(new[] { "stone_wall_2x1" }, Select().ChangedStock);
        Assert.Equal("stock prefabs changed: stone_wall_2x1", Select().Reason);

        live["stone_wall_2x1"] = StockRecord.NotStockSignature;
        Assert.Equal("stock prefabs changed: stone_wall_2x1", Select().Reason);
        live["stone_wall_2x1"] = "wall";

        live["wood_floor"] = null;
        Assert.Equal(new[] { "MWL_A: 'wood_floor' no longer resolves" }, Stale());
        Assert.Equal("no longer resolve: wood_floor", Select().Reason);
        live["wood_floor"] = "floor";

        live["Skeleton"] = "somebody registered it";
        Assert.Equal(new[] { "MWL_B: 'Skeleton' now resolves" }, Stale());
        Assert.Equal("now resolve: Skeleton", Select().Reason);
    }

    [Fact]
    public void AMissAndARefusalNameEveryChangedPrefab()
    {
        CatalogueSubject[] subjects = { new CatalogueSubject("MWL_A", "Meadows") };
        CatalogueReport report = CatalogueAudit.Run(subjects, s => Fixtures.Compatible(s.Name, s.Pack), Fixtures.Stock, Fixtures.ExcludedPacks);
        string[] names = { "stone_wall_2x1", "wood_floor", "piece_chest_wood", "Greydwarf", "Coins", "Ruby", "Skeleton",
            "loot_chest_wood", "vfx_Place_wood_wall", "Spawner_GreydwarfNest" };
        Dictionary<string, string> live = names.ToDictionary(n => n, n => n);
        Func<string, string?> sign = name => live.TryGetValue(name, out string? s) ? s : null;
        StockRecord record = new StockRecord(sign, Fixtures.Stock);
        record.Begin("MWL_A");
        foreach (string name in names)
            record.Consulted(name);
        AuditCacheKey key = Key();
        string text = Stored(key, report, record);
        foreach (string name in names)
            live[name] += " edited";

        AuditCacheReuse reuse = AuditCache.Select(AuditCacheFile.Parse(text, key), RunOf(subjects), SameInputs, sign);
        Assert.Empty(reuse.Reusable);
        foreach (string name in names)
        {
            Assert.Contains(name, reuse.Reason);
            Assert.Contains(name, reuse.Stale.Single().Value);
        }
        Assert.DoesNotContain("more)", reuse.Reason);

        List<string> log = new List<string>();
        BepInEx.Logging.ManualLogSource.Captured = log;
        try
        {
            StockRecord during = new StockRecord(sign, Fixtures.Stock);
            foreach (string name in names)
                during.Consulted(name);
            foreach (string name in names)
                live[name] += " again";
            AuditCache.Attempt attempt = new AuditCache.Attempt("unused", key, null, during);
            Assert.Null(AuditCache.Prepare(attempt, report));
            string refusal = log.Single(line => line.Contains("stock prefabs changed while the audit ran"));
            foreach (string name in names)
                Assert.Contains(name, refusal);
            Assert.DoesNotContain("more)", refusal);
        }
        finally
        {
            BepInEx.Logging.ManualLogSource.Captured = null;
        }
    }

    [Fact]
    public void AStoredVerdictIsMatchedByNameAndInputInAnyOrder()
    {
        CatalogueSubject a = new CatalogueSubject("MWL_A", "Meadows");
        CatalogueSubject b = new CatalogueSubject("MWL_B", "Swamp");
        CatalogueSubject c = new CatalogueSubject("MWL_C", "Plains");
        CatalogueReport report = CatalogueAudit.Run(new[] { a, b }, s => Fixtures.Compatible(s.Name, s.Pack), Fixtures.Stock, Fixtures.ExcludedPacks);
        AuditCacheKey key = Key();
        AuditCacheLookup parsed = AuditCacheFile.Parse(Stored(key, report, new StockRecord(_ => null, Fixtures.Stock)), key);
        Func<string, string?> sign = _ => null;

        Assert.Equal(2, AuditCache.Select(parsed, RunOf(a, b), SameInputs, sign).Reusable.Count);
        // Another order is the same verdicts; the report is rebuilt in the run's order.
        Assert.Equal(2, AuditCache.Select(parsed, RunOf(b, a), SameInputs, sign).Reusable.Count);
        // A name nothing was stored for is audited.
        Dictionary<string, string> withC = new Dictionary<string, string>(SameInputs) { ["MWL_C"] = "in-C" };
        AuditCacheReuse added = AuditCache.Select(parsed, RunOf(a, b, c), withC, sign);
        Assert.Equal(new[] { "MWL_C: no stored verdict" }, added.Stale.Select(p => p.Key + ": " + p.Value));
        // A changed input is audited, alone.
        Dictionary<string, string> changed = new Dictionary<string, string>(SameInputs) { ["MWL_B"] = "in-B, rebuilt" };
        AuditCacheReuse one = AuditCache.Select(parsed, RunOf(a, b), changed, sign);
        Assert.Equal(new[] { "MWL_A" }, one.Reusable.Keys);
        Assert.Equal(new[] { "MWL_B: its definition, manifest entry or bundle changed" }, one.Stale.Select(p => p.Key + ": " + p.Value));
    }

    [Fact]
    public void AUseTheFileDoesNotSignIsMalformed()
    {
        CatalogueReport report = SampleReport();
        AuditCacheKey key = Key();
        string text = AuditCacheFile.Render(key, report.StockBuildId, report.PolicyFingerprint,
            new List<StoredVerdict> { new StoredVerdict(report.Entries[0], "in", new[] { "Unsigned" }) }, SampleStock);
        AuditCacheLookup lookup = AuditCacheFile.Parse(text, key);
        Assert.Equal(AuditCacheMiss.Malformed, lookup.Miss);
        Assert.Contains("does not sign", lookup.Reason);
    }

    [Fact]
    public void ANameStoredTwiceIsMalformed()
    {
        CatalogueReport report = SampleReport();
        AuditCacheKey key = Key();
        string text = AuditCacheFile.Render(key, report.StockBuildId, report.PolicyFingerprint,
            new List<StoredVerdict> { new StoredVerdict(report.Entries[0], "in", new string[0]), new StoredVerdict(report.Entries[0], "in", new string[0]) },
            SampleStock);
        Assert.Equal(AuditCacheMiss.Malformed, AuditCacheFile.Parse(text, key).Miss);
    }

    [Fact]
    public void ATemplateIsCreditedWithWhatItAskedForAndAReusedOnesUsesAreSignedAgain()
    {
        Dictionary<string, string> live = new Dictionary<string, string> { ["stone_wall_2x1"] = "wall", ["wood_floor"] = "floor" };
        StockRecord record = new StockRecord(name => live.TryGetValue(name, out string? s) ? s : null, Fixtures.Stock);

        record.Begin("MWL_A");
        record.Consulted("stone_wall_2x1");
        record.Begin("MWL_B");
        record.Consulted("stone_wall_2x1");
        record.Adopt("MWL_C", new[] { "wood_floor" });

        Assert.Equal(new[] { "stone_wall_2x1" }, record.UsesOf("MWL_A"));
        Assert.Equal(new[] { "stone_wall_2x1" }, record.UsesOf("MWL_B"));
        Assert.Equal(new[] { "wood_floor" }, record.UsesOf("MWL_C"));
        Assert.Equal(new[] { "stone_wall_2x1", "wood_floor" }, record.Entries.Select(e => e.Key));
        // Adopting does not change whose template is current.
        record.Consulted("Greydwarf");
        Assert.Contains("Greydwarf", record.UsesOf("MWL_B"));
    }

    // ---------------------------------------------------------------- writing

    [Fact]
    public void AnAtomicWriteLeavesOnlyTheFile()
    {
        string dir = Path.Combine(Path.GetTempPath(), "mwl-cache-" + Guid.NewGuid().ToString("N"));
        try
        {
            string path = Path.Combine(dir, "deeper", "server-only-audit.txt");
            Assert.Null(AuditCacheFile.Save(path, "first\n"));
            Assert.Null(AuditCacheFile.Save(path, "second\n"));

            Assert.Equal("second\n", File.ReadAllText(path));
            Assert.Equal(new[] { path }, Directory.GetFiles(Path.Combine(dir, "deeper")));
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { }
        }
    }

    [Fact]
    public void AnUnwritablePathIsReportedAndNeverThrown()
    {
        string blocker = Path.Combine(Path.GetTempPath(), "mwl-cache-blocker-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(blocker, "a file where a folder should be");
        try
        {
            string? error = AuditCacheFile.Save(Path.Combine(blocker, "sub", "cache.txt"), "text");
            Assert.NotNull(error);
        }
        finally
        {
            File.Delete(blocker);
        }
    }
}
