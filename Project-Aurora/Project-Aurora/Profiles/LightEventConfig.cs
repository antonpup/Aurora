using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles;

public class LightEventConfig : INotifyPropertyChanged
{
    public string[] ProcessNames
    {
        get => _processNames;
        set
        {
            _processNames = value.Select(s => s.ToLower()).ToArray();
            ProcessNamesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler<EventArgs>? ProcessNamesChanged; 

    /// <summary>One or more REGULAR EXPRESSIONS that can be used to match the title of an application</summary>
    public string[]? ProcessTitles { get; set; }

    public string Name { get; set; }

    public string ID { get; set; }

    public string AppID { get; set; }

    public Type SettingsType { get; set; } = typeof(ApplicationSettings);

    public Type ProfileType { get; set; } = typeof(ApplicationProfile);

    public Type OverviewControlType { get; set; }

    public Type? GameStateType { get; set; }

    private readonly Lazy<LightEvent> _lightEvent;
    private string[] _processNames = Array.Empty<string>();
    public LightEvent Event => _lightEvent.Value;

    public string IconURI { get; set; }

    public HashSet<Type> ExtraAvailableLayers { get; } = new();

    public bool EnableByDefault { get; set; } = true;
    public bool EnableOverlaysByDefault { get; set; } = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LightEventConfig() : this(new Lazy<LightEvent>(() => new GameEvent_Generic()))
    {
    }

    public LightEventConfig(Lazy<LightEvent> lightEvent)
    {
        _lightEvent = lightEvent;
    }

    public LightEventConfig WithLayer<T>() where T : ILayerHandler {
        ExtraAvailableLayers.Add(typeof(T));
        return this;
    }
}