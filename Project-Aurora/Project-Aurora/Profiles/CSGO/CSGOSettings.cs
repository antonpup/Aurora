using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO
{
    public class CSGOSettings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Background
        public bool bg_team_enabled;
        public Color ct_color;
        public Color t_color;
        public Color ambient_color;
        public bool bg_enable_dimming;
        public int bg_dim_after;
        public bool bg_peripheral_use;

        //// Health
        public bool health_enabled;
        public Color healthy_color;
        public Color hurt_color;
        public PercentEffectType health_effect_type;
        public KeySequence health_sequence;

        //// Ammo
        public bool ammo_enabled;
        public Color ammo_color;
        public Color noammo_color;
        public PercentEffectType ammo_effect_type;
        public KeySequence ammo_sequence;

        //// Bomb
        public bool bomb_enabled;
        public Color bomb_flash_color;
        public Color bomb_primed_color;
        public bool bomb_display_winner_color;
        public bool bomb_gradual;
        public KeySequence bomb_sequence;
        public bool bomb_peripheral_use;

        //// Kills Indicator
        public bool kills_indicator;
        public Color kills_regular_color;
        public Color kills_headshot_color;
        public KeySequence kills_sequence;

        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        //// Burning
        public bool burning_enabled;
        public Color burning_color;
        public bool burning_animation;
        public bool burning_peripheral_use;

        //// Flashbang
        public bool flashbang_enabled;
        public Color flash_color;
        public bool flashbang_peripheral_use;

        ////Typing Keys
        public bool typing_enabled;
        public Color typing_color;
        public KeySequence typing_sequence;

        public CSGOSettings()
        {
            //General
            first_time_installed = false;
            IsEnabled = true;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {

                new Settings.Layers.Layer("CSGO Typing Indicator", new Layers.CSGOTypingIndicatorLayerHandler()),
                new Settings.Layers.Layer("CSGO Kills Indicator", new Layers.CSGOKillIndicatorLayerHandler()),
                new Settings.Layers.Layer("CSGO Flashbang Effect", new Layers.CSGOFlashbangLayerHandler()),
                new Settings.Layers.Layer("Health Indicator", new Settings.Layers.PercentLayerHandler()
                {
                    Properties = new Settings.Layers.PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 255, 0),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4,
                            Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8,
                            Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/State/Health",
                        _MaxVariablePath = "100"
                        },
                    
                }),
                new Settings.Layers.Layer("Ammo Indicator", new Settings.Layers.PercentLayerHandler()
                {
                    Properties = new Settings.Layers.PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 0, 255),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR,
                            Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT,
                            Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.15,
                        _BlinkDirection = false,
                        _VariablePath = "Player/Weapons/ActiveWeapon/AmmoClip",
                        _MaxVariablePath = "Player/Weapons/ActiveWeapon/AmmoClipMax"
                    },
                }),
                new Settings.Layers.Layer("CSGO Bomb Effect", new Layers.CSGOBombLayerHandler()),
                new Settings.Layers.Layer("CSGO Burning Effect", new Layers.CSGOBurningLayerHandler()),
                new Settings.Layers.Layer("CSGO Background", new Layers.CSGOBackgroundLayerHandler())
            };

            //Effects
            //// Background
            bg_team_enabled = true;
            ct_color = Color.FromArgb(158, 205, 255);
            t_color = Color.FromArgb(221, 99, 33);
            ambient_color = Color.FromArgb(158, 205, 255);
            bg_enable_dimming = true;
            bg_dim_after = 15; //seconds
            bg_peripheral_use = true;

            //// Health
            health_enabled = true;
            healthy_color = Color.FromArgb(0, 255, 0);
            hurt_color = Color.FromArgb(255, 0, 0);
            health_effect_type = PercentEffectType.Progressive_Gradual;
            health_sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12 });

            //// Ammo
            ammo_enabled = true;
            ammo_color = Color.FromArgb(0, 0, 255);
            noammo_color = Color.FromArgb(255, 0, 0);
            ammo_effect_type = PercentEffectType.Progressive;
            ammo_sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS });

            //// Bomb
            bomb_enabled = true;
            bomb_flash_color = Color.FromArgb(255, 0, 0);
            bomb_primed_color = Color.FromArgb(0, 255, 0);
            bomb_display_winner_color = true;
            bomb_gradual = true;
            bomb_sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.NUM_LOCK, Devices.DeviceKeys.NUM_SLASH, Devices.DeviceKeys.NUM_ASTERISK, Devices.DeviceKeys.NUM_MINUS, Devices.DeviceKeys.NUM_SEVEN, Devices.DeviceKeys.NUM_EIGHT, Devices.DeviceKeys.NUM_NINE, Devices.DeviceKeys.NUM_PLUS, Devices.DeviceKeys.NUM_FOUR, Devices.DeviceKeys.NUM_FIVE, Devices.DeviceKeys.NUM_SIX, Devices.DeviceKeys.NUM_ONE, Devices.DeviceKeys.NUM_TWO, Devices.DeviceKeys.NUM_THREE, Devices.DeviceKeys.NUM_ZERO, Devices.DeviceKeys.NUM_PERIOD, Devices.DeviceKeys.NUM_ENTER });
            bomb_peripheral_use = true;

            //// Kills Indicator
            kills_indicator = true;
            kills_regular_color = Color.FromArgb(0, 255, 0);
            kills_headshot_color = Color.FromArgb(255, 80, 0);
            kills_sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.G1, Devices.DeviceKeys.G2, Devices.DeviceKeys.G3, Devices.DeviceKeys.G4, Devices.DeviceKeys.G5 });

            //// Lighting Areas
            lighting_areas = new List<ColorZone>();

            //// Burning
            burning_enabled = true;
            burning_color = Color.FromArgb(255, 70, 0);
            burning_animation = true;
            burning_peripheral_use = true;

            //// Flashbang
            flashbang_enabled = true;
            flash_color = Color.FromArgb(255, 255, 255);
            flashbang_peripheral_use = true;

            ////Typing Keys
            typing_enabled = true;
            typing_color = Color.FromArgb(0, 255, 0);
            typing_sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.TILDE, Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS, Devices.DeviceKeys.BACKSPACE,
                                                    Devices.DeviceKeys.TAB, Devices.DeviceKeys.Q, Devices.DeviceKeys.W, Devices.DeviceKeys.E, Devices.DeviceKeys.R, Devices.DeviceKeys.T, Devices.DeviceKeys.Y, Devices.DeviceKeys.U, Devices.DeviceKeys.I, Devices.DeviceKeys.O, Devices.DeviceKeys.P, Devices.DeviceKeys.CLOSE_BRACKET, Devices.DeviceKeys.OPEN_BRACKET, Devices.DeviceKeys.BACKSLASH,
                                                    Devices.DeviceKeys.CAPS_LOCK, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.F, Devices.DeviceKeys.G, Devices.DeviceKeys.H, Devices.DeviceKeys.J, Devices.DeviceKeys.K, Devices.DeviceKeys.L, Devices.DeviceKeys.SEMICOLON, Devices.DeviceKeys.APOSTROPHE, Devices.DeviceKeys.HASHTAG, Devices.DeviceKeys.ENTER,
                                                    Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.BACKSLASH_UK, Devices.DeviceKeys.Z, Devices.DeviceKeys.X, Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.B, Devices.DeviceKeys.N, Devices.DeviceKeys.M, Devices.DeviceKeys.COMMA, Devices.DeviceKeys.PERIOD, Devices.DeviceKeys.FORWARD_SLASH, Devices.DeviceKeys.RIGHT_SHIFT,
                                                    Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.LEFT_WINDOWS, Devices.DeviceKeys.LEFT_ALT, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.RIGHT_ALT, Devices.DeviceKeys.RIGHT_WINDOWS, Devices.DeviceKeys.APPLICATION_SELECT, Devices.DeviceKeys.RIGHT_CONTROL,
                                                    Devices.DeviceKeys.ARROW_UP, Devices.DeviceKeys.ARROW_LEFT, Devices.DeviceKeys.ARROW_DOWN, Devices.DeviceKeys.ARROW_RIGHT, Devices.DeviceKeys.ESC
                                                  });
        }
    }
}
