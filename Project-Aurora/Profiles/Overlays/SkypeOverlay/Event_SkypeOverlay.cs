using System.Collections.Generic;
using Aurora.EffectsEngine;
using System.Drawing;
using System;

namespace Aurora.Profiles.Overlays.SkypeOverlay
{
    public class Event_SkypeOverlay : GameEvent
    {
        private static int _missed_messages_count = 0;
        private static bool _is_calling = false;

        public Event_SkypeOverlay()
        {
        }

        public static void SetMissedMessagesCount(int count)
        {
            _missed_messages_count = count;
        }

        public static void SetIsCalling(bool state)
        {
            _is_calling = state;
        }

        public bool IsEnabled()
        {
            return true;
        }

        public void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            EffectLayer skype_missed_messages = new EffectLayer("Overlay - Skype Missed Messages");

            if(_missed_messages_count > 0)
            {
                skype_missed_messages.Fill(Color.Orange);
            }

            layers.Enqueue(skype_missed_messages);

            frame.SetOverlayLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, GameState new_state)
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
                    Global.logger.LogLine("Exception during OnNewGameState. Error: " + e, Logging_Level.Error);
                    Global.logger.LogLine(state.ToString(), Logging_Level.None);
                }
            }
        }
    }
}
