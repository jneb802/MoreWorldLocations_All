using System.Collections.Generic;
using System.Linq;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>Current validator selection and the names actually registered by this process.</summary>
public static class ServerOnlySelection
{
    /// <summary>A station subset can narrow current passing verdicts, never override one.</summary>
    public static bool AllowsValidated(string name, bool compatible, IReadOnlyCollection<string> requested) =>
        compatible && (requested.Count == 0 || requested.Contains(name));

    public static string? ValidationNotice(IReadOnlyCollection<string> requested) =>
        requested.Count == 0 ? null :
            $"{ValidationSwitches.ApproveVariable} is set: validation run restricted to " +
            string.Join(", ", requested.OrderBy(name => name)) +
            ". Every requested template must still pass the current validator.";

    /// <summary>What the run registered, for the log.</summary>
    public static string RegisteredNotice(IReadOnlyCollection<string> registered) =>
        registered.Count == 0
            ? "Server-only mode registered no locations."
            : $"Server-only mode registered {registered.Count}: " +
              string.Join(", ", registered.OrderBy(name => name));

    /// <summary>
    /// What this process actually registered, for the parts of the runtime that
    /// have to tell an MWL location from a vanilla one.
    ///
    /// The terrain conversion is the reason this exists. It converts a
    /// location's modifiers because a stock client cannot build them — it does
    /// not have the template. A VANILLA location's modifiers it does have, and
    /// builds, so converting those as well would add the compiler's deltas on
    /// top of the shaping the client already did and sink every dolmen and
    /// wood house in the world a second time. Membership is by exact name and
    /// an unregistered name is not ours.
    /// </summary>
    public static IReadOnlyCollection<string> Registered { get; private set; } = new HashSet<string>();

    /// <summary>Record what registration produced. Called once, as registration ends.</summary>
    public static void SetRegistered(IEnumerable<string> registered) =>
        Registered = new HashSet<string>(registered ?? new string[0]);

    /// <summary>
    /// Take a name back out of the registered set.
    ///
    /// Called when enforcement withdraws a location this run's validator
    /// does not stand behind. Registration and
    /// the sweep are two steps and the second can overrule the first, so the
    /// record of what this process serves has to be able to shrink — otherwise
    /// the terrain conversion would go on treating a withdrawn template as ours
    /// and convert the modifiers of a site nothing will place.
    /// </summary>
    public static void Forget(string locationName)
    {
        if (string.IsNullOrEmpty(locationName))
            return;
        var remaining = new HashSet<string>(Registered);
        remaining.Remove(locationName);
        Registered = remaining;
    }

    /// <summary>Whether this location is one server-only mode registered.</summary>
    public static bool IsOurs(string locationName) =>
        !string.IsNullOrEmpty(locationName) && Registered.Contains(locationName);

    /// <summary>
    /// Approved names that no pack produced.
    ///
    /// A misspelled name registers no location and looks exactly like a
    /// location the world put nowhere. Those are different problems and only
    /// one of them is worth walking to a site to investigate, so the run says
    /// which it is instead of leaving it to be inferred from an empty map.
    /// </summary>
    public static IReadOnlyList<string> Unmatched(
        IReadOnlyCollection<string> approved, IReadOnlyCollection<string> registered)
    {
        var placed = new HashSet<string>(registered);
        return approved.Where(name => !placed.Contains(name))
                       .OrderBy(name => name)
                       .ToList();
    }
}
