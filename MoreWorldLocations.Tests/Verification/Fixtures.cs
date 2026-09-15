using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly.Verification;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// Templates to judge, built by hand.
///
/// The facts a real template produces come from walking a resolved asset, and
/// nothing about that walk can run without Unity. What the policy consumes is
/// values, so the fixtures are values — which is the point of the split. A
/// fixture here is a claim about a SHAPE of template ("one that carries a
/// custom component on an object the client receives"), not a copy of a
/// particular build.
/// </summary>
public static class Fixtures
{
    /// <summary>
    /// A small stock registry. Real vanilla names, so that a test reading
    /// "stone_wall_2x1" is reading the same thing the game would.
    /// </summary>
    public static readonly StockPrefabRegistry Stock = StockPrefabRegistry.FromNames(new[]
    {
        "stone_wall_2x1",
        "wood_floor",
        "piece_chest_wood",
        "Greydwarf",
        "Coins",
        "Ruby",
        "Skeleton",
        "loot_chest_wood",
        "vfx_Place_wood_wall",
        "Spawner_GreydwarfNest",
    }, buildId: "test-build");

    public static readonly IReadOnlyCollection<string> ExcludedPacks =
        new[] { "Ports", "Traders", "Trainers", "Dungeons" };

    /// <summary>An ordinary networked child of a stock prefab: the case everything else varies from.</summary>
    public static ChildFact Networked(
        string prefabName = "stone_wall_2x1",
        string? path = null,
        bool enabled = true,
        bool persistent = true,
        bool syncInitialScale = false,
        Scale3 scale = default,
        IReadOnlyList<string>? foreignComponents = null,
        IReadOnlyList<string>? referencedPrefabs = null) =>
        new ChildFact(
            path ?? ("Root/" + prefabName),
            prefabName,
            networked: true,
            underNetworkedAncestor: false,
            enabledInHierarchy: enabled,
            persistent: persistent,
            syncInitialScale: syncInitialScale,
            scale: scale.X == 0f && scale.Y == 0f && scale.Z == 0f ? Scale3.One : scale,
            hasRenderer: true,
            hasCollider: true,
            components: new[] { "ZNetView", "WearNTear" },
            foreignComponents: foreignComponents,
            referencedPrefabs: referencedPrefabs);

    /// <summary>An object with no ZNetView and no networked ancestor: only a client with the template builds it.</summary>
    public static ChildFact ProxyOnly(
        string prefabName = "DecorativeArch",
        string? path = null,
        bool enabled = true,
        bool hasRenderer = true,
        bool hasCollider = true) =>
        new ChildFact(
            path ?? ("Root/" + prefabName),
            prefabName,
            networked: false,
            underNetworkedAncestor: false,
            enabledInHierarchy: enabled,
            hasRenderer: hasRenderer,
            hasCollider: hasCollider);

    /// <summary>A non-networked object INSIDE a networked one: part of the stock prefab, so the client has it.</summary>
    public static ChildFact InsideNetworked(string prefabName = "wall_detail", string? path = null) =>
        new ChildFact(
            path ?? ("Root/stone_wall_2x1/" + prefabName),
            prefabName,
            networked: false,
            underNetworkedAncestor: true,
            enabledInHierarchy: true,
            hasRenderer: true,
            hasCollider: true);

    /// <summary>A terrain modifier at the origin with a modest radius: convertible, and inside one zone.</summary>
    public static TerrainFact Terrain(
        string path = "Root/Terrain1",
        bool enabled = true,
        bool useTerrainCompiler = false,
        float localOffsetDistance = 0f,
        float reach = 12f,
        bool level = true,
        bool smooth = true,
        bool paint = false,
        string paintType = "") =>
        new TerrainFact(path, enabled, useTerrainCompiler, localOffsetDistance, reach, level, smooth, paint, paintType);

    /// <summary>A template that passes every rule. Every blocked fixture is this with one thing changed.</summary>
    public static TemplateFacts Compatible(
        string name = "MWL_Example1",
        string pack = "Meadows",
        IEnumerable<ChildFact>? children = null,
        IEnumerable<TerrainFact>? terrain = null) =>
        new TemplateFacts(
            name,
            pack,
            children: (children ?? new[] { Networked(), Networked("wood_floor", "Root/wood_floor") }).ToList(),
            terrain: (terrain ?? new[] { Terrain() }).ToList());

    public static TemplateEvaluation Evaluate(TemplateFacts facts, StockPrefabRegistry? registry = null, ComponentPolicy? components = null) =>
        TemplatePolicy.Evaluate(facts, registry ?? Stock, ExcludedPacks, components);

    /// <summary>Whether a verdict was reached for this reason, so a test names the code rather than a count.</summary>
    public static bool HasCode(TemplateEvaluation evaluation, string code) =>
        evaluation.Findings.Any(f => f.Code == code);

    public static TemplateFinding Finding(TemplateEvaluation evaluation, string code) =>
        evaluation.Findings.Single(f => f.Code == code);
}
