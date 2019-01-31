using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.EffectsEngine;
using System.Drawing;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.Aurora_Wrapper
{
    public class GameEvent_Aurora_Wrapper : LightEvent
    {
        private WrapperLightsLayerHandler handler = new WrapperLightsLayerHandler();

        public GameEvent_Aurora_Wrapper() : base()
        {

        }

        public GameEvent_Aurora_Wrapper(LightEventConfig config) : base(config)
        {

        }

        protected virtual void UpdateExtraLights(Queue<EffectLayer> layers)
        {

        }

        public override sealed void UpdateLights(EffectFrame frame)
        {
            UpdateWrapperLights(frame);

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            if (this.Application != null)
            {
                foreach (var layer in this.Application.Profile.Layers.Reverse().ToArray())
                {
                    if (layer.Enabled && layer.LogicPass)
                        layers.Enqueue(layer.Render(_game_state));
                }

                //No need to repeat the code around this everytime this is inherited
                this.UpdateExtraLights(layers);

                //Scripts
                this.Application.UpdateEffectScripts(layers, _game_state);
            }

            frame.AddLayers(layers.ToArray());
        }

        internal virtual void UpdateWrapperLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();
            handler.Render(_game_state);
            frame.AddLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state)
        {
            UpdateWrapperLights(new_game_state);
        }

        internal virtual void UpdateWrapperLights(IGameState new_game_state)
        {
            if (new_game_state is GameState_Wrapper)
            {
                _game_state = new_game_state;

                GameState_Wrapper ngw_state = (new_game_state as GameState_Wrapper);
                handler.SetGameState(new_game_state);
            }
        }

        public new bool IsEnabled
        {
            get { return Global.Configuration.allow_all_logitech_bitmaps; }
        }
    }
    
}
