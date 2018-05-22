using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.ETS2.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.ETS2 {

    public class ETS2Profile : ApplicationProfile {

        public ETS2Profile() : base() { }

        public override void Reset() {
            base.Reset();
            Layers = new ObservableCollection<Layer>() {
                new Layer("ETS2 Blinker", new ETS2BlinkerLayerHandler()),

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
