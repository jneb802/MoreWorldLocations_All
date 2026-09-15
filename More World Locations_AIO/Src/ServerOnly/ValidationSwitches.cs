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
