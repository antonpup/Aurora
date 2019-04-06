using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles
{
    public class GameEvent_Generic : LightEvent
    {
        public GameEvent_Generic() : base()
        {
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();
            ApplicationProfile settings = this.Application.Profile;
            
            foreach (var layer in Application.Profile.Layers.Reverse().ToArray())
            {
                if (layer.Enabled)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state)
        {
            if (this.Application.Config.GameStateType != null && !new_game_state.GetType().Equals(this.Application.Config.GameStateType))
                return;

            _game_state = new_game_state;
            UpdateLayerGameStates();
        }

        private void UpdateLayerGameStates()
        {
            ApplicationProfile settings = this.Application.Profile;
            if (settings == null)
                return;

            foreach (Layer lyr in settings.Layers)
                lyr.SetGameState(_game_state);
        }

        public override void ResetGameState()
        {
            if (this.Application?.Config?.GameStateType != null)
                _game_state = (IGameState)Activator.CreateInstance(this.Application.Config.GameStateType);
            else
                _game_state = null;

            UpdateLayerGameStates();
        }
    }
}
