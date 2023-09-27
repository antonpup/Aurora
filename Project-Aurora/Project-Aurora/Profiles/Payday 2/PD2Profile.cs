using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using Common.Devices;

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
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                            DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                            DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
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
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                            DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT,
                            DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
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
