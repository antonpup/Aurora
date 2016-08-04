using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.HotlineMiami
{
    public class HMSettings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public HMSettings()
        {
            //General
            isEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D }, System.Drawing.Color.Yellow, "Movement"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.R, Devices.DeviceKeys.ESC }, System.Drawing.Color.Red, "Other Actions")
            };
        }
    }
}
