using System.Collections.Generic;
using Newtonsoft.Json;

namespace More_World_Locations_AIO;

public static class FavoritePorts
{
    private const string CustomDataKey = "MWL_FavoritePorts";

    public static HashSet<string> GetFavoritePorts(this Player player)
    {
        if (!player.m_customData.TryGetValue(CustomDataKey, out string json)) return new HashSet<string>();
        try
        {
            return JsonConvert.DeserializeObject<HashSet<string>>(json) ?? new HashSet<string>();
        }
        catch (JsonException)
        {
            return new HashSet<string>();
        }
    }

    public static bool ToggleFavoritePort(this Player player, ShipmentManager.PortID port)
    {
        HashSet<string> favorites = player.GetFavoritePorts();
        bool favorite = !favorites.Remove(port.GUID);
        if (favorite) favorites.Add(port.GUID);
        player.m_customData[CustomDataKey] = JsonConvert.SerializeObject(favorites);
        return favorite;
    }
}
