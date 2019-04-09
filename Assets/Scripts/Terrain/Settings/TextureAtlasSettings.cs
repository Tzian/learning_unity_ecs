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

    public Vector2 GetTextureCoord (ID id, Dir dir) 
    {
        switch (id)
        {
            case ID.ERRORFALLBACK: return new float2(1, 1);
            case ID.AIR: return new Vector2(0, 0);
            case ID.DESTROYSTAGE1: return new Vector2(1, 0);
            case ID.DESTROYSTAGE2: return new Vector2(2, 0);
            case ID.DESTROYSTAGE3: return new Vector2(3, 0);
            case ID.DESTROYSTAGE4: return new Vector2(4, 0);
            case ID.DESTROYSTAGE5: return new Vector2(5, 0);
            case ID.DESTROYSTAGE6: return new Vector2(6, 0);
            case ID.DESTROYSTAGE7: return new Vector2(7, 0);
            case ID.DESTROYSTAGE8: return new Vector2(8, 0);
            case ID.DESTROYSTAGE9: return new Vector2(9, 0);
            case ID.DESTROYSTAGE10: return new Vector2(10, 0);
            case ID.BEDROCK: return new Vector2(11, 0);
            case ID.BASALT: return new Vector2(12, 0);
            case ID.GRANITE: return new Vector2(13, 0);
            case ID.GABBRO: return new Vector2(14, 0);
            case ID.OBSIDIAN: return new Vector2(15, 0);
            case ID.COAL: return new Vector2(16, 0);
            case ID.CLAY: return new Vector2(17, 0);
            case ID.SLATE: return new Vector2(18, 0);
            case ID.GRAVEL: return new Vector2(19, 0);
            case ID.SAND: return new Vector2(20, 0);
            case ID.SANDSTONE: return new Vector2(21, 0);
            case ID.QUARTZITE: return new Vector2(22, 0);
            case ID.LIMESTONE: return new Vector2(23, 0);
            case ID.MARBLE: return new Vector2(24, 0);
            case ID.MUD: return new Vector2(25, 0);
            case ID.MUDSTONE: return new Vector2(26, 0);
            case ID.GNEISS: return new Vector2(27, 0);
            case ID.SHALE: return new Vector2(28, 0);
            case ID.GRASS:
                if (dir == Dir.Up) { return new Vector2(30, 0); }
                else if (dir == Dir.Down) { return new Vector2(25, 0); }
                else { return new Vector2(29, 0); }
            case ID.HALITE: return new Vector2(31, 0);


            // Atlas row Y1
            case ID.LAVA_STILL: return new Vector2(32, 1);
            case ID.LAVA_FLOWING: return new Vector2(33, 1);
            case ID.WATER_STILL: return new Vector2(34, 1);
            case ID.WATER_FLOWING: return new Vector2(35, 1);
            case ID.NITRE: return new Vector2(36, 1);
            case ID.GYPSUM: return new Vector2(37, 1);
            case ID.ANDESITE: return new Vector2(38, 1);
            case ID.RYOLITE: return new Vector2(39, 1);
            case ID.DIORITE: return new Vector2(40, 1);
            case ID.PERIDOTITE: return new Vector2(41, 1);
            case ID.PUMICE: return new Vector2(42, 1);
            case ID.CHALK: return new Vector2(43, 1);
            case ID.SILTSTONE: return new Vector2(44, 1);
            case ID.CLAYSTONE: return new Vector2(45, 1);
            case ID.SCHIST: return new Vector2(46, 1);

            // several spaces available here on atlas


            case ID.SPERRYLITE: return new Vector2(54, 1);
            case ID.URANINITE: return new Vector2(55, 1);
            case ID.BERYL: return new Vector2(56, 1);
            case ID.CASSITERITE: return new Vector2(57, 1);
            case ID.COBALTITE: return new Vector2(58, 1);
            case ID.MOLYBDENITE: return new Vector2(59, 1);
            case ID.MILLERITE: return new Vector2(60, 1);
            case ID.POLLUCITE: return new Vector2(61, 1);
            case ID.PENTLANDITE: return new Vector2(62, 1);
            case ID.PYRITE: return new Vector2(63, 1);

            // Atlas row Y2
            case ID.BAUXITE: return new Vector2(64, 2);
            case ID.CHALCOCITE: return new Vector2(65, 2);
            case ID.GOLD: return new Vector2(66, 2);
            case ID.HEMATITE: return new Vector2(67, 2);
            case ID.ACANTHITE: return new Vector2(68, 2);
            case ID.CHROMITE: return new Vector2(69, 2);
            case ID.CINNABAR: return new Vector2(70, 2);
            case ID.GALENA: return new Vector2(71, 2);
            case ID.SPHALERITE: return new Vector2(72, 2);
            case ID.ILMENITE: return new Vector2(73, 2);
            case ID.MAGNETITE: return new Vector2(74, 2);
            case ID.PYROLUSITE: return new Vector2(75, 2);
            case ID.SCHEELITE: return new Vector2(76, 2);
            case ID.JASPER_GRANITE: return new Vector2(77, 2);
            case ID.JASPER_GNEISS: return new Vector2(78, 2);
            case ID.AGATE: return new Vector2(79, 2);
            case ID.MOONSTONE_GRANITE: return new Vector2(80, 2);
            case ID.MOONSTONE_GNEISS: return new Vector2(81, 2);
            case ID.ONYX: return new Vector2(82, 2);
            case ID.OPAL: return new Vector2(83, 2);
            case ID.ALMANDINE_GRANITE: return new Vector2(84, 2);
            case ID.ALMANDINE_GNEISS: return new Vector2(85, 2);
            case ID.LABRADORITE: return new Vector2(86, 2);
            case ID.AQUAMARINE: return new Vector2(87, 2);
            case ID.TURQUOISE_SANDSTONE: return new Vector2(88, 2);
            case ID.TURQUOISE_QUARTZITE: return new Vector2(89, 2);
            case ID.LARIMAR: return new Vector2(90, 2);
            case ID.CITRINE_GRANITE: return new Vector2(91, 2);
            case ID.CITRINE_GNEISS: return new Vector2(92, 2);
            case ID.GOLDENBERYL: return new Vector2(93, 2);
            case ID.GROSSULAR_GRANITE: return new Vector2(94, 2);
            case ID.GROSSULAR_GNEISS: return new Vector2(95, 2);

            // Atlas row Y3
            case ID.EMERALD: return new Vector2(96, 3);
            case ID.PERIDOT_GRANITE: return new Vector2(97, 3);
            case ID.PERIDOT_GNEISS: return new Vector2(98, 3);
            case ID.JADE: return new Vector2(99, 3);
            case ID.RUBY_GRANITE: return new Vector2(100, 3);
            case ID.RUBY_MARBLE: return new Vector2(101, 3);
            case ID.CARNELIAN: return new Vector2(102, 3);
            case ID.PYROPE_GRANITE: return new Vector2(103, 3);
            case ID.PYROPE_GNEISS: return new Vector2(104, 3);
            case ID.TOPAZ_GRANITE: return new Vector2(105, 3);
            case ID.TOPAZ_RYOLITE: return new Vector2(106, 3);
            case ID.SUNSTONE_GRANITE: return new Vector2(107, 3);
            case ID.SUNSTONE_GNEISS: return new Vector2(108, 3);

            // space for 2 on atlas here

            case ID.AMETHYST_GRANITE: return new Vector2(111, 3);
            case ID.AMETHYST_GNEISS: return new Vector2(112, 3);
            case ID.MORGANITE: return new Vector2(113, 3);
            case ID.RHODOLITE_GRANITE: return new Vector2(114, 3);
            case ID.RHODOLITE_GNEISS: return new Vector2(115, 3);
            case ID.LAPISLAZULI_MARBLE: return new Vector2(116, 3);
            case ID.LAPISLAZULI_QUARTZITE: return new Vector2(117, 3);
            case ID.SAPPHIRE_GRANITE: return new Vector2(118, 3);
            case ID.SAPPHIRE_MARBLE: return new Vector2(119, 3);
            case ID.SODALITE_GRANITE: return new Vector2(120, 3);
            case ID.SODALITE_MARBLE: return new Vector2(121, 3);
            case ID.SODALITE_BASALT: return new Vector2(122, 3);
            case ID.TIGERSEYE: return new Vector2(123, 3);
            case ID.BLOODSTONE: return new Vector2(124, 3);
            case ID.ROCKCRYSTAL_GRANITE: return new Vector2(125, 3);
            case ID.ROCKCRYSTAL_GNEISS: return new Vector2(126, 3);
            case ID.DIAMOND: return new Vector2(127, 3);

            // Atlas row Y4
            case ID.OAK_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(129, 4); }
                else { return new Vector2(128, 4); }
            case ID.OAK_LEAVES: return new Vector2(130, 4);

            case ID.BIRCH_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(132, 4); }
                else { return new Vector2(131, 4); }
            case ID.BIRCH_LEAVES: return new Vector2(133, 4);

            case ID.DARKOAK_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(135, 4); }
                else { return new Vector2(134, 4); }
            case ID.DARKOAK_LEAVES: return new Vector2(136, 4);

            case ID.SACREDOAK_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(138, 4); }
                else { return new Vector2(137, 4); }
            case ID.SACREDOAK_LEAVES: return new Vector2(139, 4);

            case ID.SPRUCE_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(141, 4); }
                else { return new Vector2(140, 4); }
            case ID.SPRUCE_LEAVES: return new Vector2(142, 4);

            case ID.JUNGLE_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(144, 4); }
                else { return new Vector2(143, 4); }
            case ID.JUNGLE_LEAVES: return new Vector2(145, 4);

            case ID.ACACIA_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(147, 4); }
                else { return new Vector2(146, 4); }
            case ID.ACACIA_LEAVES: return new Vector2(148, 4);

            case ID.WILLOW_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(150, 4); }
                else { return new Vector2(149, 4); }
            case ID.WILLOW_LEAVES: return new Vector2(151, 4);

            case ID.UMBRAN_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(153, 4); }
                else { return new Vector2(152, 4); }
            case ID.UMBRAN_LEAVES: return new Vector2(154, 4);

            case ID.REDWOOD_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(156, 4); }
                else { return new Vector2(155, 4); }
            case ID.REDWOOD_LEAVES: return new Vector2(157, 4);

            case ID.PINE_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(159, 4); }
                else { return new Vector2(158, 4); }

            // Atlas row Y5
            case ID.PINE_LEAVES: return new Vector2(160, 5);

            case ID.PALM_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(162, 5); }
                else { return new Vector2(161, 5); }
            case ID.PALM_LEAVES: return new Vector2(163, 5);

            case ID.MANGROVE_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(165, 5); }
                else { return new Vector2(164, 5); }
            case ID.MANGROVE_LEAVES: return new Vector2(166, 5);

            case ID.MAHOGANY_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(168, 5); }
                else { return new Vector2(167, 5); }
            case ID.MAHOGANY_LEAVES: return new Vector2(169, 5);

            case ID.MAGICAL_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(171, 5); }
                else { return new Vector2(170, 5); }
            case ID.MAGICAL_LEAVES: return new Vector2(172, 5);

            case ID.JACARANDA_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(174, 5); }
                else { return new Vector2(173, 5); }
            case ID.JACARANDA_LEAVES: return new Vector2(175, 5);

            case ID.FIR_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(177, 5); }
                else { return new Vector2(176, 5); }
            case ID.FIR_LEAVES: return new Vector2(178, 5);

            case ID.EUCALYPTUS_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(180, 5); }
                else { return new Vector2(179, 5); }
            case ID.EUCALYPTUS_LEAVES: return new Vector2(181, 5);

            case ID.EBONY_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(183, 5); }
                else { return new Vector2(182, 5); }
            case ID.EBONY_LEAVES: return new Vector2(184, 5);

            case ID.HELLBARK_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(186, 5); }
                else { return new Vector2(185, 5); }
            case ID.HELLBARK_LEAVES: return new Vector2(187, 5);

            case ID.ETHEREAL_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(189, 5); }
                else { return new Vector2(188, 5); }
            case ID.ETHEREAL_LEAVES: return new Vector2(190, 5);

            case ID.CHERRY_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(192, 6); }
                else { return new Vector2(191, 5); }

            // Atlas row Y6
            case ID.CHERRY_LEAVES_WHITE: return new Vector2(193, 6);
            case ID.CHERRY_LEAVES_PINK: return new Vector2(194, 6);

            case ID.DEAD_WOOD:
                if (dir == Dir.Up || dir == Dir.Down) { return new Vector2(196, 6); }
                else { return new Vector2(195, 5); }
            case ID.DEAD_LEAVES: return new Vector2(197, 6);

            default: return new Vector2 (31, 31);  // fallback texture for when something fails
        }
    }
    
    public byte IsTransparent (ushort id) 
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