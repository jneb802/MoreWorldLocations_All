using System.Linq;
using More_World_Locations_AIO;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The four ways the first version of this tool could approve something nobody
/// had checked.
///
/// <para>Each one is the same failure wearing a different hat: the gate looked
/// like it was working. It reported on every name in the catalogue while only
/// ever opening four templates. It accepted whatever an author hung under an
/// object with a familiar name. It bound approvals to a digest that could not
/// see a wall move twenty metres or the ground drop ten. And it treated a
/// failure to write the report as a reason to leave a rejected build in the
/// world.</para>
///
/// <para>These came from a review of <c>c6f9434</c> with its own reproductions.
/// The fixtures are ported rather than copied so they run on both runtimes with
/// the rest of the suite, and the assertions are the reviewer's.</para>
/// </summary>
[Collection("templates")]
public class ReviewRegressionTests
{
    private static readonly string[] Excluded = { "Ports", "Traders", "Trainers", "Dungeons" };

    private static TemplateEvaluation Judge(GameObject root, System.Func<string, GameObject?>? stock = null) =>
        TemplatePolicy.Evaluate(
            TemplateFactsExtractor.Extract(root.name, "Meadows", root, stockPrefabOf: stock),
            VerificationData.StockPrefabs, Excluded);

    // ---- controls: what must keep working ----------------------------------

    [Fact]
    public void Control_APlainStockTemplateStillPasses()
    {
        Assert.True(Judge(Templates.Stock()).Approved);
    }

    [Fact]
    public void Control_AForeignBehaviourOnTheNetworkedObjectIsStillRejected()
    {
        GameObject root = Templates.Stock();
        Templates.ChildOf(root, 0).AddComponent<TestOnlyClientBehaviour>();

        Assert.False(Judge(root).Approved);
    }

    // ---- R1: the audit could only inspect what it had already approved -----

    [Fact]
    public void R1_AnUnapprovedDefinitionIsOpenedAndJudgedWithoutBeingRegisteredFirst()
    {
        // The circularity: templates were found through
        // ZoneSystem.m_locationsByHash, which only holds what registration put
        // there, and registration is the decision the audit is supposed to make.
        // Iterating the whole catalogue changed the number of rows in the report
        // and not the set of templates actually opened.
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();

        LocationDB.RegisterAll();

        // Registration correctly leaves it out -- it is not in the selection.
        Assert.False(world.IsInWorld("Review_NewBuild"));
        // And it was nevertheless opened and judged on its merits.
        Assert.Contains("Review_NewBuild", world.Opened);
        Assert.Equal(TemplateVerdict.Compatible,
            CatalogueAudit.Report!.Find("Review_NewBuild")!.Evaluation.Verdict);
    }

    [Fact]
    public void R1_EveryDefinitionIsOpened_NotOnlyTheApprovedOnes()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();

        LocationDB.RegisterAll();

        Assert.Equal(
            LocationDB.All.Select(l => l.Name).OrderBy(n => n, System.StringComparer.Ordinal).ToArray(),
            world.Opened.OrderBy(n => n, System.StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void R1_EveryTemplateOpenedIsReleasedAgain()
    {
        // A sweep over the whole catalogue that held every template would run a
        // dedicated server out of memory checking its own locations.
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();

        LocationDB.RegisterAll();

        Assert.Equal(world.Opened.OrderBy(n => n).ToArray(), world.Released.OrderBy(n => n).ToArray());
    }

    [Fact]
    public void R1_TheReportAccountsForNamesNoDefinitionDeclares()
    {
        // An asset that exists and is placed nowhere is a real entry with a real
        // answer. A report built only from the definitions cannot contain it, so
        // the catalogue could never be reconciled by identity.
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();

        LocationDB.RegisterAll();

        CatalogueEntry? assetOnly = CatalogueAudit.Report!.Find("MWL_SwampCastle1");
        Assert.NotNull(assetOnly);
        Assert.Equal(TemplateVerdict.MissingDefinition, assetOnly!.Evaluation.Verdict);
        // And the capitalisation pair is two rows, not one folded name.
        Assert.NotNull(CatalogueAudit.Report.Find("MWL_MayPoleHut1"));
    }

    [Fact]
    public void Control_TheApprovedFourStillRegister()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();

        LocationDB.RegisterAll();

        Assert.True(CatalogueAudit.Report!.Find("MWL_Ruins1")!.Evaluation.Approved);
        Assert.Equal(4, CatalogueAudit.Report.RegisteredCount);
        Assert.True(world.IsInWorld("MWL_Ruins1"));
    }

    // ---- R2: network ancestry was treated as proof -------------------------

    [Fact]
    public void R2_AForeignBehaviourUnderANetworkedAncestorIsRejected()
    {
        GameObject root = Templates.Stock();
        GameObject added = Templates.ChildOf(root, 0).Child("AuthoredChild");
        added.AddComponent<TestOnlyClientBehaviour>();
        added.AddComponent<Collider>();

        Assert.False(Judge(root).Approved);
    }

    [Fact]
    public void R2_AnAddedColliderUnderANetworkedAncestorNeedsProofItIsInherited()
    {
        // Nothing foreign here at all: a plain vanilla collider, hung under an
        // object with a familiar name. The client instantiates the STOCK prefab,
        // which does not have it, and an ancestor flag cannot say otherwise.
        GameObject root = Templates.Stock();
        Templates.ChildOf(root, 0).Child("ExtraInvisibleWall").AddComponent<Collider>();

        Assert.False(Judge(root).Approved);
    }

    [Fact]
    public void R2_WithNoBaselineTheAnswerIsUnresolvedRatherThanBlocked()
    {
        // "We could not check" and "we checked and it was wrong" are different
        // claims about the author's work, and only one of them is fair.
        GameObject root = Templates.Stock();
        Templates.ChildOf(root, 0).Child("ExtraInvisibleWall").AddComponent<Collider>();

        TemplateEvaluation evaluation = Judge(root);

        Assert.Equal(TemplateVerdict.Unresolved, evaluation.Verdict);
        Assert.Contains(evaluation.Findings, f => f.Code == FindingCodes.StockBaselineUnavailable);
    }

    [Fact]
    public void R2_WithABaselineAnAddedChildIsBlockedAndNamed()
    {
        GameObject root = Templates.Stock();
        Templates.ChildOf(root, 0).Child("ExtraInvisibleWall").AddComponent<Collider>();
        GameObject stock = Templates.StockPrefab("wood_floor");

        TemplateEvaluation evaluation = Judge(root, name => name == "wood_floor" ? stock : null);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        TemplateFinding finding = evaluation.Findings.Single(f => f.Code == FindingCodes.ModifiedStockSubtree);
        Assert.Contains("ExtraInvisibleWall", finding.Detail);
    }

    [Fact]
    public void R2_AnUnchangedStockSubtreeIsAccepted()
    {
        // The positive case the fix must not trade away. A stock prefab with its
        // own children is the ordinary case, and rejecting it would ban most of
        // the game rather than check anything.
        var root = new GameObject("Test_Template");
        GameObject floor = root.Child("wood_floor");
        floor.AddComponent<ZNetView>();
        floor.Child("mesh");
        floor.Child("collider").AddComponent<Collider>();

        GameObject stock = Templates.StockPrefab("wood_floor", "mesh");
        stock.Child("collider").AddComponent<Collider>();

        Assert.Equal(TemplateVerdict.Compatible, Judge(root, name => name == "wood_floor" ? stock : null).Verdict);
    }

    [Fact]
    public void R2_AChangedVanillaComponentUnderANetworkedAncestorIsRejected()
    {
        // Nothing added and nothing foreign: the same objects, the same
        // components, one field pointed somewhere else. The client builds the
        // stock prefab and rolls the stock loot.
        var root = new GameObject("Test_Template");
        GameObject chest = root.Child("piece_chest_wood");
        chest.AddComponent<ZNetView>();
        Container authored = chest.AddComponent<Container>();
        authored.m_defaultItems.m_drops.Add(new DropTable.DropData { m_item = new GameObject("Ruby") });

        var stock = new GameObject("piece_chest_wood");
        Container stockContainer = stock.AddComponent<Container>();
        stockContainer.m_defaultItems.m_drops.Add(new DropTable.DropData { m_item = new GameObject("Coins") });

        TemplateEvaluation evaluation = Judge(root, name => name == "piece_chest_wood" ? stock : null);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        Assert.Contains(evaluation.Findings, f => f.Code == FindingCodes.ModifiedStockSubtree);
    }

    [Fact]
    public void R2_ABehaviourOnTheTemplateRootIsReadAndRejected()
    {
        // The walk started at the children, so the root was never read at all --
        // and a script driving the whole site is exactly what would sit there.
        GameObject root = Templates.Stock();
        root.AddComponent<TestOnlySiteDriver>();

        TemplateEvaluation evaluation = Judge(root);

        Assert.False(evaluation.Approved);
        TemplateFinding finding = evaluation.Findings.Single(f => f.Code == FindingCodes.CustomComponent);
        Assert.Equal("TestOnlySiteDriver", finding.Value);
        Assert.Contains("template root", finding.Detail);
    }

    // ---- R3: the fingerprint did not bind the tested content ---------------

    [Fact]
    public void R3_ChangingTheTerrainOffsetInvalidatesTheContentApproval()
    {
        GameObject root = Templates.Stock();
        TerrainModifier modifier = root.Child("Terrain").AddComponent<TerrainModifier>();
        modifier.m_level = true;
        modifier.m_levelRadius = 4f;
        modifier.m_levelOffset = -2f;

        string before = Fingerprint(root);
        modifier.m_levelOffset = -12f;

        Assert.NotEqual(before, Fingerprint(root));
    }

    [Fact]
    public void R3_ChangingTheEffectiveModifierOrderInvalidatesTheContentApproval()
    {
        // Each modifier reads what the one before it left, so the same numbers
        // applied in a different order draw different ground -- and the order is
        // m_sortOrder, which a walk over the hierarchy cannot see.
        GameObject root = Templates.Stock();
        TerrainModifier first = root.Child("A").AddComponent<TerrainModifier>();
        TerrainModifier second = root.Child("B").AddComponent<TerrainModifier>();
        first.m_level = second.m_level = true;
        first.m_levelRadius = second.m_levelRadius = 4f;
        first.m_levelOffset = 2f;
        second.m_levelOffset = -2f;
        first.m_sortOrder = 0;
        second.m_sortOrder = 1;

        string before = Fingerprint(root);
        first.m_sortOrder = 2;

        Assert.NotEqual(before, Fingerprint(root));
    }

    [Fact]
    public void R3_MovingAPieceInvalidatesTheContentApproval()
    {
        // No rule can see this, and that is the point: an approval is not "the
        // rules passed", it is "this template was watched in game and behaved".
        GameObject root = Templates.Stock();
        string before = Fingerprint(root);

        root.transform.GetChild(0).position = new Vector3(20f, 0f, 0f);

        Assert.NotEqual(before, Fingerprint(root));
    }

    [Fact]
    public void R3_EquivalentContentStillFingerprintsTheSame()
    {
        // The other direction. A guard that cries wolf is a guard somebody turns
        // off, so two builds of the same template have to agree.
        Assert.Equal(Fingerprint(Templates.Stock()), Fingerprint(Templates.Stock()));
    }

    private static string Fingerprint(GameObject root) =>
        TemplateFingerprint.Of(TemplateFactsExtractor.Extract(root.name, "Meadows", root));

    // ---- R4: a reporting failure left a rejected build registered ----------

    [Fact]
    public void Control_AnIncompatibleApprovedLocationIsNeverRegistered()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        world.Asset("MWL_Ruins1").Child("Review_CustomWall").AddComponent<ZNetView>();

        LocationDB.RegisterAll();
        TemplateWorld.RunEnforcementHook();

        Assert.Equal(TemplateVerdict.Blocked, CatalogueAudit.Report!.Find("MWL_Ruins1")!.Evaluation.Verdict);
        Assert.False(world.IsInWorld("MWL_Ruins1"));
    }

    [Fact]
    public void R4_AReportingFailureDoesNotLeaveARejectedTemplateRegistered()
    {
        // Writing the report and deciding what the world holds used to be one
        // step, with the "already done" flag set before either. Logging must not
        // be able to decide whether a rejected build stays in the world.
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        world.Asset("MWL_Ruins1").Child("Review_CustomWall").AddComponent<ZNetView>();

        BepInEx.Logging.ManualLogSource.ThrowOnNextInfo = true;
        LocationDB.RegisterAll();
        TemplateWorld.RunEnforcementHook();

        Assert.Equal(TemplateVerdict.Blocked, CatalogueAudit.Report!.Find("MWL_Ruins1")!.Evaluation.Verdict);
        Assert.False(world.IsInWorld("MWL_Ruins1"));
    }

    [Fact]
    public void R4_EnforcementRunsAtTheEndOfRegistrationAndAgainLater()
    {
        // Measured on the station: the ZoneSystem.SetupLocations hook fires at
        // the MAIN MENU, before RegisterAll has been called at all. So the
        // transaction closes at the end of registration, where both halves have
        // provably happened, and the later hook is a second pass that catches
        // anything added since -- which means it must not be a no-op once the
        // first pass has run.
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();

        LocationDB.RegisterAll();
        Assert.True(world.IsInWorld("MWL_Ruins1"));
        Assert.False(world.IsInWorld("Review_NewBuild"));

        new MWLLocation { Name = "Review_NewBuild" }.Register();
        TemplateWorld.RunEnforcementHook();

        Assert.False(world.IsInWorld("Review_NewBuild"));
        Assert.True(world.IsInWorld("MWL_Ruins1"));
    }

    [Fact]
    public void R4_AProgressMessageThatThrowsDoesNotAbortTheAudit()
    {
        // Adding a progress line to a long sweep was enough to reintroduce R4:
        // a log sink that throws aborted the audit, the failed audit registered
        // nothing, and a caller that only wanted a message had decided what the
        // world contains.
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        CatalogueAudit.Progress = _ => throw new System.InvalidOperationException("the sink is on fire");

        try
        {
            LocationDB.RegisterAll();
        }
        finally
        {
            CatalogueAudit.Progress = null;
        }

        Assert.NotNull(CatalogueAudit.Report);
        Assert.True(world.IsInWorld("MWL_Ruins1"));
    }

    [Fact]
    public void R4_AnAuditThatNeverRanRegistersNothingRatherThanTheShippedSelection()
    {
        // The fallback that used to be "keep the shipped selection" is the very
        // set of unverified names the guard exists to check. An empty world with
        // a loud reason is the right direction for a verification failure.
        using var world = new TemplateWorld();
        TemplateAssets.Source = null;

        LocationDB.RegisterAll();

        Assert.All(LocationDB.All, location => Assert.False(world.IsInWorld(location.Name)));
    }

    [Fact]
    public void R4_EnforcementWithdrawsAnythingTheAuditDidNotApprove()
    {
        // Registration and the world's list are two different things, and only
        // the second one places buildings. Something else putting a location
        // there must not survive the sweep.
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        LocationDB.RegisterAll();

        new MWLLocation { Name = "Review_NewBuild" }.Register();
        Assert.True(world.IsInWorld("Review_NewBuild"));

        TemplateWorld.RunEnforcementHook();

        Assert.False(world.IsInWorld("Review_NewBuild"));
        Assert.True(world.IsInWorld("MWL_Ruins1"));
    }

    [Fact]
    public void R4_AWithdrawnLocationStopsBeingOneOfOurs()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        LocationDB.RegisterAll();
        new MWLLocation { Name = "Review_NewBuild" }.Register();
        ServerOnlySelection.SetRegistered(new[] { "MWL_Ruins1", "Review_NewBuild" });

        TemplateWorld.RunEnforcementHook();

        // Otherwise the terrain conversion would go on shaping ground for a site
        // nothing places.
        Assert.False(ServerOnlySelection.IsOurs("Review_NewBuild"));
        Assert.True(ServerOnlySelection.IsOurs("MWL_Ruins1"));
    }
}

/// <summary>
/// The template fixtures share process-wide state — the location list, the
/// asset source, the registered set — so they run one at a time.
/// </summary>
[CollectionDefinition("templates", DisableParallelization = true)]
public class TemplateCollection { }
