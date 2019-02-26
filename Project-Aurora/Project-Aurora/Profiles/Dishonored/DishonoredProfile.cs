using Aurora.Devices.Layout.Layouts;
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
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR, KeyboardKeys.FIVE,
                            KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT, KeyboardKeys.NINE, KeyboardKeys.ZERO,
                            KeyboardKeys.MINUS, KeyboardKeys.EQUALS
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
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4,
                            KeyboardKeys.F5, KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8,
                            KeyboardKeys.F9, KeyboardKeys.F10, KeyboardKeys.F11, KeyboardKeys.F12
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
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.DELETE, KeyboardKeys.END, KeyboardKeys.PAGE_DOWN,
                            KeyboardKeys.INSERT, KeyboardKeys.HOME, KeyboardKeys.PAGE_UP,
                            KeyboardKeys.PRINT_SCREEN, KeyboardKeys.SCROLL_LOCK, KeyboardKeys.PAUSE_BREAK
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
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.NUM_ONE, KeyboardKeys.NUM_TWO, KeyboardKeys.NUM_THREE, KeyboardKeys.NUM_FOUR,
                            KeyboardKeys.NUM_FIVE, KeyboardKeys.NUM_SIX, KeyboardKeys.NUM_SEVEN, KeyboardKeys.NUM_EIGHT,
                            KeyboardKeys.NUM_NINE
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
