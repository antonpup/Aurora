using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.BF3
{
    public class GameEvent_BF3 : GameEvent_Aurora_Wrapper
    {
        public GameEvent_BF3()
        {
            _game_state = new GameState_Wrapper();
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }

        protected override void UpdateExtraLights(Queue<EffectLayer> layers)
        {
            //ColorZones
            EffectLayer cz_layer = new EffectLayer("BF3 - Color Zones");
            cz_layer.DrawColorZones((this.Profile.Settings as BF3Settings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);
        }
    }
}
