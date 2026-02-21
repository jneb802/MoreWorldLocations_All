using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace More_World_Locations_AIO;

[Obsolete]
public static class PortNames
{
    private static readonly Dictionary<string, string> Tokens = new Dictionary<string, string>
    {
        { "$MWL_PortName_Skjoldhavn", "Port of Skjoldhavn" },
        { "$MWL_PortName_Ravensvik", "Ravensvik Port" },
        { "$MWL_PortName_Drakkarsund", "Drakkarsund" },
        { "$MWL_PortName_Jotunfjord", "Port of Jotunfjord" },
        { "$MWL_PortName_Miststrand", "Miststrand Port" },
        { "$MWL_PortName_Ironvik", "Ironvik" },
        { "$MWL_PortName_Stormhavn", "Port of Stormhavn" },
        { "$MWL_PortName_Eirsholm", "Eirsholm Port" },
        { "$MWL_PortName_Njordhavn", "Njordhavn" },
        { "$MWL_PortName_Hrafnnes", "Port of Hrafnnes" },
        { "$MWL_PortName_Vargrvik", "Vargrvik Port" },
        { "$MWL_PortName_Skeldholm", "Skeldholm" },
        { "$MWL_PortName_Frostsund", "Port of Frostsund" },
        { "$MWL_PortName_Ulfsfjord", "Ulfsfjord Port" },
        { "$MWL_PortName_Runavik", "Runavik" },
        { "$MWL_PortName_Ormsvik", "Port of Ormsvik" },
        { "$MWL_PortName_Dyrhavn", "Dyrhavn Port" },
        { "$MWL_PortName_Eldersund", "Eldersund" },
        { "$MWL_PortName_Seidrholm", "Port of Seidrholm" },
        { "$MWL_PortName_Skaldhavn", "Skaldhavn Port" }
    };
    
    private static readonly List<string> UsedTokens = new();
    
    public static void Setup()
    {
    }
    public static string GetRandomName()
    {
        // right, this does not work because it happens locally
        if (UsedTokens.Count >= Tokens.Count)
        {
            UsedTokens.Clear();
        }
    
        List<string> availableTokens = Tokens.Keys.Where(t => !UsedTokens.Contains(t)).ToList();
        if (availableTokens.Count == 0)
        {
            return "Port";
        }
        
        string token = availableTokens[UnityEngine.Random.Range(0, availableTokens.Count)];
        UsedTokens.Add(token);
        // ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, nameof(RPC_RegisterUsedToken), token);
        return token;
    }
    
    // [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    // private static class RegisterUsedTokenRPC
    // {
    //     [UsedImplicitly]
    //     private static void Postfix()
    //     {
    //         ZRoutedRpc.instance.Register<string>(nameof(RPC_RegisterUsedToken), RPC_RegisterUsedToken);
    //     }
    // }
    //
    // public static void RPC_RegisterUsedToken(long sender, string token)
    // {
    //     Debug.LogWarning("SOMEONE REGISTERED A NEW RANDOM NAME: " + token);
    //     UsedTokens.Add(token);
    // }
}