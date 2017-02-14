using Aurora.Settings;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.XCOM
{
    class XCOMSettings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public XCOMSettings()
        {
            //General
            IsEnabled = true;
            first_time_installed = false;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {
                new Settings.Layers.Layer("Camera Movement", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.Q, Devices.DeviceKeys.E, Devices.DeviceKeys.HOME, Devices.DeviceKeys.Z })
                    }
                }
                ),
                new Settings.Layers.Layer("Other Actions", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
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
