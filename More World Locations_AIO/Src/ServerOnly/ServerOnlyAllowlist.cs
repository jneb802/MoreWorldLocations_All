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
    /// Empty until the audit passes one: the starter set is chosen from
    /// <c>template-audit.json</c>, not from how promising a name looks. An
    /// empty allowlist is a server that places no MWL location at all, which
    /// is the correct behaviour for a build whose audit has not run.
    /// </summary>
    public static readonly IReadOnlyCollection<string> Approved = new HashSet<string>();

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
