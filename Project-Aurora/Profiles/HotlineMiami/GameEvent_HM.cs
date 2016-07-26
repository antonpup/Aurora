using System.Collections.Generic;
using Aurora.EffectsEngine;
using System.Drawing;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.HotlineMiami
{
    public class GameEvent_HM : GameEvent_Aurora_Wrapper
    {
        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles["Hotline"].Settings as HMSettings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Hotline - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles["Hotline"].Settings as HMSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            frame.AddLayers(layers.ToArray());
        }
    }
}
