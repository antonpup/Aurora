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
                new Layer("Typing Indicator", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(0, 255, 0),
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.TILDE, Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS, Devices.DeviceKeys.BACKSPACE,
                                                    Devices.DeviceKeys.TAB, Devices.DeviceKeys.Q, Devices.DeviceKeys.W, Devices.DeviceKeys.E, Devices.DeviceKeys.R, Devices.DeviceKeys.T, Devices.DeviceKeys.Y, Devices.DeviceKeys.U, Devices.DeviceKeys.I, Devices.DeviceKeys.O, Devices.DeviceKeys.P, Devices.DeviceKeys.CLOSE_BRACKET, Devices.DeviceKeys.OPEN_BRACKET, Devices.DeviceKeys.BACKSLASH,
                                                    Devices.DeviceKeys.CAPS_LOCK, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.F, Devices.DeviceKeys.G, Devices.DeviceKeys.H, Devices.DeviceKeys.J, Devices.DeviceKeys.K, Devices.DeviceKeys.L, Devices.DeviceKeys.SEMICOLON, Devices.DeviceKeys.APOSTROPHE, Devices.DeviceKeys.HASHTAG, Devices.DeviceKeys.ENTER,
                                                    Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.BACKSLASH_UK, Devices.DeviceKeys.Z, Devices.DeviceKeys.X, Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.B, Devices.DeviceKeys.N, Devices.DeviceKeys.M, Devices.DeviceKeys.COMMA, Devices.DeviceKeys.PERIOD, Devices.DeviceKeys.FORWARD_SLASH, Devices.DeviceKeys.RIGHT_SHIFT,
                                                    Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.LEFT_WINDOWS, Devices.DeviceKeys.LEFT_ALT, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.RIGHT_ALT, Devices.DeviceKeys.RIGHT_WINDOWS, Devices.DeviceKeys.APPLICATION_SELECT, Devices.DeviceKeys.RIGHT_CONTROL,
                                                    Devices.DeviceKeys.ARROW_UP, Devices.DeviceKeys.ARROW_LEFT, Devices.DeviceKeys.ARROW_DOWN, Devices.DeviceKeys.ARROW_RIGHT, Devices.DeviceKeys.ESC
                                                  })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Player/Activity", PlayerActivity.TextInput))
                ),
                new Layer("Winning Team Effect", new Layers.CSGOWinningTeamLayerHandler()),
                new Layer("Death Effect", new Layers.CSGODeathLayerHandler()),
                new Layer("Kills Indicator", new Layers.CSGOKillIndicatorLayerHandler()),
                new Layer("Flashbang Effect", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(255, 255, 255)
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicFloat("_LayerOpacity", new NumberMathsOperation(new NumberGSINumeric("Player/State/Flashed"), MathsOperator.Div, 255))
                ),
                new Layer("Smoke Effect", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(200, 200, 200)
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicFloat("_LayerOpacity", new NumberMathsOperation(new NumberGSINumeric("Player/State/Smoked"), MathsOperator.Div, 255))
                ),
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
                new Layer("Bomb Effect", new Layers.CSGOBombLayerHandler()),
                new Layer("Burning Effect", new Layers.CSGOBurningLayerHandler()),
                new Layer("Background", new Layers.CSGOBackgroundLayerHandler())
            };
        }
    }
}
