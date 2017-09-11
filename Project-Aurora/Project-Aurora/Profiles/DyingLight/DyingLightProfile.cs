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
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_CONTROL })
                    }
                }
                ),
                new Layer("Healing", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.H}),
                    }
                }
                ),
                new Layer("Flashlight", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.T})
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler())
            };
        }
    }
}
