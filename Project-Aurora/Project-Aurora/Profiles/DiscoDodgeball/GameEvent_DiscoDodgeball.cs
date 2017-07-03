using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.DiscoDodgeball
{
    public class GameEvent_DiscoDodgeball : GameEvent_Aurora_Wrapper
    {
        public GameEvent_DiscoDodgeball()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("Disco Dodgeball - Color Zones").DrawColorZones((this.Application.Profile as DiscoDodgeballProfile).lighting_areas.ToArray()));
        }
    }
}
