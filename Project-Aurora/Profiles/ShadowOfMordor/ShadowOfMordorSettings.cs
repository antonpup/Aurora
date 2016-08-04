using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.ShadowOfMordor
{
    public class ShadowOfMordorSettings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public ShadowOfMordorSettings()
        {
            //General
            isEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.SPACE }, System.Drawing.Color.Blue, "Movement"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.V, Devices.DeviceKeys.I, Devices.DeviceKeys.K, Devices.DeviceKeys.M }, System.Drawing.Color.LightBlue, "Other Actions")
            };
        }
    }
}
