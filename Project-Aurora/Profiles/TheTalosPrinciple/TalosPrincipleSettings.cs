using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.TheTalosPrinciple
{
    public class TalosPrincipleSettings
    {
        //General
        public bool isEnabled;
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public TalosPrincipleSettings()
        {
            //General
            isEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D }, System.Drawing.Color.LightBlue, "Movement"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.H, Devices.DeviceKeys.X, Devices.DeviceKeys.TAB }, System.Drawing.Color.Purple, "Other Actions")
            };
        }
    }
}
