using System.Collections.Generic;
using Aurora.Settings;

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
            isEnabled = true;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D }, System.Drawing.Color.White, "Movement"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.G, Devices.DeviceKeys.E, Devices.DeviceKeys.F, Devices.DeviceKeys.TAB }, System.Drawing.Color.Yellow, "Other Actions")
            };
        }
    }
}
