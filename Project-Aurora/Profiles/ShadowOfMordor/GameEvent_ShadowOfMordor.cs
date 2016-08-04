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
            profilename = "ShadowOfMordor";
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as ShadowOfMordorSettings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Shadow of Mordor - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles[profilename].Settings as ShadowOfMordorSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts
            Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

            frame.AddLayers(layers.ToArray());
        }
    }
}
