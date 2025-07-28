using System.Text;

namespace More_World_Locations_AIO.Waystones;

public static class WaystoneUtils
{
    public static string GetWaystoneDetails(WaystoneConfig waystoneConfig)
    {
        if (waystoneConfig == null) return "";

        var sb = new StringBuilder();

        // Check if location name is set and add location marking text
        if (!string.IsNullOrEmpty(waystoneConfig.locationDisplayName))
        {
            sb.AppendLine($"Marks a {waystoneConfig.locationDisplayName} on map");
        }

        // Check if vegetation name is set and add vegetation marking text
        if (!string.IsNullOrEmpty(waystoneConfig.vegetationDisplayName))
        {
            sb.AppendLine($"Marks a {waystoneConfig.vegetationDisplayName} on map");
        }

        // Check if reveal radius is set and add radius reveal text
        if (waystoneConfig.mapRevealRadius > 0)
        {
            sb.AppendLine($"Reveal a radius of {waystoneConfig.mapRevealRadius} on map");
        }

        return sb.ToString().TrimEnd();
    }
}