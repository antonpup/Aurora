using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO
{
    public class CSGOProfile : ApplicationProfile
    {
        public CSGOProfile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {

                new Layer("CSGO Typing Indicator", new Layers.CSGOTypingIndicatorLayerHandler()),
                new Layer("CSGO Kills Indicator", new Layers.CSGOKillIndicatorLayerHandler()),
                new Layer("CSGO Flashbang Effect", new Layers.CSGOFlashbangLayerHandler()),
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
                        _VariablePath = "Player/State/Health",
                        _MaxVariablePath = "100"
                        },

                }),
                new Layer("Ammo Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 0, 255),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive,
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR,
                            KeyboardKeys.FIVE, KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT,
                            KeyboardKeys.NINE, KeyboardKeys.ZERO, KeyboardKeys.MINUS, KeyboardKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.15,
                        _BlinkDirection = false,
                        _VariablePath = "Player/Weapons/ActiveWeapon/AmmoClip",
                        _MaxVariablePath = "Player/Weapons/ActiveWeapon/AmmoClipMax"
                    },
                }),
                new Layer("CSGO Bomb Effect", new Layers.CSGOBombLayerHandler()),
                new Layer("CSGO Burning Effect", new Layers.CSGOBurningLayerHandler()),
                new Layer("CSGO Background", new Layers.CSGOBackgroundLayerHandler())
            };
        }
    }
}
