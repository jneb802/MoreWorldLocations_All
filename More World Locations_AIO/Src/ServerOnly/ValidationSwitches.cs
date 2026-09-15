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

    /// <summary>How often the outstanding-work clock may run, in seconds.</summary>
    public const string TickSecondsVariable = Prefix + "TICK_SECONDS";

    /// <summary>
    /// The retry clock's interval, for a run that needs to SEE the waiting state.
    ///
    /// At the shipped five seconds a deliberate failure recovers before anyone
    /// can read the ledger, which makes the recovery real but leaves it
    /// ambiguous whether the clock or a neighbouring zone's generation did it.
    /// Lengthening it for one run separates them. Unset means the shipped value;
    /// a value that is not a positive number is reported and ignored.
    /// </summary>
    public static float TickSeconds(float fallback)
    {
        string? value = Environment.GetEnvironmentVariable(TickSecondsVariable);
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        return float.TryParse(value.Trim(), System.Globalization.NumberStyles.Float,
                   System.Globalization.CultureInfo.InvariantCulture, out float parsed) && parsed > 0f
            ? parsed
            : fallback;
    }

    /// <summary>The environment variable that turns on the template lifecycle trace.</summary>
    public const string TraceVariable = Prefix + "TRACE";

    /// <summary>
    /// Which templates a run should trace through load, release and reload, and
    /// which prefabs to probe beside them.
    ///
    /// <para>Value: <c>MWL_A,MWL_B;probe=Pickable_SurtlingCoreStand</c>. The
    /// names before the semicolon are templates the sweep snapshots while it
    /// holds them and again the instant they are released; <c>probe=</c> names
    /// prefabs whose loader state and component identities are recorded in
    /// every snapshot. Unset means no trace and no observers installed, which is
    /// the shipped behaviour.</para>
    /// </summary>
    public static bool TraceRequested(out IReadOnlyCollection<string> templates, out IReadOnlyCollection<string> probes) =>
        ParseTrace(Environment.GetEnvironmentVariable(TraceVariable), out templates, out probes);

    /// <summary>The parse, apart from the environment, so it can be tested.</summary>
    public static bool ParseTrace(string? value, out IReadOnlyCollection<string> templates, out IReadOnlyCollection<string> probes)
    {
        var names = new HashSet<string>();
        var probed = new HashSet<string>();
        templates = names;
        probes = probed;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        foreach (string part in value!.Split(';'))
        {
            string trimmed = part.Trim();
            if (trimmed.Length == 0)
                continue;
            if (trimmed.StartsWith("probe=", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string name in ParseNames(trimmed.Substring("probe=".Length)))
                    probed.Add(name);
            }
            else
            {
                foreach (string name in ParseNames(trimmed))
                    names.Add(name);
            }
        }
        return names.Count > 0 || probed.Count > 0;
    }

    /// <summary>The environment variable narrowing the generated-height budget for one run.</summary>
    public const string HeightBudgetVariable = Prefix + "HEIGHT_BUDGET_BYTES";

    /// <summary>
    /// A smaller height budget, so a station run can reach eviction and
    /// deferral with a few dozen zones instead of two thousand. Unset means the
    /// shipped budget; anything that is not a positive number is ignored.
    /// </summary>
    public static long HeightBudgetBytes(long fallback)
    {
        string? value = Environment.GetEnvironmentVariable(HeightBudgetVariable);
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        return long.TryParse(value.Trim(), out long parsed) && parsed > 0 ? parsed : fallback;
    }

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
    public static bool FaultZoneOnce(out int x, out int z, out int times)
    {
        times = 1;
        string? value = Environment.GetEnvironmentVariable(FaultZoneOnceVariable);
        if (string.IsNullOrWhiteSpace(value))
            return TryParseZone(null, out x, out z);

        // "x,z" or "x,z:n" -- n failures rather than one. More than one is what
        // lets a run watch the outstanding state instead of catching up with a
        // recovery that already happened: zone generation retries constantly
        // while a player moves, so a single failure is repaired before a ledger
        // can be read.
        string zonePart = value;
        int colon = value.IndexOf(':');
        if (colon >= 0)
        {
            zonePart = value.Substring(0, colon);
            if (int.TryParse(value.Substring(colon + 1).Trim(), out int parsed) && parsed > 0)
                times = parsed;
        }
        return TryParseZone(zonePart, out x, out z);
    }

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
