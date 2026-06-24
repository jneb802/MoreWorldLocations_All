using UnityEngine;

namespace More_World_Locations_AIO.Dungeons.Packs;

public static class ForbiddenCatacombsPack
{
    private const string Theme = "CD_Catacomb";
    private const string ExteriorLocation = "cd_exterior";
    private const string KitPrefix = "CD_kit_";

    // Strip set for inert build-piece kit clones: Piece runs build-only logic,
    // WearNTear runs stability checks that fail underground, ZNetView is
    // unnecessary because the room ZNetView persists children.
    private const StripFlags InertGeometry = StripFlags.PieceWNTZNV;

    public static readonly MWLDungeonPack Pack = new MWLDungeonPack
    {
        Name = "ForbiddenCatacombs",
        ThemeName = Theme,
        ExteriorLocationName = ExteriorLocation,
        Bundle = () => Prefabs.dungeonCastle,

        KitClones = new[]
        {
            new KitClone { Name = KitPrefix + "stone_arch",       VanillaSource = "stone_arch",       Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "stone_floor",      VanillaSource = "stone_floor",      Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "stone_floor_2x2",  VanillaSource = "stone_floor_2x2",  Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "stone_pillar",     VanillaSource = "stone_pillar",     Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "stone_stair",      VanillaSource = "stone_stair",      Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "stone_wall_1x1",   VanillaSource = "stone_wall_1x1",   Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "stone_wall_2x1",   VanillaSource = "stone_wall_2x1",   Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "stone_wall_4x2",   VanillaSource = "stone_wall_4x2",   Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "iron_floor_1x1",   VanillaSource = "iron_floor_1x1",   Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "iron_floor_2x2",   VanillaSource = "iron_floor_2x2",   Strip = InertGeometry },
            new KitClone { Name = KitPrefix + "blackmarble_stair", VanillaSource = "blackmarble_stair", Strip = InertGeometry },

            new KitClone
            {
                Name = KitPrefix + "RuneStone_Mistlands_bosshint",
                VanillaSource = "RuneStone_Mistlands_bosshint",
                Configure = ConfigureCatacombsRunestone,
            },
        },

        CustomPrefabs = new[]
        {
            // AltarDoor's MonoBehaviour lives under namespace Forbidden_Catacombs
            // (Src/Dungeons/AltarDoor.cs) so the bundle's serialized binding resolves.
            new CustomPrefabSpec { Name = KitPrefix + "altarHolder", Source = CustomPrefabSource.Bundled },
            new CustomPrefabSpec { Name = KitPrefix + "AltarDoor",   Source = CustomPrefabSource.Bundled },
            new CustomPrefabSpec { Name = KitPrefix + "secretdoor",  Source = CustomPrefabSource.Bundled },
            new CustomPrefabSpec { Name = KitPrefix + "CryptKey",    Source = CustomPrefabSource.Bundled },
        },

        DungeonGeneratorPrefab = new CustomPrefabSpec
        {
            Name = "DG_CatacombDungeon",
            Source = CustomPrefabSource.Bundled,
        },
    };

    // Strip the boss-pin behavior; this clone is reused as a plain lore stone.
    private static void ConfigureCatacombsRunestone(GameObject prefab)
    {
        RuneStone runeStone = prefab.GetComponent<RuneStone>();
        if (runeStone == null)
        {
            Debug.LogWarning($"ForbiddenCatacombsPack: '{prefab.name}' is missing a RuneStone component");
            return;
        }

        runeStone.m_text = "$mwl_forbiddencatacombs_runestone_text";
        runeStone.m_pinName = "";
        runeStone.m_pinType = Minimap.PinType.None;
        runeStone.m_locationName = "";
    }
}
