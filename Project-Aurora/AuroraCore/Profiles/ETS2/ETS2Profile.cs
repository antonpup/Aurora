using Aurora.Devices.Layout.Layouts;
using Aurora.EffectsEngine;
using Aurora.Profiles.ETS2.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace Aurora.Profiles.ETS2
{

    public enum ETS2_BeaconStyle
    {
        [Description("Simple Flashing")]
        Simple_Flash,
        [Description("Half Alternating")]
        Half_Alternating,
        [Description("Fancy Flashing")]
        Fancy_Flash,
        [Description("Side-to-Side")]
        Side_To_Side
    }

    public class ETS2Profile : ApplicationProfile
    {

        public ETS2Profile() : base() { }

        public override void Reset()
        {
            base.Reset();

            Color brightColor = Color.FromArgb(0, 255, 255);
            Color dimColor = Color.FromArgb(128, 0, 0, 255);
            Color hazardColor = Color.FromArgb(255, 128, 0);

            Layers = new ObservableCollection<Layer> {
                new Layer("Ignition Key", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = hazardColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.E })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanNot(new BooleanGSIBoolean("Truck/engineOn")))
                ),

                // This layer will hide all the other buttons (giving the impression the dashboard is turned off) when the ignition isn't on.
                new Layer("Dashboard Off Mask", new SolidFillLayerHandler {
                    Properties = new SolidFillLayerHandlerProperties {
                        _PrimaryColor = Color.Black,
                        _Sequence = new KeySequence(new FreeFormObject(0, 0, 830, 225))
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanNot(new BooleanGSIBoolean("Truck/electricOn")))
                ),

                new Layer("Throttle Key", new PercentGradientLayerHandler {
                    Properties = new PercentGradientLayerHandlerProperties {
                        _Gradient = new EffectBrush() {
                            type = EffectBrush.BrushType.Linear,
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0, Color.FromArgb(0, 255, 255) },
                                { 1, Color.FromArgb(0, 255, 0) }
                            }
                        },
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.W }),
                        _VariablePath = "Truck/gameThrottle",
                        _MaxVariablePath = "1",
                        _PercentType = PercentEffectType.AllAtOnce
                    }
                }),

                new Layer("Brake Key", new PercentGradientLayerHandler {
                    Properties = new PercentGradientLayerHandlerProperties {
                        _Gradient = new EffectBrush() {
                            type = EffectBrush.BrushType.Linear,
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0, Color.FromArgb(0, 255, 255) },
                                { 1, Color.FromArgb(255, 0, 0) }
                            }
                        },
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.S }),
                        _VariablePath = "Truck/gameBrake",
                        _MaxVariablePath = "1",
                        _PercentType = PercentEffectType.AllAtOnce
                    }
                }),

                new Layer("Bright Keys", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = brightColor,
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.A, KeyboardKeys.D, // Steering
                            KeyboardKeys.LEFT_SHIFT, KeyboardKeys.LEFT_CONTROL, // Gear up/down
                            KeyboardKeys.F, // Hazard light
                            KeyboardKeys.H // Horn
                        })
                    }
                }),

                new Layer("RPM", new PercentGradientLayerHandler {
                    Properties = new PercentGradientLayerHandlerProperties {
                        _Gradient = new EffectBrush() {
                            type = EffectBrush.BrushType.Linear,
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0, Color.FromArgb(65, 255, 0) },
                                { 0.65f, Color.FromArgb(67, 255, 0) },
                                { 0.75f, Color.FromArgb(0, 100, 255) },
                                { 0.85f, Color.FromArgb(255, 0, 0) },
                                { 1, Color.FromArgb(255, 0, 0) },
                            }
                        },
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR, KeyboardKeys.FIVE,
                            KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT, KeyboardKeys.NINE, KeyboardKeys.ZERO
                        }),
                        _VariablePath = "Truck/engineRpm",
                        _MaxVariablePath = "Truck/engineRpmMax"
                    }
                }),

                new Layer("Parking Brake Key", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = brightColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.SPACE })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.Red, new BooleanGSIBoolean("Truck/parkBrakeOn"))
                    )
                ),

                new Layer("Headlights (High Beam)", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = dimColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.K })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.White, new BooleanGSIBoolean("Truck/lightsBeamHighOn"))
                    )
                ),

                new Layer("Headlights", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties() {
                        _PrimaryColor = dimColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.L })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.White, new BooleanGSIBoolean("Truck/lightsBeamLowOn")) // When low beams are on, light up full white
                        .AddEntry(Color.FromArgb(128, 255, 255, 255), new BooleanGSIBoolean("Truck/lightsParkingOn")) // When parking lights are on, light up dim white
                    )
                ),

                new Layer("Left Blinkers", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = hazardColor,
                        _Sequence = new KeySequence(new[] { KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4 })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Truck/blinkerLeftOn"))
                ),

                new Layer("Right Blinkers", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = hazardColor,
                        _Sequence = new KeySequence(new[] { KeyboardKeys.F9, KeyboardKeys.F10, KeyboardKeys.F11, KeyboardKeys.F12 })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Truck/blinkerRightOn"))
                ),

                new Layer("Left Blinker Button", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = dimColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.OPEN_BRACKET })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(hazardColor, new BooleanGSIBoolean("Truck/blinkerLeftActive"))
                    )
                ),

                new Layer("Left Blinker Button", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = dimColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.CLOSE_BRACKET })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(hazardColor, new BooleanGSIBoolean("Truck/blinkerRightActive"))
                    )
                ),

                new Layer("Beacon", new ETS2BeaconLayerHandler {
                    Properties = new ETS2BeaconLayerProperties {
                        _BeaconStyle = ETS2_BeaconStyle.Fancy_Flash,
                        _PrimaryColor = hazardColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.F5, KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8 })
                    }
                }),

                new Layer("Beacon Button", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = dimColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.O })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(hazardColor, new BooleanGSIBoolean("Truck/lightsBeaconOn"))
                    )
                ),

                new Layer("Trailer Button", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = Color.FromArgb(128, 0, 0, 255),
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.T })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.Lime, new BooleanGSIBoolean("Trailer/attached"))
                    )
                ),

                new Layer("Cruise Control Button", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = dimColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.C })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(brightColor, new BooleanGSIBoolean("Truck/cruiseControlOn"))
                    )
                ),

                new Layer("Fuel", new PercentGradientLayerHandler {
                    Properties = new PercentGradientLayerHandlerProperties {
                        _Gradient = new EffectBrush() {
                            type = EffectBrush.BrushType.Linear,
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0f, Color.FromArgb(255, 0, 0) },
                                { 0.25f, Color.FromArgb(255, 0, 0) },
                                { 0.375f, Color.FromArgb(255, 255, 0) },
                                { 0.5f, Color.FromArgb(0, 255, 0) },
                                { 1f, Color.FromArgb(0, 255, 0) }
                            }
                        },
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.NUM_ONE, KeyboardKeys.NUM_FOUR, KeyboardKeys.NUM_SEVEN, KeyboardKeys.NUM_LOCK
                        }),
                        _VariablePath = "Truck/fuel",
                        _MaxVariablePath = "Truck/fuelCapacity"
                    }
                }),

                new Layer("Air Pressure", new PercentGradientLayerHandler {
                    Properties = new PercentGradientLayerHandlerProperties {
                        _Gradient = new EffectBrush() {
                            type = EffectBrush.BrushType.Linear,
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0f, Color.FromArgb(255, 0, 0) },
                                { 0.25f, Color.FromArgb(255, 0, 0) },
                                { 0.375f, Color.FromArgb(255, 255, 0) },
                                { 0.5f, Color.FromArgb(0, 255, 0) },
                                { 1f, Color.FromArgb(0, 255, 0) }
                            }
                        },
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.NUM_THREE, KeyboardKeys.NUM_SIX, KeyboardKeys.NUM_NINE, KeyboardKeys.NUM_ASTERISK
                        }),
                        _VariablePath = "Truck/airPressure",
                        _MaxVariablePath = "Truck/airPressureMax"
                    }
                }),

                new Layer("Wipers Button", new SolidColorLayerHandler {
                    Properties = new LayerHandlerProperties {
                        _PrimaryColor = dimColor,
                        _Sequence = new KeySequence(new KeyboardKeys[]{ KeyboardKeys.P })
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(brightColor, new BooleanGSIBoolean("Truck/wipersOn"))
                    )
                )
            };
        }

    }

}
