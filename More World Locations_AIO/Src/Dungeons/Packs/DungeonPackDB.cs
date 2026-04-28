namespace More_World_Locations_AIO.Dungeons.Packs;

public static class DungeonPackDB
{
    public static readonly MWLDungeonPack[] All =
    {
        UndergroundRuinsPack.Pack,
        ForbiddenCatacombsPack.Pack,
    };

    public static void RegisterAll()
    {
        foreach (MWLDungeonPack pack in All)
        {
            DungeonPackRegistrar.Register(pack);
        }
    }
}
