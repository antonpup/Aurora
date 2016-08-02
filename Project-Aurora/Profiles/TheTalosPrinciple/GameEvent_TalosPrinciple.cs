using Aurora.EffectsEngine;
using System.Collections.Generic;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.TheTalosPrinciple
{
    public class GameEvent_TalosPrinciple : GameEvent_Aurora_Wrapper
    {
        public GameEvent_TalosPrinciple()
        {
            profilename = "Talos";
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as TalosPrincipleSettings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Talos - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles[profilename].Settings as TalosPrincipleSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts
            Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

            frame.AddLayers(layers.ToArray());
        }
    }
}
