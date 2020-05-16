﻿using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;
using StringComparison = Aurora.Settings.Overrides.Logic.StringComparison;

namespace Aurora.Profiles.StardewValley
{
    public class StardewValleyProfile : ApplicationProfile
    {
        public override void Reset()
        {
            base.Reset();

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Title/Loading", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties {
                        _PrimaryColor = Color.DarkCyan
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanOr(new List<IEvaluatable<bool>> { new BooleanGSIEnum("GameState/GameState", GSI.Nodes.GameStates.TitleMenu), new BooleanGSIEnum("GameState/GameState", GSI.Nodes.GameStates.Loading) }))
                    .SetLookupTable("_PrimaryColor",new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(255, 255, 210, 132),
                                new BooleanGSIEnum("GameState/GameState", GSI.Nodes.GameStates.Loading))
                    )
                ),

                new Layer("Health Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _VariablePath = "Player/Health/Current",
                        _MaxVariablePath = "Player/Health/Max",
                        _PrimaryColor = Color.Lime,
                        _SecondaryColor = Color.Red,
                        _Sequence = new KeySequence(new[] {
                            DK.ONE, DK.TWO, DK.THREE, DK.FOUR, DK.FIVE, DK.SIX, DK.SEVEN, DK.EIGHT, DK.NINE, DK.ZERO
                        })
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/Health/BarActive"))
                ),

                new Layer("Energy Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _VariablePath = "Player/Energy/Current",
                        _MaxVariablePath = "Player/Energy/Max",
                        _PrimaryColor = Color.Yellow,
                        _SecondaryColor = Color.Red,
                        _Sequence = new KeySequence(new[] {
                            DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12
                        })
                    }
                }),

                new Layer("Debris/Fall", new SimpleParticleLayerHandler()
                {
                    Properties = new SimpleParticleLayerProperties()
                    {
                        _SpawnLocation = ParticleSpawnLocations.TopEdge,
                        _ParticleColorStops = new Utils.ColorStopCollection
                        {
                            { 0f, Color.FromArgb(255, 255, 131, 65) },
                            { 1f, Color.FromArgb(255, 255, 131, 65) }
                        },
                        _MinSpawnTime = .7f,
                        _MaxSpawnTime = .9f,
                        _MinSpawnAmount = 1,
                        _MaxSpawnAmount = 2,
                        _MinLifetime = 5,
                        _MaxLifetime = 5,
                        _MinInitialVelocityX = -0.5f,
                        _MaxInitialVelocityX = -0.2f,
                        _MinInitialVelocityY = 0.3f,
                        _MaxInitialVelocityY = 0.3f,
                        _AccelerationX = 0,
                        _AccelerationY = 0,
                        _MinSize = 7,
                        _MaxSize = 7,
                        _DeltaSize = 0,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_SpawningEnabled", new BooleanAnd(new List<IEvaluatable<bool>> { new BooleanGSIBoolean("World/Weather/IsDebrisWeather"), new BooleanGSIBoolean("Player/IsOutdoor"), new StringComparison() { Operand1 = new StringGSIString {VariablePath = "Player/CurrentLocation" }, Operand2 = new StringConstant {Value = "Desert" }, Operator = StringComparisonOperator.NotEqual } }))
                ),

                new Layer("Raining", new SimpleParticleLayerHandler()
                {
                    Properties = new SimpleParticleLayerProperties()
                    {
                        _SpawnLocation = ParticleSpawnLocations.TopEdge,
                        _ParticleColorStops = new Utils.ColorStopCollection
                        {
                            { 0f, Color.Blue },
                            { 1f, Color.Blue }
                        },
                        _MinSpawnTime = .1f,
                        _MaxSpawnTime = .2f,
                        _MinSpawnAmount = 2,
                        _MaxSpawnAmount = 2,
                        _MinLifetime = 1,
                        _MaxLifetime = 1,
                        _MinInitialVelocityX = -1.5f,
                        _MaxInitialVelocityX = -2,
                        _MinInitialVelocityY =2,
                        _MaxInitialVelocityY = 2,
                        _AccelerationX = 0,
                        _AccelerationY = 0,
                        _MinSize = 5,
                        _MaxSize = 7,
                        _DeltaSize = 0,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_SpawningEnabled", new BooleanAnd(new List<IEvaluatable<bool>> { new BooleanGSIBoolean("World/Weather/IsRaining"), new BooleanGSIBoolean("Player/IsOutdoor"), new StringComparison() { Operand1 = new StringGSIString {VariablePath = "Player/CurrentLocation" }, Operand2 = new StringConstant {Value = "Desert" }, Operator = StringComparisonOperator.NotEqual } }))
                ),

                new Layer("Snowing", new SimpleParticleLayerHandler()
                {
                    Properties = new SimpleParticleLayerProperties()
                    {
                        _SpawnLocation = ParticleSpawnLocations.TopEdge,
                        _ParticleColorStops = new Utils.ColorStopCollection
                        {
                            { 0f, Color.White },
                            { 1f, Color.White }
                        },
                        _MinSpawnTime = .1f,
                        _MaxSpawnTime = .2f,
                        _MinSpawnAmount = 1,
                        _MaxSpawnAmount = 2,
                        _MinLifetime = 4,
                        _MaxLifetime = 4,
                        _MinInitialVelocityX = 0,
                        _MaxInitialVelocityX = 0.5f,
                        _MinInitialVelocityY =0.5f,
                        _MaxInitialVelocityY = 0.5f,
                        _AccelerationX = 0,
                        _AccelerationY = 0,
                        _MinSize = 3,
                        _MaxSize = 4,
                        _DeltaSize = 0,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_SpawningEnabled", new BooleanAnd(new List<IEvaluatable<bool>> {
                        new BooleanGSIBoolean("World/Weather/IsSnowing"), new BooleanGSIBoolean("Player/IsOutdoor"), new StringComparison() { Operand1 = new StringGSIString {VariablePath = "Player/CurrentLocation" }, Operand2 = new StringConstant {Value = "Desert" }, Operator = StringComparisonOperator.NotEqual } }))
                ),

                new Layer("Background/Season", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties {
                        _PrimaryColor = Color.Transparent
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor",new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.ForestGreen,
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Spring))
                        .AddEntry(Color.Gold,
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Summer))
                        .AddEntry(Color.Brown,
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Fall))
                        .AddEntry(Color.DeepSkyBlue,
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Winter))
                    )
                ),

                new Layer("Background/Time", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties {
                        _PrimaryColor = Color.Transparent
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor",new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.Violet,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Sunrise))
                        .AddEntry(Color.Coral,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Morning))
                        .AddEntry(Color.Yellow,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Daytime))
                        .AddEntry(Color.Orange,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Evening))
                        .AddEntry(Color.OrangeRed,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Twilight))
                        .AddEntry(Color.SlateBlue,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Night))
                        .AddEntry(Color.MediumBlue,
                                new BooleanGSIEnum("World/Time/Range", GSI.Nodes.TimeRange.Midnight))
                    )
                ),
            };
        }

    }
}