using System;
using System.Linq;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class Port : MonoBehaviour, Interactable, Hoverable
{
    public int uniqueID;
    public string name;
    public float distance;     // in meters
    public string etaFormatted;         // formatted string, e.g., "3m 42s"
    public int price;
    
    public Vector3 worldPosition;
    public Heightmap.Biome biome;

    public void Awake()
    {
        this.name = "Port";
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (PortUI.portUIRoot == null) {Debug.Log("PortUI_new.portUIRoot is null");}
        PortUI.instance.SetTitle(this.name);
        PortUI.instance.SetupListElements();
        PortUI.portUIRoot.SetActive(true);
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        Debug.Log("added");
        return true;
    }

    public string GetHoverText()
    {
        string hoverText;
            
        hoverText = Localization.instance.Localize(
            $"Port\n" +
            $"[<color=yellow><b>$KEY_Use</b></color>] Open Port\n");
            
        return hoverText;
    }

    public string GetHoverName()
    {
        return Localization.instance.Localize(this.name);
    }
}