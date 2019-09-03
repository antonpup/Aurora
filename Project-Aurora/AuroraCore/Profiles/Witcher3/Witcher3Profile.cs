using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.Witcher3
{
    public class Witcher3Profile : ApplicationProfile
    {
        public Witcher3Profile() : base()
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
                        _PrimaryColor =  Color.Red,
                        _SecondaryColor = Color.DarkRed,
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new List<KeyboardKeys> {
                            KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4,
                            KeyboardKeys.F5, KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8,
                            KeyboardKeys.F9, KeyboardKeys.F10, KeyboardKeys.F11, KeyboardKeys.F12
                        }.ConvertAll(s => s.GetDeviceLED())),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/CurrentHealth",
                        _MaxVariablePath = "Player/MaximumHealth"
                    },
                }),
                new Layer("Toxicity Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.Olive,
                        _SecondaryColor = Color.DarkOliveGreen,
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new List<KeyboardKeys> {
                            KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR,
                            KeyboardKeys.FIVE, KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT,
                            KeyboardKeys.NINE, KeyboardKeys.ZERO, KeyboardKeys.MINUS, KeyboardKeys.EQUALS
                        }.ConvertAll(s => s.GetDeviceLED())),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/Toxicity",
                        _MaxVariablePath = "100"
                    },
                }),
                new Layer("Stamina Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.Orange,
                        _SecondaryColor = Color.DarkOrange,
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new List<KeyboardKeys> {
                            KeyboardKeys.NUM_ONE, KeyboardKeys.NUM_TWO, KeyboardKeys.NUM_THREE, KeyboardKeys.NUM_FOUR,
                            KeyboardKeys.NUM_FIVE, KeyboardKeys.NUM_SIX, KeyboardKeys.NUM_SEVEN, KeyboardKeys.NUM_EIGHT,
                            KeyboardKeys.NUM_NINE
                        }.ConvertAll(s => s.GetDeviceLED())),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/Stamina",
                        _MaxVariablePath = "100"
                    },
                }),
                new Layer("BG", new Layers.Witcher3BackgroundLayerHandler()),
            };
        }
    }
}
