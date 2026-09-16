using System.Collections.Generic;
using System.Linq;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>Pack exclusions and exact-name filtering against a completed validator result.</summary>
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
    /// Whether <paramref name="locationName"/> from <paramref name="packName"/>
    /// is registered in server-only mode, given an approved set.
    /// </summary>
    public static bool Allows(string packName, string locationName, IReadOnlyCollection<string> approved)
    {
        if (ExcludedPacks.Contains(packName))
            return false;
        return approved.Contains(locationName);
    }

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

}
