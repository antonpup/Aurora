using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.Magic_Duels_2012
{
    public class GameEvent_MagicDuels2012 : GameEvent_Aurora_Wrapper
    {
        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles["MagicDuels2012"].Settings as MagicDuels2012Settings).isEnabled;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //Update wrapper lighting    
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Magic Duels 2012 - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles["MagicDuels2012"].Settings as MagicDuels2012Settings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            frame.AddLayers(layers.ToArray());
        }

    }
}
