using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using Common.Devices;

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
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.W, DeviceKeys.A, DeviceKeys.S, DeviceKeys.D, DeviceKeys.SPACE, DeviceKeys.Q, DeviceKeys.E, DeviceKeys.V, DeviceKeys.R })
                    }
                }
                ),
                new Layer("Skills", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.TILDE, DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO })
                    }
                }
                ),
                new Layer("Targeting", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.T, DeviceKeys.TAB })
                    }
                }
                ),
                new Layer("User Interface", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Brown,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.O, DeviceKeys.Y, DeviceKeys.G, DeviceKeys.H, DeviceKeys.I, DeviceKeys.K, DeviceKeys.F12, DeviceKeys.F11, DeviceKeys.P, DeviceKeys.ENTER, DeviceKeys.M, DeviceKeys.LEFT_CONTROL, DeviceKeys.LEFT_ALT })
                    }
                }
                ),
                new Layer("Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.F, DeviceKeys.C })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
