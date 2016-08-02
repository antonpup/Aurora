using System.Collections.Generic;
using Aurora.EffectsEngine;
using System.Drawing;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class GameEvent_LoL : GameEvent_Aurora_Wrapper
    {
        public GameEvent_LoL()
        {
            profilename = "League of Legends";
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as LoLSettings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            if (!((Global.Configuration.ApplicationProfiles[profilename].Settings as LoLSettings).disable_cz_on_dark && last_fill_color.Equals(Color.Black)))
            {
                EffectLayer cz_layer = new EffectLayer("League - Color Zones");
                cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles[profilename].Settings as LoLSettings).lighting_areas.ToArray());
                layers.Enqueue(cz_layer);
            }

            //Scripts
            Global.Configuration.ApplicationProfiles[profilename].UpdateEffectScripts(layers, _game_state);

            frame.AddLayers(layers.ToArray());
        }
    }
}
