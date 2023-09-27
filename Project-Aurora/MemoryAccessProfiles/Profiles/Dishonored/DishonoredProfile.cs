using System.Drawing;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Common.Devices;

namespace MemoryAccessProfiles.Profiles.Dishonored;

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
            new Layer("Mana Indicator", new PercentLayerHandler()
            {
                Properties = new PercentLayerHandlerProperties()
                {
                    _PrimaryColor =  Color.Blue,
                    _SecondaryColor = Color.FromArgb(255,0,0,70),
                    _PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
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
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.DELETE, DeviceKeys.END, DeviceKeys.PAGE_DOWN,
                        DeviceKeys.INSERT, DeviceKeys.HOME, DeviceKeys.PAGE_UP,
                        DeviceKeys.PRINT_SCREEN, DeviceKeys.SCROLL_LOCK, DeviceKeys.PAUSE_BREAK
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
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.NUM_ONE, DeviceKeys.NUM_TWO, DeviceKeys.NUM_THREE, DeviceKeys.NUM_FOUR,
                        DeviceKeys.NUM_FIVE, DeviceKeys.NUM_SIX, DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_EIGHT,
                        DeviceKeys.NUM_NINE
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