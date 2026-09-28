using System;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Make an authored scale reach a client that does not hold the template.
///
/// <para><b>The defect.</b> A template can place a stock prefab at a scale the
/// author chose — a roof piece stretched to [1, 1, 1.05] to close a seam, a
/// dvergr chest at 1.2, a greydwarf root at 0.8. The server builds it at that
/// scale. A client without the mod builds the stock prefab from the ZDO, and a
/// ZDO carries a scale only when the object's <c>ZNetView.m_syncInitialScale</c>
/// says so. Left false, the client builds the object at 1 and the two disagree
/// about the size of something solid: the seam the author closed is open again,
/// and the chest is the wrong size to walk around.</para>
///
/// <para><b>The fix, and what it is not.</b> Setting the flag makes the server
/// write the scale it already has into the ZDO. Nothing about the authored
/// build changes: no child is removed, nothing is substituted, no geometry
/// moves. The only difference is that a fact the server holds is transmitted.
/// It is a conversion in the same sense as the terrain conversion — what the
/// server does so that a stock client sees the site the author built.</para>
///
/// <para><b>Only where it is read.</b> Sending is half of the transfer.
/// <c>ZNetView.Awake</c> reads the ZDO's scale inside the RECEIVING object's own
/// <c>if (m_syncInitialScale)</c>, and on a client without the mod that object
/// is the stock prefab. A stock prefab with the flag off ignores the scale
/// however the server sends it, so converting there would write a field nobody
/// reads and — worse — let the audit approve a size the client never builds.
/// The stock prefab decides: no lookup, no conversion.</para>
///
/// <para><b>Both halves or neither.</b> The audit judges a template that has
/// been through this, because that is what will be placed; the registration
/// path applies the same conversion to the asset the location is built from.
/// If only the first were true the audit would approve a scale nobody sends,
/// which is the one failure a checker must never have. The two call sites are
/// <see cref="Verification.CatalogueSweep"/>'s read and
/// <c>Common.LocationManager.AddLocation</c>; the count this returns is logged
/// by the second, so a run can show the placement path ran.</para>
///
/// <para>Applying it twice changes nothing: an object whose flag is already set
/// is skipped, which is what lets the audit and the registration both call it
/// on the same asset.</para>
/// </summary>
public static class ScaleSyncConversion
{
    /// <summary>
    /// Set <c>m_syncInitialScale</c> on every emitted object the template scales
    /// away from the stock prefab's own scale, and whose stock prefab reads a
    /// sent scale. Returns how many were changed; zero for a template that needs
    /// nothing, which is most of them.
    /// </summary>
    /// <param name="stockPrefabOf">
    /// The stock prefab by name — the object the client actually builds. Without
    /// it nothing can be said about what the client would do with a sent scale,
    /// and nothing is converted.
    /// </param>
    public static int Apply(GameObject? template, Func<string, GameObject?>? stockPrefabOf)
    {
        if (template == null || stockPrefabOf == null)
            return 0;
        int changed = 0;
        Walk(template, stockPrefabOf, ref changed);
        return changed;
    }

    /// <summary>
    /// The registration path's half, as Jötunn hands the resolved template over.
    /// Full mode does not consult it: a client that has the mod builds the
    /// template itself and already has the author's scale.
    /// </summary>
    public static void OnResolved(string locationName, object? resolved)
    {
        if (!ServerOnlyMode.Enabled)
            return;
        int changed = Apply(resolved as GameObject, TemplateAssets.StockPrefabs);
        if (changed == 0)
            return;
        try
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                $"scale sync: {changed} object(s) in '{locationName}' send their authored scale to a stock client.");
        }
        catch
        {
            // A log sink that is not there must not stop the conversion that
            // already happened from being the thing that ships.
        }
    }

    private static void Walk(GameObject go, Func<string, GameObject?> stockPrefabOf, ref int changed)
    {
        // The same question the policy's scale rule asks, deliberately: an
        // object it would flag for a scale we can actually deliver is an object
        // this converts, and nothing else is touched. A view that is disabled
        // emits nothing, so there is nothing to send and nothing to fix.
        ZNetView view = go.GetComponent<ZNetView>();
        if (view != null && view.enabled && !view.m_syncInitialScale && Deliverable(go, stockPrefabOf))
        {
            view.m_syncInitialScale = true;
            changed++;
        }

        for (int i = 0; i < go.transform.childCount; i++)
            Walk(go.transform.GetChild(i).gameObject, stockPrefabOf, ref changed);
    }

    /// <summary>
    /// Whether sending this object's scale would change what the client builds:
    /// the stock prefab of that name exists, reads a sent scale, and starts at a
    /// different size from the one the template asks for.
    /// </summary>
    private static bool Deliverable(GameObject go, Func<string, GameObject?> stockPrefabOf)
    {
        GameObject? stock;
        try
        {
            stock = stockPrefabOf(go.name);
        }
        catch
        {
            return false;   // no baseline, no claim, no conversion
        }
        if (stock == null)
            return false;

        ZNetView stockView = stock.GetComponent<ZNetView>();
        if (stockView == null || !stockView.m_syncInitialScale)
            return false;

        Vector3 mine = go.transform.localScale;
        Vector3 theirs = stock.transform.localScale;
        return System.Math.Abs(mine.x - theirs.x) >= 1e-4f
            || System.Math.Abs(mine.y - theirs.y) >= 1e-4f
            || System.Math.Abs(mine.z - theirs.z) >= 1e-4f;
    }
}
