namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Which peers the server lets in while server-only mode is on. Pure logic:
/// <see cref="VerifyClient"/> does the networking.
///
/// In server-only mode the approved locations reach a player as ordinary
/// persistent ZDOs built from prefabs the game itself ships, so a client with
/// no mod at all plays the world as it is. Such a client never answers the
/// version check; that is not a reason to turn it away, and refusing it is the
/// single reason MWL cannot serve stock players today.
///
/// A client that does answer is a different matter. A matching version is the
/// author's own test client and is admitted. A different version is turned
/// away: two builds of the mod would disagree about which locations exist and
/// what they are made of.
///
/// The decision shape, the two recorded lists and the three-case test come from
/// ProceduralRoads' <c>PeerAdmission</c> (feature/server-only), where the first
/// version relied on the refused client acting on the error it was sent; it
/// does not, so the refusal is repeated here at PeerInfo.
/// </summary>
public static class PeerAdmission
{
    public enum Verdict
    {
        /// <summary>The same build of the mod: admitted.</summary>
        SameVersion,

        /// <summary>
        /// No mod on the client: admitted, it receives the approved locations
        /// as ordinary vanilla objects.
        /// </summary>
        WithoutMod,

        /// <summary>A different build of the mod: turned away.</summary>
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

    public static bool Admits(Verdict verdict) => verdict != Verdict.VersionMismatch;

    /// <summary>
    /// The verdict for a peer whose PeerInfo has arrived, from what the server
    /// recorded when (and if) the peer answered the version check: it is on the
    /// validated list, on the refused list, or on neither.
    /// </summary>
    public static Verdict DecideFor(bool validated, bool refused) =>
        Decide(answeredVersionCheck: validated || refused, versionMatched: validated);

    /// <summary>
    /// What the server does when server-only mode is off: MWL's own content is
    /// custom prefabs the client has to have, so every peer must answer the
    /// version check with a matching version. Kept so that turning the mode on
    /// is the only thing that changes admission.
    /// </summary>
    public static bool AdmitsInFullMode(bool validated) => validated;
}
