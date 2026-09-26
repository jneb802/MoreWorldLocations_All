namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Tracks initial loads for one process. Scene resets may start another healthy
/// load, but a failed load is terminal until the process restarts: the game may
/// already have changed its in-memory world and can return to the menu on error.
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

    /// <summary>Forget scene state, never a process-terminal load failure.</summary>
    public void Reset()
    {
        _ticket++;
        if (State == LoadState.Failed)
            return;
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
