using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class LoLProfile : ApplicationProfile
    {
        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }
        public bool disable_cz_on_dark;

        public LoLProfile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {
                new ColorZone(new Devices.DeviceKeys[] { Devices.DeviceKeys.Q, Devices.DeviceKeys.W, Devices.DeviceKeys.E, Devices.DeviceKeys.R }, System.Drawing.Color.Blue, "Abilities")
            };
            disable_cz_on_dark = true;
        }
    }
}
