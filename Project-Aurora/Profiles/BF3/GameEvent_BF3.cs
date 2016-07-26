using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.BF3
{
    public class GameEvent_BF3 : GameEvent_Aurora_Wrapper
    {
        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles["BF3"].Settings as BF3Settings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame); 
            
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("BF3 - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles["BF3"].Settings as BF3Settings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            frame.AddLayers(layers.ToArray());
        }
    }
}
