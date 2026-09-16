using System;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The conversion that lets an authored scale reach a stock client, and the rule
/// it answers.
///
/// <para>The pair matters more than either half, and so does the third party:
/// the STOCK prefab. <c>ZNetView.Awake</c> reads a ZDO's scale inside the
/// receiving object's own <c>m_syncInitialScale</c>, so a template that sends a
/// scale to a stock prefab which does not read one has changed nothing. The
/// first version of this file tested the sender alone and approved exactly that
/// case.</para>
/// </summary>
public class ScaleSyncConversionTests : IDisposable
{
    /// <summary>
    /// The extractor caches stock signatures by bare prefab name for the life of
    /// the process, which is right for one sweep and wrong for a suite where
    /// each class injects its own stock tree under a familiar name. This class
    /// uses 'loot_chest_wood', which no other test injects, and clears the cache
    /// either side of itself so neither direction can be poisoned.
    /// </summary>
    public ScaleSyncConversionTests() => TemplateFactsExtractor.ForgetStockSignatures();

    public void Dispose() => TemplateFactsExtractor.ForgetStockSignatures();

    private static GameObject Networked(GameObject parent, string name, float x, float y, float z, bool sync = false)
    {
        GameObject child = parent.Child(name);
        child.transform.localScale = new Vector3(x, y, z);
        ZNetView view = child.AddComponent<ZNetView>();
        view.m_syncInitialScale = sync;
        child.Child("mesh").AddComponent<BoxCollider>();
        return child;
    }

    /// <summary>A stock prefab of that name: what a client without the mod builds.</summary>
    private static Func<string, GameObject?> Stock(string name, bool readsScale, Vector3? scale = null)
    {
        var prefab = new GameObject(name);
        prefab.AddComponent<ZNetView>().m_syncInitialScale = readsScale;
        prefab.Child("mesh").AddComponent<BoxCollider>();
        if (scale.HasValue)
            prefab.transform.localScale = scale.Value;
        return asked => asked == name ? prefab : null;
    }

    [Fact]
    public void AScaleTheStockPrefabWillReadIsConverted()
    {
        var root = new GameObject("MWL_Scaled1");
        root.AddComponent<ZNetView>();
        GameObject scaled = Networked(root, "TreasureChest_dvergrtown", 1.2f, 1.2f, 1.2f);

        Assert.Equal(1, ScaleSyncConversion.Apply(root, Stock("TreasureChest_dvergrtown", readsScale: true)));
        Assert.True(scaled.GetComponent<ZNetView>()!.m_syncInitialScale);
    }

    [Fact]
    public void AScaleTheStockPrefabWillIgnoreIsNotConverted()
    {
        var root = new GameObject("MWL_Scaled2");
        root.AddComponent<ZNetView>();
        GameObject scaled = Networked(root, "Greydwarf_Root", 0.79669f, 0.79669f, 0.79669f);

        Assert.Equal(0, ScaleSyncConversion.Apply(root, Stock("Greydwarf_Root", readsScale: false)));
        Assert.False(scaled.GetComponent<ZNetView>()!.m_syncInitialScale);
    }

    [Fact]
    public void AScaleTheStockPrefabAlreadyBuildsAtNeedsNothingSent()
    {
        var root = new GameObject("MWL_Scaled3");
        root.AddComponent<ZNetView>();
        Networked(root, "big_rock", 2f, 2f, 2f);

        // The stock prefab is authored at 2 as well: the client builds the right
        // size without a word from the server.
        Assert.Equal(0, ScaleSyncConversion.Apply(root,
            Stock("big_rock", readsScale: true, scale: new Vector3(2f, 2f, 2f))));
    }

    [Fact]
    public void UnitScaleAlreadySyncedAndDisabledViewsAreLeftAlone()
    {
        var root = new GameObject("MWL_Scaled4");
        root.AddComponent<ZNetView>();
        Networked(root, "loot_chest_wood", 1f, 1f, 1f);
        Networked(root, "loot_chest_wood", 0.99999994f, 1f, 1f);
        Networked(root, "loot_chest_wood", 1.2f, 1.2f, 1.2f, sync: true);
        GameObject off = Networked(root, "loot_chest_wood", 1.05f, 1f, 1f);
        off.GetComponent<ZNetView>()!.enabled = false;

        Assert.Equal(0, ScaleSyncConversion.Apply(root, Stock("loot_chest_wood", readsScale: true)));
        Assert.False(off.GetComponent<ZNetView>()!.m_syncInitialScale);
    }

    [Fact]
    public void WithoutABaselineNothingIsConverted()
    {
        var root = new GameObject("MWL_Scaled5");
        root.AddComponent<ZNetView>();
        Networked(root, "TreasureChest_dvergrtown", 1.2f, 1.2f, 1.2f);

        Assert.Equal(0, ScaleSyncConversion.Apply(root, _ => null));
        Assert.Equal(0, ScaleSyncConversion.Apply(root, null));
        Assert.Equal(0, ScaleSyncConversion.Apply(null, Stock("x", true)));
    }

    [Fact]
    public void ApplyingItTwiceChangesNothingTheSecondTime()
    {
        var root = new GameObject("MWL_Scaled6");
        root.AddComponent<ZNetView>();
        Networked(root, "TreasureChest_dvergrtown", 1.2f, 1.2f, 1.2f);
        Func<string, GameObject?> stock = Stock("TreasureChest_dvergrtown", readsScale: true);

        Assert.Equal(1, ScaleSyncConversion.Apply(root, stock));
        Assert.Equal(0, ScaleSyncConversion.Apply(root, stock));
    }

    [Fact]
    public void ItReachesAnObjectNestedUnderAnother()
    {
        var root = new GameObject("MWL_Scaled7");
        root.AddComponent<ZNetView>();
        GameObject deep = Networked(root.Child("Blueprint"), "wood_wall_roof_upsidedown", 1f, 1f, 1.05f);

        Assert.Equal(1, ScaleSyncConversion.Apply(root, Stock("wood_wall_roof_upsidedown", readsScale: true)));
        Assert.True(deep.GetComponent<ZNetView>()!.m_syncInitialScale);
    }

    /// <summary>
    /// The rule and the conversion meeting: the scale blocks the template, and
    /// the conversion clears it — but only because this stock prefab reads one.
    /// </summary>
    [Fact]
    public void AScaledObjectBlocksTheTemplateUntilTheConversionHasRun()
    {
        var root = new GameObject("MWL_Scaled8");
        root.AddComponent<ZNetView>();
        Networked(root, "loot_chest_wood", 1.2f, 1.2f, 1.2f);
        Func<string, GameObject?> stock = Stock("loot_chest_wood", readsScale: true);

        TemplateEvaluation before = Fixtures.Evaluate(
            TemplateFactsExtractor.Extract("MWL_Scaled8", "Meadows", root, stockPrefabOf: stock));
        Assert.True(Fixtures.HasCode(before, FindingCodes.ScaleNotSynced));

        ScaleSyncConversion.Apply(root, stock);

        TemplateEvaluation after = Fixtures.Evaluate(
            TemplateFactsExtractor.Extract("MWL_Scaled8", "Meadows", root, stockPrefabOf: stock));
        Assert.False(Fixtures.HasCode(after, FindingCodes.ScaleNotSynced));
        Assert.False(Fixtures.HasCode(after, FindingCodes.ScaleNotReceived));
    }

    /// <summary>
    /// The case the review found: a stock prefab that ignores a sent scale is
    /// not made to read one by anything on our side, and must stay blocked.
    /// </summary>
    [Fact]
    public void AStockPrefabThatIgnoresScaleKeepsTheTemplateBlocked()
    {
        var root = new GameObject("MWL_Scaled9");
        root.AddComponent<ZNetView>();
        Networked(root, "loot_chest_wood", 0.79669f, 0.79669f, 0.79669f);
        Func<string, GameObject?> stock = Stock("loot_chest_wood", readsScale: false);

        ScaleSyncConversion.Apply(root, stock);

        TemplateEvaluation after = Fixtures.Evaluate(
            TemplateFactsExtractor.Extract("MWL_Scaled9", "Meadows", root, stockPrefabOf: stock));
        Assert.False(after.Approved);
        Assert.True(Fixtures.HasCode(after, FindingCodes.ScaleNotReceived));
    }

    /// <summary>A scale nobody could check is unresolved, never compatible.</summary>
    [Fact]
    public void AScaleWithNoBaselineIsUnresolvedRatherThanApproved()
    {
        var root = new GameObject("MWL_Scaled10");
        root.AddComponent<ZNetView>();
        Networked(root, "loot_chest_wood", 1.2f, 1.2f, 1.2f, sync: true);

        TemplateEvaluation result = Fixtures.Evaluate(
            TemplateFactsExtractor.Extract("MWL_Scaled10", "Meadows", root, stockPrefabOf: _ => null));
        Assert.False(result.Approved);
        Assert.True(Fixtures.HasCode(result, FindingCodes.StockBaselineUnavailable));
    }

    /// <summary>The conversion does not buy a passing verdict for anything else.</summary>
    [Fact]
    public void ItClearsTheScaleFindingAndNoOther()
    {
        var root = new GameObject("MWL_Scaled11");
        root.AddComponent<ZNetView>();
        GameObject scaled = Networked(root, "loot_chest_wood", 1.2f, 1.2f, 1.2f);
        scaled.GetComponent<ZNetView>()!.m_persistent = false;
        Func<string, GameObject?> stock = Stock("loot_chest_wood", readsScale: true);

        string[] before = Fixtures.Evaluate(
            TemplateFactsExtractor.Extract("MWL_Scaled11", "Meadows", root, stockPrefabOf: stock))
            .Findings.Select(f => f.Code).OrderBy(c => c).ToArray();
        ScaleSyncConversion.Apply(root, stock);
        string[] after = Fixtures.Evaluate(
            TemplateFactsExtractor.Extract("MWL_Scaled11", "Meadows", root, stockPrefabOf: stock))
            .Findings.Select(f => f.Code).OrderBy(c => c).ToArray();

        Assert.Contains(FindingCodes.ScaleNotSynced, before);
        Assert.Contains(FindingCodes.NotPersistent, before);
        Assert.Equal(before.Where(c => c != FindingCodes.ScaleNotSynced).ToArray(), after);
    }

    /// <summary>
    /// Full mode does not convert. A client that has the mod builds the template
    /// itself and already has the author's scale; changing prefabs there would
    /// be this mode reaching outside itself.
    /// </summary>
    [Fact]
    public void FullModeConvertsNothing()
    {
        bool wasEnabled = ServerOnlyMode.Enabled;
        Func<string, GameObject?>? wasStock = TemplateAssets.StockPrefabs;
        try
        {
            var root = new GameObject("MWL_Scaled12");
            root.AddComponent<ZNetView>();
            GameObject scaled = Networked(root, "TreasureChest_dvergrtown", 1.2f, 1.2f, 1.2f);
            TemplateAssets.StockPrefabs = Stock("TreasureChest_dvergrtown", readsScale: true);

            ServerOnlyMode.Set(false);
            ScaleSyncConversion.OnResolved("MWL_Scaled12", root);
            Assert.False(scaled.GetComponent<ZNetView>()!.m_syncInitialScale);

            ServerOnlyMode.Set(true);
            ScaleSyncConversion.OnResolved("MWL_Scaled12", root);
            Assert.True(scaled.GetComponent<ZNetView>()!.m_syncInitialScale);
        }
        finally
        {
            ServerOnlyMode.Set(wasEnabled);
            TemplateAssets.StockPrefabs = wasStock;
        }
    }
}
