using System.Drawing;
using Aurora.Settings;
using Aurora.Settings.Layers;

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
            new Layer("Mana Indicator", new PercentLayerHandler()
            {
                Properties = new PercentLayerHandlerProperties()
                {
                    _PrimaryColor =  Color.Blue,
                    _SecondaryColor = Color.FromArgb(255,0,0,70),
                    _PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new Aurora.Devices.DeviceKeys[] {
                        Aurora.Devices.DeviceKeys.F1, Aurora.Devices.DeviceKeys.F2, Aurora.Devices.DeviceKeys.F3, Aurora.Devices.DeviceKeys.F4,
                        Aurora.Devices.DeviceKeys.F5, Aurora.Devices.DeviceKeys.F6, Aurora.Devices.DeviceKeys.F7, Aurora.Devices.DeviceKeys.F8,
                        Aurora.Devices.DeviceKeys.F9, Aurora.Devices.DeviceKeys.F10, Aurora.Devices.DeviceKeys.F11, Aurora.Devices.DeviceKeys.F12
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
                    _Sequence = new KeySequence(new Aurora.Devices.DeviceKeys[] {
                        Aurora.Devices.DeviceKeys.DELETE, Aurora.Devices.DeviceKeys.END, Aurora.Devices.DeviceKeys.PAGE_DOWN,
                        Aurora.Devices.DeviceKeys.INSERT, Aurora.Devices.DeviceKeys.HOME, Aurora.Devices.DeviceKeys.PAGE_UP,
                        Aurora.Devices.DeviceKeys.PRINT_SCREEN, Aurora.Devices.DeviceKeys.SCROLL_LOCK, Aurora.Devices.DeviceKeys.PAUSE_BREAK
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
                    _Sequence = new KeySequence(new Aurora.Devices.DeviceKeys[] {
                        Aurora.Devices.DeviceKeys.NUM_ONE, Aurora.Devices.DeviceKeys.NUM_TWO, Aurora.Devices.DeviceKeys.NUM_THREE, Aurora.Devices.DeviceKeys.NUM_FOUR,
                        Aurora.Devices.DeviceKeys.NUM_FIVE, Aurora.Devices.DeviceKeys.NUM_SIX, Aurora.Devices.DeviceKeys.NUM_SEVEN, Aurora.Devices.DeviceKeys.NUM_EIGHT,
                        Aurora.Devices.DeviceKeys.NUM_NINE
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