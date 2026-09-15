using System;
using System.Collections.Generic;
using System.Linq;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// What a zone owes and has not paid.
///
/// A site's terrain is written per zone, and a zone can refuse for reasons that
/// are not failures: its compiler belongs to another peer, its generated heights
/// are not built yet, it is not loaded here. Those have to stay outstanding —
/// the earlier version logged and returned, which is indistinguishable from
/// success to everything downstream and leaves a site half shaped with nobody
/// counting.
///
/// Kept apart from the patch so the states and their transitions can be tested
/// without a world.
/// </summary>
public static class LocationTerrainLedger
{
    /// <summary>Why a (site, zone) conversion has not been written.</summary>
    public enum State
    {
        /// <summary>Written and confirmed. Not retried.</summary>
        Done,
        /// <summary>Not written yet for a reason that may pass: retry.</summary>
        Waiting,
        /// <summary>Not written and retrying will not help: a decision is needed.</summary>
        Failed,
    }

    public readonly struct Entry
    {
        public readonly string SiteId;
        public readonly Vector2s Zone;
        public readonly string LocationName;
        public readonly State State;
        public readonly string Reason;
        public readonly int Attempts;

        public Entry(string siteId, Vector2s zone, string locationName, State state, string reason, int attempts)
        {
            SiteId = siteId;
            Zone = zone;
            LocationName = locationName;
            State = state;
            Reason = reason;
            Attempts = attempts;
        }

        public Entry With(State state, string reason) =>
            new Entry(SiteId, Zone, LocationName, state, reason, Attempts + 1);
    }

    /// <summary>
    /// How many times one (site, zone) may be attempted before it stops being
    /// "waiting" and becomes a failure someone has to look at. Unbounded waiting
    /// is how a site that will never be written stays invisible.
    /// </summary>
    public const int MaxAttempts = 40;

    private static readonly Dictionary<string, Entry> s_entries = new Dictionary<string, Entry>();

    private static string Key(string siteId) => siteId;

    /// <summary>Forget everything: a different world owes different zones.</summary>
    public static void Reset() => s_entries.Clear();

    /// <summary>Whether this conversion is already written and confirmed.</summary>
    public static bool IsDone(string siteId) =>
        s_entries.TryGetValue(Key(siteId), out Entry e) && e.State == State.Done;

    /// <summary>Record an outcome. Returns the entry as it now stands.</summary>
    public static Entry Record(string siteId, Vector2s zone, string locationName, State state, string reason)
    {
        Entry entry = s_entries.TryGetValue(Key(siteId), out Entry existing)
            ? existing.With(state, reason)
            : new Entry(siteId, zone, locationName, state, reason, 1);

        // A wait that has gone on long enough is a failure, not a wait. It keeps
        // its reason so the report says what it was waiting for.
        if (entry.State == State.Waiting && entry.Attempts >= MaxAttempts)
            entry = new Entry(entry.SiteId, entry.Zone, entry.LocationName, State.Failed,
                $"gave up after {entry.Attempts} attempts: {entry.Reason}", entry.Attempts);

        s_entries[Key(siteId)] = entry;
        return entry;
    }

    /// <summary>Entries still waiting, oldest first, so a caller can retry them.</summary>
    /// <summary>
    /// Record a wait that is NOT an attempt: the work was not tried because
    /// something this mode owns — the height budget — had no room for it yet.
    ///
    /// A scheduling or memory-pressure wait must not spend a site's failure
    /// budget: forty deferrals in a busy stretch would otherwise turn a healthy
    /// site into a reported failure without a single write having been tried.
    /// The entry keeps its attempt count exactly as it was.
    /// </summary>
    public static Entry Defer(string siteId, Vector2s zone, string locationName, string reason)
    {
        Entry entry = s_entries.TryGetValue(Key(siteId), out Entry existing)
            ? new Entry(existing.SiteId, existing.Zone, existing.LocationName,
                existing.State == State.Done ? State.Done : State.Waiting,
                existing.State == State.Done ? existing.Reason : reason, existing.Attempts)
            : new Entry(siteId, zone, locationName, State.Waiting, reason, 0);
        s_entries[Key(siteId)] = entry;
        return entry;
    }

    public static IReadOnlyList<Entry> Waiting() =>
        s_entries.Values.Where(e => e.State == State.Waiting)
                        .OrderByDescending(e => e.Attempts).ToList();

    /// <summary>Entries that will not be written without a decision.</summary>
    public static IReadOnlyList<Entry> Failures() =>
        s_entries.Values.Where(e => e.State == State.Failed).ToList();

    /// <summary>Everything, for a report.</summary>
    public static IReadOnlyList<Entry> All() => s_entries.Values.ToList();

    /// <summary>
    /// One line saying where the terrain stands. "Complete" means every
    /// conversion this process planned is written and confirmed — structures
    /// existing in ZDOs is not the same thing, and a site with a waiting zone is
    /// not finished.
    /// </summary>
    public static string Status()
    {
        int done = s_entries.Values.Count(e => e.State == State.Done);
        IReadOnlyList<Entry> waiting = Waiting();
        IReadOnlyList<Entry> failed = Failures();
        if (s_entries.Count == 0)
            return "Terrain: nothing planned yet.";
        string line = $"Terrain: {done} written, {waiting.Count} waiting, {failed.Count} failed, " +
                      $"of {s_entries.Count} (site, zone) conversion(s).";
        foreach (Entry e in failed.Take(5))
            line += $"\n  FAILED {e.SiteId}: {e.Reason}";
        foreach (Entry e in waiting.Take(5))
            line += $"\n  waiting {e.SiteId} (attempt {e.Attempts}): {e.Reason}";
        return line;
    }
}
