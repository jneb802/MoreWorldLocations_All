using HarmonyLib;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// One console command: where the terrain stands.
///
/// It exists because "the structures are in the world" is not the same claim as
/// "the terrain is written", and only the second one can be read off the ledger.
/// A site whose neighbour zone is still waiting looks finished from the middle.
///
/// Registered only in server-only mode, and it reads rather than does anything,
/// so it is not a switch an operator can leave in a bad state.
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
    }
}
