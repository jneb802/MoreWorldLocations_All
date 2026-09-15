using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// One <c>TerrainModifier</c> from a location template, as data.
///
/// A location's terrain modifiers have no <c>ZNetView</c> and
/// <c>m_useTerrainCompiler = false</c>, so nothing about them is persisted:
/// <c>Heightmap.ApplyModifiers</c> re-derives the shaping on every machine from
/// the live instances in the scene. A client that has the template instantiates
/// them through <c>SpawnMode.Client</c> and gets shaped ground. A client without
/// the mod never receives them and stands on ground the structure was not placed
/// for.
///
/// Reading the modifier into a value, away from the GameObject, is what lets the
/// server reproduce the same shaping through a persistent <c>TerrainComp</c>,
/// and what lets the conversion be tested without a scene.
/// </summary>
public readonly struct LocationTerrainOperation
{
    /// <summary>World position of the modifier, after the location's own placement.</summary>
    public readonly Vector3 Position;

    public readonly bool Level;
    public readonly float LevelRadius;

    /// <summary>
    /// Raises the point levelled TO, not the terrain: vanilla levels to
    /// <c>Position + up * LevelOffset</c>. `MWL_RuinsWell1` uses −2, which is
    /// what sinks its well below the surrounding ground.
    /// </summary>
    public readonly float LevelOffset;

    /// <summary>Square footprint rather than a circle. Applies to level and smooth alike.</summary>
    public readonly bool Square;

    public readonly bool Smooth;
    public readonly float SmoothRadius;
    public readonly float SmoothPower;

    public readonly bool Paint;
    public readonly float PaintRadius;
    public readonly float PaintStrength;
    public readonly TerrainModifier.PaintType PaintType;

    /// <summary>Skip vertices that stand above the modifier, as vanilla does.</summary>
    public readonly bool PaintHeightCheck;

    public LocationTerrainOperation(
        Vector3 position,
        bool level = false, float levelRadius = 0f, float levelOffset = 0f, bool square = false,
        bool smooth = false, float smoothRadius = 0f, float smoothPower = 3f,
        bool paint = false, float paintRadius = 0f, float paintStrength = 1f,
        TerrainModifier.PaintType paintType = TerrainModifier.PaintType.Dirt,
        bool paintHeightCheck = false)
    {
        Position = position;
        Level = level;
        LevelRadius = levelRadius;
        LevelOffset = levelOffset;
        Square = square;
        Smooth = smooth;
        SmoothRadius = smoothRadius;
        SmoothPower = smoothPower;
        Paint = paint;
        PaintRadius = paintRadius;
        PaintStrength = paintStrength;
        PaintType = paintType;
        PaintHeightCheck = paintHeightCheck;
    }

    /// <summary>
    /// How far from <see cref="Position"/> this operation can reach, so the
    /// planner knows which zones it touches. Vanilla's
    /// <c>TerrainModifier.GetRadius</c> takes the largest of the enabled radii.
    /// </summary>
    public float Radius
    {
        get
        {
            float radius = 0f;
            if (Level && LevelRadius > radius) radius = LevelRadius;
            if (Smooth && SmoothRadius > radius) radius = SmoothRadius;
            if (Paint && PaintRadius > radius) radius = PaintRadius;
            return radius;
        }
    }
}
