using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aurora.Profiles
{
    public class ScriptSettings : SettingsBase
    {
        #region Private Properties
        private KeySequence _Keys;

        private bool _Enabled = false;

        private bool _ExceptionHit = false;

        private Exception _Exception = null;
        #endregion

        #region Public Properties
        public KeySequence Keys { get { return _Keys; } set { UpdateVar(ref _Keys, value); } }

        public bool Enabled { get { return _Enabled; }
            set {
                if (value)
                {
                    ExceptionHit = false;
                    Exception = null;
                }
                UpdateVar(ref _Enabled, value);
            }
        }

        [JsonIgnore]
        public bool ExceptionHit { get { return _ExceptionHit; } set { UpdateVar(ref _ExceptionHit, value); } }

        [JsonIgnore]
        public Exception Exception { get { return _Exception; } set { UpdateVar(ref _Exception, value); } }
        #endregion

        public ScriptSettings(dynamic script)
        {
            if (script?.DefaultKeys != null && script?.DefaultKeys is KeySequence)
                Keys = script.DefaultKeys;
        }
    }

    public class ApplicationProfile : SettingsBase, IDisposable
    {
        #region Private Properties
        private string _ProfileName = "";

        private Keybind _triggerKeybind;

        private Dictionary<string, ScriptSettings> _ScriptSettings;

        private ObservableCollection<Layer> _Layers;

        private ObservableCollection<Layer> _OverlayLayers;
        #endregion

        #region Public Properties
        public string ProfileName { get { return _ProfileName; } set { UpdateVar(ref _ProfileName, value); } }

        public Keybind TriggerKeybind { get { return _triggerKeybind; } set { UpdateVar(ref _triggerKeybind, value); } }

        [JsonIgnore]
        public string ProfileFilepath { get; set; }

        public Dictionary<string, ScriptSettings> ScriptSettings { get { return _ScriptSettings; } set { UpdateVar(ref _ScriptSettings, value); } }

        public ObservableCollection<Layer> Layers { get => _Layers; set { UpdateVar(ref _Layers, value); } }

        public ObservableCollection<Layer> OverlayLayers { get => _OverlayLayers; set { UpdateVar(ref _OverlayLayers, value); } }
        #endregion

        public ApplicationProfile()
        {
            this.Reset();
        }

        public virtual void Reset()
        {
            _Layers = new ObservableCollection<Layer>();
            _OverlayLayers = new ObservableCollection<Layer>();
            _ScriptSettings = new Dictionary<string, ScriptSettings>();
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
