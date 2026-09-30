using More_World_Locations_AIO.ServerOnly;
using UnityEngine;
using Valheim.Testing;
using Xunit;
namespace More_World_Locations_AIO.Tests;

public class SharedZoneConversionTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void RealConversionLevelsAcrossBoundaryAndPreservesVergeInEitherOrder(bool reverse)
    {
        var terrain=new CompositeTerrain(new PlaneTerrain(60,.125f),
            TerrainRegion.Rectangle(80,-32,120,32,new PlaneTerrain(80,0,0,TerrainBiome.Mountain)));
        var paint=new PaintRgba(.2f,.4f,.6f,.3f);
        var world=new TerrainWorldState();
        for(int zone=0;zone<2;zone++) world.Add(zone,0,new TerrainZoneState(
            new TerrainGrid<float>(65,65,zone*64-32,-32,1,terrain.GetHeight),
            new TerrainGrid<PaintRgba>(65,65,zone*64-32,-32,1,(x,z)=>paint)));
        var before=world.Clone();
        var op=new LocationTerrainOperation(new Vector3(32,66,0),level:true,levelRadius:3,square:true,
            smooth:true,smoothRadius:3,paint:true,paintRadius:3);
        foreach(int zone in reverse ? new[]{1,0} : new[]{0,1})
        {
            var state=world[zone,0];var input=before[zone,0];
            var deltas=new TerrainZoneDeltas(new Vector3(zone*64,0,0),64,1);
            for(int z=0;z<65;z++) for(int x=0;x<65;x++)
            {
                var p=input.Paint[x,z];deltas.PaintMask[z*65+x]=new Color(p.R,p.G,p.B,p.A);
            }
            TerrainConversion.VertexHeight ground=(x,z)=>input.Heights[x,z];
            Assert.True(TerrainConversion.ApplyOnce("shared-boundary",op,deltas,ground));
            for(int z=0;z<65;z++) for(int x=0;x<65;x++)
            {
                int i=z*65+x;
                // Test-side compiler interpretation; this does not simulate native save/reload.
                state.Heights[x,z]=input.Heights[x,z]+deltas.LevelDelta[i]+deltas.SmoothDelta[i];
                var p=deltas.PaintMask[i];state.Paint[x,z]=new PaintRgba(p.r,p.g,p.b,p.a);
            }
            var once=deltas.Clone();
            Assert.False(TerrainConversion.ApplyOnce("shared-boundary",op,deltas,ground));
            Assert.Equal(once.LevelDelta,deltas.LevelDelta);Assert.Equal(once.SmoothDelta,deltas.SmoothDelta);
            Assert.Equal(once.PaintMask,deltas.PaintMask);
        }
        for(int z=30;z<=34;z++)
        {
            Assert.Equal(66,world[0,0].Heights[64,z],3);Assert.Equal(66,world[1,0].Heights[0,z],3);
            Assert.Equal(world[0,0].Paint[64,z],world[1,0].Paint[0,z]);
            Assert.Equal(.3f,world[0,0].Paint[64,z].A);
        }
        Assert.NotEqual(paint,world[0,0].Paint[64,32]);
        Assert.Equal(80,world[1,0].Heights[56,48]); // untouched terrace at world (88,16)
        for(int zone=0;zone<2;zone++)
        {
            int edge=zone==0?64:0;
            Assert.Equal(64,world[zone,0].Heights[edge,44]);
            Assert.Equal(paint,world[zone,0].Paint[edge,44]);
            Assert.Equal(64,before[zone,0].Heights[edge,32]);
            Assert.Equal(paint,before[zone,0].Paint[edge,32]);
        }
    }
}
