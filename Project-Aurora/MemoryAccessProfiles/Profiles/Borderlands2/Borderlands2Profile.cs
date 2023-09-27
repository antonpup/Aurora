using System.Drawing;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Common.Devices;

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
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE,
                        DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO,
                        DeviceKeys.MINUS, DeviceKeys.EQUALS
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
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
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