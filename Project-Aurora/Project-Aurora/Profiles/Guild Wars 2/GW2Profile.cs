using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Aurora.Profiles.Guild_Wars_2
{
    public class GW2Profile : ApplicationProfile
    {
        public GW2Profile() : base()
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
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.W, KeyboardKeys.A, KeyboardKeys.S, KeyboardKeys.D, KeyboardKeys.SPACE, KeyboardKeys.Q, KeyboardKeys.E, KeyboardKeys.V, KeyboardKeys.R })
                    }
                }
                ),
                new Layer("Skills", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.TILDE, KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR, KeyboardKeys.FIVE, KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT, KeyboardKeys.NINE, KeyboardKeys.ZERO })
                    }
                }
                ),
                new Layer("Targeting", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.T, KeyboardKeys.TAB })
                    }
                }
                ),
                new Layer("User Interface", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Brown,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.O, KeyboardKeys.Y, KeyboardKeys.G, KeyboardKeys.H, KeyboardKeys.I, KeyboardKeys.K, KeyboardKeys.F12, KeyboardKeys.F11, KeyboardKeys.P, KeyboardKeys.ENTER, KeyboardKeys.M, KeyboardKeys.LEFT_CONTROL, KeyboardKeys.LEFT_ALT })
                    }
                }
                ),
                new Layer("Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.F, KeyboardKeys.C })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
