using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Blade_and_Soul
{
    public class GameEvent_BnS : GameEvent_Aurora_Wrapper
    {
        public GameEvent_BnS()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            this.colorEnhance_Enabled = (this.Application.Profile as BnSProfile).colorEnhance_Enabled;
            this.colorEnhance_Mode = (this.Application.Profile as BnSProfile).colorEnhance_Mode;
            this.colorEnhance_color_factor = (this.Application.Profile as BnSProfile).colorEnhance_color_factor;
            this.colorEnhance_color_hsv_sine = (this.Application.Profile as BnSProfile).colorEnhance_color_hsv_sine;
            this.colorEnhance_color_hsv_gamma = (this.Application.Profile as BnSProfile).colorEnhance_color_hsv_gamma;
        }
    }
}
