using System;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// How a server-only server says so in the version it announces.
///
/// A client that has MWL is refused by a server-only server, and without this it
/// would be refused with nothing to read: its own version matches, so its
/// handshake handler takes the "same version" branch, says so in the log, and
/// then the connection closes on a bare version error. The player is left with a
/// server that looks like it should work.
///
/// So the server appends a marker to the version it announces. The client sees a
/// version that is not its own, which is already a refusal it understands, and
/// the marker lets it replace the generic "installed X, needed Y" with the one
/// thing the player can actually do.
///
/// The marker is only in what the server ANNOUNCES. What a peer sends is
/// compared against the plain version as before, so a genuine version mismatch
/// is still a version mismatch and this changes nothing in full mode.
/// </summary>
public static class ServerOnlyHandshake
{
    /// <summary>
    /// Appended to the announced version. '+' cannot appear in a version this
    /// mod has ever shipped, so a plain version can never be read as this one.
    /// </summary>
    public const string Marker = "+serveronly";

    /// <summary>What this process tells a new peer its version is.</summary>
    public static string Announce(string modVersion, bool serverOnlyMode)
    {
        if (modVersion == null) throw new ArgumentNullException(nameof(modVersion));
        return serverOnlyMode ? modVersion + Marker : modVersion;
    }

    /// <summary>Whether the version a peer announced says it is a server-only server.</summary>
    public static bool AnnouncesServerOnly(string? announcedVersion) =>
        announcedVersion != null
        && announcedVersion.EndsWith(Marker, StringComparison.Ordinal);

    /// <summary>
    /// The version without the marker, for reporting which build the server is
    /// actually running.
    /// </summary>
    public static string BaseVersion(string? announcedVersion)
    {
        if (announcedVersion == null) return "";
        return AnnouncesServerOnly(announcedVersion)
            ? announcedVersion.Substring(0, announcedVersion.Length - Marker.Length)
            : announcedVersion;
    }

    /// <summary>
    /// What a refused client is shown. Says what to do, not what went wrong:
    /// the mod is working correctly on both sides and the player only needs to
    /// know that this particular server does not want it.
    /// </summary>
    public static string ClientMessage(string modName, string? announcedVersion) =>
        modName + " " + BaseVersion(announcedVersion) + " is running on this server in server-only mode.\n"
        + "Its locations are served as ordinary objects, so play here WITHOUT " + modName + ".\n"
        + "Disable it for this server and reconnect.";
}
