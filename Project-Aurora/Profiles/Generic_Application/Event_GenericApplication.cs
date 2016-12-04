using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Settings;
using System.Windows.Forms;

namespace Aurora.Profiles.Generic_Application
{
    public class Event_GenericApplication : LightEvent
    {
        public Event_GenericApplication()
        {
        }

        private bool HasProfile()
        {
            return Global.Configuration.additional_profiles.ContainsKey(this.Profile.ProcessNames[0]);
        }

        public override bool IsEnabled()
        {
            if (HasProfile())
                return this.Profile.Settings.isEnabled;
            else
                return false;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            EffectLayer cz_layer = new EffectLayer("Color Zones");

            ColorZone[] zones = { };

            GenericApplicationSettings settings = (GenericApplicationSettings)this.Profile.Settings;

            if (HasProfile())
            {
                if (!(Global.Configuration.nighttime_enabled &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.nighttime_start_hour, Global.Configuration.nighttime_start_minute, Global.Configuration.nighttime_end_hour, Global.Configuration.nighttime_end_minute))
                    )
                {
                    zones = settings.lighting_areas_day.ToArray();
                }
                else
                {
                    zones = settings.lighting_areas_night.ToArray();
                }
            }

            cz_layer.DrawColorZones(zones.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts
            this.Profile.UpdateEffectScripts(layers);


            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");
            if (HasProfile() && settings.shortcuts_assistant_enabled)
            {
                if (Global.held_modified == Keys.LControlKey || Global.held_modified == Keys.RControlKey)
                {
                    if (Global.held_modified == Keys.LControlKey)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_CONTROL, settings.ctrl_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_CONTROL, settings.ctrl_key_color);
                    sc_assistant_layer.Set(settings.ctrl_key_sequence, settings.ctrl_key_color);
                }
                else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
                {
                    if (Global.held_modified == Keys.LMenu)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, settings.alt_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, settings.alt_key_color);
                    sc_assistant_layer.Set(settings.alt_key_sequence, settings.alt_key_color);
                }
            }
            layers.Enqueue(sc_assistant_layer);

            foreach (var layer in settings.Layers.Reverse().ToArray())
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

        public override void UpdateLights(EffectFrame frame, IGameState new_game_state)
        {
            throw new NotImplementedException();
        }
    }
}
