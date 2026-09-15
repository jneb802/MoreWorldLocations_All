using More_World_Locations_AIO.ServerOnly;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Who the server lets in.
///
/// This is the one decision that stands between MWL and a player with no mod,
/// and it is made twice for every peer: once when the version answer arrives (or
/// does not), and once when PeerInfo arrives. Both are held here, because the
/// equivalent fix in ProceduralRoads passed the first and failed the second — it
/// sent the refused client an error and then relied on the client to act on it,
/// which a client under someone else's control does not have to do.
///
/// The two modes have opposite rules, so every case is stated for both. Reading
/// only one column is how a client walks in through the mode that was not being
/// looked at.
/// </summary>
public class AdmissionTests
{
    private const bool ServerOnly = true;
    private const bool FullMode = false;

    // --- reading the version answer ---------------------------------------

    [Fact]
    public void APeerThatNeverAnswersIsTakenToHaveNoMod()
    {
        Assert.Equal(
            PeerAdmission.Verdict.WithoutMod,
            PeerAdmission.Decide(answeredVersionCheck: false, versionMatched: false));
    }

    [Fact]
    public void APeerOnTheSameVersionIsSeenAsSuch()
    {
        Assert.Equal(
            PeerAdmission.Verdict.SameVersion,
            PeerAdmission.Decide(answeredVersionCheck: true, versionMatched: true));
    }

    [Fact]
    public void APeerOnAnotherVersionIsSeenAsSuch()
    {
        Assert.Equal(
            PeerAdmission.Verdict.VersionMismatch,
            PeerAdmission.Decide(answeredVersionCheck: true, versionMatched: false));
    }

    // --- server-only mode: stock clients, and only stock clients -----------

    [Fact]
    public void ServerOnlyAdmitsAPeerWithoutTheMod()
    {
        // The whole point of the mode: the approved locations are ordinary
        // persistent objects by the time this player sees them.
        Assert.True(PeerAdmission.Admits(PeerAdmission.Verdict.WithoutMod, ServerOnly));
    }

    [Fact]
    public void ServerOnlyRefusesAPeerRunningTheSameBuildOfTheMod()
    {
        // Not an oversight: such a client builds the proxy half of every site
        // locally, putting its own terrain modifiers on top of the shaping the
        // server already persisted and cutting the ground twice. Mixed clients
        // need suppression and their own evidence; the first milestone does not
        // have either, so it does not pretend to support them.
        Assert.False(PeerAdmission.Admits(PeerAdmission.Verdict.SameVersion, ServerOnly));
        Assert.Contains("without More World Locations", PeerAdmission.RefusalReason(
            PeerAdmission.Verdict.SameVersion, ServerOnly));
    }

    [Fact]
    public void ServerOnlyRefusesAPeerOnAnotherVersion()
    {
        Assert.False(PeerAdmission.Admits(PeerAdmission.Verdict.VersionMismatch, ServerOnly));
    }

    // --- full mode is untouched -------------------------------------------

    [Fact]
    public void FullModeAdmitsOnlyTheMatchingVersion()
    {
        // MWL's ordinary content is custom prefabs the client must have. Turning
        // server-only mode on must be the only thing that changes admission.
        Assert.True(PeerAdmission.Admits(PeerAdmission.Verdict.SameVersion, FullMode));
        Assert.False(PeerAdmission.Admits(PeerAdmission.Verdict.WithoutMod, FullMode));
        Assert.False(PeerAdmission.Admits(PeerAdmission.Verdict.VersionMismatch, FullMode));
    }

    [Fact]
    public void TheTwoModesAgreeOnNothingExceptRefusingAVersionMismatch()
    {
        // Stated as a property so a future third verdict cannot be added to one
        // mode and quietly inherited by the other.
        foreach (PeerAdmission.Verdict verdict in new[]
                 {
                     PeerAdmission.Verdict.SameVersion,
                     PeerAdmission.Verdict.WithoutMod,
                     PeerAdmission.Verdict.VersionMismatch,
                 })
        {
            bool serverOnly = PeerAdmission.Admits(verdict, ServerOnly);
            bool full = PeerAdmission.Admits(verdict, FullMode);
            if (verdict == PeerAdmission.Verdict.VersionMismatch)
                Assert.False(serverOnly || full);
            else
                Assert.NotEqual(serverOnly, full);
        }
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
        // The refusal has to be repeated here. A client that ignores the error it
        // was sent when its version answer arrived would otherwise walk straight
        // in through PeerInfo.
        PeerAdmission.Verdict verdict = PeerAdmission.DecideFor(validated: false, refused: true);
        Assert.Equal(PeerAdmission.Verdict.VersionMismatch, verdict);
        Assert.False(PeerAdmission.Admits(verdict, ServerOnly));
        Assert.False(PeerAdmission.Admits(verdict, FullMode));
    }

    [Fact]
    public void AtPeerInfoAValidatedPeerIsStillRefusedByServerOnlyMode()
    {
        // The shortcut that used to sit at the top of the PeerInfo patch --
        // "already validated, so let it in" -- would have walked a modded client
        // past the one refusal server-only mode exists to make. The verdict has
        // to be reached for validated peers too.
        PeerAdmission.Verdict verdict = PeerAdmission.DecideFor(validated: true, refused: false);
        Assert.Equal(PeerAdmission.Verdict.SameVersion, verdict);
        Assert.False(PeerAdmission.Admits(verdict, ServerOnly));
        Assert.True(PeerAdmission.Admits(verdict, FullMode));
    }

    // --- the mode switch ---------------------------------------------------

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

    // --- telling the refused client what to do -----------------------------

    [Fact]
    public void AServerOnlyServerSaysSoInTheVersionItAnnounces()
    {
        Assert.Equal("5.0.9+serveronly", ServerOnlyHandshake.Announce("5.0.9", serverOnlyMode: true));
        Assert.Equal("5.0.9", ServerOnlyHandshake.Announce("5.0.9", serverOnlyMode: false));
    }

    [Fact]
    public void AClientCanTellAServerOnlyServerFromAnOrdinaryOne()
    {
        Assert.True(ServerOnlyHandshake.AnnouncesServerOnly("5.0.9+serveronly"));
        Assert.False(ServerOnlyHandshake.AnnouncesServerOnly("5.0.9"));
        Assert.False(ServerOnlyHandshake.AnnouncesServerOnly(null));
        Assert.False(ServerOnlyHandshake.AnnouncesServerOnly(""));
    }

    [Fact]
    public void TheAnnouncedVersionStillReportsWhichBuildTheServerRuns()
    {
        Assert.Equal("5.0.9", ServerOnlyHandshake.BaseVersion("5.0.9+serveronly"));
        Assert.Equal("5.0.9", ServerOnlyHandshake.BaseVersion("5.0.9"));
        Assert.Equal("", ServerOnlyHandshake.BaseVersion(null));
    }

    [Fact]
    public void ARefusedClientIsToldWhatToDoRatherThanWhatWentWrong()
    {
        // Without this the player sees "Installed: 5.0.9, Needed: 5.0.9" and a
        // closed connection: both sides working correctly, nothing to act on.
        string message = ServerOnlyHandshake.ClientMessage("More_World_Locations_AIO", "5.0.9+serveronly");
        Assert.Contains("WITHOUT More_World_Locations_AIO", message);
        Assert.Contains("Disable it for this server", message);
        Assert.DoesNotContain(ServerOnlyHandshake.Marker, message);
    }

    [Fact]
    public void TheMarkerCannotBeMistakenForAVersion()
    {
        // A plain version must never read as an announcement, or an ordinary
        // server would turn its own matching clients away.
        foreach (string version in new[] { "5.0.9", "5.0.10", "5.1.0-rc1", "0.0.0" })
            Assert.False(ServerOnlyHandshake.AnnouncesServerOnly(version));
    }
}
