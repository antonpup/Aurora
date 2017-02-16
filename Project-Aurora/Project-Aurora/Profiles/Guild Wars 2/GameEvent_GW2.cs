using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Guild_Wars_2
{
    public class GameEvent_GW2 : GameEvent_Aurora_Wrapper
    {
        public GameEvent_GW2()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("Guild Wars 2 - Color Zones").DrawColorZones((this.Profile.Settings as GW2Settings).lighting_areas.ToArray()));
        }
    }
}
