using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aurora.Settings
{
    public class ScriptSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public KeySequence Keys { get; set; }

        private bool _Enabled;

        public bool Enabled { get { return _Enabled; }
            set {
                _Enabled = value;
                if (value)
                {
                    ExceptionHit = false;
                    Exception = null;
                }
                InvokePropertyChanged();
            }
        }

        private bool _ExceptionHit = false;

        [JsonIgnore]
        public bool ExceptionHit { get { return _ExceptionHit; } set { _ExceptionHit = value; InvokePropertyChanged(); } }

        private Exception _Exception = null;

        [JsonIgnore]
        public Exception Exception { get { return _Exception; } set { _Exception = value; InvokePropertyChanged(); } }

        public ScriptSettings(dynamic script)
        {
            if (script?.DefaultKeys != null && script?.DefaultKeys is KeySequence)
                Keys = script.DefaultKeys;
        }

        private void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
