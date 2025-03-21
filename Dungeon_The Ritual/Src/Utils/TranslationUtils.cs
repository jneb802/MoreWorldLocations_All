using System.Collections.Generic;
using Jotunn.Entities;

namespace Underground_Ruins;

public class TranslationUtils
{
    public static CustomLocalization Localization;
    
    public static void AddLocalizations()
    {
        Localization = Jotunn.Managers.LocalizationManager.Instance.GetLocalization();
        
        Localization.AddTranslation("English", new Dictionary<string, string>
        {
            {"enemy_Thornskar", "Thornskar"}
        });
    }
}