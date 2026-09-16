using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// mwl_plan prints the template's own terrain operations ahead of the paged
/// children, so a client observation can sample the ground where the template
/// says it shaped it, with the operation's centre, radius and height.
/// </summary>
[Collection("templates")]
public sealed class TemplatePlanTests
{
    [Fact]
    public void TerrainOperationsRideAheadOfTheChildrenOnEveryPage()
    {
        using var world = new TerrainWorld();
        world.LocationWithLevelModifier("PlanSite", atX: 4f, levelTo: 1.5f, radius: 3f);

        List<string> page1 = TemplatePlan.Render("PlanSite", skip: 0, take: 1).ToList();
        List<string> page2 = TemplatePlan.Render("PlanSite", skip: 1, take: 1).ToList();

        foreach (List<string> page in new[] { page1, page2 })
        {
            string terrain = Assert.Single(page.Where(l => l.StartsWith("# terrain\t", System.StringComparison.Ordinal)));
            string[] f = terrain.Split('\t');
            Assert.Equal("PlanSite/terrain", f[1]);
            Assert.Equal("4", f[2]);
            Assert.Equal("level=1", f[5]);
            Assert.Equal("3", f[6]);
            Assert.Equal("1.5", f[7]);
            Assert.Equal("smooth=0", f[9]);
            Assert.Equal("paint=0", f[12]);
            Assert.Equal("enabled=1", f[17]);
        }
        // The terrain row is a comment: the header still counts children, not operations.
        Assert.Contains(page1, l => l.StartsWith("# plan PlanSite ", System.StringComparison.Ordinal) && l.Contains(" terrain=1"));
    }

    [Fact]
    public void ATemplateWithoutTerrainPrintsNoTerrainRow()
    {
        using var world = new TerrainWorld();
        world.LocationWithoutTerrain("FlatSite");
        List<string> lines = TemplatePlan.Render("FlatSite").ToList();
        Assert.DoesNotContain(lines, l => l.StartsWith("# terrain", System.StringComparison.Ordinal));
        Assert.Contains(lines, l => l.Contains(" terrain=0"));
    }
}
