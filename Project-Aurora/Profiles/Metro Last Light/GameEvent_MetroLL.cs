using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Metro_Last_Light
{
    public class GameEvent_MetroLL : GameEvent_Aurora_Wrapper
    {
        public GameEvent_MetroLL()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("MetroLL - Color Zones").DrawColorZones((this.Profile?.Settings as MetroLLSettings).lighting_areas.ToArray()));
        }
    }
}
