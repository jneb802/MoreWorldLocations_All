using System.Collections.Generic;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// A template's local scale, as three numbers.
///
/// Deliberately not <c>UnityEngine.Vector3</c>: everything in this file is read
/// off a resolved template once and then reasoned about without the engine, and
/// a facts model that needs Unity to exist is a facts model the policy cannot
/// be tested against.
/// </summary>
public readonly struct Scale3
{
    public Scale3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public static readonly Scale3 One = new Scale3(1f, 1f, 1f);

    /// <summary>
    /// Whether this scale is one within the tolerance a float round-trip leaves.
    ///
    /// Jötunn copies the mock's scale onto the real prefab, and an authored
    /// 1.0 that has been through a serialiser can come back as 0.99999994. A
    /// scale that differs from one only there is not a scale the author set.
    /// </summary>
    public bool IsUnit =>
        System.Math.Abs(X - 1f) < 1e-4f &&
        System.Math.Abs(Y - 1f) < 1e-4f &&
        System.Math.Abs(Z - 1f) < 1e-4f;

    public override string ToString() =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}, {1}, {2}]", X, Y, Z);
}

/// <summary>
/// Three numbers of a transform: a position, or a set of Euler angles.
///
/// Separate from <see cref="Scale3"/> because the two are compared differently:
/// a scale of one is the authored default and worth a tolerance, a position of
/// zero is just the origin.
/// </summary>
public readonly struct Triple3
{
    public Triple3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public override string ToString() =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture, "({0}, {1}, {2})", X, Y, Z);
}

/// <summary>
/// One object in a resolved template, as the facts the policy needs.
///
/// <para><b>Emitted versus proxy-only.</b> A dedicated server places a location
/// by instantiating the template and letting every enabled <c>ZNetView</c>
/// become a ZDO. A client that has the template builds the whole thing; a
/// client that does not build only what arrives as ZDOs. So a template splits
/// in two, and the halves have completely different questions to answer. An
/// emitted object has to exist on the stock client by name. A proxy-only object
/// has to not matter — because nothing will build it.</para>
///
/// <para><b>Why the ancestor flag.</b> A non-networked child of a networked
/// object is not proxy-only: the client instantiates the whole stock prefab,
/// its own children included. Only an object with no networked ancestor is
/// genuinely lost, which is why this is recorded rather than inferred from
/// <see cref="Networked"/> alone.</para>
/// </summary>
public sealed class ChildFact
{
    public ChildFact(
        string path,
        string prefabName,
        bool networked,
        bool underNetworkedAncestor,
        bool enabledInHierarchy,
        bool persistent = true,
        bool syncInitialScale = false,
        Scale3 scale = default,
        bool hasRenderer = false,
        bool hasCollider = false,
        IReadOnlyList<string>? components = null,
        IReadOnlyList<string>? foreignComponents = null,
        IReadOnlyList<string>? referencedPrefabs = null,
        Triple3 relativePosition = default,
        Triple3 eulerAngles = default,
        string authoredSignature = "",
        string? stockSignature = null,
        bool isRoot = false)
    {
        Path = path ?? "";
        PrefabName = prefabName ?? "";
        Networked = networked;
        UnderNetworkedAncestor = underNetworkedAncestor;
        EnabledInHierarchy = enabledInHierarchy;
        Persistent = persistent;
        SyncInitialScale = syncInitialScale;
        Scale = scale.X == 0f && scale.Y == 0f && scale.Z == 0f ? Scale3.One : scale;
        HasRenderer = hasRenderer;
        HasCollider = hasCollider;
        Components = components ?? System.Array.Empty<string>();
        ForeignComponents = foreignComponents ?? System.Array.Empty<string>();
        ReferencedPrefabs = referencedPrefabs ?? System.Array.Empty<string>();
        RelativePosition = relativePosition;
        EulerAngles = eulerAngles;
        AuthoredSignature = authoredSignature ?? "";
        StockSignature = stockSignature;
        IsRoot = isRoot;
    }

    /// <summary>Where in the template this object is, from the root down. The reason code alone is not actionable; this is.</summary>
    public string Path { get; }

    /// <summary>
    /// The object's name after Jötunn's mock resolution — which is the name the
    /// ZDO's prefab hash is taken from, and therefore the name the stock client
    /// looks up.
    /// </summary>
    public string PrefabName { get; }

    /// <summary>Carries an enabled <c>ZNetView</c>: the client receives this as a ZDO.</summary>
    public bool Networked { get; }

    /// <summary>Has a networked ancestor, so the stock client builds it as part of that prefab.</summary>
    public bool UnderNetworkedAncestor { get; }

    /// <summary>
    /// Active itself and under active parents. Vanilla reads a location's
    /// children with <c>Utils.GetEnabledComponentsInChildren</c>, so a disabled
    /// object is not spawned on any client and is not this mode's problem.
    /// </summary>
    public bool EnabledInHierarchy { get; }

    /// <summary><c>ZNetView.m_persistent</c>: a non-persistent ZDO does not survive a save.</summary>
    public bool Persistent { get; }

    /// <summary><c>ZNetView.m_syncInitialScale</c>: whether the authored scale reaches the client at all.</summary>
    public bool SyncInitialScale { get; }

    /// <summary>The authored local scale, which Jötunn copies from the mock onto the real prefab.</summary>
    public Scale3 Scale { get; }

    public bool HasRenderer { get; }
    public bool HasCollider { get; }

    /// <summary>Every component type on this object, for the detail report.</summary>
    public IReadOnlyList<string> Components { get; }

    /// <summary>
    /// Components whose type comes from an assembly that is not the game's.
    ///
    /// This is the behaviour check, and it is by assembly rather than by a list
    /// of known-bad names because the question is not "is this component one we
    /// recognise" but "could a client without the mod reconstruct what it does".
    /// Nothing outside the game's own assemblies exists on that client.
    /// </summary>
    public IReadOnlyList<string> ForeignComponents { get; }

    /// <summary>
    /// Where this object sits, in the template's own space.
    ///
    /// Not read by any rule, and in the fingerprint all the same: the
    /// fingerprint binds an approval to the template that was ACCEPTED in game,
    /// and a wall moved twenty metres is not that template however well it
    /// still passes every rule.
    /// </summary>
    public Triple3 RelativePosition { get; }

    /// <summary>How it is turned. Same reasoning as <see cref="RelativePosition"/>.</summary>
    public Triple3 EulerAngles { get; }

    /// <summary>
    /// This object and everything under it, as one comparable string.
    ///
    /// <para>For a networked object this is the thing the client is supposed to
    /// build, and it is compared against <see cref="StockSignature"/>. Network
    /// ancestry alone proves nothing: a client instantiates the STOCK prefab,
    /// not whatever the template's author hung underneath an object with a
    /// familiar name.</para>
    /// </summary>
    public string AuthoredSignature { get; }

    /// <summary>
    /// The same signature taken from the stock prefab of this name, or null when
    /// no baseline could be read.
    ///
    /// Null is not permission. A descendant is exempt only when a baseline shows
    /// it is inherited unchanged, so an absent baseline leaves the template
    /// unresolved rather than approved.
    /// </summary>
    public string? StockSignature { get; }

    /// <summary>
    /// The template root itself.
    ///
    /// It gets a row because it was silently skipped before: a walk that starts
    /// at the children never reads the root's own components, and a custom
    /// behaviour put there was invisible to every rule.
    /// </summary>
    public bool IsRoot { get; }

    /// <summary>
    /// Prefab names this object can put into the world later — a spawner's
    /// creature, a container's default items, a pickable's drop.
    ///
    /// Collected whether or not the branch is taken, because a random branch
    /// that emits an unknown prefab is a template that breaks on some seeds and
    /// not others, and that is worse than one that always breaks.
    /// </summary>
    public IReadOnlyList<string> ReferencedPrefabs { get; }
}

/// <summary>
/// One resolved <c>TerrainModifier</c>, as the facts the policy needs.
///
/// The conversion itself reads the modifier through
/// <see cref="LocationTerrainReader"/> and this does not duplicate that — these
/// are the fields that decide whether the template can be converted at all,
/// which is a different question from what the conversion produces at one site.
/// </summary>
public sealed class TerrainFact
{
    public TerrainFact(
        string path,
        bool enabled,
        bool useTerrainCompiler,
        float localOffsetDistance,
        float reach,
        bool level = false,
        bool smooth = false,
        bool paint = false,
        string paintType = "",
        int sortOrder = 0,
        float levelRadius = 0f,
        float levelOffset = 0f,
        bool square = false,
        float smoothRadius = 0f,
        float smoothPower = 0f,
        float paintRadius = 0f,
        float paintStrength = 0f,
        bool paintHeightCheck = false,
        bool playerModification = false,
        Triple3 relativePosition = default)
    {
        Path = path ?? "";
        Enabled = enabled;
        UseTerrainCompiler = useTerrainCompiler;
        LocalOffsetDistance = localOffsetDistance;
        Reach = reach;
        Level = level;
        Smooth = smooth;
        Paint = paint;
        PaintType = paintType ?? "";
        SortOrder = sortOrder;
        LevelRadius = levelRadius;
        LevelOffset = levelOffset;
        Square = square;
        SmoothRadius = smoothRadius;
        SmoothPower = smoothPower;
        PaintRadius = paintRadius;
        PaintStrength = paintStrength;
        PaintHeightCheck = paintHeightCheck;
        PlayerModification = playerModification;
        RelativePosition = relativePosition;
    }

    public string Path { get; }

    /// <summary><c>ApplyModifiers</c> tests this, so a disabled modifier shapes nothing on any client.</summary>
    public bool Enabled { get; }

    /// <summary>Already writes through a compiler, so the client receives it and the conversion must leave it alone.</summary>
    public bool UseTerrainCompiler { get; }

    /// <summary>How far from the location's origin this modifier sits, horizontally.</summary>
    public float LocalOffsetDistance { get; }

    /// <summary>
    /// The farthest this modifier reaches from the location's origin:
    /// <see cref="LocalOffsetDistance"/> plus its own largest radius. What
    /// decides whether a placement can be served, once a placement exists.
    /// </summary>
    public float Reach { get; }

    // Every field LocationTerrainReader.Operation copies onto an operation.
    // Anything less would be a fingerprint that calls two different pieces of
    // ground the same template: -2 m and -12 m of level offset are the same
    // flags, the same paint type and the same reach.

    public bool Level { get; }
    public bool Smooth { get; }
    public bool Paint { get; }
    public string PaintType { get; }

    /// <summary>
    /// Vanilla's application order, ascending, ties broken by the order the
    /// modifiers appear in the template.
    ///
    /// It decides the ground, not just the bookkeeping: each modifier reads what
    /// the one before it left, so two modifiers whose order is swapped draw
    /// different ground from the same numbers.
    /// </summary>
    public int SortOrder { get; }

    public float LevelRadius { get; }
    public float LevelOffset { get; }
    public bool Square { get; }
    public float SmoothRadius { get; }
    public float SmoothPower { get; }
    public float PaintRadius { get; }
    public float PaintStrength { get; }
    public bool PaintHeightCheck { get; }

    /// <summary>Vanilla's first sort key, before <see cref="SortOrder"/>.</summary>
    public bool PlayerModification { get; }

    /// <summary>Where the modifier sits in the template's own space.</summary>
    public Triple3 RelativePosition { get; }
}

/// <summary>
/// Everything read off one template after Jötunn has resolved it.
///
/// <para><b>After resolution, not before.</b> A bundle read is not this.
/// Resolution swaps every mock for the real prefab, which brings that prefab's
/// own children, components and settings with it —
/// <c>MWL_FulingRock1</c> has one terrain modifier in the bundle and two once
/// resolved. Facts gathered before that describe a template that never
/// exists.</para>
///
/// <para><b>Incomplete is not clean.</b> <see cref="Complete"/> is false when
/// the walk could not finish, and a policy that returned "no findings" for that
/// would be reporting the absence of evidence as evidence of absence. The
/// evaluator refuses instead.</para>
/// </summary>
public sealed class TemplateFacts
{
    public TemplateFacts(
        string name,
        string pack,
        bool sourceDeclared = true,
        bool assetLoaded = true,
        bool rootActive = true,
        IReadOnlyList<ChildFact>? children = null,
        IReadOnlyList<TerrainFact>? terrain = null,
        IReadOnlyList<string>? extractionErrors = null,
        bool complete = true,
        string interiorPrefabName = "",
        string dungeonTheme = "")
    {
        Name = name ?? "";
        Pack = pack ?? "";
        SourceDeclared = sourceDeclared;
        AssetLoaded = assetLoaded;
        RootActive = rootActive;
        Children = children ?? System.Array.Empty<ChildFact>();
        Terrain = terrain ?? System.Array.Empty<TerrainFact>();
        ExtractionErrors = extractionErrors ?? System.Array.Empty<string>();
        Complete = complete;
        InteriorPrefabName = interiorPrefabName ?? "";
        DungeonTheme = dungeonTheme ?? "";
    }

    /// <summary>The exact name, with its exact capitalisation. Never folded: a name that differs only in case is a different asset lookup.</summary>
    public string Name { get; }

    /// <summary>The pack the definition came from, which is what scope exclusion is decided by.</summary>
    public string Pack { get; }

    /// <summary>A location definition exists for this name. False for an asset nobody placed.</summary>
    public bool SourceDeclared { get; }

    /// <summary>The soft-referenced template loaded. False is unresolved, not compatible and not blocked.</summary>
    public bool AssetLoaded { get; }

    /// <summary>The root object is active. An inactive root spawns nothing at all, whatever its children say.</summary>
    public bool RootActive { get; }

    public IReadOnlyList<ChildFact> Children { get; }
    public IReadOnlyList<TerrainFact> Terrain { get; }

    /// <summary>What went wrong during the walk. Any entry makes the result unresolved.</summary>
    public IReadOnlyList<string> ExtractionErrors { get; }

    /// <summary>The walk finished. See the type remarks.</summary>
    public bool Complete { get; }

    /// <summary>
    /// The interior the definition attaches, if any.
    ///
    /// A location with an interior does not place its rooms from the template:
    /// vanilla's dungeon generator builds them at spawn time out of a room set.
    /// The rooms are prefabs of their own and this mode does not ship them, so
    /// the emission the walk can see is not the whole site.
    /// </summary>
    public string InteriorPrefabName { get; }

    /// <summary>The dungeon theme the definition attaches, if any. Same reasoning as <see cref="InteriorPrefabName"/>.</summary>
    public string DungeonTheme { get; }

    /// <summary>Whether this location builds an interior at spawn time rather than placing it from the template.</summary>
    public bool GeneratesInterior =>
        InteriorPrefabName.Length > 0 || DungeonTheme.Length > 0;

    /// <summary>A template with no definition: an asset that exists and is placed nowhere.</summary>
    public static TemplateFacts MissingDefinition(string name) =>
        new TemplateFacts(name, pack: "", sourceDeclared: false);

    /// <summary>A template whose facts could not be read, with the reason.</summary>
    public static TemplateFacts Unreadable(string name, string pack, string error) =>
        new TemplateFacts(name, pack, assetLoaded: false, complete: false,
            extractionErrors: new[] { error });
}
