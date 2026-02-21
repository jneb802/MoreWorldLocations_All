using System.Linq;

namespace More_World_Locations_AIO.Utils;

public static class UpgradeWorldCommands
{
    private static string Names(params MWLLocation[][] packs)
    {
        return string.Join(",", packs.SelectMany(p => p).Select(l => l.Name));
    }

    public static void AddUpgradeWorldCommands()
    {
        // All locations command
        UpgradeWorld.Upgrade.Register("mwl_allbiomes", "All MWL locations to the world.",
            "locations_add " + Names(
                LocationDefinitions.Meadows,
                LocationDefinitions.BlackForest,
                LocationDefinitions.Swamp,
                LocationDefinitions.Mountains,
                LocationDefinitions.Plains,
                LocationDefinitions.Mistlands,
                LocationDefinitions.Ashlands,
                LocationDefinitions.Traders, LocationDefinitions.Trainers
            ) + " start");

        // Meadows biome command
        UpgradeWorld.Upgrade.Register("mwl_meadows", "Adds MWL locations to the Meadows biome.",
            "locations_add " + Names(
                LocationDefinitions.Meadows
            ) + " start");

        // Black Forest biome command
        UpgradeWorld.Upgrade.Register("mwl_blackforest", "Adds MWL locations to the Black Forest biome.",
            "locations_add " + Names(
                LocationDefinitions.BlackForest
            ) + " start");

        // Swamp biome command
        UpgradeWorld.Upgrade.Register("mwl_swamp", "Adds MWL locations to the Swamp biome.",
            "locations_add " + Names(
                LocationDefinitions.Swamp
            ) + " start");

        // Mountains biome command
        UpgradeWorld.Upgrade.Register("mwl_mountains", "Adds MWL locations to the Mountains biome.",
            "locations_add " + Names(
                LocationDefinitions.Mountains
            ) + " start");

        // Plains biome command
        UpgradeWorld.Upgrade.Register("mwl_plains", "Adds MWL locations to the Plains biome.",
            "locations_add " + Names(LocationDefinitions.Plains) + " start");

        // Mistlands biome command
        UpgradeWorld.Upgrade.Register("mwl_mistlands", "Adds MWL locations to the Mistlands biome.",
            "locations_add " + Names(
                LocationDefinitions.Mistlands
            ) + " start");

        // Ashlands biome command
        UpgradeWorld.Upgrade.Register("mwl_ashlands", "Adds MWL locations to the Ashlands biome.",
            "locations_add " + Names(LocationDefinitions.Ashlands) + " start");

        // Ports command
        UpgradeWorld.Upgrade.Register("mwl_ports", "Adds MWL shipping port locations to corresponding biomes.",
            "locations_add " + Names(LocationDefinitions.Ports) + " start");

        // Traders command
        UpgradeWorld.Upgrade.Register("mwl_traders", "Adds MWL trader locations to corresponding biomes.",
            "locations_add " + Names(LocationDefinitions.Traders, LocationDefinitions.Trainers) + " start");
    }
}
