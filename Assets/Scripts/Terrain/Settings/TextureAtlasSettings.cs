using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public struct TextureAtlasSettings
{
    public const float tUnit = 0.03125f;

    public enum Dir { North, South, East, West, Up, Down };

    public enum ID : ushort   // updated 20th November 2018
    {
        // Atlas row Y0
        ERRORFALLBACK,
        AIR,
        DESTROYSTAGE1,
        DESTROYSTAGE2,
        DESTROYSTAGE3,
        DESTROYSTAGE4,
        DESTROYSTAGE5,
        DESTROYSTAGE6,
        DESTROYSTAGE7,
        DESTROYSTAGE8,
        DESTROYSTAGE9,
        DESTROYSTAGE10,
        BEDROCK,
        BASALT,
        GRANITE,
        GABBRO,
        OBSIDIAN,
        COAL,
        CLAY,
        SLATE,
        GRAVEL,
        SAND,
        SANDSTONE,
        QUARTZITE,
        LIMESTONE,
        MARBLE,
        MUD,
        MUDSTONE,
        GNEISS,
        SHALE,
        GRASS,
        HALITE,

        // Atlas row Y1
        LAVA_STILL,
        LAVA_FLOWING,
        WATER_STILL,
        WATER_FLOWING,
        NITRE,
        GYPSUM,
        ANDESITE,
        RYOLITE,
        DIORITE,
        PERIDOTITE,
        PUMICE,
        CHALK,
        SILTSTONE,
        CLAYSTONE,
        SCHIST,







        SPERRYLITE,
        URANINITE,
        BERYL,
        CASSITERITE,
        COBALTITE,
        MOLYBDENITE,
        MILLERITE,
        POLLUCITE,
        PENTLANDITE,
        PYRITE,

        // Atlas row Y2
        BAUXITE,
        CHALCOCITE,
        GOLD,
        HEMATITE,
        ACANTHITE,
        CHROMITE,
        CINNABAR,
        GALENA,
        SPHALERITE,
        ILMENITE,
        MAGNETITE,
        PYROLUSITE,
        SCHEELITE,
        JASPER_GRANITE,
        JASPER_GNEISS,
        AGATE,
        MOONSTONE_GRANITE,
        MOONSTONE_GNEISS,
        ONYX,
        OPAL,
        ALMANDINE_GRANITE,
        ALMANDINE_GNEISS,
        LABRADORITE,
        AQUAMARINE,
        TURQUOISE_SANDSTONE,
        TURQUOISE_QUARTZITE,
        LARIMAR,
        CITRINE_GRANITE,
        CITRINE_GNEISS,
        GOLDENBERYL,
        GROSSULAR_GRANITE,
        GROSSULAR_GNEISS,

        // Atlas row Y3
        EMERALD,
        PERIDOT_GRANITE,
        PERIDOT_GNEISS,
        JADE,
        RUBY_GRANITE,
        RUBY_MARBLE,
        CARNELIAN,
        PYROPE_GRANITE,
        PYROPE_GNEISS,
        TOPAZ_GRANITE,
        TOPAZ_RYOLITE,
        SUNSTONE_GRANITE,
        SUNSTONE_GNEISS,


        AMETHYST_GRANITE,
        AMETHYST_GNEISS,
        MORGANITE,
        RHODOLITE_GRANITE,
        RHODOLITE_GNEISS,
        LAPISLAZULI_MARBLE,
        LAPISLAZULI_QUARTZITE,
        SAPPHIRE_GRANITE,
        SAPPHIRE_MARBLE,
        SODALITE_GRANITE,
        SODALITE_MARBLE,
        SODALITE_BASALT,
        TIGERSEYE,
        BLOODSTONE,
        ROCKCRYSTAL_GRANITE,
        ROCKCRYSTAL_GNEISS,
        DIAMOND,

        // Atlas row Y4
        OAK_WOOD,
        OAK_LEAVES,
        BIRCH_WOOD,
        BIRCH_LEAVES,
        DARKOAK_WOOD,
        DARKOAK_LEAVES,
        SACREDOAK_WOOD,
        SACREDOAK_LEAVES,
        SPRUCE_WOOD,
        SPRUCE_LEAVES,
        JUNGLE_WOOD,
        JUNGLE_LEAVES,
        ACACIA_WOOD,
        ACACIA_LEAVES,
        WILLOW_WOOD,
        WILLOW_LEAVES,
        UMBRAN_WOOD,
        UMBRAN_LEAVES,
        REDWOOD_WOOD,
        REDWOOD_LEAVES,
        PINE_WOOD,

        // Atlas row Y5
        PINE_LEAVES,
        PALM_WOOD,
        PALM_LEAVES,
        MANGROVE_WOOD,
        MANGROVE_LEAVES,
        MAHOGANY_WOOD,
        MAHOGANY_LEAVES,
        MAGICAL_WOOD,
        MAGICAL_LEAVES,
        JACARANDA_WOOD,
        JACARANDA_LEAVES,
        FIR_WOOD,
        FIR_LEAVES,
        EUCALYPTUS_WOOD,
        EUCALYPTUS_LEAVES,
        EBONY_WOOD,
        EBONY_LEAVES,
        HELLBARK_WOOD,
        HELLBARK_LEAVES,
        ETHEREAL_WOOD,
        ETHEREAL_LEAVES,
        CHERRY_WOOD,

        // Atlas row Y6
        CHERRY_LEAVES_WHITE,
        CHERRY_LEAVES_PINK,
        DEAD_WOOD,
        DEAD_LEAVES,
    }

    public Vector2 GetTextureCoord (ID id, Dir dir) // updated 20th nov 2018
    {
        switch (id)
        {
            case ID.ERRORFALLBACK: return ErrorFallback;
            case ID.AIR: return Air;
            case ID.DESTROYSTAGE1: return DestroyStage1;
            case ID.DESTROYSTAGE2: return DestroyStage2;
            case ID.DESTROYSTAGE3: return DestroyStage3;
            case ID.DESTROYSTAGE4: return DestroyStage4;
            case ID.DESTROYSTAGE5: return DestroyStage5;
            case ID.DESTROYSTAGE6: return DestroyStage6;
            case ID.DESTROYSTAGE7: return DestroyStage7;
            case ID.DESTROYSTAGE8: return DestroyStage8;
            case ID.DESTROYSTAGE9: return DestroyStage9;
            case ID.DESTROYSTAGE10: return DestroyStage10;
            case ID.BEDROCK: return Bedrock;
            case ID.BASALT: return Basalt;
            case ID.GRANITE: return Granite;
            case ID.GABBRO: return Gabbro;
            case ID.OBSIDIAN: return Obsidian;
            case ID.COAL: return Coal;
            case ID.CLAY: return Clay;
            case ID.SLATE: return Slate;
            case ID.GRAVEL: return Gravel;
            case ID.SAND: return Sand;
            case ID.SANDSTONE: return Sandstone;
            case ID.QUARTZITE: return Quartzite;
            case ID.LIMESTONE: return Limestone;
            case ID.MARBLE: return Marble;
            case ID.MUD: return Mud;
            case ID.MUDSTONE: return Mudstone;
            case ID.GNEISS: return Gneiss;
            case ID.SHALE: return Shale;
            case ID.GRASS:
                if (dir == Dir.Up) { return Grass_Top; }
                else if (dir == Dir.Down) { return Mud; }
                else { return Grass_Side; }
            case ID.HALITE: return Halite;

            // Atlas row Y1
            case ID.LAVA_STILL: return Lava_Still;
            case ID.LAVA_FLOWING: return Lava_Flowing;
            case ID.WATER_STILL: return Water_Still;
            case ID.WATER_FLOWING: return Water_Flowing;
            case ID.NITRE: return Nitre;
            case ID.GYPSUM: return Gypsum;
            case ID.ANDESITE: return Andesite;
            case ID.RYOLITE: return Ryolite;
            case ID.DIORITE: return Diorite;
            case ID.PERIDOTITE: return Peridotite;
            case ID.PUMICE: return Pumice;
            case ID.CHALK: return Chalk;
            case ID.SILTSTONE: return Siltstone;
            case ID.CLAYSTONE: return Claystone;
            case ID.SCHIST: return Schist;

            // several spaces available here on atlas





            case ID.SPERRYLITE: return Sperrylite;
            case ID.URANINITE: return Uraninite;
            case ID.BERYL: return Beryl;
            case ID.CASSITERITE: return Cassiterite;
            case ID.COBALTITE: return Cobaltite;
            case ID.MOLYBDENITE: return Molybdenite;
            case ID.MILLERITE: return Millerite;
            case ID.POLLUCITE: return Pollucite;
            case ID.PENTLANDITE: return Pentlandite;
            case ID.PYRITE: return Pyrite;

            // Atlas row Y2
            case ID.BAUXITE: return Bauxite;
            case ID.CHALCOCITE: return Chalcocite;
            case ID.GOLD: return Gold;
            case ID.HEMATITE: return Hematite;
            case ID.ACANTHITE: return Acanthite;
            case ID.CHROMITE: return Chromite;
            case ID.CINNABAR: return Cinnabar;
            case ID.GALENA: return Galena;
            case ID.SPHALERITE: return Sphalerite;
            case ID.ILMENITE: return Ilmenite;
            case ID.MAGNETITE: return Magnetite;
            case ID.PYROLUSITE: return Pyrolusite;
            case ID.SCHEELITE: return Scheelite;
            case ID.JASPER_GRANITE: return Jasper_Granite;
            case ID.JASPER_GNEISS: return Jasper_Gneiss;
            case ID.AGATE: return Agate;
            case ID.MOONSTONE_GRANITE: return Moonstone_Granite;
            case ID.MOONSTONE_GNEISS: return Moonstone_Gneiss;
            case ID.ONYX: return Onyx;
            case ID.OPAL: return Opal;
            case ID.ALMANDINE_GRANITE: return Almandine_Granite;
            case ID.ALMANDINE_GNEISS: return Almandine_Gneiss;
            case ID.LABRADORITE: return Labradorite;
            case ID.AQUAMARINE: return Aquamarine;
            case ID.TURQUOISE_SANDSTONE: return Turquoise_Sandstone;
            case ID.TURQUOISE_QUARTZITE: return Turquoise_Quartzite;
            case ID.LARIMAR: return Larimar;
            case ID.CITRINE_GRANITE: return Citrine_Granite;
            case ID.CITRINE_GNEISS: return Citrine_Gneiss;
            case ID.GOLDENBERYL: return GoldenBeryl;
            case ID.GROSSULAR_GRANITE: return Grossular_Granite;
            case ID.GROSSULAR_GNEISS: return Grossular_Gneiss;

            // Atlas row Y3
            case ID.EMERALD: return Emerald;
            case ID.PERIDOT_GRANITE: return Peridot_Granite;
            case ID.PERIDOT_GNEISS: return Peridot_Gneiss;
            case ID.JADE: return Jade;
            case ID.RUBY_GRANITE: return Ruby_Granite;
            case ID.RUBY_MARBLE: return Ruby_Marble;
            case ID.CARNELIAN: return Carnelian;
            case ID.PYROPE_GRANITE: return Pyrope_Granite;
            case ID.PYROPE_GNEISS: return Pyrope_Gneiss;
            case ID.TOPAZ_GRANITE: return Topaz_Granite;
            case ID.TOPAZ_RYOLITE: return Topaz_Ryolite;
            case ID.SUNSTONE_GRANITE: return Sunstone_Granite;
            case ID.SUNSTONE_GNEISS: return Sunstone_Gneiss;

            // space for 2 on atlas here

            case ID.AMETHYST_GRANITE: return Amethyst_Granite;
            case ID.AMETHYST_GNEISS: return Amethyst_Gneiss;
            case ID.MORGANITE: return Morganite;
            case ID.RHODOLITE_GRANITE: return Rhodolite_Granite;
            case ID.RHODOLITE_GNEISS: return Rhodolite_Gneiss;
            case ID.LAPISLAZULI_MARBLE: return LapisLazuli_Marble;
            case ID.LAPISLAZULI_QUARTZITE: return LapisLazuli_Quartzite;
            case ID.SAPPHIRE_GRANITE: return Sapphite_Granite;
            case ID.SAPPHIRE_MARBLE: return Sapphire_Marble;
            case ID.SODALITE_GRANITE: return Sodalite_Granite;
            case ID.SODALITE_MARBLE: return Sodalite_Marble;
            case ID.SODALITE_BASALT: return Sodalite_Basalt;
            case ID.TIGERSEYE: return TigersEye;
            case ID.BLOODSTONE: return Bloodstone;
            case ID.ROCKCRYSTAL_GRANITE: return RockCrystal_Granite;
            case ID.ROCKCRYSTAL_GNEISS: return RockCrystal_Gneiss;
            case ID.DIAMOND: return Diamond;

            // Atlas row Y4
            case ID.OAK_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Oak_Wood_Top; }
                else { return Oak_Wood_Side; }
            case ID.OAK_LEAVES: return Oak_Leaves;

            case ID.BIRCH_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Birch_Wood_Top; }
                else { return Birch_Wood_Side; }
            case ID.BIRCH_LEAVES: return Birch_Leaves;

            case ID.DARKOAK_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Oak_Wood_Top; }
                else { return Oak_Wood_Side; }
            case ID.DARKOAK_LEAVES: return DarkOak_Leaves;

            case ID.SACREDOAK_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return SacredOak_Wood_Top; }
                else { return SacredOak_Wood_Side; }
            case ID.SACREDOAK_LEAVES: return SacredOak_Leaves;

            case ID.SPRUCE_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Spruce_Wood_Top; }
                else { return Spruce_Wood_Side; }
            case ID.SPRUCE_LEAVES: return Spruce_Leaves;

            case ID.JUNGLE_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Jungle_Wood_Top; }
                else { return Jungle_Wood_Side; }
            case ID.JUNGLE_LEAVES: return Jungle_Leaves;

            case ID.ACACIA_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Acacia_Wood_Top; }
                else { return Acacia_Wood_Side; }
            case ID.ACACIA_LEAVES: return Acacia_Leaves;

            case ID.WILLOW_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Willow_Wood_Top; }
                else { return Willow_Wood_Side; }
            case ID.WILLOW_LEAVES: return Willow_Leaves;

            case ID.UMBRAN_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Umbran_Wood_Top; }
                else { return Umbran_Wood_Side; }
            case ID.UMBRAN_LEAVES: return Umbran_Leaves;

            case ID.REDWOOD_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Redwood_Wood_Top; }
                else { return Redwood_Wood_Side; }
            case ID.REDWOOD_LEAVES: return Redwood_Leaves;

            case ID.PINE_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Pine_Wood_Top; }
                else { return Pine_Wood_Side; }

            // Atlas row Y5
            case ID.PINE_LEAVES: return Pine_Leaves;

            case ID.PALM_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Palm_Wood_Top; }
                else { return Palm_Wood_Side; }
            case ID.PALM_LEAVES: return Palm_Leaves;

            case ID.MANGROVE_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Mangrove_Wood_Top; }
                else { return Mangrove_Wood_Side; }
            case ID.MANGROVE_LEAVES: return Mangrove_Leaves;

            case ID.MAHOGANY_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Mahogany_Wood_Top; }
                else { return Mahogany_Wood_Side; }
            case ID.MAHOGANY_LEAVES: return Mahagany_Leaves;

            case ID.MAGICAL_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Magical_Wood_Top; }
                else { return Magical_Wood_Side; }
            case ID.MAGICAL_LEAVES: return Magical_Leaves;

            case ID.JACARANDA_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Jacaranda_Wood_Top; }
                else { return Jacaranda_Wood_Side; }
            case ID.JACARANDA_LEAVES: return Jacaranda_Leaves;

            case ID.FIR_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Fir_Wood_Top; }
                else { return Fir_Wood_Side; }
            case ID.FIR_LEAVES: return Fir_Leaves;

            case ID.EUCALYPTUS_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Eucalyptus_Wood_Top; }
                else { return Eucalyptus_Wood_Side; }
            case ID.EUCALYPTUS_LEAVES: return Eucalyptus_Leaves;

            case ID.EBONY_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Ebony_Wood_Top; }
                else { return Ebony_Wood_Side; }
            case ID.EBONY_LEAVES: return Ebony_Leaves;

            case ID.HELLBARK_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Hellbark_Wood_Top; }
                else { return Hellbark_Wood_Side; }
            case ID.HELLBARK_LEAVES: return Hellbark_Leaves;

            case ID.ETHEREAL_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Ethereal_Wood_Top; }
                else { return Ethereal_Wood_Side; }
            case ID.ETHEREAL_LEAVES: return Ethereal_Leaves;

            case ID.CHERRY_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Cherry_Wood_Top; }
                else { return Cherry_Wood_Side; }

            // Atlas row Y6
            case ID.CHERRY_LEAVES_WHITE: return Cherry_Leaves_White;
            case ID.CHERRY_LEAVES_PINK: return Cherry_Leaves_Pink;

            case ID.DEAD_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return Dead_Wood_Top; }
                else { return Dead_Wood_Side; }
            case ID.DEAD_LEAVES: return Dead_Leaves;

            default: return new Vector2 (31, 31);  // fallback texture for when something fails
        }
    }

    #region Texture Atlas - Vector2 Block Settings

    // Atlas row Y0
    private static Vector2 ErrorFallback = new Vector2 (1, 1);
    private static Vector2 Air = new Vector2 (0, 0);
    private static Vector2 DestroyStage1 = new Vector2 (1, 0);
    private static Vector2 DestroyStage2 = new Vector2 (2, 0);
    private static Vector2 DestroyStage3 = new Vector2 (3, 0);
    private static Vector2 DestroyStage4 = new Vector2 (4, 0);
    private static Vector2 DestroyStage5 = new Vector2 (5, 0);
    private static Vector2 DestroyStage6 = new Vector2 (6, 0);
    private static Vector2 DestroyStage7 = new Vector2 (7, 0);
    private static Vector2 DestroyStage8 = new Vector2 (8, 0);
    private static Vector2 DestroyStage9 = new Vector2 (9, 0);
    private static Vector2 DestroyStage10 = new Vector2 (10, 0);
    private static Vector2 Bedrock = new Vector2 (11, 0);
    private static Vector2 Basalt = new Vector2 (12, 0);
    private static Vector2 Granite = new Vector2 (13, 0);
    private static Vector2 Gabbro = new Vector2 (14, 0);
    private static Vector2 Obsidian = new Vector2 (15, 0);
    private static Vector2 Coal = new Vector2 (16, 0);
    private static Vector2 Clay = new Vector2 (17, 0);
    private static Vector2 Slate = new Vector2 (18, 0);
    private static Vector2 Gravel = new Vector2 (19, 0);
    private static Vector2 Sand = new Vector2 (20, 0);
    private static Vector2 Sandstone = new Vector2 (21, 0);
    private static Vector2 Quartzite = new Vector2 (22, 0);
    private static Vector2 Limestone = new Vector2 (23, 0);
    private static Vector2 Marble = new Vector2 (24, 0);
    private static Vector2 Mud = new Vector2 (25, 0);
    private static Vector2 Mudstone = new Vector2 (26, 0);
    private static Vector2 Gneiss = new Vector2 (27, 0);
    private static Vector2 Shale = new Vector2 (28, 0);
    private static Vector2 Grass_Side = new Vector2 (29, 0);
    private static Vector2 Grass_Top = new Vector2 (30, 0);
    private static Vector2 Halite = new Vector2 (31, 0);

    // Atlas row Y1
    private static Vector2 Lava_Still = new Vector2 (32, 1);
    private static Vector2 Lava_Flowing = new Vector2 (33, 1);
    private static Vector2 Water_Still = new Vector2 (34, 1);
    private static Vector2 Water_Flowing = new Vector2 (35, 1);
    private static Vector2 Nitre = new Vector2 (36, 1);
    private static Vector2 Gypsum = new Vector2 (37, 1);
    private static Vector2 Andesite = new Vector2 (38, 1);
    private static Vector2 Ryolite = new Vector2 (39, 1);
    private static Vector2 Diorite = new Vector2 (40, 1);
    private static Vector2 Peridotite = new Vector2 (41, 1);
    private static Vector2 Pumice = new Vector2 (42, 1);
    private static Vector2 Chalk = new Vector2 (43, 1);
    private static Vector2 Siltstone = new Vector2 (44, 1);
    private static Vector2 Claystone = new Vector2 (45, 1);
    private static Vector2 Schist = new Vector2 (46, 1);







    private static Vector2 Sperrylite = new Vector2 (54, 1);
    private static Vector2 Uraninite = new Vector2 (55, 1);
    private static Vector2 Beryl = new Vector2 (56, 1);
    private static Vector2 Cassiterite = new Vector2 (57, 1);
    private static Vector2 Cobaltite = new Vector2 (58, 1);
    private static Vector2 Molybdenite = new Vector2 (59, 1);
    private static Vector2 Millerite = new Vector2 (60, 1);
    private static Vector2 Pollucite = new Vector2 (61, 1);
    private static Vector2 Pentlandite = new Vector2 (62, 1);
    private static Vector2 Pyrite = new Vector2 (63, 1);

    // Atlas row Y2
    private static Vector2 Bauxite = new Vector2 (64, 2);
    private static Vector2 Chalcocite = new Vector2 (65, 2);
    private static Vector2 Gold = new Vector2 (66, 2);
    private static Vector2 Hematite = new Vector2 (67, 2);
    private static Vector2 Acanthite = new Vector2 (68, 2);
    private static Vector2 Chromite = new Vector2 (69, 2);
    private static Vector2 Cinnabar = new Vector2 (70, 2);
    private static Vector2 Galena = new Vector2 (71, 2);
    private static Vector2 Sphalerite = new Vector2 (72, 2);
    private static Vector2 Ilmenite = new Vector2 (73, 2);
    private static Vector2 Magnetite = new Vector2 (74, 2);
    private static Vector2 Pyrolusite = new Vector2 (75, 2);
    private static Vector2 Scheelite = new Vector2 (76, 2);
    private static Vector2 Jasper_Granite = new Vector2 (77, 2);
    private static Vector2 Jasper_Gneiss = new Vector2 (78, 2);
    private static Vector2 Agate = new Vector2 (79, 2);
    private static Vector2 Moonstone_Granite = new Vector2 (80, 2);
    private static Vector2 Moonstone_Gneiss = new Vector2 (81, 2);
    private static Vector2 Onyx = new Vector2 (82, 2);
    private static Vector2 Opal = new Vector2 (83, 2);
    private static Vector2 Almandine_Granite = new Vector2 (84, 2);
    private static Vector2 Almandine_Gneiss = new Vector2 (85, 2);
    private static Vector2 Labradorite = new Vector2 (86, 2);
    private static Vector2 Aquamarine = new Vector2 (87, 2);
    private static Vector2 Turquoise_Sandstone = new Vector2 (88, 2);
    private static Vector2 Turquoise_Quartzite = new Vector2 (89, 2);
    private static Vector2 Larimar = new Vector2 (90, 2);
    private static Vector2 Citrine_Granite = new Vector2 (91, 2);
    private static Vector2 Citrine_Gneiss = new Vector2 (92, 2);
    private static Vector2 GoldenBeryl = new Vector2 (93, 2);
    private static Vector2 Grossular_Granite = new Vector2 (94, 2);
    private static Vector2 Grossular_Gneiss = new Vector2 (95, 2);

    // Atlas row Y3
    private static Vector2 Emerald = new Vector2 (96, 3);
    private static Vector2 Peridot_Granite = new Vector2 (97, 3);
    private static Vector2 Peridot_Gneiss = new Vector2 (98, 3);
    private static Vector2 Jade = new Vector2 (99, 3);
    private static Vector2 Ruby_Granite = new Vector2 (100, 3);
    private static Vector2 Ruby_Marble = new Vector2 (101, 3);
    private static Vector2 Carnelian = new Vector2 (102, 3);
    private static Vector2 Pyrope_Granite = new Vector2 (103, 3);
    private static Vector2 Pyrope_Gneiss = new Vector2 (104, 3);
    private static Vector2 Topaz_Granite = new Vector2 (105, 3);
    private static Vector2 Topaz_Ryolite = new Vector2 (106, 3);
    private static Vector2 Sunstone_Granite = new Vector2 (107, 3);
    private static Vector2 Sunstone_Gneiss = new Vector2 (108, 3);


    private static Vector2 Amethyst_Granite = new Vector2 (111, 3);
    private static Vector2 Amethyst_Gneiss = new Vector2 (112, 3);
    private static Vector2 Morganite = new Vector2 (113, 3);
    private static Vector2 Rhodolite_Granite = new Vector2 (114, 3);
    private static Vector2 Rhodolite_Gneiss = new Vector2 (115, 3);
    private static Vector2 LapisLazuli_Marble = new Vector2 (116, 3);
    private static Vector2 LapisLazuli_Quartzite = new Vector2 (117, 3);
    private static Vector2 Sapphite_Granite = new Vector2 (118, 3);
    private static Vector2 Sapphire_Marble = new Vector2 (119, 3);
    private static Vector2 Sodalite_Granite = new Vector2 (120, 3);
    private static Vector2 Sodalite_Marble = new Vector2 (121, 3);
    private static Vector2 Sodalite_Basalt = new Vector2 (122, 3);
    private static Vector2 TigersEye = new Vector2 (123, 3);
    private static Vector2 Bloodstone = new Vector2 (124, 3);
    private static Vector2 RockCrystal_Granite = new Vector2 (125, 3);
    private static Vector2 RockCrystal_Gneiss = new Vector2 (126, 3);
    private static Vector2 Diamond = new Vector2 (127, 3);

    // Atlas row Y4

    private static Vector2 Oak_Wood_Side = new Vector2 (128, 4);
    private static Vector2 Oak_Wood_Top = new Vector2 (129, 4);
    private static Vector2 Oak_Leaves = new Vector2 (130, 4);
    private static Vector2 Birch_Wood_Side = new Vector2 (131, 4);
    private static Vector2 Birch_Wood_Top = new Vector2 (132, 4);
    private static Vector2 Birch_Leaves = new Vector2 (133, 4);
    private static Vector2 DarkOak_Wood_Side = new Vector2 (134, 4);
    private static Vector2 DarkOak_Wood_Top = new Vector2 (135, 4);
    private static Vector2 DarkOak_Leaves = new Vector2 (136, 4);
    private static Vector2 SacredOak_Wood_Side = new Vector2 (137, 4);
    private static Vector2 SacredOak_Wood_Top = new Vector2 (138, 4);
    private static Vector2 SacredOak_Leaves = new Vector2 (139, 4);
    private static Vector2 Spruce_Wood_Side = new Vector2 (140, 4);
    private static Vector2 Spruce_Wood_Top = new Vector2 (141, 4);
    private static Vector2 Spruce_Leaves = new Vector2 (142, 4);
    private static Vector2 Jungle_Wood_Side = new Vector2 (143, 4);
    private static Vector2 Jungle_Wood_Top = new Vector2 (144, 4);
    private static Vector2 Jungle_Leaves = new Vector2 (145, 4);
    private static Vector2 Acacia_Wood_Side = new Vector2 (146, 4);
    private static Vector2 Acacia_Wood_Top = new Vector2 (147, 4);
    private static Vector2 Acacia_Leaves = new Vector2 (148, 4);
    private static Vector2 Willow_Wood_Side = new Vector2 (149, 4);
    private static Vector2 Willow_Wood_Top = new Vector2 (150, 4);
    private static Vector2 Willow_Leaves = new Vector2 (151, 4);
    private static Vector2 Umbran_Wood_Side = new Vector2 (152, 4);
    private static Vector2 Umbran_Wood_Top = new Vector2 (153, 4);
    private static Vector2 Umbran_Leaves = new Vector2 (154, 4);
    private static Vector2 Redwood_Wood_Side = new Vector2 (155, 4);
    private static Vector2 Redwood_Wood_Top = new Vector2 (156, 4);
    private static Vector2 Redwood_Leaves = new Vector2 (157, 4);
    private static Vector2 Pine_Wood_Side = new Vector2 (158, 4);
    private static Vector2 Pine_Wood_Top = new Vector2 (159, 4);

    // Atlas row Y5
    private static Vector2 Pine_Leaves = new Vector2 (160, 5);
    private static Vector2 Palm_Wood_Side = new Vector2 (161, 5);
    private static Vector2 Palm_Wood_Top = new Vector2 (162, 5);
    private static Vector2 Palm_Leaves = new Vector2 (163, 5);
    private static Vector2 Mangrove_Wood_Side = new Vector2 (164, 5);
    private static Vector2 Mangrove_Wood_Top = new Vector2 (165, 5);
    private static Vector2 Mangrove_Leaves = new Vector2 (166, 5);
    private static Vector2 Mahogany_Wood_Side = new Vector2 (167, 5);
    private static Vector2 Mahogany_Wood_Top = new Vector2 (168, 5);
    private static Vector2 Mahagany_Leaves = new Vector2 (169, 5);
    private static Vector2 Magical_Wood_Side = new Vector2 (170, 5);
    private static Vector2 Magical_Wood_Top = new Vector2 (171, 5);
    private static Vector2 Magical_Leaves = new Vector2 (172, 5);
    private static Vector2 Jacaranda_Wood_Side = new Vector2 (173, 5);
    private static Vector2 Jacaranda_Wood_Top = new Vector2 (174, 5);
    private static Vector2 Jacaranda_Leaves = new Vector2 (175, 5);
    private static Vector2 Fir_Wood_Side = new Vector2 (176, 5);
    private static Vector2 Fir_Wood_Top = new Vector2 (177, 5);
    private static Vector2 Fir_Leaves = new Vector2 (178, 5);
    private static Vector2 Eucalyptus_Wood_Side = new Vector2 (179, 5);
    private static Vector2 Eucalyptus_Wood_Top = new Vector2 (180, 5);
    private static Vector2 Eucalyptus_Leaves = new Vector2 (181, 5);
    private static Vector2 Ebony_Wood_Side = new Vector2 (182, 5);
    private static Vector2 Ebony_Wood_Top = new Vector2 (183, 5);
    private static Vector2 Ebony_Leaves = new Vector2 (184, 5);
    private static Vector2 Hellbark_Wood_Side = new Vector2 (185, 5);
    private static Vector2 Hellbark_Wood_Top = new Vector2 (186, 5);
    private static Vector2 Hellbark_Leaves = new Vector2 (187, 5);
    private static Vector2 Ethereal_Wood_Side = new Vector2 (188, 5);
    private static Vector2 Ethereal_Wood_Top = new Vector2 (189, 5);
    private static Vector2 Ethereal_Leaves = new Vector2 (190, 5);
    private static Vector2 Cherry_Wood_Side = new Vector2 (191, 5);

    // Atlas row Y6
    private static Vector2 Cherry_Wood_Top = new Vector2 (192, 6);
    private static Vector2 Cherry_Leaves_White = new Vector2 (193, 6);
    private static Vector2 Cherry_Leaves_Pink = new Vector2 (194, 6);
    private static Vector2 Dead_Wood_Side = new Vector2 (195, 5);
    private static Vector2 Dead_Wood_Top = new Vector2 (196, 6);
    private static Vector2 Dead_Leaves = new Vector2 (197, 6);



    #endregion

    public byte IsTransparent (ushort id)  // updated 20th November 2018
    {
        switch ((ID) id)
        {
            // Atlas row Y0
            case ID.ERRORFALLBACK: return 0;
            case ID.AIR: return 1;
            case ID.DESTROYSTAGE1: return 1;
            case ID.DESTROYSTAGE2: return 1;
            case ID.DESTROYSTAGE3: return 1;
            case ID.DESTROYSTAGE4: return 1;
            case ID.DESTROYSTAGE5: return 1;
            case ID.DESTROYSTAGE6: return 1;
            case ID.DESTROYSTAGE7: return 1;
            case ID.DESTROYSTAGE8: return 1;
            case ID.DESTROYSTAGE9: return 1;
            case ID.DESTROYSTAGE10: return 1;
            case ID.BEDROCK: return 0;
            case ID.BASALT: return 0;
            case ID.GRANITE: return 0;
            case ID.GABBRO: return 0;
            case ID.OBSIDIAN: return 0;
            case ID.COAL: return 0;
            case ID.CLAY: return 0;
            case ID.SLATE: return 0;
            case ID.GRAVEL: return 0;
            case ID.SAND: return 0;
            case ID.SANDSTONE: return 0;
            case ID.QUARTZITE: return 0;
            case ID.LIMESTONE: return 0;
            case ID.MARBLE: return 0;
            case ID.MUD: return 0;
            case ID.MUDSTONE: return 0;
            case ID.GNEISS: return 0;
            case ID.SHALE: return 0;
            case ID.GRASS: return 0;
            case ID.HALITE: return 0;

            // Atlas row Y1
            case ID.LAVA_STILL: return 0;
            case ID.LAVA_FLOWING: return 0;
            case ID.WATER_STILL: return 1;
            case ID.WATER_FLOWING: return 1;
            case ID.NITRE: return 0;
            case ID.GYPSUM: return 0;
            case ID.ANDESITE: return 0;
            case ID.RYOLITE: return 0;
            case ID.DIORITE: return 0;
            case ID.PERIDOTITE: return 0;
            case ID.PUMICE: return 0;
            case ID.CHALK: return 0;
            case ID.SILTSTONE: return 0;
            case ID.CLAYSTONE: return 0;
            case ID.SCHIST: return 0;







            case ID.SPERRYLITE: return 0;
            case ID.URANINITE: return 0;
            case ID.BERYL: return 0;
            case ID.CASSITERITE: return 0;
            case ID.COBALTITE: return 0;
            case ID.MOLYBDENITE: return 0;
            case ID.MILLERITE: return 0;
            case ID.POLLUCITE: return 0;
            case ID.PENTLANDITE: return 0;
            case ID.PYRITE: return 0;

            // Atlas row Y2
            case ID.BAUXITE: return 0;
            case ID.CHALCOCITE: return 0;
            case ID.GOLD: return 0;
            case ID.HEMATITE: return 0;
            case ID.ACANTHITE: return 0;
            case ID.CHROMITE: return 0;
            case ID.CINNABAR: return 0;
            case ID.GALENA: return 0;
            case ID.SPHALERITE: return 0;
            case ID.ILMENITE: return 0;
            case ID.MAGNETITE: return 0;
            case ID.PYROLUSITE: return 0;
            case ID.SCHEELITE: return 0;
            case ID.JASPER_GRANITE: return 0;
            case ID.JASPER_GNEISS: return 0;
            case ID.AGATE: return 0;
            case ID.MOONSTONE_GRANITE: return 0;
            case ID.MOONSTONE_GNEISS: return 0;
            case ID.ONYX: return 0;
            case ID.OPAL: return 0;
            case ID.ALMANDINE_GRANITE: return 0;
            case ID.ALMANDINE_GNEISS: return 0;
            case ID.LABRADORITE: return 0;
            case ID.AQUAMARINE: return 0;
            case ID.TURQUOISE_SANDSTONE: return 0;
            case ID.TURQUOISE_QUARTZITE: return 0;
            case ID.LARIMAR: return 0;
            case ID.CITRINE_GRANITE: return 0;
            case ID.CITRINE_GNEISS: return 0;
            case ID.GOLDENBERYL: return 0;
            case ID.GROSSULAR_GRANITE: return 0;
            case ID.GROSSULAR_GNEISS: return 0;

            // Atlas row Y3
            case ID.EMERALD: return 0;
            case ID.PERIDOT_GRANITE: return 0;
            case ID.PERIDOT_GNEISS: return 0;
            case ID.JADE: return 0;
            case ID.RUBY_GRANITE: return 0;
            case ID.RUBY_MARBLE: return 0;
            case ID.CARNELIAN: return 0;
            case ID.PYROPE_GRANITE: return 0;
            case ID.PYROPE_GNEISS: return 0;
            case ID.TOPAZ_GRANITE: return 0;
            case ID.TOPAZ_RYOLITE: return 0;
            case ID.SUNSTONE_GRANITE: return 0;
            case ID.SUNSTONE_GNEISS: return 0;


            case ID.AMETHYST_GRANITE: return 0;
            case ID.AMETHYST_GNEISS: return 0;
            case ID.MORGANITE: return 0;
            case ID.RHODOLITE_GRANITE: return 0;
            case ID.RHODOLITE_GNEISS: return 0;
            case ID.LAPISLAZULI_MARBLE: return 0;
            case ID.LAPISLAZULI_QUARTZITE: return 0;
            case ID.SAPPHIRE_GRANITE: return 0;
            case ID.SAPPHIRE_MARBLE: return 0;
            case ID.SODALITE_GRANITE: return 0;
            case ID.SODALITE_MARBLE: return 0;
            case ID.SODALITE_BASALT: return 0;
            case ID.TIGERSEYE: return 0;
            case ID.BLOODSTONE: return 0;
            case ID.ROCKCRYSTAL_GRANITE: return 0;
            case ID.ROCKCRYSTAL_GNEISS: return 0;
            case ID.DIAMOND: return 0;

            // Atlas row Y4
            case ID.OAK_WOOD: return 0;
            case ID.OAK_LEAVES: return 1;
            case ID.BIRCH_WOOD: return 0;
            case ID.BIRCH_LEAVES: return 1;
            case ID.DARKOAK_WOOD: return 0;
            case ID.DARKOAK_LEAVES: return 1;
            case ID.SACREDOAK_WOOD: return 0;
            case ID.SACREDOAK_LEAVES: return 1;
            case ID.SPRUCE_WOOD: return 0;
            case ID.SPRUCE_LEAVES: return 1;
            case ID.JUNGLE_WOOD: return 0;
            case ID.JUNGLE_LEAVES: return 1;
            case ID.ACACIA_WOOD: return 0;
            case ID.ACACIA_LEAVES: return 1;
            case ID.WILLOW_WOOD: return 0;
            case ID.WILLOW_LEAVES: return 1;
            case ID.UMBRAN_WOOD: return 0;
            case ID.UMBRAN_LEAVES: return 1;
            case ID.REDWOOD_WOOD: return 0;
            case ID.REDWOOD_LEAVES: return 1;
            case ID.PINE_WOOD: return 0;

            // Atlas row Y5
            case ID.PINE_LEAVES: return 1;
            case ID.PALM_WOOD: return 0;
            case ID.PALM_LEAVES: return 1;
            case ID.MANGROVE_WOOD: return 0;
            case ID.MANGROVE_LEAVES: return 1;
            case ID.MAHOGANY_WOOD: return 0;
            case ID.MAHOGANY_LEAVES: return 1;
            case ID.MAGICAL_WOOD: return 0;
            case ID.MAGICAL_LEAVES: return 1;
            case ID.JACARANDA_WOOD: return 0;
            case ID.JACARANDA_LEAVES: return 1;
            case ID.FIR_WOOD: return 0;
            case ID.FIR_LEAVES: return 1;
            case ID.EUCALYPTUS_WOOD: return 0;
            case ID.EUCALYPTUS_LEAVES: return 1;
            case ID.EBONY_WOOD: return 0;
            case ID.EBONY_LEAVES: return 1;
            case ID.HELLBARK_WOOD: return 0;
            case ID.HELLBARK_LEAVES: return 1;
            case ID.ETHEREAL_WOOD: return 0;
            case ID.ETHEREAL_LEAVES: return 1;
            case ID.CHERRY_WOOD: return 0;

            // Atlas row Y6
            case ID.CHERRY_LEAVES_WHITE: return 1;
            case ID.CHERRY_LEAVES_PINK: return 1;
            case ID.DEAD_WOOD: return 0;
            case ID.DEAD_LEAVES: return 1;

            default: return 0;
        }
    }

    public TextureUVHelper GetUVs (ushort id, Dir dir)  // mesh gen system uses this
    {
        ID atlasId = (ID)id;
        Vector2 texturePos = GetTextureCoord(atlasId, dir);

        TextureUVHelper texUVHelper = new TextureUVHelper
        {
            pos1 = new float2(texturePos.x * tUnit + tUnit, texturePos.y * tUnit + tUnit),
            pos2 = new float2(texturePos.x * tUnit, texturePos.y * tUnit + tUnit),
            pos3 = new float2(texturePos.x * tUnit, texturePos.y * tUnit),
            pos4 = new float2(texturePos.x * tUnit + tUnit, texturePos.y * tUnit)
        };
        return texUVHelper;
    }

    public ushort From2DTo1D (Vector2 textureCoord) // use this for getting texture atlas vector2 as an int
    {
        var textureSize = 0.03125f;
        ushort X = (ushort) math.floor (textureCoord.x);
        ushort Y = (ushort) math.floor (textureCoord.y);

        return (ushort) (X + (Y * textureSize));
    }
}

public struct TextureUVHelper
{
    public float2 pos1;
    public float2 pos2;
    public float2 pos3;
    public float2 pos4;

    public float2 this[int point]
    {
        get
        {
            switch (point)
            {
                case 0: return pos1;
                case 1: return pos2;
                case 2: return pos3;
                case 3: return pos4;

                default: throw new System.ArgumentOutOfRangeException("Index out of range 3: " + point);
            }
        }
    }
}