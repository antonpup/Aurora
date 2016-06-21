using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings;
using System.Drawing;
using System.Windows.Forms;
using Aurora.Profiles.Desktop;

namespace Aurora.Profiles.Generic_Application
{
    public class Event_GenericApplication : GameEvent
    {
        private string profile_key = "";

        public Event_GenericApplication(string profile_key)
        {
            this.profile_key = profile_key;
        }

        public bool IsEnabled()
        {
            if (Global.Configuration.additional_profiles.ContainsKey(profile_key))
                return Global.Configuration.additional_profiles[profile_key].isEnabled;
            else
                return false;
        }

        public void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            EffectLayer cz_layer = new EffectLayer("Color Zones");

            ColorZone[] zones = { };

            if (Global.Configuration.additional_profiles.ContainsKey(profile_key))
            {
                if (!((Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_enabled &&
                    Utils.Time.IsCurrentTimeBetween((Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_start_hour, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_start_minute, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_end_hour, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).nighttime_end_minute))
                    )
                {
                    zones = Global.Configuration.additional_profiles[profile_key].lighting_areas_day.ToArray();
                }
                else
                {
                    zones = Global.Configuration.additional_profiles[profile_key].lighting_areas_night.ToArray();
                }
            }
                

            cz_layer.DrawColorZones(zones.ToArray());
            layers.Enqueue(cz_layer);

            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");
            if (Global.Configuration.additional_profiles.ContainsKey(profile_key) && Global.Configuration.additional_profiles[profile_key].shortcuts_assistant_enabled)
            {
                if (Global.held_modified == Keys.LControlKey || Global.held_modified == Keys.RControlKey)
                {
                    if (Global.held_modified == Keys.LControlKey)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_CONTROL, Global.Configuration.additional_profiles[profile_key].ctrl_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_CONTROL, Global.Configuration.additional_profiles[profile_key].ctrl_key_color);
                    sc_assistant_layer.Set(Global.Configuration.additional_profiles[profile_key].ctrl_key_sequence, Global.Configuration.additional_profiles[profile_key].ctrl_key_color);
                }
                else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
                {
                    if (Global.held_modified == Keys.LMenu)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, Global.Configuration.additional_profiles[profile_key].alt_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, Global.Configuration.additional_profiles[profile_key].alt_key_color);
                    sc_assistant_layer.Set(Global.Configuration.additional_profiles[profile_key].alt_key_sequence, Global.Configuration.additional_profiles[profile_key].alt_key_color);
                }
            }
            layers.Enqueue(sc_assistant_layer);

            frame.SetLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, string profile_key)
        {
            this.profile_key = profile_key;

            UpdateLights(frame);
        }

        public void UpdateLights(EffectFrame frame, GameState new_game_state)
        {
            throw new NotImplementedException();
        }
    }
}
