using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Settings;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.Generic_Application
{
    public class Event_GenericApplication : LightEvent
    {
        public Event_GenericApplication()
        {
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            GenericApplicationSettings settings = (GenericApplicationSettings)this.Profile.Settings;

            ObservableCollection<Layer> timeLayers = settings.Layers;

            //Scripts
            this.Profile.UpdateEffectScripts(layers);

            if ((Global.Configuration.nighttime_enabled &&
                Utils.Time.IsCurrentTimeBetween(Global.Configuration.nighttime_start_hour, Global.Configuration.nighttime_start_minute, Global.Configuration.nighttime_end_hour, Global.Configuration.nighttime_end_minute)) ||
                settings._simulateNighttime
                )
            {
                timeLayers = settings.Layers_NightTime;
            }

            foreach (var layer in timeLayers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, ProfileManager profile)
        {
            this.Profile = profile;

            UpdateLights(frame);
        }

        public override void SetGameState(IGameState new_game_state)
        {
            throw new NotImplementedException();
        }
    }
}
