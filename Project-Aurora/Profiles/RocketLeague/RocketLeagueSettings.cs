using Aurora.EffectsEngine;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.RocketLeague
{
    public class RocketLeagueSettings : ProfileSettings
    {
        //Effects
        //// Background
        public bool bg_enabled;
        public Color bg_ambient_color;
        public bool bg_use_team_color;
        public bool bg_user_defined_team_colors;
        public Color bg_team_1;
        public Color bg_team_2;
        public bool bg_show_team_score_split;

        //// Boost
        public bool boost_enabled;
        public Color boost_low;
        public Color boost_mid;
        public Color boost_high;
        public KeySequence boost_sequence;
        public bool boost_peripheral_use;

        //// Speed
        public bool speed_enabled;
        public Color speed_low;
        public Color speed_mid;
        public Color speed_high;
        public KeySequence speed_sequence;
        public bool speed_peripheral_use;

        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public RocketLeagueSettings()
        {
            //Genereal
            IsEnabled = true;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {
                new Settings.Layers.Layer("Boost Indicator", new Settings.Layers.PercentGradientLayerHandler()
                {
                    Properties = new Settings.Layers.PercentGradientLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5,
                            Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10,
                            Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        }),
                        _Gradient = new EffectsEngine.EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/BoostAmount",
                        _MaxVariablePath = "1.0",
                    },
                }),
                new Settings.Layers.Layer("Boost Indicator (Peripheral)", new Settings.Layers.PercentGradientLayerHandler()
                {
                    Properties = new Settings.Layers.PercentGradientLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.AllAtOnce,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.Peripheral } ),
                        _Gradient = new EffectsEngine.EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/BoostAmount",
                        _MaxVariablePath = "1.0"
                    },
                }),
                new Settings.Layers.Layer("Rocket League Background", new Layers.RocketLeagueBackgroundLayerHandler())
            };

            //Effects
            //// Background
            bg_enabled = true;
            bg_ambient_color = Color.LightBlue;
            bg_use_team_color = true;
            bg_user_defined_team_colors = false;
            bg_team_1 = Color.Orange;
            bg_team_2 = Color.Blue;
            bg_show_team_score_split = true;

            //// Boost
            boost_enabled = true;
            boost_low = Color.Yellow;
            boost_mid = Color.OrangeRed;
            boost_high = Color.Red;
            boost_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5,
                Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10,
                Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
            });
            boost_peripheral_use = true;

            //// Speed
            speed_enabled = true;
            speed_low = Color.Cyan;
            speed_mid = Color.Purple;
            speed_high = Color.Red;
            speed_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE,
                Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO,
                Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
            });
            speed_peripheral_use = false;


            //// Lighting Areas
            lighting_areas = new List<ColorZone>();
        }
    }
}
