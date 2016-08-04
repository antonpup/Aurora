using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.Serious_Sam_3
{
    public class SSam3Settings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public SSam3Settings()
        {
            //General
            isEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.SPACE }, System.Drawing.Color.DarkRed, "Movement"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.R, Devices.DeviceKeys.Q, Devices.DeviceKeys.E, Devices.DeviceKeys.LEFT_SHIFT }, System.Drawing.Color.Orange, "Other Actions")
            };
        }
    }
}
