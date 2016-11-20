using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.WormsWMD
{
    public class GameEvent_WormsWMD : GameEvent_Aurora_Wrapper
    {
        public GameEvent_WormsWMD()
        {
            profilename = "WormsWMD";
        }

        public override bool IsEnabled()
        {
            return Global.Configuration.ApplicationProfiles[profilename].Settings.isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            this.colorEnhance_Enabled = (Global.Configuration.ApplicationProfiles[profilename].Settings as WormsWMDSettings).colorEnhance_Enabled;
            this.colorEnhance_initial_factor = (Global.Configuration.ApplicationProfiles[profilename].Settings as WormsWMDSettings).colorEnhance_initial_factor;
            this.colorEnhance_color_factor = (Global.Configuration.ApplicationProfiles[profilename].Settings as WormsWMDSettings).colorEnhance_color_factor;

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
