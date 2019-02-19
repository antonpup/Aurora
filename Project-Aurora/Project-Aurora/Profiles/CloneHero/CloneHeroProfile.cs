using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;

using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Profiles.CloneHero
{
    public class CloneHeroProfile : ApplicationProfile
    {
        public CloneHeroProfile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Menu Indicator", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsAtMenu",
                        _PrimaryColor = Color.FromArgb(9, 43, 83),
                        _SecondaryColor = Color.FromArgb(0, 149, 126, 183),
                        _Sequence = new KeySequence(new FreeFormObject((float)-26.54, (float)0, (float)1008.21, (float)244.69, 0))
                    }
                }
                ),

                new Layer("Combo Break", new AnimationLayerHandler {
                    Properties = new AnimationLayerHandlerProperties {
                        _AnimationMix = new AnimationMix(new[] {
                            new AnimationTrack("Filled Rectangle Track", 0.5f)
                                .SetFrame(0, new AnimationFilledRectangle(0, 0, 800, 300, Color.Red))
                                .SetFrame(0.4f, new AnimationFilledRectangle(0, 0, 800, 300, Color.FromArgb(0, 255, 0, 0)))
                        }),
                        _AnimationDuration = 0.4f,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnLow,
                        _TriggerPath = "Player/NoteStreak",
                        _StackMode = AnimationStackMode.Stack
                    }
                }),

                new Layer("Orange Fret", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsOrangePressed",
                        _PrimaryColor = Color.FromArgb(255, 134, 0),
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new[] { DK.O, DK.P, DK.L, DK.SEMICOLON, DK.PERIOD, DK.FORWARD_SLASH })
                    }
                }
                ),

                new Layer("Blue Fret", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsBluePressed",
                        _PrimaryColor = Color.Blue,
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new[] { DK.U, DK.I, DK.J, DK.K, DK.M, DK.COMMA })
                    }
                }
                ),

                new Layer("Yellow Fret", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsYellowPressed",
                        _PrimaryColor = Color.Yellow,
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new[] { DK.T, DK.Y, DK.G, DK.H, DK.B, DK.N })
                    }
                }
                ),

                new Layer("Red Fret", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsRedPressed",
                        _PrimaryColor = Color.Red,
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new[] { DK.E, DK.R, DK.D, DK.F, DK.C, DK.V })
                    }
                }
                ),

                new Layer("Green Fret", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsGreenPressed",
                        _PrimaryColor = Color.FromArgb(0, 255, 69),
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new[] { DK.Q, DK.W, DK.A, DK.S, DK.Z, DK.X })
                    }
                }
                ),

                new Layer("FC Indicator", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsFC",
                        _PrimaryColor = Color.FromArgb(208, 181, 0),
                        _SecondaryColor = Color.FromArgb(100, 100, 100),
                        _Sequence = new KeySequence(new[] {
                            DK.OPEN_BRACKET, DK.CLOSE_BRACKET, DK.APOSTROPHE
                        }),
                    }
                }
                ),

                new Layer("Star Power Indicator", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsStarPowerActive",
                        _PrimaryColor = Color.FromArgb(0, 227, 255),
                        _SecondaryColor = Color.FromArgb(130, 130, 130),
                        _Sequence = new KeySequence(new FreeFormObject((float)-0.78, (float)35.13, (float)551.5, (float)180.7, 0))
                    }
                }
                ),

                new Layer("Star Power Percent", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)646.37, (float)41.39, (float)195, (float)148.65, -90)),
                        _PrimaryColor = Color.FromArgb(0, 227, 255),
                        _SecondaryColor = Color.Black,
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/StarPowerPercent",
                        _MaxVariablePath = "100",
                    },
                }),

                new Layer("Note Hit", new AnimationLayerHandler {
                    Properties = new AnimationLayerHandlerProperties {
                        _AnimationMix = new AnimationMix(new[] {
                            new AnimationTrack("Filled Rectangle Track", 0.5f)
                                .SetFrame(0, new AnimationFilledCircle(70, 19, 6, Color.FromArgb(246, 125, 0), 12, 0.0057f))
                                .SetFrame(0.26f, new AnimationFilledCircle(69, -3, 4, Color.FromArgb(246, 125, 0), 8, 0.1347f))
                        }),
                        _AnimationDuration = 0.4f,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnHigh,
                        _TriggerPath = "Player/Score",
                        _StackMode = AnimationStackMode.Stack
                    }
                }),

                new Layer("Note Streak 4x", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)-2.75, (float)-2.61, (float)553.3, (float)36, 0)),
                        _PrimaryColor = Color.FromArgb(199, 0, 255),
                        _SecondaryColor = Color.Transparent,
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/NoteStreak4x",
                        _MaxVariablePath = "1",
                    },
                }),

                new Layer("Note Streak 3x", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)-2.75, (float)-2.61, (float)553.3, (float)36, 0)),
                        _PrimaryColor = Color.FromArgb(0, 255, 59),
                        _SecondaryColor = Color.Transparent,
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/NoteStreak3x",
                        _MaxVariablePath = "10",
                    },
                }),

                new Layer("Note Streak 2x", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)-2.75, (float)-2.61, (float)553.3, (float)36, 0)),
                        _PrimaryColor = Color.FromArgb(255, 146, 0),
                        _SecondaryColor = Color.Transparent,
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/NoteStreak2x",
                        _MaxVariablePath = "10",
                    },
                }),

                new Layer("Note Streak 1x", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)-2.75, (float)-2.61, (float)553.3, (float)36, 0)),
                        _PrimaryColor = Color.FromArgb(255, 247, 44),
                        _SecondaryColor = Color.Transparent,
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/NoteStreak1x",
                        _MaxVariablePath = "10",
                    }
                })
            };
        }
    }
}
