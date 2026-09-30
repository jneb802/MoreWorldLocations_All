using UnityEngine;

/// <summary>
/// Builds a zone's generated terrain the way MWL's tests describe it, on the package's builder: the build the game's builder
/// thread would have finished for the zone's centre, ready for the next IsTerrainReady (Valheim.Testing.Doubles MakeReady).
/// </summary>
internal static class TerrainBuilds
{
    public static HeightmapBuilder.HMBuildData Build(this HeightmapBuilder builder, Vector2s zone, WorldGenerator world, int width = 64, float scale = 1f) =>
        builder.MakeReady(ZoneSystem.GetZonePos(zone), width, scale, false, world);
}
