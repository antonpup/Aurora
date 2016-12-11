using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.XCOM
{
    public class GameEvent_XCOM : GameEvent_Aurora_Wrapper
    {
        public GameEvent_XCOM()
        {
        }

        public override bool IsEnabled()
        {
            return (this.Profile.Settings as XCOMSettings).isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("XCOM - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as XCOMSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
