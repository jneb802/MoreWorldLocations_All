using System;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The ownership transition itself, with no dependency on the game.
///
/// It lives apart from <see cref="GameTemplateAssets"/> — which knows about
/// Jötunn and the asset loader — precisely so that both sides of the
/// acquisition boundary can be driven in a test. The version of this that lived
/// inside the game-facing class could only be exercised through a substitute
/// lease, and a substitute lease is the one thing that cannot show a defect in
/// the real one.
/// </summary>
public static class TemplateLease
{
    /// <summary>
    /// One reference to one template, given back exactly once, with the
    /// acquisition boundary observed rather than assumed.
    ///
    /// <para><b>What the loader actually does</b>, from the game's own
    /// assembly: <c>SoftReference.Load</c> calls the loader's <c>Load</c>, which
    /// increments the asset's reference count and THEN loads it, ticks, and
    /// invokes callbacks; <c>Release</c> decrements and unloads asynchronously
    /// at zero, where Jötunn also destroys the resolved mock clone. Nothing
    /// stops one of those later steps throwing, so an exception out of
    /// <c>Load</c> is not evidence either way — it is looked up.</para>
    /// </summary>
    private sealed class Lease : ITemplateHandle
    {
        private readonly ITemplateOwnership _ownership;
        private bool _held;

        private Lease(ITemplateOwnership ownership) => _ownership = ownership;

        /// <summary>
        /// Take a lease, or null when there is no such asset and nothing was
        /// acquired.
        ///
        /// A static factory rather than a constructor that loads: a constructor
        /// that throws hands the caller nothing to dispose, and whatever it had
        /// already acquired would be lost.
        /// </summary>
        internal static ITemplateHandle? TakeCore(ITemplateOwnership ownership)
        {
            bool observable = ownership.TryReferenceCount(out uint before);
            try
            {
                // The result is not checked: a failed load still acquired, and
                // the caller learns about it from a null Asset.
                ownership.Load();
                return Held(ownership);
            }
            catch (Exception ex)
            {
                if (!observable || !ownership.TryReferenceCount(out uint after))
                {
                    // Cannot see the count, so cannot say. Reported as unresolved
                    // rather than silently released or silently leaked.
                    TemplateAssets.LeaseUnresolved(
                        $"loading threw ({ex.GetType().Name}) and the loader's reference count could not be read, " +
                        "so whether a reference was acquired is unknown");
                    return null;
                }

                if (after <= before)
                    return null;    // the throw was before the increment: nothing of ours

                // Acquired, then failed. The lease exists so that it is given
                // back; the caller gets no asset out of it.
                var lease = (Lease)Held(ownership);
                lease.Dispose();
                return null;
            }
        }

        private static ITemplateHandle Held(ITemplateOwnership ownership)
        {
            var lease = new Lease(ownership) { _held = true };
            TemplateAssets.LeaseTaken();
            return lease;
        }

        public GameObject? Asset => _held ? _ownership.Asset : null;

        public void Dispose()
        {
            if (!_held)
                return;
            _held = false;
            try
            {
                _ownership.Release();
            }
            catch (Exception ex)
            {
                // Counted as still outstanding, because it may well be. A zero
                // that is reached by assuming a failed release worked is the
                // false zero this whole seam exists to avoid.
                TemplateAssets.LeaseUnresolved($"releasing threw ({ex.GetType().Name}: {ex.Message})");
                return;
            }
            TemplateAssets.LeaseReturned();
        }
    }

    /// <summary>
    /// Take a lease on one asset, or null when nothing was acquired.
    ///
    /// Null covers three different situations and the accounting tells them
    /// apart: there was no such asset, the load failed before acquiring, or it
    /// failed after acquiring and the reference has been given back.
    /// </summary>
    public static ITemplateHandle? Take(ITemplateOwnership ownership)
    {
        if (ownership == null) throw new ArgumentNullException(nameof(ownership));
        return Lease.TakeCore(ownership);
    }
}
