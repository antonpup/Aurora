using Aurora.Settings;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.Metro_Last_Light
{
    public class MetroLLSettings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public MetroLLSettings()
        {
            //General
            IsEnabled = true;
            first_time_installed = false;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {
                new Settings.Layers.Layer("Movement", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT })
                    }
                }
                ),
                new Settings.Layers.Layer("Weapons", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.R, Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.E })
                    }
                }
                ),
                new Settings.Layers.Layer("Inventory", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Blue,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.G, Devices.DeviceKeys.T, Devices.DeviceKeys.F, Devices.DeviceKeys.M, Devices.DeviceKeys.Q, Devices.DeviceKeys.N })
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
