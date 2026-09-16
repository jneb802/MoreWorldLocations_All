using System;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// One terrain modifier of a template, as values.
///
/// <para><b>Why not the component.</b> The modifiers used to be cached as a
/// <c>List&lt;TerrainModifier&gt;</c> for the life of the world. A cached
/// component keeps its GameObject, its GameObject keeps the template, and the
/// template keeps the bundle — so every template that had ever been read stayed
/// resident whether or not anything still needed it, and the release that would
/// have unloaded it could never take effect. Reading the numbers out once and
/// keeping the numbers costs a few dozen bytes and holds nothing.</para>
///
/// <para>The position is root-relative, which is what makes it usable: a
/// placement is <c>position + rotation * local</c>, so the site's operations can
/// be computed at any placement and any rotation long after the template has
/// been released.</para>
/// </summary>
public readonly struct TerrainModifierDescriptor
{
    public TerrainModifierDescriptor(
        Vector3 localPosition, bool enabled, bool useTerrainCompiler, bool playerModification,
        int sortOrder, bool level, float levelRadius, float levelOffset, bool square,
        bool smooth, float smoothRadius, float smoothPower,
        bool paint, float paintRadius, float paintStrength,
        TerrainModifier.PaintType paintType, bool paintHeightCheck)
    {
        LocalPosition = localPosition;
        Enabled = enabled;
        UseTerrainCompiler = useTerrainCompiler;
        PlayerModification = playerModification;
        SortOrder = sortOrder;
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

    /// <summary>Where the modifier sits in the template's own space.</summary>
    public Vector3 LocalPosition { get; }

    /// <summary>Enabled, and under enabled parents: the set vanilla would apply.</summary>
    public bool Enabled { get; }

    /// <summary>Already writes through a compiler, so the client receives it and the conversion leaves it alone.</summary>
    public bool UseTerrainCompiler { get; }

    /// <summary>Vanilla's first sort key. A location's modifiers are never player ones.</summary>
    public bool PlayerModification { get; }

    /// <summary>Vanilla's second sort key, which decides the ground and not just the bookkeeping.</summary>
    public int SortOrder { get; }

    public bool Level { get; }
    public float LevelRadius { get; }
    public float LevelOffset { get; }
    public bool Square { get; }
    public bool Smooth { get; }
    public float SmoothRadius { get; }
    public float SmoothPower { get; }
    public bool Paint { get; }
    public float PaintRadius { get; }
    public float PaintStrength { get; }
    public TerrainModifier.PaintType PaintType { get; }
    public bool PaintHeightCheck { get; }

    /// <summary>The largest radius this modifier actually uses.</summary>
    public float Radius => Mathf.Max(
        Level ? LevelRadius : 0f,
        Mathf.Max(Smooth ? SmoothRadius : 0f, Paint ? PaintRadius : 0f));

    /// <summary>
    /// How far this modifier reaches from the template's origin, at ANY
    /// rotation. Rotation turns a point about the origin and cannot move it
    /// further from it, which is what lets readiness be asked before a placement
    /// has a rotation.
    /// </summary>
    public float Reach => new Vector2(LocalPosition.x, LocalPosition.z).magnitude + Radius;

    /// <summary>Read one modifier's numbers, once, while the template is still leased.</summary>
    public static TerrainModifierDescriptor Of(TerrainModifier modifier, Vector3 localPosition, bool enabled) =>
        new TerrainModifierDescriptor(
            localPosition,
            enabled && modifier.enabled,
            modifier.m_useTerrainCompiler,
            modifier.m_playerModifiction,
            modifier.m_sortOrder,
            modifier.m_level, modifier.m_levelRadius, modifier.m_levelOffset, modifier.m_square,
            modifier.m_smooth, modifier.m_smoothRadius, modifier.m_smoothPower,
            modifier.m_paintCleared, modifier.m_paintRadius, modifier.m_paintStrength,
            modifier.m_paintType, modifier.m_paintHeightCheck);
}

/// <summary>
/// A template's terrain, as values, in the order the template declares it.
///
/// This is what the conversion, the readiness barrier and the site preflight all
/// read. One description per template, kept for the world, holding no Unity
/// object at all.
/// </summary>
public sealed class TerrainTemplate
{
    /// <summary>A template that loaded and has no terrain. Distinct from one that would not load.</summary>
    public static readonly TerrainTemplate None = new TerrainTemplate(new TerrainModifierDescriptor[0]);

    public TerrainTemplate(IReadOnlyList<TerrainModifierDescriptor> modifiers)
    {
        Modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
        float reach = 0f;
        foreach (TerrainModifierDescriptor modifier in modifiers)
        {
            if (modifier.Enabled && !modifier.UseTerrainCompiler)
                reach = Mathf.Max(reach, modifier.Reach);
        }
        Reach = reach;
    }

    /// <summary>In the order they appear in the template, which is vanilla's tie-break.</summary>
    public IReadOnlyList<TerrainModifierDescriptor> Modifiers { get; }

    /// <summary>The furthest any of them reaches from the origin, at any rotation.</summary>
    public float Reach { get; }

    public bool ShapesGround => Reach > 0f || Modifiers.Count > 0;

    /// <summary>
    /// Read a whole template's terrain while it is leased.
    ///
    /// <c>Utils.GetEnabledComponentsInChildren</c> is what vanilla itself uses to
    /// read a location's children, so this is the set the game would apply —
    /// including the rule that a child under an inactive parent does not count.
    /// </summary>
    public static TerrainTemplate Read(GameObject asset)
    {
        if (asset == null) throw new ArgumentNullException(nameof(asset));

        TerrainModifier[] found = global::Utils.GetEnabledComponentsInChildren<TerrainModifier>(asset);
        var modifiers = new List<TerrainModifierDescriptor>(found.Length);
        foreach (TerrainModifier modifier in found)
        {
            if (modifier == null)
                continue;
            modifiers.Add(TerrainModifierDescriptor.Of(
                modifier, asset.transform.InverseTransformPoint(modifier.transform.position), enabled: true));
        }
        return new TerrainTemplate(modifiers);
    }

    /// <summary>
    /// The site's operations at one placement.
    ///
    /// The same arithmetic the game uses — <c>placement + rotation *
    /// localPosition</c> — and the only place it is written, so the preflight
    /// and the writer cannot disagree about where a site's ground is.
    /// </summary>
    public List<LocationTerrainOperation> OperationsAt(Vector3 placement, Quaternion rotation)
    {
        var operations = new List<LocationTerrainOperation>(Modifiers.Count);
        foreach (TerrainModifierDescriptor modifier in Modifiers)
        {
            // A modifier the game itself would skip is skipped for the same
            // reason and no other: ApplyModifiers tests enabled, and one that
            // already writes through a compiler would shape the ground twice.
            if (!modifier.Enabled || modifier.UseTerrainCompiler)
                continue;
            operations.Add(new LocationTerrainOperation(
                placement + rotation * modifier.LocalPosition,
                level: modifier.Level,
                levelRadius: modifier.LevelRadius,
                levelOffset: modifier.LevelOffset,
                square: modifier.Square,
                smooth: modifier.Smooth,
                smoothRadius: modifier.SmoothRadius,
                smoothPower: modifier.SmoothPower,
                paint: modifier.Paint,
                paintRadius: modifier.PaintRadius,
                paintStrength: modifier.PaintStrength,
                paintType: modifier.PaintType,
                paintHeightCheck: modifier.PaintHeightCheck,
                sortOrder: modifier.SortOrder));
        }
        return operations;
    }
}
