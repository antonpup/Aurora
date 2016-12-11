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

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Hotline - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as HMSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
