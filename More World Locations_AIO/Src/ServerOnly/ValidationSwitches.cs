using System;
using System.Collections.Generic;
using System.Linq;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Switches that exist to validate this mode, not to play with.
///
/// They are read from the environment rather than bound into the config file.
/// A config key cannot be taken back once it has been written to a user's
/// disk: renaming or removing it leaves the old key sitting there, and a
/// setting that only ever mattered to whoever was proving the mode works is
/// not worth that. An environment variable costs the user nothing, appears
/// nowhere, and disappears when it is no longer set.
///
/// Every switch is named <c>MOREWORLDLOCATIONS_&lt;NAME&gt;</c> and an unset
/// environment is exactly the shipped behaviour.
/// </summary>
public static class ValidationSwitches
{
    internal const string Prefix = "MOREWORLDLOCATIONS_";

    /// <summary>The environment variable naming templates to register for a validation run.</summary>
    public const string ApproveVariable = Prefix + "APPROVE";

    /// <summary>
    /// Templates to add to <see cref="ServerOnlyAllowlist.Approved"/> for this
    /// process only, comma separated.
    ///
    /// <para>The shipped <c>Approved</c> set stays empty until a template has
    /// passed in game — that is the rule this switch exists to serve, not to
    /// get around. A name here is a template someone is about to watch on a
    /// stock client, and it lives for one process.</para>
    /// </summary>
    public static IReadOnlyCollection<string> ApprovedForValidation() =>
        ParseNames(Environment.GetEnvironmentVariable(ApproveVariable));

    /// <summary>The environment variable naming a zone whose first write must fail.</summary>
    public const string FaultZoneOnceVariable = Prefix + "FAULT_ZONE_ONCE";

    /// <summary>
    /// A zone whose FIRST terrain write is made to fail, once, and never again.
    ///
    /// It exists to prove in a running game what the tests prove in isolation:
    /// that a write which does not land is reported as outstanding rather than
    /// vanishing, and that the next attempt completes it. Unset by default, so
    /// an unset environment behaves exactly as the shipped build.
    /// </summary>
    public static bool FaultZoneOnce(out int x, out int z) =>
        TryParseZone(Environment.GetEnvironmentVariable(FaultZoneOnceVariable), out x, out z);

    /// <summary>
    /// "x,z" as zone indices. Anything else names no zone rather than a wrong
    /// one: a typo that silently faulted zone 0,0 would be worse than no switch.
    /// </summary>
    public static bool TryParseZone(string? value, out int x, out int z)
    {
        x = 0;
        z = 0;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        string[] parts = value.Split(',');
        return parts.Length == 2
               && int.TryParse(parts[0].Trim(), out x)
               && int.TryParse(parts[1].Trim(), out z);
    }

    /// <summary>
    /// The parse, separately from the environment, so it can be tested and so
    /// the caller can report what it read.
    ///
    /// Empty and whitespace-only entries are dropped rather than becoming a
    /// template named "": a trailing comma is a typo, not a request.
    /// </summary>
    public static IReadOnlyCollection<string> ParseNames(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new HashSet<string>();

        return new HashSet<string>(
            value.Split(',')
                 .Select(name => name.Trim())
                 .Where(name => name.Length > 0));
    }
}
