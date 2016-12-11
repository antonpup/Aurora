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

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Magic Duels 2012 - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as MagicDuels2012Settings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }

    }
}
