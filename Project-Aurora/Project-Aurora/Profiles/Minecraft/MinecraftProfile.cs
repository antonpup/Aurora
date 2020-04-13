using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Minecraft.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
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

            // Keys that do something and should be highlighted in a static color
            DK[] controlKeys = new[] { DK.W, DK.A, DK.S, DK.D, DK.E, DK.SPACE, DK.LEFT_SHIFT, DK.LEFT_CONTROL };

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>() {
                new Layer("Controls Assistant Layer", new MinecraftKeyConflictLayerHandler()),

                new Layer("Health Bar", new MinecraftHealthBarLayerHandler() {
                    Properties = new MinecraftHealthBarLayerHandlerProperties() {
                        _Sequence = new KeySequence(new[] {
                            DK.Z, DK.X, DK.C, DK.V, DK.B, DK.N, DK.M, DK.COMMA, DK.PERIOD, DK.FORWARD_SLASH
                        })
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/InGame"))
                ),

                new Layer("Experience Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Experience",
                        _MaxVariablePath = "Player/ExperienceMax",
                        _PrimaryColor = Color.FromArgb(255, 255, 0),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12
                        })
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/InGame"))
                ),

                new Layer("Toolbar", new ToolbarLayerHandler() {
                    Properties = new ToolbarLayerHandlerProperties() {
                        _PrimaryColor = Color.Transparent,
                        _SecondaryColor = Color.White,
                        _EnableScroll = true,
                        _ScrollLoop = true,
                        _Sequence = new KeySequence(new[] {
                            DK.ONE, DK.TWO, DK.THREE, DK.FOUR, DK.FIVE, DK.SIX, DK.SEVEN, DK.EIGHT, DK.NINE
                        })
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/InGame"))
                ),

                new Layer("Water Controls", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Blue,
                        _Sequence = new KeySequence(controlKeys)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanAnd(new List<BooleanGSIBoolean>(new[] { 
                        new BooleanGSIBoolean("Player/IsInWater"),new BooleanGSIBoolean("Player/InGame") }
                    )))
                ),

                new Layer("Sneaking Controls", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(45, 90, 90),
                        _Sequence = new KeySequence(controlKeys)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanAnd(new List<BooleanGSIBoolean>(new[] {
                        new BooleanGSIBoolean("Player/IsSneaking"),new BooleanGSIBoolean("Player/InGame") }
                    )))
                ),

                new Layer("Horse Controls", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(controlKeys)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanAnd(new List<BooleanGSIBoolean>(new[] {
                        new BooleanGSIBoolean("Player/IsRidingHorse"),new BooleanGSIBoolean("Player/InGame") }
                    )))
                ),

                new Layer("Player Controls", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(controlKeys)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/InGame"))
                ),

                new Layer("On Fire", new MinecraftBurnLayerHandler()),

                new Layer("Raining", new MinecraftRainLayerHandler()),

                new Layer("Grass Block Top", new MinecraftBackgroundLayerHandler() {
                    Properties = new MinecraftBackgroundLayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(44, 168, 32),
                        _SecondaryColor = Color.FromArgb(30, 80, 25),
                        _Sequence = new KeySequence(new FreeFormObject(0, -60, 900, 128))
                    }
                }, 
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(125,42,123), new BooleanGSINumeric("World/DimensionID", 1))//The End
                        .AddEntry(Color.FromArgb(255,183,0), new BooleanGSINumeric("World/DimensionID", -1))//Nether
                    )
                    .SetLookupTable("_SecondaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(49,0,59), new BooleanGSINumeric("World/DimensionID", 1))//The End
                        .AddEntry(Color.FromArgb(87,83,0), new BooleanGSINumeric("World/DimensionID", -1))//Nether
                    )
                ),

                new Layer("Grass Block Side", new MinecraftBackgroundLayerHandler() {
                    Properties = new MinecraftBackgroundLayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(125, 70, 15),//(102, 59, 20),
                        _SecondaryColor = Color.FromArgb(80, 50, 25)
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(209,232,80), new BooleanGSINumeric("World/DimensionID", 1))//The End
                        .AddEntry(Color.FromArgb(184,26,0), new BooleanGSINumeric("World/DimensionID", -1))//Nether
                    )
                    .SetLookupTable("_SecondaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(107,102,49), new BooleanGSINumeric("World/DimensionID", 1))//The End
                        .AddEntry(Color.FromArgb(59,8,0), new BooleanGSINumeric("World/DimensionID", -1))//Nether
                    )
                ),
            };
        }
    }
}
