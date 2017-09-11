using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Aurora.Profiles.Metro_Last_Light
{
    public class MetroLLProfile : ApplicationProfile
    {
        public MetroLLProfile() : base()
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
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT })
                    }
                }
                ),
                new Layer("Weapons", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.R, Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.E })
                    }
                }
                ),
                new Layer("Inventory", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Blue,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.G, Devices.DeviceKeys.T, Devices.DeviceKeys.F, Devices.DeviceKeys.M, Devices.DeviceKeys.Q, Devices.DeviceKeys.N })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
