using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Magic_Duels_2012
{
    public class GameEvent_MagicDuels2012 : GameEvent_Aurora_Wrapper
    {
        public GameEvent_MagicDuels2012()
        {
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            layers.Enqueue(new EffectLayer("Magic Duels 2012 - Color Zones").DrawColorZones((this.Profile.Settings as MagicDuels2012Settings).lighting_areas.ToArray()));
        }

    }
}
