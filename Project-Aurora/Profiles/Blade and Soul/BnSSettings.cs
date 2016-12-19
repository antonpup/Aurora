using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.Blade_and_Soul
{
    public class BnSSettings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public BnSSettings()
        {
            //General
            isEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>();
        }
    }
}
