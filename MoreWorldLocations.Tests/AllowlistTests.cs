using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// What a server-only world is allowed to place.
///
/// The failure these guard against is quiet: a template nobody audited gets
/// registered, the world places it, and a player with no mod walks up to a
/// collection of ZDOs with no floor under them. So the rule is membership by
/// exact name, unknown means blocked, and four packs are excluded whatever
/// their names say.
/// </summary>
public class AllowlistTests
{
    private static MWLLocation[] Pack(params string[] names) =>
        names.Select(n => new MWLLocation { Name = n }).ToArray();

    [Fact]
    public void AnUnauditedTemplateIsBlocked()
    {
        Assert.False(ServerOnlyAllowlist.Allows("Meadows", "MWL_NeverAudited"));
    }

    [Fact]
    public void OnlyTemplatesWatchedEndToEndAreApproved()
    {
        // The rule this replaces was "empty until the audit passes something".
        // The audit is screening; what ships is what has been watched on a
        // client without the mod. These four have that evidence and nothing
        // else does -- MWL_FulingRock1 in particular proved how the terrain
        // writer behaves at a zone boundary and under a failure, and proved
        // nothing about its 87 objects.
        Assert.Equal(
            new[] { "MWL_MeadowsTomb4", "MWL_Ruins1", "MWL_RuinsWell1", "MWL_WoodTower2" },
            ServerOnlyAllowlist.Approved.OrderBy(n => n));
        Assert.DoesNotContain("MWL_FulingRock1", ServerOnlyAllowlist.Approved);
    }

    [Fact]
    public void AnApprovedTemplateInAnExcludedPackIsStillRefused()
    {
        // Membership in Approved is not a way past the pack exclusions.
        Assert.False(ServerOnlyAllowlist.Allows("Dungeons", "MWL_Ruins1"));
        Assert.True(ServerOnlyAllowlist.Allows("Meadows", "MWL_Ruins1"));
    }

    [Theory]
    [InlineData("Ports")]
    [InlineData("Traders")]
    [InlineData("Trainers")]
    [InlineData("Dungeons")]
    public void AnExcludedPackIsRefusedWhateverItContains(string pack)
    {
        // Listed separately from the approved set because RegisterAll registers
        // Dungeons unconditionally: excluding the pack is what actually keeps
        // dungeon interiors out, not the port and trader toggles.
        Assert.Contains(pack, ServerOnlyAllowlist.ExcludedPacks);

        // Against an approved set that DOES contain the name. Asking with the
        // shipped (empty) set would pass whether or not the pack guard exists.
        HashSet<string> approved = new() { "MWL_Anything" };
        Assert.False(ServerOnlyAllowlist.Allows(pack, "MWL_Anything", approved));
        Assert.Empty(ServerOnlyAllowlist.Filter(pack, Pack("MWL_Anything"), approved));
    }

    [Fact]
    public void FilterKeepsOnlyApprovedNamesAndPreservesOrder()
    {
        // Order matters: placement walks the registered list, and letting a set's
        // iteration order decide which sites are considered first is the shape of
        // bug that once chose which POIs got roads. The approved set is passed in
        // so this exercises the shipped Filter rather than a copy of its rule.
        HashSet<string> approved = new() { "MWL_D", "MWL_B" };
        List<MWLLocation> kept =
            ServerOnlyAllowlist.Filter("Meadows", Pack("MWL_A", "MWL_B", "MWL_C", "MWL_D"), approved)
                .ToList();

        Assert.Equal(new[] { "MWL_B", "MWL_D" }, kept.Select(l => l.Name).ToArray());
    }

    [Fact]
    public void FilterIsCaseSensitiveSoANearMissIsBlocked()
    {
        // Membership is by exact name: "mwl_ruins1" is not MWL_Ruins1, and a
        // template that resolves by near-miss is a template nobody audited.
        HashSet<string> approved = new() { "MWL_Ruins1" };
        Assert.Empty(ServerOnlyAllowlist.Filter("Meadows", Pack("mwl_ruins1"), approved));
        Assert.Single(ServerOnlyAllowlist.Filter("Meadows", Pack("MWL_Ruins1"), approved));
    }

    [Fact]
    public void AnExcludedPackIsRefusedEvenWhenTheNameIsApproved()
    {
        // The decisive case the real allowlist cannot show while it is empty.
        HashSet<string> approved = new() { "MWL_Ruins1" };
        Assert.False(ServerOnlyAllowlist.Allows("Dungeons", "MWL_Ruins1", approved));
        Assert.True(ServerOnlyAllowlist.Allows("Meadows", "MWL_Ruins1", approved));
    }

    [Fact]
    public void FilterOnTheRealAllowlistDropsEverythingUnapproved()
    {
        MWLLocation[] pack = Pack("MWL_Ruins1", "MWL_RuinsWell1", "MWL_NeverAudited");
        List<MWLLocation> kept = ServerOnlyAllowlist.Filter("Meadows", pack).ToList();

        Assert.All(kept, l => Assert.Contains(l.Name, ServerOnlyAllowlist.Approved));
        Assert.DoesNotContain(kept, l => l.Name == "MWL_NeverAudited");
    }

    [Fact]
    public void FilterOnAnExcludedPackKeepsNothing()
    {
        Assert.Empty(ServerOnlyAllowlist.Filter("Dungeons", Pack("MWL_Ruins1", "MWL_Anything")));
    }
}
