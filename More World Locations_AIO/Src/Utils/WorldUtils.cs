using System;
using More_World_Locations_AIO.Shipments;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace More_World_Locations_AIO.Utils;

public static class WorldUtils
{
    public static Port GetPortInRange(Vector3 center, float distance)
    {
        var list = SceneManager.GetActiveScene().GetRootGameObjects();

        for (int lcv = 0; lcv < list.Length; lcv++)
        {
            var obj = list[lcv];
            Port port = obj.GetComponent<Port>();

            if (port != null && InBounds(center, obj.transform.position, distance))
            {
                return port;
            }
        }

        return null;
    } 
    
    public static LocationProxy GetLocationInRange(Vector3 center, float distance)
    {
        var list = SceneManager.GetActiveScene().GetRootGameObjects();
    
        for (int lcv = 0; lcv < list.Length; lcv++)
        {
            var obj = list[lcv];
            LocationProxy location = obj.GetComponent<LocationProxy>();
    
            if (location != null && InBounds(center, obj.transform.position, distance))
            {
                // Debug.Log($"GetLocationInRange: Found locationProxy with name {location.name}");
                return location;
            }
        }
    
        return null;
    } 
    
    public static bool InBounds(Vector3 center, Vector3 position, float distance)
    {
        var delta = center - position;
        var mag = GetMaximumDistance(delta.x, delta.z);
        return mag <= distance;
    }
    
    public static float GetMaximumDistance(float x, float z)
    {
        return (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(z, 2));
    }
    
    // public static Location GetLocationInRange(Vector3 position, float range)
    // {
    //     Collider[] hits = Physics.OverlapBox(position, Vector3.one * range, Quaternion.identity);
    //
    //     foreach (var hit in hits)
    //     {
    //         var location = hit.transform.root.gameObject.GetComponentInChildren<Location>();
    //         if (location != null)
    //         {
    //             return location;
    //         }
    //     }
    //
    //     return null;
    // }
}