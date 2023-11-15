using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Microsoft.Win32;

namespace Aurora.Settings.Controls;

/// <summary>
/// Interaction logic for Control_SubProfileManager.xaml
/// </summary>
public partial class Control_ProfileManager
{
    private static ApplicationProfile? Clipboard { get; set; }

    public delegate void ProfileSelectedHandler(ApplicationProfile profile);

    public event ProfileSelectedHandler? ProfileSelected;

    public static readonly DependencyProperty FocusedApplicationProperty = DependencyProperty.Register(nameof(FocusedApplication),
        typeof(Profiles.Application), typeof(Control_ProfileManager), new PropertyMetadata(null, FocusedProfileChanged));

    private readonly Dictionary<Profiles.Application, ApplicationProfile> _lastSelectedProfile = new();

    public Profiles.Application? FocusedApplication
    {
        get => (Profiles.Application)GetValue(FocusedApplicationProperty);
        set => SetValue(FocusedApplicationProperty, value);
    }

    public Control_ProfileManager()
    {
        InitializeComponent();

        lstProfiles.SelectionMode = SelectionMode.Single;
        lstProfiles.SelectionChanged += lstProfiles_SelectionChanged;
    }

    private static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
        var self = (Control_ProfileManager)source;
        if (e.OldValue != null)
        {
            var prof = (Profiles.Application)e.OldValue;
            prof.ProfileChanged -= self.UpdateProfiles;

            if (self._lastSelectedProfile.ContainsKey(prof))
                self._lastSelectedProfile.Remove(prof);

            self._lastSelectedProfile.Add(prof, self.lstProfiles.SelectedItem as ApplicationProfile);
        }
        self.UpdateProfiles();
        if (e.NewValue == null) return;
        var profile = (Profiles.Application)e.NewValue;

        profile.ProfileChanged += self.UpdateProfiles;

        if (self._lastSelectedProfile.TryGetValue(profile, out var selectedProfile))
            self.lstProfiles.SelectedItem = selectedProfile;
    }

    private void UpdateProfiles()
    {
        UpdateProfiles(null, EventArgs.Empty);
    }

    private void UpdateProfiles(object? sender, EventArgs e)
    {
        lstProfiles.ItemsSource = FocusedApplication?.Profiles;
        lstProfiles.Items.SortDescriptions.Add(
            new System.ComponentModel.SortDescription("ProfileName",
                System.ComponentModel.ListSortDirection.Ascending));
        lstProfiles.SelectedItem = FocusedApplication?.Profiles.First(profile => System.IO.Path.GetFileNameWithoutExtension(profile.ProfileFilepath).Equals(FocusedApplication?.Settings.SelectedProfile));
    }

    private void lstProfiles_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count != 1) return;
        if (lstProfiles.SelectedItem != null)
        {
            if (lstProfiles.SelectedItem is not ApplicationProfile)
                throw new ArgumentException($"Items contained in the ListView must be of type 'ProfileSettings', not '{lstProfiles.SelectedItem.GetType()}'");

            FocusedApplication?.SwitchToProfile(lstProfiles.SelectedItem as ApplicationProfile);

            ProfileSelected?.Invoke(lstProfiles.SelectedItem as ApplicationProfile);
            btnDeleteProfile.IsEnabled = true;
        }
        else
            btnDeleteProfile.IsEnabled = false;
    }

    private void btnNewProfile_Click(object? sender, RoutedEventArgs e)
    {
        FocusedApplication?.AddNewProfile();

        lstProfiles.SelectedIndex = lstProfiles.Items.Count - 1;
    }

    private void buttonDeleteProfile_Click(object? sender, EventArgs e)
    {
        if (lstProfiles.SelectedIndex <= -1) return;
        if (FocusedApplication.Profiles.Count == 1)
        {
            MessageBox.Show("You cannot delete the last profile!");
            return;
        }

        if (MessageBox.Show(
                $"Are you sure you want to delete Profile '{((ApplicationProfile)lstProfiles.SelectedItem).ProfileName}'",
                "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes) return;
        var profile = (ApplicationProfile)lstProfiles.SelectedItem;

        FocusedApplication.DeleteProfile(profile);
    }

    private void btnProfilePath_Click(object? sender, RoutedEventArgs e)
    {
        if (FocusedApplication != null)
        {
            System.Diagnostics.Process.Start("explorer", FocusedApplication.GetProfileFolderPath());
        }
    }


    private void btnProfileReset_Click(object? sender, RoutedEventArgs e)
    {
        if (MessageBox.Show($"Are you sure you want to reset the \"{FocusedApplication.Profile.ProfileName}\" profile?",
                "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            FocusedApplication?.ResetProfile();
    }

    private void lstProfiles_KeyDown(object? sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (e.Key == Key.C)
                btnCopyProfile_Click(sender, EventArgs.Empty);
            else if (e.Key == Key.V)
                btnPasteProfile_Click(sender, EventArgs.Empty);
        }
        else if (e.Key == Key.Delete)
            buttonDeleteProfile_Click(sender, EventArgs.Empty);
    }

    private void Hyperlink_RequestNavigate(object? sender, RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start("explorer", e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    private void UserControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Height < 80)
        {
            textblockDownload.Visibility = Visibility.Collapsed;
            borderBottom.Visibility = Visibility.Collapsed;
        }
        else
        {
            textblockDownload.Visibility = Visibility.Visible;
            borderBottom.Visibility = Visibility.Visible;
        }
    }

    private void btnImportProfile_Click(object? sender, EventArgs e)
    {
        try
        {
            // Create OpenFileDialog 
            var dlg = new OpenFileDialog
            {
                Title = "Import profile",
                // Set filter for file extension and default file extension 
                Filter = "Importable files (*.json;*.cueprofile;*.cuefolder)|*.json;*.cueprofile;*.cuefolder|JSON files (*.json)|*.json|CUE Profile Files (*.cueprofile;*.cuefolder)|*.cueprofile;*.cuefolder"
            };

            // Display OpenFileDialog by calling ShowDialog method
            if (dlg.ShowDialog() == true)
                FocusedApplication.ImportProfile(dlg.FileName);
        }
        catch (Exception exception)
        {
            Global.logger.Error(exception, "Profile import error:");
        }
    }

    private void btnExportProfile_Click(object? sender, EventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export profile",
            Filter = "JSON file (*.json)|*.json"
        };

        if (dialog.ShowDialog() == true)
            FocusedApplication.SaveProfile(FocusedApplication.Profile, dialog.FileName);
    }

    private void btnCopyProfile_Click(object? sender, EventArgs e)
    {
        Clipboard = (lstProfiles.SelectedItem as ApplicationProfile)?.TryClone(true) as ApplicationProfile;
    }

    private void btnPasteProfile_Click(object? sender, EventArgs e)
    {
        Global.isDebug = false;
        if (Clipboard == null) return;

        var src = Clipboard;

        // Since we may be copying from one application to another, we need to re-create an application
        // profile since GTA profiles would not work with Desktop profiles for example.
        var @new = FocusedApplication.AddNewProfile(src.ProfileName + " - Copy");
        @new.TriggerKeybind = src.TriggerKeybind.Clone();
        @new.Layers.Clear();

        // We then need to copy all layers from the layer on the clipboard to this new profile.
        // Check all the layers types to ensure that they can be added to this application (to prevent
        // crashes when copying a layer from an application that has a special layer unique to that app)
        for (var i = 0; i < src.Layers.Count; i++)
            if (FocusedApplication.IsAllowedLayer(src.Layers[i].Handler.GetType()))
                @new.Layers.Add((Layer)src.Layers[i].Clone());
            
        FocusedApplication.SaveProfiles();
    }
}