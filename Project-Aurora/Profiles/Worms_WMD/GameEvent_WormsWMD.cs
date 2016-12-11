﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.WormsWMD
{
    public class GameEvent_WormsWMD : GameEvent_Aurora_Wrapper
    {
        public GameEvent_WormsWMD()
        {
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            this.colorEnhance_Enabled = (this.Profile.Settings as WormsWMDSettings).colorEnhance_Enabled;
            this.colorEnhance_initial_factor = (this.Profile.Settings as WormsWMDSettings).colorEnhance_initial_factor;
            this.colorEnhance_color_factor = (this.Profile.Settings as WormsWMDSettings).colorEnhance_color_factor;
        }
    }
}
