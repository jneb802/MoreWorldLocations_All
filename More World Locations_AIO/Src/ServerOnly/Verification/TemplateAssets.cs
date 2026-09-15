using System;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// A template held open for as long as it is being read, and exactly one
/// reference to it.
///
/// <para><b>Presence is not ownership.</b> The first version asked whether the
/// asset was already loaded and took a reference only if it was not. That is
/// two bugs in one line: when somebody else had it loaded we never acquired,
/// and when we had loaded it ourselves the release depended on remembering
/// that. The loader counts references — <c>Load</c> increments before it loads,
/// <c>Release</c> decrements and unloads at zero, and Jötunn destroys the
/// resolved clone on that same zero — so a lease that takes one and gives one
/// back is the whole contract, whoever else is holding the asset.</para>
///
/// <para>Disposal is idempotent, and it happens on every path: a normal read, a
/// refusal, a cancellation, an exception. A sweep over 190-odd templates that
/// leaked one reference each would keep the whole catalogue resident.</para>
/// </summary>
public interface ITemplateHandle : IDisposable
{
    /// <summary>The template after mock resolution, or null if it would not load.</summary>
    GameObject? Asset { get; }
}

/// <summary>
/// Where a template to audit comes from, and where a stock prefab to compare it
/// against comes from.
///
/// <para><b>Why this exists at all.</b> The audit used to find templates by
/// looking them up in <c>ZoneSystem.m_locationsByHash</c>, which only holds
/// locations that were already registered — and registration is the decision
/// the audit is supposed to be making. Every unapproved build was therefore
/// reported as unresolved without its asset ever being opened, so iterating the
/// whole catalogue changed the number of rows in the report and not the set of
/// templates actually inspected. Loading has to be independent of eligibility,
/// or the tool can only ever confirm what it already approves.</para>
///
/// <para>The implementation is installed once at startup rather than reached
/// for directly, so that the decision code can be exercised without a running
/// asset manager and so that nothing in the audit depends on a template having
/// been registered first.</para>
/// </summary>
public static class TemplateAssets
{
    /// <summary>
    /// Open one template by name, through the game's ordinary asset loading and
    /// mock resolution. Null when this process has no way to load templates.
    /// </summary>
    public static Func<string, ITemplateHandle?>? Source { get; set; }

    /// <summary>
    /// The stock prefab of a name, for comparing an object against what the
    /// client would actually build. Null when no baseline is available, which
    /// leaves templates unresolved rather than approved.
    /// </summary>
    public static Func<string, GameObject?>? StockPrefabs { get; set; }

    /// <summary>Open a template, or null when there is no source or it will not load.</summary>
    public static ITemplateHandle? Open(string name) =>
        Source == null ? null : Source(name);

    /// <summary>
    /// What is known against the stock prefabs this process compares with, or
    /// empty when nothing is.
    ///
    /// Set once at startup. It exists so that the limit on the baseline is
    /// reported with every verdict rather than living in a comment.
    /// </summary>
    public static string BaselineProvenance { get; set; } = "";

    /// <summary>
    /// Leases taken out and not yet given back.
    ///
    /// Counted here rather than inside an implementation so that a test and a
    /// station run ask the same question of the same number. Zero after a sweep
    /// — success or failure — is the whole of "the audit gave back what it
    /// took".
    /// </summary>
    public static int OutstandingLeases { get; private set; }

    /// <summary>The most leases held at once, so a sweep's working set has a number.</summary>
    public static int PeakLeases { get; private set; }

    /// <summary>The same accounting, reachable from a test double. See LeaseTaken.</summary>
    public static void LeaseTakenForTest() => LeaseTaken();

    /// <summary>The same accounting, reachable from a test double. See LeaseReturned.</summary>
    public static void LeaseReturnedForTest() => LeaseReturned();

    internal static void LeaseTaken()
    {
        OutstandingLeases++;
        if (OutstandingLeases > PeakLeases)
            PeakLeases = OutstandingLeases;
    }

    internal static void LeaseReturned() => OutstandingLeases--;

    /// <summary>
    /// Leases whose fate could not be established: a release that threw, or a
    /// load that threw where the reference count could not be read.
    ///
    /// Kept apart from <see cref="OutstandingLeases"/> and reported, because a
    /// zero reached by assuming the unknown went well is worse than a number
    /// that admits it does not know.
    /// </summary>
    public static int UnresolvedLeases { get; private set; }

    /// <summary>The reasons, in order, so the report names them rather than counting them.</summary>
    public static IReadOnlyList<string> UnresolvedReasons => s_unresolved;

    private static readonly List<string> s_unresolved = new List<string>();

    internal static void LeaseUnresolved(string reason)
    {
        UnresolvedLeases++;
        if (s_unresolved.Count < 32)
            s_unresolved.Add(reason);
    }

    /// <summary>Forget the accounting for a new world. Never called to paper over a leak.</summary>
    public static void ResetAccounting()
    {
        OutstandingLeases = 0;
        PeakLeases = 0;
        UnresolvedLeases = 0;
        s_unresolved.Clear();
    }

    /// <summary>Whether this process can load templates at all.</summary>
    public static bool CanLoad => Source != null;
}
