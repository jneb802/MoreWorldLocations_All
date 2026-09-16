namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Which peers the server lets in, and under which mode. Pure logic:
/// <see cref="VerifyClient"/> does the networking.
///
/// Two modes, two rules, and they are opposites.
///
/// In ordinary full mode MWL's content is custom prefabs the client has to have,
/// so a peer is admitted only if it answered the version check with a matching
/// version. That is MWL's behaviour today and nothing here changes it.
///
/// In server-only mode the approved locations reach a player as ordinary
/// persistent ZDOs built from prefabs the game itself ships, so a client with no
/// mod at all plays the world as it is -- and a client that HAS the mod does
/// not. It would build the proxy half of every site locally: its own terrain
/// modifiers on top of the shaping the server already persisted, doubling every
/// cut, and its own copy of anything the template holds outside the networked
/// children. So the first milestone is stock clients only, and a peer that
/// reports MWL is turned away whatever version it reports.
///
/// This is MWL's own handshake, not a detector. It says nothing about other
/// client mods, and a client that chooses not to answer is indistinguishable
/// from one that has nothing to answer with.
///
/// The decision shape, the two recorded lists and the three-case test come from
/// ProceduralRoads' <c>PeerAdmission</c> (feature/server-only), where the first
/// version relied on the refused client acting on the error it was sent; it does
/// not, so the refusal is repeated at PeerInfo.
/// </summary>
public static class PeerAdmission
{
    public enum Verdict
    {
        /// <summary>The same build of the mod.</summary>
        SameVersion,

        /// <summary>
        /// No answer to the version check: taken to be a client without the mod.
        /// </summary>
        WithoutMod,

        /// <summary>A different build of the mod.</summary>
        VersionMismatch,
    }

    /// <param name="answeredVersionCheck">Whether the peer sent a version at all.</param>
    /// <param name="versionMatched">Whether the version it sent matched ours.</param>
    public static Verdict Decide(bool answeredVersionCheck, bool versionMatched)
    {
        if (!answeredVersionCheck)
            return Verdict.WithoutMod;
        return versionMatched ? Verdict.SameVersion : Verdict.VersionMismatch;
    }

    /// <summary>
    /// Whether this peer is let in. The mode is a required argument rather than
    /// something read from a flag inside, so no caller can reach a verdict
    /// without saying which world it is deciding for -- the shortcut that let a
    /// validated peer in before the mode was consulted was exactly that mistake.
    /// </summary>
    public static bool Admits(Verdict verdict, bool serverOnlyMode) =>
        serverOnlyMode
            ? verdict == Verdict.WithoutMod
            : verdict == Verdict.SameVersion;

    /// <summary>
    /// The verdict for a peer whose PeerInfo has arrived, from what the server
    /// recorded when (and if) the peer answered the version check: it is on the
    /// validated list, on the refused list, or on neither.
    /// </summary>
    public static Verdict DecideFor(bool validated, bool refused) =>
        Decide(answeredVersionCheck: validated || refused, versionMatched: validated);

    /// <summary>Why a peer was turned away, for the server's log.</summary>
    public static string RefusalReason(Verdict verdict, bool serverOnlyMode)
    {
        if (Admits(verdict, serverOnlyMode))
            return "";
        if (serverOnlyMode)
            return verdict == Verdict.SameVersion
                ? "this server serves its locations as ordinary objects and is for clients without "
                  + "More World Locations; a client that has it would build every site a second time"
                : "the peer reports another More World Locations version, and this server is for "
                  + "clients without the mod";
        return "the peer did not answer the More World Locations version check with a matching version";
    }
}
