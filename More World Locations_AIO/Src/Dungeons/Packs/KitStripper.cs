using UnityEngine;

namespace More_World_Locations_AIO.Dungeons.Packs;

public static class KitStripper
{
    public static void Strip(GameObject prefab, StripFlags flags)
    {
        if (prefab == null || flags == StripFlags.None) return;

        if ((flags & StripFlags.Piece) != 0)
        {
            foreach (Piece piece in prefab.GetComponentsInChildren<Piece>(true))
            {
                Object.DestroyImmediate(piece);
            }
        }

        if ((flags & StripFlags.WearNTear) != 0)
        {
            foreach (WearNTear wnt in prefab.GetComponentsInChildren<WearNTear>(true))
            {
                Object.DestroyImmediate(wnt);
            }
        }

        if ((flags & StripFlags.StaticPhysics) != 0)
        {
            foreach (StaticPhysics sp in prefab.GetComponentsInChildren<StaticPhysics>(true))
            {
                Object.DestroyImmediate(sp);
            }
        }

        // ZNetView last: removing it earlier would invalidate references some
        // of the other components hold during their own destruction.
        if ((flags & StripFlags.ZNetView) != 0)
        {
            foreach (ZNetView zv in prefab.GetComponentsInChildren<ZNetView>(true))
            {
                Object.DestroyImmediate(zv);
            }
        }
    }
}
