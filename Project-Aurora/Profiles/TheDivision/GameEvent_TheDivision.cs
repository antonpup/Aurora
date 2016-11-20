using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.TheDivision
{
    public class GameEvent_TheDivision : GameEvent_Aurora_Wrapper
    {
        public GameEvent_TheDivision()
        {
            profilename = "The Division";
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as TheDivisionSettings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

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
