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
        string text = AuditCacheFile.Render(key, report, SampleStock);

        AuditCacheLookup lookup = AuditCacheFile.Parse(text, key);

        Assert.True(lookup.Hit, lookup.Reason);
        AssertSameReport(report, lookup.Contents!.Report);
        Assert.Equal(SampleStock, lookup.Contents.Stock);
        // One record per line: nothing in a free-text field made a line of its own.
        Assert.Equal(1 + 1 + key.Components.Count + 1 + SampleStock.Count + report.Entries.Count
                     + report.Entries.Sum(e => e.Evaluation.Findings.Count) + 1,
            text.Split('\n').Length - 1);
    }

    [Fact]
    public void RenderingIsDeterministic()
    {
        Assert.Equal(AuditCacheFile.Render(Key(), SampleReport(), SampleStock),
            AuditCacheFile.Render(Key(), SampleReport(), SampleStock));
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
        string stored = AuditCacheFile.Render(baseline, SampleReport(), SampleStock);

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
        string stored = AuditCacheFile.Render(Key(), SampleReport(), SampleStock);
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
        missing.Remove("mwl-files");
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
    public void ThePolicyPartSortsTheSubsetAndTheSubjectsPartKeepsOrder()
    {
        Assert.Equal(AuditCacheCanonical.Policy("p", new[] { "B", "A" }), AuditCacheCanonical.Policy("p", new[] { "A", "B" }));
        Assert.NotEqual(AuditCacheCanonical.Policy("p", new[] { "A" }), AuditCacheCanonical.Policy("p", new[] { "A", "B" }));

        CatalogueSubject a = new CatalogueSubject("MWL_A", "Meadows");
        CatalogueSubject b = new CatalogueSubject("MWL_B", "Swamp", interiorPrefabName: "Crypt");
        Assert.NotEqual(AuditCacheCanonical.Subjects(new[] { a, b }), AuditCacheCanonical.Subjects(new[] { b, a }));
        Assert.NotEqual(AuditCacheCanonical.Subjects(new[] { a, b }),
            AuditCacheCanonical.Subjects(new[] { a, new CatalogueSubject("MWL_B", "Swamp") }));
    }

    [Theory]
    [InlineData("warpalicious.More_World_Locations_AIO.cfg", true)]
    [InlineData("warpalicious.More_World_Locations_LootLists.yml", true)]
    [InlineData("warpalicious.More_World_Locations_Localization.English.yml", true)]
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

    /// <summary>A BepInEx installation on disk, as the engine half reads it.</summary>
    private sealed class Installation : IDisposable
    {
        public readonly string Root = Path.Combine(Path.GetTempPath(), "mwl-install-" + Guid.NewGuid().ToString("N"));

        /// <summary>Beside the DLL, as in an ordinary install, unless a test moves it.</summary>
        public string? ManifestDirectory;

        public Installation()
        {
            Write("plugins/MWL/More_World_Locations_AIO.dll", "mwl dll");
            Write("plugins/MWL/Bundles/mwl_ruins1", "bundle");
            Write("plugins/MWL/assetBundleManifest_full", "manifest");
            Write("plugins/Other/Other.dll", "other dll");
            Write("plugins/Jotunn/Jotunn.dll", "jotunn");
            Write("core/BepInEx.dll", "bepinex");
            Write("Managed/assembly_valheim.dll", "game");
            Write("config/warpalicious.More_World_Locations_AIO.cfg", "mwl settings");
            Write("config/warpalicious.More_World_Locations_LootLists.yml", "loot");
            Write("config/other.plugin.cfg", "other settings");
            Write("config/other.plugin.output.json", "per boot");
            Write("patchers/Patcher.dll", "patcher");
        }

        public void Write(string relative, string content)
        {
            string path = Path.Combine(Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public AuditCacheKey Key()
        {
            Dictionary<string, string> parts = Parts();
            foreach (KeyValuePair<string, string> part in AuditCacheCanonical.Installation(
                         Path.Combine(Root, "plugins/MWL"), ManifestDirectory, Path.Combine(Root, "config"),
                         "warpalicious.More_World_Locations_AIO",
                         Path.Combine(Root, "Managed/assembly_valheim.dll"), "1.0.15", "40", headless: true,
                         Path.Combine(Root, "plugins/Jotunn/Jotunn.dll"), Path.Combine(Root, "core"), null))
                parts[part.Key] = part.Value;
            return AuditCacheKey.FromCanonical(parts);
        }

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
    [InlineData("plugins/MWL/Bundles/mwl_ruins1", "mwl-files")]
    [InlineData("plugins/MWL/Bundles/new_bundle", "mwl-files")]
    [InlineData("plugins/MWL/More_World_Locations_AIO.dll", "mwl-files")]
    [InlineData("config/warpalicious.More_World_Locations_AIO.cfg", "mwl-config")]
    [InlineData("config/warpalicious.More_World_Locations_LootLists.yml", "mwl-config")]
    [InlineData("plugins/Jotunn/Jotunn.dll", "loader")]
    [InlineData("core/BepInEx.dll", "loader")]
    [InlineData("core/0Harmony.dll", "loader")]
    [InlineData("Managed/assembly_valheim.dll", "game")]
    public void MwlItsSettingsTheLoaderAndTheGameDoChangeIt(string file, string part)
    {
        using Installation install = new Installation();
        AuditCacheKey before = install.Key();
        string stored = AuditCacheFile.Render(before, SampleReport(), SampleStock);

        install.Write(file, "changed");

        AuditCacheLookup lookup = AuditCacheFile.Parse(stored, install.Key());
        Assert.Equal(AuditCacheMiss.KeyChanged, lookup.Miss);
        Assert.Equal(new[] { part }, lookup.Changed);
    }

    [Fact]
    public void ARewrittenCfgCommentIsNotAChangeButAValueIs()
    {
        // As BepInEx writes MWL's settings every start: the analytics id's
        // DEFAULT is a fresh random value each time, while its value stays.
        const string cfg = "warpalicious.More_World_Locations_AIO.cfg";
        using Installation install = new Installation();
        install.Write("config/" + cfg,
            "## Settings file was created by plugin More_World_Locations_AIO\n\n[Analytics]\n\n" +
            "## Random anonymous ID. Change or delete to reset.\n# Setting type: String\n" +
            "# Default value: 30ee238c-4f80-4da6-89a7-df331feaa7c3\nInstanceID = 4e5ad3bc\n");
        AuditCacheKey before = install.Key();
        string stored = AuditCacheFile.Render(before, SampleReport(), SampleStock);

        install.Write("config/" + cfg,
            "## Settings file was created by plugin More_World_Locations_AIO\r\n\r\n[Analytics]\r\n\r\n" +
            "## Random anonymous ID. Change or delete to reset.\r\n# Setting type: String\r\n" +
            "# Default value: cbe17a60-757f-47f2-b41c-e7fd8091bbba\r\nInstanceID = 4e5ad3bc\r\n");
        Assert.Equal(before.Digest, install.Key().Digest);

        install.Write("config/" + cfg, "[Analytics]\nInstanceID = something else\n");
        AuditCacheLookup lookup = AuditCacheFile.Parse(stored, install.Key());
        Assert.Equal(AuditCacheMiss.KeyChanged, lookup.Miss);
        Assert.Equal(new[] { "mwl-config" }, lookup.Changed);
    }

    [Fact]
    public void BundlesFoundOutsideMwlsFolderAreHashedToo()
    {
        using Installation install = new Installation();
        install.Write("plugins/warpalicious-More_World_Locations_AIO/assetBundleManifest_full", "manifest");
        install.Write("plugins/warpalicious-More_World_Locations_AIO/Bundles/mwl_ruins1", "bundle");
        install.ManifestDirectory = Path.Combine(install.Root, "plugins/warpalicious-More_World_Locations_AIO");
        string stored = AuditCacheFile.Render(install.Key(), SampleReport(), SampleStock);

        install.Write("plugins/warpalicious-More_World_Locations_AIO/Bundles/mwl_ruins1", "bundle, changed");

        Assert.Equal(new[] { "mwl-files" }, AuditCacheFile.Parse(stored, install.Key()).Changed);

        // Beside the DLL it is already covered, and not counted twice.
        install.ManifestDirectory = Path.Combine(install.Root, "plugins/MWL");
        install.Write("plugins/warpalicious-More_World_Locations_AIO/Bundles/mwl_ruins1", "bundle, changed again");
        AuditCacheKey beside = install.Key();
        install.Write("plugins/warpalicious-More_World_Locations_AIO/Bundles/mwl_ruins1", "irrelevant now");
        Assert.Equal(beside.Digest, install.Key().Digest);
    }

    [Fact]
    public void ATreeIsSortedRelativeAndTellsAbsentFromEmpty()
    {
        using Installation install = new Installation();
        FileTree mwl = AuditCacheCanonical.HashTree(Path.Combine(install.Root, "plugins/MWL"));
        Assert.Equal(new[] { "Bundles/mwl_ruins1", "More_World_Locations_AIO.dll", "assetBundleManifest_full" },
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
            Path.Combine(install.Root, "empty"), null, Path.Combine(install.Root, "config"), "g",
            Path.Combine(install.Root, "Managed/assembly_valheim.dll"), "v", "n", true,
            Path.Combine(install.Root, "plugins/Jotunn/Jotunn.dll"), Path.Combine(install.Root, "core"), null));
    }

    // ---------------------------------------------------------------- failing closed

    [Fact]
    public void EverythingWrongWithTheFileIsAMissWithItsOwnReason()
    {
        AuditCacheKey key = Key();
        string good = AuditCacheFile.Render(key, SampleReport(), SampleStock);
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
        string text = Reseal(AuditCacheFile.Render(key, SampleReport(), SampleStock), body => ReplaceFirst(body, from, to));

        AuditCacheLookup lookup = AuditCacheFile.Parse(text, key);

        Assert.Equal(AuditCacheMiss.UnknownValue, lookup.Miss);
        Assert.Contains(what, lookup.Reason);
    }

    [Fact]
    public void AnEntryMissingOneOfItsFindingsIsAMiss()
    {
        AuditCacheKey key = Key();
        string text = Reseal(AuditCacheFile.Render(key, SampleReport(), SampleStock), body =>
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
        string text = Reseal(AuditCacheFile.Render(key, SampleReport(), SampleStock),
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

    [Fact]
    public void AStockPrefabThatChangedVanishedOrAppearedIsAMissThatNamesIt()
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
        foreach (string name in live.Keys.ToList())
            record.Consulted(name);
        AuditCacheKey key = Key();
        string text = AuditCacheFile.Render(key, report, record.Entries);

        AuditCacheLookup Check() => AuditCache.Check(AuditCacheFile.Parse(text, key), RunOf(subjects), sign);

        Assert.True(Check().Hit, Check().Reason);

        // Not a stock name: whatever it resolves to now is not a reason to audit.
        live["Vines"] = "something else is loaded now";
        Assert.True(Check().Hit, Check().Reason);

        live["stone_wall_2x1"] = "wall with a new child";
        AuditCacheLookup changed = Check();
        Assert.Equal(AuditCacheMiss.Stock, changed.Miss);
        Assert.Equal(new[] { "stone_wall_2x1" }, changed.Changed);
        Assert.Equal("stock prefabs changed: stone_wall_2x1", changed.Reason);

        live["stone_wall_2x1"] = StockRecord.NotStockSignature;
        Assert.Equal("stock prefabs changed: stone_wall_2x1", Check().Reason);
        live["stone_wall_2x1"] = "wall";

        live["wood_floor"] = null;
        AuditCacheLookup vanished = Check();
        Assert.Equal(new[] { "wood_floor" }, vanished.Changed);
        Assert.Equal("no longer resolve: wood_floor", vanished.Reason);
        live["wood_floor"] = "floor";

        live["Skeleton"] = "somebody registered it";
        AuditCacheLookup appeared = Check();
        Assert.Equal(new[] { "Skeleton" }, appeared.Changed);
        Assert.Equal("now resolve: Skeleton", appeared.Reason);
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
        foreach (string name in names)
            record.Consulted(name);
        AuditCacheKey key = Key();
        string text = AuditCacheFile.Render(key, report, record.Entries);
        foreach (string name in names)
            live[name] += " edited";

        AuditCacheLookup lookup = AuditCache.Check(AuditCacheFile.Parse(text, key), RunOf(subjects), sign);
        foreach (string name in names)
            Assert.Contains(name, lookup.Reason);
        Assert.DoesNotContain("more)", lookup.Reason);

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
    public void AStoredReportForOtherNamesOrAnotherOrderIsAMiss()
    {
        CatalogueSubject a = new CatalogueSubject("MWL_A", "Meadows");
        CatalogueSubject b = new CatalogueSubject("MWL_B", "Swamp");
        CatalogueReport report = CatalogueAudit.Run(new[] { a, b }, s => Fixtures.Compatible(s.Name, s.Pack), Fixtures.Stock, Fixtures.ExcludedPacks);
        AuditCacheKey key = Key();
        AuditCacheLookup parsed = AuditCacheFile.Parse(AuditCacheFile.Render(key, report, SampleStock.Take(1).ToList()), key);
        Func<string, string?> sign = _ => null;

        Assert.True(AuditCache.Check(parsed, RunOf(a, b), sign).Hit);
        Assert.Equal(AuditCacheMiss.Subjects, AuditCache.Check(parsed, RunOf(b, a), sign).Miss);
        Assert.Equal(AuditCacheMiss.Subjects, AuditCache.Check(parsed, RunOf(a), sign).Miss);
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
