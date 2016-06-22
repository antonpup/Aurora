using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Generic_Application
{
    public class GenericApplicationSettings : ProfileSettings
    {
        //Generic
        public string ApplicationName;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas_day { get; set; }
        public List<ColorZone> lighting_areas_night { get; set; }

        //// Shortcut Assistant
        public bool shortcuts_assistant_enabled;
        public Color ctrl_key_color;
        public KeySequence ctrl_key_sequence;
        public Color alt_key_color;
        public KeySequence alt_key_sequence;

        public GenericApplicationSettings()
        {
            //Generic
            ApplicationName = "New Application Profile";
            isEnabled = true;

            //Effects
            //// Lighting Areas
            lighting_areas_day = new List<ColorZone>();
            lighting_areas_night = new List<ColorZone>();

            //// Shortcuts Assistant
            shortcuts_assistant_enabled = true;
            ctrl_key_color = Color.Blue;
            ctrl_key_sequence = new KeySequence();
            alt_key_color = Color.Yellow;
            alt_key_sequence = new KeySequence();
        }

        public GenericApplicationSettings(string appname)
        {
            //Generic
            ApplicationName = appname;
            isEnabled = true;

            //Effects
            //// Lighting Areas
            lighting_areas_day = new List<ColorZone>();
            lighting_areas_night = new List<ColorZone>();

            //// Shortcuts Assistant
            shortcuts_assistant_enabled = true;
            ctrl_key_color = Color.Blue;
            ctrl_key_sequence = new KeySequence();
            alt_key_color = Color.Yellow;
            alt_key_sequence = new KeySequence();
        }
    }
}
