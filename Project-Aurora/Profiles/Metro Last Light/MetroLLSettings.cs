using Aurora.Settings;
using System.Collections.Generic;

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
            isEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT }, System.Drawing.Color.Orange, "Movement"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.R, Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.E }, System.Drawing.Color.Red, "Weapons"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.G, Devices.DeviceKeys.T, Devices.DeviceKeys.F, Devices.DeviceKeys.M, Devices.DeviceKeys.Q, Devices.DeviceKeys.N }, System.Drawing.Color.Blue, "Inventory")
            };
        }
    }
}
