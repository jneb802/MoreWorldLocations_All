using System.Collections.Generic;

namespace More_World_Locations_AIO.Shrines;

public class ShrineConfig
{
    public string internalName;
    public string displayName;
    public StatusEffect statusEffect;
    public Shrine.ShrineOffering shrineOffering;
    
    public ShrineConfig(string internalName, string displayName, StatusEffect statusEffect, Shrine.ShrineOffering shrineOffering)
    {
        this.internalName = internalName;
        this.displayName = displayName;
        this.statusEffect = statusEffect;
        this.shrineOffering = shrineOffering;
    }
    
    public Shrine.ShrineOffering GetShrineOffering()
    {
        return shrineOffering;
    }
    
}