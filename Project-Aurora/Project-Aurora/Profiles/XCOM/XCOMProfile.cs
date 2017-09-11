using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Aurora.Profiles.XCOM
{
    class XCOMProfile : ApplicationProfile
    {
        public XCOMProfile()
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
                new Layer("Camera Movement", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.Q, Devices.DeviceKeys.E, Devices.DeviceKeys.HOME, Devices.DeviceKeys.Z })
                    }
                }
                ),
                new Layer("Other Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.DarkOrange,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.ENTER, Devices.DeviceKeys.ESC, Devices.DeviceKeys.V, Devices.DeviceKeys.X, Devices.DeviceKeys.BACKSPACE, Devices.DeviceKeys.F1, Devices.DeviceKeys.R, Devices.DeviceKeys.B, Devices.DeviceKeys.Y })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
