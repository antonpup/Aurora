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

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Blade and Soul - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as BnSSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
