using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.ShadowOfMordor
{
    public class GameEvent_ShadowOfMordor : GameEvent_Aurora_Wrapper
    {
        public GameEvent_ShadowOfMordor()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("Shadow of Mordor - Color Zones").DrawColorZones((this.Profile.Settings as ShadowOfMordorSettings).lighting_areas.ToArray()));
        }
    }
}
