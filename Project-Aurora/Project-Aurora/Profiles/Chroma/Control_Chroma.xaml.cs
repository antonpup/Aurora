using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CSScripting;
using Microsoft.Scripting.Utils;
using Microsoft.Win32;

namespace Aurora.Profiles.Chroma;

public partial class Control_Chroma : INotifyPropertyChanged
{
    public ObservableCollection<string> EnabledPrograms { get; } = new();
    public ObservableCollection<string> ExcludedPrograms => _settings.ExcludedPrograms;
    public string SelectedEnabledProgram { get; set; }
    public string SelectedExcludedProgram { get; set; }

    private readonly ChromaApplication _profile;
    private readonly ChromaApplicationSettings _settings;

    public Control_Chroma(Application profile)
    {
        _profile = (ChromaApplication)profile;
        _settings = (ChromaApplicationSettings)profile.Settings;

        InitializeComponent();
        SetSettings();

        _settings.ExcludedPrograms.CollectionChanged += ExcludedProgramsOnCollectionChanged;
        _profile.ChromaAppsChanged += ProfileOnChromaAppsChanged;

        profile.ProfileChanged += (_, _) => SetSettings();
        RefreshEnabledPrograms();
    }

    private void ProfileOnChromaAppsChanged(object sender, EventArgs e)
    {
        RefreshEnabledPrograms();
    }

    private void ExcludedProgramsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshEnabledPrograms();
    }

    private void RefreshEnabledPrograms()
    {
        Dispatcher.Invoke(() =>
        {
            EnabledPrograms.Clear();
            EnabledPrograms.AddRange(_profile.AllChromaApps.Except(_settings.ExcludedPrograms));
        });
    }

    private void SetSettings()
    {
        GameEnabled.IsChecked = _profile.Settings.IsEnabled;
    }

    private void GameEnabled_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        _profile.Settings.IsEnabled = GameEnabled.IsChecked ?? false;
        _profile.SaveProfiles();
    }

    private void ExcludedAdd_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(SelectedEnabledProgram)) return;
        _settings.ExcludedPrograms.Add(SelectedEnabledProgram);
        ReorderChromaRegistry();
    }

    private void ExcludedRemove_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(SelectedExcludedProgram)) return;
        _settings.ExcludedPrograms.Remove(SelectedExcludedProgram);
        _profile.Config.ProcessNames.AddItem(SelectedExcludedProgram.ToLower());
        ReorderChromaRegistry();
    }

    private void ReorderChromaRegistry()
    {
        var value = string.Join(';', EnabledPrograms) + ";Aurora.exe";
        if (ExcludedPrograms.Count > 0)
        {
            value += ";";
            value += string.Join(';', ExcludedPrograms.Where(s => !s.Equals("Aurora.exe")));
        }
        
        using var registryKey = Registry.LocalMachine.OpenSubKey(ChromaApplication.AppsKey, true);
        registryKey.SetValue(ChromaApplication.PriorityValue, value);
    }
}