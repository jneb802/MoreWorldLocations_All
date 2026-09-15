using System.Collections.Generic;
using System.Linq;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Which locations a server-only world registers.
///
/// The decision is made BEFORE registration rather than by turning features
/// off afterwards, because the existing switches do not describe a vanilla
/// subset: <c>LocationDB.RegisterAll</c> registers the Dungeons pack whatever
/// the port and trader toggles say, so a world built by flipping those toggles
/// would still carry dungeon locations whose interiors a stock client cannot
/// build.
///
/// Membership is by exact name and nothing else. A template that has not been
/// audited is not "probably fine": the audit is what establishes that every
/// part a player needs is a networked child of a prefab the stock client
/// already has, so an unaudited name is blocked. That is also why the
/// allowlist is a fixed list in code for the first release rather than a
/// config key -- an operator adding a name to a config file would be
/// approving a template nobody audited.
/// </summary>
public static class ServerOnlyAllowlist
{
    /// <summary>
    /// The packs that are excluded wholesale, whatever their names. Ports,
    /// traders and trainers bring NPCs, custom items and shipping; dungeons
    /// bring interiors generated from custom rooms. None of that survives on a
    /// client without the mod, and none of it is in the first release.
    /// </summary>
    public static readonly IReadOnlyList<string> ExcludedPacks =
        new[] { "Ports", "Traders", "Trainers", "Dungeons" };

    /// <summary>
    /// Approved templates, keyed by name.
    ///
    /// A name is here because it has been watched end to end on a client that
    /// does not have the mod — arrival by prefab and transform, collision, and
    /// the same site again after a save and a restart — not because an audit
    /// found nothing wrong with it. The audit is screening; it decides what is
    /// worth running, and the run decides what ships. The runtime template is
    /// the authority in any case: MWL_FulingRock1 has two terrain modifiers once
    /// Jötunn has resolved it where the bundle shows one.
    ///
    /// The four below are the ones with that evidence, recorded in
    /// <c>MWL-STEP3-EVIDENCE.md</c>, <c>MWL-STEP4-EVIDENCE.md</c> and
    /// <c>MWL-NIGHT-20260915.md</c>:
    ///
    /// <list type="bullet">
    /// <item><c>MWL_MeadowsTomb4</c> — 11 networked children, no terrain.</item>
    /// <item><c>MWL_WoodTower2</c> — 42 children including a chest, no terrain.</item>
    /// <item><c>MWL_RuinsWell1</c> — one level operation, −2 m, no paint.</item>
    /// <item><c>MWL_Ruins1</c> — level, smooth and Dirt paint together.</item>
    /// </list>
    ///
    /// Deliberately NOT here: <c>MWL_FulingRock1</c>. Its zone-boundary and
    /// failure-recovery runs established how the terrain writer behaves; they
    /// established nothing about its 87 objects, its two spawners or its chest.
    /// </summary>
    public static readonly IReadOnlyCollection<string> Approved = new HashSet<string>
    {
        "MWL_MeadowsTomb4",
        "MWL_WoodTower2",
        "MWL_RuinsWell1",
        "MWL_Ruins1",
    };

    /// <summary>
    /// Whether <paramref name="locationName"/> from <paramref name="packName"/>
    /// is registered in server-only mode, given an approved set.
    /// </summary>
    public static bool Allows(string packName, string locationName, IReadOnlyCollection<string> approved)
    {
        if (ExcludedPacks.Contains(packName))
            return false;
        return approved.Contains(locationName);
    }

    /// <summary>As <see cref="Allows(string, string, IReadOnlyCollection{string})"/>, against <see cref="Approved"/>.</summary>
    public static bool Allows(string packName, string locationName) =>
        Allows(packName, locationName, Approved);

    /// <summary>
    /// The subset of <paramref name="pack"/> that server-only mode registers.
    /// Order is preserved so that placement, which walks the registered list,
    /// does not depend on the iteration order of a set.
    /// </summary>
    public static IEnumerable<MWLLocation> Filter(
        string packName, IEnumerable<MWLLocation> pack, IReadOnlyCollection<string> approved)
    {
        foreach (MWLLocation location in pack)
        {
            if (Allows(packName, location.Name, approved))
                yield return location;
        }
    }

    /// <summary>As <see cref="Filter(string, IEnumerable{MWLLocation}, IReadOnlyCollection{string})"/>, against <see cref="Approved"/>.</summary>
    public static IEnumerable<MWLLocation> Filter(string packName, IEnumerable<MWLLocation> pack) =>
        Filter(packName, pack, Approved);
}
