using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.Blacklight
{
    public class GameEvent_BLight : GameEvent_Aurora_Wrapper
    {
        public GameEvent_BLight()
        {
            profilename = "BLight";
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as BLightSettings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Blacklight - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles[profilename].Settings as BLightSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts
            Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

            frame.AddLayers(layers.ToArray());
        }
    }
}
