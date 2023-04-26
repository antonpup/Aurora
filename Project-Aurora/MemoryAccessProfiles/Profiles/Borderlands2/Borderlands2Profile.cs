using System.Drawing;
using Aurora.Settings;
using Aurora.Settings.Layers;

namespace MemoryAccessProfiles.Profiles.Borderlands2;

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
                    _Sequence = new KeySequence(new Aurora.Devices.DeviceKeys[] {
                        Aurora.Devices.DeviceKeys.ONE, Aurora.Devices.DeviceKeys.TWO, Aurora.Devices.DeviceKeys.THREE, Aurora.Devices.DeviceKeys.FOUR, Aurora.Devices.DeviceKeys.FIVE,
                        Aurora.Devices.DeviceKeys.SIX, Aurora.Devices.DeviceKeys.SEVEN, Aurora.Devices.DeviceKeys.EIGHT, Aurora.Devices.DeviceKeys.NINE, Aurora.Devices.DeviceKeys.ZERO,
                        Aurora.Devices.DeviceKeys.MINUS, Aurora.Devices.DeviceKeys.EQUALS
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
                    _Sequence = new KeySequence(new Aurora.Devices.DeviceKeys[] {
                        Aurora.Devices.DeviceKeys.F1, Aurora.Devices.DeviceKeys.F2, Aurora.Devices.DeviceKeys.F3, Aurora.Devices.DeviceKeys.F4,
                        Aurora.Devices.DeviceKeys.F5, Aurora.Devices.DeviceKeys.F6, Aurora.Devices.DeviceKeys.F7, Aurora.Devices.DeviceKeys.F8,
                        Aurora.Devices.DeviceKeys.F9, Aurora.Devices.DeviceKeys.F10, Aurora.Devices.DeviceKeys.F11, Aurora.Devices.DeviceKeys.F12
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