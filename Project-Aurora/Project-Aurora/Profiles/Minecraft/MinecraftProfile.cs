using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Minecraft.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using Octokit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Profiles.Minecraft
{

    public class MinecraftProfile : ApplicationProfile
    {

        public MinecraftProfile() : base() { }

        public override void Reset()
        {
            base.Reset();

            // Keys that do something and should be highlighted in a static color
            DK[] controlKeys = new[] { DK.W, DK.A, DK.S, DK.D, DK.E, DK.SPACE, DK.LEFT_SHIFT, DK.LEFT_CONTROL };

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>() {
                new Layer("Controls Assistant Layer", new MinecraftKeyConflictLayerHandler()),

                new Layer("Health Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _VariablePath = "Player/Health",
                        _MaxVariablePath = "Player/HealthMax",
                        _PrimaryColor = Color.Red,
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.Z, DK.X, DK.C, DK.V, DK.B, DK.N, DK.M, DK.COMMA, DK.PERIOD, DK.FORWARD_SLASH
                        })
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/InGame"))
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(255, 210, 0), new BooleanGSIBoolean("Player/PlayerEffects/HasAbsorption"))
                        .AddEntry(Color.FromArgb(240, 75, 100), new BooleanGSIBoolean("Player/PlayerEffects/HasRegeneration"))
                        .AddEntry(Color.FromArgb(145, 160, 30), new BooleanGSIBoolean("Player/PlayerEffects/HasPoison"))
                        .AddEntry(Color.FromArgb(70, 5, 5), new BooleanGSIBoolean("Player/PlayerEffects/HasWither"))
                    )
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

                new Layer("On Fire", new SimpleParticleLayerHandler()
                {
                    Properties = new SimpleParticleLayerProperties()
                    {
                        _SpawnLocation = ParticleSpawnLocations.BottomEdge,
                        _ParticleColorStops = new Utils.ColorStopCollection()
                        {
                            { 0f, Color.Orange },
                            { 0.6f , Color.Red },
                            { 1f, Color.Black }
                        },
                        _MinSpawnTime = 0.05f,
                        _MaxSpawnTime = 0.05f,
                        _MinSpawnAmount = 8,
                        _MaxSpawnAmount = 10,
                        _MinLifetime = 0.5f,
                        _MaxLifetime = 2f,
                        _MinInitialVelocityX = 0,
                        _MaxInitialVelocityX = 0,
                        _MinInitialVelocityY = -5f,
                        _MaxInitialVelocityY = -0.8f,
                        _AccelerationX = 0f,
                        _AccelerationY = 0.5f,
                        _MinSize = 8,
                        _MaxSize = 12,
                        _DeltaSize = -4,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_SpawningEnabled", new BooleanGSIBoolean("Player/IsBurning"))
                ),

                new Layer("Raining", new SimpleParticleLayerHandler()
                {
                    Properties = new SimpleParticleLayerProperties()
                    {
                        _SpawnLocation = ParticleSpawnLocations.TopEdge,
                        _ParticleColorStops = new Utils.ColorStopCollection
                        {
                            { 0f, Color.Cyan },
                            { 1f, Color.Cyan }
                        },
                        _MinSpawnTime = .1f,
                        _MaxSpawnTime = .2f,
                        _MinSpawnAmount = 1,
                        _MaxSpawnAmount = 2,
                        _MinLifetime = 1,
                        _MaxLifetime = 1,
                        _MinInitialVelocityX = 0,
                        _MaxInitialVelocityX = 0,
                        _MinInitialVelocityY =3,
                        _MaxInitialVelocityY = 3,
                        _AccelerationX = 0,
                        _AccelerationY = 0,
                        _MinSize = 2,
                        _MaxSize = 4,
                        _DeltaSize = 0,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_SpawningEnabled", new BooleanGSIBoolean("World/IsRaining"))
                ),

                new Layer("Grass Block Top", new MinecraftBackgroundLayerHandler() {
                    Properties = new MinecraftBackgroundLayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(44, 168, 32),
                        _SecondaryColor = Color.FromArgb(30, 80, 25),
                        _Sequence = new KeySequence(new FreeFormObject(0, -60, 900, 128))
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(125,42,123), new BooleanAnd(new Evaluatable<bool>[] {
                            new BooleanGSINumeric("World/DimensionID", 1),
                            new BooleanGSIBoolean("Player/InGame")
                        }))//The End
                        .AddEntry(Color.FromArgb(255,183,0), new BooleanAnd(new Evaluatable<bool>[] {
                            new BooleanGSINumeric("World/DimensionID", -1),
                            new BooleanGSIBoolean("Player/InGame")
                        }))//The Nether
                    )
                    .SetLookupTable("_SecondaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(49,0,59), new BooleanAnd(new Evaluatable<bool>[] {
                            new BooleanGSINumeric("World/DimensionID", 1),
                            new BooleanGSIBoolean("Player/InGame")
                        }))//The End
                        .AddEntry(Color.FromArgb(87,83,0), new BooleanAnd(new Evaluatable<bool>[] {
                            new BooleanGSINumeric("World/DimensionID", -1),
                            new BooleanGSIBoolean("Player/InGame")
                        }))//The Nether
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
                        .AddEntry(Color.FromArgb(209,232,80), new BooleanAnd(new Evaluatable<bool>[] {
                            new BooleanGSINumeric("World/DimensionID", 1),
                            new BooleanGSIBoolean("Player/InGame")
                        }))//The End
                        .AddEntry(Color.FromArgb(184,26,0), new BooleanAnd(new Evaluatable<bool>[] {
                            new BooleanGSINumeric("World/DimensionID", -1),
                            new BooleanGSIBoolean("Player/InGame")
                        }))//The Nether
                    )
                    .SetLookupTable("_SecondaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(107,102,49),new BooleanAnd(new Evaluatable<bool>[] {
                            new BooleanGSINumeric("World/DimensionID", 1),
                            new BooleanGSIBoolean("Player/InGame")
                        }))//The End
                        .AddEntry(Color.FromArgb(59,8,0),  new BooleanAnd(new Evaluatable<bool>[] {
                            new BooleanGSINumeric("World/DimensionID", -1),
                            new BooleanGSIBoolean("Player/InGame")
                        }))//The Nether
                    )
                ),
            };
        }
    }
}
