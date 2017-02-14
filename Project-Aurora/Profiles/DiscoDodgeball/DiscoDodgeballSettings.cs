using Aurora.Settings;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.DiscoDodgeball
{
    public class DiscoDodgeballSettings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public DiscoDodgeballSettings()
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
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.SPACE })
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
