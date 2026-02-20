using System.IO;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

namespace More_World_Locations_AIO;

public static class MWLLocalizations
{
    private const string LocalizationFileName = "warpalicious.More_World_Locations_Localization";

    public static void Load(PortInit.Toggle useCustom)
    {
        var localization = Jotunn.Managers.LocalizationManager.Instance.GetLocalization();

        // Always load embedded English as the default
        string embeddedContent = AssetUtils.LoadTextFromResources(LocalizationFileName + ".yml");
        localization.AddYamlFile("English", embeddedContent);

        if (useCustom != PortInit.Toggle.On) return;

        // Check if any disk files exist; if not, auto-extract English template
        string[] diskFiles = Directory.GetFiles(Paths.ConfigPath, LocalizationFileName + ".*.yml");
        if (diskFiles.Length == 0)
        {
            string extractPath = Path.Combine(Paths.ConfigPath, LocalizationFileName + ".English.yml");
            try
            {
                File.WriteAllText(extractPath, embeddedContent);
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                    "Auto-extracted " + LocalizationFileName + ".English.yml to BepInEx config folder");
            }
            catch (System.Exception ex)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                    "Failed to extract localization YAML: " + ex.Message);
            }
            return;
        }

        // Load each language file found on disk
        foreach (string file in diskFiles)
        {
            string language = Path.GetFileNameWithoutExtension(file)
                                  .Substring(LocalizationFileName.Length + 1);
            string content = File.ReadAllText(file);
            localization.AddYamlFile(language, content);
        }
    }
}
