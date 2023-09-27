using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using Common.Devices;

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
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.W, DeviceKeys.A, DeviceKeys.S, DeviceKeys.D, DeviceKeys.LEFT_CONTROL, DeviceKeys.SPACE, DeviceKeys.LEFT_SHIFT })
                    }
                }
                ),
                new Layer("Weapons", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.R, DeviceKeys.C, DeviceKeys.V, DeviceKeys.E })
                    }
                }
                ),
                new Layer("Inventory", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Blue,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.G, DeviceKeys.T, DeviceKeys.F, DeviceKeys.M, DeviceKeys.Q, DeviceKeys.N })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
