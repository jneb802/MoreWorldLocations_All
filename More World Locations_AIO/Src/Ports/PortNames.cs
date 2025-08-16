using System.Collections.Generic;
using Jotunn.Entities;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class PortNames
{
    public static CustomLocalization Localization;
    public static List<string> availablePortNames;
    
    public static void AddPortLocalizations()
    {
        Localization = Jotunn.Managers.LocalizationManager.Instance.GetLocalization();

        Localization.AddTranslation("English", new Dictionary<string, string>
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
        });
    }
    
    private static readonly string[] AllTokens = new[]
    {
        "$MWL_PortName_Skjoldhavn",
        "$MWL_PortName_Ravensvik",
        "$MWL_PortName_Drakkarsund",
        "$MWL_PortName_Jotunfjord",
        "$MWL_PortName_Miststrand",
        "$MWL_PortName_Ironvik",
        "$MWL_PortName_Stormhavn",
        "$MWL_PortName_Eirsholm",
        "$MWL_PortName_Njordhavn",
        "$MWL_PortName_Hrafnnes",
        "$MWL_PortName_Vargrvik",
        "$MWL_PortName_Skeldholm",
        "$MWL_PortName_Frostsund",
        "$MWL_PortName_Ulfsfjord",
        "$MWL_PortName_Runavik",
        "$MWL_PortName_Ormsvik",
        "$MWL_PortName_Dyrhavn",
        "$MWL_PortName_Eldersund",
        "$MWL_PortName_Seidrholm",
        "$MWL_PortName_Skaldhavn"
    };
    
    public static void ResetAvailable()
    {
        availablePortNames = new List<string>(AllTokens);
    }
    
    public static string GetRandomPortName()
    {
        if (availablePortNames == null)
            ResetAvailable(); // seed once if not initialized

        if (availablePortNames.Count == 0)
        {
            Debug.LogWarning("[PortNames] No available port names left. Call PortNames.ResetAvailable() to reseed.");
            return null;
        }

        int idx = Random.Range(0, availablePortNames.Count);
        string token = availablePortNames[idx];
        availablePortNames.RemoveAt(idx);
        return token;
    }
}