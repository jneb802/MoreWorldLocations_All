using System;
using System.Collections.Generic;
using System.Linq;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>One zone of the world, by its zone coordinates.</summary>
public readonly struct ZoneKey : IEquatable<ZoneKey>
{
    public readonly int X;
    public readonly int Y;

    public ZoneKey(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(ZoneKey other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is ZoneKey other && Equals(other);

    public override int GetHashCode() => unchecked(X * 397 ^ Y);

    public override string ToString() => X + "," + Y;

    public static ZoneKey Parse(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        string[] parts = text.Split(',');
        if (parts.Length != 2
            || !int.TryParse(parts[0], out int x)
            || !int.TryParse(parts[1], out int y))
            throw new FormatException("not a zone key: '" + text + "'");
        return new ZoneKey(x, y);
    }
}

/// <summary>
/// What one site's terrain still owes, across every zone it reaches.
///
/// A site's exterior radius does not respect zone boundaries: one modifier can
/// want vertices in two, three or four compilers, and each of those is written
/// separately and saved separately. So "this site's terrain is done" is not
/// something any single zone can answer, and a bake that asks a zone and gets
/// yes has learned nothing about the other three.
///
/// The ledger holds the planned work as (zone, operation) pairs and calls the
/// site finished only when every pair has been written. That is the plan's
/// invariant that completion is derived from the required outcomes rather than
/// from a queue running dry: an empty queue and a finished site are different
/// facts, and Roads learned the difference the hard way.
///
/// It says nothing about when a write is durable. A pair counts as written when
/// the caller says so, and the caller should say so only once the compiler
/// carrying it has been saved.
/// </summary>
public sealed class SiteTerrainLedger
{
    private readonly SortedSet<string> _required = new SortedSet<string>(StringComparer.Ordinal);
    private readonly SortedSet<string> _written = new SortedSet<string>(StringComparer.Ordinal);

    public string SiteId { get; }

    public SiteTerrainLedger(string siteId)
    {
        if (string.IsNullOrEmpty(siteId))
            throw new ArgumentException("a site needs an identity", nameof(siteId));
        SiteId = siteId;
    }

    private static string Pair(ZoneKey zone, string operationId)
    {
        if (string.IsNullOrEmpty(operationId))
            throw new ArgumentException("an operation needs an identity", nameof(operationId));
        if (operationId.IndexOf('@') >= 0 || operationId.IndexOf('\n') >= 0)
            throw new ArgumentException(
                "an operation identity cannot contain '@' or a newline: the ledger stores pairs as "
                + "'<operation>@<zone>', one per line",
                nameof(operationId));
        return operationId + "@" + zone;
    }

    /// <summary>Declare that this site owes <paramref name="operationId"/> in this zone.</summary>
    public void Require(ZoneKey zone, string operationId) => _required.Add(Pair(zone, operationId));

    /// <summary>
    /// Record that it has been written and saved. Recording something that was
    /// never required is a planning error and throws rather than quietly
    /// enlarging the site's footprint.
    /// </summary>
    public void MarkWritten(ZoneKey zone, string operationId)
    {
        string pair = Pair(zone, operationId);
        if (!_required.Contains(pair))
            throw new InvalidOperationException(
                "'" + pair + "' was written for site '" + SiteId + "' but never planned");
        _written.Add(pair);
    }

    public bool IsWritten(ZoneKey zone, string operationId) => _written.Contains(Pair(zone, operationId));

    /// <summary>
    /// True only when every planned pair has been written. A site with nothing
    /// planned is NOT complete: an empty plan means planning has not run, and
    /// calling that done is how a site with no terrain at all gets a pass.
    /// </summary>
    public bool IsComplete => _required.Count > 0 && _written.Count == _required.Count;

    /// <summary>What is still owed, in a stable order, for a report or a retry.</summary>
    public IEnumerable<string> Outstanding => _required.Where(pair => !_written.Contains(pair));

    public int RequiredCount => _required.Count;

    public int WrittenCount => _written.Count;

    /// <summary>
    /// The ledger as text, to be stored with the world. Two sections, each
    /// sorted, so the same state always produces the same bytes.
    /// </summary>
    public string Serialize() =>
        "site=" + SiteId + "\n"
        + "required=" + string.Join("|", _required) + "\n"
        + "written=" + string.Join("|", _written);

    public static SiteTerrainLedger Deserialize(string serialized)
    {
        if (string.IsNullOrEmpty(serialized))
            throw new ArgumentException("nothing to read", nameof(serialized));

        string? siteId = null;
        List<string> required = new List<string>();
        List<string> written = new List<string>();

        foreach (string line in serialized.Split('\n'))
        {
            int split = line.IndexOf('=');
            if (split < 0)
                throw new FormatException("not a ledger line: '" + line + "'");
            string key = line.Substring(0, split);
            string value = line.Substring(split + 1);

            switch (key)
            {
                case "site": siteId = value; break;
                case "required": required.AddRange(Split(value)); break;
                case "written": written.AddRange(Split(value)); break;
                default: throw new FormatException("unknown ledger field '" + key + "'");
            }
        }

        if (string.IsNullOrEmpty(siteId))
            throw new FormatException("a ledger without a site identity cannot be read");

        SiteTerrainLedger ledger = new SiteTerrainLedger(siteId!);
        foreach (string pair in required) ledger._required.Add(pair);
        foreach (string pair in written)
        {
            if (!ledger._required.Contains(pair))
                throw new FormatException(
                    "saved ledger for '" + siteId + "' records '" + pair + "' as written but never required");
            ledger._written.Add(pair);
        }
        return ledger;
    }

    private static IEnumerable<string> Split(string value) =>
        string.IsNullOrEmpty(value)
            ? Enumerable.Empty<string>()
            : value.Split('|').Where(part => part.Length > 0);
}
