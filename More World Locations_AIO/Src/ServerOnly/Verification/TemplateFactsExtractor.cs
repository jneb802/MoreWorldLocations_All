using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// Reads a resolved template and says what is in it.
///
/// <para><b>Resolved, not the bundle.</b> Everything here runs over the
/// GameObject Jötunn has already fixed the references on. A bundle read
/// describes a template that never exists: resolution swaps every mock for the
/// real prefab and brings that prefab's children, components and settings with
/// it — <c>MWL_FulingRock1</c> carries one terrain modifier in the bundle and
/// two once resolved.</para>
///
/// <para><b>It decides nothing.</b> Every judgement is in
/// <see cref="TemplatePolicy"/>, which is pure and has the tests. This half
/// exists because a GameObject hierarchy cannot be reasoned about without
/// Unity, so it is kept as thin as a walk can be: find things, write down what
/// they are, hand them over.</para>
///
/// <para><b>What it does when it cannot read something.</b> Says so. An
/// extraction note makes the whole template unresolved rather than compatible,
/// because a walk that found nothing and a walk that could not look are the same
/// output and must not be the same verdict. A game update that renames a field
/// this reads should stop the catalogue, not quietly shrink it.</para>
/// </summary>
public static class TemplateFactsExtractor
{
    /// <summary>
    /// The facts of one resolved template.
    /// </summary>
    /// <param name="name">The exact location name, as declared. Never folded.</param>
    /// <param name="pack">The pack the definition came from.</param>
    /// <param name="resolved">The template after mock resolution, or null when it would not load.</param>
    /// <param name="stockPrefabOf">
    /// The stock prefab of a given name, or null when none is available.
    ///
    /// This is what makes an object the client receives checkable at all. The
    /// client instantiates the stock prefab — its components, its children, its
    /// fields — so the only way to know whether the author changed something is
    /// to hold the two side by side. Injected rather than reached for directly,
    /// because where a baseline comes from is the caller's business and the
    /// walk's correctness should not depend on a running ZNetScene.
    /// </param>
    public static TemplateFacts Extract(
        string name, string pack, GameObject? resolved, string interiorPrefabName = "", string dungeonTheme = "",
        Func<string, GameObject?>? stockPrefabOf = null,
        string baselineProvenance = "")
    {
        if (resolved == null)
            return TemplateFacts.Unreadable(name, pack, "the soft-referenced template resolved to null");

        var children = new List<ChildFact>();
        var terrain = new List<TerrainFact>();
        var errors = new List<string>();
        var uncompared = new SortedSet<string>(StringComparer.Ordinal);

        try
        {
            // The root itself, first. A walk that starts at the children never
            // reads it, and a behaviour attached there drives the whole site.
            children.Add(FactOf(resolved, resolved.transform, resolved.transform, name,
                enabled: resolved.activeSelf, underNetworked: false, isRoot: true, stockPrefabOf, errors, uncompared));
            TerrainModifier rootModifier = resolved.GetComponent<TerrainModifier>();
            if (rootModifier != null)
                terrain.Add(TerrainFactOf(rootModifier, resolved.transform, resolved.transform, name, resolved.activeSelf));

            Walk(resolved.transform, resolved.transform, name, resolved.activeSelf,
                resolved.GetComponent<ZNetView>() != null, children, terrain, errors, stockPrefabOf, uncompared);
        }
        catch (Exception ex)
        {
            errors.Add($"the walk threw at some point inside the hierarchy: {ex.GetType().Name}: {ex.Message}");
            return new TemplateFacts(name, pack, children: children, terrain: terrain,
                extractionErrors: errors, complete: false,
                interiorPrefabName: interiorPrefabName, dungeonTheme: dungeonTheme,
                uncomparedComponents: new List<string>(uncompared),
                baselineProvenance: stockPrefabOf == null ? "" : baselineProvenance);
        }

        return new TemplateFacts(name, pack,
            assetLoaded: true,
            rootActive: resolved.activeSelf,
            children: children,
            terrain: terrain,
            extractionErrors: errors,
            complete: true,
            interiorPrefabName: interiorPrefabName,
            dungeonTheme: dungeonTheme,
            uncomparedComponents: new List<string>(uncompared),
            baselineProvenance: stockPrefabOf == null ? "" : baselineProvenance);
    }

    /// <summary>
    /// One object and everything under it.
    ///
    /// <paramref name="enabledSoFar"/> and <paramref name="underNetworked"/>
    /// are carried down rather than recomputed, because both are questions about
    /// the path from the root and an object cannot answer either by looking at
    /// itself.
    /// </summary>
    private static void Walk(
        Transform node, Transform root, string rootName,
        bool enabledSoFar, bool underNetworked,
        List<ChildFact> children, List<TerrainFact> terrain, List<string> errors,
        Func<string, GameObject?>? stockPrefabOf, ISet<string> uncompared)
    {
        for (int i = 0; i < node.childCount; i++)
        {
            Transform child = node.GetChild(i);
            GameObject go = child.gameObject;
            bool enabled = enabledSoFar && go.activeSelf;

            ZNetView view = go.GetComponent<ZNetView>();
            // A disabled ZNetView is not a network boundary: vanilla reads a
            // location's children with GetEnabledComponentsInChildren, so the
            // game would not spawn a ZDO for it either.
            bool networked = view != null && view.enabled;

            children.Add(FactOf(go, child, root, rootName, enabled, underNetworked, false, stockPrefabOf, errors, uncompared));

            TerrainModifier modifier = go.GetComponent<TerrainModifier>();
            if (modifier != null)
                terrain.Add(TerrainFactOf(modifier, child, root, rootName, enabled));

            Walk(child, root, rootName, enabled, underNetworked || networked, children, terrain, errors, stockPrefabOf, uncompared);
        }
    }

    /// <summary>One object as facts, whether it is the root or a child.</summary>
    private static ChildFact FactOf(
        GameObject go, Transform node, Transform root, string rootName,
        bool enabled, bool underNetworked, bool isRoot,
        Func<string, GameObject?>? stockPrefabOf, List<string> errors, ISet<string> uncompared)
    {
        ZNetView view = go.GetComponent<ZNetView>();
        bool networked = view != null && view.enabled;

        // Only an object the client actually receives has a stock counterpart to
        // be compared against. For anything else the comparison is meaningless:
        // nothing is instantiated from a name.
        string authored = networked ? Signature(go, node, errors, uncompared) : "";

        // What a client without the mod would build for this name: whether its
        // ZNetView reads a scale out of the ZDO at all, and the scale it starts
        // at. Both are facts about the stock prefab, not about the template, and
        // the scale rule cannot be answered without them.
        bool? stockSyncsScale = null;
        Scale3? stockScale = null;
        if (networked && stockPrefabOf != null)
        {
            GameObject? stockPrefab = null;
            try
            {
                stockPrefab = stockPrefabOf(go.name);
            }
            catch (Exception ex)
            {
                errors.Add($"the stock prefab '{go.name}' could not be read ({ex.GetType().Name}: {ex.Message})");
            }
            if (stockPrefab != null)
            {
                ZNetView stockView = stockPrefab.GetComponent<ZNetView>();
                stockSyncsScale = stockView != null && stockView.m_syncInitialScale;
                Vector3 s = stockPrefab.transform.localScale;
                stockScale = new Scale3(s.x, s.y, s.z);
            }
        }

        string? stock = null;
        if (networked && stockPrefabOf != null && authored.Length > 0)
        {
            stock = StockSignature(go.name, stockPrefabOf, errors, uncompared);
            // The two match for almost every object in the catalogue, and
            // keeping two identical copies of a big subtree's text for each of
            // seventy thousand children is how an audit runs a server out of
            // memory. Equal collapses to one short digest, which compares the
            // same; unequal is kept in full, because that is the case where
            // somebody has to be told WHICH line differs.
            if (stock != null && string.Equals(stock, authored, StringComparison.Ordinal))
            {
                authored = TemplateFingerprint.Digest(authored);
                stock = authored;
            }
        }

        Vector3 local = root.InverseTransformPoint(node.position);
        return new ChildFact(
            path: PathOf(node, root, rootName),
            prefabName: go.name,
            networked: networked,
            underNetworkedAncestor: underNetworked,
            enabledInHierarchy: enabled,
            persistent: view == null || view.m_persistent,
            syncInitialScale: view != null && view.m_syncInitialScale,
            scale: new Scale3(node.localScale.x, node.localScale.y, node.localScale.z),
            hasRenderer: go.GetComponent<Renderer>() != null,
            hasCollider: go.GetComponent<Collider>() != null,
            components: ComponentNames(go, errors),
            foreignComponents: ForeignComponentNames(go, errors),
            referencedPrefabs: ReferencedPrefabs(go, errors),
            relativePosition: new Triple3(local.x, local.y, local.z),
            eulerAngles: new Triple3(node.eulerAngles.x, node.eulerAngles.y, node.eulerAngles.z),
            rotation: new Quat4(node.rotation.x, node.rotation.y, node.rotation.z, node.rotation.w),
            authoredSignature: authored,
            stockSignature: stock,
            isRoot: isRoot,
            stockSyncInitialScale: stockSyncsScale,
            stockScale: stockScale);
    }

    /// <summary>
    /// A stock prefab's signature, computed once per name.
    ///
    /// <para>Measured on the station, not guessed: without this the audit walked
    /// the stock prefab's whole subtree again for every child that used it, so a
    /// template with three thousand walls did three thousand identical walks.
    /// The server climbed past five gigabytes in the first few templates and had
    /// to be stopped. A stock prefab does not change while the process runs, so
    /// the second walk can only ever produce what the first one did.</para>
    ///
    /// <para>Cleared when a world is, beside the other per-world state.</para>
    /// </summary>
    /// <summary>
    /// How many bytes of stock signatures the audit may hold at once.
    ///
    /// A signature is text, and a large prefab's runs to hundreds of kilobytes;
    /// an unbounded dictionary of them across a 194-template sweep is the second
    /// thing that grew without limit. The budget is the audit's, not the
    /// world's: it is cleared when the sweep ends, whether it ended by
    /// finishing, by being cancelled, or by throwing.
    /// </summary>
    public const long StockSignatureBudgetBytes = 16L * 1024 * 1024;

    private static readonly ByteBudgetCache<string, string> s_stockSignatures =
        new ByteBudgetCache<string, string>(
            StockSignatureBudgetBytes, signature => signature.Length * sizeof(char), StringComparer.Ordinal);

    /// <summary>Names whose stock prefab does not exist, kept as a fact rather than re-looked-up.</summary>
    private static readonly HashSet<string> s_withoutStock = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>What the signature cache is holding, for a run's accounting.</summary>
    public static long StockSignatureBytes => s_stockSignatures.Bytes;

    public static int StockSignatureEntries => s_stockSignatures.Count;

    /// <summary>The sweep is over, however it ended. Audit-owned memory goes with it.</summary>
    internal static void ForgetStockSignatures()
    {
        s_stockSignatures.Clear();
        s_withoutStock.Clear();
    }

    private static string? StockSignature(
        string name, Func<string, GameObject?> stockPrefabOf, List<string> errors, ISet<string> uncompared)
    {
        if (s_withoutStock.Contains(name))
            return null;

        string cached = s_stockSignatures.Peek(name);
        if (cached != null)
            return cached;

        GameObject? reference = Reference(stockPrefabOf, name, errors);
        if (reference == null)
        {
            s_withoutStock.Add(name);
            return null;
        }

        string signature = Signature(reference, reference.transform, errors, uncompared);
        // A signature too large for the whole budget is USED and not kept:
        // admitting it would evict everything else and still be over. The
        // comparison is identical; only the next caller pays for it again.
        s_stockSignatures.Put(name, signature);
        return signature;
    }

    /// <summary>
    /// Everything a verdict can read off one live prefab, as text: what the
    /// verdict cache records at audit time and compares at the next start.
    ///
    /// <para>It starts from <see cref="Signature"/>, the comparison's own
    /// routine, so a prefab the comparison would call different is different
    /// here. That routine leaves out what the comparison judges separately, and
    /// a stored verdict depends on those too, so they are added: the root's
    /// scale and activeness (the scale rule's baseline), every ZNetView's
    /// settings (whether an object is networked, persistent, and reads a sent
    /// scale — including on a mock's copy, where they decide the template's own
    /// facts), every behaviour's enabled flag (a terrain modifier's among them),
    /// and each component's assembly (the foreign-component rule reads it; the
    /// signature records type names only). A prefab whose subtree is too deep
    /// to walk says so in the text, which is also what the audit reports.</para>
    /// </summary>
    public static string LiveSignature(GameObject prefab)
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));

        List<string> errors = new List<string>();
        SortedSet<string> uncompared = new SortedSet<string>(StringComparer.Ordinal);
        System.Text.StringBuilder text = new System.Text.StringBuilder();
        text.Append("signature\n").Append(Signature(prefab, prefab.transform, errors, uncompared));
        text.Append("root|").Append(prefab.activeSelf ? "on" : "off")
            .Append('|').Append(Triple(prefab.transform.localScale)).Append('\n');
        AppendIdentity(prefab, prefab.transform, prefab.name, text, depth: 0);
        foreach (string error in errors)
            text.Append("error|").Append(error).Append('\n');
        foreach (string type in uncompared)
            text.Append("uncompared|").Append(type).Append('\n');
        return text.ToString();
    }

    private static void AppendIdentity(GameObject go, Transform node, string path, System.Text.StringBuilder text, int depth)
    {
        if (depth > MaxSignatureDepth)
        {
            text.Append("deeper|").Append(path).Append('\n');
            return;
        }

        text.Append("node|").Append(path);
        foreach (Component component in go.GetComponents<Component>())
        {
            if (component == null)
            {
                text.Append("|(missing script)");
                continue;
            }
            Type type = component.GetType();
            text.Append('|').Append(type.FullName).Append('@').Append(type.Assembly.GetName().Name);
            // ZNetView and TerrainModifier by name first: they are what the
            // facts read, and in the test doubles neither derives from
            // Behaviour.
            if (component is ZNetView view)
                text.Append("(enabled=").Append(view.enabled ? '1' : '0').Append(',').Append(FieldValues(view, type)).Append(')');
            else if (component is TerrainModifier modifier)
                text.Append("(enabled=").Append(modifier.enabled ? '1' : '0').Append(')');
            else if (component is Behaviour behaviour)
                text.Append("(enabled=").Append(behaviour.enabled ? '1' : '0').Append(')');
        }
        text.Append('\n');

        for (int i = 0; i < node.childCount; i++)
        {
            Transform child = node.GetChild(i);
            AppendIdentity(child.gameObject, child, path + "/" + child.gameObject.name, text, depth + 1);
        }
    }

    private static GameObject? Reference(Func<string, GameObject?> stockPrefabOf, string name, List<string> errors)
    {
        try
        {
            return stockPrefabOf(name);
        }
        catch (Exception ex)
        {
            errors.Add($"the stock prefab for '{name}' could not be looked up: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Everything about one object that decides what a client ends up holding,
    /// as one comparable string: its own components and their settings, and
    /// every descendant with the same, plus where each descendant sits and
    /// whether it is switched on.
    ///
    /// <para><b>What is deliberately left out, and why only that.</b> The
    /// emitted object's OWN transform. Its position and rotation are the
    /// placement, sent in the ZDO; its scale is sent too when the prefab syncs
    /// the initial scale, and judged by the scale rule when it does not. So
    /// comparing the root's transform against a prefab's default would reject a
    /// building the game reproduces perfectly — which it did. A DESCENDANT's
    /// transform is sent nowhere at all: the client builds it from the stock
    /// prefab, so a collider moved twenty metres, grown from one to twenty, or
    /// switched off exists on the server and nowhere else, and those are
    /// compared.</para>
    ///
    /// <para>The network view is left out for the same reason as the root
    /// transform: it is what makes the object reach the client, its settings are
    /// judged on their own, and a template's copy and the stock prefab's differ
    /// in ways that say nothing about what is built.</para>
    ///
    /// <para>Empty means there is nothing to prove: an object with no extra
    /// components and no children is fully described by its name, and the
    /// client's stock prefab of that name is the whole answer.</para>
    /// </summary>
    private static string Signature(GameObject go, Transform node, List<string> errors, ISet<string> uncompared)
    {
        var text = new System.Text.StringBuilder();
        AppendSignature(go, node, "", text, errors, uncompared, depth: 0);
        string signature = text.ToString();
        return signature == "\n" ? "" : signature;
    }

    /// <summary>
    /// How deep a subtree is walked before the comparison gives up and says so.
    ///
    /// Twelve was too shallow, measured: MWL_AshlandsFort3 came back unresolved
    /// because a creature's finger bones sit deeper than that, and a rig is
    /// ordinary content rather than anything unusual. Twenty-four clears every
    /// hierarchy in the catalogue; anything past it is still reported rather
    /// than quietly truncated, because a subtree compared in part is not a
    /// subtree compared.
    /// </summary>
    private const int MaxSignatureDepth = 24;

    private static void AppendSignature(
        GameObject go, Transform node, string path, System.Text.StringBuilder text,
        List<string> errors, ISet<string> uncompared, int depth)
    {
        if (depth > MaxSignatureDepth)
        {
            errors.Add($"'{go.name}' is nested deeper than {MaxSignatureDepth} levels; its subtree was not compared in full");
            return;
        }

        var parts = new List<string>();
        foreach (Component component in go.GetComponents<Component>())
        {
            if (component == null)
            {
                parts.Add("(missing script)");
                continue;
            }
            Type type = component.GetType();
            // The transform is geometry, recorded below; the network view is the
            // boundary, judged separately.
            if (type.Name == "Transform" || type.Name == "ZNetView")
                continue;
            parts.Add(type.Name + ComponentValues(component, type, uncompared));
        }
        parts.Sort(StringComparer.Ordinal);

        foreach (string referenced in ReferencedPrefabs(go, errors))
            parts.Add("->" + referenced);

        if (depth > 0)
        {
            // A descendant's whole transform and its activeness. None of it is
            // transmitted, so a difference here is a difference the client never
            // hears about.
            text.Append(path).Append('|')
                .Append(go.activeSelf ? "on" : "off").Append('|')
                .Append(Triple(node.localPosition)).Append('|')
                .Append(Triple(node.localRotation.eulerAngles)).Append('|')
                .Append(Triple(node.localScale)).Append('|')
                .Append(string.Join(",", parts.ToArray()))
                .Append('\n');
        }
        else if (parts.Count > 0)
        {
            // The emitted object itself: its components, not its transform.
            text.Append(path).Append('|')
                .Append(go.activeSelf ? "on" : "off").Append('|')
                .Append(string.Join(",", parts.ToArray()))
                .Append('\n');
        }

        for (int i = 0; i < node.childCount; i++)
        {
            Transform child = node.GetChild(i);
            AppendSignature(child.gameObject, child, path + "/" + child.gameObject.name, text, errors, uncompared, depth + 1);
        }
    }

    /// <summary>
    /// The settings of one component that decide what a player meets.
    ///
    /// <para>Two sources, because Valheim prefabs carry two kinds of component.
    /// The game's own are ordinary behaviours whose settings are public fields,
    /// and those are read by reflection — a changed drop table, a changed
    /// health, a changed spawn. Unity's built-in components keep their settings
    /// in native properties that no field walk can see, so the ones deciding
    /// whether a player can walk through something or see it are read by
    /// name.</para>
    ///
    /// <para>What is left is engine and presentation state — animators, audio,
    /// particles, lights, levels of detail. Those are named in
    /// <paramref name="uncompared"/> rather than compared, and the evaluation
    /// says so, because "we compared everything we know how to compare" is a
    /// different claim from "these are equivalent" and only the first is
    /// true.</para>
    /// </summary>
    private static string ComponentValues(Component component, Type type, ISet<string> uncompared)
    {
        string capability = CapabilityValues(component);
        if (capability != null)
            return "(" + capability + ")";

        // The test is what was actually read, not which assembly the type came
        // from. A Valheim behaviour keeps its settings in public fields and they
        // are read here; Unity's own components keep theirs in native properties
        // that no field walk can see, so reading their fields reads nothing at
        // all — and "nothing" is the signal, wherever the type lives.
        string fields = FieldValues(component, type);
        if (fields.Length > 0)
            return "(" + fields + ")";

        uncompared.Add(type.Name);
        return "(?)";
    }

    /// <summary>
    /// Unity's own components, by capability: what a player can walk into, and
    /// what they can see. Null for a type this does not know how to read.
    /// </summary>
    private static string CapabilityValues(Component component)
    {
        switch (component)
        {
            case BoxCollider box:
                return $"trigger={box.isTrigger},enabled={box.enabled},size={Triple(box.size)},centre={Triple(box.center)}";
            case SphereCollider sphere:
                return $"trigger={sphere.isTrigger},enabled={sphere.enabled},r={Fixed(sphere.radius)},centre={Triple(sphere.center)}";
            case CapsuleCollider capsule:
                return $"trigger={capsule.isTrigger},enabled={capsule.enabled},r={Fixed(capsule.radius)}," +
                       $"h={Fixed(capsule.height)},axis={capsule.direction},centre={Triple(capsule.center)}";
            case MeshCollider mesh:
                return $"trigger={mesh.isTrigger},enabled={mesh.enabled},convex={mesh.convex},mesh={Named(mesh.sharedMesh)}";
            case Collider other:
                return $"trigger={other.isTrigger},enabled={other.enabled}";
            case MeshFilter filter:
                return $"mesh={Named(filter.sharedMesh)}";
            case Renderer renderer:
                return $"enabled={renderer.enabled},materials={Materials(renderer)}";
            default:
                return null;
        }
    }

    private static string Materials(Renderer renderer)
    {
        var names = new List<string>();
        foreach (Material material in renderer.sharedMaterials)
            names.Add(Named(material));
        return string.Join("+", names.ToArray());
    }

    private static string Named(UnityEngine.Object value) => value == null ? "-" : value.name;

    /// <summary>
    /// A game component's public fields, as text.
    ///
    /// Values, enums, strings and references by name, one level deep. Not a dump
    /// of engine state: a field holding another object is recorded by that
    /// object's name, which is what decides what the server puts into the world,
    /// and nothing is followed further.
    /// </summary>
    private static string FieldValues(Component component, Type type)
    {
        var parts = new List<string>();
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            string value = FieldValue(field.GetValue(component));
            if (value != null)
                parts.Add(field.Name + "=" + value);
        }
        parts.Sort(StringComparer.Ordinal);
        return string.Join(",", parts.ToArray());
    }

    private static string FieldValue(object value)
    {
        switch (value)
        {
            case null:
                return "-";
            case string text:
                return text;
            case bool flag:
                return flag ? "1" : "0";
            case float number:
                return Fixed(number);
            case double number:
                return Fixed((float)number);
            case Enum choice:
                return choice.ToString();
            case UnityEngine.Object reference:
                return reference.name;
            case Vector3 point:
                return Triple(point);
            case System.Collections.IEnumerable list:
                var parts = new List<string>();
                int seen = 0;
                foreach (object item in list)
                {
                    // Bounded: a long list is a list, and its first entries are
                    // what a change to it moves.
                    if (seen++ >= 32)
                    {
                        parts.Add("…");
                        break;
                    }
                    parts.Add(FieldValue(item) ?? "?");
                }
                return "[" + string.Join(";", parts.ToArray()) + "]";
            default:
                return value.GetType().IsPrimitive ? value.ToString() : null;
        }
    }

    private static string Triple(Vector3 value) =>
        Fixed(value.x) + "," + Fixed(value.y) + "," + Fixed(value.z);

    private static string Fixed(float value) =>
        (value == 0f ? 0f : value).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// A modifier's reach: how far its effect extends from the location's own
    /// origin, which is what decides whether a placement can be served.
    ///
    /// The distance is measured in the template's local space, so it is a
    /// property of the template and not of any one site — which is the whole
    /// point of keeping this check apart from the per-site preflight.
    /// </summary>
    private static TerrainFact TerrainFactOf(
        TerrainModifier modifier, Transform node, Transform root, string rootName, bool enabled)
    {
        Vector3 local = root.InverseTransformPoint(node.position);
        float offset = new Vector2(local.x, local.z).magnitude;
        float radius = Mathf.Max(
            modifier.m_level ? modifier.m_levelRadius : 0f,
            Mathf.Max(
                modifier.m_smooth ? modifier.m_smoothRadius : 0f,
                modifier.m_paintCleared ? modifier.m_paintRadius : 0f));

        return new TerrainFact(
            path: PathOf(node, root, rootName),
            enabled: enabled && modifier.enabled,
            useTerrainCompiler: modifier.m_useTerrainCompiler,
            localOffsetDistance: offset,
            reach: offset + radius,
            level: modifier.m_level,
            smooth: modifier.m_smooth,
            paint: modifier.m_paintCleared,
            // Recorded whatever the flag says. m_paintType is what the ground
            // ends up painted with if the flag is ever turned on, and a
            // fingerprint that dropped it would call two different templates
            // the same one.
            paintType: modifier.m_paintType.ToString(),
            // Every value LocationTerrainReader.Operation copies. A digest built
            // from the flags alone cannot tell -2 m of level offset from -12 m.
            sortOrder: modifier.m_sortOrder,
            levelRadius: modifier.m_levelRadius,
            levelOffset: modifier.m_levelOffset,
            square: modifier.m_square,
            smoothRadius: modifier.m_smoothRadius,
            smoothPower: modifier.m_smoothPower,
            paintRadius: modifier.m_paintRadius,
            paintStrength: modifier.m_paintStrength,
            paintHeightCheck: modifier.m_paintHeightCheck,
            playerModification: modifier.m_playerModifiction,
            relativePosition: new Triple3(local.x, local.y, local.z));
    }

    /// <summary>The path from the template root, which is what makes a finding actionable.</summary>
    private static string PathOf(Transform node, Transform root, string rootName)
    {
        var parts = new List<string>();
        Transform current = node;
        while (current != null && current != root)
        {
            parts.Add(current.name);
            current = current.parent;
        }
        parts.Add(rootName);
        parts.Reverse();
        return string.Join("/", parts.ToArray());
    }

    private static IReadOnlyList<string> ComponentNames(GameObject go, List<string> errors)
    {
        var names = new List<string>();
        foreach (Component component in go.GetComponents<Component>())
        {
            // A null entry is a component whose script is missing from this
            // build. It is worth recording as such: something was authored here
            // and nothing will run.
            if (component == null)
            {
                names.Add("(missing script)");
                errors.Add($"{go.name} carries a component whose script this build does not have");
                continue;
            }
            names.Add(component.GetType().Name);
        }
        return names;
    }

    /// <summary>
    /// Components whose type is not in an assembly a client without the mod
    /// loads.
    ///
    /// By assembly rather than by a list of known-bad names: the question is not
    /// whether a component is recognised, but whether a client that has never
    /// heard of it can still reconstruct what it does. Nothing outside the
    /// game's own assemblies exists on that client.
    /// </summary>
    private static IReadOnlyList<string> ForeignComponentNames(GameObject go, List<string> errors)
    {
        var names = new List<string>();
        foreach (Component component in go.GetComponents<Component>())
        {
            if (component == null)
                continue;
            Type type = component.GetType();
            string assembly = type.Assembly.GetName().Name;
            if (!ComponentPolicy.Default.IsGameAssembly(assembly))
                names.Add(type.Name);
        }
        return names;
    }

    /// <summary>
    /// Prefabs this object can put into the world later.
    ///
    /// <para>Deliberately a short, named list rather than a reflective sweep of
    /// every reference. What matters is what the SERVER resolves and persists:
    /// a stock client builds the stock prefab and runs the stock prefab's own
    /// behaviour, so a field the server changed does not reach it — except
    /// through saved data, and these are the components that write prefab
    /// identities into saved data.</para>
    ///
    /// <para>Each read is guarded and a failure becomes an extraction note. A
    /// game update that renames one of these fields makes the catalogue
    /// unresolved, which stops it; silently reading nothing would shrink it
    /// while still looking like a working check.</para>
    /// </summary>
    private static IReadOnlyList<string> ReferencedPrefabs(GameObject go, List<string> errors)
    {
        var names = new List<string>();
        try
        {
            Container container = go.GetComponent<Container>();
            if (container != null)
                AddDropTable(container.m_defaultItems, names);

            CreatureSpawner spawner = go.GetComponent<CreatureSpawner>();
            if (spawner != null && spawner.m_creaturePrefab != null)
                names.Add(spawner.m_creaturePrefab.name);

            DropOnDestroyed dropOnDestroyed = go.GetComponent<DropOnDestroyed>();
            if (dropOnDestroyed != null)
                AddDropTable(dropOnDestroyed.m_dropWhenDestroyed, names);

            SpawnArea spawnArea = go.GetComponent<SpawnArea>();
            if (spawnArea != null && spawnArea.m_prefabs != null)
            {
                foreach (SpawnArea.SpawnData spawn in spawnArea.m_prefabs)
                {
                    if (spawn != null && spawn.m_prefab != null)
                        names.Add(spawn.m_prefab.name);
                }
            }

            PickableItem pickable = go.GetComponent<PickableItem>();
            if (pickable != null && pickable.m_randomItemPrefabs != null)
            {
                // RandomItem is a struct, so there is no null entry to skip --
                // only an empty slot, which carries no prefab.
                foreach (PickableItem.RandomItem item in pickable.m_randomItemPrefabs)
                {
                    if (item.m_itemPrefab != null)
                        names.Add(item.m_itemPrefab.name);
                }
            }

            Pickable single = go.GetComponent<Pickable>();
            if (single != null && single.m_itemPrefab != null)
                names.Add(single.m_itemPrefab.name);
        }
        catch (Exception ex)
        {
            errors.Add($"{go.name}: a spawn reference could not be read — {ex.GetType().Name}: {ex.Message}");
        }
        return names;
    }

    private static void AddDropTable(DropTable? table, List<string> names)
    {
        if (table?.m_drops == null)
            return;
        foreach (DropTable.DropData drop in table.m_drops)
        {
            if (drop.m_item != null)
                names.Add(drop.m_item.name);
        }
    }
}
