using System.Collections.Generic;

namespace More_World_Locations_AIO.Shipments;

public static class PortManager
{
    public static Dictionary<int, Port> ports = new Dictionary<int, Port>();

    public static void BuildPorts()
    {
        ports = new Dictionary<int, Port>
        {
            { 123, new Port { name = "Port A", uniqueID = 123, distance = 1200f, etaFormatted = "2m 10s", price = 1000 } },
            { 456, new Port { name = "Port B", uniqueID = 456, distance = 950f, etaFormatted = "1m 45s", price = 850 } },
            { 101, new Port { name = "Westhaven Dock", uniqueID = 101, distance = 1600f, etaFormatted = "3m 10s", price = 1150 } },
            { 102, new Port { name = "Stonecliff Harbor", uniqueID = 102, distance = 1850f, etaFormatted = "3m 50s", price = 1300 } },
            { 103, new Port { name = "Elder's Landing", uniqueID = 103, distance = 800f, etaFormatted = "1m 20s", price = 700 } },
            { 104, new Port { name = "Frostbay Outpost", uniqueID = 104, distance = 2100f, etaFormatted = "4m 20s", price = 1400 } },
            { 105, new Port { name = "Irondeep Quay", uniqueID = 105, distance = 1450f, etaFormatted = "2m 55s", price = 950 } }
        };
    }
}