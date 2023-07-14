using System.Collections.Generic;

namespace Aurora.Settings;

public static class PeripheralLayoutMap
{
    public static readonly IReadOnlyDictionary<PreferredKeyboard, string> KeyboardLayoutMap = new Dictionary<PreferredKeyboard, string>
    {
        {PreferredKeyboard.Logitech_G910, "logitech_g910.json"},
        {PreferredKeyboard.Logitech_G810, "logitech_g810.json"},
        {PreferredKeyboard.Logitech_GPRO, "logitech_gpro.json"},
        {PreferredKeyboard.Logitech_G410, "logitech_g410.json"},
        {PreferredKeyboard.Logitech_G815, "logitech_g815.json"},
        {PreferredKeyboard.Logitech_G513, "logitech_g513.json"},
        {PreferredKeyboard.Logitech_G213, "logitech_g213.json"},
        {PreferredKeyboard.Logitech_G915, "logitech_g915.json"},
        {PreferredKeyboard.Corsair_K95, "corsair_k95.json"},
        {PreferredKeyboard.Corsair_K95_PL, "corsair_k95_platinum.json"},
        {PreferredKeyboard.Corsair_K70, "corsair_k70.json"},
        {PreferredKeyboard.Corsair_K70MK2, "corsair_k70_mk2.json"},
        {PreferredKeyboard.Corsair_K70_PRO, "corsair_k70_pro.json"},
        {PreferredKeyboard.Corsair_K65, "corsair_k65.json"},
        {PreferredKeyboard.Corsair_STRAFE, "corsair_strafe.json"},
        {PreferredKeyboard.Corsair_STRAFE_MK2, "corsair_strafe_mk2.json"},
        {PreferredKeyboard.Corsair_K68, "corsair_k68.json"},
        {PreferredKeyboard.Corsair_K100, "corsair_k100.json"},
        {PreferredKeyboard.Razer_Blackwidow, "razer_blackwidow.json"},
        {PreferredKeyboard.Razer_Blackwidow_X, "razer_blackwidow_x.json"},
        {PreferredKeyboard.Razer_Blackwidow_TE, "razer_blackwidow_te.json"},
        {PreferredKeyboard.Razer_Blade, "razer_blade.json"},
        {PreferredKeyboard.Clevo_3_Zone, "clevo_3_zone.json"},
        {PreferredKeyboard.Masterkeys_Pro_L, "masterkeys_pro_l.json"},
        {PreferredKeyboard.Masterkeys_Pro_S, "masterkeys_pro_s.json"},
        {PreferredKeyboard.Masterkeys_MK750, "masterkeys_mk750.json"},
        {PreferredKeyboard.Masterkeys_MK730, "masterkeys_mk730.json"},
        {PreferredKeyboard.Cooler_Master_SK650, "cooler_master_sk650.json"},
        {PreferredKeyboard.Roccat_Ryos, "roccat_ryos.json"},
        {PreferredKeyboard.Roccat_Vualcan_TKL, "roccat_vulcan_tkl.json"},
        {PreferredKeyboard.SteelSeries_Apex_M800, "steelseries_apex_m800.json"},
        {PreferredKeyboard.SteelSeries_Apex_M750, "steelseries_apex_m750.json"},
        {PreferredKeyboard.SteelSeries_Apex_M750_TKL, "steelseries_apex_m750_tkl.json"},
        {PreferredKeyboard.Wooting_One, "wooting_one.json"},
        {PreferredKeyboard.Asus_Strix_Flare, "asus_strix_flare.json"},
        {PreferredKeyboard.Asus_Strix_Scope, "asus_strix_scope.json"},
        {PreferredKeyboard.SoundBlasterX_Vanguard_K08, "soundblasterx_vanguardk08.json"},
        {PreferredKeyboard.GenericLaptop, "generic_laptop.json"},
        {PreferredKeyboard.GenericLaptopNumpad, "generic_laptop_numpad.json"},
        {PreferredKeyboard.Debug, "additional_lights.json"},
        {PreferredKeyboard.Custom, "custom_keyboard.json"},
        {PreferredKeyboard.Generic60, "generic_60_percent.json"},
        {PreferredKeyboard.Drevo_BladeMaster, "drevo_blademaster.json"},
        {PreferredKeyboard.Wooting_Two, "wooting_two.json"},
        {PreferredKeyboard.MSI_GP66_US, "msi_gp66_us.json"},
        {PreferredKeyboard.Bloody_full, "bloody_full.json"},

        {PreferredKeyboard.Uniwill2ND_35X_1, "Uniwill2ND_35X_1.json"},
        {PreferredKeyboard.Uniwill2ND_35X_2, "Uniwill2ND_35X_2.json"},
        //keyboare 2.1
        {PreferredKeyboard.Uniwill2P1_550_US, "Uniwill2P1_550_US.json"},
        {PreferredKeyboard.Uniwill2P1_550_UK, "Uniwill2P1_550_UK.json"},
        {PreferredKeyboard.Uniwill2P1_550_BR, "Uniwill2P1_550_BR.json"},
        {PreferredKeyboard.Uniwill2P1_550_JP, "Uniwill2P1_550_JP.json"},
        //keyboare 2.2
        {PreferredKeyboard.Uniwill2P2_650_US, "Uniwill2P2_650_US.json"},
        {PreferredKeyboard.Uniwill2P2_650_UK, "Uniwill2P2_650_UK.json"},
        {PreferredKeyboard.Uniwill2P2_650_BR, "Uniwill2P2_650_BR.json"},
        {PreferredKeyboard.Uniwill2P2_650_JP, "Uniwill2P2_650_JP.json"},

        {PreferredKeyboard.Ducky_Shine_7, "ducky_shine_7.json"},
        {PreferredKeyboard.Ducky_One_2_RGB_TKL, "ducky_one_2_rgb_tkl.json"},
        {PreferredKeyboard.OMEN_Sequencer, "omen_sequencer.json"},
        {PreferredKeyboard.OMEN_Four_Zone, "omen_four_zone.json"},
        {PreferredKeyboard.HyperX_Alloy_Elite_RGB, "hyperx_alloy_elite_rgb.json"},
        {PreferredKeyboard.Keychron_Q1_Knob_Ansi, "keychron_q1_knob_ansi.json"},
    };

    public static readonly IReadOnlyDictionary<PreferredMouse, string> MouseLayoutMap = new Dictionary<PreferredMouse, string>
    {
          {PreferredMouse.Generic_Peripheral, "generic_peripheral.json"},
          {PreferredMouse.Generic_Openrgb_Mouse, "generic_openrgb_mouse.json"},
          {PreferredMouse.Custom, "custom_mouse.json"},
          {PreferredMouse.Logitech_G900, "logitech_g900_features.json"},
          {PreferredMouse.Logitech_G502, "logitech_g502_features.json"},
          {PreferredMouse.Logitech_G102, "logitech_g102_features.json"},
          {PreferredMouse.Corsair_Sabre, "corsair_sabre_features.json"},
          {PreferredMouse.Corsair_M65, "corsair_m65_features.json"},
          {PreferredMouse.Corsair_Katar, "corsair_katar_features.json"},
          {PreferredMouse.Clevo_Touchpad, "clevo_touchpad_features.json"},
          {PreferredMouse.Roccat_Kone_Pure, "roccat_kone_pure_features.json"},
          {PreferredMouse.SteelSeries_Rival_300, "steelseries_rival_300_features.json"},
          {PreferredMouse.SteelSeries_Rival_300_HP_OMEN_Edition, "steelseries_rival_300_hp_omen_edition_features.json"},
          {PreferredMouse.Asus_Pugio, "asus_pugio_features.json"},
          {PreferredMouse.Bloody_W60, "bloody_w60_features.json"},
          {PreferredMouse.OMEN_Photon, "omen_photon_features.json"},
          {PreferredMouse.OMEN_Outpost_Plus_Photon, "omen_outpost_plus_photon_features.json"},
          {PreferredMouse.OMEN_Vector, "omen_vector_features.json"},
          {PreferredMouse.OMEN_Vector_Essentials, "omen_vector_essentials_features.json"},
          {PreferredMouse.Razer_Mamba_TE, "razer_mamba_te_features.json"},
          {PreferredMouse.Razer_naga_Pro, "razer_naga_pro_features.json"},
    };

    public static readonly IReadOnlyDictionary<PreferredMousepad, string> MousepadLayoutMap = new Dictionary<PreferredMousepad, string>()
    {
        {PreferredMousepad.Generic_Mousepad, "generic_mousepad.json"},
        {PreferredMousepad.Custom, "custom_mousepad.json"},
        {PreferredMousepad.Two_Zone_Plus_Logo_Mousepad, "2zone_1logo_mousepad.json"},
        {PreferredMousepad.SteelSeries_QcK_Prism, "steelseries_qck_prism_features.json"},
        {PreferredMousepad.SteelSeries_QcK_2_Zone, "steelseries_qck_2zone_features.json"},
        {PreferredMousepad.Bloody_MP50RS, "bloody_mp-50rs.json"},
        {PreferredMousepad.Razer_19_Led, "razer_19_leds.json"},
    };

    public static readonly IReadOnlyDictionary<PreferredHeadset, string> HeadsetLayoutMap = new Dictionary<PreferredHeadset, string>()
    {
        {PreferredHeadset.One_Led_Headset, "1led_headset.json"},
        {PreferredHeadset.Two_Led_Headset, "2led_headset.json"},
        {PreferredHeadset.Custom, "custom_headset.json"},
    };

    public static readonly IReadOnlyDictionary<PreferredChromaLeds, string> ChromaLayoutMap = new Dictionary<PreferredChromaLeds, string>
    {
        {PreferredChromaLeds.Suggested, "chroma.json"},
        {PreferredChromaLeds.Custom, "custom_chroma.json"},
    };
}