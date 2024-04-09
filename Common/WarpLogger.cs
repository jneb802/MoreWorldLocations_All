using BepInEx.Logging;

namespace Meadows_Pack_1;

public class WarpLogger
{
    public static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(Meadows_Pack_1Plugin.ModName);
}