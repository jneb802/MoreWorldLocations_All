using More_World_Locations_AIO.ServerOnly;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Who the server lets in.
///
/// This is the one decision that stands between MWL and a player with no mod,
/// and it is made twice for every peer: once when the version answer arrives
/// (or does not), and once when PeerInfo arrives. The tests hold both, because
/// the equivalent fix in ProceduralRoads passed the first and failed the second
/// -- it sent the mismatched client an error and then relied on the client to
/// act on it, which a client under someone else's control does not have to do.
/// </summary>
public class AdmissionTests
{
    [Fact]
    public void APeerThatNeverAnswersIsTakenToHaveNoMod()
    {
        Assert.Equal(
            PeerAdmission.Verdict.WithoutMod,
            PeerAdmission.Decide(answeredVersionCheck: false, versionMatched: false));
    }

    [Fact]
    public void APeerWithoutTheModIsAdmitted()
    {
        // The whole point of the mode: the approved locations are ordinary
        // persistent objects by the time this player sees them.
        Assert.True(PeerAdmission.Admits(PeerAdmission.Verdict.WithoutMod));
    }

    [Fact]
    public void APeerOnTheSameVersionIsAdmitted()
    {
        Assert.Equal(
            PeerAdmission.Verdict.SameVersion,
            PeerAdmission.Decide(answeredVersionCheck: true, versionMatched: true));
        Assert.True(PeerAdmission.Admits(PeerAdmission.Verdict.SameVersion));
    }

    [Fact]
    public void APeerOnAnotherVersionIsRefused()
    {
        // Two builds of the mod would disagree about which locations exist and
        // what they are made of, and the client would generate its own half of
        // a site the server never placed.
        Assert.Equal(
            PeerAdmission.Verdict.VersionMismatch,
            PeerAdmission.Decide(answeredVersionCheck: true, versionMatched: false));
        Assert.False(PeerAdmission.Admits(PeerAdmission.Verdict.VersionMismatch));
    }

    // --- what the PeerInfo handler actually has to work from ---------------
    //
    // At PeerInfo the server does not have the peer's version in hand; it has
    // only which of the two lists the peer landed on when its answer arrived.

    [Fact]
    public void AtPeerInfoAPeerOnNeitherListHasNoMod()
    {
        Assert.Equal(
            PeerAdmission.Verdict.WithoutMod,
            PeerAdmission.DecideFor(validated: false, refused: false));
    }

    [Fact]
    public void AtPeerInfoARecordedMismatchIsRefusedAgain()
    {
        // The refusal has to be repeated here. A client that ignores the error
        // it was sent when its version answer arrived would otherwise walk
        // straight in through PeerInfo.
        PeerAdmission.Verdict verdict = PeerAdmission.DecideFor(validated: false, refused: true);
        Assert.Equal(PeerAdmission.Verdict.VersionMismatch, verdict);
        Assert.False(PeerAdmission.Admits(verdict));
    }

    [Fact]
    public void AtPeerInfoAValidatedPeerIsAdmitted()
    {
        Assert.Equal(
            PeerAdmission.Verdict.SameVersion,
            PeerAdmission.DecideFor(validated: true, refused: false));
    }

    // --- the mode switch ---------------------------------------------------

    [Fact]
    public void WithoutServerOnlyModeOnlyAValidatedPeerIsAdmitted()
    {
        // MWL's ordinary content is custom prefabs the client must have, so
        // full mode keeps refusing everything that did not answer. Turning the
        // mode on must be the only thing that changes admission.
        Assert.True(PeerAdmission.AdmitsInFullMode(validated: true));
        Assert.False(PeerAdmission.AdmitsInFullMode(validated: false));
    }

    [Fact]
    public void ServerOnlyModeIsOffUntilItIsSet()
    {
        // Awake reads the config before any peer can connect; an unset flag
        // therefore means MWL behaves exactly as it does today.
        ServerOnlyMode.Set(false);
        Assert.False(ServerOnlyMode.Enabled);

        ServerOnlyMode.Set(true);
        Assert.True(ServerOnlyMode.Enabled);

        ServerOnlyMode.Set(false);
    }
}
