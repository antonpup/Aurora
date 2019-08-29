using System.Collections.Generic;
using Aurora.EffectsEngine;
using System.Drawing;
using System;
using System.Diagnostics;

namespace Aurora.Profiles.Overlays.SkypeOverlay
{
    public class Event_SkypeOverlay : LightEvent
    {
        private static int _missed_messages_count = 0;
        private static bool _is_calling = false;

        public Event_SkypeOverlay()
        {
            Config = new LightEventConfig
            {
                ID = "skype.exe",
                GameStateType = typeof(State_SkypeOverlay),
                Type = LightEventType.Overlay
            };

            if (Global.Configuration.skype_overlay_settings.enabled)
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = System.IO.Path.Combine(Global.ExecutingDirectory, "Aurora-SkypeIntegration.exe");
                    Process.Start(startInfo);
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Could not start Skype Integration. Error: " + exc);
                }
            }
        }

        public static void SetMissedMessagesCount(int count)
        {
            _missed_messages_count = count;
        }

        public static void SetIsCalling(bool state)
        {
            _is_calling = state;
        }

        public override bool IsEnabled
        {
            get { return Global.Configuration.skype_overlay_settings.enabled; }
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            long time = Utils.Time.GetMillisecondsSinceEpoch();

            if (Global.Configuration.skype_overlay_settings.mm_enabled)
            {
                if (_missed_messages_count > 0)
                {
                    EffectLayer skype_missed_messages = new EffectLayer("Overlay - Skype Missed Messages");

                    ColorSpectrum mm_spec = new ColorSpectrum(Global.Configuration.skype_overlay_settings.mm_color_primary, Global.Configuration.skype_overlay_settings.mm_color_secondary, Global.Configuration.skype_overlay_settings.mm_color_primary);
                    Color color = Color.Orange;

                    if (Global.Configuration.skype_overlay_settings.mm_blink)
                        color = mm_spec.GetColorAt((time % 2000L) / 2000.0f);
                    else
                        color = mm_spec.GetColorAt(0);

                    skype_missed_messages.Set(Global.Configuration.skype_overlay_settings.mm_sequence, color);

                    layers.Enqueue(skype_missed_messages);
                }
            }

            if (Global.Configuration.skype_overlay_settings.call_enabled)
            {
                if (_is_calling)
                {
                    EffectLayer skype_ringing_call = new EffectLayer("Overlay - Skype Ringing Call");

                    ColorSpectrum mm_spec = new ColorSpectrum(Global.Configuration.skype_overlay_settings.call_color_primary, Global.Configuration.skype_overlay_settings.call_color_secondary, Global.Configuration.skype_overlay_settings.call_color_primary);
                    Color color = Color.Green;

                    if (Global.Configuration.skype_overlay_settings.call_blink)
                        color = mm_spec.GetColorAt((time % 2000L) / 2000.0f);
                    else
                        color = mm_spec.GetColorAt(0);

                    skype_ringing_call.Set(Global.Configuration.skype_overlay_settings.call_sequence, color);

                    layers.Enqueue(skype_ringing_call);
                }
            }


            frame.AddOverlayLayers(layers.ToArray());
        }

        public override void UpdateOverlayLights(EffectFrame frame)
        {
            //base.UpdateOverlayLights(frame);
        }

        public override void SetGameState(IGameState new_state)
        {
            if (new_state is State_SkypeOverlay)
            {
                State_SkypeOverlay state = (new_state as State_SkypeOverlay);

                try
                {
                    SetMissedMessagesCount(state.Data.MissedMessagesCount);
                    SetIsCalling(state.Data.IsCalled);
                }
                catch (Exception e)
                {
                    Global.logger.Error("Exception during OnNewGameState. Error: " + e);
                    Global.logger.Info(state.ToString());
                }
            }
        }
    }
}
