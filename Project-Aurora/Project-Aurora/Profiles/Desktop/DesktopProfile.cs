using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Boolean;
using Common.Devices;
using Common.Utils;

namespace Aurora.Profiles.Desktop;

public enum InteractiveEffects
{
    [Description("None")] None = 0,
    [Description("Key Wave")] Wave = 1,
    [Description("Key Wave (Filled)")] WaveFilled = 3,
    [Description("Key Fade")] KeyPress = 2,
    [Description("Arrow Flow")] ArrowFlow = 4,
    [Description("Key Wave (Rainbow)")] WaveRainbow = 5,
}

public class DesktopProfile : ApplicationProfile
{
    private void AddVolumeOverlay()
    {
        OverlayLayers.Add(new Layer("Volume Overlay", new PercentGradientLayerHandler
        {
            Properties = new PercentGradientLayerHandlerProperties
            {
                _Sequence = new KeySequence(new[]
                {
                    DeviceKeys.VOLUME_MUTE, DeviceKeys.VOLUME_UP, DeviceKeys.VOLUME_DOWN
                }),
                _PercentType = PercentEffectType.AllAtOnce,
                _Gradient = new EffectBrush
                {
                    type = EffectBrush.BrushType.Linear,
                    wrap = EffectBrush.BrushWrap.None,
                    colorGradients = new SortedDictionary<double, Color>
                    {
                        { 0f, Color.Red },
                        { 0.18f, Color.FromArgb(255, 251, 0) },
                        { 0.4f, Color.Lime },
                        { 0.8f, Color.FromArgb(138, 255, 200) },
                        { 1f, Color.FromArgb(91, 0, 255) }
                    }
                },
                _VariablePath = "LocalPCInfo/SystemVolume",
                _MaxVariablePath = "1"
            },
        }));
    }

    private void AddPause()
    {
        OverlayLayers.Add(new Layer("Media Pause", new SolidColorLayerHandler()
        {
            Properties = new LayerHandlerProperties
            {
                _PrimaryColor = Color.Yellow,
                _Sequence = new KeySequence(new[]
                {
                    DeviceKeys.MEDIA_PLAY, DeviceKeys.MEDIA_PLAY_PAUSE
                }),
            },
        }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled",
            new BooleanGSIBoolean("LocalPCInfo/Media/MediaPlaying")
        )));
    }

    private void AddPlay()
    {
        OverlayLayers.Add(new Layer("Media Play", new SolidColorLayerHandler
        {
            Properties = new LayerHandlerProperties
            {
                _PrimaryColor = Color.Lime,
                _Sequence = new KeySequence(new[]
                {
                    DeviceKeys.MEDIA_PLAY, DeviceKeys.MEDIA_PLAY_PAUSE
                }),
            },
        }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled",
            new BooleanAnd(new List<Evaluatable<bool>>
            {
                new BooleanGSIBoolean("LocalPCInfo/Media/HasMedia"), new BooleanNot( new BooleanGSIBoolean("LocalPCInfo/Media/MediaPlaying") )
            } )
        )));
    }

    private void AddNext()
    {
        OverlayLayers.Add(new Layer("Media Next", new SolidColorLayerHandler
        {
            Properties = new LayerHandlerProperties
            {
                _PrimaryColor = Color.Lime,
                _Sequence = new KeySequence(new[]
                {
                    DeviceKeys.MEDIA_NEXT
                }),
            },
        }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled",
            new BooleanGSIBoolean("LocalPCInfo/Media/HasNextMedia")
        )));
    }

    private void AddPrevious()
    {
        OverlayLayers.Add(new Layer("Media Previous", new SolidColorLayerHandler
        {
            Properties = new LayerHandlerProperties
            {
                _PrimaryColor = Color.Lime,
                _Sequence = new KeySequence(new[]
                {
                    DeviceKeys.MEDIA_PREVIOUS
                }),
            },
        }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled",
            new BooleanGSIBoolean("LocalPCInfo/Media/HasPreviousMedia")
        )));
    }

    private void AddHasMedia()
    {
        OverlayLayers.Add(new Layer("Media Playing", new SolidColorLayerHandler
        {
            Properties = new LayerHandlerProperties
            {
                _PrimaryColor = Color.Lime,
                _Sequence = new KeySequence(new[]
                {
                    DeviceKeys.VOLUME_MUTE,
                    DeviceKeys.MEDIA_PREVIOUS,
                    DeviceKeys.MEDIA_PLAY, DeviceKeys.MEDIA_PLAY_PAUSE,
                    DeviceKeys.MEDIA_STOP,
                    DeviceKeys.MEDIA_NEXT,
                }),
            },
        }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled",
            new BooleanGSIBoolean("LocalPCInfo/Media/HasMedia")
        )));
    }

    private void AddOverlays()
    {
        AddVolumeOverlay();
        AddPause();
        AddPlay();
        AddNext();
        AddPrevious();
        AddHasMedia();
    }

    public override void Reset()
    {
        base.Reset();
        AddOverlays();

        Layers = new ObservableCollection<Layer>
        {
            new("Num Lock", new LockColourLayerHandler
            {
                Properties = new LockColourLayerHandlerProperties
                {
                    _ToggledKey = Keys.NumLock,
                    _PrimaryColor = CommonColorUtils.FastColor(108, 20, 255),
                    _SecondaryColor = CommonColorUtils.FastColor(255, 0, 4, 120),
                    _Sequence = new KeySequence(new[] { DeviceKeys.NUM_LOCK }),
                }
            }),
            new("Caps", new LockColourLayerHandler
            {
                Properties = new LockColourLayerHandlerProperties
                {
                    _ToggledKey = Keys.Capital,
                    _PrimaryColor = CommonColorUtils.FastColor(164, 45, 69),
                    _SecondaryColor = CommonColorUtils.FastColor(0, 0, 0, 0),
                    _Sequence = new KeySequence(new[] { DeviceKeys.CAPS_LOCK }),
                }
            }),
            new("Ctrl Shortcuts", new ShortcutAssistantLayerHandler
            {
                Properties = new ShortcutAssistantLayerHandlerProperties
                {
                    _PrimaryColor = Color.Red,
                    ShortcutKeys = new Keybind[]
                    {
                        new(new[] { Keys.LControlKey, Keys.X }),
                        new(new[] { Keys.LControlKey, Keys.C }),
                        new(new[] { Keys.LControlKey, Keys.V }),
                        new(new[] { Keys.LControlKey, Keys.Z }),
                        new(new[] { Keys.LControlKey, Keys.F4 }),
                        new(new[] { Keys.LControlKey, Keys.A }),
                        new(new[] { Keys.LControlKey, Keys.D }),
                        new(new[] { Keys.LControlKey, Keys.R }),
                        new(new[] { Keys.LControlKey, Keys.Y }),
                        new(new[] { Keys.LControlKey, Keys.Right }),
                        new(new[] { Keys.LControlKey, Keys.Left }),
                        new(new[] { Keys.LControlKey, Keys.Down }),
                        new(new[] { Keys.LControlKey, Keys.Up }),
                        new(new[] { Keys.LControlKey, Keys.LMenu, Keys.Tab }),
                        new(new[] { Keys.LControlKey, Keys.LMenu, Keys.Delete }),
                        new(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Up }),
                        new(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Down }),
                        new(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Left }),
                        new(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Right }),
                        new(new[] { Keys.LControlKey, Keys.Escape }),
                        new(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Escape }),
                        new(new[] { Keys.LControlKey, Keys.Escape }),
                        new(new[] { Keys.LControlKey, Keys.F })
                    },
                    PresentationType = ShortcutAssistantPresentationType.ProgressiveSuggestion,
                }
            }),
            new("Win Shortcuts", new ShortcutAssistantLayerHandler
            {
                Properties = new ShortcutAssistantLayerHandlerProperties
                {
                    _PrimaryColor = Color.Blue,
                    ShortcutKeys = new Keybind[]
                    {
                        new(new[] { Keys.LWin, Keys.L }),
                        new(new[] { Keys.LWin, Keys.D }),
                        new(new[] { Keys.LWin, Keys.B }),
                        new(new[] { Keys.LWin, Keys.A }),
                        new(new[] { Keys.LWin, Keys.LMenu, Keys.D }),
                        new(new[] { Keys.LWin, Keys.E }),
                        new(new[] { Keys.LWin, Keys.G }),
                        new(new[] { Keys.LWin, Keys.I }),
                        new(new[] { Keys.LWin, Keys.M }),
                        new(new[] { Keys.LWin, Keys.P }),
                        new(new[] { Keys.LWin, Keys.R }),
                        new(new[] { Keys.LWin, Keys.S }),
                        new(new[] { Keys.LWin, Keys.Up }),
                        new(new[] { Keys.LWin, Keys.Down }),
                        new(new[] { Keys.LWin, Keys.Left }),
                        new(new[] { Keys.LWin, Keys.Right }),
                        new(new[] { Keys.LWin, Keys.Home }),
                        new(new[] { Keys.LWin, Keys.D })
                    },
                    PresentationType = ShortcutAssistantPresentationType.ProgressiveSuggestion,
                }
            }),
            new("Alt Shortcuts", new ShortcutAssistantLayerHandler
            {
                Properties = new ShortcutAssistantLayerHandlerProperties
                {
                    _PrimaryColor = Color.Yellow,
                    ShortcutKeys = new Keybind[]
                    {
                        new(new[] { Keys.LMenu, Keys.Tab }),
                        new(new[] { Keys.LMenu, Keys.F4 }),
                        new(new[] { Keys.LMenu, Keys.Space }),
                        new(new[] { Keys.LMenu, Keys.Left }),
                        new(new[] { Keys.LMenu, Keys.Right }),
                        new(new[] { Keys.LMenu, Keys.PageUp }),
                        new(new[] { Keys.LMenu, Keys.PageDown }),
                        new(new[] { Keys.LMenu, Keys.Tab }),
                    },
                    PresentationType = ShortcutAssistantPresentationType.ProgressiveSuggestion,
                }
            }),
            new("CPU Usage", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor = Color.FromArgb(0, 205, 255),
                    _SecondaryColor = Color.FromArgb(0, 65, 80),
                    _PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[]
                    {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                    }),
                    _BlinkThreshold = 0.0,
                    _BlinkDirection = false,
                    _VariablePath = "LocalPCInfo/CPU/Usage",
                    _MaxVariablePath = "100"
                },
                EnableSmoothing = true,
            })
            {
                Enabled = false,
            },
            new("RAM Usage", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor = Color.FromArgb(255, 80, 0),
                    _SecondaryColor = Color.FromArgb(90, 30, 0),
                    _PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[]
                    {
                        DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                        DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT,
                        DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
                    }),
                    _BlinkThreshold = 0.0,
                    _BlinkDirection = false,
                    _VariablePath = "LocalPCInfo/RAM/Used",
                    _MaxVariablePath = "LocalPCInfo/RAM/Total"
                },
                EnableSmoothing = true
            })
            {
                Enabled = false,
            },
            new("Interactive Layer", new InteractiveLayerHandler
            {
                Properties = new InteractiveLayerHandlerProperties
                {
                    InteractiveEffect = InteractiveEffects.KeyPress,
                    _PrimaryColor = Color.FromArgb(127, 0, 255, 0),
                    _SecondaryColor = Color.FromArgb(127, 255, 0, 0),
                    EffectSpeed = 32.5f,
                    EffectWidth = 4
                }
            }),
            new("Gradient Wave", new GradientLayerHandler
            {
                Properties = new GradientLayerHandlerProperties
                {
                    Sequence =
                    {
                        Freeform = new FreeFormObject(0, 0, 1600, 400),
                        Type = KeySequenceType.FreeForm
                    },
                    _GradientConfig = new LayerEffectConfig
                    {
                        Angle = 90,
                        Speed = 1,
                        GradientSize = 19.5f,
                        AnimationType = AnimationType.TranslateXy,
                        Brush = new EffectBrush
                        {
                            type = EffectBrush.BrushType.Linear,
                            wrap = EffectBrush.BrushWrap.Repeat,
                            start = new PointF(0, -0.5f),
                            center = new PointF(0, 0),
                            end = new PointF(1, 1),
                            colorGradients =
                            {
                                [0] = CommonColorUtils.FastColor(0, 0, 0, 0),
                                [0.06593407690525055] = CommonColorUtils.FastColor(0, 0, 0, 0),
                                [0.1538461595773697] = CommonColorUtils.FastColor(153, 59, 237),
                                [0.24337075650691986] = CommonColorUtils.FastColor(10, 0, 0, 0),
                                [0.4263019561767578] = CommonColorUtils.FastColor(0, 0, 0, 0),
                                [0.5358933806419373] = CommonColorUtils.FastColor(151, 183, 63),
                                [0.6483517289161682] = CommonColorUtils.FastColor(4, 0, 0, 0),
                                [0.7614668011665344] = CommonColorUtils.FastColor(0, 24, 24, 51),
                                [0.8626373410224915] = CommonColorUtils.FastColor(129, 255, 239, 48),
                                [0.9395604133605957] = CommonColorUtils.FastColor(4, 194, 189, 127),
                                [1] = CommonColorUtils.FastColor(4, 194, 189, 12),
                            }
                        }
                    }
                }
            }),
            new("Background", new GradientLayerHandler
            {
                Properties = new GradientLayerHandlerProperties
                {
                    _LayerOpacity = 0.08f,
                    Sequence =
                    {
                        Freeform = new FreeFormObject(0, 0, 1600, 400),
                        Type = KeySequenceType.FreeForm
                    },
                    _GradientConfig = new LayerEffectConfig
                    {
                        Angle = 0,
                        Speed = 1,
                        GradientSize = 0,
                        AnimationType = AnimationType.TranslateXy,
                        AnimationReverse = true,
                        Brush = new EffectBrush(new ColorSpectrum(
                            CommonColorUtils.FastColor(253, 83, 30),
                            CommonColorUtils.FastColor(136, 219, 61),
                            CommonColorUtils.FastColor(255, 83, 30)
                            ))
                        {
                            start = new PointF(0, -0.5f),
                            center = new PointF(0, 0),
                            end = new PointF(1, 1),
                            wrap = EffectBrush.BrushWrap.Repeat,
                        }
                    }
                }
            }),
        };
    }
}