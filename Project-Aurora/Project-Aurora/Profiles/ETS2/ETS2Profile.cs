using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.ETS2.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.ETS2 {

    public enum ETS2_BeaconStyle {
        [Description("Simple Flashing")]
        Simple_Flash,
        [Description("Half Alternating")]
        Half_Alternating,
        [Description("Fancy Flashing")]
        Fancy_Flash,
        [Description("Side-to-Side")]
        Side_To_Side
    }

    public class ETS2Profile : ApplicationProfile {

        public ETS2Profile() : base() { }

        public override void Reset() {
            base.Reset();

            Color brightColor = Color.FromArgb(0, 255, 255);
            Color dimColor = Color.FromArgb(128, 0, 0, 255);
            Color hazardColor = Color.FromArgb(255, 128, 0);

            Layers = new ObservableCollection<Layer>() {
                new Layer("Ignition Key", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = Color.FromArgb(0, 0, 0, 0),
                        _SecondaryColor = hazardColor,
                        _ConditionPath = "Truck/engineOn",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.E })
                    }
                }),

                // This layer will hide all the other buttons (giving the impression the dashboard is turned off) when the ignition isn't on.
                new Layer("Dashboard Off Mask", new ConditionalLayerHandler(){
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = Color.FromArgb(0, 0, 0, 0),
                        _SecondaryColor = Color.FromArgb(255, 0, 0, 0),
                        _ConditionPath = "Truck/electricOn",
                        _Sequence = new KeySequence(new FreeFormObject(0, 0, 830, 225))
                    }
                }),

                new Layer("Throttle Key", new PercentGradientLayerHandler() {
                    Properties = new PercentGradientLayerHandlerProperties() {
                        _Gradient = new EffectBrush() {
                            type = EffectBrush.BrushType.Linear,
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0, Color.FromArgb(0, 255, 255) },
                                { 1, Color.FromArgb(0, 255, 0) }
                            }
                        },
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.W }),
                        _VariablePath = "Truck/gameThrottle",
                        _MaxVariablePath = "1",
                        _PercentType = PercentEffectType.AllAtOnce
                    }
                }),

                new Layer("Brake Key", new PercentGradientLayerHandler() {
                    Properties = new PercentGradientLayerHandlerProperties() {
                        _Gradient = new EffectBrush() {
                            type = EffectBrush.BrushType.Linear,
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0, Color.FromArgb(0, 255, 255) },
                                { 1, Color.FromArgb(255, 0, 0) }
                            }
                        },
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.S }),
                        _VariablePath = "Truck/gameBrake",
                        _MaxVariablePath = "1",
                        _PercentType = PercentEffectType.AllAtOnce
                    }
                }),

                new Layer("Bright Keys", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties() {
                        _PrimaryColor = brightColor,
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.A, DeviceKeys.D, // Steering
                            DeviceKeys.LEFT_SHIFT, DeviceKeys.LEFT_CONTROL, // Gear up/down
                            DeviceKeys.F, // Hazard light
                            DeviceKeys.H // Horn
                        })
                    }
                }),

                new Layer("RPM", new PercentGradientLayerHandler() {
                    Properties = new PercentGradientLayerHandlerProperties() {
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
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE,
                            DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO
                        }),
                        _VariablePath = "Truck/engineRpm",
                        _MaxVariablePath = "Truck/engineRpmMax"
                    }
                }),

                new Layer("Parking Brake Key", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = Color.FromArgb(255, 0, 0),
                        _SecondaryColor = brightColor,
                        _ConditionPath = "Truck/parkBrakeOn",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.SPACE })
                    }
                }),

                new Layer("Headlights (High Beam)", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = Color.FromArgb(255, 255, 255),
                        _SecondaryColor = dimColor,
                        _ConditionPath = "Truck/lightsBeamHighOn",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.K })
                    }
                }),

                new Layer("Headlights (Low Beam)", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = Color.FromArgb(255, 255, 255),
                        _SecondaryColor = Color.FromArgb(0, 0, 0, 0),
                        _ConditionPath = "Truck/lightsBeamLowOn",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.L })
                    }
                }),

                new Layer("Headlights (Parking)", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = Color.FromArgb(128, 255, 255, 255),
                        _SecondaryColor = dimColor,
                        _ConditionPath = "Truck/lightsParkingOn",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.L })
                    }
                }),

                new Layer("Blinkers", new ETS2BlinkerLayerHandler()),

                new Layer("Left Blinker Button", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = hazardColor,
                        _SecondaryColor = dimColor,
                        _ConditionPath = "Truck/blinkerLeftActive",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.OPEN_BRACKET })
                    }
                }),

                new Layer("Right Blinker Button", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = hazardColor,
                        _SecondaryColor = dimColor,
                        _ConditionPath = "Truck/blinkerRightActive",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.CLOSE_BRACKET })
                    }
                }),

                new Layer("Beacon", new ETS2BeaconLayerHandler() {
                    Properties = new ETS2BeaconLayerProperties() {
                        _BeaconStyle = ETS2_BeaconStyle.Fancy_Flash,
                        _PrimaryColor = hazardColor,
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8 })
                    }
                }),

                new Layer("Beacon Button", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = hazardColor,
                        _SecondaryColor = dimColor,
                        _ConditionPath = "Truck/lightsBeaconOn",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.O })
                    }
                }),

                new Layer("Trailer Key", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = Color.FromArgb(0, 255, 0),
                        _SecondaryColor = Color.FromArgb(128, 0, 0, 255),
                        _ConditionPath = "Trailer/attached",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.T })
                    }
                }),

                new Layer("Cruise Control Button", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = brightColor,
                        _SecondaryColor = dimColor,
                        _ConditionPath = "Truck/cruiseControlOn",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.C })
                    }
                }),

                new Layer("Fuel", new PercentGradientLayerHandler() {
                    Properties = new PercentGradientLayerHandlerProperties() {
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
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.NUM_ONE, DeviceKeys.NUM_FOUR, DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_LOCK
                        }),
                        _VariablePath = "Truck/fuel",
                        _MaxVariablePath = "Truck/fuelCapacity"
                    }
                }),

                new Layer("Air Pressure", new PercentGradientLayerHandler() {
                    Properties = new PercentGradientLayerHandlerProperties() {
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
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.NUM_THREE, DeviceKeys.NUM_SIX, DeviceKeys.NUM_NINE, DeviceKeys.NUM_ASTERISK
                        }),
                        _VariablePath = "Truck/airPressure",
                        _MaxVariablePath = "Truck/airPressureMax"
                    }
                }),

                new Layer("Wiper Button", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _PrimaryColor = brightColor,
                        _SecondaryColor = dimColor,
                        _ConditionPath = "Truck/wipersOn",
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.P })
                    }
                }),
            };
        }

    }

}
