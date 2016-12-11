using Aurora.EffectsEngine;
using System.Collections.Generic;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.TheTalosPrinciple
{
    public class GameEvent_TalosPrinciple : GameEvent_Aurora_Wrapper
    {
        public GameEvent_TalosPrinciple()
        {
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Talos - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as TalosPrincipleSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
