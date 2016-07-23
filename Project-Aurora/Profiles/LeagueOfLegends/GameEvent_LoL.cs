using System.Collections.Generic;
using Aurora.EffectsEngine;
using System.Drawing;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class GameEvent_LoL : GameEvent_Aurora_Wrapper
    {
        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles["League of Legends"].Settings as LoLSettings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            if (!((Global.Configuration.ApplicationProfiles["League of Legends"].Settings as LoLSettings).disable_cz_on_dark && last_fill_color.Equals(Color.Black)))
            {
                EffectLayer cz_layer = new EffectLayer("League - Color Zones");
                cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles["League of Legends"].Settings as LoLSettings).lighting_areas.ToArray());
                layers.Enqueue(cz_layer);
            }

            frame.AddLayers(layers.ToArray());
        }
    }
}
