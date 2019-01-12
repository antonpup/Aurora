using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
//using Aurora.Profiles.Subnautica.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Profiles.Subnautica {

    public class SubnauticaProfile : ApplicationProfile {

        public SubnauticaProfile() : base() { }
        
        public override void Reset() {
            base.Reset();

            // Keys that do something and should be highlighted in a static color
            DK[] controlKeys = new[] { DK.W, DK.A, DK.S, DK.D, DK.E, DK.SPACE, DK.LEFT_SHIFT, DK.LEFT_CONTROL };

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>() {

                new Layer("Health", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Health",
                        _MaxVariablePath = "100",
                        _PrimaryColor = Color.FromArgb(255, 0, 0),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12
                        })
                    }
                }),

                new Layer("Food", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Food",
                        _MaxVariablePath = "100",
                        _PrimaryColor = Color.FromArgb(139, 69, 19),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.Q, DK.W, DK.E, DK.R, DK.T, DK.Y, DK.U, DK.I, DK.O, DK.P
                        })
                    }
                }),

                new Layer("Water", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Water",
                        _MaxVariablePath = "100",
                        _PrimaryColor = Color.FromArgb(0, 0, 255),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.A, DK.S, DK.D, DK.F, DK.G, DK.H, DK.J, DK.K,DK.L
                        })
                    }
                }),

                new Layer("Oxygen", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/OxygenAvailable",
                        _MaxVariablePath = "Player/OxygenCapacity",
                        _PrimaryColor = Color.FromArgb(29, 131, 176),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.ONE, DK.TWO, DK.THREE, DK.FOUR, DK.FIVE, DK.SIX, DK.SEVEN, DK.EIGHT, DK.NINE, DK.ZERO, DK.MINUS, DK.EQUALS
                        })
                    }
                }),

                new Layer("Background", new PercentGradientLayerHandler() {
                    Properties = new PercentGradientLayerHandlerProperties {
                        _Gradient = new EffectBrush() {
                            type = EffectBrush.BrushType.Linear,
                            start = new PointF(0, 0),
                            end = new PointF(1, 0),
                            colorGradients = new SortedDictionary<float, Color> {
                                { 0, Color.FromArgb(46, 176, 255) },
                                { 1, Color.FromArgb(0, 3, 53) }
                            }
                        },
                        _VariablePath = "Player/DepthLevel",
                        _MaxVariablePath = "-40",
                        _PrimaryColor = Color.FromArgb(29, 131, 176),
                        _SecondaryColor = Color.Transparent,
                        _PercentType = PercentEffectType.AllAtOnce,
                        _Sequence = new KeySequence(new FreeFormObject(0, -36, 980, 265))
                    }
                }),

            };
        }
        
    }
}
