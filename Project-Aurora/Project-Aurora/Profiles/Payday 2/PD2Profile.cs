using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.Payday_2.GSI.Nodes;
using Aurora.Profiles.Payday_2.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.Payday_2
{
    public class PD2Colours
    {
        public static Color GameTheme = Color.FromArgb(4, 119, 207);
        public static Color PlayerState_Standard = Color.FromArgb(4, 119, 207);
        public static Color PlayerState_Tazed = Color.FromArgb(255, 0, 0);
        public static Color PlayerState_Arrested = Color.FromArgb(255, 0, 0);
        public static Color PlayerState_Downed = Color.FromArgb(255, 0, 0);
        public static Color PlayerState_Civilian = Color.FromArgb(200, 200, 200);
        public static Color LevelState_Loud = Color.FromArgb(255, 255, 0);
        public static Color LevelState_Winters = Color.FromArgb(255, 165, 0);
        public static Color LevelState_PointOfNoReturn = Color.FromArgb(255, 0, 0);
    }

    public class PD2Profile : ApplicationProfile
    {
        public PD2Profile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>
            {
                // PD2 Mask On Animation
                new Layer("Mask On Animation", new PD2MaskOnAnimationLayerHandler()),

                // PD2 Health/Ammo Indicators
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
                        _VariablePath = "LocalPlayer/Health/Current",
                        _MaxVariablePath = "LocalPlayer/Health/Max"
                    },
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_Enabled", new OverrideLookupTableBuilder<bool>()
                        .AddEntry(false, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Civilian))
                        .AddEntry(false, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Undefined))
                        .AddEntry(false, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Mask_Off))
                    )
                ),
                new Layer("Ammo Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 0, 255),
                        _SecondaryColor = Color.FromArgb(255, 0, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] {
                            Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR,
                            Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT,
                            Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "LocalPlayer/Weapons/SelectedWeapon/Current_Clip",
                        _MaxVariablePath = "LocalPlayer/Weapons/SelectedWeapon/Max_Clip"
                    },
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_Enabled", new OverrideLookupTableBuilder<bool>()
                        .AddEntry(false, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Civilian))
                        .AddEntry(false, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Undefined))
                        .AddEntry(false, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Mask_Off))
                    )
                ),

                // PD2 Loud (Assault) State Animation
                new Layer("Loud (Assault) Animation", new GradientLayerHandler()
                {
                    Properties = new GradientLayerHandlerProperties()
                    {
                        _GradientConfig = new LayerEffectConfig()
                        {
                            Speed = 10,
                            Angle = 0,
                            GradientSize = 100,
                            AnimationType = AnimationType.Translate_XY,
                            AnimationReverse = false,
                            Brush = new EffectBrush()
                            {
                                type = EffectBrush.BrushType.Linear,
                                wrap = EffectBrush.BrushWrap.Repeat,
                                colorGradients = new SortedDictionary<float, Color> {
                                    { 0, PD2Colours.LevelState_Loud },
                                    { 0.5F, PD2Colours.LevelState_Loud },
                                    { 1, Color.White }
                                },
                                start = new PointF()
                                {
                                    X = 0.0F,
                                    Y = -0.5F
                                },
                                end = new PointF()
                                {
                                    X = 1F,
                                    Y = 1F
                                },
                                center = new PointF()
                                {
                                    X = 1F,
                                    Y = 1F
                                }
                            }
                        },
                        _Sequence = new KeySequence()
                        {
                            type = KeySequenceType.FreeForm,
                            freeform = Effects.WholeCanvasFreeForm
                        }
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Level/Phase", LevelPhase.Assault))
                ),

                // PD2 Loud (Assault) State Animation
                new Layer("Loud (Winters) Animation", new GradientLayerHandler()
                {
                    Properties = new GradientLayerHandlerProperties()
                    {
                        _GradientConfig = new LayerEffectConfig()
                        {
                            Speed = 10,
                            Angle = 0,
                            GradientSize = 100,
                            AnimationType = AnimationType.Translate_XY,
                            AnimationReverse = false,
                            Brush = new EffectBrush()
                            {
                                type = EffectBrush.BrushType.Linear,
                                wrap = EffectBrush.BrushWrap.Repeat,
                                colorGradients = new SortedDictionary<float, Color> {
                                    { 0, PD2Colours.LevelState_Winters },
                                    { 0.5F, PD2Colours.LevelState_Winters },
                                    { 1, Color.White }
                                },
                                start = new PointF()
                                {
                                    X = 0.0F,
                                    Y = -0.5F
                                },
                                end = new PointF()
                                {
                                    X = 1F,
                                    Y = 1F
                                },
                                center = new PointF()
                                {
                                    X = 1F,
                                    Y = 1F
                                }
                            }
                        },
                        _Sequence = new KeySequence()
                        {
                            type = KeySequenceType.FreeForm,
                            freeform = Effects.WholeCanvasFreeForm
                        }
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Level/Phase", LevelPhase.Winters))
                ),

                // PD2 Loud (Point of No Return) State Animation
                new Layer("Loud (Point of No Return) Animation", new GradientFillLayerHandler()
                {
                    Properties = new GradientFillLayerHandlerProperties()
                    {
                        _FillEntireKeyboard = true,
                        _GradientConfig = new LayerEffectConfig()
                        {
                            Speed = 10f,
                            Brush = new EffectBrush()
                            {
                                type = EffectBrush.BrushType.Linear,
                                wrap = EffectBrush.BrushWrap.Repeat,
                                colorGradients = new SortedDictionary<float, Color> {
                                    { 0, Color.Black },
                                    { 0.25f, PD2Colours.LevelState_PointOfNoReturn },
                                    { 0.5f, Color.Black },
                                    { 0.75f, PD2Colours.LevelState_PointOfNoReturn },
                                    { 1, Color.Black }
                                },
                                start = new PointF()
                                {
                                    X = 0F,
                                    Y = 1F
                                },
                                end = new PointF()
                                {
                                    X = 1F,
                                    Y = 1F
                                },
                                center = new PointF()
                                {
                                    X = 0F,
                                    Y = 0F
                                }
                            }
                        }
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIEnum("Level/Phase", LevelPhase.Point_of_no_return))
                ),

                // PD2 Level State Backgrounds
                new Layer("Level State Backgrounds", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(0,0,0,0)
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_Enabled", new OverrideLookupTableBuilder<bool>()
                        .AddEntry(true, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Standard))
                    )
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(PD2Colours.LevelState_Loud, new BooleanGSIEnum("Level/Phase", LevelPhase.Loud))
                        .AddEntry(PD2Colours.LevelState_Loud, new BooleanGSIEnum("Level/Phase", LevelPhase.Assault))
                    )
                ),

                // PD2 Player State Backgrounds
                new Layer("Player State Backgrounds", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(0,0,0,0)
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_Enabled", new OverrideLookupTableBuilder<bool>()
                        .AddEntry(true, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Standard))
                    )
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(PD2Colours.GameTheme, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Standard))
                        .AddEntry(PD2Colours.GameTheme, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Carry))
                        .AddEntry(PD2Colours.PlayerState_Arrested, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Arrested))
                        .AddEntry(PD2Colours.PlayerState_Civilian, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Mask_Off))
                        .AddEntry(PD2Colours.PlayerState_Civilian, new BooleanGSIEnum("LocalPlayer/State", PlayerState.Civilian))
                    )
                ),

                // PD2 Logo-Colour Background Effect
                new Layer("Background Effect", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = PD2Colours.GameTheme
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_Enabled", new OverrideLookupTableBuilder<bool>()
                        .AddEntry(true, new BooleanGSIEnum("Game/State", GameStates.Undefined))
                        .AddEntry(true, new BooleanGSIEnum("Game/State", GameStates.None))
                        .AddEntry(true, new BooleanGSIEnum("Game/State", GameStates.Kit_menu))
                        .AddEntry(true, new BooleanGSIEnum("Game/State", GameStates.Loot_menu))
                        .AddEntry(true, new BooleanGSIEnum("Game/State", GameStates.Menu_Pause))
                        .AddEntry(false, new BooleanGSIEnum("Game/State", GameStates.Mission_end_failure))
                        .AddEntry(false, new BooleanGSIEnum("Game/State", GameStates.Mission_end_success))
                        .AddEntry(false, new BooleanGSIEnum("Game/State", GameStates.Ingame))
                    )
                )
            };
        }
    }
}
