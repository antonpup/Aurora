using System.Collections.Generic;
using Aurora.Settings;
using System.Drawing;

namespace Aurora.Profiles.BF3
{
    public class BF3Settings : ProfileSettings
    {
        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public BF3Settings()
        {
            //General
            IsEnabled = true;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {
                new Settings.Layers.Layer("Movement", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D })
                    }
                }
                ),
                new Settings.Layers.Layer("Other Actions", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.G, Devices.DeviceKeys.E, Devices.DeviceKeys.F, Devices.DeviceKeys.TAB })
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
