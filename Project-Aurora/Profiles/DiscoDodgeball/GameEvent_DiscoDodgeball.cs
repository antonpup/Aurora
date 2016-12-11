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

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Disco Dodgeball - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as DiscoDodgeballSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
