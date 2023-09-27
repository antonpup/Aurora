using System.Collections.ObjectModel;
using System.Drawing;
using Aurora.Devices;
using Aurora.Profiles.Witcher3.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Common.Devices;

namespace Aurora.Profiles.Witcher3;

public class Witcher3Profile : ApplicationProfile
{

    public override void Reset()
    {
        base.Reset();
        Layers = new ObservableCollection<Layer>
        {
            new("Health Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor =  Color.Red,
                    _SecondaryColor = Color.DarkRed,
                    _PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[] {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                    }),
                    _BlinkThreshold = 0.0,
                    _BlinkDirection = false,
                    _VariablePath = "Player/CurrentHealth",
                    _MaxVariablePath = "Player/MaximumHealth"
                },
            }),
            new("Toxicity Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor =  Color.Olive,
                    _SecondaryColor = Color.DarkOliveGreen,
                    _PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[] {
                        DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                        DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT,
                        DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
                    }),
                    _BlinkThreshold = 0.0,
                    _BlinkDirection = false,
                    _VariablePath = "Player/Toxicity",
                    _MaxVariablePath = "100"
                },
            }),
            new("Stamina Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor =  Color.Orange,
                    _SecondaryColor = Color.DarkOrange,
                    _PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[] {
                        DeviceKeys.NUM_ONE, DeviceKeys.NUM_TWO, DeviceKeys.NUM_THREE, DeviceKeys.NUM_FOUR,
                        DeviceKeys.NUM_FIVE, DeviceKeys.NUM_SIX, DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_EIGHT,
                        DeviceKeys.NUM_NINE
                    }),
                    _BlinkThreshold = 0.0,
                    _BlinkDirection = false,
                    _VariablePath = "Player/Stamina",
                    _MaxVariablePath = "100"
                },
            }),
            new("BG", new Witcher3BackgroundLayerHandler()),
        };
    }
}