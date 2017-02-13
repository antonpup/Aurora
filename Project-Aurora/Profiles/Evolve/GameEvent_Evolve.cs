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

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("Evolve - Color Zones").DrawColorZones((this.Profile.Settings as EvolveSettings).lighting_areas.ToArray()));
        }
    }
}
