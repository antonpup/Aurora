using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Metro_Last_Light
{
    public class GameEvent_MetroLL : GameEvent_Aurora_Wrapper
    {
        public GameEvent_MetroLL()
        {
        }

        public override bool IsEnabled()
        {
            return (this.Profile?.Settings as MetroLLSettings).isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("MetroLL - Color Zones");
            cz_layer.DrawColorZones((this.Profile?.Settings as MetroLLSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
