using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.XCOM
{
    public class GameEvent_XCOM : GameEvent_Aurora_Wrapper
    {
        public GameEvent_XCOM()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("XCOM - Color Zones").DrawColorZones((this.Profile.Settings as XCOMSettings).lighting_areas.ToArray()));
        }
    }
}
