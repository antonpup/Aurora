using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.XCOM
{
    class XCOMProfile : ApplicationProfile
    {
        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public XCOMProfile()
        {
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
                }
                )
            };

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>();
        }
    }
}
