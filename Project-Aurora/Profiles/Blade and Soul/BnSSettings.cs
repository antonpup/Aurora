using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.Blade_and_Soul
{
    public class BnSSettings : ProfileSettings
    {
        //Effects
        //// Color Enhancing
        public bool colorEnhance_Enabled;
        public float colorEnhance_initial_factor;
        public int colorEnhance_color_factor;

        public BnSSettings()
        {
            //General
            isEnabled = true;

            //Effects
            //// Color Enhancing
            colorEnhance_Enabled = true;
            colorEnhance_initial_factor = 3.0f;
            colorEnhance_color_factor = 90;
        }
    }
}
