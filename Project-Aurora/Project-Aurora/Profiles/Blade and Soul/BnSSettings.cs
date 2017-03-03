﻿using Aurora.Settings;
using System.Collections.Generic;

namespace Aurora.Profiles.Blade_and_Soul
{
    public class BnSSettings : ProfileSettings
    {
        //Effects
        //// Color Enhancing
        public bool colorEnhance_Enabled;
        public int colorEnhance_Mode;
        public int colorEnhance_color_factor;
        public float colorEnhance_color_hsv_sine;
        public float colorEnhance_color_hsv_gamma;

        public BnSSettings()
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
