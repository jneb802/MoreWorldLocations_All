using System;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// A resolved template, held open for as long as it is being read.
///
/// Disposable because the alternative is holding every template in the
/// catalogue at once: a sweep over 190-odd of them would keep the lot in memory
/// on a dedicated server, and a server that ran out of memory checking its
/// locations would be a poor trade for the check.
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

    /// <summary>Whether this process can load templates at all.</summary>
    public static bool CanLoad => Source != null;
}
