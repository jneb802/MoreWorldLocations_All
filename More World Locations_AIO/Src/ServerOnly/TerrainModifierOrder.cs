using System;
using System.Collections.Generic;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// The order vanilla applies a set of terrain modifiers in.
///
/// <para>Its own file, and free of the engine, because two different things
/// need the same answer about the same modifiers and they must not be allowed
/// to disagree: the conversion, which draws the ground, and the content
/// fingerprint, which has to call two orderings of one set of modifiers two
/// different templates. A second copy of this rule would let an approval
/// outlive the ground it was granted for.</para>
///
/// <para><c>Heightmap.ApplyModifiers</c> runs them through
/// <c>TerrainModifier.SortByModifiers</c>: player modifications first, then
/// <c>m_sortOrder</c> ascending, then creation time — and for a location's
/// children, instantiated together, creation time is the order they appear in
/// the template. So a stable sort over the template's own order is the whole
/// rule.</para>
/// </summary>
public static class TerrainModifierOrder
{
    /// <param name="items">In the order they appear in the template.</param>
    /// <param name="playerModificationOf">Vanilla's first key. A location's modifiers are never player ones.</param>
    /// <param name="sortOrderOf">Vanilla's second key.</param>
    public static List<T> Apply<T>(
        IReadOnlyList<T> items, Func<T, bool> playerModificationOf, Func<T, int> sortOrderOf)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (playerModificationOf == null) throw new ArgumentNullException(nameof(playerModificationOf));
        if (sortOrderOf == null) throw new ArgumentNullException(nameof(sortOrderOf));

        // A hand-written stable sort rather than List.Sort, which is not stable:
        // a sort that silently decides its own ties is a selector nobody wrote.
        var indexed = new List<KeyValuePair<int, T>>(items.Count);
        for (int i = 0; i < items.Count; i++)
            indexed.Add(new KeyValuePair<int, T>(i, items[i]));

        indexed.Sort((a, b) =>
        {
            int player = (playerModificationOf(a.Value) ? 1 : 0).CompareTo(playerModificationOf(b.Value) ? 1 : 0);
            if (player != 0)
                return player;
            int order = sortOrderOf(a.Value).CompareTo(sortOrderOf(b.Value));
            return order != 0 ? order : a.Key.CompareTo(b.Key);
        });

        var sorted = new List<T>(indexed.Count);
        foreach (KeyValuePair<int, T> entry in indexed)
            sorted.Add(entry.Value);
        return sorted;
    }
}
