using System;
using System.Collections.Specialized;
using System.Linq;
using Aurora.Utils;

namespace Aurora.Profiles.Chroma;

public class ChromaApplication : Application
{
    public event EventHandler<EventArgs> ChromaAppsChanged; 

    public const string AppsKey = @"SOFTWARE\\WOW6432Node\\Razer Chroma SDK\\Apps";
    public const string PriorityValue = "PriorityList";

    private readonly RegistryWatcher _registryWatcher = new(RegistryHiveOpt.LocalMachine, AppsKey, PriorityValue);

    private string[] _allChromaApps = {};

    public ChromaApplication() : base(new LightEventConfig
    {
        Name = "Chroma Apps",
        ID = "chroma",
        ProcessNames = new string[] { },
        ProfileType = typeof(RazerChromaProfile),
        OverviewControlType = typeof(Control_Chroma),
        GameStateType = typeof(GameState_Wrapper),
        IconURI = "Resources/chroma.png",
        EnableByDefault = true,
        SettingsType = typeof(ChromaApplicationSettings)
    })
    {
        _registryWatcher.RegistryChanged += RegistryWatcherOnRegistryChanged;
        _registryWatcher.StartWatching();
    }

    public override void Dispose()
    {
        base.Dispose();
        ((ChromaApplicationSettings)Settings).ExcludedPrograms.CollectionChanged -= ExcludedProgramsOnCollectionChanged;
        
        _registryWatcher.StopWatching();
        _registryWatcher.RegistryChanged -= RegistryWatcherOnRegistryChanged;
    }

    protected override void LoadSettings(Type settingsType)
    {
        base.LoadSettings(settingsType);
        
        ((ChromaApplicationSettings)Settings).ExcludedPrograms.CollectionChanged += ExcludedProgramsOnCollectionChanged;
        FilterAndSetProcesses();
    }

    private void ExcludedProgramsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        FilterAndSetProcesses();
    }

    private void RegistryWatcherOnRegistryChanged(object sender, RegistryChangedEventArgs e)
    {
        if (e.Data is not string chromaAppList)
        {
            return;
        }
        _allChromaApps = chromaAppList.Split(';');
        FilterAndSetProcesses();
    }

    private void FilterAndSetProcesses()
    {
        Config.ProcessNames = _allChromaApps.Where(process =>
        {
            if (Settings is ChromaApplicationSettings chromaApplicationSettings)
            {
                return !chromaApplicationSettings.ExcludedPrograms.Contains(process);
            }

            return true;
        }).ToArray();

        ChromaAppsChanged?.Invoke(this, EventArgs.Empty);
    }
}