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
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.Q, Devices.DeviceKeys.E, Devices.DeviceKeys.V, Devices.DeviceKeys.R })
                    }
                }
                ),
                new Layer("Skills", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.TILDE, Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO })
                    }
                }
                ),
                new Layer("Targeting", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.T, Devices.DeviceKeys.TAB })
                    }
                }
                ),
                new Layer("User Interface", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Brown,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.O, Devices.DeviceKeys.Y, Devices.DeviceKeys.G, Devices.DeviceKeys.H, Devices.DeviceKeys.I, Devices.DeviceKeys.K, Devices.DeviceKeys.F12, Devices.DeviceKeys.F11, Devices.DeviceKeys.P, Devices.DeviceKeys.ENTER, Devices.DeviceKeys.M, Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.LEFT_ALT })
                    }
                }
                ),
                new Layer("Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.F, Devices.DeviceKeys.C })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
