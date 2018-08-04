using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Minecraft.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
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
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>() {
                new Layer("Health Bar", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Health",
                        _MaxVariablePath = "Player/HealthMax",
                        _PrimaryColor = Color.Red,
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.Z, DK.X, DK.C, DK.V, DK.B, DK.N, DK.M, DK.COMMA, DK.PERIOD, DK.FORWARD_SLASH
                        })
                    }
                }),

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
                }),

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
                }),

                new Layer("Keys", new ConditionalLayerHandler() {
                    Properties = new ConditionalLayerProperties() {
                        _ConditionPath = "Player/InGame",
                        _PrimaryColor = Color.FromArgb(0, 255, 255),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.W, DK.A, DK.S, DK.D, DK.E, DK.SPACE, DK.LEFT_SHIFT, DK.LEFT_CONTROL
                        })
                    }
                }),

                new Layer("On Fire", new MinecraftBurnLayerHandler()),

                new Layer("Raining", new MinecraftRainLayerHandler()),

                new Layer("Grass Block Top", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(44, 168, 32),
                        _Sequence = new KeySequence(new FreeFormObject(0, -60, 900, 128))
                    }
                }),

                new Layer("Grass Block Side", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(102, 59, 20)
                    }
                })
            };
        }
    }
}
