using System.Collections.Generic;
using Jotunn.Entities;

namespace More_World_Traders.Utils;

public class TranslationUtils
{
    public static CustomLocalization Localization;

    public static void AddLocalizations()
    {
        Localization = Jotunn.Managers.LocalizationManager.Instance.GetLocalization();
    
        Localization.AddTranslation("English", new Dictionary<string, string>
        {
            {"$se_blacksmithStone", "Blacksmith Stone"},
            {"$item_mwl_blacksmithstone", "Blacksmith Stone"},
            {"$item_mwl_blacksmithstone_description", "Upgrades an armor or weapon past max quality. Place an armor or weapon in top left cell in inventory and consume this item."},
        });
    }

}