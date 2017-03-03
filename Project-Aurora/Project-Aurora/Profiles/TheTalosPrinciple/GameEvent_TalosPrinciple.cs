using Aurora.EffectsEngine;
using System.Collections.Generic;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.TheTalosPrinciple
{
    public class GameEvent_TalosPrinciple : GameEvent_Aurora_Wrapper
    {
        public GameEvent_TalosPrinciple()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("Talos - Color Zones").DrawColorZones((this.Profile.Settings as TalosPrincipleSettings).lighting_areas.ToArray()));
        }
    }
}
