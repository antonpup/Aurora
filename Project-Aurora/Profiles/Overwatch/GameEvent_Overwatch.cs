using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.Overwatch
{
    public class GameEvent_Overwatch : GameEvent_Aurora_Wrapper
    {
        public GameEvent_Overwatch()
        {
            profilename = "Overwatch";
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as OverwatchSettings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            this.colorEnhance_Enabled = (Global.Configuration.ApplicationProfiles[profilename].Settings as OverwatchSettings).colorEnhance_Enabled;
            this.colorEnhance_initial_factor = (Global.Configuration.ApplicationProfiles[profilename].Settings as OverwatchSettings).colorEnhance_initial_factor;
            this.colorEnhance_color_factor = (Global.Configuration.ApplicationProfiles[profilename].Settings as OverwatchSettings).colorEnhance_color_factor;

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
