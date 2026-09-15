using System.Collections.Generic;
using System.Globalization;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// Whether one template can be served to a client that does not have the mod.
///
/// <para>Pure, and separate from the walk that produced the facts, for the same
/// reason the terrain arithmetic is separate from the zone hook: this is where
/// every judgement is made, and a judgement that needs a running world to
/// exercise is a judgement nobody exercises. The extractor answers "what is in
/// this template"; this answers "and can a stock client build it".</para>
///
/// <para><b>What it deliberately does not do.</b> It never asks whether a
/// template is well built. A stairway that stops part way up, a room with one
/// door, a tower with no roof — those are the author's, they arrive on the
/// client exactly as authored, and they are not this mode's business. The only
/// question here is whether what arrives is what was drawn.</para>
/// </summary>
public static class TemplatePolicy
{
    /// <summary>
    /// A zone's side, in metres. The terrain conversion sees a site's own zone
    /// and its eight neighbours, so an operation reaching no further than one
    /// zone from the location's origin is inside that ring wherever in the zone
    /// the location lands.
    /// </summary>
    public const float ZoneSize = 64f;

    /// <summary>The prefix Jötunn strips when it swaps a mock for the real prefab.</summary>
    public const string MockPrefix = "JVLmock_";

    /// <summary>
    /// Evaluate one template.
    /// </summary>
    /// <param name="facts">Read off the template AFTER Jötunn resolved it.</param>
    /// <param name="registry">What the stock client has. An empty one makes every verdict unresolved rather than blocked.</param>
    /// <param name="excludedPacks">Packs this mode does not serve, whatever they contain.</param>
    /// <param name="components">Which foreign components are forgiven, and which assemblies are the game's.</param>
    public static TemplateEvaluation Evaluate(
        TemplateFacts facts,
        StockPrefabRegistry registry,
        IReadOnlyCollection<string> excludedPacks,
        ComponentPolicy? components = null)
    {
        if (facts == null) throw new System.ArgumentNullException(nameof(facts));
        if (registry == null) throw new System.ArgumentNullException(nameof(registry));
        if (excludedPacks == null) throw new System.ArgumentNullException(nameof(excludedPacks));
        components ??= ComponentPolicy.Default;

        // Scope first, and on its own. A pack this mode does not serve is not
        // "compatible but withheld" and not "blocked": running the technical
        // rules over it would produce reasons nobody should act on, and an
        // operator reading that a trader post was excluded for a custom
        // component would go and fix the wrong thing.
        if (Contains(excludedPacks, facts.Pack))
        {
            return One(facts, TemplateVerdict.ScopeExcluded, new TemplateFinding(
                FindingCodes.ScopeExcludedPack, FindingSeverity.Blocking, "",
                $"the {facts.Pack} pack is outside server-only mode: its locations need client code this mode does not ship.",
                facts.Pack));
        }

        if (!facts.SourceDeclared)
        {
            return One(facts, TemplateVerdict.MissingDefinition, new TemplateFinding(
                FindingCodes.NoActiveDefinition, FindingSeverity.Blocking, "",
                "an asset exists under this name and no location definition places it, so nothing would be built anywhere. " +
                "This is a gap in the definitions, not a prefab the stock client cannot handle."));
        }

        var findings = new List<TemplateFinding>();

        // Unresolved is a verdict, not a quiet pass. "No findings" over a walk
        // that stopped early says only that nothing was looked at.
        if (!facts.AssetLoaded)
        {
            findings.Add(new TemplateFinding(FindingCodes.TemplateNotLoaded, FindingSeverity.Blocking, "",
                "the soft-referenced template would not load, so nothing is known about what it contains."));
        }
        foreach (string error in facts.ExtractionErrors)
        {
            findings.Add(new TemplateFinding(FindingCodes.ExtractionIncomplete, FindingSeverity.Blocking, "",
                "the template could not be read all the way through: " + error));
        }
        if (!facts.Complete && facts.ExtractionErrors.Count == 0)
        {
            findings.Add(new TemplateFinding(FindingCodes.ExtractionIncomplete, FindingSeverity.Blocking, "",
                "the walk over the template did not finish and gave no reason."));
        }
        if (registry.Count == 0)
        {
            findings.Add(new TemplateFinding(FindingCodes.RegistryUnavailable, FindingSeverity.Blocking, "",
                "there is no stock prefab snapshot to check names against. Without it every name looks unknown, " +
                "which would exclude the whole catalogue for a missing file."));
        }
        if (findings.Count > 0)
            return new TemplateEvaluation(facts.Name, facts.Pack, TemplateVerdict.Unresolved, findings);

        EvaluateTemplate(facts, findings);
        foreach (ChildFact child in facts.Children)
            EvaluateChild(child, registry, components, findings);
        EvaluateEmission(facts, findings);
        EvaluateTerrain(facts, findings);

        bool blocked = false;
        foreach (TemplateFinding finding in findings)
        {
            if (finding.Severity == FindingSeverity.Blocking)
            {
                blocked = true;
                break;
            }
        }
        return new TemplateEvaluation(facts.Name, facts.Pack,
            blocked ? TemplateVerdict.Blocked : TemplateVerdict.Compatible, findings);
    }

    private static void EvaluateTemplate(TemplateFacts facts, List<TemplateFinding> findings)
    {
        if (!facts.RootActive)
        {
            findings.Add(new TemplateFinding(FindingCodes.InactiveRoot, FindingSeverity.Blocking, facts.Name,
                "the template root is inactive, so Utils.IsEnabledInHeirarchy rejects every child and the site is placed empty. " +
                "Nothing arrives on any client, with the mod or without it."));
        }

        if (facts.GeneratesInterior)
        {
            string attached = facts.InteriorPrefabName.Length > 0 ? facts.InteriorPrefabName : facts.DungeonTheme;
            findings.Add(new TemplateFinding(FindingCodes.InteriorDungeon, FindingSeverity.Blocking, facts.Name,
                $"the definition attaches the interior '{attached}', which vanilla's dungeon generator builds at spawn " +
                "time out of a room set rather than placing from the template. Those rooms are prefabs this mode does " +
                "not ship, so the objects the walk can see are not the whole site and the part it cannot see is the " +
                "part a player goes inside.", attached));
        }
    }

    private static void EvaluateChild(
        ChildFact child, StockPrefabRegistry registry, ComponentPolicy components, List<TemplateFinding> findings)
    {
        // Vanilla reads a location's children with
        // Utils.GetEnabledComponentsInChildren, so a disabled object is spawned
        // on no client at all and is nobody's difference.
        if (!child.EnabledInHierarchy)
            return;

        if (HasMockPrefix(child.PrefabName))
        {
            EvaluateUnresolvedMock(child, findings);
            // Everything below asks what the resolved prefab does. There isn't
            // one, so asking would invent an answer.
            return;
        }

        if (child.Networked)
        {
            EvaluateIdentity(child, registry, findings);
            EvaluateBehaviour(child, components, findings);
            EvaluatePersistence(child, findings);
            EvaluateScale(child, findings);
            EvaluateSpawnReferences(child, registry, findings);
            return;
        }

        // A non-networked child of a networked object is not lost: the client
        // instantiates the whole stock prefab, its own children included. Only
        // an object with no networked ancestor exists solely in the half of the
        // template that a client without the mod never builds.
        if (!child.UnderNetworkedAncestor)
            EvaluateProxyOnly(child, findings);
    }

    private static void EvaluateUnresolvedMock(ChildFact child, List<TemplateFinding> findings)
    {
        string name = child.PrefabName;
        if (HasMockPrefix(name.Substring(MockPrefix.Length)))
        {
            findings.Add(new TemplateFinding(FindingCodes.DoubleMockPrefix, FindingSeverity.Blocking, child.Path,
                $"'{name}' carries the mock prefix twice. Jötunn strips one, looks for " +
                $"'{name.Substring(MockPrefix.Length)}', does not find it, and never swaps the real prefab in — " +
                "on the server as well as the client, so the object the author placed exists nowhere.", name));
            return;
        }

        findings.Add(new TemplateFinding(FindingCodes.UnresolvedMock, FindingSeverity.Blocking, child.Path,
            $"'{name}' is still a mock after resolution, so the server itself holds a placeholder where the author " +
            "placed an object. Nothing is sent to the client because there is nothing there.", name));
    }

    private static void EvaluateIdentity(ChildFact child, StockPrefabRegistry registry, List<TemplateFinding> findings)
    {
        // The ZDO carries the hash, so the hash is the identity. Asking the
        // registry what this name's hash actually reaches answers both
        // questions at once: nothing, something else, or this prefab.
        string? reached = registry.ResolvesTo(child.PrefabName);
        if (reached == null)
        {
            findings.Add(new TemplateFinding(FindingCodes.UnknownPrefab, FindingSeverity.Blocking, child.Path,
                $"'{child.PrefabName}' is not in the stock prefab snapshot ({registry.GameBuildId}). The client receives a " +
                "ZDO whose prefab hash resolves to nothing and builds no object at all.", child.PrefabName));
            return;
        }
        if (!string.Equals(reached, child.PrefabName, System.StringComparison.Ordinal))
        {
            findings.Add(new TemplateFinding(FindingCodes.PrefabHashCollision, FindingSeverity.Blocking, child.Path,
                $"'{child.PrefabName}' hashes to the same value as the stock prefab '{reached}'. The client looks up the " +
                $"hash, so it would build '{reached}' here — the wrong object, silently, and only on the client.",
                child.PrefabName));
        }
    }

    private static void EvaluateBehaviour(ChildFact child, ComponentPolicy components, List<TemplateFinding> findings)
    {
        foreach (string component in child.ForeignComponents)
        {
            if (components.IsHarmless(component))
                continue;
            findings.Add(new TemplateFinding(FindingCodes.CustomComponent, FindingSeverity.Blocking, child.Path,
                $"'{component}' is not a type the stock client has. Whatever this object does, the client cannot " +
                "reconstruct it from ordinary saved data, so its behaviour exists only where the mod is installed.",
                component));
        }
    }

    private static void EvaluatePersistence(ChildFact child, List<TemplateFinding> findings)
    {
        if (child.Persistent)
            return;
        findings.Add(new TemplateFinding(FindingCodes.NotPersistent, FindingSeverity.Blocking, child.Path,
            $"'{child.PrefabName}' has a non-persistent ZNetView, so its ZDO is never saved. A client with the template " +
            "rebuilds the object from the template after a restart; a client without it has only the save, and the " +
            "object is gone.", child.PrefabName));
    }

    private static void EvaluateScale(ChildFact child, List<TemplateFinding> findings)
    {
        if (child.Scale.IsUnit || child.SyncInitialScale)
            return;
        findings.Add(new TemplateFinding(FindingCodes.ScaleNotSynced, FindingSeverity.Blocking, child.Path,
            $"'{child.PrefabName}' is authored at scale {child.Scale} and its ZNetView does not send the initial scale, " +
            "so the client builds it at 1. The server and the client would disagree about the size of a solid object.",
            child.Scale.ToString()));
    }

    private static void EvaluateSpawnReferences(ChildFact child, StockPrefabRegistry registry, List<TemplateFinding> findings)
    {
        foreach (string referenced in child.ReferencedPrefabs)
        {
            if (registry.Has(referenced))
                continue;
            findings.Add(new TemplateFinding(FindingCodes.UnknownSpawnReference, FindingSeverity.Blocking, child.Path,
                $"'{child.PrefabName}' can put '{referenced}' into the world — a spawn, a drop or a container's " +
                "contents — and the stock client has no such prefab. A branch that is taken on some seeds and not " +
                "others is worse than one that always fails, so the reference is judged whether or not it is rolled.",
                referenced));
        }
    }

    private static void EvaluateProxyOnly(ChildFact child, List<TemplateFinding> findings)
    {
        if (!child.HasRenderer && !child.HasCollider)
            return;

        string what = child.HasRenderer && child.HasCollider ? "rendered and collidable"
            : child.HasRenderer ? "rendered" : "collidable";
        findings.Add(new TemplateFinding(FindingCodes.EssentialProxyOnly, FindingSeverity.Blocking, child.Path,
            $"'{child.PrefabName}' is {what} and has no networked ancestor, so only a client holding the template " +
            "would build it. A client without the mod walks through it, or past a gap where it should be.",
            child.PrefabName));
    }

    private static void EvaluateEmission(TemplateFacts facts, List<TemplateFinding> findings)
    {
        int emitted = 0;
        foreach (ChildFact child in facts.Children)
        {
            if (child.EnabledInHierarchy && child.Networked)
                emitted++;
        }
        int terrain = 0;
        foreach (TerrainFact modifier in facts.Terrain)
        {
            if (modifier.Enabled)
                terrain++;
        }
        if (emitted > 0 || terrain > 0)
            return;

        findings.Add(new TemplateFinding(FindingCodes.NothingEmitted, FindingSeverity.Blocking, facts.Name,
            "the template emits no networked object and shapes no ground, so a client without the mod finds an " +
            "empty patch where the map says there is a location."));
    }

    private static void EvaluateTerrain(TemplateFacts facts, List<TemplateFinding> findings)
    {
        foreach (TerrainFact modifier in facts.Terrain)
        {
            // A disabled modifier shapes nothing on any client; one that already
            // writes through a compiler reaches the client on its own and the
            // conversion leaves it alone.
            if (!modifier.Enabled || modifier.UseTerrainCompiler)
                continue;

            if (modifier.Reach <= ZoneSize)
                continue;

            // Advisory, and this is the line between the two checks the whole
            // design turns on. The template is convertible; what is in doubt is
            // whether a PARTICULAR placement can be served, and that is the
            // per-site preflight's answer, not this one's. Excluding the
            // template here would withhold every site because some site might
            // straddle the wrong zones.
            findings.Add(new TemplateFinding(FindingCodes.TerrainReachBeyondRing, FindingSeverity.Advisory, modifier.Path,
                string.Format(CultureInfo.InvariantCulture,
                    "shapes ground up to {0:0.#} m from the location's origin, further than the {1:0} m the conversion " +
                    "can see from one zone. Placements that land near a zone edge will be refused by the per-site " +
                    "check and reported by site, which is a placement limit and not a defect in the template.",
                    modifier.Reach, ZoneSize),
                modifier.Reach.ToString("0.#", CultureInfo.InvariantCulture)));
        }
    }

    private static bool HasMockPrefix(string name) =>
        name != null && name.StartsWith(MockPrefix, System.StringComparison.Ordinal);

    private static bool Contains(IReadOnlyCollection<string> names, string value)
    {
        foreach (string name in names)
        {
            if (string.Equals(name, value, System.StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static TemplateEvaluation One(TemplateFacts facts, TemplateVerdict verdict, TemplateFinding finding) =>
        new TemplateEvaluation(facts.Name, facts.Pack, verdict, new[] { finding });
}
