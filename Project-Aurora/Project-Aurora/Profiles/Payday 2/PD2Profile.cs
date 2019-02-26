using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.Payday_2
{
    public class PD2Profile : ApplicationProfile
    {
        public PD2Profile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>
            {
                new Layer("Payday 2 Flashbang", new Payday_2.Layers.PD2FlashbangLayerHandler()),
                new Layer("Health Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 255, 0),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4,
                            KeyboardKeys.F5, KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8,
                            KeyboardKeys.F9, KeyboardKeys.F10, KeyboardKeys.F11, KeyboardKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "LocalPlayer/Health/Current",
                        _MaxVariablePath = "LocalPlayer/Health/Max"
                    },
                }),
                new Layer("Ammo Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 0, 255),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR,
                            KeyboardKeys.FIVE, KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT,
                            KeyboardKeys.NINE, KeyboardKeys.ZERO, KeyboardKeys.MINUS, KeyboardKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "LocalPlayer/Weapons/SelectedWeapon/Current_Clip",
                        _MaxVariablePath = "LocalPlayer/Weapons/SelectedWeapon/Max_Clip"
                    },
                }),
                new Layer("Payday 2 States", new Payday_2.Layers.PD2StatesLayerHandler()),
                new Layer("Payday 2 Background", new Payday_2.Layers.PD2BackgroundLayerHandler())
            };
        }
    }
}
