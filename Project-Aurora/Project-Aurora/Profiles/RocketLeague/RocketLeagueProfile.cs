using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.RocketLeague
{
    public class RocketLeagueProfile : ApplicationProfile
    {
        public RocketLeagueProfile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Boost Indicator", new PercentGradientLayerHandler()
                {
                    Properties = new PercentGradientLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new List<KeyboardKeys> {
                            KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4, KeyboardKeys.F5,
                            KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8, KeyboardKeys.F9, KeyboardKeys.F10,
                            KeyboardKeys.F11, KeyboardKeys.F12
                        }.ConvertAll(s => s.GetDeviceLED())),
                        _Gradient = new EffectsEngine.EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/BoostAmount",
                        _MaxVariablePath = "100",
                    },
                }),
                new Layer("Boost Indicator (Peripheral)", new PercentGradientLayerHandler()
                {
                    Properties = new PercentGradientLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.AllAtOnce,
                        //TODO: Sort out Peripheral lights
                        _Sequence = new KeySequence(new List<MouseLights> { MouseLights.All }.ConvertAll(s => s.GetDeviceLED()) ),
                        _Gradient = new EffectsEngine.EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/BoostAmount",
                        _MaxVariablePath = "100"
                    },
                }),
                new Layer("Rocket League Background", new Layers.RocketLeagueBackgroundLayerHandler())
            };
        }
    }
}
