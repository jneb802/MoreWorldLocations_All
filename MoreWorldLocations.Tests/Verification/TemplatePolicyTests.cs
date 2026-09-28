using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// Whether a template can be served to a player who has never installed the mod.
///
/// <para>The failure these guard against has one shape in every form it takes:
/// the server is sure the building is there, and the player walks through it.
/// Each test below is one way for the two to disagree — an object the client
/// cannot name, a script only the server has, a wall that lives in the half of
/// the template a client without the mod never builds.</para>
///
/// <para>Each rejection fixture is <see cref="Fixtures.Compatible"/> with ONE
/// thing changed, and asserts the reason code as well as the verdict. A test
/// that only checked "blocked" would pass if every template were blocked for
/// the wrong reason, which is exactly the bug a gate like this invites.</para>
/// </summary>
public class TemplatePolicyTests
{
    [Fact]
    public void ATemplateWhoseObjectsAreAllStockIsCompatible()
    {
        TemplateEvaluation evaluation = Fixtures.Evaluate(Fixtures.Compatible());

        Assert.Equal(TemplateVerdict.Compatible, evaluation.Verdict);
        Assert.Empty(evaluation.Findings);
        Assert.True(evaluation.Approved);
    }

    // ---- client prefab identity -------------------------------------------

    [Fact]
    public void AnObjectTheStockClientDoesNotHaveIsBlocked()
    {
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked(), Fixtures.Networked("MWL_Shrine", "Root/Shrine") });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        TemplateFinding finding = Fixtures.Finding(evaluation, FindingCodes.UnknownPrefab);
        // The path is the point: "something is unknown" sends a reader to a
        // station log, "Root/Shrine is unknown" does not.
        Assert.Equal("Root/Shrine", finding.Path);
        Assert.Equal("MWL_Shrine", finding.Value);
    }

    [Fact]
    public void ANameThatHashesToADifferentStockPrefabIsBlockedAsACollision()
    {
        // A ZDO carries the hash, not the name, so a template child whose name
        // collides with a stock prefab looks correct on the server and builds
        // the WRONG object on the client.
        //
        // No two names in the shipped snapshot collide -- that was measured over
        // all 4644 -- and searching for a string that collides with a chosen one
        // is a 2^32 hunt that would buy nothing. So the premise is supplied: a
        // registry in which the stock prefab is recorded under the hash the
        // impostor's name produces. What is under test is the rule that a name
        // is only known when the hash leads back to that very name, and that
        // rule is exercised exactly.
        int impostorHash = "MWL_Impostor".GetStableHashCode();
        StockPrefabRegistry colliding = StockPrefabRegistry.Parse(
            "#buildid test-build\n" +
            "stone_wall_2x1\t" + impostorHash + "\n" +
            "wood_floor\t" + "wood_floor".GetStableHashCode() + "\n");

        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked("MWL_Impostor", "Root/Impostor") },
            terrain: new TerrainFact[0]);
        TemplateEvaluation evaluation = TemplatePolicy.Evaluate(facts, colliding, Fixtures.ExcludedPacks);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        TemplateFinding finding = Fixtures.Finding(evaluation, FindingCodes.PrefabHashCollision);
        Assert.Equal("Root/Impostor", finding.Path);
        Assert.Contains("stone_wall_2x1", finding.Detail);
        // And not merely unknown: the two need different fixes, and one of them
        // is silent on the client.
        Assert.False(Fixtures.HasCode(evaluation, FindingCodes.UnknownPrefab));
    }

    [Fact]
    public void AnUnresolvedMockIsBlockedRatherThanTreatedAsAnUnknownPrefab()
    {
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked("JVLmock_SacredPillar", "Root/Pillar") });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        Assert.True(Fixtures.HasCode(evaluation, FindingCodes.UnresolvedMock));
        // A placeholder the server itself is holding is a different problem
        // from a real prefab the client lacks, and fixing one would not fix the
        // other.
        Assert.False(Fixtures.HasCode(evaluation, FindingCodes.UnknownPrefab));
    }

    [Fact]
    public void AMockPrefixedTwiceIsNamedAsSuchBecauseStrippingOneFindsNothing()
    {
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked("JVLmock_JVLmock_Pickable_SurtlingCoreStand", "Root/Base2/Core") });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        Assert.True(Fixtures.HasCode(evaluation, FindingCodes.DoubleMockPrefix));
        Assert.False(Fixtures.HasCode(evaluation, FindingCodes.UnresolvedMock));
    }

    // ---- behaviour ---------------------------------------------------------

    [Fact]
    public void ACustomScriptOnAnObjectWithAPerfectlyVanillaNameIsBlocked()
    {
        // The dangerous shape: every name resolves, so a check by name alone
        // passes it, and the behaviour a player came for exists only where the
        // mod is installed.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[]
            {
                Fixtures.Networked(),
                Fixtures.Networked("piece_chest_wood", "Root/Chest",
                    foreignComponents: new[] { "Shrine" }),
            });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        TemplateFinding finding = Fixtures.Finding(evaluation, FindingCodes.CustomComponent);
        Assert.Equal("Root/Chest", finding.Path);
        Assert.Equal("Shrine", finding.Value);
    }

    [Fact]
    public void AForeignComponentThePolicyForgivesDoesNotBlock()
    {
        // Capability, not template: the forgiveness is granted to the TYPE, once,
        // with the evidence beside it -- never to a template by name.
        var policy = new ComponentPolicy(harmlessForeignComponents: new[] { "HarmlessMarker" });
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked(foreignComponents: new[] { "HarmlessMarker" }) });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts, components: policy);

        Assert.Equal(TemplateVerdict.Compatible, evaluation.Verdict);
    }

    [Fact]
    public void TheShippedPolicyForgivesNothing()
    {
        // The default is empty on purpose. An entry is a measured claim, and
        // nothing has been measured yet; a policy that starts with a few
        // plausible names would be the per-template exception list wearing a
        // different hat.
        Assert.Empty(ComponentPolicy.Default.HarmlessForeignComponents);
    }

    [Fact]
    public void AComponentFromAUnityModuleIsNotForeign()
    {
        Assert.True(ComponentPolicy.Default.IsGameAssembly("assembly_valheim"));
        Assert.True(ComponentPolicy.Default.IsGameAssembly("UnityEngine.CoreModule"));
        // Unity splits its engine into modules whose names move between
        // versions, so the family is matched rather than each member.
        Assert.True(ComponentPolicy.Default.IsGameAssembly("UnityEngine.SomeModuleAddedLater"));
        Assert.False(ComponentPolicy.Default.IsGameAssembly("More_World_Locations_AIO"));
        Assert.False(ComponentPolicy.Default.IsGameAssembly("Jotunn"));
    }

    // ---- object reconstruction --------------------------------------------

    [Fact]
    public void ScenaryThatOnlyTheTemplateOwnerWouldBuildIsBlocked()
    {
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked(), Fixtures.ProxyOnly("ExteriorGateway", "Root/Interior/ExteriorGateway") });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        Assert.Equal("Root/Interior/ExteriorGateway", Fixtures.Finding(evaluation, FindingCodes.EssentialProxyOnly).Path);
    }

    [Fact]
    public void AProxyOnlyObjectThatIsNeitherSeenNorTouchedIsNotAProblem()
    {
        // An empty grouping object is how anyone organises a hierarchy. Blocking
        // on it would exclude most of the catalogue for tidiness.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[]
            {
                Fixtures.Networked(),
                Fixtures.ProxyOnly("Exterior", "Root/Exterior", hasRenderer: false, hasCollider: false),
            });

        Assert.Equal(TemplateVerdict.Compatible, Fixtures.Evaluate(facts).Verdict);
    }

    [Fact]
    public void ANonNetworkedChildOfANetworkedObjectIsTheStockPrefabsOwnAndIsFine()
    {
        // This is the distinction the whole extractor turns on. A client without
        // the mod does not have the TEMPLATE; it does have every stock prefab,
        // children included, so a detail inside one arrives with it.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked(), Fixtures.InsideNetworked() });

        Assert.Equal(TemplateVerdict.Compatible, Fixtures.Evaluate(facts).Verdict);
    }

    [Fact]
    public void ADisabledObjectIsJudgedByNobodyBecauseNoClientSpawnsIt()
    {
        // Vanilla reads a location's children with
        // Utils.GetEnabledComponentsInChildren, so a disabled object is absent
        // from the world with the mod and without it. Judging it would exclude
        // templates over objects that do not exist.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[]
            {
                Fixtures.Networked(),
                Fixtures.Networked("MWL_Shrine", "Root/DisabledShrine", enabled: false),
                Fixtures.ProxyOnly("DisabledArch", "Root/DisabledArch", enabled: false),
            });

        Assert.Equal(TemplateVerdict.Compatible, Fixtures.Evaluate(facts).Verdict);
    }

    [Fact]
    public void ATemplateWhoseRootIsInactiveSpawnsNothingAndIsBlocked()
    {
        var facts = new TemplateFacts("MWL_StoneBeacon1", "BlackForest",
            rootActive: false,
            children: new[] { Fixtures.Networked() });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        Assert.True(Fixtures.HasCode(evaluation, FindingCodes.InactiveRoot));
    }

    [Fact]
    public void ATemplateThatEmitsNothingAtAllIsBlockedRatherThanCalledCompatible()
    {
        // Nothing to go wrong is not the same as nothing wrong: this places a
        // name on the map over an empty patch of grass.
        var facts = new TemplateFacts("MWL_Empty1", "Meadows",
            children: new[] { Fixtures.ProxyOnly("Grouping", "Root/Grouping", hasRenderer: false, hasCollider: false) });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        Assert.True(Fixtures.HasCode(evaluation, FindingCodes.NothingEmitted));
    }

    // ---- transform and scale ----------------------------------------------

    [Fact]
    public void AScaledObjectWhosePrefabDoesNotSendItsScaleIsBlocked()
    {
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked(scale: new Scale3(2f, 1f, 2f), syncInitialScale: false) });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        Assert.Contains("[2, 1, 2]", Fixtures.Finding(evaluation, FindingCodes.ScaleNotSynced).Detail);
    }

    [Fact]
    public void AScaledObjectWhosePrefabDoesSendItsScaleIsFine()
    {
        // A resolved scaled mock is not automatically incompatible. Whether the
        // scale reaches the client is a property of the real prefab, and here it
        // does.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked(scale: new Scale3(2f, 1f, 2f), syncInitialScale: true) });

        Assert.Equal(TemplateVerdict.Compatible, Fixtures.Evaluate(facts).Verdict);
    }

    [Fact]
    public void AScaleThatIsOneOnlyAfterAFloatRoundTripIsStillOne()
    {
        // 0.99999994 is what an authored 1 comes back as through a serialiser.
        // Treating it as a scale the author set would block on arithmetic noise.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked(scale: new Scale3(0.99999994f, 1.00000006f, 1f), syncInitialScale: false) });

        Assert.Equal(TemplateVerdict.Compatible, Fixtures.Evaluate(facts).Verdict);
    }

    // ---- persistence -------------------------------------------------------

    [Fact]
    public void AnObjectWhoseZdoIsNeverSavedIsBlocked()
    {
        // With the mod, a restart rebuilds this from the template. Without it,
        // the save is all there is, and the object is simply gone -- which is a
        // site that changes when the server restarts.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[] { Fixtures.Networked(persistent: false) });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        Assert.True(Fixtures.HasCode(evaluation, FindingCodes.NotPersistent));
    }

    // ---- randomisation, spawning and containers ----------------------------

    [Fact]
    public void ASpawnerWhoseCreatureIsStockIsSupported()
    {
        // Vanilla randomisation is fine when the server persists its outcome.
        // Rejecting a template for being random would exclude most of the game.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[]
            {
                Fixtures.Networked("Spawner_GreydwarfNest", "Root/Nest",
                    referencedPrefabs: new[] { "Greydwarf" }),
                Fixtures.Networked("piece_chest_wood", "Root/Chest",
                    referencedPrefabs: new[] { "Coins", "Ruby" }),
            });

        Assert.Equal(TemplateVerdict.Compatible, Fixtures.Evaluate(facts).Verdict);
    }

    [Fact]
    public void ABranchThatCanEmitANonStockPrefabIsBlockedEvenThoughItMightNeverBeRolled()
    {
        // A template that breaks on some seeds and not others is worse than one
        // that always breaks: the first one ships.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[]
            {
                Fixtures.Networked("piece_chest_wood", "Root/Chest",
                    referencedPrefabs: new[] { "Coins", "MWL_AncientKey" }),
            });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        TemplateFinding finding = Fixtures.Finding(evaluation, FindingCodes.UnknownSpawnReference);
        Assert.Equal("MWL_AncientKey", finding.Value);
        Assert.Equal("Root/Chest", finding.Path);
    }

    [Fact]
    public void ALocationThatBuildsAnInteriorFromRoomsIsBlockedWithThatReason()
    {
        var facts = new TemplateFacts("BFD_Exterior", "BlackForest",
            children: new[] { Fixtures.Networked() },
            dungeonTheme: "MWL_BlackForestDungeon");

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Blocked, evaluation.Verdict);
        // The reason has to be the interior, not "we found nothing wrong with
        // the exterior": the part the walk cannot see is the part a player goes
        // inside.
        Assert.True(Fixtures.HasCode(evaluation, FindingCodes.InteriorDungeon));
    }

    // ---- terrain: template capability versus placement feasibility ---------

    [Fact]
    public void SupportedTerrainDoesNotBlockBecauseConvertingItIsWhatThisModeIsFor()
    {
        TemplateFacts facts = Fixtures.Compatible(
            terrain: new[] { Fixtures.Terrain(level: true, smooth: true, paint: true, paintType: "Dirt") });

        Assert.Equal(TemplateVerdict.Compatible, Fixtures.Evaluate(facts).Verdict);
    }

    [Fact]
    public void TerrainReachingPastTheZoneRingIsANoteAboutPlacementAndNotABlock()
    {
        // The line the whole design turns on. This template converts perfectly
        // well; what is in doubt is whether a PARTICULAR site can be served, and
        // that is the per-site preflight's answer. Blocking here would withhold
        // every site because some site might straddle the wrong zones.
        TemplateFacts facts = Fixtures.Compatible(
            terrain: new[] { Fixtures.Terrain(localOffsetDistance: 60f, reach: 90f) });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Compatible, evaluation.Verdict);
        TemplateFinding finding = Fixtures.Finding(evaluation, FindingCodes.TerrainReachBeyondRing);
        Assert.Equal(FindingSeverity.Advisory, finding.Severity);
        Assert.Contains("per-site check", finding.Detail);
    }

    [Fact]
    public void AModifierThatAlreadyWritesThroughACompilerIsLeftAlone()
    {
        // It reaches the client on its own. Converting it as well would shape
        // the ground twice.
        TemplateFacts facts = Fixtures.Compatible(
            terrain: new[] { Fixtures.Terrain(useTerrainCompiler: true, reach: 200f) });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Compatible, evaluation.Verdict);
        Assert.Empty(evaluation.Findings);
    }

    [Fact]
    public void ADisabledModifierShapesNothingAndIsNotReported()
    {
        TemplateFacts facts = Fixtures.Compatible(
            terrain: new[] { Fixtures.Terrain(enabled: false, reach: 200f) });

        Assert.Empty(Fixtures.Evaluate(facts).Findings);
    }

    [Fact]
    public void TerrainAloneIsEnoughToCountAsEmission()
    {
        // A site that only shapes ground -- a clearing, a ford -- emits no
        // object and is not empty.
        var facts = new TemplateFacts("MWL_Clearing1", "Meadows",
            children: new ChildFact[0],
            terrain: new[] { Fixtures.Terrain() });

        Assert.Equal(TemplateVerdict.Compatible, Fixtures.Evaluate(facts).Verdict);
    }

    // ---- scope, and the order the questions are asked ----------------------

    [Fact]
    public void AnExcludedPackIsExcludedEvenWhenEverySructuralRuleWouldPass()
    {
        // The pinned hazard: a trader post whose objects are all stock still
        // does not ship, because what it is for needs client code this mode does
        // not carry.
        TemplateFacts facts = Fixtures.Compatible(name: "MWL_TraderPost1", pack: "Traders");

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.ScopeExcluded, evaluation.Verdict);
        Assert.Equal(FindingCodes.ScopeExcludedPack, Assert.Single(evaluation.Findings).Code);
    }

    [Fact]
    public void AnExcludedPackIsNotAlsoReportedForItsTechnicalFaults()
    {
        // Running the technical rules over an out-of-scope pack would produce
        // reasons nobody should act on: an operator reading that a trader post
        // was excluded for a custom component goes and fixes the wrong thing.
        TemplateFacts facts = Fixtures.Compatible(
            name: "MWL_TraderPost1", pack: "Traders",
            children: new[] { Fixtures.Networked("MWL_Shrine", "Root/Shrine") });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.ScopeExcluded, evaluation.Verdict);
        Assert.False(Fixtures.HasCode(evaluation, FindingCodes.UnknownPrefab));
    }

    [Fact]
    public void AnAssetWithNoLocationDefinitionIsItsOwnResultAndNotAnIncompatibility()
    {
        TemplateEvaluation evaluation = Fixtures.Evaluate(TemplateFacts.MissingDefinition("MWL_SwampCastle1"));

        Assert.Equal(TemplateVerdict.MissingDefinition, evaluation.Verdict);
        Assert.Equal(FindingCodes.NoActiveDefinition, Assert.Single(evaluation.Findings).Code);
    }

    // ---- what "no findings" is allowed to mean -----------------------------

    [Fact]
    public void AWalkThatDidNotFinishIsUnresolvedRatherThanCompatible()
    {
        // The dangerous pass: extraction fails, nothing is found, and nothing
        // found reads as nothing wrong.
        var facts = new TemplateFacts("MWL_Half1", "Meadows",
            children: new[] { Fixtures.Networked() },
            complete: false,
            extractionErrors: new[] { "the child walk exceeded its frame budget" });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.Equal(TemplateVerdict.Unresolved, evaluation.Verdict);
        Assert.False(evaluation.Approved);
        Assert.Contains("frame budget", Fixtures.Finding(evaluation, FindingCodes.ExtractionIncomplete).Detail);
    }

    [Fact]
    public void AWalkMarkedIncompleteWithNoReasonStillDoesNotPass()
    {
        var facts = new TemplateFacts("MWL_Half2", "Meadows",
            children: new[] { Fixtures.Networked() },
            complete: false);

        Assert.Equal(TemplateVerdict.Unresolved, Fixtures.Evaluate(facts).Verdict);
    }

    [Fact]
    public void ATemplateThatWouldNotLoadIsUnresolvedRatherThanBlocked()
    {
        // Blocked is a claim about the template. This is a claim about the run.
        TemplateEvaluation evaluation = Fixtures.Evaluate(
            TemplateFacts.Unreadable("MWL_Missing1", "Meadows", "the soft reference resolved to null"));

        Assert.Equal(TemplateVerdict.Unresolved, evaluation.Verdict);
        Assert.True(Fixtures.HasCode(evaluation, FindingCodes.TemplateNotLoaded));
    }

    [Fact]
    public void WithNoStockSnapshotEverythingIsUnresolvedAndNothingIsBlamedOnTheTemplate()
    {
        // Without the snapshot every name looks unknown, and a build that shipped
        // without it would exclude the whole catalogue and blame the author.
        TemplateEvaluation evaluation = TemplatePolicy.Evaluate(
            Fixtures.Compatible(), StockPrefabRegistry.Empty, Fixtures.ExcludedPacks);

        Assert.Equal(TemplateVerdict.Unresolved, evaluation.Verdict);
        Assert.True(Fixtures.HasCode(evaluation, FindingCodes.RegistryUnavailable));
    }

    [Fact]
    public void OneTemplatesFailureSaysNothingAboutTheNext()
    {
        // Isolation, pinned: a bad template must not take its neighbours with it
        // and must not be granted anything by theirs.
        var evaluations = new List<TemplateEvaluation>
        {
            Fixtures.Evaluate(Fixtures.Compatible("MWL_Good1")),
            Fixtures.Evaluate(Fixtures.Compatible("MWL_Bad1",
                children: new[] { Fixtures.Networked("MWL_Shrine", "Root/Shrine") })),
            Fixtures.Evaluate(Fixtures.Compatible("MWL_Good2")),
        };

        // ToArray, not a lazy Select: on Mono xunit compares a deferred
        // enumerable of enums against an enum array by their underlying ints and
        // reports a difference that is not there.
        Assert.Equal(
            new[] { TemplateVerdict.Compatible, TemplateVerdict.Blocked, TemplateVerdict.Compatible },
            evaluations.Select(e => e.Verdict).ToArray());
    }

    [Fact]
    public void EveryReasonCarriesSomethingToActOn()
    {
        // A reason code with no path, no value and no explanation is a reason
        // nobody can use, and this gate exists so that nobody has to read a
        // station log to find out why a location is missing.
        TemplateFacts facts = Fixtures.Compatible(
            children: new[]
            {
                Fixtures.Networked("MWL_Shrine", "Root/Shrine"),
                Fixtures.Networked("piece_chest_wood", "Root/Chest", foreignComponents: new[] { "Waystone" }),
                Fixtures.ProxyOnly("Arch", "Root/Arch"),
                Fixtures.Networked("wood_floor", "Root/Floor", persistent: false),
            });

        TemplateEvaluation evaluation = Fixtures.Evaluate(facts);

        Assert.NotEmpty(evaluation.Findings);
        Assert.All(evaluation.Findings, finding =>
        {
            Assert.NotEqual("", finding.Code);
            Assert.NotEqual("", finding.Path);
            Assert.True(finding.Detail.Length > 40, $"{finding.Code} explains nothing: '{finding.Detail}'");
        });
        // And the codes are distinct problems, not one problem reported four ways.
        Assert.Equal(4, evaluation.Codes.Count);
    }
}
