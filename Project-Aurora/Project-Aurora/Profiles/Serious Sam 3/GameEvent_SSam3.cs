using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Serious_Sam_3
{
    public class GameEvent_SSam3 : GameEvent_Aurora_Wrapper
    {
        public GameEvent_SSam3()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("SSam3 - Color Zones").DrawColorZones((this.Profile.Settings as SSam3Settings).lighting_areas.ToArray()));
        }
    }
}
