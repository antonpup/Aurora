using Aurora.EffectsEngine;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.Dota_2.Layers
{
    public class Dota2ItemLayerHandlerProperties : LayerHandlerProperties2Color<Dota2ItemLayerHandlerProperties>
    {
        public Color? _EmptyItemColor { get; set; }

        [JsonIgnore]
        public Color EmptyItemColor { get { return Logic._EmptyItemColor ?? _EmptyItemColor ?? Color.Empty; } }

        public Color? _ItemsColor { get; set; }

        [JsonIgnore]
        public Color ItemsColor { get { return Logic._ItemsColor ?? _ItemsColor ?? Color.Empty; } }

        public bool? _UseItemColors { get; set; }

        [JsonIgnore]
        public bool UseItemColors { get { return Logic._UseItemColors ?? _UseItemColors ?? false; } }

        public Color? _ItemCooldownColor { get; set; }

        [JsonIgnore]
        public Color ItemCooldownColor { get { return Logic._ItemCooldownColor ?? _ItemCooldownColor ?? Color.Empty; } }

        public Color? _ItemNoChargersColor { get; set; }

        [JsonIgnore]
        public Color ItemNoChargersColor { get { return Logic._ItemNoChargersColor ?? _ItemNoChargersColor ?? Color.Empty; } }

        public List<Devices.DeviceKeys> _ItemKeys { get; set; }

        [JsonIgnore]
        public List<Devices.DeviceKeys> ItemKeys { get { return Logic._ItemKeys ?? _ItemKeys ?? new List<Devices.DeviceKeys>(); } }

        public Dota2ItemLayerHandlerProperties() : base() { }

        public Dota2ItemLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._EmptyItemColor = Color.FromArgb(0, 0, 0);
            this._ItemsColor = Color.FromArgb(255, 255, 255);
            this._ItemCooldownColor = Color.FromArgb(0, 0, 0);
            this._ItemNoChargersColor = Color.FromArgb(150, 150, 150);
            this._UseItemColors = true;
            this._ItemKeys = new List<Devices.DeviceKeys>() { Devices.DeviceKeys.Z, Devices.DeviceKeys.X, Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.B, Devices.DeviceKeys.N, Devices.DeviceKeys.INSERT, Devices.DeviceKeys.HOME, Devices.DeviceKeys.PAGE_UP, Devices.DeviceKeys.DELETE, Devices.DeviceKeys.END, Devices.DeviceKeys.PAGE_DOWN };
        }

    }

    public class Dota2ItemLayerHandler : LayerHandler<Dota2ItemLayerHandlerProperties>
    {
        private static Dictionary<string, Color> item_colors = new Dictionary<string, Color>()
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

        private static Dictionary<string, Color> bottle_rune_colors = new Dictionary<string, Color>()
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

        protected override UserControl CreateControl()
        {
            return new Control_Dota2ItemLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer items_layer = new EffectLayer("Dota 2 - Items");

            if (state is GameState_Dota2)
            {
                GameState_Dota2 dota2state = state as GameState_Dota2;

                if (Properties.ItemKeys.Count >= 6)
                {
                    for (int index = 0; index < dota2state.Items.InventoryCount; index++)
                    {
                        Item item = dota2state.Items.GetInventoryAt(index);
                        Devices.DeviceKeys key = Properties.ItemKeys[index];

                        if (item.Name.Equals("empty"))
                            items_layer.Set(key, Properties.EmptyItemColor);
                        else
                        {
                            if (Properties.UseItemColors && item_colors.ContainsKey(item.Name))
                            {
                                if (!String.IsNullOrWhiteSpace(item.ContainsRune))
                                    items_layer.Set(key, Utils.ColorUtils.BlendColors(item_colors[item.Name], bottle_rune_colors[item.ContainsRune], 0.8));
                                else
                                    items_layer.Set(key, item_colors[item.Name]);
                            }
                            else
                                items_layer.Set(key, Properties.ItemsColor);

                            //Cooldown
                            if (item.Cooldown > 5)
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), Properties.ItemCooldownColor, 1.0));
                            else if (item.Cooldown > 0 && item.Cooldown <= 5)
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), Properties.ItemCooldownColor, item.Cooldown / 5.0));

                            //Charges
                            if (item.Charges == 0)
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), Properties.ItemNoChargersColor, 0.7));
                        }
                    }

                    for (int index = 0; index < dota2state.Items.StashCount; index++)
                    {
                        Item item = dota2state.Items.GetStashAt(index);
                        Devices.DeviceKeys key = Properties.ItemKeys[6 + index];

                        if (item.Name.Equals("empty"))
                        {
                            items_layer.Set(key, Properties.EmptyItemColor);
                        }
                        else
                        {
                            if (Properties.UseItemColors && item_colors.ContainsKey(item.Name))
                            {
                                if (!String.IsNullOrWhiteSpace(item.ContainsRune))
                                    items_layer.Set(key, Utils.ColorUtils.BlendColors(item_colors[item.Name], bottle_rune_colors[item.ContainsRune], 0.8));
                                else
                                    items_layer.Set(key, item_colors[item.Name]);
                            }
                            else
                                items_layer.Set(key, Properties.ItemsColor);

                            //Cooldown
                            if (item.Cooldown > 5)
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), Properties.ItemCooldownColor, 1.0));
                            else if (item.Cooldown > 0 && item.Cooldown <= 5)
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), Properties.ItemCooldownColor, item.Cooldown / 5.0));

                            //Charges
                            if (item.Charges == 0)
                                items_layer.Set(key, Utils.ColorUtils.BlendColors(items_layer.Get(key), Properties.ItemNoChargersColor, 0.7));
                        }
                    }
                }
            }

            return items_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2ItemLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
