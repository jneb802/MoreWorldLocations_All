using System.Collections.Generic;

namespace More_World_Locations_AIO;

public static class TraderNames
{
    private static readonly List<string> RandomTraderNames = new()
    {
        "$trader_port_name_1",
        "$trader_port_name_2",
        "$trader_port_name_3",
        "$trader_port_name_4",
        "$trader_port_name_5",
        "$trader_port_name_6",
        "$trader_port_name_7",
        "$trader_port_name_8",
    };

    public static string GetRandomName() => RandomTraderNames[UnityEngine.Random.Range(0, RandomTraderNames.Count)];
}
