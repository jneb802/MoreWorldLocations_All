using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class PortDB
{
    static PortDB? _instance;
    
    public Dictionary<string, Port> allPorts = new Dictionary<string, Port>();
    public Dictionary<string, PortData> allPortData = new Dictionary<string, PortData>();
    
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

    [System.Serializable]
    public class PortData
    {
        public string m_portID;
        public string m_localizationKey;
        public Vector3 m_worldPosition;

        public PortData() {}
        
        public PortData(string portID, string localizationKey, Vector3 worldPosition)
        {
            m_portID = portID;
            m_localizationKey = localizationKey;
            m_worldPosition = worldPosition;
        }
        
    }
}