using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// The emission trace records what vanilla selected, what it emitted and what
/// was destroyed, at the boundaries where those happen, and nothing else.
///
/// <para>Harmony's attachment of the hooks is station-tested; these drive the
/// recording methods directly with the same arguments the hooks pass.</para>
/// </summary>
[Collection("templates")]
public sealed class EmissionTraceTests : IDisposable
{
    private readonly List<string> _rows = new();

    public EmissionTraceTests()
    {
        ZDOMan.instance = new ZDOMan();
        EmissionTrace.Forget();
        EmissionTrace.Configure(new[] { "MWL_Traced" }, all: false);
        EmissionTrace.Sink = _rows.Add;
    }

    public void Dispose()
    {
        EmissionTrace.Sink = null;
        EmissionTrace.Configure(Array.Empty<string>(), all: false);
        EmissionTrace.Forget();
    }

    private IEnumerable<string[]> Rows(string kind) =>
        _rows.Where(r => r.StartsWith(EmissionTrace.Tag + " " + kind + "\t", StringComparison.Ordinal))
            .Select(r => r.Substring(EmissionTrace.Tag.Length + 1).Split('\t').Skip(1).ToArray());

    /// <summary>A template: root, a Blueprint node, and children under it.</summary>
    private static GameObject Template(string name, out GameObject blueprint)
    {
        var root = new GameObject(name);
        blueprint = root.Child("Blueprint");
        return root;
    }

    private static GameObject Networked(GameObject parent, string prefab, float x, float z)
    {
        GameObject go = parent.Child(prefab);
        go.transform.position = new Vector3(x, 0f, z);
        go.AddComponent<ZNetView>();
        return go;
    }

    private static ZNetView[] Enabled(GameObject root) => global::Utils.GetEnabledComponentsInChildren<ZNetView>(root);

    private static void Begin(GameObject root, Vector3 at, int seed = 7) =>
        EmissionTrace.BeginSite(root.name, root, seed, at, Quaternion.identity, "Ghost", "fp", Enabled(root));

    /// <summary>The clone vanilla instantiates: same prefab name plus "(Clone)", at the world position.</summary>
    private static (ZNetView view, ZDO zdo) Clone(string prefab, Vector3 at)
    {
        var go = new GameObject(prefab + "(Clone)");
        go.transform.position = at;
        var zdo = ZDOMan.instance!.CreateNewZDO(at, prefab.GetStableHashCode());
        var view = go.AddComponent<ZNetView>();
        view.Zdo = zdo;
        return (view, zdo);
    }

    // ---- the switch ----------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void AnUnsetSwitchTracesNothing(string? value)
    {
        Assert.False(ValidationSwitches.ParseEmissionTrace(value, out var names, out bool all));
        Assert.Empty(names);
        Assert.False(all);
    }

    [Fact]
    public void TheSwitchNamesTemplatesOrEverything()
    {
        Assert.True(ValidationSwitches.ParseEmissionTrace(" MWL_A, MWL_B ", out var names, out bool all));
        Assert.Equal(new[] { "MWL_A", "MWL_B" }, names.OrderBy(n => n).ToArray());
        Assert.False(all);
        Assert.True(ValidationSwitches.ParseEmissionTrace("*", out names, out all));
        Assert.True(all);
    }

    [Fact]
    public void AnUntracedTemplateProducesNoRowsAndNoSite()
    {
        GameObject root = Template("MWL_Other", out GameObject bp);
        Networked(bp, "wood_floor", 1f, 0f);
        Begin(root, new Vector3(100f, 30f, 200f));
        Assert.Null(EmissionTrace.CurrentSite);
        EmissionTrace.Randomised("RandomSpawn", bp, true, EmissionTrace.NetworkedUnder(bp));
        EmissionTrace.EndSite();
        Assert.Empty(_rows);
    }

    // ---- selection -----------------------------------------------------------

    [Fact]
    public void AnUnselectedBranchIsRecordedWithItsMembersInactive()
    {
        GameObject root = Template("MWL_Traced", out GameObject bp);
        GameObject branch = bp.Child("Random1");
        branch.AddComponent<RandomSpawn>();
        GameObject chest = Networked(branch, "piece_chest_wood", 2f, 3f);
        GameObject wall = Networked(bp, "stone_wall_2x1", 0f, 1f);
        Begin(root, new Vector3(100f, 30f, 200f));

        // Vanilla's SetSpawned(false): the branch and every networked object under it.
        branch.activeSelf = false;
        chest.activeSelf = false;
        EmissionTrace.Randomised("RandomSpawn", branch, branch.activeSelf, EmissionTrace.NetworkedUnder(branch));

        string[] r = Assert.Single(Rows("R"));
        Assert.Equal(new[] { "MWL_Traced", "100,30,200", "7", "RandomSpawn", "MWL_Traced/Blueprint/Random1", "0" }, r);
        string[] c = Assert.Single(Rows("C"));
        Assert.Equal("MWL_Traced/Blueprint/Random1/piece_chest_wood", c[4]);
        Assert.Equal("piece_chest_wood", c[5]);
        Assert.Equal("0", c[6]);
        // The wall is not under the randomiser and is not a member of it.
        Assert.DoesNotContain(Rows("C"), row => row[4].EndsWith("stone_wall_2x1", StringComparison.Ordinal));
        _ = wall;
    }

    [Fact]
    public void ASelectedBranchRecordsItsMembersActive()
    {
        GameObject root = Template("MWL_Traced", out GameObject bp);
        GameObject branch = bp.Child("Random1");
        branch.AddComponent<RandomSpawn>();
        Networked(branch, "piece_chest_wood", 2f, 3f);
        Begin(root, Vector3.zero);
        EmissionTrace.Randomised("RandomSpawn", branch, true, EmissionTrace.NetworkedUnder(branch));
        Assert.Equal("1", Assert.Single(Rows("R"))[5]);
        Assert.Equal("1", Assert.Single(Rows("C"))[6]);
    }

    [Fact]
    public void ARandomiserThatIsItselfNetworkedListsItself()
    {
        // RandomSpawn on an object carrying a ZNetView: SetSpawned(false)
        // deactivates that object too, so it must be a member of its own record.
        GameObject root = Template("MWL_Traced", out GameObject bp);
        GameObject self = Networked(bp, "Spawner_GreydwarfNest", 4f, 4f);
        self.AddComponent<RandomSpawn>();
        Begin(root, Vector3.zero);
        self.activeSelf = false;
        EmissionTrace.Randomised("RandomSpawn", self, false, EmissionTrace.NetworkedUnder(self));
        string[] c = Assert.Single(Rows("C"));
        Assert.Equal("MWL_Traced/Blueprint/Spawner_GreydwarfNest", c[3]);
        Assert.Equal(c[3], c[4]);
        Assert.Equal("0", c[6]);
    }

    // ---- emission ------------------------------------------------------------

    [Fact]
    public void AnEmittedObjectIsMatchedToItsMemberByNameAndPosition()
    {
        GameObject root = Template("MWL_Traced", out GameObject bp);
        Networked(bp, "wood_floor", 1f, 0f);
        Networked(bp, "wood_floor", 3f, 0f);
        var site = new Vector3(100f, 30f, 200f);
        Begin(root, site);
        Assert.Equal("2", Assert.Single(Rows("S"))[5]);

        var (view, zdo) = Clone("wood_floor", new Vector3(103f, 30f, 200f));
        EmissionTrace.Emitted(view, zdo);
        EmissionTrace.EndSite();

        string[] e = Assert.Single(Rows("E"));
        Assert.Equal("MWL_Traced/Blueprint/wood_floor", e[3]);
        Assert.Equal("wood_floor", e[4]);
        Assert.Equal(zdo.m_uid.ToString(), e[5]);
        Assert.Equal("103,30,200", e[6]);
        Assert.Equal("1", Assert.Single(Rows("Z"))[3]);
        Assert.Null(EmissionTrace.CurrentSite);
    }

    [Fact]
    public void AMemberPositionIsTakenRelativeToTheRootAsVanillaResetsIt()
    {
        // The asset root stands at (10,0,10) in the loaded bundle. Vanilla
        // moves it to the origin before spawning, so a child at (11,0,10)
        // lands at site + (1,0,0), and that is where the clone is matched.
        GameObject root = Template("MWL_Traced", out GameObject bp);
        root.transform.position = new Vector3(10f, 0f, 10f);
        Networked(bp, "wood_floor", 11f, 10f);
        Begin(root, new Vector3(100f, 30f, 200f));
        var (view, zdo) = Clone("wood_floor", new Vector3(101f, 30f, 200f));
        EmissionTrace.Emitted(view, zdo);
        Assert.Equal("MWL_Traced/Blueprint/wood_floor", Assert.Single(Rows("E"))[3]);
    }

    [Fact]
    public void AnObjectNothingPredictedIsRecordedAsUnmatchedNotGuessed()
    {
        GameObject root = Template("MWL_Traced", out GameObject bp);
        Networked(bp, "wood_floor", 1f, 0f);
        Begin(root, Vector3.zero);
        var (view, zdo) = Clone("wood_floor", new Vector3(5f, 0f, 5f));
        EmissionTrace.Emitted(view, zdo);
        Assert.Equal("?", Assert.Single(Rows("E"))[3]);
    }

    [Fact]
    public void AnEmissionOutsideAnyTracedSiteIsNotRecorded()
    {
        var (view, zdo) = Clone("wood_floor", Vector3.zero);
        EmissionTrace.Emitted(view, zdo);
        Assert.Empty(_rows);
        Assert.Equal(0, EmissionTrace.Watched);
    }

    // ---- destruction ---------------------------------------------------------

    [Fact]
    public void ATracedObjectsDestructionIsRecordedWithItsPathAndWho()
    {
        GameObject root = Template("MWL_Traced", out GameObject bp);
        Networked(bp, "wood_floor", 1f, 0f);
        Begin(root, new Vector3(100f, 30f, 200f));
        var (view, zdo) = Clone("wood_floor", new Vector3(101f, 30f, 200f));
        EmissionTrace.Emitted(view, zdo);
        EmissionTrace.EndSite();
        Assert.Equal(1, EmissionTrace.Watched);

        EmissionTrace.Destroyed(zdo.m_uid, "peer 42");
        string[] x = Assert.Single(Rows("X"));
        Assert.Equal(new[] { "MWL_Traced", "100,30,200", "7", "MWL_Traced/Blueprint/wood_floor", "wood_floor", zdo.m_uid.ToString(), "peer 42" }, x);
        Assert.Equal(0, EmissionTrace.Watched);

        // Twice is once: the second destroy of a forgotten id is nothing.
        EmissionTrace.Destroyed(zdo.m_uid, "peer 42");
        Assert.Single(Rows("X"));
    }

    [Fact]
    public void AnUntracedObjectsDestructionIsSilent()
    {
        var other = ZDOMan.instance!.CreateNewZDO(Vector3.zero, 1);
        EmissionTrace.Destroyed(other.m_uid, "peer 1");
        Assert.Empty(_rows);
    }

    [Fact]
    public void ANewWorldForgetsTheSiteAndTheWatchedObjects()
    {
        GameObject root = Template("MWL_Traced", out GameObject bp);
        Networked(bp, "wood_floor", 1f, 0f);
        Begin(root, Vector3.zero);
        var (view, zdo) = Clone("wood_floor", new Vector3(1f, 0f, 0f));
        EmissionTrace.Emitted(view, zdo);
        EmissionTrace.Forget();
        Assert.Null(EmissionTrace.CurrentSite);
        Assert.Equal(0, EmissionTrace.Watched);
        EmissionTrace.Destroyed(zdo.m_uid, "peer 1");
        Assert.Empty(Rows("X"));
    }
}
