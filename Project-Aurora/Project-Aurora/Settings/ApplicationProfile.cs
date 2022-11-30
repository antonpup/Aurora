using Aurora.Settings.Layers;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Aurora.Settings;

public class ScriptSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    [OnChangedMethod(nameof(OnEnabledChanged))] public bool Enabled { get; set; }
    [JsonIgnore] public bool ExceptionHit { get; set; }
    [JsonIgnore] public Exception Exception { get; set; }

    private void OnEnabledChanged()
    {
        if (!Enabled) return;
        ExceptionHit = false;
        Exception = null;
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

    protected ApplicationProfile()
    {
        Reset();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void Reset()
    {
        Layers = new ObservableCollection<Layer>();
        OverlayLayers = new ObservableCollection<Layer>();
        ScriptSettings = new Dictionary<string, ScriptSettings>();
        TriggerKeybind = new Keybind();
    }

    public void SetApplication(Profiles.Application app)
    {
        foreach (var l in Layers)
            l.SetProfile(app);

        foreach (var l in OverlayLayers)
            l.SetProfile(app);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        foreach (var l in Layers)
            l.Dispose();

        foreach (var l in OverlayLayers)
            l.Dispose();
    }

    ~ApplicationProfile()
    {
        Dispose(false);
    }
}