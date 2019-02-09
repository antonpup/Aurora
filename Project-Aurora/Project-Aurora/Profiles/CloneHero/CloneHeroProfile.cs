using Aurora.EffectsEngine;
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

                new Layer("FC Indicator", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/IsFC",
                        _PrimaryColor = Color.FromArgb(255, 181, 0),
                        _SecondaryColor = Color.FromArgb(100, 100, 100),
                        _Sequence = new KeySequence(new[] {
                            DK.Peripheral, DK.ADDITIONALLIGHT1, DK.ADDITIONALLIGHT2,
                            DK.ADDITIONALLIGHT3, DK.ADDITIONALLIGHT4, DK.ADDITIONALLIGHT5,
                            DK.ADDITIONALLIGHT6, DK.ADDITIONALLIGHT7, DK.ADDITIONALLIGHT8,
                            DK.ADDITIONALLIGHT9, DK.ADDITIONALLIGHT10, DK.ADDITIONALLIGHT11,
                            DK.ADDITIONALLIGHT12, DK.ADDITIONALLIGHT13, DK.ADDITIONALLIGHT14,
                            DK.ADDITIONALLIGHT15, DK.ADDITIONALLIGHT16, DK.ADDITIONALLIGHT17,
                            DK.ADDITIONALLIGHT18, DK.ADDITIONALLIGHT19, DK.ADDITIONALLIGHT20,
                            DK.ADDITIONALLIGHT21, DK.ADDITIONALLIGHT22, DK.ADDITIONALLIGHT23,
                            DK.ADDITIONALLIGHT24, DK.ADDITIONALLIGHT25, DK.ADDITIONALLIGHT26,
                            DK.ADDITIONALLIGHT27, DK.ADDITIONALLIGHT28, DK.ADDITIONALLIGHT29,
                            DK.ADDITIONALLIGHT30, DK.ADDITIONALLIGHT31, DK.ADDITIONALLIGHT32
                        }),
                    }
                }
                ),

                new Layer("Star Power Activated", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/SPActivated",
                        _PrimaryColor = Color.FromArgb(0, 227, 255),
                        _SecondaryColor = Color.FromArgb(0, 150, 150, 150),
                        _Sequence = new KeySequence(new FreeFormObject((float)0.78, (float)0, (float)545.25, (float)215.8, 0))
                    }
                }
                ),

                new Layer("Star Power Percent", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)572.6, (float)-32.39, (float)217, (float)274.35, -90)),
                        _PrimaryColor = Color.FromArgb(0, 227, 255),
                        _SecondaryColor = Color.Black,
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Player/SPPercent",
                        _MaxVariablePath = "100",
                    },
                }),

                new Layer("Note Streak 4x", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)165, (float)-169.41, (float)217, (float)546.82, -90)),
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
                        _Sequence = new KeySequence(new FreeFormObject((float)165, (float)-169.41, (float)217, (float)546.82, -90)),
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
                        _Sequence = new KeySequence(new FreeFormObject((float)165, (float)-169.41, (float)217, (float)546.82, -90)),
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
                        _Sequence = new KeySequence(new FreeFormObject((float)165, (float)-169.41, (float)217, (float)546.82, -90)),
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
