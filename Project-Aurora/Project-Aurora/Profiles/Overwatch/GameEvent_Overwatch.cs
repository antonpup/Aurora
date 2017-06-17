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

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            this.colorEnhance_Enabled = (this.Application.Profile as OverwatchProfile).colorEnhance_Enabled;
            this.colorEnhance_Mode = (this.Application.Profile as OverwatchProfile).colorEnhance_Mode;
            this.colorEnhance_color_factor = (this.Application.Profile as OverwatchProfile).colorEnhance_color_factor;
            this.colorEnhance_color_hsv_sine = (this.Application.Profile as OverwatchProfile).colorEnhance_color_hsv_sine;
            this.colorEnhance_color_hsv_gamma = (this.Application.Profile as OverwatchProfile).colorEnhance_color_hsv_gamma;
        }
    }
}
