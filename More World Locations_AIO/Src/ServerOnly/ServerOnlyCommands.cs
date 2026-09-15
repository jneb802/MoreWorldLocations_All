using HarmonyLib;
using More_World_Locations_AIO.ServerOnly.Verification;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// The console commands: where the terrain stands, and why a location is or is
/// not in this world.
///
/// It exists because "the structures are in the world" is not the same claim as
/// "the terrain is written", and only the second one can be read off the ledger.
/// A site whose neighbour zone is still waiting looks finished from the middle.
///
/// <para>The catalogue commands exist for one question: "why is
/// <c>MWL_SwampTemple1</c> not in my world". Before them the answer lived in a
/// private station log, which meant the answer did not exist. Now the reasons
/// travel with the build that reached them, with the object path attached, and
/// an operator can get the whole thing out as a table.</para>
///
/// Registered only in server-only mode, and they read rather than do anything,
/// so they are not switches an operator can leave in a bad state.
/// </summary>
[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public static class ServerOnlyCommands
{
    private static bool s_registered;

    private static void Postfix()
    {
        if (!ServerOnlyMode.Enabled || s_registered)
            return;
        s_registered = true;

        new Terminal.ConsoleCommand("mwl_terrain", "Server-only mode: what terrain has been written, what is waiting and what failed",
            (Terminal.ConsoleEventFailable)delegate(Terminal.ConsoleEventArgs args)
            {
                foreach (string line in LocationTerrainWriter.Status().Split('\n'))
                    args.Context.AddString(line);
                return true;
            });

        new Terminal.ConsoleCommand("mwl_catalogue", "Server-only mode: which locations this world serves and why the rest are missing. 'mwl_catalogue export' for every reason as a table",
            (Terminal.ConsoleEventFailable)delegate(Terminal.ConsoleEventArgs args)
            {
                CatalogueReport? report = CatalogueAudit.Report;
                if (report == null)
                {
                    args.Context.AddString("The catalogue has not been swept yet. It runs when the world's locations are set up.");
                    return true;
                }

                bool export = args.Length > 1 && args[1] == "export";
                foreach (string line in (export ? report.Export() : report.Summary()).Split('\n'))
                    args.Context.AddString(line);
                return true;
            });

        new Terminal.ConsoleCommand("mwl_location", "Server-only mode: everything the audit found about one location, by exact name",
            (Terminal.ConsoleEventFailable)delegate(Terminal.ConsoleEventArgs args)
            {
                if (args.Length < 2)
                {
                    args.Context.AddString("mwl_location <exact name>");
                    return true;
                }

                CatalogueReport? report = CatalogueAudit.Report;
                if (report == null)
                {
                    args.Context.AddString("The catalogue has not been swept yet. It runs when the world's locations are set up.");
                    return true;
                }

                foreach (string line in report.Explain(args[1]).Split('\n'))
                    args.Context.AddString(line);
                return true;
            });
    }
}
