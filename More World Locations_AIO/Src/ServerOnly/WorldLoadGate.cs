namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Owns the lifetime of one world's initial load. Registration may be retried
/// before loading starts; a failed load needs a new world session because it
/// may already have changed the game's in-memory world.
/// </summary>
public sealed class WorldLoadGate
{
    public enum LoadState { Waiting, Loading, Loaded, Failed }

    private int _ticket;
    public LoadState State { get; private set; }
    public bool Requested { get; private set; }
    public bool CanSave => State == LoadState.Loaded;
    public bool Deferred => Requested && State != LoadState.Loaded;
    public string Failure { get; private set; } = "";

    public void Reset()
    {
        _ticket++;
        State = LoadState.Waiting;
        Requested = false;
        Failure = "";
    }

    public bool TryBegin(bool registrationReady, out int ticket)
    {
        ticket = -1;
        Requested = true;
        if (!registrationReady || State != LoadState.Waiting)
            return false;
        State = LoadState.Loading;
        ticket = ++_ticket;
        return true;
    }

    public void Complete(int ticket, bool gameLoadError, string? exception)
    {
        if (ticket != _ticket || State != LoadState.Loading)
            return; // A skipped call or a finalizer from a previous world.
        if (exception != null || gameLoadError)
        {
            State = LoadState.Failed;
            Failure = exception ?? "Valheim reported a world-load error";
            return;
        }
        State = LoadState.Loaded;
    }
}
