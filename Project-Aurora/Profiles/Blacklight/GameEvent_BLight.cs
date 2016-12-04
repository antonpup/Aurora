using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Blacklight
{
    public class GameEvent_BLight : GameEvent_Aurora_Wrapper
    {
        public GameEvent_BLight()
        {
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Blacklight - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as BLightSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
