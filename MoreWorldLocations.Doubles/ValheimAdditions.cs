// MWL's additions to the Valheim doubles from Valheim.Testing.Doubles. Global namespace, like the game types they extend.
#nullable enable
// ReSharper disable InconsistentNaming

/// <summary>
/// The few game constants the shims have to agree on. Valheim puts the water surface at y = 30; the package names it
/// WorldGenerator.WaterLevel, and MWL's synthetic worlds read it from here.
/// </summary>
public static class ShimWorld
{
    public const float SeaLevel = WorldGenerator.WaterLevel;
}
