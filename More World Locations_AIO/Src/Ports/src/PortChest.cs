using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO;

// A stable token links a port slot to its chest across world saves. Valheim can
// renumber ZDO IDs when loading a world, so raw IDs cannot be saved as this link.
public class PortChest : MonoBehaviour
{
    public const string TokenKey = "MWL_PortChestToken";
    private static readonly Dictionary<string, Container> Loaded = new();
    private string token = "";
    private Container? container;

    private void Awake()
    {
        ZNetView view = GetComponent<ZNetView>();
        if (!view || !view.IsValid()) return;
        token = view.GetZDO().GetString(TokenKey);
        container = GetComponent<Container>();
        if (!string.IsNullOrEmpty(token) && container) Loaded[token] = container;
    }

    private void OnDestroy()
    {
        // Scene unloading only removes the local lookup. Never delete network data.
        if (Loaded.TryGetValue(token, out Container current) && current == container) Loaded.Remove(token);
    }

    public static Container? Find(string token) => Loaded.TryGetValue(token, out Container container) ? container : null;
}
