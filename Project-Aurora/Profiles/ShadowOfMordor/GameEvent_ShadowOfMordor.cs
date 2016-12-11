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

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Shadow of Mordor - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as ShadowOfMordorSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
