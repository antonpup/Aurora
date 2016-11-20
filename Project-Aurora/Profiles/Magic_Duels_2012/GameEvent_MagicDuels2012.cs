using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.Magic_Duels_2012
{
    public class GameEvent_MagicDuels2012 : GameEvent_Aurora_Wrapper
    {
        public GameEvent_MagicDuels2012()
        {
            profilename = "MagicDuels2012";
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as MagicDuels2012Settings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Magic Duels 2012 - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles[profilename].Settings as MagicDuels2012Settings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts
            Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

            foreach (var layer in Global.Configuration.ApplicationProfiles[profilename].Settings.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());
        }

    }
}
