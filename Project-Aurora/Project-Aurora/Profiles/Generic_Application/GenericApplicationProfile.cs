using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Generic_Application
{
    public class GenericApplicationProfile : ApplicationProfile
    {
        //Generic
        public string ApplicationName;

        [Newtonsoft.Json.JsonIgnore]
        public bool _simulateNighttime = false;

        [Newtonsoft.Json.JsonIgnore]
        public bool _simulateDaytime = false;

        public ObservableCollection<Layer> Layers_NightTime { get; set; }

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

        public GenericApplicationProfile() : base()
        {
            //Generic
            ApplicationName = "New Application Profile";
        }

        public GenericApplicationProfile(string appname) : base()
        {
            ApplicationName = appname;
        }

        public override void Reset()
        {
            base.Reset();
            Layers_NightTime = new ObservableCollection<Layer>();

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
