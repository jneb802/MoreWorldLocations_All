namespace More_World_Locations_AIO;

/// <summary>
/// Shim for the mod's location record. The real type carries a Jotunn
/// <c>LocationConfig</c> and an asset path, which need the game's assemblies;
/// the allowlist reads only the name, so that is all the harness stands in for.
/// </summary>
public class MWLLocation
{
    public string Name { get; set; } = "";
}
