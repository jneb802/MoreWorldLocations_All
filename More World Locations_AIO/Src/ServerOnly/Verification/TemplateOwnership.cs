using System;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// One asset's ownership, separated from the lease that uses it so that both
/// sides of the acquisition boundary can be tested.
///
/// <para><b>Why a seam.</b> The lease used to catch every exception out of
/// <c>Load</c> and call it a pre-acquisition failure. The loader's own order is
/// increment, then load, then tick, then invoke callbacks — and nothing in it
/// stops a callback throwing after the increment. So "it threw, therefore
/// nothing was acquired" is a guess, and when it is wrong the reference is
/// leaked while the counter reports zero outstanding. The boundary has to be
/// observed, not assumed.</para>
/// </summary>
public interface ITemplateOwnership
{
    /// <summary>The asset, once loaded. Null when it would not load.</summary>
    GameObject? Asset { get; }

    /// <summary>
    /// How many references the loader currently holds for this asset, or false
    /// when this process cannot see the count.
    ///
    /// Reading it before and after a load is what turns "did we acquire?" from a
    /// guess into a measurement.
    /// </summary>
    bool TryReferenceCount(out uint count);

    /// <summary>Acquire and load. May throw, before or after the acquisition.</summary>
    void Load();

    /// <summary>Give one reference back. May throw.</summary>
    void Release();
}

/// <summary>
/// What a lease knows about whether it gave back what it took.
///
/// <see cref="Unresolved"/> is the honest third answer: it exists because a
/// counter that reports zero outstanding when it cannot actually tell is worse
/// than one that says so.
/// </summary>
public enum LeaseOutcome
{
    /// <summary>Nothing was acquired, so there is nothing to give back.</summary>
    NothingAcquired,

    /// <summary>Acquired and released.</summary>
    Released,

    /// <summary>Acquired, and whether it was released could not be established.</summary>
    Unresolved,
}
