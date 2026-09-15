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
    public static TemplateFacts Extract(
        string name, string pack, GameObject? resolved, string interiorPrefabName = "", string dungeonTheme = "")
    {
        if (resolved == null)
            return TemplateFacts.Unreadable(name, pack, "the soft-referenced template resolved to null");

        var children = new List<ChildFact>();
        var terrain = new List<TerrainFact>();
        var errors = new List<string>();

        try
        {
            Walk(resolved.transform, resolved.transform, name, resolved.activeSelf, false, children, terrain, errors);
        }
        catch (Exception ex)
        {
            errors.Add($"the walk threw at some point inside the hierarchy: {ex.GetType().Name}: {ex.Message}");
            return new TemplateFacts(name, pack, children: children, terrain: terrain,
                extractionErrors: errors, complete: false,
                interiorPrefabName: interiorPrefabName, dungeonTheme: dungeonTheme);
        }

        return new TemplateFacts(name, pack,
            assetLoaded: true,
            rootActive: resolved.activeSelf,
            children: children,
            terrain: terrain,
            extractionErrors: errors,
            complete: true,
            interiorPrefabName: interiorPrefabName,
            dungeonTheme: dungeonTheme);
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
        List<ChildFact> children, List<TerrainFact> terrain, List<string> errors)
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

            children.Add(new ChildFact(
                path: PathOf(child, root, rootName),
                prefabName: go.name,
                networked: networked,
                underNetworkedAncestor: underNetworked,
                enabledInHierarchy: enabled,
                persistent: view == null || view.m_persistent,
                syncInitialScale: view != null && view.m_syncInitialScale,
                scale: new Scale3(child.localScale.x, child.localScale.y, child.localScale.z),
                hasRenderer: go.GetComponent<Renderer>() != null,
                hasCollider: go.GetComponent<Collider>() != null,
                components: ComponentNames(go, errors),
                foreignComponents: ForeignComponentNames(go, errors),
                referencedPrefabs: ReferencedPrefabs(go, errors)));

            TerrainModifier modifier = go.GetComponent<TerrainModifier>();
            if (modifier != null)
                terrain.Add(TerrainFactOf(modifier, child, root, rootName, enabled));

            Walk(child, root, rootName, enabled, underNetworked || networked, children, terrain, errors);
        }
    }

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
            paintType: modifier.m_paintCleared ? modifier.m_paintType.ToString() : "");
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
