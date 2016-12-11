using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.Evolve
{
    public class EvolveSettings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public EvolveSettings()
        {
            //General
            isEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_CONTROL }, System.Drawing.Color.DarkRed, "Movement"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR }, System.Drawing.Color.OrangeRed, "Other Actions")
            };
        }
    }
}
