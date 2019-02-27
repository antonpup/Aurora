using Aurora.Devices;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Aurora.Profiles.Osu {

    public class OsuProfile : ApplicationProfile {

        public OsuProfile() : base() { }

        public override void Reset() {
            base.Reset();

            // Big thanks to @Ant on the Aurora Discord for creating this default profile :)

            Layers = new ObservableCollection<Layer> {
                new Layer("Typing", new InteractiveLayerHandler {
                    Properties = new InteractiveLayerHandlerProperties {
                        _InteractiveEffect = Desktop.InteractiveEffects.KeyPress,
                        _PrimaryColor = Color.FromArgb(255, 0, 240),
                        _SecondaryColor = Color.Black,
                        _EffectSpeed = 20,
                        _EffectWidth = 5
                    }
                }),

                new Layer("Drawing", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = Color.DeepSkyBlue,
                        _Sequence = new KeySequence(new[] { DeviceKeys.C })
                    }
                }),

                new Layer("Clickers", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = Color.FromArgb(255, 0, 240),
                        _Sequence = new KeySequence(new[] { DeviceKeys.Z,DeviceKeys.LEFT_ALT })
                    }
                }),

                new Layer("Accuracy", new PercentLayerHandler {
                    Properties = new PercentLayerHandlerProperties {
                        _PrimaryColor = Color.Lime,
                        _SecondaryColor = Color.Red,
                        _VariablePath = "Game/Accuracy",
                        _MaxVariablePath = "100",
                        _Sequence = new KeySequence(new[] { DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO }),
                        _PercentType = PercentEffectType.Progressive_Gradual
                    }
                }),

                new Layer("Miss", new AnimationLayerHandler {
                    Properties = new AnimationLayerHandlerProperties {
                        _AnimationMix = new AnimationMix(new[] {
                            new AnimationTrack("Filled Rectangle Track", 0.5f)
                                .SetFrame(0, new AnimationFilledRectangle(0, 0, 800, 300, Color.Red))
                                .SetFrame(0.5f, new AnimationFilledRectangle(0, 0, 800, 300, Color.FromArgb(0, 255, 0, 0)))
                        }),
                        _AnimationDuration = 0.5f,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnChange,
                        _TriggerPath = "Game/CountMiss",
                        _StackMode = AnimationStackMode.Stack
                    }
                }),

                new Layer("Combo Break", new AnimationLayerHandler {
                    Properties = new AnimationLayerHandlerProperties {
                        _AnimationMix = new AnimationMix(new[] {
                            new AnimationTrack("Filled Rectangle Track", 0.5f)
                                .SetFrame(0, new AnimationFilledRectangle(0, 0, 800, 300, Color.Red))
                                .SetFrame(0.5f, new AnimationFilledRectangle(0, 0, 800, 300, Color.FromArgb(0, 255, 0, 0)))
                        }),
                        _AnimationDuration = 0.5f,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnLow,
                        _TriggerPath = "Game/CountMiss",
                        _StackMode = AnimationStackMode.Stack
                    }
                }),

                new Layer("Healthbar", new PercentLayerHandler {
                    Properties = new PercentLayerHandlerProperties {
                        _PrimaryColor = Color.FromArgb(0, 160, 255),
                        _SecondaryColor = Color.Black,
                        _VariablePath = "Game/HP",
                        _MaxVariablePath = "200",
                        _Sequence = new KeySequence(new[] { DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12 }),
                        _PercentType = PercentEffectType.Progressive_Gradual
                    }
                }),

                new Layer("Score 300", new AnimationLayerHandler {
                    Properties = new AnimationLayerHandlerProperties {
                        _AnimationMix = CreateCircleAnimation(Color.FromArgb(0, 170, 255), 5),
                        _AnimationDuration = 1,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnHigh,
                        _TriggerPath = "Game/Count300",
                        _StackMode = AnimationStackMode.Stack
                    }
                }),

                new Layer("Score 100", new AnimationLayerHandler {
                    Properties = new AnimationLayerHandlerProperties {
                        _AnimationMix = CreateCircleAnimation(Color.Lime, 4),
                        _AnimationDuration = 1,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnHigh,
                        _TriggerPath = "Game/Count100",
                        _StackMode = AnimationStackMode.Stack
                    }
                }),

                new Layer("Score 50", new AnimationLayerHandler {
                    Properties = new AnimationLayerHandlerProperties {
                        _AnimationMix = CreateCircleAnimation(Color.Orange, 3),
                        _AnimationDuration = 1,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnHigh,
                        _TriggerPath = "Game/Count50",
                        _StackMode = AnimationStackMode.Stack
                    }
                }),

                new Layer("Equalizer background", new EqualizerLayerHandler {
                    Properties = new EqualizerLayerHandlerProperties {
                        _ViewType = EqualizerPresentationType.GradientColorShift,
                        _MaxAmplitude = 25,
                        _Gradient = new EffectsEngine.EffectBrush {
                            type = EffectsEngine.EffectBrush.BrushType.Linear,
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0, Color.Magenta },
                                { 0.5f, Color.Cyan },
                                { 1, Color.Magenta }
                            }
                        },
                        _Frequencies = new SortedSet<float> { 60, 170, 310, 600, 1000, 1500, 2000, 2500 },
                    },
                    _Opacity = .2f
                })
            };
        }

        // Little helper function to create the score circle animations
        private AnimationMix CreateCircleAnimation(Color c, int w) => new AnimationMix(new[] {
            new AnimationTrack("Circle Track", 1)
                .SetFrame(0, new AnimationCircle(10, 20, 0, c, w))
                .SetFrame(1, new AnimationCircle(70, 20, 100, c, w))
        });
    }
}
