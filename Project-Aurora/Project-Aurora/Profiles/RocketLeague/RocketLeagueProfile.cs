using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using Aurora.Profiles.RocketLeague.Layers;
using Aurora.Settings.Overrides.Logic.Builder;
using Aurora.Settings.Overrides.Logic;

namespace Aurora.Profiles.RocketLeague
{
    public class RocketLeagueBMProfile : ApplicationProfile
    {
        public RocketLeagueBMProfile() : base()
        {

        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Boost Indicator (Peripheral)", new PercentGradientLayerHandler()
                {
                    Properties = new PercentGradientLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.AllAtOnce,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.Peripheral, Devices.DeviceKeys.Peripheral_Logo } ),
                        _Gradient = new EffectsEngine.EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/Boost",
                        _MaxVariablePath = "1"
                    },
                }),
                new Layer("Boost Indicator", new PercentGradientLayerHandler()
                {
                    Properties = new PercentGradientLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5,
                            Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10,
                            Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        }),
                        _Gradient = new EffectsEngine.EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/Boost",
                        _MaxVariablePath = "1",
                    },
                }),
                new Layer("Boost Background", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Black,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4, Devices.DeviceKeys.F5,
                            Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9, Devices.DeviceKeys.F10,
                            Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
                        })
                    }
                }),
                new Layer("Goal Explosion", new RocketLeagueGoalExplosionLayerHandler()),
                new Layer("Score Split", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(Effects.WholeCanvasFreeForm),
                        _VariablePath = "YourTeam/Goals",
                        _MaxVariablePath ="Match/TotalGoals",
                        _PrimaryColor = Color.Transparent,
                        _SecondaryColor = Color.Transparent
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicDouble("_Value", new IfElseNumeric( new BooleanAnd( new List<Evaluatable<bool>>{//if match is tied 0 - 0
                                                                                    new BooleanMathsComparison(new NumberGSINumeric("YourTeam/Goals"), new NumberConstant(0)),
                                                                                    new BooleanMathsComparison(new NumberGSINumeric("OpponentTeam/Goals"), new NumberConstant(0))
                                                                                }
                                                                ),
                                                                new NumberConstant(1),//then set the value to 1, so it is split 50-50
                                                                new NumberGSINumeric("YourTeam/Goals")//otherwise set to our goals
                                                )
                    )
                    .SetDynamicDouble("_MaxValue", new IfElseNumeric( new BooleanAnd( new List<Evaluatable<bool>>{//if match is tied 0 - 0
                                                                                    new BooleanMathsComparison(new NumberGSINumeric("YourTeam/Goals"), new NumberConstant(0)),
                                                                                    new BooleanMathsComparison(new NumberGSINumeric("OpponentTeam/Goals"), new NumberConstant(0))
                                                                                }
                                                                ),
                                                                new NumberConstant(2),//then set the max to 2, so it is split 50-50
                                                                new NumberGSINumeric("Match/TotalGoals")//otherwise set to total goals
                                                )
                    )
                    .SetDynamicColor("_PrimaryColor", new NumberConstant(1),
                                                      new NumberGSINumeric("YourTeam/Red"),
                                                      new NumberGSINumeric("YourTeam/Green"),
                                                      new NumberGSINumeric("YourTeam/Blue"))
                    .SetDynamicColor("_SecondaryColor", new NumberConstant(1),
                                                        new NumberGSINumeric("OpponentTeam/Red"),
                                                        new NumberGSINumeric("OpponentTeam/Green"),
                                                        new NumberGSINumeric("OpponentTeam/Blue"))
                    .SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Game/Status", ComparisonOperator.NEQ, -1))
                ),
                new Layer("Background Layer", new SolidFillLayerHandler()
                {
                    Properties = new SolidFillLayerHandlerProperties
                    {
                        _PrimaryColor = Color.Blue
                    }
                }),
            };
        }
    }
}
