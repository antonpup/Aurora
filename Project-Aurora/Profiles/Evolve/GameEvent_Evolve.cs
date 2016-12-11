using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Evolve
{
    public class GameEvent_Evolve : GameEvent_Aurora_Wrapper
    {
        public GameEvent_Evolve()
        {
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Evolve - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as EvolveSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
