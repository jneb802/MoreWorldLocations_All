using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// One template's emission plan as the SERVER holds it, after Jötunn has
/// resolved the mocks.
///
/// <para><b>Why this exists.</b> The acceptance gate compares a placed site
/// against an expected set of children, and that expectation has so far been
/// read out of the asset bundles offline, before resolution. Resolution is
/// exactly where the two can part: a mock is swapped for the real prefab and
/// brings that prefab's own children, components and settings with it. An
/// expectation taken before that is a reasonable first approximation and not
/// evidence. This prints what the server actually resolved, so the two can be
/// reconciled and the offline plan used only where they agree.</para>
///
/// <para>It also prints the resolved root's own GameObject name beside the name
/// it was asked for. Those are not always the same — <c>MWL_MaypoleHut1</c> is
/// asked for and <c>MWL_MayPoleHut1</c> is what the bundle holds — and a
/// coverage count that silently joins the two is counting something it has not
/// checked.</para>
///
/// <para>Diagnostic surface: it opens one template, reads it, and gives the
/// lease straight back, exactly as the sweep does. It decides nothing.</para>
/// </summary>
public static class TemplatePlan
{
    /// <summary>Tab-separated lines: a header, then one row per child.</summary>
    public static IEnumerable<string> Render(string name, int skip = 0, int take = int.MaxValue)
    {
        if (string.IsNullOrEmpty(name))
            return new[] { "mwl_plan <exact name> [skip] [take]" };
        if (!ServerOnlyMode.Enabled)
            return new[] { "mwl_plan is a server-only mode diagnostic; this process is in full mode." };
        if (!TemplateAssets.CanLoad)
            return new[] { "this process has no way to load templates, so there is no plan to read." };

        try
        {
            return Read(name, skip, take);
        }
        catch (Exception ex)
        {
            return new[] { $"reading '{name}' failed: {ex.GetType().Name}: {ex.Message}" };
        }
    }

    private static IEnumerable<string> Read(string name, int skip, int take)
    {
        var lines = new List<string>();
        using (ITemplateHandle? handle = TemplateAssets.Open(name))
        {
            if (handle?.Asset == null)
            {
                lines.Add($"# plan {name} asset=(none) — no asset loaded under this exact name.");
                return lines;
            }

            // The same conversion the audit and the registration path apply, so
            // the plan describes the object that will be placed.
            ScaleSyncConversion.Apply(handle.Asset, TemplateAssets.StockPrefabs);

            TemplateFacts facts = TemplateFactsExtractor.Extract(
                name, "", handle.Asset, stockPrefabOf: TemplateAssets.StockPrefabs);

            int networked = 0;
            int randomisers = 0;
            foreach (ChildFact child in facts.Children)
            {
                if (child.Networked && child.EnabledInHierarchy)
                    networked++;
                foreach (string component in child.Components)
                {
                    if (component == "RandomSpawn" || component == "RandomObject")
                        randomisers++;
                }
            }

            lines.Add($"# plan {name} asset={handle.Asset.name} children={facts.Children.Count} " +
                      $"emitted={networked} randomisers={randomisers} terrain={facts.Terrain.Count}");
            lines.Add("path\tprefab\tnetworked\tenabled\tpos_x\tpos_y\tpos_z\teuler_x\teuler_y\teuler_z\t" +
                      "quat_x\tquat_y\tquat_z\tquat_w\tscale_x\tscale_y\tscale_z\tpersistent\tsync\tstock_sync\tcomponents");

            int index = 0;
            int shown = 0;
            foreach (ChildFact child in facts.Children)
            {
                if (index++ < skip)
                    continue;
                if (shown++ >= take)
                {
                    lines.Add($"# more: {facts.Children.Count - index + 1} row(s) not shown; mwl_plan {name} {index - 1}");
                    break;
                }
                lines.Add(string.Join("\t", new[]
                {
                    child.Path,
                    child.PrefabName,
                    child.Networked ? "1" : "0",
                    child.EnabledInHierarchy ? "1" : "0",
                    N(child.RelativePosition.X), N(child.RelativePosition.Y), N(child.RelativePosition.Z),
                    N(child.EulerAngles.X), N(child.EulerAngles.Y), N(child.EulerAngles.Z),
                    N(child.Rotation.X), N(child.Rotation.Y), N(child.Rotation.Z), N(child.Rotation.W),
                    N(child.Scale.X), N(child.Scale.Y), N(child.Scale.Z),
                    child.Persistent ? "1" : "0",
                    child.SyncInitialScale ? "1" : "0",
                    child.StockSyncInitialScale == null ? "?" : (child.StockSyncInitialScale.Value ? "1" : "0"),
                    string.Join(",", Components(child)),
                }));
            }
        }
        return lines;
    }

    private static string[] Components(ChildFact child)
    {
        var names = new List<string>(child.Components);
        names.Sort(StringComparer.Ordinal);
        return names.ToArray();
    }

    /// <summary>Six decimals, invariant: the gate compares these across machines.</summary>
    private static string N(float value) =>
        (value == 0f ? 0f : value).ToString("0.######", CultureInfo.InvariantCulture);
}
