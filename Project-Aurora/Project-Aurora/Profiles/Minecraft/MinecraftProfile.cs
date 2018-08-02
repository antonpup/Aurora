using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Profiles.Minecraft {

    public class MinecraftProfile : ApplicationProfile {

        public MinecraftProfile() : base() { }

        public override void Reset() {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>() {
                new Layer("Sneak Dimmer", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _ConditionPath = "Player/IsSneaking",
                        _PrimaryColor = Color.FromArgb(127, 0, 0, 0),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new FreeFormObject(-40, -75, 900, 300))
                    }
                }),

                new Layer("Experience Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Experience",
                        _MaxVariablePath = "Player/ExperienceMax",
                        _PrimaryColor = Color.FromArgb(14, 209, 53),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12
                        })
                    }
                }),

                new Layer("Health Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Health",
                        _MaxVariablePath = "Player/HealthMax",
                        _PrimaryColor = Color.Red,
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.ONE, DK.TWO, DK.THREE, DK.FOUR, DK.FIVE, DK.SIX, DK.SEVEN, DK.EIGHT, DK.NINE, DK.ZERO
                        })
                    }
                }),

                new Layer("Keys", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _ConditionPath = "Player/InGame",
                        _PrimaryColor = Color.FromArgb(0, 255, 255),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.W, DK.A, DK.S, DK.D, DK.E, DK.SPACE, DK.LEFT_SHIFT, DK.LEFT_CONTROL
                        })
                    }
                }),

                new Layer("In Water", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _ConditionPath = "Player/IsInWater",
                        _PrimaryColor = Color.FromArgb(22, 114, 224),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new FreeFormObject(-40, -75, 900, 300))
                    }
                }),

                new Layer("On Fire", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _ConditionPath = "Player/IsBurning",
                        _PrimaryColor = Color.FromArgb(255, 195, 0),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new FreeFormObject(-40, -75, 900, 300))
                    }
                }),

                new Layer("-- Background --", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _ConditionPath = "Player/InGame",
                        _PrimaryColor = Color.Black,
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new FreeFormObject(-40, -75, 900, 300))
                    }
                }),

                new Layer("Minecraft Text Animation", new AnimationLayerHandler() {
                    Properties = new AnimationLayerHandlerProperties() {
                        _AnimationMix = new AnimationMix(new[] {
                            new AnimationTrack("Manual Color Track", 6.75f)
                                .SetFrame(0.00f, CreateManualColorFrame(DK.M))
                                .SetFrame(0.75f, CreateManualColorFrame(DK.I))
                                .SetFrame(1.50f, CreateManualColorFrame(DK.N))
                                .SetFrame(2.25f, CreateManualColorFrame(DK.E))
                                .SetFrame(3.00f, CreateManualColorFrame(DK.C))
                                .SetFrame(3.75f, CreateManualColorFrame(DK.R))
                                .SetFrame(4.50f, CreateManualColorFrame(DK.A))
                                .SetFrame(5.25f, CreateManualColorFrame(DK.F))
                                .SetFrame(6.00f, CreateManualColorFrame(DK.T))
                        }),
                        _AnimationDuration = 7.5f,
                        _Sequence = new KeySequence(new[] {
                            DK.M, DK.I, DK.N, DK.E, DK.C, DK.R, DK.A, DK.F, DK.T
                        })
                    }
                }),

                new Layer("Grass Block Top", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(44, 168, 32),
                        _Sequence = new KeySequence(new FreeFormObject(0, -60, 900, 128))
                    }
                }),

                new Layer("Grass Block Side", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(102, 59, 20)
                    }
                })
            };
        }

        private AnimationManualColorFrame CreateManualColorFrame(DK k) {
            return (AnimationManualColorFrame)new AnimationManualColorFrame(new Dictionary<DK, Color> { { k, Color.Cyan } }).SetDuration(0.6f);
        }
    }
}
