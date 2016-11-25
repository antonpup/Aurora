using Aurora.Settings;

namespace Aurora.Profiles.WormsWMD
{
    public class WormsWMDSettings : ProfileSettings
    {
        //Effects
        //// Color Enhancing
        public bool colorEnhance_Enabled;
        public float colorEnhance_initial_factor;
        public int colorEnhance_color_factor;

        public WormsWMDSettings()
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
