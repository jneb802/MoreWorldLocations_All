using System.Collections.Generic;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// What binds an approval to the template it was granted for.
///
/// Two failures to avoid, in opposite directions. A fingerprint that misses a
/// policy-relevant change lets an approval outlive the thing it approved — the
/// author ships a new version and the server keeps registering it on last
/// month's evidence. A fingerprint that moves on changes no rule can see makes
/// the drift report cry wolf, and a guard nobody believes gets switched off.
/// </summary>
public class TemplateFingerprintTests
{
    private static TemplateFacts Facts(
        IEnumerable<ChildFact>? children = null,
        IEnumerable<TerrainFact>? terrain = null,
        string name = "MWL_Example1") =>
        Fixtures.Compatible(name, children: children, terrain: terrain);

    [Fact]
    public void TheSameFactsAlwaysGiveTheSameDigest()
    {
        // It is written into a shipped file and compared on another machine and
        // another runtime, so string.GetHashCode -- randomised per process --
        // would be worse than useless.
        Assert.Equal(TemplateFingerprint.Of(Facts()), TemplateFingerprint.Of(Facts()));
        Assert.Equal("1ebf7e9a0aa15d7c".Length, TemplateFingerprint.Of(Facts()).Length);
    }

    [Fact]
    public void ChildrenInADifferentHierarchyOrderAreTheSameTemplate()
    {
        string a = TemplateFingerprint.Of(Facts(children: new[]
        {
            Fixtures.Networked("wood_floor", "Root/a"),
            Fixtures.Networked("stone_wall_2x1", "Root/b"),
        }));
        string b = TemplateFingerprint.Of(Facts(children: new[]
        {
            Fixtures.Networked("stone_wall_2x1", "Root/b"),
            Fixtures.Networked("wood_floor", "Root/a"),
        }));

        Assert.Equal(a, b);
    }

    [Fact]
    public void TerrainInADifferentORDERIsDifferentGround()
    {
        // Not sorted, deliberately. The order modifiers appear in is the order
        // vanilla applies them, and a smooth pass that reads ground a level pass
        // has already flattened is not the same ground as the reverse.
        string a = TemplateFingerprint.Of(Facts(terrain: new[]
        {
            Fixtures.Terrain("Root/T1", level: true, smooth: false),
            Fixtures.Terrain("Root/T2", level: false, smooth: true),
        }));
        string b = TemplateFingerprint.Of(Facts(terrain: new[]
        {
            Fixtures.Terrain("Root/T2", level: false, smooth: true),
            Fixtures.Terrain("Root/T1", level: true, smooth: false),
        }));

        Assert.NotEqual(a, b);
    }

    [Theory]
    [InlineData("prefab")]
    [InlineData("networked")]
    [InlineData("enabled")]
    [InlineData("persistent")]
    [InlineData("scale")]
    [InlineData("syncScale")]
    [InlineData("renderer")]
    [InlineData("collider")]
    [InlineData("ancestor")]
    [InlineData("foreign")]
    [InlineData("referenced")]
    public void EveryFactAVerdictTurnsOnMovesTheDigest(string change)
    {
        ChildFact original = Fixtures.Networked("stone_wall_2x1", "Root/w");
        ChildFact changed = change switch
        {
            "prefab" => Fixtures.Networked("wood_floor", "Root/w"),
            "networked" => new ChildFact("Root/w", "stone_wall_2x1", networked: false, underNetworkedAncestor: false,
                enabledInHierarchy: true, hasRenderer: true, hasCollider: true),
            "enabled" => Fixtures.Networked("stone_wall_2x1", "Root/w", enabled: false),
            "persistent" => Fixtures.Networked("stone_wall_2x1", "Root/w", persistent: false),
            "scale" => Fixtures.Networked("stone_wall_2x1", "Root/w", scale: new Scale3(2f, 1f, 1f)),
            "syncScale" => Fixtures.Networked("stone_wall_2x1", "Root/w", syncInitialScale: true),
            "renderer" => new ChildFact("Root/w", "stone_wall_2x1", networked: true, underNetworkedAncestor: false,
                enabledInHierarchy: true, hasRenderer: false, hasCollider: true),
            "collider" => new ChildFact("Root/w", "stone_wall_2x1", networked: true, underNetworkedAncestor: false,
                enabledInHierarchy: true, hasRenderer: true, hasCollider: false),
            "ancestor" => new ChildFact("Root/w", "stone_wall_2x1", networked: true, underNetworkedAncestor: true,
                enabledInHierarchy: true, hasRenderer: true, hasCollider: true),
            "foreign" => Fixtures.Networked("stone_wall_2x1", "Root/w", foreignComponents: new[] { "Shrine" }),
            "referenced" => Fixtures.Networked("stone_wall_2x1", "Root/w", referencedPrefabs: new[] { "Greydwarf" }),
            _ => original,
        };

        Assert.NotEqual(
            TemplateFingerprint.Of(Facts(children: new[] { original })),
            TemplateFingerprint.Of(Facts(children: new[] { changed })));
    }

    [Fact]
    public void ARootThatBecomesInactiveMovesTheDigest()
    {
        var active = new TemplateFacts("MWL_X", "Meadows", children: new[] { Fixtures.Networked() });
        var inactive = new TemplateFacts("MWL_X", "Meadows", rootActive: false, children: new[] { Fixtures.Networked() });

        Assert.NotEqual(TemplateFingerprint.Of(active), TemplateFingerprint.Of(inactive));
    }

    [Fact]
    public void AnInteriorAppearingMovesTheDigest()
    {
        var plain = new TemplateFacts("MWL_X", "Meadows", children: new[] { Fixtures.Networked() });
        var withInterior = new TemplateFacts("MWL_X", "Meadows", children: new[] { Fixtures.Networked() },
            dungeonTheme: "MWL_BlackForestDungeon");

        Assert.NotEqual(TemplateFingerprint.Of(plain), TemplateFingerprint.Of(withInterior));
    }

    [Fact]
    public void FloatNoiseBelowWhatAnyRuleCanSeeDoesNotMoveTheDigest()
    {
        // Round-trip formatting differs between runtimes at the last digit, and
        // a fingerprint compared across machines has to survive that.
        string a = TemplateFingerprint.Of(Facts(terrain: new[] { Fixtures.Terrain(reach: 12.0000f) }));
        string b = TemplateFingerprint.Of(Facts(terrain: new[] { Fixtures.Terrain(reach: 12.00004f) }));

        Assert.Equal(a, b);
    }

    [Fact]
    public void TheCanonicalFormIsReadableSoARunCanSayWHICHFactMoved()
    {
        // A digest that differs tells nobody anything on its own.
        string canonical = TemplateFingerprint.Canonical(Facts(children: new[] { Fixtures.Networked() }));

        Assert.Contains("name=MWL_Example1", canonical);
        Assert.Contains("child=Root/stone_wall_2x1|stone_wall_2x1|", canonical);
    }

    [Fact]
    public void ThePolicyDigestMovesWhenTheRulesDoAndNotOtherwise()
    {
        var packs = new[] { "Ports" };
        string plain = TemplateFingerprint.OfPolicy(Fixtures.Stock, ComponentPolicy.Default, packs);

        Assert.Equal(plain, TemplateFingerprint.OfPolicy(Fixtures.Stock, ComponentPolicy.Default, packs));
        Assert.NotEqual(plain, TemplateFingerprint.OfPolicy(Fixtures.Stock, ComponentPolicy.Default, new[] { "Ports", "Traders" }));
        Assert.NotEqual(plain, TemplateFingerprint.OfPolicy(Fixtures.Stock,
            new ComponentPolicy(harmlessForeignComponents: new[] { "Marker" }), packs));
        // A different game build is different rules: the names were checked
        // against a different list of what the client has.
        Assert.NotEqual(plain, TemplateFingerprint.OfPolicy(
            StockPrefabRegistry.FromNames(new[] { "wood_floor" }, buildId: "other"), ComponentPolicy.Default, packs));
    }

    [Fact]
    public void ThePolicyDigestDoesNotDependOnTheORDEROfTheExcludedPacks()
    {
        Assert.Equal(
            TemplateFingerprint.OfPolicy(Fixtures.Stock, ComponentPolicy.Default, new[] { "Ports", "Traders" }),
            TemplateFingerprint.OfPolicy(Fixtures.Stock, ComponentPolicy.Default, new[] { "Traders", "Ports" }));
    }
}
