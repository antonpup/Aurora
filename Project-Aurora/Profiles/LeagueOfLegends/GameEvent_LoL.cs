using System.Collections.Generic;
using Aurora.EffectsEngine;
using System.Drawing;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class GameEvent_LoL : GameEvent_Aurora_Wrapper
    {
        public GameEvent_LoL()
        {
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            if (!((this.Profile.Settings as LoLSettings).disable_cz_on_dark && last_fill_color.Equals(Color.Black)))
            {
                EffectLayer cz_layer = new EffectLayer("League - Color Zones");
                cz_layer.DrawColorZones((this.Profile.Settings as LoLSettings).lighting_areas.ToArray());
                layers.Enqueue(cz_layer);
            }
        }
    }
}
