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

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("SSam3 - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as SSam3Settings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
