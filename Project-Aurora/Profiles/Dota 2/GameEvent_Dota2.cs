using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.EffectsEngine.Functions;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Dota_2
{
    enum Dota2AbilityEffects
    {
        None,
        abaddon_death_coil,
        abaddon_borrowed_time,
        alchemist_acid_spray,
        ancient_apparition_ice_vortex,
        ancient_apparition_ice_blast,
        antimage_blink,
        antimage_mana_void,
        axe_berserkers_call,
        beastmaster_primal_roar,
        brewmaster_thunder_clap,
        centaur_hoof_stomp,
        chaos_knight_chaos_bolt,
        crystal_maiden_crystal_nova,
        doom_bringer_scorched_earth,
        dragon_knight_breathe_fire,
        earthshaker_fissure,
        earthshaker_echo_slam,
        elder_titan_earth_splitter,

        kunkka_torrent,
        kunkka_ghostship,
        legion_commander_overwhelming_odds,
        life_stealer_rage,
        magnataur_shockwave,
        omniknight_purification,
        omniknight_repel,
        sandking_epicenter,
        slardar_slithereen_crush,

        lina_dragon_slave,
        lina_light_strike_array,
        lina_laguna_blade,
        nevermore_shadowraze,
        nevermore_requiem,
        rattletrap_rocket_flare,
        razor_plasma_field,
        riki_smoke_screen,
        zuus_arc_lightning,
        zuus_lightning_bolt,
        zuus_thundergods_wrath

    }

    public class GameEvent_Dota2 : LightEvent
    {
        private long lastUpdate = 0;
        private int updateRate = 1; //1 second
        private Random randomizer = new Random();

        private static bool isPlayingKillStreakAnimation = false;
        private double ks_blendamount = 0.0;
        private static long ks_duration = 4000;
        private static long ks_end_time = 0;

        private static bool isDimming = false;
        private static double dim_value = 1.0;
        private static int dim_bg_at = 15;

        private static float[] killstreak_offsets = {  0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                                                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                                            };

        private static float killstreak_line_stop = 5.0f;
        private static float killstreak_line_spaces = 6.0f;

        private static float abiltiyeffect_keyframe = 0.0f;
        private static Dota2AbilityEffects currentabilityeffect = Dota2AbilityEffects.None;
        private static float abilityeffect_time = 0.0f;

        //Game Integration stuff
        private static int mapTime = 0;
        private static PlayerTeam current_team = PlayerTeam.Undefined;
        private static bool isAlive = true;
        private static int respawnTime = 0;
        private static int health = 0;
        private static int health_max = 100;
        private static int mana = 0;
        private static int mana_max = 100;
        private static int kills = 0;
        private static int player_killstreak = 0;
        private static Abilities_Dota2 abilities;
        private static Items_Dota2 items;
        private static PlayerActivity current_activity = PlayerActivity.Undefined;

        Dictionary<string, Color> item_colors = new Dictionary<string, Color>()
        {
            { "empty", Color.FromArgb(0, 0, 0, 0) },
            { "item_aegis", Color.FromArgb(255, 240, 200) },
            { "item_courier", Color.FromArgb(180, 120, 50) },
            { "item_boots_of_elves", Color.FromArgb(115, 140, 50) },
            { "item_belt_of_strength", Color.FromArgb(160, 130, 75) },
            { "item_blade_of_alacrity", Color.FromArgb(130, 150, 140) },
            { "item_blades_of_attack", Color.FromArgb(160, 115, 40) },
            { "item_blink", Color.FromArgb(100, 200, 200) },
            { "item_boots", Color.FromArgb(130, 95, 60) },
            { "item_bottle", Color.FromArgb(35, 155, 185) },
            { "item_broadsword", Color.FromArgb(190, 190, 180) },
            { "item_chainmail", Color.FromArgb(150, 140, 130) },
            { "item_cheese", Color.FromArgb(235, 240, 70) },
            { "item_circlet", Color.FromArgb(220, 160, 30) },
            { "item_clarity", Color.FromArgb(150, 210, 250) },
            { "item_claymore", Color.FromArgb(190, 190, 180) },
            { "item_cloak", Color.FromArgb(115, 40, 20) },
            { "item_demon_edge", Color.FromArgb(30, 240, 150) },
            { "item_dust", Color.FromArgb(220, 180, 245) },
            { "item_eagle", Color.FromArgb(155, 180, 110) },
            { "item_enchanted_mango", Color.FromArgb(240,155,30)},
            { "item_energy_booster", Color.FromArgb(75, 77, 235) },
            { "item_faerie_fire", Color.FromArgb(190, 255, 100) },
            { "item_flying_courier", Color.FromArgb(180, 120, 50) },
            { "item_gauntlets", Color.FromArgb(190, 186, 177) },
            { "item_gem", Color.FromArgb(86, 212, 83) },
            { "item_ghost", Color.FromArgb(249, 252, 120) },
            { "item_gloves", Color.FromArgb(65, 212, 80) },
            { "item_flask", Color.FromArgb(6, 202, 17) },
            { "item_helm_of_iron_will", Color.FromArgb(238, 200, 53) },
            { "item_hyperstone", Color.FromArgb(56, 238, 188) },
            { "item_branches", Color.FromArgb(226, 184, 24) },
            { "item_javelin", Color.FromArgb(172, 165, 160) },
            { "item_magic_stick", Color.FromArgb(120, 215, 173) },
            { "item_mantle", Color.FromArgb(123, 182, 234) },
            { "item_mithril_hammer", Color.FromArgb(193, 210, 195) },
            { "item_lifesteal", Color.FromArgb(149, 139, 134) },
            { "item_mystic_staff", Color.FromArgb(204, 228, 158) },
            { "item_ward_observer", Color.FromArgb(196, 169, 55) },
            { "item_ogre_axe", Color.FromArgb(161, 62, 54) },
            { "item_orb_of_venom", Color.FromArgb(237, 206, 38) },
            { "item_platemail", Color.FromArgb(119, 33, 10) },
            { "item_point_booster", Color.FromArgb(226 ,97, 234) },
            { "item_quarterstaff", Color.FromArgb(130, 98, 24) },
            { "item_quelling_blade", Color.FromArgb(188, 188, 183) },
            { "item_reaver", Color.FromArgb(171, 142, 35) },
            { "item_ring_of_health", Color.FromArgb(166, 48, 4) },
            { "item_ring_of_protection", Color.FromArgb(208, 104, 6) },
            { "item_ring_of_regen", Color.FromArgb(214, 59, 62) },
            { "item_robe", Color.FromArgb(115, 183, 203) },
            { "item_relic", Color.FromArgb(225, 165, 17) },
            { "item_sobi_mask", Color.FromArgb(126, 114, 217) },
            { "item_ward_sentry", Color.FromArgb(107, 207, 214) },
            { "item_shadow_amulet", Color.FromArgb(201, 120, 191) },
            { "item_slippers", Color.FromArgb(99, 188, 25) },
            { "item_smoke_of_deceit", Color.FromArgb(180, 122, 243) },
            { "item_staff_of_wizardry", Color.FromArgb(34, 147, 207) },
            { "item_stout_shield", Color.FromArgb(182, 75, 19) },
            { "item_talisman_of_evasion", Color.FromArgb(181, 250, 238) },
            { "item_tango", Color.FromArgb(80, 243, 194) },
            { "item_tango_single", Color.FromArgb(23, 201, 87) },
            { "item_tpscroll", Color.FromArgb(212, 182, 139) },
            { "item_ultimate_orb", Color.FromArgb(238, 240, 229) },
            { "item_vitality_booster", Color.FromArgb(219, 55, 64) },
            { "item_void_stone", Color.FromArgb(40, 242, 253) },
            { "item_abyssal_blade", Color.FromArgb(123, 94, 99) },
            { "item_recipe_abyssal_blade", Color.FromArgb(218, 177, 103) },
            { "item_aether_lens", Color.FromArgb(97, 241, 221) },
            { "item_recipe_aether_lens", Color.FromArgb(218, 177, 103) },
            { "item_ultimate_scepter", Color.FromArgb(70, 142, 244) },
            { "item_recipe_ultimate_scepter", Color.FromArgb(218, 177, 103) },
            { "item_arcane_boots", Color.FromArgb(67, 170, 218) },
            { "item_recipe_arcane_boots", Color.FromArgb(218, 177, 103) },
            { "item_armlet", Color.FromArgb(229, 165, 172) },
            { "item_recipe_armlet", Color.FromArgb(218, 177, 103) },
            { "item_assault", Color.FromArgb(56, 196, 202) },
            { "item_recipe_assault", Color.FromArgb(218, 177, 103) },
            { "item_bfury", Color.FromArgb(231, 165, 89) },
            { "item_recipe_bfury", Color.FromArgb(218, 177, 103) },
            { "item_black_king_bar", Color.FromArgb(200, 155, 13) },
            { "item_recipe_black_king_bar", Color.FromArgb(218, 177, 103) },
            { "item_blade_mail", Color.FromArgb(53, 33, 55) },
            { "item_recipe_blade_mail", Color.FromArgb(218, 177, 103) },
            { "item_bloodstone", Color.FromArgb(227, 33, 46) },
            { "item_recipe_bloodstone", Color.FromArgb(218, 177, 103) },
            { "item_travel_boots", Color.FromArgb(188, 140, 82) },
            { "item_travel_boots_2", Color.FromArgb(226, 189, 123) },
            { "item_recipe_travel_boots", Color.FromArgb(218, 177, 103) },
            { "item_bracer", Color.FromArgb(189, 95, 19) },
            { "item_recipe_bracer", Color.FromArgb(218, 177, 103) },
            { "item_buckler", Color.FromArgb(213, 177, 75) },
            { "item_recipe_buckler", Color.FromArgb(218, 177, 103) },
            { "item_butterfly", Color.FromArgb(226, 243, 142) },
            { "item_recipe_butterfly", Color.FromArgb(218, 177, 103) },
            { "item_crimson_guard", Color.FromArgb(189, 113, 2) },
            { "item_recipe_crimson_guard", Color.FromArgb(218, 177, 103) },
            { "item_lesser_crit", Color.FromArgb(234, 50, 51) },
            { "item_recipe_lesser_crit", Color.FromArgb(218, 177, 103) },
            { "item_greater_crit", Color.FromArgb(254, 88, 77) },
            { "item_recipe_greater_crit", Color.FromArgb(218, 177, 103) },
            { "item_dagon", Color.FromArgb(235, 170, 18) },
            { "item_dagon_2", Color.FromArgb(236, 190, 45) },
            { "item_dagon_3", Color.FromArgb(252, 211, 94) },
            { "item_dagon_4", Color.FromArgb(251, 216, 78) },
            { "item_dagon_5", Color.FromArgb(255, 214, 109) },
            { "item_recipe_dagon", Color.FromArgb(218, 177, 103) },
            { "item_desolator", Color.FromArgb(216, 15, 14) },
            { "item_recipe_desolator", Color.FromArgb(218, 177, 103) },
            { "item_diffusal_blade", Color.FromArgb(112, 44, 199) },
            { "item_diffusal_blade_2", Color.FromArgb(189, 39, 170) },
            { "item_recipe_diffusal_blade", Color.FromArgb(218, 177, 103) },
            { "item_dragon_lance", Color.FromArgb(141, 26, 32) },
            { "item_recipe_dragon_lance", Color.FromArgb(218, 177, 103) },
            { "item_ancient_janggo", Color.FromArgb(168, 186, 97) },
            { "item_recipe_ancient_janggo", Color.FromArgb(218, 177, 103) },
            { "item_ethereal_blade", Color.FromArgb(209, 222, 73) },
            { "item_recipe_ethereal_blade", Color.FromArgb(218, 177, 103) },
            { "item_cyclone", Color.FromArgb(231, 229, 223) },
            { "item_recipe_cyclone", Color.FromArgb(218, 177, 103) },
            { "item_skadi", Color.FromArgb(42, 207, 204) },
            { "item_recipe_skadi", Color.FromArgb(218, 177, 103) },
            { "item_force_staff", Color.FromArgb(123, 225, 187) },
            { "item_recipe_force_staff", Color.FromArgb(218, 177, 103) },
            { "item_glimmer_cape", Color.FromArgb(50, 77, 184) },
            { "item_recipe_glimmer_cape", Color.FromArgb(218, 177, 103) },
            { "item_guardian_greaves", Color.FromArgb(73, 169, 128) },
            { "item_recipe_guardian_greaves", Color.FromArgb(218, 177, 103) },
            { "item_hand_of_midas", Color.FromArgb(222, 154, 21) },
            { "item_recipe_hand_of_midas", Color.FromArgb(218, 177, 103) },
            { "item_headdress", Color.FromArgb(53, 110, 83) },
            { "item_recipe_headdress", Color.FromArgb(218, 177, 103) },
            { "item_heart", Color.FromArgb(196, 51, 31) },
            { "item_recipe_heart", Color.FromArgb(218, 177, 103) },
            { "item_heavens_halberd", Color.FromArgb(235, 240, 218) },
            { "item_recipe_heavens_halberd", Color.FromArgb(218, 177, 103) },
            { "item_helm_of_the_dominator", Color.FromArgb(114, 57, 6) },
            { "item_recipe_helm_of_the_dominator", Color.FromArgb(218, 177, 103) },
            { "item_hood_of_defiance", Color.FromArgb(133, 84, 106) },
            { "item_recipe_hood_of_defiance", Color.FromArgb(218, 177, 103) },
            { "item_iron_talon", Color.FromArgb(200, 151, 70) },
            { "item_recipe_iron_talon", Color.FromArgb(218, 177, 103) },
            { "item_sphere", Color.FromArgb(148, 214, 235) },
            { "item_recipe_sphere", Color.FromArgb(218, 177, 103) },
            { "item_lotus_orb", Color.FromArgb(241, 56, 120) },
            { "item_recipe_lotus_orb", Color.FromArgb(218, 177, 103) },
            { "item_maelstrom", Color.FromArgb(76, 191, 216) },
            { "item_recipe_maelstrom", Color.FromArgb(218, 177, 103) },
            { "item_magic_wand", Color.FromArgb(188, 228, 210) },
            { "item_recipe_magic_wand", Color.FromArgb(218, 177, 103) },
            { "item_manta", Color.FromArgb(133, 180, 223) },
            { "item_recipe_manta", Color.FromArgb(218, 177, 103) },
            { "item_mask_of_madness", Color.FromArgb(185, 74, 51) },
            { "item_recipe_mask_of_madness", Color.FromArgb(218, 177, 103) },
            { "item_medallion_of_courage", Color.FromArgb(209, 173, 108) },
            { "item_recipe_medallion_of_courage", Color.FromArgb(218, 177, 103) },
            { "item_mekansm", Color.FromArgb(167, 221, 211) },
            { "item_recipe_mekansm", Color.FromArgb(218, 177, 103) },
            { "item_mjollnir", Color.FromArgb(245, 250, 225) },
            { "item_recipe_mjollnir", Color.FromArgb(218, 177, 103) },
            { "item_monkey_king_bar", Color.FromArgb(203, 146, 29) },
            { "item_recipe_monkey_king_bar", Color.FromArgb(218, 177, 103) },
            { "item_moon_shard", Color.FromArgb(100, 111, 170) },
            { "item_recipe_moon_shard", Color.FromArgb(218, 177, 103) },
            { "item_necronomicon", Color.FromArgb(25, 130, 117) },
            { "item_necronomicon_2", Color.FromArgb(159, 40, 15) },
            { "item_necronomicon_3", Color.FromArgb(97, 143, 3) },
            { "item_recipe_necronomicon", Color.FromArgb(218, 177, 103) },
            { "item_null_talisman", Color.FromArgb(173, 51, 210) },
            { "item_recipe_null_talisman", Color.FromArgb(218, 177, 103) },
            { "item_oblivion_staff", Color.FromArgb(198, 197, 195) },
            { "item_recipe_oblivion_staff", Color.FromArgb(218, 177, 103) },
            { "item_ward_dispenser", Color.FromArgb(174, 174, 119) },
            { "item_recipe_ward_dispenser", Color.FromArgb(218, 177, 103) },
            { "item_octarine_core", Color.FromArgb(243, 165, 250) },
            { "item_recipe_octarine_core", Color.FromArgb(218, 177, 103) },
            { "item_orchid", Color.FromArgb(249, 190, 174) },
            { "item_recipe_orchid", Color.FromArgb(218, 177, 103) },
            { "item_pers", Color.FromArgb(227, 53, 125) },
            { "item_recipe_pers", Color.FromArgb(218, 177, 103) },
            { "item_phase_boots", Color.FromArgb(147, 67, 240) },
            { "item_recipe_phase_boots", Color.FromArgb(218, 177, 103) },
            { "item_pipe", Color.FromArgb(197, 158, 15) },
            { "item_recipe_pipe", Color.FromArgb(218, 177, 103) },
            { "item_poor_mans_shield", Color.FromArgb(146, 142, 136) },
            { "item_recipe_poor_mans_shield", Color.FromArgb(218, 177, 103) },
            { "item_power_treads", Color.FromArgb(163, 122, 94) },
            { "item_recipe_power_treads", Color.FromArgb(218, 177, 103) },
            { "item_radiance", Color.FromArgb(249, 235, 138) },
            { "item_recipe_radiance", Color.FromArgb(218, 177, 103) },
            { "item_rapier", Color.FromArgb(229, 217, 120) },
            { "item_recipe_rapier", Color.FromArgb(218, 177, 103) },
            { "item_refresher", Color.FromArgb(225, 252, 193) },
            { "item_recipe_refresher", Color.FromArgb(218, 177, 103) },
            { "item_ring_of_aquila", Color.FromArgb(205, 182, 96) },
            { "item_recipe_ring_of_aquila", Color.FromArgb(218, 177, 103) },
            { "item_ring_of_basilius", Color.FromArgb(106, 145, 210) },
            { "item_recipe_ring_of_basilius", Color.FromArgb(218, 177, 103) },
            { "item_rod_of_atos", Color.FromArgb(247, 99, 92) },
            { "item_recipe_rod_of_atos", Color.FromArgb(218, 177, 103) },
            { "item_sange", Color.FromArgb(172, 7, 7) },
            { "item_recipe_sange", Color.FromArgb(218, 177, 103) },
            { "item_sange_and_yasha", Color.FromArgb(244, 38, 251) },
            { "item_recipe_sange_and_yasha", Color.FromArgb(218, 177, 103) },
            { "item_satanic", Color.FromArgb(194, 79, 8) },
            { "item_recipe_satanic", Color.FromArgb(218, 177, 103) },
            { "item_sheepstick", Color.FromArgb(123, 217, 215) },
            { "item_recipe_sheepstick", Color.FromArgb(218, 177, 103) },
            { "item_invis_sword", Color.FromArgb(196, 74, 220) },
            { "item_recipe_invis_sword", Color.FromArgb(218, 177, 103) },
            { "item_shivas_guard", Color.FromArgb(91, 199, 207) },
            { "item_recipe_shivas_guard", Color.FromArgb(218, 177, 103) },
            { "item_silver_edge", Color.FromArgb(250, 177, 249) },
            { "item_recipe_silver_edge", Color.FromArgb(218, 177, 103) },
            { "item_basher", Color.FromArgb(215, 158, 164) },
            { "item_recipe_basher", Color.FromArgb(218, 177, 103) },
            { "item_solar_crest", Color.FromArgb(255, 150, 84) },
            { "item_recipe_solar_crest", Color.FromArgb(218, 177, 103) },
            { "item_soul_booster", Color.FromArgb(252, 211, 232) },
            { "item_recipe_soul_booster", Color.FromArgb(218, 177, 103) },
            { "item_soul_ring", Color.FromArgb(205, 149, 41) },
            { "item_recipe_soul_ring", Color.FromArgb(218, 177, 103) },
            { "item_tranquil_boots", Color.FromArgb(97, 221, 123) },
            { "item_recipe_tranquil_boots", Color.FromArgb(218, 177, 103) },
            { "item_urn_of_shadows", Color.FromArgb(108, 168, 25) },
            { "item_recipe_urn_of_shadows", Color.FromArgb(218, 177, 103) },
            { "item_vanguard", Color.FromArgb(100, 68, 76) },
            { "item_recipe_vanguard", Color.FromArgb(218, 177, 103) },
            { "item_veil_of_discord", Color.FromArgb(159, 0, 148) },
            { "item_recipe_veil_of_discord", Color.FromArgb(218, 177, 103) },
            { "item_vladmir", Color.FromArgb(157, 157, 129) },
            { "item_recipe_vladmir", Color.FromArgb(218, 177, 103) },
            { "item_wraith_band", Color.FromArgb(134, 174, 101) },
            { "item_recipe_wraith_band", Color.FromArgb(218, 177, 103) },
            { "item_yasha", Color.FromArgb(13, 190, 90) },
            { "item_recipe_yasha", Color.FromArgb(218, 177, 103) },
            { "item_halloween_candy_corn", Color.FromArgb(207, 220, 71) },
            { "item_mystery_hook", Color.FromArgb(218, 177, 103) },
            { "item_mystery_arrow", Color.FromArgb(218, 177, 103) },
            { "item_mystery_missile", Color.FromArgb(218, 177, 103) },
            { "item_mystery_toss", Color.FromArgb(218, 177, 103) },
            { "item_mystery_vacuum", Color.FromArgb(218, 177, 103) },
            { "item_halloween_rapier", Color.FromArgb(172, 202, 12) },
            { "item_greevil_whistle_toggle", Color.FromArgb(62, 121, 100) },
            { "item_winter_stocking", Color.FromArgb(191, 121, 101) },
            { "item_winter_skates", Color.FromArgb(152, 91, 69) },
            { "item_winter_cake", Color.FromArgb(201, 190, 181) },
            { "item_winter_cookie", Color.FromArgb(180, 60, 40) },
            { "item_winter_coco", Color.FromArgb(152, 144, 148) },
            { "item_winter_ham", Color.FromArgb(232, 161, 122) },
            { "item_winter_kringle", Color.FromArgb(197, 124, 56) },
            { "item_winter_mushroom", Color.FromArgb(189, 38, 24) },
            { "item_winter_greevil_treat", Color.FromArgb(150, 114, 106) },
            { "item_winter_greevil_garbage", Color.FromArgb(122, 151, 114) },
            { "item_winter_grevil_chewy", Color.FromArgb(157, 148, 104) },
            { "item_greevil_whistle", Color.FromArgb(62, 121, 100) }
        };

        Dictionary<string, Color> bottle_rune_colors = new Dictionary<string, Color>()
        {
            { "empty", Color.FromArgb(35, 155, 185) },
            { "arcane", Color.FromArgb(244, 92, 246) },
            { "bounty", Color.FromArgb(252, 145, 12) },
            { "double_damage", Color.FromArgb(59, 199, 255) },
            { "haste", Color.FromArgb(228, 46, 20) },
            { "illusion", Color.FromArgb(253, 216, 116) },
            { "invisibility", Color.FromArgb(146, 85, 183) },
            { "regen", Color.FromArgb(146, 85, 183) }
        };

        private long previoustime = 0;
        private long currenttime = 0;

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        private AnimationTrack razor_plasma_field_track;
        private AnimationTrack crystal_maiden_crystal_nova_track;
        private AnimationTrack riki_smoke_screen_track;
        private AnimationTrack lina_dragon_slave_track;
        private AnimationTrack lina_light_strike_array_track;
        private AnimationTrack lina_laguna_blade_track;
        private AnimationTrack abaddon_death_coil_track;
        private AnimationTrack nevermore_shadowraze_track;
        private AnimationTrack nevermore_requiem_track;
        private AnimationTrack zuus_arc_lightning_track;
        private AnimationTrack zuus_lightning_bolt_track;
        private AnimationTrack zuus_lightning_bolt_shade_track;
        private AnimationTrack antimage_blink_track;
        private AnimationTrack antimage_mana_void_track;
        private AnimationTrack antimage_mana_void_core_track;
        private AnimationTrack ancient_apparition_ice_blast_track;
        private AnimationTrack axe_berserkers_call_track;
        private AnimationTrack beastmaster_primal_roar_track;
        private AnimationTrack brewmaster_thunder_clap_track;
        private AnimationTrack centaur_hoof_stomp_track;
        private AnimationMix chaos_knight_chaos_bolt_mix;
        private AnimationTrack rattletrap_rocket_flare_track;
        private AnimationTrack dragon_knight_breathe_fire_track;
        private AnimationTrack elder_titan_earth_splitter_track;
        private AnimationMix kunkka_torrent_mix;
        private AnimationTrack kunkka_ghostship_track;
        private AnimationTrack legion_commander_overwhelming_odds_track;
        private AnimationTrack life_stealer_rage_track;
        private AnimationTrack magnataur_shockwave_track;
        private AnimationTrack omniknight_purification_track;
        private AnimationTrack omniknight_repel_track;
        private AnimationMix sandking_epicenter_mix;
        private AnimationTrack slardar_slithereen_crush_track;

        public GameEvent_Dota2()
        {
            profilename = "Dota 2";
            UpdateAnimations();
        }

        public void UpdateAnimations()
        {
            razor_plasma_field_track = new AnimationTrack("Razor Plasma Field", 2.0f);
            razor_plasma_field_track.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 200, 255), 3)
                );
            razor_plasma_field_track.SetFrame(1.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 200, 255), 3)
                );
            razor_plasma_field_track.SetFrame(2.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 200, 255), 3)
                );


            crystal_maiden_crystal_nova_track = new AnimationTrack("CM Crystal Nova", 1.0f);
            crystal_maiden_crystal_nova_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_height / 2.0f, Color.FromArgb(0, 200, 255))
                );
            crystal_maiden_crystal_nova_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f / 2.0f, Color.FromArgb(0, 200, 255))
                );
            crystal_maiden_crystal_nova_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 200, 255))
                );

            riki_smoke_screen_track = new AnimationTrack("Riki Smoke Screen", 6.5f);
            riki_smoke_screen_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_height / 2.0f, Color.FromArgb(163, 70, 255))
                );
            riki_smoke_screen_track.SetFrame(5.525f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f / 2.0f, Color.FromArgb(163, 70, 255))
                );
            riki_smoke_screen_track.SetFrame(6.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 163, 70, 255))
                );

            lina_dragon_slave_track = new AnimationTrack("Lina Dragon Slave", 1.25f);
            lina_dragon_slave_track.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.10f, Color.FromArgb(255, 80, 0))
                );
            lina_dragon_slave_track.SetFrame(0.9375f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f, Color.FromArgb(255, 80, 0))
                );
            lina_dragon_slave_track.SetFrame(1.25f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 80, 0))
                );

            lina_light_strike_array_track = new AnimationTrack("Lina Light Strike", 2.0f);
            lina_light_strike_array_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 255, 80, 0))
                );
            lina_light_strike_array_track.SetFrame(0.49f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 255, 80, 0))
                );
            lina_light_strike_array_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.1f / 2.0f, Color.FromArgb(255, 80, 0))
                );
            lina_light_strike_array_track.SetFrame(1.25f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f / 2.0f, Color.FromArgb(255, 80, 0))
                );
            lina_light_strike_array_track.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 255, 80, 0))
                );

            nevermore_shadowraze_track = new AnimationTrack("Shadow Fiend Raze", 0.7f);
            nevermore_shadowraze_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            nevermore_shadowraze_track.SetFrame(0.595f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.075f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            nevermore_shadowraze_track.SetFrame(0.7f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 255, 0, 0))
                );

            nevermore_requiem_track = new AnimationTrack("Shadow Field Requiem", 2.0f);
            nevermore_requiem_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 0, 0))
                );
            nevermore_requiem_track.SetFrame(1.7f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.85f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            nevermore_requiem_track.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 255, 0, 0))
                );

            zuus_lightning_bolt_track = new AnimationTrack("Zeus Lighting Bolt", 0.5f);
            zuus_lightning_bolt_track.SetFrame(0.0f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(0, 205, 255), 15)
                );
            zuus_lightning_bolt_track.SetFrame(0.425f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(0, 205, 255), 15)
                );
            zuus_lightning_bolt_track.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(0, 0, 205, 255), 15)
                );

            zuus_lightning_bolt_shade_track = new AnimationTrack("Zeus Lighting Bolt Shade", 0.5f);
            zuus_lightning_bolt_shade_track.SetFrame(0.0f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(180, 0, 205, 255), 20)
                );
            zuus_lightning_bolt_shade_track.SetFrame(0.425f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(180, 0, 205, 255), 20)
                );
            zuus_lightning_bolt_shade_track.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(0, 0, 205, 255), 20)
                );

            antimage_blink_track = new AnimationTrack("Anti-mage Blink", 0.5f);
            antimage_blink_track.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(128, 0, 255), 3)
                );
            antimage_blink_track.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(128, 0, 255), 3)
                );

            antimage_mana_void_track = new AnimationTrack("Anti-mage Void", 0.5f);
            antimage_mana_void_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.10f, Color.FromArgb(0, 0, 255))
                );
            antimage_mana_void_track.SetFrame(0.425f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.85f, Color.FromArgb(0, 0, 255))
                );
            antimage_mana_void_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 0, 0, 255))
                );

            antimage_mana_void_core_track = new AnimationTrack("Anti-mage Void Core", 0.5f);
            antimage_mana_void_core_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 255, 255))
                );
            antimage_mana_void_core_track.SetFrame(0.425f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.25f, Color.FromArgb(255, 255, 255))
                );
            antimage_mana_void_core_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(0, 255, 255, 255))
                );

            ancient_apparition_ice_blast_track = new AnimationTrack("AA Ice Blast", 1.0f);
            ancient_apparition_ice_blast_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(200, 200, 255))
                );
            ancient_apparition_ice_blast_track.SetFrame(0.85f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.85f, Color.FromArgb(200, 200, 255))
                );
            ancient_apparition_ice_blast_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 200, 200, 255))
                );

            axe_berserkers_call_track = new AnimationTrack("Axe Berserker", 0.7f);
            axe_berserkers_call_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 50, 0))
                );
            axe_berserkers_call_track.SetFrame(0.595f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.85f, Color.FromArgb(255, 50, 0))
                );
            axe_berserkers_call_track.SetFrame(0.7f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 50, 0))
                );

            beastmaster_primal_roar_track = new AnimationTrack("BM Primal Roar", 1.0f);
            beastmaster_primal_roar_track.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, 0, Color.FromArgb(255, 200, 100))
                );
            beastmaster_primal_roar_track.SetFrame(0.75f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f, Color.FromArgb(255, 200, 100))
                );
            beastmaster_primal_roar_track.SetFrame(1.0f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 200, 100))
                );

            brewmaster_thunder_clap_track = new AnimationTrack("Brewmaster Thunder Clap", 1.5f);
            brewmaster_thunder_clap_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(170, 90, 0))
                );
            brewmaster_thunder_clap_track.SetFrame(0.75f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(170, 90, 0))
                );
            brewmaster_thunder_clap_track.SetFrame(1.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 170, 90, 0))
                );

            centaur_hoof_stomp_track = new AnimationTrack("Centaur Stomp", 1.0f);
            centaur_hoof_stomp_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 50, 0))
                );
            centaur_hoof_stomp_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(255, 50, 0))
                );
            centaur_hoof_stomp_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 50, 0))
                );

            chaos_knight_chaos_bolt_mix = new AnimationMix();

            AnimationTrack chaos_knight_chaos_bolt_projectile_path = new AnimationTrack("Chaos Knight Bolt - Projectile Path", 0.5f);
            chaos_knight_chaos_bolt_projectile_path.SetFrame(0.0f,
                new AnimationLine(new PointF(0, Effects.canvas_height_center), new PointF(0, Effects.canvas_height_center), Color.FromArgb(255, 70, 0), 3)
                );
            chaos_knight_chaos_bolt_projectile_path.SetFrame(0.25f,
                new AnimationLine(new PointF(0, Effects.canvas_height_center), new PointF(Effects.canvas_width_center, Effects.canvas_height_center), Color.FromArgb(255, 70, 0), 3)
                );
            chaos_knight_chaos_bolt_projectile_path.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.canvas_width_center, Effects.canvas_height_center), new PointF(Effects.canvas_width_center, Effects.canvas_height_center), Color.FromArgb(0, 255, 70, 0), 3)
                );
            chaos_knight_chaos_bolt_mix.AddTrack(chaos_knight_chaos_bolt_projectile_path);

            AnimationTrack chaos_knight_chaos_bolt_projectile = new AnimationTrack("Chaos Knight Bolt - Projectile", 0.25f, 0.25f);
            chaos_knight_chaos_bolt_projectile.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(175, 0, 0))
                );
            chaos_knight_chaos_bolt_projectile.SetFrame(0.25f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(0, 175, 0, 0))
                );
            chaos_knight_chaos_bolt_mix.AddTrack(chaos_knight_chaos_bolt_projectile);

            rattletrap_rocket_flare_track = new AnimationTrack("Clockwork Rocket Flare", 0.5f);
            rattletrap_rocket_flare_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 80, 0))
                );
            rattletrap_rocket_flare_track.SetFrame(0.25f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.25f, Color.FromArgb(255, 80, 0))
                );
            rattletrap_rocket_flare_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(0, 255, 80, 0))
                );

            dragon_knight_breathe_fire_track = new AnimationTrack("Dragon Knight Breathe", 1.25f);
            dragon_knight_breathe_fire_track.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.10f, Color.FromArgb(255, 80, 0))
                );
            dragon_knight_breathe_fire_track.SetFrame(0.9375f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f, Color.FromArgb(255, 80, 0))
                );
            dragon_knight_breathe_fire_track.SetFrame(1.25f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 80, 0))
                );

            elder_titan_earth_splitter_track = new AnimationTrack("Elder Titan Earth Splitter", 1.0f, 3.0f);
            elder_titan_earth_splitter_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 255, 220))
                );
            elder_titan_earth_splitter_track.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 0, 255, 220))
                );

            kunkka_torrent_mix = new AnimationMix();
            AnimationTrack kunkka_torrent_bg_track = new AnimationTrack("Kunka Torrent BG", 4.0f);
            kunkka_torrent_bg_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 0, 0))
                );
            kunkka_torrent_bg_track.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 60, 80))
                );
            kunkka_torrent_bg_track.SetFrame(3.6f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 60, 80))
                );
            kunkka_torrent_bg_track.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 0, 60, 80))
                );

            AnimationTrack kunkka_torrent_spash_track = new AnimationTrack("Kunka Torrent Splash", 2.4f, 1.6f);

            kunkka_torrent_spash_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.1f / 2.0f, Color.FromArgb(0, 220, 245))
                );
            kunkka_torrent_spash_track.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.83f / 2.0f, Color.FromArgb(0, 220, 245))
                );
            kunkka_torrent_spash_track.SetFrame(2.4f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 220, 245))
                );

            kunkka_torrent_mix.AddTrack(kunkka_torrent_bg_track);
            kunkka_torrent_mix.AddTrack(kunkka_torrent_spash_track);


            kunkka_ghostship_track = new AnimationTrack("Kunka Ghostship", 2.7f);

            kunkka_ghostship_track.SetFrame(0.0f,
                    new AnimationFilledCircle(-(Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 220, 245))
                    );
            kunkka_ghostship_track.SetFrame(2.3f,
                new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f) * 0.85f, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 220, 245))
                );
            kunkka_ghostship_track.SetFrame(2.7f,
                new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 220, 245))
                );

            legion_commander_overwhelming_odds_track = new AnimationTrack("Legion Commander Overwhelming Odds", 1.0f);
            legion_commander_overwhelming_odds_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 255, 145, 0))
                );
            legion_commander_overwhelming_odds_track.SetFrame(0.3f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 145, 0))
                );
            legion_commander_overwhelming_odds_track.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 145, 0))
                );
            legion_commander_overwhelming_odds_track.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 255, 145, 0))
                );

            life_stealer_rage_track = new AnimationTrack("Life Stealer Rage", 6.0f);
            life_stealer_rage_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(1.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(2.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(2.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(3.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(3.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(4.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(5.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(5.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(6.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );

            magnataur_shockwave_track = new AnimationTrack("Magnataur Shockwave", 1.0f);
            magnataur_shockwave_track.SetFrame(0.0f,
                    new AnimationFilledCircle(-(Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 205, 255))
                    );
            magnataur_shockwave_track.SetFrame(0.9f,
                new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f) * 0.9f, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 205, 255))
                );
            magnataur_shockwave_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 205, 255))
                );

            omniknight_purification_track = new AnimationTrack("Omniknight Purification", 1.0f);
            omniknight_purification_track.SetFrame(0.0f,
                    new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0))
                    );
            omniknight_purification_track.SetFrame(0.8f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0))
                );
            omniknight_purification_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 255, 160, 0))
                );

            omniknight_repel_track = new AnimationTrack("Omniknight Repel", 12.0f);
            omniknight_repel_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(1.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(2.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(2.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(3.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(3.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(4.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(5.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(5.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(6.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(6.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(7.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(7.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(8.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(8.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(9.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(9.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(10.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(10.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(11.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(11.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(12.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );

            sandking_epicenter_mix = new AnimationMix();
            AnimationTrack sandking_epicenter_wave0 = new AnimationTrack("Sandsking Epicenter Wave0", 0.5f);
            sandking_epicenter_wave0.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(115, 255, 0), 4)
                );
            sandking_epicenter_wave0.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(115, 255, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave1 = new AnimationTrack("Sandsking Epicenter Wave1", 0.5f, 2.0f);
            sandking_epicenter_wave1.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave1.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave2 = new AnimationTrack("Sandsking Epicenter Wave2", 0.5f, 2.16f);
            sandking_epicenter_wave2.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave2.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave3 = new AnimationTrack("Sandsking Epicenter Wave3", 0.5f, 2.32f);
            sandking_epicenter_wave3.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave3.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave4 = new AnimationTrack("Sandsking Epicenter Wave4", 0.5f, 2.48f);
            sandking_epicenter_wave4.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave4.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave5 = new AnimationTrack("Sandsking Epicenter Wave5", 0.5f, 2.64f);
            sandking_epicenter_wave5.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave5.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave6 = new AnimationTrack("Sandsking Epicenter Wave6", 0.5f, 2.8f);
            sandking_epicenter_wave6.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave6.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave7 = new AnimationTrack("Sandsking Epicenter Wave7", 0.5f, 2.96f);
            sandking_epicenter_wave7.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave7.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave8 = new AnimationTrack("Sandsking Epicenter Wave8", 0.5f, 3.12f);
            sandking_epicenter_wave8.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave8.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave9 = new AnimationTrack("Sandsking Epicenter Wave9", 0.5f, 3.28f);
            sandking_epicenter_wave9.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave9.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave10 = new AnimationTrack("Sandsking Epicenter Wave10", 0.5f, 3.44f);
            sandking_epicenter_wave10.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave10.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave11 = new AnimationTrack("Sandsking Epicenter Wave11", 0.5f, 3.6f);
            sandking_epicenter_wave11.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave11.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );
            AnimationTrack sandking_epicenter_wave12 = new AnimationTrack("Sandsking Epicenter Wave12", 0.5f, 3.76f);
            sandking_epicenter_wave12.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave12.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave13 = new AnimationTrack("Sandsking Epicenter Wave13", 0.5f, 3.92f);
            sandking_epicenter_wave13.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave13.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave14 = new AnimationTrack("Sandsking Epicenter Wave14", 0.5f, 4.08f);
            sandking_epicenter_wave14.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave14.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave15 = new AnimationTrack("Sandsking Epicenter Wave15", 0.5f, 4.24f);
            sandking_epicenter_wave15.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave15.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave16 = new AnimationTrack("Sandsking Epicenter Wave16", 0.5f, 4.4f);
            sandking_epicenter_wave16.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave16.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave17 = new AnimationTrack("Sandsking Epicenter Wave17", 0.5f, 4.56f);
            sandking_epicenter_wave17.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave17.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave18 = new AnimationTrack("Sandsking Epicenter Wave18", 0.5f, 4.72f);
            sandking_epicenter_wave18.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave18.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave19 = new AnimationTrack("Sandsking Epicenter Wave19", 0.5f, 4.88f);
            sandking_epicenter_wave19.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave19.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave20 = new AnimationTrack("Sandsking Epicenter Wave20", 0.5f, 5f);
            sandking_epicenter_wave20.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave20.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave0);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave1);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave2);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave3);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave4);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave5);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave6);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave7);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave8);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave9);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave10);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave11);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave12);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave13);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave14);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave15);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave16);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave17);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave18);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave19);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave20);


            slardar_slithereen_crush_track = new AnimationTrack("Slardar SMASH!", 0.5f);
            slardar_slithereen_crush_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 150, 255))
                );
            slardar_slithereen_crush_track.SetFrame(0.45f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, (Effects.canvas_biggest / 2.0f) * 0.9f, Color.FromArgb(0, 150, 255))
                );
            slardar_slithereen_crush_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 150, 255))
                );

        }

        public static void SetMapTime(int time)
        {
            GameEvent_Dota2.mapTime = time;
        }

        public static void SetTeam(PlayerTeam team)
        {
            GameEvent_Dota2.current_team = team;
        }

        public static void SetAlive(bool alive)
        {
            GameEvent_Dota2.isAlive = alive;
        }

        public static void SetRespawnTime(int time)
        {
            GameEvent_Dota2.respawnTime = time;
        }

        public static void SetHealth(int health)
        {
            GameEvent_Dota2.health = health;
        }

        public static void SetHealthMax(int health)
        {
            GameEvent_Dota2.health_max = health;
        }

        public static void SetMana(int mana)
        {
            GameEvent_Dota2.mana = mana;
        }

        public static void SetManaMax(int mana)
        {
            GameEvent_Dota2.mana_max = mana;
        }

        public static void SetKillStreak(int streak)
        {
            if (GameEvent_Dota2.player_killstreak != streak && streak > 0 && streak < killstreak_offsets.Length)
                killstreak_offsets[streak] = Effects.canvas_width;

            GameEvent_Dota2.player_killstreak = streak;
        }

        public static void SetKills(int count)
        {
            if (count > GameEvent_Dota2.kills)
                GotAKill();

            GameEvent_Dota2.kills = count;
        }

        public static void SetPlayerActivity(PlayerActivity activity)
        {
            GameEvent_Dota2.current_activity = activity;
        }

        public static void SetAbilities(Abilities_Dota2 abilities)
        {
            GameEvent_Dota2.abilities = abilities;
        }

        public static void SetItems(Items_Dota2 items)
        {
            GameEvent_Dota2.items = items;
        }

        public static void Respawned()
        {
            isDimming = false;
            dim_bg_at = mapTime + (Global.Configuration.ApplicationProfiles["Dota 2"].Settings as Dota2Settings).bg_dim_after;
            dim_value = 1.0;
        }

        public static void GotAKill()
        {
            isPlayingKillStreakAnimation = true;

            ks_end_time = Time.GetMillisecondsSinceEpoch() + ks_duration;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            if (currenttime - previoustime > 300000 || (currenttime == 0 && previoustime == 0))
                UpdateAnimations();

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            if (isPlayingKillStreakAnimation && Time.GetMillisecondsSinceEpoch() >= ks_end_time)
            {
                isPlayingKillStreakAnimation = false;
            }

            //update background
            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).bg_team_enabled)
            {
                EffectLayer bg_layer = new EffectLayer("Dota 2 - Background");

                Color bg_color = (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).ambient_color;

                if (current_team == PlayerTeam.Dire)
                    bg_color = (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).dire_color;
                else if (current_team == PlayerTeam.Radiant)
                    bg_color = (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).radiant_color;

                if (current_team == PlayerTeam.Dire || current_team == PlayerTeam.Radiant)
                {
                    if (dim_bg_at <= mapTime || !isAlive)
                    {
                        isDimming = true;
                        bg_color = Utils.ColorUtils.MultiplyColorByScalar(bg_color, getDimmingValue());
                    }
                    else
                    {
                        isDimming = false;
                        dim_value = 1.0;
                    }

                    if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).bg_respawn_glow && !isAlive)
                    {
                        bg_color = Utils.ColorUtils.BlendColors(bg_color, (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).bg_respawn_glow_color, (respawnTime > 5 ? 0.0 : 1.0 - (respawnTime / 5.0)));
                    }
                }

                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).bg_display_killstreaks && player_killstreak >= 2)
                {
                    Color[] killstreakcolors = (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).bg_killstreakcolors.ToArray();

                    int curr_ks = (player_killstreak > 10 ? 10 : player_killstreak);

                    if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).bg_killstreaks_lines)
                    {
                        for (int str = 2; str <= curr_ks; str++)
                        {
                            killstreak_offsets[str] -= 0.75f;
                            if (killstreak_offsets[str] <= killstreak_line_stop + str * killstreak_line_spaces)
                                killstreak_offsets[str] = killstreak_line_stop + str * killstreak_line_spaces;

                            EffectFunction ks_line = new EffectLine(-1f, killstreak_offsets[str]);

                            EffectColorFunction ks_col_func = new EffectColorFunction(ks_line, new ColorSpectrum(killstreakcolors[str]), 2);

                            bg_layer.AddPostFunction(ks_col_func);
                        }
                    }
                    else
                    {
                        bg_color = Utils.ColorUtils.BlendColors(bg_color, killstreakcolors[curr_ks], getKSEffectValue());
                    }
                }

                bg_layer.Fill(bg_color);

                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).bg_peripheral_use)
                    bg_layer.Set(Devices.DeviceKeys.Peripheral, bg_color);

                layers.Enqueue(bg_layer);
            }

            //Update Ability effects
            EffectLayer ability_effects_layer = new EffectLayer("Dota 2 - Ability Effects");

            if (abiltiyeffect_keyframe >= abilityeffect_time)
            {
                currentabilityeffect = Dota2AbilityEffects.None;
                abiltiyeffect_keyframe = 0.0f;
            }

            float mid_x = Effects.canvas_width / 2.0f;
            float mid_y = Effects.canvas_height / 2.0f;


            if (currentabilityeffect == Dota2AbilityEffects.razor_plasma_field)
            {
                razor_plasma_field_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.crystal_maiden_crystal_nova)
            {
                crystal_maiden_crystal_nova_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.riki_smoke_screen)
            {
                riki_smoke_screen_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.lina_dragon_slave)
            {
                lina_dragon_slave_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.lina_light_strike_array)
            {
                lina_light_strike_array_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.lina_laguna_blade)
            {
                lina_laguna_blade_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();

                //Kept to remember the times without AnimationTracks. The pain was real.
                /*
                if (abiltiyeffect_keyframe == 0.0f)
                {
                    laguna_point1 = new EffectPoint(mid_x, mid_y + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                    laguna_point2 = new EffectPoint(mid_x + 3.0f, mid_y + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                    laguna_point3 = new EffectPoint(mid_x + 6.0f, mid_y + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                    laguna_point4 = new EffectPoint(mid_x + 9.0f, mid_y + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                }

                float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                ColorSpectrum lina_blade_spectrum_step1 = new ColorSpectrum(
                    Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(255, 255, 255), alpha_percent)
                    );
                ColorSpectrum lina_blade_spectrum_step2 = new ColorSpectrum(
                    Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(255, 255, 255), alpha_percent),
                    Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(170, 170, 255), alpha_percent)
                    );
                ColorSpectrum lina_blade_spectrum_step3 = new ColorSpectrum(
                    Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(170, 170, 255), alpha_percent),
                    Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(85, 85, 255), alpha_percent)
                    );
                ColorSpectrum lina_blade_spectrum_step4 = new ColorSpectrum(
                    Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(85, 85, 255), alpha_percent),
                    Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(0, 0, 255), alpha_percent)
                    );


                EffectFunction linefunc1 = new EffectLine(new EffectPoint(0, mid_y), laguna_point1, true);
                EffectColorFunction line1 = new EffectColorFunction(linefunc1, lina_blade_spectrum_step1, 2);
                ability_effects_layer.AddPostFunction(line1);

                EffectFunction linefunc2 = new EffectLine(laguna_point1, laguna_point2, true);
                EffectColorFunction line2 = new EffectColorFunction(linefunc2, lina_blade_spectrum_step2, 3);
                ability_effects_layer.AddPostFunction(line2);

                EffectFunction linefunc3 = new EffectLine(laguna_point2, laguna_point3, true);
                EffectColorFunction line3 = new EffectColorFunction(linefunc3, lina_blade_spectrum_step3, 3);
                ability_effects_layer.AddPostFunction(line3);

                EffectFunction linefunc4 = new EffectLine(laguna_point3, laguna_point4, true);
                EffectColorFunction line4 = new EffectColorFunction(linefunc4, lina_blade_spectrum_step4, 5);
                ability_effects_layer.AddPostFunction(line4);
                */
            }
            else if (currentabilityeffect == Dota2AbilityEffects.abaddon_death_coil)
            {
                abaddon_death_coil_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.abaddon_borrowed_time)
            {
                float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                float fluctuatiuons = (float)Math.Sin(((double)progress) * Math.PI * 1.0f) + 0.75f;

                Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(0, 205, 255), fluctuatiuons * alpha_percent);

                ability_effects_layer.Fill(color);
            }
            else if (currentabilityeffect == Dota2AbilityEffects.nevermore_shadowraze)
            {
                nevermore_shadowraze_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.nevermore_requiem)
            {
                nevermore_requiem_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.zuus_arc_lightning)
            {
                zuus_arc_lightning_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.zuus_lightning_bolt)
            {
                zuus_lightning_bolt_shade_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                zuus_lightning_bolt_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.zuus_thundergods_wrath)
            {
                float x_offset = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                float alpha_percent = (x_offset >= 0.85f ? 1.0f + (1.0f - (1.17f) * x_offset) : 1.0f);

                ability_effects_layer.Fill(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(0, 205, 255), alpha_percent));
            }
            else if (currentabilityeffect == Dota2AbilityEffects.antimage_blink)
            {
                antimage_blink_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.antimage_mana_void)
            {
                antimage_mana_void_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                antimage_mana_void_core_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.ancient_apparition_ice_vortex)
            {
                float x_offset = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                float alpha_percent = (x_offset >= 0.85f ? 1.0f + (1.0f - (1.17f) * x_offset) : 1.0f);

                ability_effects_layer.Fill(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 200, 255), alpha_percent));
            }
            else if (currentabilityeffect == Dota2AbilityEffects.ancient_apparition_ice_blast)
            {
                ancient_apparition_ice_blast_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.alchemist_acid_spray)
            {
                float alpha_percent = (float)Math.Pow(Math.Sin(((double)abiltiyeffect_keyframe / (abilityeffect_time / 16)) * Math.PI), 2.0);

                ability_effects_layer.Fill(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(140, 160, 0), (alpha_percent < 0.2f ? 0.2f : alpha_percent)));

                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.axe_berserkers_call)
            {
                axe_berserkers_call_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.beastmaster_primal_roar)
            {
                beastmaster_primal_roar_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.brewmaster_thunder_clap)
            {
                brewmaster_thunder_clap_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.centaur_hoof_stomp)
            {
                centaur_hoof_stomp_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.chaos_knight_chaos_bolt)
            {
                chaos_knight_chaos_bolt_mix.Draw(ability_effects_layer.GetGraphics(), abiltiyeffect_keyframe);
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.rattletrap_rocket_flare)
            {
                rattletrap_rocket_flare_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.doom_bringer_scorched_earth)
            {
                float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                float fluctuatiuons = (float)Math.Sin(((double)progress) * Math.PI * 1.0f) + 0.75f;

                Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuatiuons * alpha_percent);

                ability_effects_layer.Fill(color);
            }
            else if (currentabilityeffect == Dota2AbilityEffects.dragon_knight_breathe_fire)
            {
                dragon_knight_breathe_fire_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.earthshaker_fissure)
            {
                float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                float fluctuatiuons = (float)Math.Sin(((double)progress) * Math.PI * 1.0f) + 0.75f;

                Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuatiuons * alpha_percent);

                ability_effects_layer.Fill(color);
            }
            else if (currentabilityeffect == Dota2AbilityEffects.earthshaker_echo_slam)
            {
                float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                float fluctuatiuons = (float)Math.Sin(((double)progress) * Math.PI * 1.0f) + 0.75f;

                Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuatiuons * alpha_percent);

                ability_effects_layer.Fill(color);
            }
            else if (currentabilityeffect == Dota2AbilityEffects.elder_titan_earth_splitter)
            {
                elder_titan_earth_splitter_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.kunkka_torrent)
            {
                kunkka_torrent_mix.Draw(ability_effects_layer.GetGraphics(), abiltiyeffect_keyframe);
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.kunkka_ghostship)
            {
                kunkka_ghostship_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.legion_commander_overwhelming_odds)
            {
                legion_commander_overwhelming_odds_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.life_stealer_rage)
            {
                life_stealer_rage_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.magnataur_shockwave)
            {
                magnataur_shockwave_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.omniknight_purification)
            {
                omniknight_purification_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.omniknight_repel)
            {
                omniknight_repel_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.sandking_epicenter)
            {
                sandking_epicenter_mix.Draw(ability_effects_layer.GetGraphics(), abiltiyeffect_keyframe);
                abiltiyeffect_keyframe += getDeltaTime();
            }
            else if (currentabilityeffect == Dota2AbilityEffects.slardar_slithereen_crush)
            {
                slardar_slithereen_crush_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                abiltiyeffect_keyframe += getDeltaTime();
            }

            layers.Enqueue(ability_effects_layer);

            //Not initialized
            if (current_team != PlayerTeam.Undefined && current_team != PlayerTeam.None)
            {
                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mimic_respawn_timer && !isAlive)
                {
                    EffectLayer mimic_respawn_layer = new EffectLayer("Dota 2 - Mimic Respawn");

                    //Update Health
                    if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).health_enabled)
                    {
                        mimic_respawn_layer.PercentEffect((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mimic_respawn_timer_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mimic_respawn_timer_respawning_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).health_sequence,
                            (double)(respawnTime > 4 ? 5.0 : respawnTime),
                            4.0,
                            PercentEffectType.AllAtOnce);
                    }
                    //Update Mana
                    if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mana_enabled)
                    {
                        mimic_respawn_layer.PercentEffect((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mimic_respawn_timer_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mimic_respawn_timer_respawning_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mana_sequence,
                            (double)(respawnTime > 4 ? 5.0 : respawnTime),
                            4.0,
                            PercentEffectType.AllAtOnce);
                    }

                    layers.Enqueue(mimic_respawn_layer);
                }
                else
                {


                    //Update Health
                    if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).health_enabled)
                    {
                        EffectLayer hpbar_layer = new EffectLayer("Dota 2 - HP Bar");

                        hpbar_layer.PercentEffect((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).healthy_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).hurt_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).health_sequence,
                            (double)health,
                            (double)health_max,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).health_effect_type);

                        layers.Enqueue(hpbar_layer);
                    }

                    //Update Mana
                    if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mana_enabled)
                    {
                        EffectLayer manabar_layer = new EffectLayer("Dota 2 - ManaBar");

                        manabar_layer.PercentEffect((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mana_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).nomana_color,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mana_sequence,
                            (double)mana,
                            (double)mana_max,
                            (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).mana_effect_type);

                        layers.Enqueue(manabar_layer);
                    }
                }


                //Abilities
                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).abilitykeys_enabled && abilities != null && (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).ability_keys.Count >= 6)
                {
                    EffectLayer abilities_layer = new EffectLayer("Dota 2 - Abilities");

                    for (int index = 0; index < abilities.Count; index++)
                    {
                        Ability ability = abilities[index];
                        Devices.DeviceKeys key = (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).ability_keys[index];

                        if (ability.IsUltimate)
                            key = (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).ability_keys[5];

                        if (ability.CanCast && ability.Cooldown == 0 && ability.Level > 0)
                        {
                            abilities_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).ability_can_use_color);
                        }
                        else if (ability.Cooldown <= 5 && ability.Level > 0)
                        {
                            abilities_layer.Set(key, Utils.ColorUtils.BlendColors((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).ability_can_use_color, (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).ability_can_not_use_color, (double)ability.Cooldown / 5.0));
                        }
                        else
                        {
                            abilities_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).ability_can_not_use_color);
                        }
                    }

                    layers.Enqueue(abilities_layer);
                }

                //Items
                if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_enabled && items != null && (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_keys.Count >= 6)
                {
                    EffectLayer items_layer = new EffectLayer("Dota 2 - Items");

                    for (int index = 0; index < items.CountInventory; index++)
                    {
                        Item item = items.GetInventoryAt(index);
                        Devices.DeviceKeys key = (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_keys[index];

                        if (item.Name.Equals("empty"))
                        {
                            items_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_empty_color);
                        }
                        else
                        {
                            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_use_item_color && item_colors.ContainsKey(item.Name))
                            {
                                if (!String.IsNullOrWhiteSpace(item.ContainsRune))
                                {
                                    items_layer.Set(key, Utils.ColorUtils.BlendColors(item_colors[item.Name], bottle_rune_colors[item.ContainsRune], 0.8));
                                }
                                else
                                {
                                    items_layer.Set(key, item_colors[item.Name]);
                                }
                            }
                            else
                            {
                                items_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_color);
                            }

                            //Cooldown
                            if (item.Cooldown > 5)
                            {
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_on_cooldown_color, 1.0));
                            }
                            else if (item.Cooldown > 0 && item.Cooldown <= 5)
                            {
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_on_cooldown_color, item.Cooldown / 5.0));
                            }

                            //Charges
                            if (item.Charges == 0)
                            {
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_no_charges_color, 0.7));
                            }
                        }
                    }

                    for (int index = 0; index < items.CountStash; index++)
                    {
                        Item item = items.GetStashAt(index);
                        Devices.DeviceKeys key = (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_keys[6 + index];

                        if (item.Name.Equals("empty"))
                        {
                            items_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_empty_color);
                        }
                        else
                        {
                            if ((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_use_item_color && item_colors.ContainsKey(item.Name))
                            {
                                if (!String.IsNullOrWhiteSpace(item.ContainsRune))
                                {
                                    items_layer.Set(key, Utils.ColorUtils.BlendColors(item_colors[item.Name], bottle_rune_colors[item.ContainsRune], 0.8));
                                }
                                else
                                {
                                    items_layer.Set(key, item_colors[item.Name]);
                                }
                            }
                            else
                            {
                                items_layer.Set(key, (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_color);
                            }

                            //Cooldown
                            if (item.Cooldown > 5)
                            {
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_on_cooldown_color, 1.0));
                            }
                            else if (item.Cooldown > 0 && item.Cooldown <= 5)
                            {
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_on_cooldown_color, item.Cooldown / 5.0));
                            }

                            //Charges
                            if (item.Charges == 0)
                            {
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).items_no_charges_color, 0.7));
                            }
                        }
                    }

                    //Scripts
                    Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

                    layers.Enqueue(items_layer);
                }

            }

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Dota 2 - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            frame.AddLayers(layers.ToArray());
        }

        private double getDimmingValue()
        {
            if (isDimming && (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).bg_enable_dimming)
            {
                dim_value -= 0.02;
                return dim_value = (dim_value < 0.0 ? 0.0 : dim_value);
            }
            else
                return dim_value = 1.0;
        }

        private double getKSEffectValue()
        {
            if (isPlayingKillStreakAnimation)
            {
                ks_blendamount += 0.15;
                return ks_blendamount = (ks_blendamount > 1.0 ? 1.0 : ks_blendamount);
            }
            else
            {
                ks_blendamount -= 0.15;
                return ks_blendamount = (ks_blendamount < 0.0 ? 0.0 : ks_blendamount);
            }

        }

        public override void UpdateLights(EffectFrame frame, GameState new_game_state)
        {
            if (new_game_state is GameState_Dota2)
            {
                _game_state = new_game_state;

                GameState_Dota2 newgs = (new_game_state as GameState_Dota2);

                try
                {
                    if (abilities != null && newgs.Abilities.Count == abilities.Count)
                    {
                        for (int ability_id = 0; ability_id < newgs.Abilities.Count; ability_id++)
                        {
                            Ability ability = newgs.Abilities[ability_id];

                            if (!ability.CanCast && abilities[ability_id].CanCast)
                            {
                                //Casted ability
                                if (ability.Name.Equals("razor_plasma_field"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.razor_plasma_field;
                                    abilityeffect_time = 2.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("crystal_maiden_crystal_nova"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.crystal_maiden_crystal_nova;
                                    abilityeffect_time = 1.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("riki_smoke_screen"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.riki_smoke_screen;
                                    abilityeffect_time = 6.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("lina_dragon_slave"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.lina_dragon_slave;
                                    abilityeffect_time = 1.25f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("lina_light_strike_array"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.lina_light_strike_array;
                                    abilityeffect_time = 2.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("lina_laguna_blade"))
                                {
                                    lina_laguna_blade_track = new AnimationTrack("Lina Laguna Blade", 0.5f);

                                    PointF laguna_point1 = new PointF(0, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF laguna_point2 = new PointF(Effects.canvas_width_center + 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF laguna_point3 = new PointF(Effects.canvas_width_center + 6.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF laguna_point4 = new PointF(Effects.canvas_width_center + 9.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));

                                    lina_laguna_blade_track.SetFrame(0.0f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), laguna_point1, Color.FromArgb(255, 255, 255), 2),
                                            new AnimationLine(laguna_point1, laguna_point2, Color.FromArgb(255, 255, 255), Color.FromArgb(170, 170, 255), 3),
                                            new AnimationLine(laguna_point2, laguna_point3, Color.FromArgb(170, 170, 255), Color.FromArgb(85, 85, 255), 3),
                                            new AnimationLine(laguna_point3, laguna_point4, Color.FromArgb(85, 85, 255), Color.FromArgb(0, 0, 255), 5),
                                            }
                                            )
                                        );

                                    lina_laguna_blade_track.SetFrame(0.45f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), laguna_point1, Color.FromArgb(255, 255, 255), 2),
                                            new AnimationLine(laguna_point1, laguna_point2, Color.FromArgb(255, 255, 255), Color.FromArgb(170, 170, 255), 3),
                                            new AnimationLine(laguna_point2, laguna_point3, Color.FromArgb(170, 170, 255), Color.FromArgb(85, 85, 255), 3),
                                            new AnimationLine(laguna_point3, laguna_point4, Color.FromArgb(85, 85, 255), Color.FromArgb(0, 0, 255), 5),
                                            }
                                            )
                                        );

                                    lina_laguna_blade_track.SetFrame(0.5f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), laguna_point1, Color.FromArgb(0, 255, 255, 255), 2),
                                            new AnimationLine(laguna_point1, laguna_point2, Color.FromArgb(0, 255, 255, 255), Color.FromArgb(0, 170, 170, 255), 3),
                                            new AnimationLine(laguna_point2, laguna_point3, Color.FromArgb(0, 170, 170, 255), Color.FromArgb(0, 85, 85, 255), 3),
                                            new AnimationLine(laguna_point3, laguna_point4, Color.FromArgb(0, 85, 85, 255), Color.FromArgb(0, 0, 0, 255), 5),
                                            }
                                            )
                                        );

                                    currentabilityeffect = Dota2AbilityEffects.lina_laguna_blade;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("abaddon_death_coil"))
                                {
                                    abaddon_death_coil_track = new AnimationTrack("Abaddon Dealth Coil", 0.5f);

                                    PointF death_coil_point1 = new PointF(Effects.canvas_width_center - 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF death_coil_point2 = new PointF(Effects.canvas_width_center, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF death_coil_point3 = new PointF(Effects.canvas_width_center + 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF death_coil_point4 = new PointF(Effects.canvas_width_center + 9.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));

                                    abaddon_death_coil_track.SetFrame(0.0f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), death_coil_point1, Color.FromArgb(0, 160, 210), 2),
                                            new AnimationLine(death_coil_point1, death_coil_point2, Color.FromArgb(0, 160, 210), 3),
                                            new AnimationLine(death_coil_point2, death_coil_point3, Color.FromArgb(0, 160, 210), 3),
                                            new AnimationLine(death_coil_point3, death_coil_point4, Color.FromArgb(0, 160, 210), 5),
                                            }
                                            )
                                        );

                                    abaddon_death_coil_track.SetFrame(0.45f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), death_coil_point1, Color.FromArgb(0, 160, 210), 2),
                                            new AnimationLine(death_coil_point1, death_coil_point2, Color.FromArgb(0, 160, 210), 3),
                                            new AnimationLine(death_coil_point2, death_coil_point3, Color.FromArgb(0, 160, 210), 3),
                                            new AnimationLine(death_coil_point3, death_coil_point4, Color.FromArgb(0, 160, 210), 5),
                                            }
                                            )
                                        );

                                    abaddon_death_coil_track.SetFrame(0.5f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), death_coil_point1, Color.FromArgb(0, 0, 160, 210), 2),
                                            new AnimationLine(death_coil_point1, death_coil_point2, Color.FromArgb(0, 0, 160, 210), 3),
                                            new AnimationLine(death_coil_point2, death_coil_point3, Color.FromArgb(0, 0, 160, 210), 3),
                                            new AnimationLine(death_coil_point3, death_coil_point4, Color.FromArgb(0, 0, 160, 210), 5),
                                            }
                                            )
                                        );

                                    currentabilityeffect = Dota2AbilityEffects.abaddon_death_coil;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("abaddon_borrowed_time"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.abaddon_borrowed_time;

                                    if (ability.Level == 1)
                                        abilityeffect_time = 4.0f;
                                    else if (ability.Level == 2)
                                        abilityeffect_time = 5.0f;
                                    else if (ability.Level == 3)
                                        abilityeffect_time = 6.0f;
                                    else
                                        abilityeffect_time = 6.0f;

                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("nevermore_shadowraze1") || ability.Name.Equals("nevermore_shadowraze2") || ability.Name.Equals("nevermore_shadowraze3"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.nevermore_shadowraze;
                                    abilityeffect_time = 0.7f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("nevermore_requiem"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.nevermore_requiem;
                                    abilityeffect_time = 2.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("zuus_arc_lightning"))
                                {
                                    zuus_arc_lightning_track = new AnimationTrack("Zeus Arc Lightning", 0.5f);

                                    PointF zuus_lightning_point1 = new PointF(Effects.canvas_width_center - 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF zuus_lightning_point2 = new PointF(Effects.canvas_width_center, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF zuus_lightning_point3 = new PointF(Effects.canvas_width_center + 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                    PointF zuus_lightning_point4 = new PointF(Effects.canvas_width_center + 9.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));

                                    zuus_arc_lightning_track.SetFrame(0.0f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), zuus_lightning_point1, Color.FromArgb(0, 205, 255), 2),
                                            new AnimationLine(zuus_lightning_point1, zuus_lightning_point2, Color.FromArgb(0, 205, 255), 3),
                                            new AnimationLine(zuus_lightning_point2, zuus_lightning_point3, Color.FromArgb(0, 205, 255) , 3),
                                            new AnimationLine(zuus_lightning_point3, zuus_lightning_point4, Color.FromArgb(0, 205, 255), 5),
                                            }
                                            )
                                        );

                                    zuus_arc_lightning_track.SetFrame(0.45f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), zuus_lightning_point1, Color.FromArgb(0, 205, 255), 2),
                                            new AnimationLine(zuus_lightning_point1, zuus_lightning_point2, Color.FromArgb(0, 205, 255), 3),
                                            new AnimationLine(zuus_lightning_point2, zuus_lightning_point3, Color.FromArgb(0, 205, 255) , 3),
                                            new AnimationLine(zuus_lightning_point3, zuus_lightning_point4, Color.FromArgb(0, 205, 255), 5),
                                            }
                                            )
                                        );

                                    zuus_arc_lightning_track.SetFrame(0.5f,
                                        new AnimationLines(
                                            new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), zuus_lightning_point1, Color.FromArgb(0, 0, 205, 255), 2),
                                            new AnimationLine(zuus_lightning_point1, zuus_lightning_point2, Color.FromArgb(0, 0, 205, 255), 3),
                                            new AnimationLine(zuus_lightning_point2, zuus_lightning_point3, Color.FromArgb(0, 0, 205, 255), 3),
                                            new AnimationLine(zuus_lightning_point3, zuus_lightning_point4, Color.FromArgb(0, 0, 205, 255), 5),
                                            }
                                            )
                                        );

                                    currentabilityeffect = Dota2AbilityEffects.zuus_arc_lightning;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("zuus_lightning_bolt"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.zuus_lightning_bolt;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("zuus_thundergods_wrath"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.zuus_thundergods_wrath;
                                    abilityeffect_time = 0.25f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("antimage_blink"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.antimage_blink;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("antimage_mana_void"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.antimage_mana_void;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("ancient_apparition_ice_vortex"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.ancient_apparition_ice_vortex;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("ancient_apparition_ice_blast"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.ancient_apparition_ice_blast;
                                    abilityeffect_time = 1.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("alchemist_acid_spray"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.alchemist_acid_spray;
                                    abilityeffect_time = 16.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("axe_berserkers_call"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.axe_berserkers_call;
                                    abilityeffect_time = 0.7f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("beastmaster_primal_roar"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.beastmaster_primal_roar;
                                    abilityeffect_time = 1.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("brewmaster_thunder_clap"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.brewmaster_thunder_clap;
                                    abilityeffect_time = 1.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("centaur_hoof_stomp"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.centaur_hoof_stomp;
                                    abilityeffect_time = 1.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("chaos_knight_chaos_bolt"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.chaos_knight_chaos_bolt;
                                    abilityeffect_time = 1.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("rattletrap_rocket_flare"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.rattletrap_rocket_flare;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("doom_bringer_scorched_earth"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.doom_bringer_scorched_earth;

                                    if (ability.Level == 2)
                                        abilityeffect_time = 12.0f;
                                    else if (ability.Level == 3)
                                        abilityeffect_time = 14.0f;
                                    else if (ability.Level == 4)
                                        abilityeffect_time = 16.0f;
                                    else
                                        abilityeffect_time = 10.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("dragon_knight_breathe_fire"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.dragon_knight_breathe_fire;
                                    abilityeffect_time = 1.25f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("earthshaker_fissure"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.earthshaker_fissure;
                                    abilityeffect_time = 0.25f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("earthshaker_echo_slam"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.earthshaker_echo_slam;
                                    abilityeffect_time = 0.25f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("elder_titan_earth_splitter"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.elder_titan_earth_splitter;
                                    abilityeffect_time = 4.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("kunkka_torrent"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.kunkka_torrent;
                                    abilityeffect_time = 4.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("kunkka_ghostship"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.kunkka_ghostship;
                                    abilityeffect_time = 2.7f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("legion_commander_overwhelming_odds"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.legion_commander_overwhelming_odds;
                                    abilityeffect_time = 1.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("life_stealer_rage"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.life_stealer_rage;

                                    if (ability.Level == 1)
                                        abilityeffect_time = 3.0f;
                                    else if (ability.Level == 2)
                                        abilityeffect_time = 4.0f;
                                    else if (ability.Level == 3)
                                        abilityeffect_time = 5.0f;
                                    if (ability.Level == 4)
                                        abilityeffect_time = 6.0f;

                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("magnataur_shockwave"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.magnataur_shockwave;
                                    abilityeffect_time = 1.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("omniknight_purification"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.omniknight_purification;
                                    abilityeffect_time = 1.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("omniknight_repel"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.omniknight_repel;

                                    if (ability.Level == 1)
                                        abilityeffect_time = 6.0f;
                                    else if (ability.Level == 2)
                                        abilityeffect_time = 8.0f;
                                    else if (ability.Level == 3)
                                        abilityeffect_time = 10.0f;
                                    if (ability.Level == 4)
                                        abilityeffect_time = 12.0f;

                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("sandking_epicenter"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.sandking_epicenter;
                                    abilityeffect_time = 5.0f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else if (ability.Name.Equals("slardar_slithereen_crush"))
                                {
                                    currentabilityeffect = Dota2AbilityEffects.slardar_slithereen_crush;
                                    abilityeffect_time = 0.5f;
                                    abiltiyeffect_keyframe = 0.0f;
                                }
                                else
                                {
                                    if (Global.isDebug)
                                        System.Diagnostics.Debug.WriteLine("Unknown Ability: " + ability.Name);
                                }

                            }
                        }
                    }

                    SetMapTime(newgs.Map.GameTime);
                    SetTeam(newgs.Player.Team);
                    SetAlive(newgs.Hero.IsAlive);
                    SetRespawnTime(newgs.Hero.SecondsToRespawn);
                    SetHealth(newgs.Hero.Health);
                    SetHealthMax(newgs.Hero.MaxHealth);
                    SetMana(newgs.Hero.Mana);
                    SetManaMax(newgs.Hero.MaxMana);
                    SetPlayerActivity(newgs.Player.Activity);
                    SetAbilities(newgs.Abilities);
                    SetItems(newgs.Items);
                    SetKillStreak(newgs.Player.KillStreak);
                    SetKills(newgs.Player.Kills);


                    if (newgs.Previously.Hero.HealthPercent == 0 && newgs.Hero.HealthPercent == 100 && !newgs.Previously.Hero.IsAlive && newgs.Hero.IsAlive)
                    {
                        Respawned();
                    }

                    if (newgs.Previously.Player.Kills != -1 && newgs.Previously.Player.Kills < newgs.Player.Kills)
                    {
                        GotAKill();
                    }

                }
                catch (Exception e)
                {
                    Global.logger.LogLine("Exception during OnNewGameState. Error: " + e, Logging_Level.Error);
                    Global.logger.LogLine(newgs.ToString(), Logging_Level.None);
                }

                UpdateLights(frame);
            }
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as Dota2Settings).isEnabled;
        }
    }
}
