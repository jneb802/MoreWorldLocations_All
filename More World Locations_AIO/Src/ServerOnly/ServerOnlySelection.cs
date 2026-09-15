using System.Collections.Generic;
using System.Linq;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Which templates this process registers in server-only mode, and what it
/// says about the choice afterwards.
///
/// Separate from <see cref="ServerOnlyAllowlist"/>, which answers "is this
/// name approved": this answers "what counts as approved right now, and did
/// the world do what that implied". It is deliberately free of Jötunn and the
/// game so the reasoning can be tested, and <c>LocationDB</c> holds nothing
/// but the wiring.
/// </summary>
public static class ServerOnlySelection
{
    /// <summary>
    /// The approved set for one process: the shipped allowlist, plus whatever
    /// a validation run asked for.
    ///
    /// <para><paramref name="shipped"/> is never modified. It stays empty
    /// until a template has been watched on a stock client — a validation run
    /// adds to a copy that dies with the process, which is the whole reason
    /// the switch is an environment variable.</para>
    ///
    /// <para>Outside server-only mode there is nothing to compose: every
    /// location registers, so a validation request is meaningless and is
    /// ignored rather than quietly remembered.</para>
    /// </summary>
    public static HashSet<string> Compose(
        IReadOnlyCollection<string> shipped, IReadOnlyCollection<string> requested, bool serverOnly)
    {
        var approved = new HashSet<string>(shipped);
        if (!serverOnly)
            return approved;

        foreach (string name in requested)
            approved.Add(name);
        return approved;
    }

    /// <summary>
    /// The line to log when a validation run has added templates, or null when
    /// it has not. Loud, and it says what it is: nobody should read a
    /// validation run's world as evidence about the shipped build.
    /// </summary>
    public static string? ValidationNotice(IReadOnlyCollection<string> requested)
    {
        if (requested.Count == 0)
            return null;

        return $"{ValidationSwitches.ApproveVariable} is set: registering {requested.Count} " +
               "template(s) the shipped allowlist does not approve — " +
               string.Join(", ", requested.OrderBy(name => name)) +
               ". This is a validation run, not a release configuration.";
    }

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
