using System.Collections.Generic;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Drawing;
using System.Runtime.Serialization;
using System.Linq;

namespace Aurora.Profiles.BF3
{
    public class BF3Profile : ApplicationProfile
    {
        public BF3Profile() : base()
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
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D })
                    }
                }),
                new Layer("Other Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.G, Devices.DeviceKeys.E, Devices.DeviceKeys.F, Devices.DeviceKeys.TAB })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
