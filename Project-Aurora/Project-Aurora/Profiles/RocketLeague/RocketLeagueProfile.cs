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
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5,
                            Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10,
                            Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        }),
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
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.Peripheral } ),
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
