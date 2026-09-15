using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The spawn gate, the readiness barrier, and what "the same as the stock
/// prefab" has to mean.
///
/// <para>Three failures from a review of <c>8e709b7</c>, and they share a
/// shape: a check that runs, answers, and is asking about the wrong thing. It
/// measured a 33-wide grid over a 65-wide zone, so every modifier in the outer
/// half of a zone was outside the vertices it looked at. It published sites it
/// had not been able to check, which is the gap it was built to close. And it
/// compared subtrees by component name and hierarchy, which accepts a collider
/// moved twenty metres and rejects a building the game scales correctly.</para>
///
/// <para>These pin the decisions. Harmony's timing, vanilla's placement
/// bookkeeping and a real client's physics are not modelled and need a running
/// world.</para>
/// </summary>
[Collection("templates")]
public class GateAndSubtreeTests
{
    // ---- the zone grid -----------------------------------------------------

    [Fact]
    public void ThePreflightAndTheWriterMeasureTheSameZone()
    {
        // They did not: 32 here and 64 there. A constant nobody shares is a
        // constant two people will disagree about, and the disagreement was a
        // grid covering the middle quarter of every zone.
        Assert.Equal(64, TerrainZoneDeltas.ZoneWidth);
        Assert.Equal(1f, TerrainZoneDeltas.ZoneScale);

        var zone = new TerrainZoneDeltas(ZoneSystem.GetZonePos(new Vector2s(0, 0)),
            TerrainZoneDeltas.ZoneWidth, TerrainZoneDeltas.ZoneScale);
        Assert.Equal(65, zone.Pitch);
    }

    [Fact]
    public void AnImpossibleCutInTheOuterHalfOfAZoneIsRefused()
    {
        // x = 24 is inside the real zone, which runs to 32, and outside the
        // half-sized grid the gate used to build. On the old grid this cut of
        // thirty metres touched no vertex at all and was waved through.
        using var world = new TerrainWorld();
        ZoneSystem.ZoneLocation location = world.LocationWithLevelModifier("ReviewSite", atX: 24f, levelTo: 0f);
        world.GroundEverywhere(30f);

        Assert.False(LocationSpawnGate.MayPublish(location, Vector3.zero, Quaternion.identity));
        // The REASON, not just the refusal. On the half-sized grid this modifier
        // touched no vertex, so the gate fell through to "the ground could not
        // be read" and withheld the site for the wrong reason -- which a test
        // asserting only "not published" cannot tell from the right one.
        Assert.Contains(SiteRefusalCodes.Unrepresentable, LocationSpawnGate.Status());
    }

    [Fact]
    public void AnImpossibleCutInTheMiddleOfAZoneIsStillRefused()
    {
        // The control for the one above: the old grid caught this, and the new
        // one must not have lost it while gaining the outer half.
        using var world = new TerrainWorld();
        ZoneSystem.ZoneLocation location = world.LocationWithLevelModifier("ReviewSite", atX: 0f, levelTo: 0f);
        world.GroundEverywhere(30f);

        Assert.False(LocationSpawnGate.MayPublish(location, Vector3.zero, Quaternion.identity));
        Assert.Contains(SiteRefusalCodes.Unrepresentable, LocationSpawnGate.Status());
    }

    [Fact]
    public void AServeableCutIsPublished()
    {
        // Not an indiscriminately refusing gate.
        using var world = new TerrainWorld();
        ZoneSystem.ZoneLocation location = world.LocationWithLevelModifier("ReviewSite", atX: 24f, levelTo: 29f);
        world.GroundEverywhere(30f);

        Assert.True(LocationSpawnGate.MayPublish(location, Vector3.zero, Quaternion.identity));
    }

    [Fact]
    public void HeightsKeptForOneGridAreNotHandedToAnother()
    {
        // The keeping was by zone alone, so a 32-wide answer was handed to a
        // 64-wide caller for ever -- "1089 long, not 4225" -- and the detached
        // writer could never recover even with the right build sitting ready.
        using var world = new TerrainWorld();
        var zone = new Vector2s(0, 0);

        world.Builder.Build(zone, world.World, width: 32);
        Assert.True(LocationTerrainBridge.TryGeneratedHeightAt(
            new TerrainZoneDeltas(ZoneSystem.GetZonePos(zone), 32, 1f), null, out _, out _));

        world.Builder.Build(zone, world.World, width: TerrainZoneDeltas.ZoneWidth);
        Assert.True(
            LocationTerrainBridge.TryGeneratedHeightAt(
                new TerrainZoneDeltas(ZoneSystem.GetZonePos(zone), TerrainZoneDeltas.ZoneWidth, TerrainZoneDeltas.ZoneScale),
                null, out _, out string reason),
            reason);
    }

    [Fact]
    public void AHeightArrayOfTheWrongLengthIsRefusedRatherThanReinterpreted()
    {
        // Accepting a longer array and reading it with this grid's stride walks
        // a 65-wide zone along 33-wide rows: every row after the first comes
        // from the wrong place, and the result still looks like terrain.
        using var world = new TerrainWorld();
        var zone = new Vector2s(0, 0);
        world.Builder.Build(zone, world.World, width: TerrainZoneDeltas.ZoneWidth);

        Assert.False(LocationTerrainBridge.TryGeneratedHeightAt(
            new TerrainZoneDeltas(ZoneSystem.GetZonePos(zone), 32, 1f), null, out _, out string reason));
        Assert.Contains("not", reason);
    }

    // ---- nothing is published on an unanswered question --------------------

    [Fact]
    public void ASiteWhoseGroundCannotBeReadIsHeldRatherThanPublished()
    {
        using var world = new TerrainWorld();
        ZoneSystem.ZoneLocation location = world.LocationWithLevelModifier("ReviewSite", atX: 0f, levelTo: 29f);
        // No ground built at all: nothing can be established either way.

        Assert.False(LocationSpawnGate.MayPublish(location, Vector3.zero, Quaternion.identity));
        Assert.Contains(SiteRefusalCodes.GroundUnknown, LocationSpawnGate.Status());
    }

    [Fact]
    public void ATemplateThatWouldNotLoadIsHeldRatherThanTreatedAsHavingNoTerrain()
    {
        // The two used to be the same answer here and they are opposite ones:
        // "this shapes no ground" publishes, "nobody knows what this does to the
        // ground" must not.
        using var world = new TerrainWorld();
        var location = new ZoneSystem.ZoneLocation { m_prefabName = "ReviewSite" };
        location.m_prefab.Name = "ReviewSite";
        location.m_prefab.Asset = null;
        ServerOnlySelection.SetRegistered(new[] { "ReviewSite" });

        Assert.False(LocationSpawnGate.MayPublish(location, Vector3.zero, Quaternion.identity));
    }

    [Fact]
    public void ALocationWithNoTerrainAtAllIsPublished()
    {
        // Most templates. Holding them would empty the world.
        using var world = new TerrainWorld();
        var location = new ZoneSystem.ZoneLocation { m_prefabName = "ReviewSite" };
        location.m_prefab.Name = "ReviewSite";
        location.m_prefab.Asset = new GameObject("ReviewSite");
        ServerOnlySelection.SetRegistered(new[] { "ReviewSite" });

        Assert.True(LocationSpawnGate.MayPublish(location, Vector3.zero, Quaternion.identity));
    }

    [Fact]
    public void AZoneWaitsWhileItsSitesGroundIsNotBuilt()
    {
        // The barrier sits where vanilla's own two readiness checks sit, and
        // behaves the same way: the zone is not generated and is tried again.
        using var world = new TerrainWorld();
        world.PlaceInstance("ReviewSite", new Vector2s(0, 0), atX: 0f, levelTo: 29f);

        Assert.False(ZoneReadinessBarrier.MayGenerate(new Vector2s(0, 0), ZoneSystem.SpawnMode.Ghost));
        Assert.Equal(1, ZoneReadinessBarrier.Holds[new Vector2s(0, 0)]);
    }

    [Fact]
    public void AHeldZoneGeneratesOnceItsGroundIsReady()
    {
        // unavailable -> ready -> placed, exactly once, with nothing published
        // in between.
        using var world = new TerrainWorld();
        world.PlaceInstance("ReviewSite", new Vector2s(0, 0), atX: 0f, levelTo: 29f);
        Assert.False(ZoneReadinessBarrier.MayGenerate(new Vector2s(0, 0), ZoneSystem.SpawnMode.Ghost));

        world.GroundEverywhere(30f);

        Assert.True(ZoneReadinessBarrier.MayGenerate(new Vector2s(0, 0), ZoneSystem.SpawnMode.Ghost));
        Assert.False(ZoneReadinessBarrier.Holds.ContainsKey(new Vector2s(0, 0)));
        Assert.True(LocationSpawnGate.MayPublish(
            world.Instance("ReviewSite").m_location, Vector3.zero, Quaternion.identity));
    }

    [Fact]
    public void AZoneIsNotHeldForEver()
    {
        // A world that never finishes generating is worse than a missing
        // location, so the hold is bounded and the placement is then given up --
        // refused by identity, never published.
        using var world = new TerrainWorld();
        world.PlaceInstance("ReviewSite", new Vector2s(0, 0), atX: 0f, levelTo: 29f);

        for (int attempt = 0; attempt < ZoneReadinessBarrier.MaxHolds - 1; attempt++)
            Assert.False(ZoneReadinessBarrier.MayGenerate(new Vector2s(0, 0), ZoneSystem.SpawnMode.Ghost));

        // The last attempt gives up and lets the zone through...
        Assert.True(ZoneReadinessBarrier.MayGenerate(new Vector2s(0, 0), ZoneSystem.SpawnMode.Ghost));
        // ...and the site itself is refused, so the zone generates WITHOUT it.
        world.GroundEverywhere(30f);
        Assert.False(LocationSpawnGate.MayPublish(
            world.Instance("ReviewSite").m_location, Vector3.zero, Quaternion.identity));
        Assert.Contains(SiteRefusalCodes.NeverReadable, LocationSpawnGate.Status());
    }

    [Fact]
    public void AZoneWithNoSiteOfOursIsNeverHeld()
    {
        using var world = new TerrainWorld();

        Assert.True(ZoneReadinessBarrier.MayGenerate(new Vector2s(5, 5), ZoneSystem.SpawnMode.Ghost));
    }

    [Fact]
    public void AClientRebuildingFromZdosIsNeverHeld()
    {
        // Holding there would withhold a site somebody already has.
        using var world = new TerrainWorld();
        world.PlaceInstance("ReviewSite", new Vector2s(0, 0), atX: 0f, levelTo: 29f);

        Assert.True(ZoneReadinessBarrier.MayGenerate(new Vector2s(0, 0), ZoneSystem.SpawnMode.Client));
    }

    // ---- what "the same as the stock prefab" has to mean -------------------

    private static readonly string[] Excluded = { "Ports", "Traders", "Trainers", "Dungeons" };

    private static (GameObject Root, GameObject Stock, GameObject Emitted) Trees()
    {
        var root = new GameObject("ReviewSite");
        GameObject emitted = root.Child("wood_floor");
        emitted.AddComponent<ZNetView>();
        emitted.Child("mesh").AddComponent<BoxCollider>();

        var stock = new GameObject("wood_floor");
        stock.AddComponent<ZNetView>();
        stock.Child("mesh").AddComponent<BoxCollider>();
        return (root, stock, emitted);
    }

    private static TemplateEvaluation Judge(GameObject root, GameObject stock) =>
        TemplatePolicy.Evaluate(
            TemplateFactsExtractor.Extract("ReviewSite", "Meadows", root, stockPrefabOf: _ => stock),
            VerificationData.StockPrefabs, Excluded);

    [Fact]
    public void Control_AnUnchangedStockSubtreePasses()
    {
        (GameObject root, GameObject stock, _) = Trees();

        Assert.True(Judge(root, stock).Approved);
    }

    [Fact]
    public void AnInheritedColliderMovedTwentyMetresIsNotStockEquivalent()
    {
        // Nothing about a descendant's transform is transmitted: the client
        // builds it from the stock prefab, so this collider is twenty metres
        // away on the server and nowhere else.
        (GameObject root, GameObject stock, GameObject emitted) = Trees();
        emitted.transform.GetChild(0).localPosition = new Vector3(20f, 0f, 0f);

        Assert.False(Judge(root, stock).Approved);
    }

    [Fact]
    public void AnInheritedColliderGrownTwentyfoldIsNotStockEquivalent()
    {
        // A collider's size lives in a native property, which no field walk
        // sees. It decides what a player can walk through, so it is read by name.
        (GameObject root, GameObject stock, GameObject emitted) = Trees();
        emitted.transform.GetChild(0).gameObject.GetComponent<BoxCollider>()!.size = new Vector3(20f, 1f, 1f);

        Assert.False(Judge(root, stock).Approved);
    }

    [Fact]
    public void AnInheritedColliderSwitchedOffIsNotStockEquivalent()
    {
        (GameObject root, GameObject stock, GameObject emitted) = Trees();
        emitted.transform.GetChild(0).gameObject.activeSelf = false;

        Assert.False(Judge(root, stock).Approved);
    }

    [Fact]
    public void ANetworkSyncedRootScaleIsSupported()
    {
        // The rejection this replaced: the signature included the emitted
        // object's own scale and compared it against a prefab's default of one,
        // so a building the game scales perfectly well was refused. The root's
        // transform IS transmitted -- that is what placement is -- and the scale
        // with it when the prefab syncs the initial scale.
        (GameObject root, GameObject stock, GameObject emitted) = Trees();
        emitted.AddComponent<BoxCollider>();
        stock.AddComponent<BoxCollider>();
        emitted.GetComponent<ZNetView>()!.m_syncInitialScale = true;
        stock.GetComponent<ZNetView>()!.m_syncInitialScale = true;
        emitted.transform.localScale = new Vector3(2f, 2f, 2f);

        Assert.True(Judge(root, stock).Approved);
    }

    [Fact]
    public void AnUnsyncedRootScaleIsStillRefused()
    {
        // The other half of the same rule: when the prefab does not send the
        // scale, the client builds at one and the two disagree about the size of
        // a solid object.
        (GameObject root, GameObject stock, GameObject emitted) = Trees();
        emitted.transform.localScale = new Vector3(2f, 2f, 2f);

        TemplateEvaluation evaluation = Judge(root, stock);

        Assert.False(evaluation.Approved);
        Assert.Contains(evaluation.Findings, f => f.Code == FindingCodes.ScaleNotSynced);
    }

    [Fact]
    public void AChangedGameComponentFieldIsNotStockEquivalent()
    {
        // The game's own components keep their settings in public fields, so a
        // drop table pointed somewhere else is read by reflection.
        (GameObject root, GameObject stock, GameObject emitted) = Trees();
        Container authored = emitted.Child("chest").AddComponent<Container>();
        authored.m_defaultItems.m_drops.Add(new DropTable.DropData { m_item = new GameObject("Ruby") });
        Container original = stock.Child("chest").AddComponent<Container>();
        original.m_defaultItems.m_drops.Add(new DropTable.DropData { m_item = new GameObject("Coins") });

        Assert.False(Judge(root, stock).Approved);
    }

    [Fact]
    public void AComponentWhoseSettingsCannotBeReadIsNamedRatherThanClaimedEquivalent()
    {
        // Advisory, not blocking. Every stock prefab in the game carries an
        // animator or a light, and blocking on them would exclude the catalogue
        // rather than check it -- but passing silently would be claiming an
        // equivalence the comparison did not establish.
        (GameObject root, GameObject stock, GameObject emitted) = Trees();
        emitted.transform.GetChild(0).gameObject.AddComponent<Animator>();
        stock.transform.GetChild(0).gameObject.AddComponent<Animator>();

        TemplateEvaluation evaluation = Judge(root, stock);

        Assert.True(evaluation.Approved);
        TemplateFinding finding = evaluation.Findings.Single(f => f.Code == FindingCodes.SubtreeNotFullyCompared);
        Assert.Equal(FindingSeverity.Advisory, finding.Severity);
        Assert.Contains("Animator", finding.Value);
    }

    [Fact]
    public void ABaselineFromAServerCarryingOtherModsIsReportedAsSuch()
    {
        // The baseline is the running server's registry, and an exact name does
        // not prove the object behind it is unmodified. There is nothing
        // pristine available in-process, so the limit is stated with the verdict
        // rather than left in a comment.
        (GameObject root, GameObject stock, _) = Trees();
        TemplateFacts facts = TemplateFactsExtractor.Extract(
            "ReviewSite", "Meadows", root, stockPrefabOf: _ => stock,
            baselineProvenance: "2 other plugin(s) are loaded — Foo, Bar");

        TemplateEvaluation evaluation = TemplatePolicy.Evaluate(facts, VerificationData.StockPrefabs, Excluded);

        Assert.True(evaluation.Approved);
        Assert.Contains(evaluation.Findings, f => f.Code == FindingCodes.StockBaselineProvenance);
    }

    [Fact]
    public void NothingIsReportedAboutABaselineWithNothingAgainstIt()
    {
        (GameObject root, GameObject stock, _) = Trees();

        Assert.DoesNotContain(Judge(root, stock).Findings, f => f.Code == FindingCodes.StockBaselineProvenance);
    }
}
