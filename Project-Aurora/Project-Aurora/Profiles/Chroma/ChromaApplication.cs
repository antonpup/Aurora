using System;
using System.Collections.Specialized;
using System.Linq;
using Aurora.Modules.Razer;
using Aurora.Utils;

namespace Aurora.Profiles.Chroma;

public class ChromaApplication : Application
{
    public event EventHandler<EventArgs>? ChromaAppsChanged;

    public const string AppsKey = @"SOFTWARE\\WOW6432Node\\Razer Chroma SDK\\Apps";
    public const string PriorityValue = "PriorityList";

    private readonly RegistryWatcher _registryWatcher = new(RegistryHiveOpt.LocalMachine, AppsKey, PriorityValue);

    public string[] AllChromaApps { get; private set; } = Array.Empty<string>();

    public ChromaApplication() : base(new LightEventConfig
    {
        Name = "Chroma Apps",
        ID = "chroma",
        ProcessNames = Array.Empty<string>(),
        ProfileType = typeof(RazerChromaProfile),
        OverviewControlType = typeof(Control_Chroma),
        GameStateType = typeof(GameState_Wrapper),
        IconURI = "Resources/chroma.png",
        EnableByDefault = true,
        SettingsType = typeof(ChromaApplicationSettings)
    })
    {
        //TODO move to sdk init
        _registryWatcher.RegistryChanged += RegistryWatcherOnRegistryChanged;
        _registryWatcher.StartWatching();

        RzHelper.ChromaAppChanged += RzHelperOnChromaAppChanged;
    }

    private void RzHelperOnChromaAppChanged(object? sender, ChromaAppChangedEventArgs e)
    {
        FilterAndSetProcesses();
    }

    public override void Dispose()
    {
        base.Dispose();
        ((ChromaApplicationSettings)Settings).ExcludedPrograms.CollectionChanged -= ExcludedProgramsOnCollectionChanged;
        RzHelper.ChromaAppChanged -= RzHelperOnChromaAppChanged;
    }

    protected override void LoadSettings(Type settingsType)
    {
        base.LoadSettings(settingsType);

        ((ChromaApplicationSettings)Settings).ExcludedPrograms.CollectionChanged += ExcludedProgramsOnCollectionChanged;
        FilterAndSetProcesses();
    }

    private void ExcludedProgramsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        FilterAndSetProcesses();
    }

    private void RegistryWatcherOnRegistryChanged(object? sender, RegistryChangedEventArgs e)
    {
        if (e.Data is not string chromaAppList)
        {
            return;
        }

        AllChromaApps = chromaAppList.Split(';');
        FilterAndSetProcesses();
    }

    private void FilterAndSetProcesses()
    {
        if (Settings is not ChromaApplicationSettings chromaApplicationSettings)
        {
            return;
        }
        Config.ProcessNames = new []{ RzHelper.CurrentAppExecutable }
            .Where(processName => !string.IsNullOrWhiteSpace(processName))
            .Where(s => !chromaApplicationSettings.ExcludedPrograms.Contains(s))
            .ToArray();

        ChromaAppsChanged?.Invoke(this, EventArgs.Empty);
    }
}