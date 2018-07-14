using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.Dishonored
{
    public class DishonoredProfile : ApplicationProfile
    {
        public DishonoredProfile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Health Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _SecondaryColor = Color.FromArgb(255,70,0,0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE,
                            Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO,
                            Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/CurrentHealth",
                        _MaxVariablePath = "Player/MaximumHealth"
                    },
                }),
                new Layer("Mana Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.Blue,
                        _SecondaryColor = Color.FromArgb(255,0,0,70),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4,
                            Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8,
                            Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/CurrentMana",
                        _MaxVariablePath = "Player/MaximumMana"
                    },
                }),
                new Layer("Mana Potions", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.Blue,
                        _SecondaryColor = Color.FromArgb(255,0,0,70),
                        _PercentType = PercentEffectType.Progressive,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.DELETE, Devices.DeviceKeys.END, Devices.DeviceKeys.PAGE_DOWN,
                            Devices.DeviceKeys.INSERT, Devices.DeviceKeys.HOME, Devices.DeviceKeys.PAGE_UP,
                            Devices.DeviceKeys.PRINT_SCREEN, Devices.DeviceKeys.SCROLL_LOCK, Devices.DeviceKeys.PAUSE_BREAK
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/ManaPots",
                        _MaxVariablePath = "9"
                    },
                }),
                new Layer("Health Potions", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.Red,
                        _SecondaryColor = Color.FromArgb(255,70,0,0),
                        _PercentType = PercentEffectType.Progressive,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.NUM_ONE, Devices.DeviceKeys.NUM_TWO, Devices.DeviceKeys.NUM_THREE, Devices.DeviceKeys.NUM_FOUR,
                            Devices.DeviceKeys.NUM_FIVE, Devices.DeviceKeys.NUM_SIX, Devices.DeviceKeys.NUM_SEVEN, Devices.DeviceKeys.NUM_EIGHT,
                            Devices.DeviceKeys.NUM_NINE
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/HealthPots",
                        _MaxVariablePath = "9"
                    },
                }),
                new Layer("Background", new SolidFillLayerHandler(){
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Gray
                    }
                })
            };
        }
    }
}
