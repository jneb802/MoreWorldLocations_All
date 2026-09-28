using UnityEngine;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Reused verbatim from ProceduralRoads' test harness (feature/server-only),
/// where it stood in for Valheim's world generator headlessly.
///
/// Deterministic pseudo-Valheim world: an island with a gaussian hill profile,
/// a mountain ridge, and an optional river channel crossing it. Heights use
/// the same conventions as the game (sea level 30, deep water below 28).
/// </summary>
public class SyntheticWorld : WorldGenerator
{
    public bool HasRiver = true;

    /// <summary>River channel runs roughly north-south near x = RiverX.</summary>
    public float RiverX = 100f;
    public float RiverHalfWidth = 24f;

    public bool HasMountain = true;
    public float MountainX = -250f;
    public float MountainHalfWidth = 140f;
    public float MountainHeight = 42f;

    public float IslandRadius = 600f;
    public float IslandPeakHeight = 18f; // above sea level at the island center

    /// <summary>
    /// When set, every point is at this height.
    ///
    /// For tests about a decision rather than about terrain: flat ground makes
    /// the arithmetic checkable by hand, which is the only reason to want it.
    /// </summary>
    public float? FlatHeight;

    public override float GetHeight(float wx, float wy)
    {
        if (FlatHeight.HasValue)
            return FlatHeight.Value;

        float r = Mathf.Sqrt(wx * wx + wy * wy);

        // Island: smooth dome from sea floor (20) to a low inland plateau.
        float t = Mathf.Clamp01(1f - r / IslandRadius);
        float height = 20f + (10f + IslandPeakHeight) * Mathf.SmoothStep(0f, 1f, t * 1.6f);

        // Gentle deterministic roughness so the terrain is not perfectly flat.
        height += Noise(wx * 0.02f, wy * 0.02f) * 1.5f;

        // Mountain ridge running north-south.
        if (HasMountain)
        {
            float md = Mathf.Abs(wx - MountainX - Mathf.Sin(wy * 0.004f) * 40f);
            float mt = Mathf.Clamp01(1f - md / MountainHalfWidth);
            height += MountainHeight * mt * mt;
        }

        // River carves the terrain down below sea level in its channel.
        if (HasRiver)
        {
            float weight = RiverWeightAt(wx, wy);
            if (weight > 0f)
                height = Mathf.Lerp(height, 26f, weight);
        }

        return height;
    }

    public override Heightmap.Biome GetBiome(float wx, float wy)
    {
        if (GetHeight(wx, wy) < ShimWorld.SeaLevel - 2f)
            return Heightmap.Biome.Ocean;
        if (HasMountain && Mathf.Abs(wx - MountainX) < MountainHalfWidth * 0.6f)
            return Heightmap.Biome.Mountain;
        return Heightmap.Biome.Meadows;
    }

    public override void GetRiverWeight(float wx, float wy, out float weight, out float width)
    {
        weight = HasRiver ? RiverWeightAt(wx, wy) : 0f;
        width = weight > 0f ? RiverHalfWidth * 2f : 0f;
    }

    private float RiverWeightAt(float wx, float wy)
    {
        // Meandering channel, only inside the island footprint.
        float channelX = RiverX + Mathf.Sin(wy * 0.006f) * 60f;
        float d = Mathf.Abs(wx - channelX);
        if (d >= RiverHalfWidth)
            return 0f;
        return Mathf.Clamp01(1f - d / RiverHalfWidth);
    }

    /// <summary>Cheap deterministic value noise (no Unity PerlinNoise available headless).</summary>
    private static float Noise(float x, float y)
    {
        int xi = (int)System.Math.Floor(x);
        int yi = (int)System.Math.Floor(y);
        float xf = x - xi, yf = y - yi;

        float a = Hash(xi, yi);
        float b = Hash(xi + 1, yi);
        float c = Hash(xi, yi + 1);
        float d = Hash(xi + 1, yi + 1);

        float u = xf * xf * (3f - 2f * xf);
        float v = yf * yf * (3f - 2f * yf);

        return Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
    }

    private static float Hash(int x, int y)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0x7fffffff) / (float)int.MaxValue;
        }
    }
}
