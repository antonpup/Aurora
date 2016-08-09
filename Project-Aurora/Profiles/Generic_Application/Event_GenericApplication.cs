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
        public Event_GenericApplication(string profilename)
        {
            this.profilename = profilename;
        }

        public override bool IsEnabled()
        {
            if (Global.Configuration.additional_profiles.ContainsKey(profilename))
                return (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).isEnabled;
            else
                return false;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            EffectLayer cz_layer = new EffectLayer("Color Zones");

            ColorZone[] zones = { };

            if (Global.Configuration.additional_profiles.ContainsKey(profilename))
            {
                if (!(Global.Configuration.nighttime_enabled &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.nighttime_start_hour, Global.Configuration.nighttime_start_minute, Global.Configuration.nighttime_end_hour, Global.Configuration.nighttime_end_minute))
                    )
                {
                    zones = (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).lighting_areas_day.ToArray();
                }
                else
                {
                    zones = (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).lighting_areas_night.ToArray();
                }
            }

            cz_layer.DrawColorZones(zones.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts
            Global.Configuration.additional_profiles[profilename].UpdateEffectScripts(layers);

            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");
            if (Global.Configuration.additional_profiles.ContainsKey(profilename) && (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).shortcuts_assistant_enabled)
            {
                if (Global.held_modified == Keys.LControlKey || Global.held_modified == Keys.RControlKey)
                {
                    if (Global.held_modified == Keys.LControlKey)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_CONTROL, (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).ctrl_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_CONTROL, (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).ctrl_key_color);
                    sc_assistant_layer.Set((Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).ctrl_key_sequence, (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).ctrl_key_color);
                }
                else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
                {
                    if (Global.held_modified == Keys.LMenu)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).alt_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).alt_key_color);
                    sc_assistant_layer.Set((Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).alt_key_sequence, (Global.Configuration.additional_profiles[profilename].Settings as GenericApplicationSettings).alt_key_color);
                }
            }
            layers.Enqueue(sc_assistant_layer);

            frame.AddLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, string profilename)
        {
            this.profilename = profilename;

            UpdateLights(frame);
        }

        public override void UpdateLights(EffectFrame frame, GameState new_game_state)
        {
            throw new NotImplementedException();
        }
    }
}
