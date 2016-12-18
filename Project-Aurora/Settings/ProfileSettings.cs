using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Aurora.Settings
{
    public class ScriptSettings
    {
        public KeySequence Keys { get; set; }

        public bool Enabled { get; set; }

        public ScriptSettings(dynamic script)
        {
            if (script?.DefaultKeys != null && script?.DefaultKeys is KeySequence)
                Keys = script.DefaultKeys;
        }
    }

    public class ProfileSettings
    {
        public bool isEnabled { get; set; }

        public Dictionary<string, ScriptSettings> ScriptSettings { get; set; }

        public ObservableCollection<Layer> Layers { get; set; }

        public bool Hidden { get; set; }

        public ProfileSettings()
        {
            isEnabled = true;
            ScriptSettings = new Dictionary<string, ScriptSettings>();
            Layers = new ObservableCollection<Layer>();
        }
    }
}
