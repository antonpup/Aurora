using System.Collections.Generic;
using Aurora.EffectsEngine;
using System.Drawing;

namespace Aurora.Profiles.HotlineMiami
{
    public class GameEvent_HM : GameEvent
    {
        Color bg_color = Color.Black;

        public bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles["Hotline"].Settings as HMSettings).isEnabled;
        }

        public void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            //Background
            EffectLayer bg_layer = new EffectLayer("Hotline - Background", bg_color);
            layers.Enqueue(bg_layer);

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Hotline - Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.ApplicationProfiles["Hotline"].Settings as HMSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            frame.SetLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, GameState new_game_state)
        {
            if (new_game_state is GameState_Wrapper)
            {
                GameState_Wrapper ngw_state = (new_game_state as GameState_Wrapper);

                if (ngw_state.Command.Equals("LFX_Update"))
                {
                    Color newfill = Color.FromArgb(ngw_state.Command_Data.red_start, ngw_state.Command_Data.green_start, ngw_state.Command_Data.blue_start);

                    if (!bg_color.Equals(newfill))
                        bg_color = newfill;
                }
                else if(ngw_state.Command.Equals("LFX_Reset"))
                {
                    bg_color = Color.Black;
                }
                else if (ngw_state.Command.Equals("LFX_Light"))
                {

                }
                else
                    Global.logger.LogLine("Unknown Wrapper Command: " + ngw_state.Command, Logging_Level.Info, false);
            }

            UpdateLights(frame);
        }
    }
}
