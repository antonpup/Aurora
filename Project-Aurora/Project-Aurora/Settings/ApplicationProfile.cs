using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aurora.Settings
{
    public abstract class Settings : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            string str = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder });

            return JsonConvert.DeserializeObject(
                    str,
                    this.GetType(),
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder }
                    );
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

    public class ApplicationProfile : Settings, IDisposable
    {
        #region Private Properties
        private string _ProfileName = "";

        private Keybind _triggerKeybind;

        private Dictionary<string, ScriptSettings> _ScriptSettings;

        private ObservableCollection<Layer> _Layers;

        private ObservableCollection<Layer> _OverlayLayers;
        #endregion

        #region Public Properties
        public string ProfileName { get => _ProfileName; set { _ProfileName = value; InvokePropertyChanged(); } }

        public Keybind TriggerKeybind { get => _triggerKeybind; set { _triggerKeybind = value; InvokePropertyChanged(); } }

        [JsonIgnore]
        public string ProfileFilepath { get; set; }

        public Dictionary<string, ScriptSettings> ScriptSettings { get => _ScriptSettings; set { _ScriptSettings = value; InvokePropertyChanged(); } }

        public ObservableCollection<Layer> Layers { get => _Layers; set { _Layers = value; InvokePropertyChanged(); } }

        public ObservableCollection<Layer> OverlayLayers { get => _OverlayLayers; set { _OverlayLayers = value; InvokePropertyChanged(); } }
        #endregion

        public ApplicationProfile()
        {
            this.Reset();
        }

        public virtual void Reset()
        {
            _Layers = new ObservableCollection<Layer>();
            _OverlayLayers = new ObservableCollection<Layer>();
            _ScriptSettings = new Dictionary<string, Aurora.Settings.ScriptSettings>();
            _triggerKeybind = new Keybind();
        }

        public virtual void SetApplication(Aurora.Profiles.Application app)
        {
            foreach (Layer l in _Layers)
                l.SetProfile(app);

            foreach (Layer l in _OverlayLayers)
                l.SetProfile(app);
        }

        public virtual void Dispose()
        {
            foreach (Layer l in _Layers)
                l.Dispose();

            foreach (Layer l in _OverlayLayers)
                l.Dispose();
        }
    }
}
