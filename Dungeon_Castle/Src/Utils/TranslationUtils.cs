using System.Collections.Generic;
using Jotunn.Entities;

namespace Dungeon_Castle.Utils;

public class TranslationUtils
{
    public static CustomLocalization Localization;
    
    public static void AddLocalizations()
    {
        Localization = Jotunn.Managers.LocalizationManager.Instance.GetLocalization();
        
        Localization.AddTranslation("English", new Dictionary<string, string>
        {
            {"piece_CD_kit_piece_walltorch", "Wall Torch"}
        });
    }
}