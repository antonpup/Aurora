using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
//using Aurora.Profiles.Slime_Rancher.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Profiles.Slime_Rancher
{

    public class SlimeRancherProfile : ApplicationProfile {

        public SlimeRancherProfile() : base() { }
        
        public override void Reset() {
            base.Reset();

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>() {
                new Layer("Health", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Health/Current",
                        _MaxVariablePath = "Player/Health/Max",
                        _PrimaryColor = Color.FromArgb(255, 0, 0),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12
                        }),
                        _BlinkThreshold = 0.25
                    }
                }),

                new Layer("Energy", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Energy/Current",
                        _MaxVariablePath = "Player/Energy/Max",
                        _PrimaryColor = Color.FromArgb(9, 173, 233),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.Q, DK.W, DK.E, DK.R, DK.T, DK.Y, DK.U, DK.I, DK.O, DK.P
                        }),
                        _BlinkThreshold = 0.25
                    }
                }),

                new Layer("Radiation", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Radiation/Current",
                        _MaxVariablePath = "Player/Radiation/Max",
                        _PrimaryColor = Color.FromArgb(60, 233, 118),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.A, DK.S, DK.D, DK.F, DK.G, DK.H, DK.J, DK.K,DK.L
                        }),
                        _BlinkThreshold = 0.25
                    }
                }),
                
            };
        }
        
    }
}
