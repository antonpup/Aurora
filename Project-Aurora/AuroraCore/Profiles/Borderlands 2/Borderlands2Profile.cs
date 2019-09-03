using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Drawing;

namespace Aurora.Profiles.Borderlands2
{
    public class Borderlands2Profile : ApplicationProfile
    {
        public Borderlands2Profile() : base()
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
                        _SecondaryColor = Color.DarkRed,
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
                new Layer("Shield Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.Cyan,
                        _SecondaryColor = Color.DarkCyan,
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4,
                            KeyboardKeys.F5, KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8,
                            KeyboardKeys.F9, KeyboardKeys.F10, KeyboardKeys.F11, KeyboardKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/CurrentShield",
                        _MaxVariablePath = "Player/MaximumShield"
                    },
                }),
                new Layer("Borderlands 2 Background", new SolidFillLayerHandler(){
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.LightGoldenrodYellow
                    }
                })
            };
        }
    }
}
