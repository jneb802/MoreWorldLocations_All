using System;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>One-shot station faults. Never reset by a resweep or a world-load retry.</summary>
public sealed class StartupFaults
{
    public const string Variable = "MOREWORLDLOCATIONS_FAULT_STARTUP_ONCE";
    internal static StartupFaults Current { get; set; } =
        new StartupFaults(() => Environment.GetEnvironmentVariable(Variable));

    private readonly Func<string?> _read;
    private string? _mode;
    private bool _readOnce;
    private bool _consumed;

    public StartupFaults(string? mode) : this(() => mode) { }
    private StartupFaults(Func<string?> read) => _read = read;

    private bool Take(string mode)
    {
        if (!_readOnce)
        {
            string value = (_read() ?? "").Trim();
            if (value != "" && value != "registration" && value != "load-throw" && value != "load-error")
                throw new InvalidOperationException($"Unknown {Variable} value '{value}'.");
            _mode = value;
            _readOnce = true;
        }
        if (_consumed || _mode != mode) return false;
        _consumed = true;
        return true;
    }

    public void AfterRegistration(string name, Action<string> announce)
    {
        if (!Take("registration")) return;
        string reason = $"[STARTUP FAULT] registration failed ON PURPOSE after registering '{name}'";
        Announce(announce, reason);
        throw new InvalidOperationException(reason);
    }

    /// <summary>Called only after the real load gate enters Loading; the ordinary finalizer handles both outcomes.</summary>
    public bool BeforeLoad(Action setGameLoadError, Action<string> announce)
    {
        if (Take("load-throw"))
        {
            const string reason = "[STARTUP FAULT] world load threw ON PURPOSE";
            Announce(announce, reason);
            throw new InvalidOperationException(reason);
        }
        if (Take("load-error"))
        {
            Announce(announce, "[STARTUP FAULT] world load returned an error ON PURPOSE");
            setGameLoadError();
            return false;
        }
        return true;
    }

    private static void Announce(Action<string> announce, string message)
    {
        try { announce(message); }
        catch { } // Logging cannot consume a fault without exercising it.
    }
}
