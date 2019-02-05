using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Aurora.Profiles.DyingLight
{
    public class DyingLightProfile : ApplicationProfile
    {
        public DyingLightProfile() : base()
        {
            
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (!Layers.Any(lyr => lyr.Handler.GetType().Equals(typeof(Aurora.Settings.Layers.WrapperLightsLayerHandler))))
                Layers.Add(new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()));
        }


        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Movement", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.DodgerBlue,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.W, KeyboardKeys.A, KeyboardKeys.S, KeyboardKeys.D, KeyboardKeys.SPACE, KeyboardKeys.C, KeyboardKeys.LEFT_SHIFT })
                    }
                }
                ),
                new Layer("Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.DarkOrange,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.Q, KeyboardKeys.E, KeyboardKeys.R, KeyboardKeys.F, KeyboardKeys.V, KeyboardKeys.B })
                    }
                }
                ),
                new Layer("Inventory", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Magenta,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.I, KeyboardKeys.M, KeyboardKeys.L, KeyboardKeys.U})
                    }
                }
                ),
                new Layer("Healing", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.H}),
                    }
                }
                ),
                new Layer("Flashlight", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.T})
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()
                {
                    Properties = new WrapperLightsLayerHandlerProperties()
                    {
                        _ColorEnhanceMode = 1            
                    }
                })
            };
        }
    }
}
