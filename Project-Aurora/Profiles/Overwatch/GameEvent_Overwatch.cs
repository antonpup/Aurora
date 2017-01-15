using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.Overwatch
{
    public class GameEvent_Overwatch : GameEvent_Aurora_Wrapper
    {
        public GameEvent_Overwatch()
        {
        }

        public override bool IsEnabled()
        {
            return (this.Profile.Settings as OverwatchSettings).isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            this.colorEnhance_Enabled = (this.Profile.Settings as OverwatchSettings).colorEnhance_Enabled;
            this.colorEnhance_Mode = (this.Profile.Settings as OverwatchSettings).colorEnhance_Mode;
            this.colorEnhance_color_factor = (this.Profile.Settings as OverwatchSettings).colorEnhance_color_factor;
            this.colorEnhance_color_hsv_sine = (this.Profile.Settings as OverwatchSettings).colorEnhance_color_hsv_sine;
            this.colorEnhance_color_hsv_gamma = (this.Profile.Settings as OverwatchSettings).colorEnhance_color_hsv_gamma;
        }
    }
}
