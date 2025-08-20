using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class PortDB
{
    static PortDB? _instance;
    
    public Dictionary<string, Port> allPorts = new Dictionary<string, Port>();
    
    public static PortDB Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PortDB();
            }
            return _instance;
        }
    }
    
    public Port GetPort(string portId)
    {
        if (allPorts.ContainsKey(portId))
        {
            return allPorts[portId];
        }

        Debug.Log($"PortManager.GetPort: Failed to get port: {portId} from PortManager.allPorts");
        return null;
    }
}