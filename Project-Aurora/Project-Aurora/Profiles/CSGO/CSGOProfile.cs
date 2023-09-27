using System.Collections.ObjectModel;
using System.Drawing;
using Aurora.Devices;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Profiles.CSGO.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using Common.Devices;

namespace Aurora.Profiles.CSGO;

public class CSGOProfile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();
        Layers = new ObservableCollection<Layer>
        {
            new("Typing Indicator", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(0, 255, 0),
                        _Sequence = new KeySequence(new[] { DeviceKeys.TILDE, DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE,
                            DeviceKeys.FOUR, DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO,
                            DeviceKeys.MINUS, DeviceKeys.EQUALS, DeviceKeys.BACKSPACE, DeviceKeys.TAB, DeviceKeys.Q, DeviceKeys.W, DeviceKeys.E,
                            DeviceKeys.R, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.U, DeviceKeys.I, DeviceKeys.O, DeviceKeys.P, DeviceKeys.CLOSE_BRACKET,
                            DeviceKeys.OPEN_BRACKET, DeviceKeys.BACKSLASH, DeviceKeys.CAPS_LOCK, DeviceKeys.A, DeviceKeys.S, DeviceKeys.D, DeviceKeys.F,
                            DeviceKeys.G, DeviceKeys.H, DeviceKeys.J, DeviceKeys.K, DeviceKeys.L, DeviceKeys.SEMICOLON, DeviceKeys.APOSTROPHE, DeviceKeys.HASHTAG, DeviceKeys.ENTER,
                            DeviceKeys.LEFT_SHIFT, DeviceKeys.BACKSLASH_UK, DeviceKeys.Z, DeviceKeys.X, DeviceKeys.C, DeviceKeys.V, DeviceKeys.B,
                            DeviceKeys.N, DeviceKeys.M, DeviceKeys.COMMA, DeviceKeys.PERIOD, DeviceKeys.FORWARD_SLASH, DeviceKeys.RIGHT_SHIFT,
                            DeviceKeys.LEFT_CONTROL, DeviceKeys.LEFT_WINDOWS, DeviceKeys.LEFT_ALT, DeviceKeys.SPACE, DeviceKeys.RIGHT_ALT,
                            DeviceKeys.RIGHT_WINDOWS, DeviceKeys.APPLICATION_SELECT, DeviceKeys.RIGHT_CONTROL,
                            DeviceKeys.ARROW_UP, DeviceKeys.ARROW_LEFT, DeviceKeys.ARROW_DOWN, DeviceKeys.ARROW_RIGHT, DeviceKeys.ESC
                        })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Player/Activity", PlayerActivity.TextInput))
            ),
            new("Winning Team Effect", new CSGOWinningTeamLayerHandler()),
            new("Death Effect", new CSGODeathLayerHandler()),
            new("Kills Indicator", new CSGOKillIndicatorLayerHandler()),
            new("Flashbang Effect", new SolidFillLayerHandler
                {
                    Properties = new SolidFillLayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(255, 255, 255)
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicFloat("_LayerOpacity", new NumberMathsOperation(new NumberGSINumeric("Player/State/Flashed"), MathsOperator.Div, 255))
            ),
            new("Smoke Effect", new SolidFillLayerHandler
                {
                    Properties = new SolidFillLayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(200, 200, 200)
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicFloat("_LayerOpacity", new NumberMathsOperation(new NumberGSINumeric("Player/State/Smoked"), MathsOperator.Div, 255))
            ),
            new("Health Indicator", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        _PrimaryColor =  Color.FromArgb(0, 255, 0),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new[] {
                            DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                            DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                            DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/State/Health",
                        _MaxVariablePath = "100"
                    },

                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Round/Phase", RoundPhase.Live))
            ),
            new("Ammo Indicator", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        _PrimaryColor =  Color.FromArgb(0, 0, 255),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive,
                        _Sequence = new KeySequence(new[] {
                            DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                            DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT,
                            DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.15,
                        _BlinkDirection = false,
                        _VariablePath = "Player/Weapons/ActiveWeapon/AmmoClip",
                        _MaxVariablePath = "Player/Weapons/ActiveWeapon/AmmoClipMax"
                    },
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Round/Phase", RoundPhase.Live))
            ),
            new("Bomb Effect", new CSGOBombLayerHandler()),
            new("Burning Effect", new CSGOBurningLayerHandler()),
            new("Background", new CSGOBackgroundLayerHandler())
        };
    }
}