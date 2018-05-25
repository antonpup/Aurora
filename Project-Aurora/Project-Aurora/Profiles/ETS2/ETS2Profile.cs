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
            Layers = new ObservableCollection<Layer>() {
                new Layer("Blinkers", new ETS2BlinkerLayerHandler()),

                new Layer("Beacon", new ETS2BeaconLayerHandler() {
                    Properties = new ETS2BeaconLayerProperties() {
                        _BeaconStyle = ETS2_BeaconStyle.Fancy_Flash,
                        _PrimaryColor = Color.FromArgb(255, 128, 0),
                        _Sequence = new KeySequence(new DeviceKeys[]{ DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8 })
                    }
                }),

                new Layer("Throttle", new PercentGradientLayerHandler() {
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

                new Layer("Keys", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(0, 255, 255),
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.W, DeviceKeys.A, DeviceKeys.S, DeviceKeys.D, // Throttle/steering
                            DeviceKeys.T, // Trailer attach/unattach
                            DeviceKeys.LEFT_SHIFT, DeviceKeys.LEFT_CONTROL, // Gear up/down
                            DeviceKeys.SPACE // Handbrake
                        })
                    }
                })
            };
        }

    }

}
