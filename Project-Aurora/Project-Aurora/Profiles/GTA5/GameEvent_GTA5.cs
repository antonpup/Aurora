using System;
using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.GTA5.GSI;
using System.Drawing;
using Aurora.Settings;
using System.Linq;

namespace Aurora.Profiles.GTA5
{
    public class GameEvent_GTA5 : LightEvent
    {
        public GameEvent_GTA5() : base()
        {
            
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            GTA5Settings settings = (GTA5Settings)this.Profile.Settings;

            foreach (var layer in settings.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            //Scripts
            this.Profile.UpdateEffectScripts(layers, _game_state);

            //ColorZones
            layers.Enqueue(new EffectLayer("GTA 5 - Color Zones").DrawColorZones((this.Profile.Settings as GTA5Settings).lighting_areas.ToArray()));

            frame.AddLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state)
        {
            if (new_game_state is GameState_GTA5)
            {
                _game_state = new_game_state;
            }
        }

        public override void ResetGameState()
        {
            _game_state = new GameState_GTA5();
        }
    }
}
