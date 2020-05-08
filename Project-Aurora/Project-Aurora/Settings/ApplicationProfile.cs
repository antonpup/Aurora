using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Microsoft.Scripting.Utils;
using Newtonsoft.Json;
using PropertyChanged;
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
        [OnChangedMethod(nameof(OnEnabledChanged))] public bool Enabled { get; set; }
        [JsonIgnore] public bool ExceptionHit { get; set; }
        [JsonIgnore] public Exception Exception { get; set; }

        public ScriptSettings(dynamic script)
        {
            if (script?.DefaultKeys != null && script?.DefaultKeys is KeySequence)
                Keys = script.DefaultKeys;
        }

        private void OnEnabledChanged() {
            if (Enabled) {
                ExceptionHit = false;
                Exception = null;
            }
        }
    }

    public class ApplicationProfile : INotifyPropertyChanged, IDisposable
    {
        public string ProfileName { get; set; }
        public Keybind TriggerKeybind { get; set; }
        [JsonIgnore] public string ProfileFilepath { get; set; }
        public Dictionary<string, ScriptSettings> ScriptSettings { get; set; }
        public ObservableCollection<Layer> Layers { get; set; }
        public ObservableCollection<Layer> OverlayLayers { get; set; }

        public ApplicationProfile()
        {
            this.Reset();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Reset()
        {
            Layers = new ObservableCollection<Layer>();
            OverlayLayers = new ObservableCollection<Layer>();
            ScriptSettings = new Dictionary<string, ScriptSettings>();
            TriggerKeybind = new Keybind();
        }

        public virtual void SetApplication(Profiles.Application app)
        {
            foreach (Layer l in Layers)
                l.SetProfile(app);

            foreach (Layer l in OverlayLayers)
                l.SetProfile(app);
        }

        public virtual void Dispose()
        {
            foreach (Layer l in Layers)
                l.Dispose();

            foreach (Layer l in OverlayLayers)
                l.Dispose();
        }
    }
}
