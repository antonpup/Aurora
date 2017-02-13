using System.Collections.Generic;
using Aurora.Settings;

namespace Aurora.Profiles.Magic_Duels_2012
{
    public class MagicDuels2012Settings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public MagicDuels2012Settings()
        {
            //General
            IsEnabled = true;
            first_time_installed = false;

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() {};
        }
    }
}
