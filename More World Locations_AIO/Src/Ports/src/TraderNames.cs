using System.Collections.Generic;

namespace More_World_Locations_AIO;

public static class TraderNames
{
    private static readonly List<string> RandomTraderNames = new()
    {
        new LocalKeys.Key("$trader_port_name_1", "Halmar").GetKey(),
        new LocalKeys.Key("$trader_port_name_2", "Halvar").GetKey(),
        new LocalKeys.Key("$trader_port_name_3", "Halrik").GetKey(),
        new LocalKeys.Key("$trader_port_name_4", "Halgrim").GetKey(),
        new LocalKeys.Key("$trader_port_name_5", "Halden").GetKey(),
        new LocalKeys.Key("$trader_port_name_6", "Halfi").GetKey(),
        new LocalKeys.Key("$trader_port_name_7", "Halvor").GetKey(),
        new LocalKeys.Key("$trader_port_name_8", "Halkar").GetKey()
    };
    
    public static string GetRandomName() => RandomTraderNames[UnityEngine.Random.Range(0, RandomTraderNames.Count)];
}