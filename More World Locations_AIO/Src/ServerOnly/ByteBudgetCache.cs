using System;
using System.Collections.Generic;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// A cache that knows how many bytes it is holding, evicts the least recently
/// used when it would go over, and never evicts something in use.
///
/// <para><b>Why a budget and not a count.</b> The two things cached here are
/// wildly different sizes — a zone's generated heights are a fixed 16 kB, a
/// template's subtree signature is anything from a line to a megabyte — so "a
/// hundred entries" is not a memory bound in either case. A byte budget is, and
/// it is the number a run can be held to.</para>
///
/// <para><b>Why leases.</b> The heights cache exists because the terrain builder
/// hands its answer over once; two callers need it, and the second must not find
/// it evicted between them. An entry in use is pinned, and eviction skips it —
/// so a cache under pressure defers rather than pulling the ground out from
/// under a conversion in progress.</para>
/// </summary>
public sealed class ByteBudgetCache<TKey, TValue> where TValue : class
{
    internal sealed class Entry
    {
        public TValue Value;
        public int Bytes;
        public int Pins;
        public long UsedAt;
    }

    private readonly Dictionary<TKey, Entry> _entries;

    /// <summary>
    /// Entries taken out of the table while a lease still held them.
    ///
    /// They are gone as far as lookups are concerned and very much present as
    /// far as memory is concerned, so their bytes keep counting until the last
    /// lease returns. Without this, replacing or clearing a pinned key made the
    /// old allocation vanish from the accounting while the conversion reading it
    /// still had it — the budget would report eight bytes with sixteen alive.
    /// </summary>
    private readonly List<Entry> _retired = new List<Entry>();

    private readonly Func<TValue, int> _bytesOf;
    private long _clock;

    public ByteBudgetCache(long budgetBytes, Func<TValue, int> bytesOf, IEqualityComparer<TKey>? comparer = null)
    {
        if (budgetBytes <= 0) throw new ArgumentOutOfRangeException(nameof(budgetBytes));
        BudgetBytes = budgetBytes;
        _bytesOf = bytesOf ?? throw new ArgumentNullException(nameof(bytesOf));
        _entries = new Dictionary<TKey, Entry>(comparer);
    }

    /// <summary>The cap. Nothing here exceeds it except a single pinned working set, which is reported.</summary>
    public long BudgetBytes { get; }

    /// <summary>
    /// What is held right now: everything in the table, plus anything retired
    /// that a lease has not finished with.
    /// </summary>
    public long Bytes { get; private set; }

    /// <summary>Allocations no longer reachable through the cache and not yet released by their leases.</summary>
    public int Retired => _retired.Count;

    /// <summary>
    /// Make room for <paramref name="bytes"/> by evicting the least recently
    /// used unpinned entries, and say whether it now fits.
    ///
    /// <para>Asked BEFORE a buffer is built or fetched, so that a caller whose
    /// source hands its answer over once — the terrain builder — does not
    /// consume it only to find the cache full. False means everything left is
    /// in use: the caller defers and asks again, and nothing was consumed.</para>
    /// </summary>
    public bool TryReserve(int bytes)
    {
        if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
        if (bytes > BudgetBytes)
            return false;
        MakeRoomFor(bytes);
        return Bytes + bytes <= BudgetBytes;
    }

    /// <summary>
    /// Count a buffer that is in use and NOT in the table — one the cache would
    /// not admit, or one that must not be shared — until its lease returns.
    ///
    /// <para>The gap this closes: a builder result the cache refused was handed
    /// to the caller and used, live, outside the reported budget, so the cache
    /// number was not a bound on working memory. Adopted bytes are retired
    /// bytes with no key: counted, unreachable, dropped on release.</para>
    /// </summary>
    public Lease Adopt(TValue value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var entry = new Entry { Value = value, Bytes = _bytesOf(value), Pins = 1, UsedAt = ++_clock };
        _retired.Add(entry);
        Bytes += entry.Bytes;
        return new Lease(this, default!, entry);
    }

    public int Count => _entries.Count;

    /// <summary>Entries currently in use and therefore not evictable.</summary>
    public int Pinned
    {
        get
        {
            int pinned = 0;
            foreach (KeyValuePair<TKey, Entry> entry in _entries)
            {
                if (entry.Value.Pins > 0)
                    pinned++;
            }
            return pinned;
        }
    }

    /// <summary>The value, or null, without changing what is held.</summary>
    public TValue? Peek(TKey key)
    {
        if (!_entries.TryGetValue(key, out Entry entry))
            return null;
        entry.UsedAt = ++_clock;
        return entry.Value;
    }

    /// <summary>
    /// Put a value in, evicting unpinned entries until it fits.
    ///
    /// <para>A value larger than the whole budget is NOT cached: it is returned
    /// to the caller to use and drop. Admitting it would evict everything else
    /// and still be over, which is a cache that has stopped being one.</para>
    /// </summary>
    /// <returns>True when it was kept; false when it was too large to keep.</returns>
    public bool Put(TKey key, TValue value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        int bytes = _bytesOf(value);
        // The old value under this key is retired, not forgotten: if a lease
        // still holds it, it is still memory and still counts. That is what
        // keeps the replacement honest — with the old one retained, a new one
        // has to fit alongside it or not be admitted at all.
        Remove(key);
        if (bytes > BudgetBytes)
            return false;

        MakeRoomFor(bytes);
        if (Bytes + bytes > BudgetBytes)
            return false;   // everything left is pinned

        _entries[key] = new Entry { Value = value, Bytes = bytes, UsedAt = ++_clock };
        Bytes += bytes;
        return true;
    }

    /// <summary>
    /// Hold an entry against eviction until the lease is returned.
    ///
    /// Null when there is nothing under that key. The lease is idempotent, like
    /// every other one here: a double release would unpin somebody else's use.
    /// </summary>
    public Lease? Pin(TKey key)
    {
        if (!_entries.TryGetValue(key, out Entry entry))
            return null;
        entry.Pins++;
        entry.UsedAt = ++_clock;
        return new Lease(this, key, entry);
    }

    public bool Remove(TKey key)
    {
        if (!_entries.TryGetValue(key, out Entry entry))
            return false;
        _entries.Remove(key);
        Retire(entry);
        return true;
    }

    /// <summary>
    /// Drop everything the cache can reach.
    ///
    /// Anything a lease still holds is retired rather than forgotten: its bytes
    /// keep counting until that lease returns. A teardown that zeroed the
    /// accounting while a conversion was still reading an array would report a
    /// bound it was not keeping — and it is exactly during teardown that
    /// something is most likely to be mid-operation.
    /// </summary>
    public void Clear()
    {
        foreach (KeyValuePair<TKey, Entry> entry in _entries)
            Retire(entry.Value);
        _entries.Clear();
    }

    /// <summary>Out of the table; out of the accounting only once nothing holds it.</summary>
    private void Retire(Entry entry)
    {
        if (entry.Pins > 0)
        {
            _retired.Add(entry);
            return;
        }
        Bytes -= entry.Bytes;
    }

    private void MakeRoomFor(int bytes)
    {
        while (Bytes + bytes > BudgetBytes)
        {
            TKey? oldest = default;
            long oldestUsedAt = long.MaxValue;
            bool found = false;
            foreach (KeyValuePair<TKey, Entry> candidate in _entries)
            {
                // Never the pinned ones: something is reading that right now,
                // and the whole reason this cache exists is that the source can
                // only be asked once.
                if (candidate.Value.Pins > 0 || candidate.Value.UsedAt >= oldestUsedAt)
                    continue;
                oldest = candidate.Key;
                oldestUsedAt = candidate.Value.UsedAt;
                found = true;
            }
            if (!found)
                return;     // everything left is in use
            Remove(oldest!);
        }
    }

    /// <summary>One use of one entry, held against eviction until it is returned.</summary>
    public sealed class Lease : IDisposable
    {
        private readonly ByteBudgetCache<TKey, TValue> _cache;
        private readonly TKey _key;
        private Entry? _entry;

        internal Lease(ByteBudgetCache<TKey, TValue> cache, TKey key, Entry entry)
        {
            _cache = cache;
            _key = key;
            _entry = entry;
        }

        public TValue Value => _entry?.Value ?? throw new ObjectDisposedException(nameof(Lease));

        public void Dispose()
        {
            if (_entry == null)
                return;
            Entry entry = _entry;
            _entry = null;
            entry.Pins--;
            // The last lease on a retired entry is what finally frees its bytes.
            if (entry.Pins == 0 && _cache._retired.Remove(entry))
                _cache.Bytes -= entry.Bytes;
            _ = _key;
        }
    }
}
