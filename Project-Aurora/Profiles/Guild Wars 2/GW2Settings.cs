using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.Guild_Wars_2
{
    public class GW2Settings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public GW2Settings()
        {
            //General
            isEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.Q, Devices.DeviceKeys.E, Devices.DeviceKeys.V, Devices.DeviceKeys.R }, System.Drawing.Color.Orange, "Movement"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.TILDE, Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO }, System.Drawing.Color.Green, "Skills"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.T, Devices.DeviceKeys.TAB }, System.Drawing.Color.Red, "Targeting"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.O, Devices.DeviceKeys.Y, Devices.DeviceKeys.G, Devices.DeviceKeys.H, Devices.DeviceKeys.I, Devices.DeviceKeys.K, Devices.DeviceKeys.F12, Devices.DeviceKeys.F11, Devices.DeviceKeys.P, Devices.DeviceKeys.ENTER, Devices.DeviceKeys.M, Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.LEFT_ALT }, System.Drawing.Color.Brown, "User Interface"),
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.F, Devices.DeviceKeys.C }, System.Drawing.Color.Yellow, "Actions")
            };
        }
    }
}
