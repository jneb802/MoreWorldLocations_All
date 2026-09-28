using System;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

/// <summary>
/// Whether a reference was acquired, when loading threw.
///
/// <para>The lease used to treat every exception out of <c>Load</c> as a
/// pre-acquisition failure. The loader's order is increment, then load, then
/// tick, then invoke callbacks, and nothing stops one of those later steps
/// throwing — so that guess leaks a reference while the counter cheerfully
/// reports zero outstanding. A counter that reports zero when it cannot tell is
/// worse than one that admits it.</para>
///
/// <para>These drive the production lease through the ownership seam, with the
/// failure placed on either side of the acquisition. What they cannot establish
/// is the real loader's internals; that is what the seam's production
/// implementation reads from the game's own tables.</para>
/// </summary>
public class OwnershipBoundaryTests
{
    /// <summary>A loader whose reference count is real and whose failures can be placed.</summary>
    private sealed class Loader : ITemplateOwnership
    {
        public uint References;
        public bool Observable = true;
        public bool ThrowBeforeAcquiring;
        public bool ThrowAfterAcquiring;
        public bool ThrowOnRelease;

        public Loader(uint externallyHeld = 0) => References = externallyHeld;

        public GameObject? Asset { get; } = new GameObject("fixture");

        public bool TryReferenceCount(out uint count)
        {
            count = References;
            return Observable;
        }

        public void Load()
        {
            if (ThrowBeforeAcquiring)
                throw new InvalidOperationException("initialisation failed before acquisition");
            References++;
            // The loader's own order: hold the reference, then do the work that
            // can fail.
            if (ThrowAfterAcquiring)
                throw new InvalidOperationException("a load callback failed after acquisition");
        }

        public void Release()
        {
            if (ThrowOnRelease)
                throw new InvalidOperationException("release failed");
            References--;
        }
    }

    private static ITemplateHandle? Take(Loader loader)
    {
        TemplateAssets.ResetAccounting();
        // The production lease, not a stand-in. A substitute lease is the one
        // thing that cannot show a defect in the real one.
        return TemplateLease.Take(loader);
    }

    [Fact]
    public void AnOrdinaryLoadLeavesAnExternalOwnersReferenceAlone()
    {
        var loader = new Loader(externallyHeld: 1);

        using (ITemplateHandle? lease = Take(loader))
        {
            Assert.NotNull(lease);
            Assert.Equal(2u, loader.References);
        }

        Assert.Equal(1u, loader.References);
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
        Assert.Equal(0, TemplateAssets.UnresolvedLeases);
    }

    [Fact]
    public void AThrowBeforeAcquiringReleasesNothing()
    {
        // The dangerous direction: releasing what we never took decrements
        // somebody else's hold on a shared asset.
        var loader = new Loader(externallyHeld: 1) { ThrowBeforeAcquiring = true };

        ITemplateHandle? lease = Take(loader);

        Assert.Null(lease);
        Assert.Equal(1u, loader.References);
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
    }

    [Fact]
    public void AThrowAfterAcquiringGivesTheReferenceBack()
    {
        // The leak the review found: the reference was taken, the load failed,
        // and the lease reported nothing outstanding while the count stayed up.
        var loader = new Loader(externallyHeld: 1) { ThrowAfterAcquiring = true };

        ITemplateHandle? lease = Take(loader);

        Assert.Null(lease);
        Assert.Equal(1u, loader.References);
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
        Assert.Equal(0, TemplateAssets.UnresolvedLeases);
    }

    [Fact]
    public void AThrowThatCannotBeLocatedIsReportedRatherThanGuessedAt()
    {
        // No readable count, so neither "released" nor "leaked" can be claimed.
        // Saying so is the point; a confident zero here would be a lie.
        var loader = new Loader(externallyHeld: 1) { ThrowAfterAcquiring = true, Observable = false };

        ITemplateHandle? lease = Take(loader);

        Assert.Null(lease);
        Assert.Equal(1, TemplateAssets.UnresolvedLeases);
        Assert.Contains("could not be read", TemplateAssets.UnresolvedReasons[0]);
    }

    [Fact]
    public void AReleaseThatThrowsStaysCountedRatherThanReportingAFalseZero()
    {
        var loader = new Loader { ThrowOnRelease = true };

        ITemplateHandle? lease = Take(loader);
        lease!.Dispose();

        Assert.Equal(1, TemplateAssets.OutstandingLeases);
        Assert.Equal(1, TemplateAssets.UnresolvedLeases);
        Assert.Contains("releasing threw", TemplateAssets.UnresolvedReasons[0]);
    }

    [Fact]
    public void DisposingTwiceReleasesOnce()
    {
        var loader = new Loader(externallyHeld: 1);

        ITemplateHandle? lease = Take(loader);
        lease!.Dispose();
        lease.Dispose();

        Assert.Equal(1u, loader.References);
        Assert.Equal(0, TemplateAssets.OutstandingLeases);
    }
}
