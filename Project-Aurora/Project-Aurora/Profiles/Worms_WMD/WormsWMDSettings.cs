using Aurora.Settings;

namespace Aurora.Profiles.WormsWMD
{
    public class WormsWMDSettings : ProfileSettings
    {
        //Effects
        //// Color Enhancing
        public bool colorEnhance_Enabled;
        public int colorEnhance_Mode;
        public int colorEnhance_color_factor;
        public float colorEnhance_color_hsv_sine;
        public float colorEnhance_color_hsv_gamma;

        public WormsWMDSettings()
        {
            //General
            IsEnabled = true;

            //Effects
            //// Color Enhancing
            colorEnhance_Enabled = true;
            colorEnhance_Mode = 0;
            colorEnhance_color_factor = 90;
            colorEnhance_color_hsv_sine = 0.1f;
            colorEnhance_color_hsv_gamma = 2.5f;
        }
    }
}
