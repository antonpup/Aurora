using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.HotlineMiami
{
    public class GameEvent_HM : GameEvent_Aurora_Wrapper
    {
        public GameEvent_HM()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("Hotline - Color Zones").DrawColorZones((this.Profile.Settings as HMSettings).lighting_areas.ToArray()));
        }
    }
}
