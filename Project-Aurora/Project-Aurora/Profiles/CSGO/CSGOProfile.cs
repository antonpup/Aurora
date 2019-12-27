using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
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
                new Layer("CSGO Winning Team Effect", new Layers.CSGOWinningTeamLayerHandler()),
                new Layer("CSGO Flashbang Effect", new Layers.CSGOFlashbangLayerHandler()),
                new Layer("Health Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 255, 0),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4,
                            Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8,
                            Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/State/Health",
                        _MaxVariablePath = "100"
                        },

                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Round/Phase", RoundPhase.Live))
                ),
                new Layer("Ammo Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 0, 255),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR,
                            Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT,
                            Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.15,
                        _BlinkDirection = false,
                        _VariablePath = "Player/Weapons/ActiveWeapon/AmmoClip",
                        _MaxVariablePath = "Player/Weapons/ActiveWeapon/AmmoClipMax"
                    },
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Round/Phase", RoundPhase.Live))
                ),
                new Layer("CSGO Bomb Effect", new Layers.CSGOBombLayerHandler()),
                new Layer("CSGO Burning Effect", new Layers.CSGOBurningLayerHandler()),
                new Layer("CSGO Background", new Layers.CSGOBackgroundLayerHandler())
            };
        }
    }
}
