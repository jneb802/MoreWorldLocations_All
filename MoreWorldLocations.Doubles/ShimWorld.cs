/// <summary>
/// The few game constants the shims have to agree on. Valheim puts the water
/// surface at y = 30; everything else in the harness is expressed against it.
/// Global namespace, like the game types the shims stand in for.
/// </summary>
public static class ShimWorld
{
    public const float SeaLevel = 30f;
}
