namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Whether this process is running as a server that serves its locations to
/// players who have no mod installed.
///
/// The mode is one operator decision that changes several things at once --
/// which locations are registered, which patches matter, and which peers are
/// admitted -- so every one of them reads this single flag rather than the
/// config entry, and the flag is set once during Awake. That also keeps the
/// decision testable without BepInEx: the tests set it directly.
///
/// It is a config key rather than an environment variable because it is the
/// product, not a validation switch: an operator sets it once in the config
/// file and it stays set. Validation switches stay in the environment, where
/// they cost the user nothing and disappear when unset.
/// </summary>
public static class ServerOnlyMode
{
    /// <summary>
    /// False until Awake reads the config, which is before any peer can
    /// connect: unset therefore means MWL behaves exactly as it does today.
    /// </summary>
    public static bool Enabled { get; private set; }

    public static void Set(bool enabled) => Enabled = enabled;
}
