using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using Common.Devices;

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
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.W, DeviceKeys.A, DeviceKeys.S, DeviceKeys.D, DeviceKeys.Q, DeviceKeys.E, DeviceKeys.HOME, DeviceKeys.Z })
                    }
                }
                ),
                new Layer("Other Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.DarkOrange,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.ENTER, DeviceKeys.ESC, DeviceKeys.V, DeviceKeys.X, DeviceKeys.BACKSPACE, DeviceKeys.F1, DeviceKeys.R, DeviceKeys.B, DeviceKeys.Y })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
