using Aurora.Settings;

namespace Aurora.Profiles.Overwatch
{
    public class OverwatchSettings : ProfileSettings
    {
        //Effects
        //// Color Enhancing
        public bool colorEnhance_Enabled;
        public int colorEnhance_Mode;
        public int colorEnhance_color_factor;
        public float colorEnhance_color_simple;
        public float colorEnhance_color_gamma;

        public OverwatchSettings()
        {
            //General
            isEnabled = true;

            //Effects
            //// Color Enhancing
            colorEnhance_Enabled = true;
            colorEnhance_Mode = 0;
            colorEnhance_color_factor = 90;
            colorEnhance_color_simple = 1.2f;
            colorEnhance_color_gamma = 2.5f;
        }
    }
}
