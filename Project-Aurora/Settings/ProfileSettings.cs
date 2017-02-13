using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aurora.Settings
{
    public abstract class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ScriptSettings : Settings
    {
        #region Private Properties
        private KeySequence _Keys;

        private bool _Enabled = false;

        private bool _ExceptionHit = false;

        private Exception _Exception = null;
        #endregion

        #region Public Properties
        public KeySequence Keys { get { return _Keys; } set { _Keys = value; InvokePropertyChanged(); } }

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

        [JsonIgnore]
        public bool ExceptionHit { get { return _ExceptionHit; } set { _ExceptionHit = value; InvokePropertyChanged(); } }

        [JsonIgnore]
        public Exception Exception { get { return _Exception; } set { _Exception = value; InvokePropertyChanged(); } }
        #endregion

        public ScriptSettings(dynamic script)
        {
            if (script?.DefaultKeys != null && script?.DefaultKeys is KeySequence)
                Keys = script.DefaultKeys;
        }
    }

    public class ProfileSettings : Settings
    {
        #region Private Properties
        private bool isEnabled = true;

        private Dictionary<string, ScriptSettings> _ScriptSettings = new Dictionary<string, ScriptSettings>();

        private ObservableCollection<Layer> _Layers = new ObservableCollection<Layer>();

        private bool _Hidden = false;
        #endregion

        #region Public Properties
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; InvokePropertyChanged(); } }

        public Dictionary<string, ScriptSettings> ScriptSettings { get { return _ScriptSettings; } set { _ScriptSettings = value; InvokePropertyChanged(); } }

        public ObservableCollection<Layer> Layers { get { return _Layers; } set { _Layers = value; InvokePropertyChanged(); } }

        public bool Hidden { get { return _Hidden; } set { _Hidden = value; InvokePropertyChanged(); } }
        #endregion

        public ProfileSettings()
        {
            
        }
    }
}
